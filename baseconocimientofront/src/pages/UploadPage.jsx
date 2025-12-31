import React, { useState, useEffect } from 'react';
import { Upload, File, CheckCircle, AlertCircle, Loader2, Tag, Info } from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const UploadPage = () => {
  // Estados para el formulario
  const [file, setFile] = useState(null);
  const [title, setTitle] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [version, setVersion] = useState('1.0');
  const [description, setDescription] = useState('');
  
  // Estados de UI y datos
  const [categories, setCategories] = useState([]);
  const [isUploading, setIsUploading] = useState(false);
  const [message, setMessage] = useState({ type: '', text: '' });

  //Cargar categorías para el selector
  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const res = await fetch(`${API_BASE_URL}/Categorias?soloActivas=true`);
        const data = await res.json();
        setCategories(data.categorias || []);
      } catch (err) {
        console.error("Error cargando categorías:", err);
      }
    };
    fetchCategories();
  }, []);

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
      // Sugerir título basado en el nombre del archivo
      if (!title) setTitle(selectedFile.name.replace('.pdf', ''));
      setMessage({ type: '', text: '' });
    } else {
      setMessage({ type: 'error', text: 'Por favor, selecciona un archivo PDF válido.' });
    }
  };

  //Enviar el archivo y metadatos a la API
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!file || !categoryId || !title) {
      setMessage({ type: 'error', text: 'Completá los campos obligatorios (*)' });
      return;
    }

    setIsUploading(true);
    setMessage({ type: 'info', text: 'Subiendo y procesando conocimiento...' });

    // Usamos FormData para enviar el archivo físico
    const formData = new FormData();
    formData.append('Archivo', file);
    formData.append('Titulo', title);
    formData.append('CategoriaId', categoryId);
    formData.append('Version', version);
    formData.append('Descripcion', description);
    formData.append('UsuarioId', "00000000-0000-0000-0000-000000000000"); // ID de prueba

    try {
      const response = await fetch(`${API_BASE_URL}/Manuales`, {
        method: 'POST',
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
        
        <header className="mb-10">
          <h2 className="text-3xl font-black text-white uppercase italic tracking-tighter">Cargar Conocimiento</h2>
          <p className="text-slate-400 mt-2">Alimentá el motor RAG de Inuzaru subiendo manuales técnicos en formato PDF o TXT.</p>
        </header>

        <form onSubmit={handleSubmit} className="space-y-6">
          
          <div className={`
            relative border-2 border-dashed rounded-3xl p-10 transition-all text-center
            ${file ? 'border-blue-500 bg-blue-600/5' : 'border-slate-800 hover:border-slate-700 bg-slate-900/30'}
          `}>
            <input 
              type="file" 
              accept=".pdf" 
              onChange={handleFileChange}
              className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
            />
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
                <div>
                  <p className="text-slate-300 font-medium">Arrastrá tu PDF o hacé clic aquí</p>
                  <p className="text-xs text-slate-500 mt-1">Límite máximo: 50MB</p>
                </div>
              )}
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* TÍTULO */}
            <div className="space-y-2">
              <label className="text-[10px] font-black uppercase tracking-widest text-slate-500 flex items-center gap-2">
                <File size={12} /> Título del Manual *
              </label>
              <input 
                type="text"
                value={title}
                onChange={e => setTitle(e.target.value)}
                placeholder="Ej: Manual de Mantenimiento"
                className="w-full bg-[#1e293b] border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:outline-none focus:border-blue-500 transition-colors"
              />
            </div>

            {/* CATEGORÍA */}
            <div className="space-y-2">
              <label className="text-[10px] font-black uppercase tracking-widest text-slate-500 flex items-center gap-2">
                <Tag size={12} /> Rama / Categoría *
              </label>
              <select 
    value={categoryId}
    onChange={e => setCategoryId(e.target.value)}
    className="w-full bg-[#1e293b] border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:outline-none focus:border-blue-500 transition-colors appearance-none"
  >
    <option value="">Seleccioná una rama...</option>
    
    {/* AQUÍ CAMBIAMOS EL MAP: Llamamos a flattenCategories pasándole el estado 'categories' */}
    {flattenCategories(categories).map(cat => (
      <option key={cat.id} value={cat.id}>
        {cat.nombre}
      </option>
    ))}
  </select>
            </div>
          </div>

          {/* VERSIÓN Y DESCRIPCIÓN */}
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
              <div className="md:col-span-1 space-y-2">
                <label className="text-[10px] font-black uppercase tracking-widest text-slate-500">Versión</label>
                <input 
                  type="text"
                  value={version}
                  onChange={e => setVersion(e.target.value)}
                  className="w-full bg-[#1e293b] border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:outline-none"
                />
              </div>
              <div className="md:col-span-3 space-y-2">
                <label className="text-[10px] font-black uppercase tracking-widest text-slate-500 flex items-center gap-2">
                  <Info size={12} /> Resumen / Notas
                </label>
                <input 
                  type="text"
                  value={description}
                  onChange={e => setDescription(e.target.value)}
                  placeholder="Breve descripción del contenido..."
                  className="w-full bg-[#1e293b] border border-slate-800 rounded-xl px-4 py-3 text-sm text-white focus:outline-none"
                />
              </div>
            </div>
          </div>

          {/* MENSAJES DE ESTADO */}
          {message.text && (
            <div className={`p-4 rounded-2xl flex items-center gap-3 animate-in fade-in zoom-in duration-300 ${
              message.type === 'error' ? 'bg-red-500/10 text-red-400 border border-red-500/20' : 
              message.type === 'success' ? 'bg-green-500/10 text-green-400 border border-green-500/20' : 
              'bg-blue-500/10 text-blue-400 border border-blue-500/20'
            }`}>
              {message.type === 'error' ? <AlertCircle size={20} /> : <CheckCircle size={20} />}
              <span className="text-sm font-medium">{message.text}</span>
            </div>
          )}

          {/* BOTÓN DE ACCIÓN */}
          <button 
            type="submit"
            disabled={isUploading}
            className={`
              w-full py-4 rounded-2xl font-black uppercase tracking-widest text-sm transition-all flex items-center justify-center gap-3
              ${isUploading ? 'bg-slate-800 text-slate-500 cursor-not-allowed' : 'bg-blue-600 hover:bg-blue-500 text-white shadow-xl shadow-blue-600/20'}
            `}
          >
            {isUploading ? (
              <><Loader2 className="animate-spin" /> Procesando Chunks...</>
            ) : (
              'Inyectar en Base de Conocimiento'
            )}
          </button>
        </form>
      </div>
    </div>
  );
};

export default UploadPage;