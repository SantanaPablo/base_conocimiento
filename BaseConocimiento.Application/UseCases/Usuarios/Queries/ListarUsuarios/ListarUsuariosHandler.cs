using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Usuarios.Queries.ListarUsuarios
{
    public class ListarUsuariosHandler : IRequestHandler<ListarUsuariosQuery, ListarUsuariosResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListarUsuariosHandler> _logger;

        public ListarUsuariosHandler(IUnitOfWork unitOfWork, ILogger<ListarUsuariosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ListarUsuariosResponse> Handle(ListarUsuariosQuery request, CancellationToken ct)
        {
            try
            {
                var usuarios = await _unitOfWork.Usuarios.ListarConEstadisticasAsync(
                    soloActivos: request.SoloActivos,
                    departamento: request.Departamento,
                    ct
                );

                var usuariosDto = usuarios.Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    NombreCompleto = u.NombreCompleto,
                    Email = u.Email,
                    Departamento = u.Departamento,
                    Rol = u.Rol.ToString(),
                    EsActivo = u.EsActivo,
                    FechaRegistro = u.FechaRegistro,
                    UltimoAcceso = u.UltimoAcceso,
                    TotalManualesSubidos = u.ManualesSubidos?.Count ?? 0,
                    TotalConsultas = u.ConsultasRealizadas?.Count ?? 0
                }).ToList();

                return new ListarUsuariosResponse
                {
                    Exitoso = true,
                    Usuarios = usuariosDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar usuarios");
                return new ListarUsuariosResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error: {ex.Message}"
                };
            }
        }
    }
}
