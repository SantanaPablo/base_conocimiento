import React, { useState, useEffect, useRef, memo, startTransition } from 'react';
import { Send, User, Search, Upload, Book, RefreshCcw, Trash2, Activity, AlertTriangle, ShieldCheck, Menu, X, FileText } from 'lucide-react';
import InuzaruAvatar from '../assets/inuzaru_avatar.png';
import SubirManuales from './SubirManuales';
import Biblioteca from './Biblioteca';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const InuzaruChat = () => {
  const [activeTab, setActiveTab] = useState('consultar');
  const [conversacionId, setConversacionId] = useState(null);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [servicesStatus, setServicesStatus] = useState({
    api: 'loading', qdrant: 'loading', database: 'loading', ollama: 'loading', redis: 'loading'
  });

  const [messages, setMessages] = useState([
    { role: 'bot', content: '¡Hola! Soy Inuzaru. He olfateado tus manuales y estoy listo para responder. ¿Qué deseas saber?' }
  ]);

  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const scrollRef = useRef(null);

  useEffect(() => {
    const initChat = async () => {
      try {
        const res = await fetch(`${API_BASE_URL}/Conversaciones/crear`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ usuarioId: "pablo_dev" })
        });
        
        if (!res.ok) throw new Error("No se pudo establecer conexión con Redis");
        
        const data = await res.json();
        setConversacionId(data.conversacionId);
      } catch (err) {
        console.error("Error iniciando sesión:", err);
        setMessages(prev => [...prev, { 
          role: 'bot', 
          content: '⚠️ Che, no pude activar mi memoria (Redis). Fijate si el contenedor de Docker está corriendo.' 
        }]);
      }
    };
    initChat();
  }, []);

  useEffect(() => {
    scrollRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages.length, isLoading]);

  //Health Checks
  const checkHealth = async () => {
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL.replace('/api', '')}/health`);
      if (!response.ok && response.status !== 503) throw new Error();
      
      const data = await response.json();
      const getStatus = (name) => data.servicios?.find(s => s.nombre === name)?.estado === 'Healthy' ? 'up' : 'down';
      
      setServicesStatus({
        api: 'up',
        database: getStatus('PostgreSQL'),
        qdrant: getStatus('Qdrant'),
        ollama: getStatus('Ollama'),
        redis: getStatus('Redis')
      });
    } catch {
      setServicesStatus({ api: 'down', database: 'down', qdrant: 'down', ollama: 'down', redis: 'down' });
    }
  };

  useEffect(() => {
    checkHealth();
    const i = setInterval(checkHealth, 30000);
    return () => clearInterval(i);
  }, []);

  const handleResetChat = async () => {
    if (!conversacionId || !window.confirm("¿Limpiar memoria?")) return;
    try {
      await fetch(`${API_BASE_URL}/Conversaciones/${conversacionId}`, { method: 'DELETE' });
      setMessages([{ role: 'bot', content: '¡Olfato reseteado! He olvidado lo anterior. ¿En qué más puedo ayudarte?' }]);
    } catch (err) { console.error(err); }
  };

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim() || isLoading || !conversacionId) return;

    const question = input;
    startTransition(() => { setMessages(prev => [...prev, { role: 'user', content: question }]); });
    setInput('');
    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/Conversaciones/preguntar`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          pregunta: question, 
          conversacionId, 
          usuarioId: "pablo_dev", 
          categoria: null, 
          topK: 3 
        })
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.mensaje || `Error del servidor (${response.status})`);
      }

      const data = await response.json();
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: data.respuesta,
        fuentes: data.fuentes 
      }]);
    } catch (error) {
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: `¡Grrr! Algo salió mal: ${error.message}` 
      }]);
    } finally { 
      setIsLoading(false); 
    }
  };

  return (
    <div className="flex h-screen overflow-hidden bg-[#0f172a] text-slate-200">
      
      {/* OVERLAY PARA MOBILE */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 bg-black/60 z-40 lg:hidden backdrop-blur-sm" onClick={() => setIsMobileMenuOpen(false)} />
      )}

      {/* SIDEBAR */}
      <aside className={`fixed inset-y-0 left-0 z-50 w-72 bg-[#1e293b] border-r border-blue-500/20 p-6 transition-transform duration-300 ease-in-out transform flex flex-col ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'} lg:translate-x-0 lg:static lg:inset-auto`}>
        
        {/* CABECERA SIDEBAR */}
        <div className="flex items-center justify-between mb-8 shrink-0 text-white">
          <div className="flex items-center gap-3">
            <img src={InuzaruAvatar} className="w-10 h-10 rounded-full border border-blue-400" />
            <div>
              <h1 className="font-black text-xl uppercase italic leading-none tracking-tighter">Inuzaru</h1>
              <p className="text-[10px] text-blue-400 font-bold uppercase tracking-widest mt-1">RAG ENGINE 1.0</p>
            </div>
          </div>
          <button className="lg:hidden" onClick={() => setIsMobileMenuOpen(false)}><X size={24} /></button>
        </div>

        {/* NAVEGACIÓN */}
        <nav className="space-y-1 mb-1 shrink-0">
          <NavItem icon={<Search size={18} />} label="Consultar IA" active={activeTab === 'consultar'} onClick={() => { setActiveTab('consultar'); setIsMobileMenuOpen(false); }} />
          <NavItem icon={<Upload size={18} />} label="Subir Manuales" active={activeTab === 'subir'} onClick={() => { setActiveTab('subir'); setIsMobileMenuOpen(false); }} />
          <NavItem icon={<Book size={18} />} label="Biblioteca" active={activeTab === 'biblioteca'} onClick={() => { setActiveTab('biblioteca'); setIsMobileMenuOpen(false); }} />
          
          <div onClick={handleResetChat} className="flex items-center gap-3 p-3 rounded-xl cursor-pointer text-red-400/60 hover:bg-red-500/10 hover:text-red-400 transition-all mt-4 border border-transparent hover:border-red-500/20">
             <Trash2 size={18} />
             <span className="text-sm font-semibold">Limpiar Memoria</span>
          </div>
        </nav>

        {/* LOGO CENTRAL (Debajo del menú y arriba del Health) */}
        <div className="flex-1 flex items-center justify-center pointer-events-none my-2 relative">
  <img 
    src={InuzaruAvatar} 
    className="
      w-full h-full scale-150
      object-contain 
      opacity-125 brightness-110 contrast-125
      transition-all duration-700
    "
    style={{ 
      /* La máscara radial ayuda a que no se note el corte en los bordes */
      maskImage: 'radial-gradient(circle, black 40%, transparent 95%)',
      WebkitMaskImage: 'radial-gradient(circle, black 40%, transparent 95%)'
    }}
  />
