import React, { useState, useEffect } from 'react';
import { Upload, File, CheckCircle, AlertCircle, Loader2, Tag, Info } from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const UploadPage = ({ user }) => {
  const [file, setFile] = useState(null);
  const [title, setTitle] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [version, setVersion] = useState('1.0');
  const [description, setDescription] = useState('');
  
  const [categories, setCategories] = useState([]);
  const [isUploading, setIsUploading] = useState(false);
  const [message, setMessage] = useState({ type: '', text: '' });

  const token = localStorage.getItem('inuzaru_token');

  // Cargar categorías protegidas por JWT
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const res = await fetch(`${API_BASE_URL}/Categorias?soloActivas=true`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        const data = await res.json();
        setCategories(data.categorias || []);
      } catch (err) {
        console.error("Error cargando categorías:", err);
      }
    };
    if (token) fetchCategories();
  }, [token]);

  const flattenCategories = (cats, level = 0) => {
    let flattened = [];
    cats.forEach(cat => {
      flattened.push({ id: cat.id, nombre: `${'— '.repeat(level)}${cat.nombre}` });
      if (cat.subCategorias && cat.subCategorias.length > 0) {
        flattened = [...flattened, ...flattenCategories(cat.subCategorias, level + 1)];
      }
    });
    return flattened;
  };

  const handleFileChange = (e) => {
    const selectedFile = e.target.files[0];
    if (selectedFile && selectedFile.type === "application/pdf") {
      setFile(selectedFile);
      if (!title) setTitle(selectedFile.name.replace('.pdf', ''));
      setMessage({ type: '', text: '' });
    } else {
      setMessage({ type: 'error', text: 'Seleccioná un PDF válido.' });
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!file || !categoryId || !title) {
      setMessage({ type: 'error', text: 'Completá los campos obligatorios (*)' });
      return;
    }

    setIsUploading(true);
    setMessage({ type: 'info', text: 'Subiendo y procesando conocimiento...' });

    const formData = new FormData();
    formData.append('Archivo', file);
    formData.append('Titulo', title);
    formData.append('CategoriaId', categoryId);
    formData.append('Version', version);
    formData.append('Descripcion', description);
    formData.append('UsuarioId', user.id); // ID REAL DEL USUARIO

    try {
      const response = await fetch(`${API_BASE_URL}/Manuales`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData,
      });

      const data = await response.json();

      if (response.ok) {
        setMessage({ 
          type: 'success', 
          text: `¡Éxito! Manual procesado en ${data.chunksProcesados} fragmentos.` 
        });
        setFile(null);
        setTitle('');
        setDescription('');
      } else {
        throw new Error(data.mensaje || 'Error al subir el manual');
      }
    } catch (err) {
      setMessage({ type: 'error', text: err.message });
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="h-full overflow-y-auto bg-[#0f172a] p-8">
      <div className="max-w-3xl mx-auto">
        <header className="mb-10 text-center md:text-left">
          <h2 className="text-3xl font-black text-white uppercase italic tracking-tighter">Cargar Conocimiento</h2>
          <p className="text-slate-400 mt-2 italic">Identificado como: <span className="text-blue-400">{user.nombreCompleto}</span></p>
        </header>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className={`relative border-2 border-dashed rounded-[2.5rem] p-10 transition-all text-center ${file ? 'border-blue-500 bg-blue-600/5' : 'border-slate-800 hover:border-slate-700 bg-slate-900/30'}`}>
            <input type="file" accept=".pdf" onChange={handleFileChange} className="absolute inset-0 w-full h-full opacity-0 cursor-pointer" />
            <div className="flex flex-col items-center gap-4">
              <div className={`p-4 rounded-2xl ${file ? 'bg-blue-600 text-white' : 'bg-slate-800 text-slate-500'}`}>
                <Upload size={32} />
              </div>
              {file ? (
                <div>
                  <p className="text-blue-400 font-bold">{file.name}</p>
                  <p className="text-[10px] text-slate-500 uppercase mt-1">{(file.size / 1024 / 1024).toFixed(2)} MB</p>
                </div>
              ) : (
                <p className="text-slate-300 font-medium tracking-tight">Arrastrá tu PDF o hacé clic aquí (Max 50MB)</p>
              )}
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-2">
              <label className="text-[10px] font-black uppercase tracking-widest text-slate-500 flex items-center gap-2 ml-1"><File size={12} /> Título *</label>
              <input type="text" value={title} onChange={e => setTitle(e.target.value)} className="w-full bg-[#1e293b] border border-slate-800 rounded-2xl px-5 py-4 text-sm text-white focus:outline-none focus:border-blue-500" placeholder="Ej: Manual Fujitsu F9870" />
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-black uppercase tracking-widest text-slate-500 flex items-center gap-2 ml-1"><Tag size={12} /> Rama *</label>
              <select value={categoryId} onChange={e => setCategoryId(e.target.value)} className="w-full bg-[#1e293b] border border-slate-800 rounded-2xl px-5 py-4 text-sm text-white focus:outline-none focus:border-blue-500 appearance-none">
                <option value="">Seleccioná una rama...</option>
                {flattenCategories(categories).map(cat => (
                  <option key={cat.id} value={cat.id}>{cat.nombre}</option>
                ))}
              </select>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <div className="md:col-span-1 space-y-2">
              <label className="text-[10px] font-black uppercase tracking-widest text-slate-500 ml-1">Versión</label>
              <input type="text" value={version} onChange={e => setVersion(e.target.value)} className="w-full bg-[#1e293b] border border-slate-800 rounded-2xl px-5 py-4 text-sm text-white focus:outline-none text-center" />
            </div>
            <div className="md:col-span-3 space-y-2">
              <label className="text-[10px] font-black uppercase tracking-widest text-slate-500 flex items-center gap-2 ml-1"><Info size={12} /> Resumen</label>
              <input type="text" value={description} onChange={e => setDescription(e.target.value)} className="w-full bg-[#1e293b] border border-slate-800 rounded-2xl px-5 py-4 text-sm text-white focus:outline-none" placeholder="Notas sobre el contenido..." />
            </div>
          </div>

          {message.text && (
            <div className={`p-4 rounded-2xl flex items-center gap-3 animate-in fade-in zoom-in duration-300 ${message.type === 'error' ? 'bg-red-500/10 text-red-400 border border-red-500/20' : message.type === 'success' ? 'bg-green-500/10 text-green-400 border border-green-500/20' : 'bg-blue-500/10 text-blue-400 border border-blue-500/20'}`}>
              {message.type === 'error' ? <AlertCircle size={20} /> : <CheckCircle size={20} />}
              <span className="text-sm font-bold uppercase tracking-tight">{message.text}</span>
            </div>
          )}

          <button type="submit" disabled={isUploading} className={`w-full py-5 rounded-[1.5rem] font-black uppercase tracking-[0.2em] text-sm transition-all flex items-center justify-center gap-3 ${isUploading ? 'bg-slate-800 text-slate-500' : 'bg-blue-600 hover:bg-blue-500 text-white shadow-xl shadow-blue-600/30'}`}>
            {isUploading ? <><Loader2 className="animate-spin" /> Procesando Chunks...</> : 'Inyectar en Motor RAG'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default UploadPage;