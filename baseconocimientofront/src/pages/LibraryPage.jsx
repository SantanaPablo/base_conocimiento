import React, { useState, useEffect } from 'react';
import { 
  Search, 
  Filter, 
  FileText, 
  Download, 
  Trash2, 
  Edit3, 
  MoreVertical,
  Calendar,
  Database
} from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const LibraryPage = () => {
  const [manuales, setManuales] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Estados
  const [search, setSearch] = useState('');
  const [selectedCat, setSelectedCat] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  //categorías para el filtro
  useEffect(() => {
    fetch(`${API_BASE_URL}/Categorias?soloActivas=true`)
      .then(res => res.json())
      .then(data => setCategorias(data.categorias || []));
  }, []);

  // filtros aplicados
  const fetchManuales = async () => {
    setLoading(true);
    try {
      const queryParams = new URLSearchParams({
        pagina: page,
        tamañoPagina: 8,
        terminoBusqueda: search,
        categoriaId: selectedCat
      });

      const res = await fetch(`${API_BASE_URL}/Manuales?${queryParams}`);
      const data = await res.json();
      
      if (data.exitoso) {
        setManuales(data.manuales || []);
        setTotalPages(data.totalPaginas || 1);
      }
    } catch (err) {
      console.error("Error al listar manuales:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchManuales();
  }, [page, selectedCat]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setPage(1);
    fetchManuales();
  };
 //Descargar Manual
const handleDownload = async (manualId) => {
  try {
    const res = await fetch(`${API_BASE_URL}/Manuales/${manualId}/descargar`);

    if (!res.ok) throw new Error('Error al descargar');

    const blob = await res.blob();

    //obtener nombre
    const disposition = res.headers.get('content-disposition');
let fileName = 'archivo';

if (disposition) {
  const utf8Match = disposition.match(/filename\*=(?:UTF-8'')?(.+)/i);
  if (utf8Match && utf8Match[1]) {
    fileName = decodeURIComponent(utf8Match[1]);
  } else {
    const asciiMatch = disposition.match(/filename="?([^"]+)"?/i);
    if (asciiMatch && asciiMatch[1]) {
      fileName = asciiMatch[1];
    }
  }
}

    const url = window.URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();

    a.remove();
    window.URL.revokeObjectURL(url);
  } catch (err) {
    console.error('Error descargando manual:', err);
    alert('No se pudo descargar el archivo');
  }
};



  //Eliminar manual
  const handleDelete = async (id) => {
    if (!window.confirm("¿Seguro que querés eliminar este manual? Se borrarán sus fragmentos del RAG.")) return;
    
    try {
      const res = await fetch(`${API_BASE_URL}/Manuales/${id}`, { method: 'DELETE' });
      if (res.ok) {
        setManuales(manuales.filter(m => m.id !== id));
      }
    } catch (err) {
      console.error("Error al eliminar:", err);
    }
  };

  return (
    <div className="h-full flex flex-col bg-[#0f172a] p-8 overflow-hidden">
      
      {/* CABECERA Y BUSCADOR */}
      <header className="mb-8 space-y-6">
        <div>
          <h2 className="text-3xl font-black text-white uppercase italic tracking-tighter">Biblioteca Global</h2>
          <p className="text-slate-400">Administración central de archivos y conocimiento indexado.</p>
        </div>

        <form onSubmit={handleSearchSubmit} className="flex flex-wrap gap-4">
          <div className="flex-1 min-w-[300px] relative">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500" size={18} />
            <input 
              type="text"
              placeholder="Buscar por título o contenido..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full bg-[#1e293b] border border-slate-800 rounded-2xl py-3 pl-12 pr-4 text-sm text-white focus:outline-none focus:border-blue-500 transition-all"
            />
          </div>
          
          <select 
            value={selectedCat}
            onChange={(e) => setSelectedCat(e.target.value)}
            className="bg-[#1e293b] border border-slate-800 rounded-2xl px-4 py-3 text-sm text-white focus:outline-none focus:border-blue-500"
          >
            <option value="">Todas las categorías</option>
            {categorias.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
          </select>

          <button type="submit" className="bg-blue-600 hover:bg-blue-500 text-white px-6 rounded-2xl font-bold text-sm transition-all shadow-lg shadow-blue-600/20">
            Filtrar
          </button>
        </form>
      </header>

      {/* LISTADO DE MANUALES */}
      <div className="flex-1 overflow-y-auto pr-2 scrollbar-thin scrollbar-thumb-slate-800">
        {loading ? (
          <div className="h-64 flex items-center justify-center text-blue-400 animate-pulse font-bold uppercase tracking-widest">
            Consultando Base de Datos...
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-3">
            {manuales.map(m => (
              <ManualListItem key={m.id} manual={m} onDelete={handleDelete} onDownload={handleDownload}/>
            ))}
            
            {manuales.length === 0 && (
              <div className="py-20 text-center bg-[#1e293b]/20 border-2 border-dashed border-slate-800 rounded-3xl">
                <p className="text-slate-500 italic">No se encontraron manuales con esos criterios.</p>
              </div>
            )}
          </div>
        )}
      </div>

      {/* PAGINACIÓN */}
      <footer className="mt-6 flex justify-between items-center bg-[#1e293b] p-4 rounded-2xl border border-slate-800">
        <span className="text-xs text-slate-500 font-bold uppercase tracking-widest">Página {page} de {totalPages}</span>
        <div className="flex gap-2">
          <button 
            disabled={page === 1}
            onClick={() => setPage(p => p - 1)}
            className="px-4 py-2 bg-slate-800 rounded-xl text-xs font-bold hover:bg-slate-700 disabled:opacity-30 transition-all"
          >
            Anterior
          </button>
          <button 
            disabled={page === totalPages}
            onClick={() => setPage(p => p + 1)}
            className="px-4 py-2 bg-blue-600 rounded-xl text-xs font-bold hover:bg-blue-500 disabled:opacity-30 transition-all"
          >
            Siguiente
          </button>
        </div>
      </footer>
    </div>
  );
};

//FILA DE LA BIBLIOTECA
const ManualListItem = ({ manual, onDelete, onDownload }) => (
  <div className="bg-[#1e293b] border border-slate-800 p-4 rounded-2xl hover:border-slate-700 transition-all group">
    <div className="flex items-center gap-6">
      <div className="w-12 h-12 bg-slate-800 rounded-xl flex items-center justify-center text-slate-400 group-hover:text-blue-400 transition-colors">
        <FileText size={24} />
      </div>
      
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-3">
          <h4 className="text-sm font-bold text-white truncate">{manual.titulo}</h4>
          <span className="text-[10px] bg-blue-600/10 text-blue-500 px-2 py-0.5 rounded-md font-black uppercase">v{manual.version}</span>
        </div>
        <div className="flex gap-4 mt-2">
          <div className="flex items-center gap-1.5 text-[10px] text-slate-500">
            <Calendar size={12} />
            <span>{new Date(manual.fechaSubida).toLocaleDateString()}</span>
          </div>
          <div className="flex items-center gap-1.5 text-[10px] text-slate-500">
            <Database size={12} />
            <span>{(manual.tamañoBytes / 1024 / 1024).toFixed(2)} MB</span>
          </div>
        </div>
      </div>

      <div className="flex items-center gap-2">
        <button
         onClick={() => onDownload(manual.id)}
          className="p-2.5 bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 rounded-xl transition-all"
          title="Descargar"
        >
          <Download size={16} />
        </button>
        {/*<button className="p-2.5 bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 rounded-xl transition-all" title="Editar">
          <Edit3 size={16} />
        </button>*/}
        <button 
          onClick={() => onDelete(manual.id)}
          className="p-2.5 bg-red-500/10 text-red-500/60 hover:text-red-500 hover:bg-red-500/20 rounded-xl transition-all" 
          title="Eliminar"
        >
          <Trash2 size={16} />
        </button>
      </div>
    </div>
  </div>
);

export default LibraryPage;