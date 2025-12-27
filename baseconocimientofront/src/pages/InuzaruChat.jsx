import React, { useState, useEffect, useRef, memo, startTransition } from 'react';
import { Send, User, Search, Upload, Book, RefreshCcw } from 'lucide-react';
import InuzaruAvatar from '../assets/inuzaru_avatar.png';
import SubirManuales from './SubirManuales';
import Biblioteca from './Biblioteca';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const InuzaruChat = () => {
  const [activeTab, setActiveTab] = useState('consultar');
  const [servicesStatus, setServicesStatus] = useState({
    api: 'loading',
    qdrant: 'loading',
    database: 'loading',
    ollama: 'loading',
    redis: 'loading'
  });

  const [messages, setMessages] = useState([
    { role: 'bot', content: '¡Hola! Soy Inuzaru. He olfateado tus manuales y estoy listo para responder. ¿Qué deseas saber?' }
  ]);

  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const scrollRef = useRef(null);

  useEffect(() => {
    scrollRef.current?.scrollIntoView({ behavior: 'auto' });
  }, [messages.length]);

  const checkHealth = async () => {
    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL.replace('/api', '')}/health`);
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
    const t = setTimeout(checkHealth, 1500);
    const i = setInterval(checkHealth, 30000);
    return () => { clearTimeout(t); clearInterval(i); };
  }, []);

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim() || isLoading) return;

    const question = input;
    startTransition(() => {
      setMessages(prev => [...prev, { role: 'user', content: question }]);
    });

    setInput('');
    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/Consultas/preguntar`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ pregunta: question, categoria: 'Microinformatica', topK: 3 })
      });

      const data = await response.json();
      if (!data.exitoso) throw new Error();
      setMessages(prev => [...prev, { role: 'bot', content: data.respuesta }]);
    } catch {
      setMessages(prev => [...prev, { role: 'bot', content: 'Error de conexión con la base de conocimiento.' }]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex h-screen overflow-hidden bg-[#0f172a] text-slate-200">

      {/* SIDEBAR */}
      <aside className="w-72 bg-[#1e293b] border-r border-blue-500/20 p-6 hidden lg:flex flex-col min-h-0">
        <div className="flex items-center gap-3 mb-8 shrink-0">
          <img src={InuzaruAvatar} className="w-10 h-10 rounded-full border border-blue-400" />
          <div>
            <h1 className="font-black text-xl uppercase italic">Inuzaru</h1>
            <p className="text-[10px] text-blue-400 font-bold uppercase tracking-widest">RAG Engine v1.0</p>
          </div>
        </div>

        <nav className="space-y-0 mb-1 shrink-0">
          <NavItem icon={<Search size={18} />} label="Consultar IA" active={activeTab === 'consultar'} onClick={() => setActiveTab('consultar')} />
          <NavItem icon={<Upload size={18} />} label="Subir Manuales" active={activeTab === 'subir'} onClick={() => setActiveTab('subir')} />
          <NavItem icon={<Book size={18} />} label="Biblioteca" active={activeTab === 'biblioteca'} onClick={() => setActiveTab('biblioteca')} />
        </nav>

        {/* AVATAR RESPONSIVO A ALTURA */}
        <div className="flex-1 min-h-1 flex items-center justify-center pointer-events-none">
          <img
            src={InuzaruAvatar}
            className="w-72 h-72 object-cover brightness-125 contrast-125"
          />
        </div>

        <HealthPanel servicesStatus={servicesStatus} onRefresh={checkHealth} />
      </aside>

      {/* MAIN */}
      <main className="flex-1 flex flex-col min-h-0">
        <header className="h-16 shrink-0 border-b border-slate-800 flex items-center px-8">
          <span className="text-sm text-slate-400 uppercase tracking-widest italic">Terminal de Consulta</span>
        </header>

        {activeTab === 'consultar' && (
  <>
    <div className="flex-1 min-h-0 overflow-y-auto p-6">
      {messages.map((m, i) => <ChatMessage key={i} message={m} />)}
      {isLoading && <div className="opacity-40 animate-pulse">Inuzaru pensando...</div>}
      <div ref={scrollRef} />
    </div>

    <form onSubmit={handleSendMessage} className="shrink-0 p-6 border-t border-slate-800">
      <div className="relative max-w-4xl mx-auto">
        <input
          value={input}
          onChange={e => setInput(e.target.value)}
          placeholder="Escribe tu consulta técnica..."
          className="w-full bg-[#1e293b] border border-slate-700 rounded-2xl py-5 pl-6 pr-16"
        />
        <button className="absolute right-3 top-1/2 -translate-y-1/2 bg-blue-600 p-3 rounded-xl">
          <Send size={20} />
        </button>
      </div>
    </form>
  </>
)}

{activeTab === 'subir' && (
  <div className="flex-1 min-h-0 overflow-y-auto p-6">
    <SubirManuales />
  </div>
)}

{activeTab === 'biblioteca' && (
  <div className="flex-1 min-h-0 overflow-y-auto p-6">
    <Biblioteca />
  </div>
)}

      </main>
    </div>
  );
};

const NavItem = ({ icon, label, active, onClick }) => (
  <div onClick={onClick} className={`flex items-center gap-3 p-3 rounded-xl cursor-pointer ${active ? 'bg-blue-600/10 text-blue-400' : 'text-slate-500 hover:bg-slate-800'}`}>
    {icon}<span className="text-sm font-semibold">{label}</span>
  </div>
);

const HealthPanel = memo(({ servicesStatus, onRefresh }) => (
  <div className="mt-4 bg-slate-900/80 border border-slate-700/50 rounded-2xl p-4 shrink-0">
    <div className="flex justify-between mb-2">
      <span className="text-[10px] uppercase font-black text-blue-400">System Health</span>
      <button onClick={onRefresh}><RefreshCcw size={12} /></button>
    </div>
    {Object.entries(servicesStatus).map(([k, v]) => (
      <div key={k} className="flex justify-between text-[11px]">
        <span className="capitalize text-slate-400">{k}</span>
        <span className={v === 'up' ? 'text-green-500' : 'text-red-500'}>{v.toUpperCase()}</span>
      </div>
    ))}
  </div>
));

const ChatMessage = memo(({ message }) => {
  const isBot = message.role === 'bot';
  return (
    <div className={`flex gap-4 mb-6 ${isBot ? '' : 'flex-row-reverse'}`}>
      <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${isBot ? 'bg-slate-800' : 'bg-blue-600'}`}>
        {isBot ? <img src={InuzaruAvatar} className="w-8 h-8 rounded-full" /> : <User size={18} />}
      </div>
      <div className={`max-w-[80%] p-4 rounded-2xl ${isBot ? 'bg-[#1e293b]' : 'bg-blue-600 text-white'}`}>
        <p className="whitespace-pre-line">{message.content}</p>
      </div>
    </div>
  );
});

export default InuzaruChat;
