import React, { useState, useEffect, useRef, startTransition } from 'react';
import { Send, User, RefreshCcw, FileText, Sparkles } from 'lucide-react';
import InuzaruAvatar from '../assets/inuzaru_avatar.png';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const ChatPage = ({ user, conversacionId, setConversacionId, messages, setMessages }) => {
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const scrollRef = useRef(null);
  const initialized = useRef(false);

  const token = localStorage.getItem('inuzaru_token');

  // Inicializar sesión en Redis vinculada al Usuario Real
  useEffect(() => {
    if (conversacionId || initialized.current || !user) return;

    const initSession = async () => {
      try {
        initialized.current = true;
        const res = await fetch(`${API_BASE_URL}/Conversaciones/crear`, {
          method: 'POST',
          headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify({ usuarioId: user.id })
        });

        const data = await res.json();
        if (data.conversacionId) {
          setConversacionId(data.conversacionId);
        }
      } catch (err) {
        console.error("Error en sesión Redis:", err);
        initialized.current = false;
      }
    };

    initSession();
  }, [conversacionId, setConversacionId, user, token]);

  useEffect(() => {
    scrollRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isLoading]);

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim() || isLoading || !conversacionId) return;

    const userMessage = input;
    setInput('');
    setIsLoading(true);
    
    startTransition(() => {
      setMessages(prev => [...prev, { role: 'user', content: userMessage }]);
    });

    try {
      const response = await fetch(`${API_BASE_URL}/Consultas/consultar-con-conversacion`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}` 
        },
        body: JSON.stringify({ 
          pregunta: userMessage, 
          conversacionId: conversacionId,
          usuarioId: user.id, 
          topK: 5
        })
      });

      if (!response.ok) throw new Error("Error IA");

      const data = await response.json();
      
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: data.respuesta,
        fuentes: data.fuentes
      }]);
    } catch (error) {
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: 'Hubo un problema de conexión. Verificá Ollama y tu sesión.' 
      }]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-[#0f172a]">
      <header className="px-8 py-5 border-b border-slate-800 flex justify-between items-center bg-[#0f172a]/80 backdrop-blur-md z-10">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-blue-600/10 rounded-lg text-blue-400">
            <Sparkles size={20} />
          </div>
          <div>
            <h2 className="text-sm font-black uppercase tracking-widest text-white italic leading-none">Terminal IA</h2>
            <p className="text-[9px] text-slate-500 font-bold uppercase mt-1 tracking-tighter">
              Sesión: <span className="text-blue-500/70">{conversacionId?.substring(0, 8)}...</span>
            </p>
          </div>
        </div>
      </header>

      <div className="flex-1 overflow-y-auto p-6 space-y-8 scrollbar-thin scrollbar-thumb-slate-800">
        <div className="max-w-4xl mx-auto space-y-8">
          {messages.map((m, i) => <ChatMessage key={i} message={m} />)}
          {isLoading && (
            <div className="flex gap-4 items-center animate-pulse">
              <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center border border-blue-500/20">
                <RefreshCcw size={16} className="animate-spin text-blue-400" />
              </div>
              <p className="text-xs italic text-blue-400 font-bold uppercase tracking-widest">Inuzaru está olfateando manuales...</p>
            </div>
          )}
          <div ref={scrollRef} />
        </div>
      </div>

      <div className="p-6">
        <form onSubmit={handleSendMessage} className="max-w-4xl mx-auto relative group">
          <div className="absolute -inset-1 bg-gradient-to-r from-blue-600 to-cyan-600 rounded-[1.5rem] blur opacity-10 group-focus-within:opacity-30 transition-all duration-500"></div>
          <div className="relative flex items-center bg-[#1e293b] border border-slate-700 rounded-[1.5rem] p-2.5 focus-within:border-blue-500/50 transition-all shadow-2xl">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="¿Qué sistema necesitas revisar hoy?"
              className="flex-1 bg-transparent border-none text-white px-5 py-3 text-sm focus:outline-none placeholder:text-slate-600"
              disabled={isLoading || !conversacionId}
            />
            <button type="submit" disabled={isLoading || !input.trim() || !conversacionId} className="bg-blue-600 hover:bg-blue-500 text-white p-4 rounded-2xl shadow-lg shadow-blue-600/30 disabled:bg-slate-800 transition-all group-active:scale-95">
              <Send size={18} />
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const ChatMessage = ({ message }) => {
  const isBot = message.role === 'bot';
  return (
    <div className={`flex gap-4 ${isBot ? 'flex-row' : 'flex-row-reverse animate-in slide-in-from-right-4 duration-300'}`}>
      <div className={`w-10 h-10 rounded-xl shrink-0 flex items-center justify-center border ${isBot ? 'bg-slate-800 border-blue-500/20' : 'bg-blue-600 border-blue-400 shadow-xl shadow-blue-600/20'}`}>
        {isBot ? <img src={InuzaruAvatar} alt="Bot" className="w-8 h-8 rounded-full" /> : <User size={20} className="text-white" />}
      </div>
      <div className={`max-w-[85%] space-y-3`}>
        <div className={`p-5 rounded-[1.5rem] text-sm leading-relaxed shadow-sm ${isBot ? 'bg-[#1e293b] text-slate-300 border border-slate-800' : 'bg-blue-600 text-white font-medium'}`}>
          <p className="whitespace-pre-line">{message.content}</p>
        </div>
        {isBot && message.fuentes && message.fuentes.length > 0 && (
          <div className="flex flex-wrap gap-2 px-1">
            {message.fuentes.map((f, idx) => (
              <div key={idx} className="flex items-center gap-2 bg-slate-900/50 border border-slate-800/50 px-3 py-1.5 rounded-xl text-[10px] text-blue-400 font-black uppercase tracking-tighter hover:border-blue-500/30 transition-colors" title={f.textoFragmento}>
                <FileText size={12} /> Pág. {f.numeroPagina} • {f.titulo}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ChatPage;