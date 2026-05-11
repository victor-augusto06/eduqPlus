using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using EduqPlus.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service {
    public class AuditoriaService : IAuditoriaService {

        private readonly EduqPlusContext _context;
        public AuditoriaService(EduqPlusContext context) {
            _context = context;
        }

        public async Task<IEnumerable<AuditoriaResponseDTO>> ObterAuditoriasCursoAsync(Guid cursoId) {
            var auditorias = await _context.Auditoria
                .AsNoTracking()
                .Where(a => a.CursoId == cursoId)
                .Select(c => new AuditoriaResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    AuditorId = c.AuditorId,
                    NomeAuditor = c.Auditor.Nome,
                    TituloCurso = c.Curso.Titulo,
                    DataAuditoria = c.DataAuditoria,
                    Resultado = c.Resultado,
                    ObservacaoAuditor = c.ObservacaoAuditor
                })
                .ToListAsync();
            return auditorias;
        }

        public async Task<IEnumerable<AuditoriaResponseDTO>> ObterAuditoriasPendentesAsync() {
            var auditorias = await _context.Auditoria
                .AsNoTracking()
                .Where(a => a.Resultado == EStatusAuditoria.EmAnalise)
                .Select(c => new AuditoriaResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    AuditorId = c.AuditorId, 
                    NomeAuditor = c.Auditor.Nome,
                    TituloCurso = c.Curso.Titulo,
                    DataAuditoria = c.DataAuditoria,
                    Resultado = c.Resultado,
                    ObservacaoAuditor = c.ObservacaoAuditor
                })
                .ToListAsync();
            return auditorias;
        }

        public async Task<IEnumerable<AuditoriaResponseDTO>> ObterAuditoriasConcluidasAsync(Guid? auditorId = null) {

            var query = _context.Auditoria
                .AsNoTracking()
                .Where(a => a.Resultado != EStatusAuditoria.EmAnalise);

            if (auditorId.HasValue) {
                query = query.Where(a => a.AuditorId == auditorId.Value);
            }

            var auditorias = await query
                .Select(a => new AuditoriaResponseDTO {
                    Id = a.Id,
                    CursoId = a.CursoId,
                    TituloCurso = a.Curso.Titulo,
                    DataAuditoria = a.DataAuditoria,
                    Resultado = a.Resultado,
                    ObservacaoAuditor = a.ObservacaoAuditor
                })
                .ToListAsync();

            return auditorias;
        }

        public async Task<AuditoriaResponseDTO> CriarAuditoriaAsync(AuditoriaCreateDTO auditoriaDto) {

            var infoRelacionada = await _context.Cursos
                .Where(c => c.Id == auditoriaDto.CursoId)
                .Select(c => new {
                    Titulo = c.Titulo,
                    NomeAuditor = _context.Usuarios
                    .Where(u => u.Id == auditoriaDto.AuditorId)
                    .Select(u => u.Nome)
                    .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            var novaAuditoria = new Auditoria {
                Id = Guid.NewGuid(),
                CursoId = auditoriaDto.CursoId,
                AuditorId = auditoriaDto.AuditorId,
                CriterioAnalisado = auditoriaDto.CriterioAnalisado,
                Resultado = auditoriaDto.Resultado,
                ObservacaoAuditor = auditoriaDto.ObservacaoAuditor,
                DataAuditoria = DateTime.Now
            };

            _context.Auditoria.Add(novaAuditoria);
            await _context.SaveChangesAsync();

            return new AuditoriaResponseDTO {
                Id = novaAuditoria.Id,
                CursoId = novaAuditoria.CursoId,
                AuditorId = novaAuditoria.AuditorId,
                NomeAuditor = infoRelacionada?.NomeAuditor ?? "Sistema",
                TituloCurso = infoRelacionada?.Titulo ?? string.Empty,
                DataAuditoria = novaAuditoria.DataAuditoria,
                Resultado = novaAuditoria.Resultado,
                ObservacaoAuditor = novaAuditoria.ObservacaoAuditor
            };
        }

        public async Task<AuditoriaResponseDTO> AtualizarAuditoriaAsync(Guid id, Guid auditorId, AuditoriaUpdateDTO auditoriaDto) {
            
            var auditoriaExistente = await _context.Auditoria
                .Include(a => a.Curso)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auditoriaExistente == null) 
                throw new Exception("Auditoria não encontrada.");

            auditoriaExistente.AuditorId = auditorId;
            auditoriaExistente.Resultado = auditoriaDto.Resultado;
            auditoriaExistente.ObservacaoAuditor = auditoriaDto.ObservacaoAuditor;

            await _context.SaveChangesAsync();

            return new AuditoriaResponseDTO {
                Id = auditoriaExistente.Id,
                CursoId = auditoriaExistente.CursoId,
                AuditorId = auditoriaExistente.AuditorId,
                NomeAuditor = _context.Usuarios
                    .Where(u => u.Id == auditorId)
                    .Select(u => u.Nome)
                    .FirstOrDefault() ?? "Sistema",
                TituloCurso = auditoriaExistente.Curso.Titulo,
                DataAuditoria = auditoriaExistente.DataAuditoria,
                Resultado = auditoriaExistente.Resultado,
                ObservacaoAuditor = auditoriaExistente.ObservacaoAuditor
            };
        }
    }
}
