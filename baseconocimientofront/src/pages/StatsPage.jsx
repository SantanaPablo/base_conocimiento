import React, { useState, useEffect } from 'react';
import { 
  BarChart3, 
  MessageSquare, 
  Files, 
  Users, 
  Clock, 
  Activity, 
  ArrowUpRight,
  Search,
  History
} from 'lucide-react';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const StatsPage = () => {
  const [stats, setStats] = useState(null);
  const [historial, setHistorial] = useState([]);
  const [loading, setLoading] = useState(true);
  
  // Filtros para el historial
  const [page, setPage] = useState(1);

  //historial
  const fetchData = async () => {
    setLoading(true);
    try {
      // generales
      const resStats = await fetch(`${API_BASE_URL}/Estadisticas/generales`);
      const dataStats = await resStats.json();
      
      const resHistorial = await fetch(`${API_BASE_URL}/Estadisticas/historial?pagina=${page}&tamañoPagina=10`);
      const dataHistorial = await resHistorial.json();

      if (dataStats.exitoso) setStats(dataStats);
      if (dataHistorial.exitoso) setHistorial(dataHistorial.items || []);
      
    } catch (err) {
      console.error("Error al cargar analíticas:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [page]);

  return (
    <div className="h-full bg-[#0f172a] p-8 overflow-y-auto scrollbar-thin scrollbar-thumb-slate-800">
      <div className="max-w-6xl mx-auto space-y-10">
        
        {/* CABECERA */}
        <header>
          <div className="flex items-center gap-3 mb-2">
            <BarChart3 className="text-blue-500" size={24} />
            <h2 className="text-3xl font-black text-white uppercase italic tracking-tighter">Panel de Telemetría</h2>
          </div>
          <p className="text-slate-400">Análisis del motor RAG y métricas de interacción con la base de conocimiento.</p>
        </header>

        {/* MÉTRICAS RÁPIDAS */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <StatCard 
            icon={<Files />} 
            label="Manuales Indexados" 
            value={stats?.totalManuales || '0'} 
            color="blue"
          />
          <StatCard 
            icon={<MessageSquare />} 
            label="Consultas Totales" 
            value={stats?.totalConsultas || '0'} 
            color="cyan"
          />
          <StatCard 
            icon={<Users />} 
            label="Usuarios Activos" 
            value={stats?.totalUsuarios || '0'} 
            color="purple"
          />
          <StatCard 
            icon={<Activity />} 
            label="Precisión RAG" 
            value="94.2%" 
            color="green"
            isTrend
          />
        </div>

        {/* SECCIÓN DE HISTORIAL */}
        <div className="bg-[#1e293b] border border-slate-800 rounded-[2.5rem] overflow-hidden shadow-2xl">
          <div className="p-6 border-b border-slate-800 flex justify-between items-center bg-[#1e293b]/50">
            <div className="flex items-center gap-3">
              <History className="text-blue-400" size={20} />
              <h3 className="font-bold text-slate-100 uppercase tracking-widest text-sm">Registro de Actividad</h3>
            </div>
            <button onClick={fetchData} className="text-xs text-slate-500 hover:text-blue-400 transition-colors">
              Actualizar logs
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="bg-slate-900/50 text-slate-500 text-[10px] uppercase font-black tracking-[0.2em]">
                  <th className="px-6 py-4">Fecha / Hora</th>
                  <th className="px-6 py-4">Usuario</th>
                  <th className="px-6 py-4">Consulta (Trigger)</th>
                  <th className="px-6 py-4">Fuentes Citadas</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800">
                {historial.map((item, idx) => (
                  <tr key={idx} className="hover:bg-blue-600/5 transition-colors group">
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2 text-slate-400 text-xs font-mono">
                        <Clock size={12} />
                        {new Date(item.fechaConsulta).toLocaleTimeString()}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <span className="text-xs font-bold text-slate-300">Pablo Britez</span>
                    </td>
                    <td className="px-6 py-4">
                      <p className="text-xs text-slate-400 max-w-xs truncate group-hover:text-slate-200">
                        {item.pregunta}
                      </p>
                    </td>
                    <td className="px-6 py-4">
                      <span className="px-2 py-1 bg-slate-800 rounded text-[9px] font-black text-blue-400 border border-blue-500/10">
                        {item.cantidadFuentes || 0} CITAS
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            
            {historial.length === 0 && !loading && (
              <div className="py-20 text-center">
                <p className="text-slate-600 italic">No hay registros de actividad recientes.</p>
              </div>
            )}
          </div>
        </div>

      </div>
    </div>
  );
};

/**
 * tarjetas de métricas
 */
const StatCard = ({ icon, label, value, color, isTrend }) => {
  const colors = {
    blue: 'text-blue-500 bg-blue-500/10 border-blue-500/20',
    cyan: 'text-cyan-500 bg-cyan-500/10 border-cyan-500/20',
    purple: 'text-purple-500 bg-purple-500/10 border-purple-500/20',
    green: 'text-green-500 bg-green-500/10 border-green-500/20'
  };

  return (
    <div className="bg-[#1e293b] p-6 rounded-3xl border border-slate-800 hover:border-slate-700 transition-all shadow-lg group relative overflow-hidden">
      <div className={`w-12 h-12 rounded-2xl flex items-center justify-center mb-4 ${colors[color]}`}>
        {icon}
      </div>
      <div>
        <p className="text-[10px] font-black uppercase text-slate-500 tracking-widest mb-1">{label}</p>
        <div className="flex items-baseline gap-2">
          <h4 className="text-3xl font-black text-white tracking-tighter">{value}</h4>
          {isTrend && <ArrowUpRight size={16} className="text-green-500" />}
        </div>
      </div>
      <div className="absolute -right-4 -bottom-4 w-24 h-24 bg-blue-600/5 rounded-full blur-3xl group-hover:bg-blue-600/10 transition-all" />
    </div>
  );
};

export default StatsPage;