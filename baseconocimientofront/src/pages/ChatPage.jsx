import React, { useState, useEffect, useRef, startTransition } from 'react';
import { Send, User, RefreshCcw, FileText, Sparkles, WifiOff } from 'lucide-react';

import InuzaruGif from '../assets/inuzaru_gif.gif';
import InuzaruAvatar from '../assets/inuzaru_avatar.png';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const ChatPage = ({ user, conversacionId, setConversacionId, messages, setMessages }) => {
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [errorStatus, setErrorStatus] = useState(false);
  const scrollRef = useRef(null);
  const token = localStorage.getItem('inuzaru_token');

  // Redis
  useEffect(() => {
    if (conversacionId || !user) return;
    const initSession = async () => {
      try {
        const res = await fetch(`${API_BASE_URL}/Conversaciones/crear`, {
          method: 'POST',
          headers: { 
            'Content-Type': 'application/json', 
            'Authorization': `Bearer ${token}` 
          },
          body: JSON.stringify({ usuarioId: user.id })
        });
        const data = await res.json();
        if (data.conversacionId) setConversacionId(data.conversacionId);
      } catch (err) {
        console.error("Error de enlace Redis:", err);
        setErrorStatus(true);
      }
    };
    initSession();
  }, [conversacionId, setConversacionId, user, token]);

  // Scroll automático
  useEffect(() => { 
    scrollRef.current?.scrollIntoView({ behavior: 'smooth' }); 
  }, [messages, isLoading]);

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim() || isLoading || !conversacionId) return;

    const userMessage = input;
    setInput('');
    setIsLoading(true);
    setErrorStatus(false);
    
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
          conversacionId, 
          usuarioId: user.id, 
          topK: 5 
        })
      });

      if (!response.ok) throw new Error();
      const data = await response.json();
      
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: data.respuesta,
        fuentes: data.fuentes
      }]);
    } catch {
      setErrorStatus(true);
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: 'Error de red. Inuzaru no pudo olfatear la base de datos. Verificá Ollama.' 
      }]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-[#0f172a] relative overflow-hidden">
      
      {/* PANTALLA INICIAL: Muestra el GIF si no hay conversación activa */}
      {messages.length <= 1 && !isLoading && (
        <div className="absolute inset-0 flex flex-col items-center justify-center z-0 px-6">
          <div className="relative group">
            {/* Aura de resplandor neón */}
            <div className={`absolute -inset-16 rounded-full blur-[100px] opacity-10 transition-all duration-1000 ${errorStatus ? 'bg-red-600' : 'animate-pulse'}`} />
            
            <img 
              src={InuzaruGif} 
              alt="Inuzaru Mascot"
              className={`w-64 h-64 md:w-64 md:h-64 object-contain relative z-10 transition-all duration-500
                ${errorStatus ? 'grayscale brightness-50 contrast-125' : 'filter drop-shadow-[0_0_10px_rgba(59,130,246,0.3)]'}
              `}
              style={{ mixBlendMode: 'screen' }} 
            />
          </div>

          <div className="mt-8 text-center animate-in fade-in zoom-in duration-700">
            <h2 className="text-3xl font-black uppercase italic text-white tracking-tighter">Inuzaru Engine</h2>
            <div className="flex items-center justify-center gap-3 mt-3">
              <div className={`w-2 h-2 rounded-full ${errorStatus ? 'bg-red-500 animate-ping' : 'bg-blue-400 animate-pulse'}`} />
              <p className="text-[10px] text-blue-400 font-black uppercase tracking-[0.4em]">
                {errorStatus ? 'SISTEMA FUERA DE LÍNEA' : 'SOPORTE TÉCNICO INTELIGENTE'}
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Header Desktop */}
      <header className="hidden lg:flex px-8 py-5 border-b border-slate-800 justify-between items-center bg-[#0f172a]/80 backdrop-blur-md z-10">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-blue-600/10 rounded-lg text-blue-400">
            <Sparkles size={20} />
          </div>
          <div>
            <h2 className="text-xs font-black uppercase tracking-widest text-white italic">Terminal de Consulta</h2>
            <p className="text-[9px] text-slate-500 font-bold uppercase">Sesión activa: {conversacionId?.substring(0,8)}</p>
          </div>
        </div>
      </header>

      {/* Mensajes del Chat */}
      <div className="flex-1 overflow-y-auto p-4 md:p-8 space-y-6 z-10">
        <div className="max-w-4xl mx-auto space-y-8">
          {messages.map((m, i) => <ChatMessage key={i} message={m} />)}
          
          {isLoading && (
            <div className="flex gap-4 items-center animate-in fade-in duration-300">
              <div className="relative">
                <img src={InuzaruGif} className="w-10 h-10 rounded-xl border border-blue-500/20 shadow-lg shadow-blue-500/10" style={{ mixBlendMode: 'screen' }} />
                <div className="absolute -bottom-1 -right-1 w-3 h-3 bg-blue-500 rounded-full animate-ping" />
              </div>
              <p className="text-[10px] text-blue-400 font-black uppercase tracking-widest animate-pulse">Inuzaru olfateando manuales...</p>
            </div>
          )}

          {errorStatus && !isLoading && (
            <div className="flex gap-3 items-center p-4 bg-red-500/10 border border-red-500/20 rounded-2xl text-red-400">
              <WifiOff size={18} />
              <span className="text-[10px] font-black uppercase tracking-widest leading-none">Vínculo con servidor interrumpido</span>
            </div>
          )}
          <div ref={scrollRef} />
        </div>
      </div>

      {/* Formulario de Input */}
      <div className="p-4 md:p-8 bg-gradient-to-t from-[#0f172a] via-[#0f172a] to-transparent z-20">
        <form onSubmit={handleSendMessage} className="max-w-4xl mx-auto relative group">
          <div className="absolute -inset-1 bg-gradient-to-r from-blue-600 to-cyan-600 rounded-2xl blur opacity-10 group-focus-within:opacity-30 transition-all duration-500" />
          <div className="relative flex items-center bg-[#1e293b] border border-slate-700 rounded-2xl p-2 focus-within:border-blue-500/50 shadow-2xl">
            <input
              type="text" 
              value={input} 
              onChange={(e) => setInput(e.target.value)}
              placeholder="Describí la falla o el sistema..."
              className="flex-1 bg-transparent border-none text-white px-4 py-3 text-sm focus:outline-none placeholder:text-slate-600"
              disabled={isLoading}
            />
            <button 
              type="submit" 
              disabled={isLoading || !input.trim()} 
              className="bg-blue-600 hover:bg-blue-500 text-white p-4 rounded-xl shadow-lg shadow-blue-600/20 active:scale-95 transition-all disabled:bg-slate-800"
            >
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
    <div className={`flex gap-4 ${isBot ? 'flex-row' : 'flex-row-reverse animate-in slide-in-from-bottom-2'}`}>
      <div className={`w-10 h-10 rounded-xl shrink-0 flex items-center justify-center border ${isBot ? 'bg-slate-800 border-blue-500/20 shadow-inner' : 'bg-blue-600 border-blue-400 shadow-lg shadow-blue-600/20'}`}>
        {isBot ? (
          <img src={InuzaruAvatar} alt="Bot" className="w-8 h-8 rounded-full filter drop-shadow-[0_0_5px_rgba(59,130,246,0.5)]" />
        ) : (
          <User size={20} className="text-white" />
        )}
      </div>
      <div className="max-w-[85%] space-y-3">
        <div className={`p-5 rounded-2xl text-sm leading-relaxed ${isBot ? 'bg-[#1e293b] text-slate-300 border border-slate-800' : 'bg-blue-600 text-white font-medium shadow-md'}`}>
          <p className="whitespace-pre-line">{message.content}</p>
        </div>
        {isBot && message.fuentes && message.fuentes.length > 0 && (
          <div className="flex flex-wrap gap-2">
            {message.fuentes.map((f, idx) => (
              <div key={idx} className="flex items-center gap-2 bg-slate-900/50 border border-slate-800 px-3 py-1.5 rounded-xl text-[9px] text-blue-400 font-black uppercase hover:border-blue-500/30 transition-all cursor-default">
                <FileText size={10} /> Pág. {f.numeroPagina} • {f.titulo?.substring(0, 20)}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ChatPage;