import React, { useState, useRef } from 'react';
import { Upload, FileText, CheckCircle, AlertCircle, Loader2, X, Database, Info } from 'lucide-react';

const SubirManuales = () => {
  const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7148/api';
  const CATEGORIAS_TEMP = ["Microinformatica", "Redes", "Impresoras"];

  const [dragActive, setDragActive] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [status, setStatus] = useState({ type: null, message: '' });
  const [errors, setErrors] = useState({}); // Seguimiento de campos vacíos

  const [file, setFile] = useState(null);
  const [metadata, setMetadata] = useState({
    titulo: '',
    categoria: CATEGORIAS_TEMP[0],
    subCategoria: '',
    version: '1.0',
    descripcion: ''
  });

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setMetadata(prev => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: false }));
    }
  };

  const handleFileChange = (e) => {
    const selectedFile = e.target.files[0];
    if (selectedFile && selectedFile.type === "application/pdf") {
      setFile(selectedFile);
      if (!metadata.titulo) {
        setMetadata(prev => ({ ...prev, titulo: selectedFile.name.replace('.pdf', '') }));
      }
      setStatus({ type: null, message: '' });
    } else {
      setStatus({ type: 'error', message: 'Selecciona un archivo PDF válido.' });
    }
  };

  const validateForm = () => {
    const newErrors = {};
    if (!metadata.titulo.trim()) newErrors.titulo = true;
    if (!metadata.descripcion.trim()) newErrors.descripcion = true;
    if (!file) newErrors.file = true;

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleUpload = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      setStatus({ type: 'error', message: 'Por favor, completa los campos obligatorios.' });
      return;
    }

    setIsUploading(true);
    setStatus({ type: null, message: '' });

    const formData = new FormData();
    formData.append('Archivo', file);
    formData.append('Titulo', metadata.titulo);
    formData.append('Categoria', metadata.categoria);
    formData.append('SubCategoria', metadata.subCategoria);
    formData.append('Version', metadata.version);
    formData.append('Descripcion', metadata.descripcion);

    try {
      const response = await fetch(`${API_BASE_URL}/Manuales`, {
        method: 'POST',
        body: formData,
      });

      const result = await response.json();

      if (response.ok && result.exitoso) {
        setStatus({ 
          type: 'success', 
          message: `Manual "${metadata.titulo}" indexado con éxito.` 
        });
        // Resetear formulario
        setFile(null);
        setMetadata({
          titulo: '',
          categoria: CATEGORIAS_TEMP[0],
          subCategoria: '',
          version: '1.0',
          descripcion: ''
        });
      } else {
        setStatus({ type: 'error', message: result.mensaje || 'Error reportado por el servidor.' });
      }
    } catch (error) {
      setStatus({ type: 'error', message: 'No se pudo conectar con el servidor .NET.' });
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="p-8 max-w-6xl mx-auto animate-in fade-in duration-700">
      <header className="mb-8 flex items-end justify-between border-b border-slate-800 pb-6">
        <div>
          <h2 className="text-4xl font-black text-white tracking-tighter uppercase italic">
            Ingesta <span className="text-blue-500">Inuzaru</span>
          </h2>
          <p className="text-slate-400 text-sm font-medium mt-1 uppercase tracking-widest text-[10px]">
            Base de Conocimiento
          </p>
        </div>
        <div className="hidden md:flex bg-slate-800/50 px-4 py-2 rounded-full border border-slate-700 items-center gap-2">
          <div className="w-2 h-2 bg-emerald-500 rounded-full animate-pulse" />
          <span className="text-[10px] font-bold text-slate-300 uppercase tracking-widest">Qdrant Node Online</span>
        </div>
      </header>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        <div className="lg:col-span-8 space-y-6">
          
          <div 
            className={`relative group border-2 border-dashed rounded-[2rem] p-10 transition-all flex flex-col items-center justify-center
              ${dragActive ? 'border-blue-500 bg-blue-500/10' : 'border-slate-700 bg-slate-900/40 hover:border-slate-600'}
              ${errors.file ? 'border-red-500/50 bg-red-500/5' : ''}`}
            onDragOver={(e) => { e.preventDefault(); setDragActive(true); }}
            onDragLeave={() => setDragActive(false)}
            onDrop={(e) => {
              e.preventDefault();
              setDragActive(false);
              handleFileChange({ target: { files: e.dataTransfer.files } });
            }}
          >
            {!file ? (
              <>
                <Upload className={`${errors.file ? 'text-red-400' : 'text-blue-500'} mb-4`} size={40} />
                <p className="text-white font-bold text-lg">Arrastra el manual PDF</p>
                {errors.file && <p className="text-red-400 text-xs mt-2 font-bold uppercase">El archivo es obligatorio</p>}
              </>
            ) : (
              <div className="flex flex-col items-center animate-in zoom-in-95">
                <FileText className="text-emerald-400 mb-4" size={48} />
                <p className="text-white font-bold text-center max-w-xs truncate">{file.name}</p>
                <button onClick={() => setFile(null)} className="mt-4 text-[10px] text-slate-500 hover:text-red-400 uppercase font-black tracking-widest flex items-center gap-1 transition-colors">
                  <X size={12} /> Eliminar selección
                </button>
              </div>
            )}
            <input type="file" className="absolute inset-0 opacity-0 cursor-pointer" accept=".pdf" onChange={handleFileChange} disabled={isUploading} />
          </div>

          <div className="bg-slate-900/50 p-8 rounded-[2rem] border border-slate-800 shadow-2xl space-y-5">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              
              {/* Título */}
              <div className="md:col-span-2">
                <label className={`text-[10px] font-black uppercase tracking-[0.2em] mb-2 block ${errors.titulo ? 'text-red-400' : 'text-slate-500'}`}>
                  Título del Documento {errors.titulo && '• Requerido'}
                </label>
                <input 
                  name="titulo" 
                  value={metadata.titulo} 
                  onChange={handleInputChange} 
                  className={`w-full bg-slate-950 border rounded-xl px-4 py-3 text-white outline-none transition-all ${errors.titulo ? 'border-red-500/50' : 'border-slate-800 focus:border-blue-500'}`}
                  placeholder="Ej: Manual de Usuario Lotus Notes 8.5"
                />
              </div>

              <div className="md:col-span-2">
                <label className={`text-[10px] font-black uppercase tracking-[0.2em] mb-2 block ${errors.descripcion ? 'text-red-400' : 'text-slate-500'}`}>
                  Descripción del Contenido {errors.descripcion && '• Requerido'}
                </label>
                <textarea 
                  name="descripcion" 
                  value={metadata.descripcion} 
                  onChange={handleInputChange} 
                  rows="3"
                  className={`w-full bg-slate-950 border rounded-xl px-4 py-3 text-white outline-none transition-all resize-none ${errors.descripcion ? 'border-red-500/50' : 'border-slate-800 focus:border-blue-500'}`}
                  placeholder="Breve resumen para ayudar al motor de IA a contextualizar el manual..."
                />
              </div>

              <div>
                <label className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em] mb-2 block">Categoría Principal</label>
                <select 
                  name="categoria" 
                  value={metadata.categoria} 
                  onChange={handleInputChange} 
                  className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-white outline-none focus:border-blue-500 transition-all cursor-pointer"
                >
                  {CATEGORIAS_TEMP.map(cat => (
                    <option key={cat} value={cat}>{cat}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-[10px] font-black text-slate-500 uppercase tracking-[0.2em] mb-2 block">Sub-Categoría (Tag)</label>
                <input 
                  name="subCategoria" 
                  value={metadata.subCategoria} 
                  onChange={handleInputChange} 
                  className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-white outline-none focus:border-blue-500 transition-all" 
                  placeholder="Ej: v8.5, Soporte, Hardware..." 
                />
              </div>
            </div>

            <button 
              onClick={handleUpload}
              disabled={isUploading}
              className="w-full bg-blue-600 hover:bg-blue-500 disabled:bg-slate-800 text-white font-black py-4 rounded-xl transition-all flex items-center justify-center gap-3 shadow-lg shadow-blue-900/10 group"
            >
              {isUploading ? (
                <>
                  <Loader2 className="animate-spin" size={20} />
                  <span className="tracking-[0.2em] uppercase text-sm">Procesando Vectores...</span>
                </>
              ) : (
                <>
                  <Upload size={20} className="group-hover:-translate-y-1 transition-transform" />
                  <span className="tracking-[0.2em] uppercase text-sm">Iniciar Ingesta de Datos</span>
                </>
              )}
            </button>
          </div>
        </div>

        <div className="lg:col-span-4">
          <div className="bg-slate-950/50 border border-slate-800 rounded-[2rem] p-6 sticky top-8 backdrop-blur-sm">
            <h3 className="text-[10px] font-black uppercase tracking-[0.3em] text-blue-500 mb-6 flex items-center gap-2">
              <Info size={14} />
              Log de Operaciones
            </h3>
            
            <div className="space-y-4 font-mono text-[11px]">
              {isUploading && (
                <div className="text-blue-400 bg-blue-500/5 p-4 rounded-2xl border border-blue-500/10 animate-pulse flex gap-3">
                  <Loader2 size={16} className="shrink-0 animate-spin" />
                  <p>Enviando stream a <b>{API_BASE_URL}</b>. El servidor está generando los chunks y embeddings en Ollama...</p>
                </div>
              )}

              {status.type === 'success' && (
                <div className="text-emerald-400 bg-emerald-500/5 p-4 rounded-2xl border border-emerald-500/10 flex gap-3 animate-in slide-in-from-right-4">
                  <CheckCircle size={16} className="shrink-0" />
                  <p>{status.message}</p>
                </div>
              )}

              {status.type === 'error' && (
                <div className="text-red-400 bg-red-500/5 p-4 rounded-2xl border border-red-500/10 flex gap-3 animate-in shake">
                  <AlertCircle size={16} className="shrink-0" />
                  <p>{status.message}</p>
                </div>
              )}

              {!isUploading && !status.type && (
                <div className="py-12 flex flex-col items-center justify-center text-slate-700 opacity-50">
                  <Database size={32} strokeWidth={1} className="mb-2" />
                  <p className="text-center italic uppercase tracking-tighter">Esperando parámetros de entrada...</p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SubirManuales;