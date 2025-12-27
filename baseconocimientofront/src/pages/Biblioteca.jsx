import React, { useState, useEffect } from 'react';
import { Book, FileText, Trash2, ExternalLink, Database, Loader2 } from 'lucide-react';

const Biblioteca = () => {
  const [manuales, setManuales] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  const fetchManuales = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/Manuales?pagina=1&tamañoPagina=50`);
      const data = await response.json();
      
      setManuales(data.manuales || []); 
      
    } catch (error) {
      console.error("Error cargando biblioteca:", error);
      setManuales([]);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => { fetchManuales(); }, []);

  const eliminarManual = async (id) => {
    if (!window.confirm("¿Seguro que quieres eliminar este manual de Qdrant y la DB?")) return;
    
    try {
      const res = await fetch(`${import.meta.env.VITE_API_URL}/Manuales/${id}`, { method: 'DELETE' });
      if (res.ok) fetchManuales(); // Recargar lista
    } catch (error) {
      alert("Error al eliminar");
    }
  };

  if (isLoading) return (
    <div className="flex flex-col items-center justify-center h-full text-blue-400 gap-4">
      <Loader2 className="animate-spin" size={40} />
      <p className="font-mono text-xs uppercase tracking-widest">Sincronizando con Qdrant...</p>
    </div>
  );

  return (
    <div className="p-8 max-w-6xl mx-auto animate-in fade-in">
      <header className="mb-10 flex justify-between items-end">
        <div>
          <h2 className="text-3xl font-black text-white tracking-tighter uppercase italic">Biblioteca de Vectores</h2>
          <p className="text-slate-400 text-sm">Índice real de conocimiento procesado.</p>
        </div>
        <div className="flex items-center gap-2 px-4 py-2 bg-blue-500/10 border border-blue-500/20 rounded-lg text-blue-400 text-xs font-bold uppercase">
          <Database size={14} /> {manuales.length} Documentos Activos
        </div>
      </header>

      <div className="grid grid-cols-1 gap-4">
        {manuales.length === 0 && <p className="text-slate-500 italic">No hay manuales cargados aún.</p>}
        {manuales.map((doc) => (
          <div key={doc.id} className="group flex items-center gap-6 bg-[#1e293b]/40 border border-slate-800 p-5 rounded-2xl hover:bg-slate-800/60 transition-all hover:border-blue-500/30">
            <div className={`p-4 rounded-xl border ${doc.estado === 1 ? 'text-blue-400 border-blue-500/20 bg-slate-900' : 'text-slate-500 border-slate-800'}`}>
              <FileText size={24} />
            </div>
            
            <div className="flex-1">
              <h4 className="text-slate-100 font-bold mb-1">{doc.titulo}</h4>
              <div className="flex gap-4 text-[10px] text-slate-500 font-bold uppercase tracking-tight">
                <span className="bg-slate-800 px-2 py-0.5 rounded text-blue-400">{doc.categoria}</span>
                <span className="flex items-center gap-1"><Book size={12}/> v{doc.version}</span>
                <span>{new Date(doc.fechaCreacion || Date.now()).toLocaleDateString()}</span>
              </div>
            </div>

            <div className="flex gap-2">
              <button className="p-2.5 text-slate-400 hover:text-white hover:bg-slate-700 rounded-lg transition-all">
                <ExternalLink size={18} />
              </button>
              <button 
                onClick={() => eliminarManual(doc.id)}
                className="p-2.5 text-slate-500 hover:text-red-400 hover:bg-red-400/10 rounded-lg transition-all"
              >
                <Trash2 size={18} />
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Biblioteca;