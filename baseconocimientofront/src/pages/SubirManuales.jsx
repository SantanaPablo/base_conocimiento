import React, { useState } from 'react';
import { Upload, FileText, CheckCircle, AlertCircle, Loader2 } from 'lucide-react';

const SubirManuales = () => {
  const [dragActive, setDragActive] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [status, setStatus] = useState(null); // 'success' | 'error'

  const handleUpload = async (e) => {
    e.preventDefault();
    setIsUploading(true);
    // Simulación de carga (aquí iría tu fetch al endpoint de Ingestión)
    setTimeout(() => {
      setIsUploading(false);
      setStatus('success');
    }, 2000);
  };

  return (
    <div className="p-8 max-w-4xl mx-auto animate-in fade-in slide-in-from-bottom-4">
      <header className="mb-10">
        <h2 className="text-3xl font-black text-white tracking-tighter uppercase">Ingesta de Conocimiento</h2>
        <p className="text-slate-400 text-sm">Entrena a Inuzaru subiendo nuevos manuales en PDF.</p>
      </header>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Formulario */}
        <div className="md:col-span-2 space-y-6">
          <div 
            className={`relative group border-2 border-dashed rounded-3xl p-12 transition-all flex flex-col items-center justify-center
              ${dragActive ? 'border-blue-500 bg-blue-500/10' : 'border-slate-700 bg-slate-800/30 hover:border-slate-500'}`}
            onDragOver={() => setDragActive(true)}
            onDragLeave={() => setDragActive(false)}
          >
            <div className="bg-blue-600/20 p-4 rounded-2xl mb-4 group-hover:scale-110 transition-transform">
              <Upload className="text-blue-400" size={32} />
            </div>
            <p className="text-white font-bold text-center">Arrastra tus archivos PDF aquí</p>
            <p className="text-slate-500 text-xs mt-2 italic">Tamaño máximo recomendado: 20MB por archivo</p>
            <input type="file" className="absolute inset-0 opacity-0 cursor-pointer" onChange={handleUpload} />
          </div>

          <div className="flex gap-4">
             <select className="flex-1 bg-slate-800 border border-slate-700 rounded-xl px-4 py-3 text-sm focus:outline-none focus:border-blue-500">
                <option value="Microinformatica">Microinformática</option>
                <option value="Redes">Redes y Conectividad</option>
                <option value="Sistemas">Sistemas Operativos</option>
             </select>
             <button 
                onClick={handleUpload}
                disabled={isUploading}
                className="bg-blue-600 hover:bg-blue-500 px-8 py-3 rounded-xl font-bold text-sm transition-all flex items-center gap-2 disabled:opacity-50"
              >
               {isUploading ? <Loader2 className="animate-spin" size={18}/> : 'Procesar Manual'}
             </button>
          </div>
        </div>

        {/* Status / Log */}
        <div className="bg-slate-900/50 border border-slate-800 rounded-3xl p-6 h-fit">
          <h3 className="text-[10px] font-black uppercase tracking-widest text-slate-500 mb-4">Registro de Actividad</h3>
          <div className="space-y-4">
            {status === 'success' && (
              <div className="flex gap-3 text-green-400 text-xs items-start">
                <CheckCircle size={14} className="shrink-0" />
                <span>Manual de "Lotus Notes v8.5" indexado correctamente en Qdrant.</span>
              </div>
            )}
            {isUploading && (
              <div className="flex gap-3 text-blue-400 text-xs items-start animate-pulse">
                <Loader2 size={14} className="shrink-0 animate-spin" />
                <span>Extrayendo texto y generando embeddings...</span>
              </div>
            )}
            {!isUploading && !status && <p className="text-slate-600 text-[11px] italic">Esperando archivos para procesar...</p>}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SubirManuales;