</div>

        {/* HEALTH CHECK (Abajo de todo) */}
        <div className="shrink-0">
          <HealthPanel servicesStatus={servicesStatus} onRefresh={checkHealth} />
        </div>
      </aside>

      {/* ÁREA PRINCIPAL */}
      <main className="flex-1 flex flex-col min-h-0 bg-gradient-to-b from-[#0f172a] to-[#1e293b]">
        
        <header className="h-16 shrink-0 border-b border-slate-800 flex items-center px-4 lg:px-8 justify-between bg-[#0f172a]/50 backdrop-blur-md">
          <div className="flex items-center gap-4">
            <button className="lg:hidden text-slate-200" onClick={() => setIsMobileMenuOpen(true)}><Menu size={24} /></button>
            <div className="flex items-center gap-3">
              <div className={`w-2 h-2 rounded-full animate-pulse shadow-lg ${servicesStatus.api === 'up' ? 'bg-blue-500 shadow-blue-500/50' : 'bg-red-500 shadow-red-500/50'}`}></div>
              <span className="text-xs font-bold text-slate-400 uppercase tracking-widest italic">Terminal Técnica</span>
            </div>
          </div>
          {conversacionId && <span className="hidden sm:inline text-[9px] font-mono text-slate-600">SID: {conversacionId}</span>}
        </header>

        <div className="flex-1 flex flex-col min-h-0">
          {activeTab === 'consultar' ? (
            <>
              {servicesStatus.api === 'down' && (
                <div className="bg-red-600/20 border-b border-red-500/20 text-red-400 text-[10px] py-1 text-center font-bold uppercase tracking-widest animate-pulse">
                  Conexión perdida con el núcleo de Inuzaru
                </div>
              )}

              <div className="flex-1 min-h-0 overflow-y-auto p-4 lg:p-6 space-y-6 scrollbar-thin scrollbar-thumb-slate-800">
                {messages.map((m, i) => <ChatMessage key={i} message={m} />)}
                {isLoading && (
                  <div className="flex gap-4 items-center opacity-60 animate-in fade-in">
                     <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center border border-blue-500/20">
                        <RefreshCcw size={16} className="animate-spin text-blue-400" />
                     </div>
                     <div className="text-xs italic text-blue-400/80 tracking-tight">Inuzaru olfateando...</div>
                  </div>
                )}
                <div ref={scrollRef} className="h-2" />
              </div>

              <div className="p-4 lg:p-6 bg-gradient-to-t from-[#0f172a]">
                <form onSubmit={handleSendMessage} className="relative max-w-4xl mx-auto group">
                    <div className="absolute -inset-0.5 bg-gradient-to-r from-blue-600 to-cyan-600 rounded-2xl blur opacity-20 group-focus-within:opacity-40 transition"></div>
                    <input
                        value={input}
                        onChange={e => setInput(e.target.value)}
                        placeholder={servicesStatus.api === 'down' ? "Sistema fuera de servicio..." : "Escribe tu consulta..."}
                        disabled={!conversacionId || isLoading || servicesStatus.api === 'down'}
                        className="relative w-full bg-[#1e293b] border border-slate-700 rounded-2xl py-4 lg:py-5 pl-6 pr-16 focus:outline-none focus:border-blue-500 transition-all text-sm disabled:opacity-50"
                    />
                    <button 
                        disabled={!conversacionId || isLoading || servicesStatus.api === 'down'}
                        className="absolute right-3 top-1/2 -translate-y-1/2 bg-blue-600 p-2 lg:p-3 rounded-xl hover:bg-blue-500 transition-colors shadow-lg shadow-blue-600/20 disabled:bg-slate-700"
                    >
                        <Send size={20} />
                    </button>
                </form>
              </div>
            </>
          ) : activeTab === 'subir' ? <SubirManuales /> : <Biblioteca />}
        </div>
      </main>
    </div>
  );
};

