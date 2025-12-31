import React, { useState, useEffect } from 'react';
import { Plus, Tag, FolderTree, Palette, Type, Save, AlertCircle, CheckCircle } from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const AdminCategoriasPage = () => {
  const [nombre, setNombre] = useState('');
  const [descripcion, setDescripcion] = useState('');
  const [padreId, setPadreId] = useState('');
  const [color, setColor] = useState('#3b82f6');
  const [icono, setIcono] = useState('folder');
  const [categoriasExistentes, setCategoriasExistentes] = useState([]);
  const [status, setStatus] = useState({ type: '', msg: '' });

  const cargarCategorias = async () => {
    try {
      const res = await fetch(`${API_BASE_URL}/Categorias?soloActivas=true`);
      const data = await res.json();
      setCategoriasExistentes(data.categorias || []);
    } catch (err) { console.error(err); }
  };

  useEffect(() => { cargarCategorias(); }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setStatus({ type: 'info', msg: 'Generando nueva rama...' });

    const command = {
      nombre: nombre.toLowerCase(),
      descripcion,
      categoriaPadreId: padreId || null, // Sincronizado con tu DB
      color,
      icono,
      orden: 1,
      esActiva: true // Campo real de tu tabla
    };

    try {
      const res = await fetch(`${API_BASE_URL}/Categorias`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(command)
      });

      if (res.ok) {
        setStatus({ type: 'success', msg: `¡Rama "${nombre}" creada con éxito!` });
        setNombre('');
        setDescripcion('');
        setPadreId('');
        cargarCategorias(); // Recargamos la lista
      } else {
        throw new Error('Error al crear la categoría');
      }
    } catch (err) {
      setStatus({ type: 'error', msg: err.message });
    }
  };

  return (
    <div className="h-full p-8 bg-[#0f172a] overflow-y-auto scrollbar-thin scrollbar-thumb-slate-800">
      <div className="max-w-2xl mx-auto bg-[#1e293b] rounded-[2.5rem] border border-slate-800 p-8 shadow-2xl">
        <header className="mb-8 flex items-center gap-4 border-b border-slate-800 pb-6">
          <div className="p-3 bg-blue-600 rounded-2xl text-white shadow-lg shadow-blue-600/20">
            <Plus size={24} />
          </div>
          <div>
            <h2 className="text-2xl font-black text-white uppercase italic tracking-tighter">Nueva Categoría</h2>
            <p className="text-[10px] text-blue-400 font-bold uppercase tracking-widest">Alimentar la jerarquía de Inuzaru</p>
          </div>
        </header>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-2">
              <label className="text-[10px] font-black text-slate-500 uppercase px-1">Nombre de la Rama</label>
              <input value={nombre} onChange={e => setNombre(e.target.value)} className="w-full bg-slate-900 border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:border-blue-500 outline-none transition-all" placeholder="ej: sap" required />
            </div>
            <div className="space-y-2">
              <label className="text-[10px] font-black text-slate-500 uppercase px-1">Colgar de (Padre)</label>
              <select value={padreId} onChange={e => setPadreId(e.target.value)} className="w-full bg-slate-900 border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:border-blue-500 outline-none appearance-none cursor-pointer">
                <option value="">Rama Raíz (Principal)</option>
                {categoriasExistentes.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
              </select>
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-[10px] font-black text-slate-500 uppercase px-1">Descripción Técnica</label>
            <textarea value={descripcion} onChange={e => setDescripcion(e.target.value)} className="w-full bg-slate-900 border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:border-blue-500 outline-none h-24 resize-none" placeholder="Breve descripción del conocimiento en esta rama..." />
          </div>

          <div className="grid grid-cols-2 gap-6">
            <div className="space-y-2">
              <label className="text-[10px] font-black text-slate-500 uppercase px-1">Color Identificador</label>
              <div className="flex gap-3">
                <input type="color" value={color} onChange={e => setColor(e.target.value)} className="w-12 h-11 bg-slate-900 border border-slate-800 rounded-xl p-1 cursor-pointer" />
                <input value={color} readOnly className="flex-1 bg-slate-900 border border-slate-800 rounded-xl px-4 py-3 text-xs font-mono text-slate-400" />
              </div>
            </div>
            <div className="space-y-2">
              <label className="text-[10px] font-black text-slate-500 uppercase px-1">Icono (Lucide)</label>
              <input value={icono} onChange={e => setIcono(e.target.value)} className="w-full bg-slate-900 border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:border-blue-500 outline-none" placeholder="ej: monitor, cpu, printer..." />
            </div>
          </div>

          {status.msg && (
            <div className={`p-4 rounded-2xl text-xs font-bold flex items-center gap-3 animate-in zoom-in-95 ${status.type === 'error' ? 'bg-red-500/10 text-red-400 border border-red-500/20' : 'bg-blue-500/10 text-blue-400 border border-blue-500/20'}`}>
              {status.type === 'error' ? <AlertCircle size={16} /> : <CheckCircle size={16} />}
              {status.msg}
            </div>
          )}

          <button type="submit" className="w-full bg-blue-600 hover:bg-blue-500 text-white font-black uppercase py-4 rounded-2xl shadow-xl shadow-blue-600/30 transition-all flex items-center justify-center gap-2">
            <Save size={18} /> Inyectar Rama en la DB
          </button>
        </form>
      </div>
    </div>
  );
};

export default AdminCategoriasPage;