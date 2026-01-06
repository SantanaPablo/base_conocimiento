# IA INUZARU RAG ENGINE 🦧🐕
> **Transformando la documentación estática en conversaciones inteligentes y privadas.**

**INUZARU** es un motor de **Retrieval-Augmented Generation (RAG)** diseñado para optimizar el acceso a bases de conocimiento técnicas. Permite a los equipos técnicos dejar de buscar manualmente en archivos PDF extensos para empezar a conversar directamente con la documentación de forma inteligente, local y segura.

---

## 🎯 El Problema y la Solución
Ante un incidente técnico, el tiempo es crítico. **INUZARU** elimina la dependencia de "el que más sabe" o de búsquedas infinitas en chats, permitiendo que cada técnico suba manuales aprobados y obtenga respuestas precisas con trazabilidad total.

### ✅ Ventajas Clave
* **Privacidad Total:** Los datos son 100% locales. Nada sale de tu infraestructura para alimentar modelos externos.
* **Soberanía Tecnológica:** Sin costos por tokens ni dependencia de APIs de terceros.
* **Cero Alucinaciones:** El sistema se restringe estrictamente al contexto de los manuales cargados.
* **Trazabilidad:** Cada respuesta indica la fuente exacta y el número de página del manual consultado.
* **Modo Offline:** Diseñado para funcionar en redes locales cerradas o entornos empresariales restringidos.

### ⚠️ Consideraciones
* **Hardware:** Requiere inversión en GPU dedicada para la ejecución del LLM local (Ollama/LocalAI).

---

## 🛠️ Stack Tecnológico

| Componente | Tecnología |
| :--- | :--- |
| **Backend** | .NET 8 / Core API (C#) |
| **Frontend** | React (JavaScript) |
| **Buscador Semántico** | Qdrant (Vector Database) |
| **Caché y Contexto** | Redis |
| **Base de Datos** | PostgreSQL (Gestión de metadatos) |
| **Contenerización** | Docker & Docker Compose |

---

## 🏗️ Arquitectura y Patrones
El sistema sigue los estándares de **Clean Architecture** y **Domain-Driven Design (DDD)**, estructurado en las siguientes capas presentes en este repositorio:

* **BaseConocimiento.API:** Punto de entrada y controladores.
* **BaseConocimiento.Application:** Lógica de negocio, DTOs y casos de uso (CQRS).
* **BaseConocimiento.Domain:** Entidades de núcleo y reglas de negocio.
* **BaseConocimiento.Infraestructure:** Implementación de persistencia (PostgreSQL), clientes de Qdrant y servicios externos.

**Principios aplicados:**
* **SOLID:** Para un código mantenible y robusto.
* **CQRS:** Segregación de comandos y consultas.
* **Unit of Work:** Consistencia transaccional entre la base de datos relacional y el motor vectorial.

---

## 🚀 Cómo empezar

### Requisitos previos
* Docker y Docker Compose
* SDK de .NET 8

### Instalación
1. **Clonar el repositorio:**
   git clone [https://github.com/SantanaPablo/base_conocimiento.git](https://github.com/SantanaPablo/base_conocimiento.git)
   cd base_conocimiento
2. **Levantar infraestructura:** El proyecto incluye un archivo docker-compose.yml que levanta Qdrant, Redis y PostgreSQL automáticamente:
   docker-compose up -d


# Desarrollado por Pablo Santana



   
