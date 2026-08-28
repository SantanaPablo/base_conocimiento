# Diagnóstico: Ollama lento en GTX 1060 (CachyOS/Linux)

## Resumen del problema

Ollama respondía rápido en Windows pero se puso lento al migrar a CachyOS (Linux) con la misma GTX 1060 (3GB). La causa fueron **dos problemas encadenados**, no uno solo.

---

## Causa raíz 1: el paquete de Ollama no soportaba la GPU

CachyOS/Arch instala Ollama vía dos paquetes: `ollama` (CPU) y `ollama-cuda` (GPU). El problema es que **`ollama-cuda` de los repos venía compilado contra CUDA 13**, y Nvidia dejó de dar soporte a las arquitecturas **Maxwell, Pascal y Volta** a partir de esa versión del toolkit. La GTX 1060 es Pascal (compute capability 6.1), así que quedó afuera.

Se confirmó con el log del servicio:

```bash
journalctl -u ollama -e --no-pager | grep -i -E "gpu|cuda|nvidia"
```

Salida clave que confirmó el diagnóstico:
```
warning: no usable GPU found, --gpu-layers option will be ignored
skipping CUDA device — compute capability not in compiled architectures device="NVIDIA GeForce GTX 1060 3GB" cc=610 archs="[750 800 860...]"
```

`cc=610` (tu placa) no estaba en la lista `archs` (todas 750 = Turing en adelante). Resultado: Ollama corría el modelo **100% en CPU**, mucho más lento.

### Solución: reinstalar con el instalador oficial (no el paquete de Arch)

El binario oficial de ollama.com trae soporte más amplio de arquitecturas que el paquete empaquetado por Arch/CachyOS.

**Linux (CachyOS/Arch):**
```bash
# Sacar el paquete de los repos (compilado sin soporte Pascal)
sudo pacman -R ollama ollama-cuda

# Instalar la build oficial
curl -fsSL https://ollama.com/install.sh | sh

# Confirmar versión
ollama --version
```

**Windows:** el instalador de https://ollama.com/download ya usa builds oficiales por defecto, por eso ahí nunca hubo este problema.

---

## Causa raíz 2: la GPU (3GB) no alcanza para el modelo completo

Una vez detectada la GPU, apareció un segundo error:

```
Error: 500 Internal Server Error: llama-server process has terminated: exit status 1: cudaMalloc failed: out of memory
alloc_tensor_range: failed to allocate CUDA0 buffer of size 2011539712
```

El Modelfile tenía `PARAMETER num_gpu 99`, que fuerza **todas** las capas del modelo a la GPU. Con solo 3GB de VRAM (y ~1GB ya ocupado por el escritorio: KDE Plasma, Zoom, navegador, etc.), el modelo de 2.4GB no entraba completo.

### Solución: offload parcial de capas (num_gpu)

En vez de todo-o-nada, se le indica a Ollama cuántas capas mandar a GPU y dejar el resto en CPU. Se prueba subiendo el número de a poco hasta encontrar el máximo que no tira error de memoria.

**Modelfile (`inuzaru`):**
```
FROM llama3.2:3b
PARAMETER num_ctx 2048
PARAMETER num_gpu 21      # antes: 99 — bajado por límite de VRAM (3GB)
PARAMETER temperature 0.3
SYSTEM """
...
"""
```

Comandos usados para probar distintos valores (Linux, pero son los mismos en Windows con `ollama` en PATH):
```bash
ollama create inuzaru -f Modelfile
ollama run inuzaru "hola"
ollama ps
```

`ollama ps` muestra el `%CPU/%GPU` real de la última corrida:
```
NAME              PROCESSOR
inuzaru:latest    24%/76% CPU/GPU     ← con num_gpu 21 (mejor resultado)
inuzaru:latest    67%/33% CPU/GPU     ← con num_gpu 20
inuzaru:latest    100% CPU            ← estado original, sin GPU
```

Con `num_gpu 22` volvió a fallar por falta de memoria, así que **21 quedó como valor definitivo**.

---

## Comandos de diagnóstico para el futuro (guardar esta sección)

Si vuelve a andar lento, revisar en este orden:

**1. ¿Está usando GPU?**
```bash
# Linux
ollama ps
nvidia-smi

# Windows (PowerShell/CMD) — mismos comandos, Ollama y nvidia-smi funcionan igual
ollama ps
nvidia-smi
```
Buscar la columna `PROCESSOR` en `ollama ps`. Si dice `100% CPU`, hay un problema de detección de GPU o de memoria.

**2. ¿Hay memoria libre en la GPU?**
```bash
nvidia-smi --query-gpu=memory.used,memory.total,memory.free --format=csv
```
Cerrar Zoom, navegadores con aceleración GPU, etc. si el margen libre es poco.

**3. ¿El log menciona algún error de GPU/CUDA?**
```bash
# Linux
journalctl -u ollama -f
# (dejarlo corriendo y en otra terminal, mandar una consulta de prueba)

# Windows
# Ollama corre como app en bandeja o servicio; ver logs en:
# %LOCALAPPDATA%\Ollama\server.log
```

**4. Si el error es `cudaMalloc failed: out of memory`:**
Bajar `num_gpu` en el Modelfile del modelo afectado, reconstruir con `ollama create <nombre> -f Modelfile`, y volver a probar con `ollama ps` hasta encontrar el número máximo de capas que entra sin fallar.

**5. Si el error es `no usable GPU found` o menciona `compute capability not in compiled architectures`:**
El binario de Ollama no tiene soporte para tu arquitectura de GPU. Reinstalar con el script oficial (Linux) o el instalador de la web (Windows), no con el paquete del gestor de paquetes de la distro.

---

## Nota aparte (no relacionada a GPU)

Al reinstalar Ollama en Linux, el directorio de modelos cambió de ubicación y se perdió `nomic-embed-text` (usado para embeddings). Esto causaba un error `404 Not Found` al llamar a `/api/embeddings` desde la app. Se resolvió volviendo a bajarlo:
```bash
ollama pull nomic-embed-text
```
Si en el futuro aparece un 404 en el servicio de embeddings, revisar primero con `ollama list` si el modelo sigue instalado.