// --- SUB-COMPONENTES ---

const NavItem = ({ icon, label, active, onClick }) => (
  <div onClick={onClick} className={`flex items-center gap-3 p-3 rounded-xl cursor-pointer transition-all ${active ? 'bg-blue-600/10 text-blue-400 border border-blue-600/20 shadow-lg' : 'text-slate-500 hover:bg-slate-800 hover:text-slate-300'}`}>
    {icon}<span className="text-sm font-semibold tracking-tight">{label}</span>
  </div>
);

const HealthPanel = memo(({ servicesStatus, onRefresh }) => (
  <div className="bg-[#0f172a]/80 border border-slate-700/50 rounded-2xl p-4 shadow-xl backdrop-blur-sm">
    <div className="flex justify-between items-center mb-3 pb-2 border-b border-slate-800">
      <div className="flex items-center gap-2">
        <Activity size={12} className="text-blue-400" />
        <span className="text-[10px] uppercase font-black text-blue-400 tracking-widest">Telemetry</span>
      </div>
      <button onClick={onRefresh} className="text-slate-600 hover:text-blue-400"><RefreshCcw size={10} /></button>
    </div>
    <div className="space-y-2">
      {Object.entries(servicesStatus).map(([k, v]) => (
        <div key={k} className="flex justify-between items-center text-[10px]">
          <span className="capitalize text-slate-500 font-medium">{k}</span>
          <div className="flex items-center gap-2">
            <span className={`font-black ${v === 'up' ? 'text-green-500' : v === 'loading' ? 'text-slate-600' : 'text-red-500'}`}>{v.toUpperCase()}</span>
            <div className={`w-1.5 h-1.5 rounded-full ${v === 'up' ? 'bg-green-500 shadow-[0_0_5px_rgba(34,197,94,0.5)]' : 'bg-red-500'}`} />
          </div>
        </div>
      ))}
    </div>
  </div>
));

const ChatMessage = memo(({ message }) => {
  const isBot = message.role === 'bot';
  return (
    <div className={`flex gap-3 lg:gap-4 ${isBot ? 'animate-in fade-in slide-in-from-left-2' : 'flex-row-reverse animate-in fade-in slide-in-from-right-2'}`}>
      <div className={`w-8 h-8 lg:w-10 lg:h-10 rounded-xl flex items-center justify-center shrink-0 border ${isBot ? 'bg-slate-800 border-blue-500/20' : 'bg-blue-600 shadow-lg shadow-blue-600/20'}`}>
        {isBot ? <img src={InuzaruAvatar} className="w-6 h-6 lg:w-8 lg:h-8 rounded-full" /> : <User size={18} className="text-white" />}
      </div>
      <div className={`max-w-[85%] lg:max-w-[80%] p-3 lg:p-4 rounded-2xl shadow-sm ${isBot ? 'bg-[#1e293b] border border-slate-700 text-slate-300' : 'bg-blue-600 text-white'}`}>
        <p className="text-[13px] lg:text-[14px] whitespace-pre-line leading-relaxed selection:bg-blue-500/30">{message.content}</p>
        
        {isBot && message.fuentes && message.fuentes.length > 0 && (
          <div className="mt-4 pt-3 border-t border-slate-700/50 flex flex-wrap gap-2">
            {message.fuentes.map((f, idx) => (
              <a 
                key={idx}
                href={`${API_BASE_URL}/Manuales/download/${f.nombreArchivo}#page=${f.numeroPagina}`}
                target="_blank"
                rel="noreferrer"
                className="flex items-center gap-1.5 text-[10px] bg-slate-900/50 hover:bg-slate-900 border border-slate-700 px-2 py-1 rounded-lg transition-colors text-blue-400"
              >
                <FileText size={12} />
                <span>Pág. {f.numeroPagina} - {f.titulo}</span>
              </a>
            ))}
          </div>
        )}
      </div>
    </div>
  );
});

export default InuzaruChat;