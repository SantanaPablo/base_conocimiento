import React, { useState, useEffect, useRef, startTransition } from 'react';
import { Send, User, RefreshCcw, FileText, Sparkles, AlertCircle } from 'lucide-react';
import InuzaruAvatar from '../assets/inuzaru_avatar.png';

const API_BASE_URL = import.meta.env.VITE_API_URL;

// props
const ChatPage = ({ conversacionId, setConversacionId, messages, setMessages }) => {
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const scrollRef = useRef(null);

  //Inicializar sesión en Redis
  useEffect(() => {
    if (conversacionId || initialized.current) return;

    const initSession = async () => {
      try {
        initialized.current = true;
        const res = await fetch(`${API_BASE_URL}/Conversaciones/crear`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ usuarioId: "00000000-0000-0000-0000-000000000000" })
        });
        const data = await res.json();
        if (data.conversacionId) {
          setConversacionId(data.conversacionId);
        }
      } catch (err) {
        console.error("Error al iniciar sesión en Redis:", err);
        initialized.current = false;
      }
    };
    initSession();
  }, [conversacionId, setConversacionId]);

  // Auto-scroll
  useEffect(() => {
    scrollRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isLoading]);

  //Consulta al RAG
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
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          pregunta: userMessage, 
          conversacionId: conversacionId,
          usuarioId: "00000000-0000-0000-0000-000000000000", 
          topK: 5
        })
      });

      if (!response.ok) throw new Error("Error en la respuesta de la IA");

      const data = await response.json();
      
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: data.respuesta,
        fuentes: data.fuentes
      }]);
    } catch (error) {
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: 'Perdí el rastro del manual. Revisá si Ollama y Qdrant están corriendo correctamente.' 
      }]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-[#0f172a]">
      {/* Header*/}
      <header className="px-8 py-4 border-b border-slate-800 flex justify-between items-center bg-[#0f172a]/80 backdrop-blur-md z-10">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-blue-600/10 rounded-lg text-blue-400">
            <Sparkles size={20} />
          </div>
          <div>
            <h2 className="text-sm font-black uppercase tracking-widest text-white italic">Terminal de Consulta IA</h2>
            <p className="text-[10px] text-slate-500 font-bold uppercase tracking-tighter">Motor RAG Activo • Ollama Conectado</p>
          </div>
        </div>
      </header>

      {/* MENSAJES */}
      <div className="flex-1 overflow-y-auto p-6 space-y-8 scrollbar-thin scrollbar-thumb-slate-800">
        <div className="max-w-4xl mx-auto space-y-8">
          {messages.map((m, i) => (
            <ChatMessage key={i} message={m} />
          ))}
          {isLoading && (
            <div className="flex gap-4 items-center opacity-60">
              <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center border border-blue-500/20">
                <RefreshCcw size={16} className="animate-spin text-blue-400" />
              </div>
              <p className="text-xs italic text-blue-400/80 font-medium">Olfateando manuales...</p>
            </div>
          )}
          <div ref={scrollRef} />
        </div>
      </div>

      {/* INPUT */}
      <div className="p-6 bg-gradient-to-t from-[#0f172a] via-[#0f172a] to-transparent">
        <form onSubmit={handleSendMessage} className="max-w-4xl mx-auto relative group">
          <div className="absolute -inset-1 bg-gradient-to-r from-blue-600 to-cyan-600 rounded-2xl blur opacity-10 group-focus-within:opacity-25 transition-all"></div>
          <div className="relative flex items-center bg-[#1e293b] border border-slate-700 rounded-2xl p-2 focus-within:border-blue-500 transition-colors">
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Hacé una pregunta sobre los manuales técnicos..."
              className="flex-1 bg-transparent border-none text-white px-4 py-3 text-sm focus:outline-none placeholder:text-slate-500"
              disabled={isLoading || !conversacionId}
            />
            <button 
              type="submit"
              disabled={isLoading || !input.trim() || !conversacionId}
              className="bg-blue-600 hover:bg-blue-500 text-white p-3 rounded-xl shadow-lg shadow-blue-600/20 disabled:bg-slate-700 disabled:shadow-none transition-all"
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
    <div className={`flex gap-4 ${isBot ? 'flex-row' : 'flex-row-reverse'}`}>
      <div className={`w-10 h-10 rounded-xl shrink-0 flex items-center justify-center border ${
        isBot ? 'bg-slate-800 border-blue-500/20' : 'bg-blue-600 border-blue-400 shadow-lg shadow-blue-600/20'
      }`}>
        {isBot ? <img src={InuzaruAvatar} alt="Bot" className="w-8 h-8 rounded-full" /> : <User size={20} className="text-white" />}
      </div>
      <div className="max-w-[85%] space-y-4">
        <div className={`p-4 rounded-2xl text-sm leading-relaxed ${
          isBot ? 'bg-[#1e293b] text-slate-300 border border-slate-700' : 'bg-blue-600 text-white font-medium'
        }`}>
          <p className="whitespace-pre-line">{message.content}</p>
        </div>
        {isBot && message.fuentes && message.fuentes.length > 0 && (
          <div className="flex flex-wrap gap-2 px-1">
            {message.fuentes.map((f, idx) => (
              <div key={idx} className="flex items-center gap-2 bg-slate-900/50 border border-slate-800 px-3 py-1.5 rounded-lg text-blue-400" title={f.textoFragmento}>
                <FileText size={12} />
                <span className="text-[10px] font-bold uppercase tracking-tight">
                  Pág. {f.numeroPagina} • {f.titulo}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ChatPage;