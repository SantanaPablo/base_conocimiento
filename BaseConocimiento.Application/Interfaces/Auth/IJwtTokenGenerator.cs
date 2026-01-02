using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseConocimiento.Application.Interfaces.Auth
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid usuarioId, string email, string nombreCompleto, string rol);
    }
}
