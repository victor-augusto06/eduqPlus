using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Services {
    public class CursoService : ICursoService {

        private readonly EduqPlusContext _context;

        public CursoService(EduqPlusContext context) {
            _context = context;
        }

        public async Task<IEnumerable<CursoResponseDTO>> ObterTodosAsync() {
            var cursos = await _context.Cursos
                .AsNoTracking()
                .Select(c => new CursoResponseDTO 
                {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    TrustScore = c.TrustScore,
                    StatusAuditoria = c.StatusAuditoria
                })
                .ToListAsync();
            return cursos;
        }

        public async Task<CursoResponseDTO> CriarCursoAsync(CursoCreateDTO cursoDto) {
            var novoCurso = new Curso {
                ProdutorId = cursoDto.ProdutorId,
                Titulo = cursoDto.Titulo,
                DescricaoOriginal = cursoDto.DescricaoOriginal,
                PlataformaHospedagem = cursoDto.PlataformaHospedagem
            };

            _context.Cursos.Add(novoCurso);
            await _context.SaveChangesAsync();

            return new CursoResponseDTO {
                Id = novoCurso.Id,
                Titulo = novoCurso.Titulo,
                TrustScore = novoCurso.TrustScore,
                StatusAuditoria = novoCurso.StatusAuditoria
            };
        }
    }
}