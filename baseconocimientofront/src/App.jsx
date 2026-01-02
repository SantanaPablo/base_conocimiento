import React, { useState, useEffect, memo } from 'react';
import { 
  MessageSquare, UploadCloud, Library, FolderTree, 
  Settings, RefreshCcw, Activity, Menu, X, ChevronRight, LogOut 
} from 'lucide-react';

// Páginas e Integraciones
import ChatPage from './pages/ChatPage';
import UploadPage from './pages/UploadPage';
import LibraryPage from './pages/LibraryPage';
import CategoriesPage from './pages/CategoriesPage';
import AdminCategoriasPage from './pages/AdminCategoriasPage';
import Login from './pages/Login'; // Importamos el Login
import InuzaruAvatar from './assets/inuzaru_avatar.png';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const App = () => {
  // --- LÓGICA DE AUTENTICACIÓN NUEVA ---
  const [user, setUser] = useState(() => {
    const savedUser = localStorage.getItem('inuzaru_user');
    const token = localStorage.getItem('inuzaru_token');
    return (savedUser && token) ? JSON.parse(savedUser) : null;
  });

  const [currentSection, setCurrentSection] = useState('chat');
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  
  // chat persistente
  const [conversacionId, setConversacionId] = useState(null);
  const [messages, setMessages] = useState([
    { role: 'bot', content: 'Mi olfato digital está al 100%. ¿Qué duda técnica tenés hoy?' }
  ]);

  const [servicesStatus, setServicesStatus] = useState({
    api: 'loading', qdrant: 'loading', database: 'loading', ollama: 'loading', redis: 'loading'
  });

  // --- TU LÓGICA DE SALUD (LA QUE ANDA BIEN) ---
  const checkHealth = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/Infraestructura/estado`);
      const data = await response.json();
      const getStatus = (name) => {
        const s = data.servicios?.find(serv => serv.nombre === name);
        return s?.estado === 'Healthy' ? 'up' : 'down';
      };
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
    if (user) { // Solo checkear salud si hay usuario logueado
      checkHealth();
      const i = setInterval(checkHealth, 30000);
      return () => clearInterval(i);
    }
  }, [user]);

  // --- ACCIÓN DE LOGOUT ---
  const handleLogout = () => {
    localStorage.removeItem('inuzaru_token');
    localStorage.removeItem('inuzaru_user');
    setUser(null);
    setConversacionId(null);
  };

  const navigateTo = (section) => {
    setCurrentSection(section);
    setIsMobileMenuOpen(false);
  };

  // --- RENDER CONDICIONAL DE LOGIN ---
  if (!user) {
    return <Login onLoginSuccess={(userData) => setUser(userData)} />;
  }

  const renderContent = () => {
    switch (currentSection) {
      case 'chat': return (
        <ChatPage 
          user={user}
          conversacionId={conversacionId} 
          setConversacionId={setConversacionId}
          messages={messages}
          setMessages={setMessages}
        />
      );
      case 'subir': return <UploadPage user={user} />;
      case 'biblioteca': return <LibraryPage user={user} />;
      case 'categorias': return <CategoriesPage user={user} />;
      case 'admin-categorias': return <AdminCategoriasPage user={user} />;
      default: return <ChatPage user={user} />;
    }
  };

  return (
    <div className="flex h-screen bg-[#0f172a] text-slate-200 overflow-hidden font-sans relative">
      
      {isMobileMenuOpen && (
        <div className="fixed inset-0 bg-black/60 z-40 lg:hidden backdrop-blur-sm transition-opacity" onClick={() => setIsMobileMenuOpen(false)} />
      )}

      <aside className={`fixed inset-y-0 left-0 z-50 w-72 bg-[#1e293b] border-r border-blue-500/10 p-6 flex flex-col shadow-2xl transition-transform duration-300 ease-in-out ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'} lg:translate-x-0 lg:static lg:inset-auto`}>
        <div className="mb-8 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <img src={InuzaruAvatar} className="w-10 h-10 rounded-full border border-blue-400" alt="Logo" />
            <div>
              <h1 className="text-xl font-black uppercase italic leading-none tracking-tighter text-white">Inuzaru</h1>
              <p className="text-[10px] text-blue-400 font-bold uppercase tracking-widest mt-1">RAG ENGINE 2.0</p>
            </div>
          </div>
          <button className="lg:hidden text-slate-400 hover:text-white" onClick={() => setIsMobileMenuOpen(false)}><X size={24} /></button>
        </div>

        <nav className="space-y-1">
          <MenuButton icon={<MessageSquare size={18} />} label="Chat Inteligente" active={currentSection === 'chat'} onClick={() => navigateTo('chat')} />
          <MenuButton icon={<FolderTree size={18} />} label="Explorador Ramas" active={currentSection === 'categorias'} onClick={() => navigateTo('categorias')} />
          <MenuButton icon={<Library size={18} />} label="Biblioteca Global" active={currentSection === 'biblioteca'} onClick={() => navigateTo('biblioteca')} />
          <MenuButton icon={<UploadCloud size={18} />} label="Cargar Manuales" active={currentSection === 'subir'} onClick={() => navigateTo('subir')} />
          
          {/* Solo mostrar si es Admin (Rol 1 o texto Admin) */}
          {(user.rol === 1 || user.rol === "Admin" || user.rol === "Administrador") && (
            <MenuButton icon={<Settings size={18} />} label="Admin. Categorías" active={currentSection === 'admin-categorias'} onClick={() => navigateTo('admin-categorias')} />
          )}
        </nav>

        {/* LOGO DECORATIVO QUE TE GUSTA */}
        <div className="flex-1 flex items-center justify-center pointer-events-none my-4 relative overflow-hidden opacity-150">
          <img src={InuzaruAvatar} className="w-full h-full scale-150 object-contain" style={{ maskImage: 'radial-gradient(circle, black 30%, transparent 80%)', WebkitMaskImage: 'radial-gradient(circle, black 30%, transparent 80%)' }} />
        </div>

        {/* PANEL DE USUARIO Y LOGOUT */}
        <div className="mt-4 pt-4 border-t border-slate-800">
          <div className="flex items-center gap-3 mb-4 px-2">
            <div className="w-8 h-8 rounded-lg bg-blue-600 flex items-center justify-center font-black text-xs text-white">
              {user.nombreCompleto?.charAt(0).toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-bold text-white truncate">{user.nombreCompleto}</p>
              <p className="text-[9px] text-slate-500 uppercase font-black">{user.rol}</p>
            </div>
            <button onClick={handleLogout} className="text-slate-500 hover:text-red-400 transition-colors">
              <LogOut size={16} />
            </button>
          </div>
        </div>

        <HealthPanel servicesStatus={servicesStatus} onRefresh={checkHealth} />
      </aside>

      <main className="flex-1 flex flex-col min-w-0 overflow-hidden" style={{ zoom: "125%" }}>
        <header className="h-16 lg:hidden bg-[#0f172a]/80 backdrop-blur-md border-b border-slate-800 flex items-center px-6 justify-between shrink-0">
          <button className="text-blue-400" onClick={() => setIsMobileMenuOpen(true)}><Menu size={28} /></button>
          <div className="flex items-center gap-2">
            <div className={`w-2 h-2 rounded-full animate-pulse ${servicesStatus.api === 'up' ? 'bg-blue-500' : 'bg-red-500'}`} />
            <span className="text-[10px] font-black uppercase italic tracking-widest text-slate-400">Terminal Móvil</span>
          </div>
          <img src={InuzaruAvatar} className="w-8 h-8 rounded-full border border-blue-500/30" alt="Avatar" />
        </header>

        <div className="flex-1 overflow-hidden relative">
          {renderContent()}
        </div>
      </main>
    </div>
  );
};

// --- SUBCOMPONENTES ---

const MenuButton = ({ icon, label, active, onClick }) => (
  <button onClick={onClick} className={`w-full flex items-center justify-between p-3.5 rounded-2xl transition-all ${active ? 'bg-blue-600 text-white shadow-lg shadow-blue-600/20 font-bold' : 'text-slate-400 hover:bg-slate-800'}`}>
    <div className="flex items-center gap-3">
      <span className={active ? 'text-white' : 'text-slate-500'}>{icon}</span>
      <span className="text-sm tracking-tight">{label}</span>
    </div>
    {active && <ChevronRight size={14} className="opacity-50" />}
  </button>
);

const HealthPanel = memo(({ servicesStatus, onRefresh }) => (
  <div className="bg-[#0f172a]/80 border border-slate-700/50 rounded-2xl p-4 shadow-xl backdrop-blur-sm mt-4">
    <div className="flex justify-between items-center mb-3 pb-2 border-b border-slate-800">
      <div className="flex items-center gap-2">
        <Activity size={12} className="text-blue-400" />
        <span className="text-[10px] uppercase font-black text-blue-400 tracking-widest">Servicios</span>
      </div>
      <button onClick={onRefresh} className="text-slate-600 hover:text-blue-400"><RefreshCcw size={10} /></button>
    </div>
    <div className="space-y-1.5">
      {Object.entries(servicesStatus).map(([k, v]) => (
        <div key={k} className="flex justify-between items-center text-[10px]">
          <span className="capitalize text-slate-500 font-medium">{k}</span>
          <div className="flex items-center gap-2 font-black">
            <span className={v === 'up' ? 'text-green-500' : 'text-red-500'}>{v.toUpperCase()}</span>
            <div className={`w-1 h-1 rounded-full ${v === 'up' ? 'bg-green-500' : 'bg-red-500'}`} />
          </div>
        </div>
      ))}
    </div>
  </div>
));

export default App;