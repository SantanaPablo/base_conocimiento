using BaseConocimiento.Application.Interfaces.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.UseCases.Categorias.Queries.ObtenerCategoriaPorId
{
    public class ObtenerUsuarioPorIdHandler : IRequestHandler<ObtenerUsuarioPorIdQuery, ObtenerUsuarioPorIdResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtenerUsuarioPorIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ObtenerUsuarioPorIdResponse> Handle(ObtenerUsuarioPorIdQuery request, CancellationToken ct)
        {
            var usuario = await _unitOfWork.Usuarios.ObtenerConDetallesAsync(request.UsuarioId, ct);

            if (usuario == null)
            {
                return new ObtenerUsuarioPorIdResponse
                {
                    Exitoso = false,
                    Mensaje = "Usuario no encontrado"
                };
            }

            return new ObtenerUsuarioPorIdResponse
            {
                Exitoso = true,
                Usuario = new UsuarioDetalleDto
                {
                    Id = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email,
                    Departamento = usuario.Departamento,
                    Rol = usuario.Rol.ToString(),
                    EsActivo = usuario.EsActivo,
                    FechaRegistro = usuario.FechaRegistro,
                    UltimoAcceso = usuario.UltimoAcceso,
                    TotalManualesSubidos = usuario.ManualesSubidos?.Count ?? 0,
                    TotalConsultas = usuario.ConsultasRealizadas?.Count ?? 0,
                    UltimaConsulta = usuario.ConsultasRealizadas?.OrderByDescending(c => c.FechaConsulta).FirstOrDefault()?.FechaConsulta
                }
            };
        }
    }
}
