import React, { useState, useEffect } from 'react';
import { FolderTree, Folder, FileText, ChevronRight, ChevronDown, BookOpen, Database } from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const CategoriesPage = () => {
  const [categorias, setCategorias] = useState([]);
  const [selectedCat, setSelectedCat] = useState(null);
  const [manuales, setManuales] = useState([]);
  const [loadingManuales, setLoadingManuales] = useState(false);

  //arbol de categorías
  useEffect(() => {
    const fetchTree = async () => {
      try {
        const res = await fetch(`${API_BASE_URL}/Categorias?soloActivas=true&incluirSubcategorias=true`);
        const data = await res.json();
        // El backend devuelve la jerarquía completa
        setCategorias(data.categorias || []);
      } catch (err) {
        console.error("Error al cargar el árbol de conocimiento:", err);
      }
    };
    fetchTree();
  }, []);

  //manuales pertenecientes a la rama seleccionada
  const handleSelectCategory = async (catId) => {
    setSelectedCat(catId);
    setLoadingManuales(true);
    try {
      const res = await fetch(`${API_BASE_URL}/Manuales?categoriaId=${catId}`);
      const data = await res.json();
      // Filtramos manuales por el ID de la categoría elegida
      setManuales(data.manuales || []);
    } catch (err) {
      console.error("Error al obtener manuales de la rama:", err);
    } finally {
      setLoadingManuales(false);
    }
  };


  return (
    <div className="flex h-full bg-[#0f172a] overflow-hidden">
      
      {/* EXPLORADOR DE RAMAS */}
      <aside className="w-80 border-r border-slate-800 p-6 overflow-y-auto bg-[#1e293b]/20">
        <div className="flex items-center gap-3 mb-8">
          <div className="p-2 bg-blue-600/10 rounded-lg text-blue-400">
            <FolderTree size={20} />
          </div>
          <h2 className="font-black uppercase italic tracking-tighter text-slate-100">Ramas Técnicas</h2>
        </div>
        
        <div className="space-y-1">
          {categorias.map(cat => (
            <CategoryNode 
              key={cat.id} 
              node={cat} 
              onSelect={handleSelectCategory} 
              selectedId={selectedCat}
            />
          ))}
          {categorias.length === 0 && (
            <p className="text-xs text-slate-600 italic px-2">No hay ramas configuradas.</p>
          )}
        </div>
      </aside>

      {/* VISUALIZADOR DE ARCHIVOS */}
      <main className="flex-1 p-8 overflow-y-auto scrollbar-thin scrollbar-thumb-slate-800">
        {selectedCat ? (
          <div className="max-w-5xl mx-auto space-y-8 animate-in fade-in slide-in-from-bottom-2 duration-300">
            <header className="flex justify-between items-end border-b border-slate-800 pb-6">
              <div>
                <p className="text-[10px] text-blue-500 font-black uppercase tracking-[0.2em] mb-1">Explorando Contenido</p>
                <h3 className="text-3xl font-bold text-white tracking-tight">Archivos en esta Rama</h3>
              </div>
              <div className="text-right">
                <span className="text-xs font-mono text-slate-500 bg-slate-900 px-3 py-1 rounded-full border border-slate-800">
                  {manuales.length} OBJETOS INDEXADOS
                </span>
              </div>
            </header>

            {loadingManuales ? (
              <div className="flex flex-col items-center justify-center py-20 text-blue-400 gap-4">
                <div className="w-10 h-10 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
                <span className="text-xs font-black uppercase tracking-widest">Consultando Qdrant...</span>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {manuales.map(m => (
                  <ManualFileCard key={m.id} manual={m} />
                ))}
                {manuales.length === 0 && (
                  <div className="col-span-full py-24 text-center border-2 border-dashed border-slate-800 rounded-[2rem] bg-slate-900/10">
                    <BookOpen size={48} className="mx-auto mb-4 text-slate-700" />
                    <p className="text-slate-500 font-medium">Esta rama todavía no tiene conocimiento inyectado.</p>
                  </div>
                )}
              </div>
            )}
          </div>
        ) : (
          <div className="h-full flex flex-col items-center justify-center text-slate-600 space-y-4 opacity-40">
            <div className="p-8 rounded-full bg-slate-800/30 border border-slate-800">
              <FolderTree size={64} />
            </div>
            <p className="text-lg font-bold uppercase tracking-tighter">Seleccioná una rama para ver sus manuales</p>
          </div>
        )}
      </main>
    </div>
  );
};

const CategoryNode = ({ node, onSelect, selectedId }) => {
  const [isOpen, setIsOpen] = useState(false);

  const hasChildren = node.subCategorias && node.subCategorias.length > 0;
  const isSelected = selectedId === node.id;

  return (
    <div className="select-none">
      <div 
        onClick={() => {
          if (hasChildren) setIsOpen(!isOpen);
          onSelect(node.id);
        }}
        className={`
          flex items-center gap-2 p-2.5 rounded-xl cursor-pointer transition-all group
          ${isSelected 
            ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20' 
            : 'hover:bg-slate-800 text-slate-400 hover:text-slate-200'}
        `}
      >
        <div className="w-4 flex items-center justify-center">
          {hasChildren && (
            isOpen ? <ChevronDown size={14} /> : <ChevronRight size={14} />
          )}
        </div>
        <Folder size={16} className={isSelected ? 'text-white' : 'text-slate-500 group-hover:text-blue-400'} />
        <span className="text-sm font-semibold tracking-tight truncate">{node.nombre}</span>
      </div>

      {hasChildren && isOpen && (
        <div className="ml-5 border-l border-slate-800 pl-2 mt-1 space-y-1">
          {node.subCategorias.map(child => (
            <CategoryNode 
              key={child.id} 
              node={child} 
              onSelect={onSelect} 
              selectedId={selectedId} 
            />
          ))}
        </div>
      )}
    </div>
  );
};

const ManualFileCard = ({ manual }) => (
  <div className="bg-[#1e293b] p-5 rounded-3xl border border-slate-800 hover:border-blue-500/40 transition-all group cursor-pointer hover:shadow-2xl hover:shadow-blue-900/10">
    <div className="flex flex-col h-full justify-between">
      <div className="flex gap-4 mb-6">
        <div className="p-3 bg-slate-800 rounded-2xl text-slate-500 group-hover:text-blue-400 group-hover:bg-blue-600/10 transition-all">
          <FileText size={28} />
        </div>
        <div className="flex-1 min-w-0">
          <h4 className="text-sm font-bold text-slate-100 truncate group-hover:text-blue-400 transition-colors">
            {manual.titulo}
          </h4>
          <span className="inline-block mt-1 px-2 py-0.5 bg-slate-900 text-blue-500 text-[9px] font-black uppercase rounded border border-blue-500/20">
            Versión {manual.version || '1.0'}
          </span>
        </div>
      </div>
      
      <div className="flex items-center justify-between pt-4 border-t border-slate-800/50">
        <div className="flex items-center gap-1.5 text-slate-500">
          <Database size={12} />
          <span className="text-[10px] font-mono font-bold uppercase">
            {(manual.tamañoBytes / 1024 / 1024).toFixed(2)} MB
          </span>
        </div>
      </div>
    </div>
  </div>
);

export default CategoriesPage;