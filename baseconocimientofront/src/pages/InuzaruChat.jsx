import React, { useState, useEffect, useRef } from 'react';
import { Send, User, Bot, Sparkles, Search, Upload, Book, FileText } from 'lucide-react';
import InuzaruAvatar from '../assets/inuzaru_avatar.png';
import SubirManuales from './SubirManuales'; // Asegúrate de que estos archivos existan
import Biblioteca from './Biblioteca';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const InuzaruChat = () => {
  // 1. Estado para controlar qué vista mostrar
  const [activeTab, setActiveTab] = useState('consultar');
  const [messages, setMessages] = useState([
    { role: 'bot', content: '¡Hola! Soy Inuzaru. He olfateado tus manuales y estoy listo para responder. ¿Qué deseas saber?' }
  ]);
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const scrollRef = useRef(null);

  useEffect(() => {
    scrollRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim() || isLoading) return;

    const userMessage = { role: 'user', content: input };
    setMessages(prev => [...prev, userMessage]);
    setInput('');
    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/Consultas/preguntar`, {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ 
          pregunta: input,
          categoria: "Microinformatica",
          topK: 3
        })
      });
      
      const data = await response.json();
      
      if (data.exitoso) {
        setMessages(prev => [...prev, { 
          role: 'bot', 
          content: data.respuesta, 
          fuentes: data.fuentes 
        }]);
      } else {
        throw new Error(data.mensaje);
      }
    } catch (error) {
      setMessages(prev => [...prev, { 
        role: 'bot', 
        content: '¡Grrr! No pude conectar con la base de conocimiento. Verifica el backend.' 
      }]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex h-screen bg-[#0f172a] text-slate-200 overflow-hidden">
      {/* Sidebar */}
      <aside className="w-72 bg-[#1e293b] border-r border-blue-500/20 p-6 hidden lg:flex flex-col">
        <div className="flex items-center gap-3 mb-10">
          <div className="relative">
            <img src={InuzaruAvatar} alt="Inuzaru" className="w-10 h-10 rounded-full border border-blue-400 object-cover" />
            <div className="absolute bottom-0 right-0 w-2.5 h-2.5 bg-green-500 rounded-full border-2 border-[#1e293b]"></div>
          </div>
          <div>
            <h1 className="font-black text-xl tracking-tighter text-white">INUZARU</h1>
            <p className="text-[10px] text-blue-400 font-bold uppercase tracking-widest">RAG Engine v1.0</p>
          </div>
        </div>

        {/* Navegación con onClick añadido */}
        <nav className="space-y-1 mb-8">
          <NavItem 
            icon={<Search size={18}/>} 
            label="Consultar IA" 
            active={activeTab === 'consultar'} 
            onClick={() => setActiveTab('consultar')}
          />
          <NavItem 
            icon={<Upload size={18}/>} 
            label="Subir Manuales" 
            active={activeTab === 'subir'} 
            onClick={() => setActiveTab('subir')}
          />
          <NavItem 
            icon={<Book size={18}/>} 
            label="Biblioteca" 
            active={activeTab === 'biblioteca'} 
            onClick={() => setActiveTab('biblioteca')}
          />
        </nav>

        {/* Logo Central Grande (Sin cambiar tamaño) */}
        <div className="flex-1 flex flex-col items-center justify-center relative opacity-80 hover:opacity-100 transition-opacity">
          <div className="absolute w-40 h-40 bg-blue-500/10 rounded-full blur-3xl"></div>
          <img 
            src={InuzaruAvatar} 
            alt="Inuzaru Logo" 
            className="w-96 h-96 object-cover brightness-125 contrast-125"
            style={{
              maskImage: 'linear-gradient(to bottom, black 60%, transparent 100%)',
              WebkitMaskImage: 'linear-gradient(to bottom, black 60%, transparent 100%)'
            }}
          />
        </div>

        {/* Estado del Sistema */}
        <div className="mt-auto p-4 bg-slate-900/50 rounded-xl border border-slate-700">
          <p className="text-[10px] text-slate-500 mb-2 uppercase font-bold">Estado del Sistema</p>
          <div className="flex items-center gap-2 text-xs text-green-400">
            <div className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></div>
            Base de conocimiento lista
          </div>
        </div>
      </aside>

      {/* Área Principal Dinámica */}
      <main className="flex-1 flex flex-col bg-gradient-to-b from-[#0f172a] to-[#1e293b]">
        {activeTab === 'consultar' ? (
          <>
            <header className="h-16 border-b border-slate-800 flex items-center px-8 justify-between">
              <span className="text-sm font-medium text-slate-400 uppercase tracking-widest">Nueva Sesión de Consulta</span>
              <div className="flex gap-4">
                 
              </div>
            </header>

            <div className="flex-1 overflow-y-auto p-6 space-y-6">
              {messages.map((msg, i) => (
                <ChatMessage key={i} message={msg} />
              ))}
              {isLoading && (
                <div className="flex gap-3 animate-pulse">
                  <div className="w-8 h-8 rounded-full bg-slate-800 border border-blue-500/30 shrink-0" />
                  <div className="h-10 w-48 bg-slate-800 rounded-2xl" />
                </div>
              )}
              <div ref={scrollRef} />
            </div>

            <div className="p-6">
              <form onSubmit={handleSendMessage} className="max-w-4xl mx-auto relative">
                <input
                  type="text"
                  value={input}
                  onChange={(e) => setInput(e.target.value)}
                  placeholder="Pregunta sobre manuales, versiones o procesos..."
                  className="w-full bg-[#1e293b]/50 border border-slate-700 rounded-2xl py-5 pl-6 pr-16 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 shadow-2xl backdrop-blur-sm"
                />
                <button className="absolute right-3 top-1/2 -translate-y-1/2 bg-blue-600 p-3 rounded-xl hover:bg-blue-500 transition-all text-white">
                  <Send size={20} />
                </button>
              </form>
            </div>
          </>
        ) : activeTab === 'subir' ? (
          <SubirManuales />
        ) : (
          <Biblioteca />
        )}
      </main>
    </div>
  );
};

// Sub-componentes corregidos
const NavItem = ({ icon, label, active, onClick }) => (
  <div 
    onClick={onClick}
    className={`flex items-center gap-3 p-3 rounded-xl cursor-pointer transition-all ${active ? 'bg-blue-600/10 text-blue-400 border border-blue-600/20' : 'text-slate-500 hover:bg-slate-800'}`}
  >
    {icon} <span className="text-sm font-semibold">{label}</span>
  </div>
);

const ChatMessage = ({ message }) => {
  const isBot = message.role === 'bot';
  return (
    <div className={`flex gap-4 ${message.role === 'user' ? 'flex-row-reverse' : ''}`}>
      <div className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 border ${isBot ? 'bg-slate-800 border-blue-500/30' : 'bg-blue-600 border-transparent'}`}>
        {isBot ? <img src={InuzaruAvatar} className="w-8 h-8 rounded-full" alt="bot" /> : <User size={20} />}
      </div>
      <div className={`max-w-[80%] p-4 rounded-2xl ${isBot ? 'bg-[#1e293b] border border-slate-700' : 'bg-blue-600 text-white'}`}>
        <p className="text-[15px] leading-relaxed whitespace-pre-line">{message.content}</p>
        
        {message.fuentes && message.fuentes.length > 0 && (
          <div className="mt-4 pt-3 border-t border-slate-700/50">
            <p className="text-[10px] uppercase font-black text-blue-400 mb-2 tracking-widest flex items-center gap-1">
              <FileText size={12}/> Fuentes Verificadas
            </p>
            <div className="grid grid-cols-1 gap-2">
              {message.fuentes.map((f, i) => (
                <div key={i} className="text-[11px] bg-slate-900/50 p-2 rounded border border-slate-700 flex justify-between">
                  <span className="truncate italic">"{f.textoFragmento?.substring(0, 50)}..."</span>
                  <span className="text-blue-500 font-bold ml-2 shrink-0">{f.nombreArchivo} (P. {f.pagina})</span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default InuzaruChat;