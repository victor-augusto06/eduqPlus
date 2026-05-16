using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using EduqPlus.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service {
    public class AvaliacaoService : IAvaliacaoService {

        private readonly EduqPlusContext _context;
        private readonly ICursoService _cursoService;
        private readonly IWebHostEnvironment _environment;

        public AvaliacaoService(EduqPlusContext context, ICursoService cursoService, IWebHostEnvironment environment) {
            _context = context;
            _cursoService = cursoService;
            _environment = environment;
        }

        public async Task<bool> ExcluirAvaliacaoAsync(Guid id, Guid usuarioId) {
            try 
            {
                var avaliacaoExistente = await _context.Avaliacoes
                    .Include(a => a.Curso)
                        .ThenInclude(c => c.Avaliacoes)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (avaliacaoExistente == null)
                    throw new Exception("Avaliação não encontrada.");

                if (avaliacaoExistente.UsuarioId != usuarioId)
                    throw new Exception("Você não tem permissão para excluir esta avaliação.");

                if (!string.IsNullOrEmpty(avaliacaoExistente.UrlComprovante)) {

                    string caminhoRelativo = avaliacaoExistente.UrlComprovante.TrimStart('/');
                    string pastaBase = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", ".."));
                    string caminhoFisicoCompleto = Path.Combine(pastaBase, caminhoRelativo.Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(caminhoFisicoCompleto)) {
                        System.IO.File.Delete(caminhoFisicoCompleto);
                    }
                }

                _context.Avaliacoes.Remove(avaliacaoExistente);

                avaliacaoExistente.Curso.AtualizarTrustScore();

                await _context.SaveChangesAsync();
                
                return true;
            } 
            catch (Exception ex) 
            {
                Console.WriteLine("AvaliacaoService.ExcluirAvaliacaoAsync - Erro inesperado ocorreu: " + ex.ToString());
                return false;
            }
        }

        public async Task<AvaliacaoResponseDTO> AtualizarAvaliacaoAsync(Guid id, Guid usuarioId, AvaliacaoUpdateDTO avaliacaoDTO) {
            var avaliacaoExistente = await _context.Avaliacoes
                .Include(a => a.Curso)
                    .ThenInclude(c => c.Avaliacoes)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (avaliacaoExistente == null)
                throw new Exception("Avaliação não encontrada.");

            if (avaliacaoExistente.UsuarioId != usuarioId)
                throw new Exception("Você não tem permissão para alterar esta avaliação.");

            avaliacaoExistente.NotaEntrega = avaliacaoDTO.NotaEntrega;
            avaliacaoExistente.NotaSuporte = avaliacaoDTO.NotaSuporte;
            avaliacaoExistente.Comentario = avaliacaoDTO.Comentario;
            avaliacaoExistente.UrlComprovante = avaliacaoDTO.UrlComprovante;
            avaliacaoExistente.Curso.AtualizarTrustScore();

            await _context.SaveChangesAsync();

            return new AvaliacaoResponseDTO {
                Id = avaliacaoExistente.Id,
                CursoId = avaliacaoExistente.CursoId,
                UsuarioId = usuarioId,
                NotaEntrega = avaliacaoExistente.NotaEntrega,
                NotaSuporte = avaliacaoExistente.NotaSuporte,
                Comentario = avaliacaoExistente.Comentario,
                Data = avaliacaoExistente.Data,
                StatusComprovante = avaliacaoExistente.StatusComprovante,
                IsCompraVerificada = avaliacaoExistente.IsCompraVerificada
            };
        }

        public async Task<AvaliacaoResponseDTO> CriarAvaliacaoAsync(AvaliacaoCreateDTO avaliacaoDTO) {
            var curso = await _context.Cursos
                .Include(c => c.Avaliacoes)
                .FirstOrDefaultAsync(c => c.Id == avaliacaoDTO.CursoId);

            if (curso == null)
                throw new Exception("Curso não encontrado.");

            string? urlCaminhoArquivo = null;

            if (avaliacaoDTO.UrlComprovante != null && avaliacaoDTO.UrlComprovante.Length > 0) {
                var extensoesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
                var extensao = Path.GetExtension(avaliacaoDTO.UrlComprovante.FileName).ToLowerInvariant();

                if (!extensoesPermitidas.Contains(extensao))
                    throw new Exception("Formato de arquivo não permitido. Envie apenas PDF, JPG ou PNG.");

                if (avaliacaoDTO.UrlComprovante.Length > 5 * 1024 * 1024)
                    throw new Exception("O arquivo excede o tamanho máximo permitido de 5MB.");

                string nomeUnicoArquivo = Guid.NewGuid().ToString() + extensao;

                string pastaDestino = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "files", "comprovantes"));

                if (!Directory.Exists(pastaDestino))
                    Directory.CreateDirectory(pastaDestino);

                string caminhoCompletoFisico = Path.Combine(pastaDestino, nomeUnicoArquivo);

                using (var stream = new FileStream(caminhoCompletoFisico, FileMode.Create)) {
                    await avaliacaoDTO.UrlComprovante.CopyToAsync(stream);
                }

                urlCaminhoArquivo = $"/files/comprovantes/{nomeUnicoArquivo}";
            }

            var novaAvaliacao = new Avaliacao {
                Id = Guid.NewGuid(),
                CursoId = avaliacaoDTO.CursoId,
                UsuarioId = avaliacaoDTO.UsuarioId,
                NotaEntrega = avaliacaoDTO.NotaEntrega,
                NotaSuporte = avaliacaoDTO.NotaSuporte,
                Comentario = avaliacaoDTO.Comentario,
                UrlComprovante = urlCaminhoArquivo,
                StatusComprovante = EStatusComprovante.Pendente,
                Data = DateTime.Now,
                IsCompraVerificada = false
            };

            _context.Avaliacoes.Add(novaAvaliacao);
            curso.AtualizarTrustScore();
            await _context.SaveChangesAsync();

            await _cursoService.VerificarEAtualizarResumoIaAsync(novaAvaliacao.CursoId);

            return new AvaliacaoResponseDTO {
                Id = novaAvaliacao.Id,
                CursoId = novaAvaliacao.CursoId,
                UsuarioId = novaAvaliacao.UsuarioId,
                NotaEntrega = novaAvaliacao.NotaEntrega,
                NotaSuporte = novaAvaliacao.NotaSuporte,
                Comentario = novaAvaliacao.Comentario,
                Data = novaAvaliacao.Data,
                StatusComprovante = novaAvaliacao.StatusComprovante,
                IsCompraVerificada = novaAvaliacao.IsCompraVerificada
            };
        }

        public async Task<IEnumerable<AvaliacaoResponseDTO>> ObterAvaliacoesUsuarioAsync(Guid usuarioId) {
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.UsuarioId == usuarioId)
                .Select(c => new AvaliacaoResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    NotaEntrega = c.NotaEntrega,
                    NotaSuporte = c.NotaSuporte,
                    Comentario = c.Comentario,
                    Data = c.Data,
                    StatusComprovante = c.StatusComprovante,
                    IsCompraVerificada = c.IsCompraVerificada
                })
                .ToListAsync();

            return avaliacoes;
        }

        public async Task<IEnumerable<AvaliacaoResponseDTO>> ObterAvaliacoesValidadasAsync(Guid cursoId) {
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.CursoId == cursoId && a.IsCompraVerificada == true)
                .Select(c => new AvaliacaoResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    NotaEntrega = c.NotaEntrega,
                    NotaSuporte = c.NotaSuporte,
                    Comentario = c.Comentario,
                    Data = c.Data,
                    StatusComprovante = c.StatusComprovante,
                    IsCompraVerificada = c.IsCompraVerificada
                })
                .ToListAsync();

            return avaliacoes;
        }

        public async Task<AvaliacaoResponseDTO> ObterPorIdAsync(Guid id) {
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Select(c => new AvaliacaoResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    NotaEntrega = c.NotaEntrega,
                    NotaSuporte = c.NotaSuporte,
                    Comentario = c.Comentario,
                    Data = c.Data,
                    StatusComprovante = c.StatusComprovante,
                    IsCompraVerificada = c.IsCompraVerificada
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (avaliacoes == null)
                throw new Exception("Avaliação não encontrada.");

            return avaliacoes;
        }

        public async Task<IEnumerable<AvaliacaoResponseDTO>> ObterTodosAsync(Guid cursoId) {
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.CursoId == cursoId)
                .Select(c => new AvaliacaoResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    NotaEntrega = c.NotaEntrega,
                    NotaSuporte = c.NotaSuporte,
                    Comentario = c.Comentario,
                    Data = c.Data,
                    StatusComprovante = c.StatusComprovante,
                    IsCompraVerificada = c.IsCompraVerificada
                })
                .ToListAsync();
            return avaliacoes;
        }

        public async Task<IEnumerable<AvaliacaoResponseAdminDTO>> ObterAvaliacoesAdminPorStatusAsync(EStatusComprovante status) {
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.StatusComprovante == status) 
                .Select(c => new AvaliacaoResponseAdminDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    NotaEntrega = c.NotaEntrega,
                    NotaSuporte = c.NotaSuporte,
                    Comentario = c.Comentario,
                    Data = c.Data,
                    UrlComprovante = c.UrlComprovante,
                    StatusComprovante = c.StatusComprovante,
                    IsCompraVerificada = c.IsCompraVerificada
                })
                .ToListAsync();

            return avaliacoes;
        }

        public async Task<AvaliacaoResponseAdminDTO> ValidarComprovanteAsync(Guid id, Guid usuarioId, EStatusComprovante statusComprovante) {
            var usuarioAdmin = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuarioAdmin == null || usuarioAdmin.Role != ERoleUsuario.Admin) {
                throw new Exception("Apenas administradores podem validar comprovantes.");
            }

            var avaliacaoExistente = await _context.Avaliacoes
                .FirstOrDefaultAsync(a => a.Id == id);

            if (avaliacaoExistente == null)
                throw new Exception("Avaliação não encontrada.");

            avaliacaoExistente.StatusComprovante = statusComprovante;

            if (statusComprovante == EStatusComprovante.Aprovado) {
                avaliacaoExistente.IsCompraVerificada = true;
            } else {
                avaliacaoExistente.IsCompraVerificada = false;
            }

            await _context.SaveChangesAsync();

            return new AvaliacaoResponseAdminDTO {
                Id = avaliacaoExistente.Id,
                CursoId = avaliacaoExistente.CursoId,
                UsuarioId = avaliacaoExistente.UsuarioId,
                NotaEntrega = avaliacaoExistente.NotaEntrega,
                NotaSuporte = avaliacaoExistente.NotaSuporte,
                Comentario = avaliacaoExistente.Comentario,
                UrlComprovante = avaliacaoExistente.UrlComprovante,
                Data = avaliacaoExistente.Data,
                StatusComprovante = avaliacaoExistente.StatusComprovante,
                IsCompraVerificada = avaliacaoExistente.IsCompraVerificada
            };
        }
    }
}
