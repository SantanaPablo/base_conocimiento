import React, { useState } from 'react';
import { Lock, Mail, Loader2, ShieldAlert, Sparkles } from 'lucide-react';
import InuzaruAvatar from '../assets/inuzaru_avatar.png';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const Login = ({ onLoginSuccess }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      const response = await fetch(`${API_BASE_URL}/Auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });

      const data = await response.json();

      if (response.ok && data.exitoso) {
        // Guardamos el token
        localStorage.setItem('inuzaru_token', data.token);
        localStorage.setItem('inuzaru_user', JSON.stringify(data.usuario));
        
        onLoginSuccess(data.usuario);
      } else {
        setError(data.mensaje || 'Credenciales inválidas. Acceso denegado.');
      }
    } catch (err) {
      setError('Fallo de conexión con el motor de autenticación.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#0f172a] flex items-center justify-center p-6 relative overflow-hidden">
      {/* Efectos de fondo cyberpunk */}
      <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-blue-600/10 blur-[120px] rounded-full" />
      <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-cyan-600/10 blur-[120px] rounded-full" />

      <div className="w-full max-w-md z-10">
        <div className="bg-[#1e293b]/50 backdrop-blur-xl border border-slate-800 p-8 rounded-[2.5rem] shadow-2xl">
          <header className="text-center mb-10">
            <div className="relative inline-block">
              <img src={InuzaruAvatar} alt="Inuzaru" className="w-24 h-24 rounded-full border-2 border-blue-500/30 p-1 mx-auto shadow-lg shadow-blue-500/10" />
              <div className="absolute bottom-0 right-0 p-1.5 bg-blue-600 rounded-full border-4 border-[#1e293b]">
                <Sparkles size={14} className="text-white" />
              </div>
            </div>
            <h1 className="mt-6 text-3xl font-black uppercase italic tracking-tighter text-white">Inuzaru Login</h1>
            <p className="text-[10px] text-blue-400 font-bold uppercase tracking-[0.2em] mt-2">Terminal de Acceso Seguro</p>
          </header>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <label className="text-[10px] font-black uppercase text-slate-500 ml-1 tracking-widest">Identificador (Email)</label>
              <div className="relative group">
                <Mail className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-blue-400 transition-colors" size={18} />
                <input 
                  type="email" 
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full bg-slate-900/50 border border-slate-700 rounded-2xl py-4 pl-12 pr-4 text-sm text-white focus:outline-none focus:border-blue-500 transition-all placeholder:text-slate-600"
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-[10px] font-black uppercase text-slate-500 ml-1 tracking-widest">Código de Acceso</label>
              <div className="relative group">
                <Lock className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500 group-focus-within:text-blue-400 transition-colors" size={18} />
                <input 
                  type="password" 
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full bg-slate-900/50 border border-slate-700 rounded-2xl py-4 pl-12 pr-4 text-sm text-white focus:outline-none focus:border-blue-500 transition-all placeholder:text-slate-600"
                  placeholder="••••••••"
                />
              </div>
            </div>

            {error && (
              <div className="bg-red-500/10 border border-red-500/20 p-4 rounded-2xl flex items-center gap-3 text-red-400 text-xs font-bold animate-pulse">
                <ShieldAlert size={18} />
                <span>{error}</span>
              </div>
            )}

            <button 
              type="submit" 
              disabled={isLoading}
              className="w-full bg-blue-600 hover:bg-blue-500 disabled:bg-slate-800 text-white font-black uppercase py-4 rounded-2xl shadow-xl shadow-blue-600/20 transition-all flex items-center justify-center gap-3"
            >
              {isLoading ? (
                <>
                  <Loader2 className="animate-spin" size={20} />
                  <span>Autenticando...</span>
                </>
              ) : (
                <span>Ingresar a la Terminal</span>
              )}
            </button>
          </form>

          <p className="text-center text-[9px] text-slate-600 mt-8 uppercase tracking-widest font-bold">
            Inuzaru RAG System • 2026
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;