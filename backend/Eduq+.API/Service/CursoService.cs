using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
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
            var cursoId = Guid.NewGuid();

            var novoCurso = new Curso {
                Id = cursoId,
                ProdutorId = cursoDto.ProdutorId,
                CategoriaId = cursoDto.CategoriaId,
                UsuarioId = cursoDto.UsuarioId,
                Titulo = cursoDto.Titulo,
                DescricaoOriginal = cursoDto.DescricaoOriginal,
                PlataformaHospedagem = cursoDto.PlataformaHospedagem,
                PromessaCursos = cursoDto.PromessaCursos.Select(p => new PromessaCurso {
                    Id = Guid.NewGuid(),
                    CursoId = cursoId,
                    Descricao = p.Descricao
                }).ToList()
            };

            _context.Cursos.Add(novoCurso);
            await _context.SaveChangesAsync();

            return new CursoResponseDTO {
                Id = novoCurso.Id,
                CategoriaId = novoCurso.CategoriaId,
                ProdutorId = novoCurso.ProdutorId,
                UsuarioId = novoCurso.UsuarioId,
                DescricaoOriginal = novoCurso.DescricaoOriginal,
                Titulo = novoCurso.Titulo,
                PlataformaHospedagem = novoCurso.PlataformaHospedagem,
                TrustScore = novoCurso.TrustScore,
                StatusAuditoria = novoCurso.StatusAuditoria,
                ResumoReputacao = novoCurso.ResumoReputacao,
                PromessaCursos = novoCurso.PromessaCursos.Select(p => new PromessaCursoResponseDTO {
                    Id = p.Id,
                    CursoId = p.CursoId,
                    Descricao = p.Descricao,
                    CumpridaNaAuditoria = p.CumpridaNaAuditoria
                }).ToList(),
                Auditoria = new List<AuditoriaResponseDTO>(),
                Avaliacoes = new List<AvaliacaoResponseDTO>(),
                Denuncia = new List<DenunciaResponseDTO>()
            };
        }

        public async Task<CursoResponseDTO> ObterPorIdAsync(Guid id) {
            var curso = await _context.Cursos
                .AsNoTracking()
                .Select(c => new CursoResponseDTO {
                    Id = c.Id,
                    CategoriaId = c.CategoriaId,
                    ProdutorId = c.ProdutorId,
                    UsuarioId = c.UsuarioId,
                    DescricaoOriginal = c.DescricaoOriginal ?? string.Empty,
                    Titulo = c.Titulo,
                    PlataformaHospedagem = c.PlataformaHospedagem,
                    TrustScore = c.TrustScore,
                    StatusAuditoria = c.StatusAuditoria,
                    ResumoReputacao = c.ResumoReputacao,
                    PromessaCursos = c.PromessaCursos.Select(p => new PromessaCursoResponseDTO {
                        Id = p.Id,
                        CursoId = p.CursoId,
                        Descricao = p.Descricao,
                        CumpridaNaAuditoria = p.CumpridaNaAuditoria
                    }).ToList(),
                    Auditoria = c.Auditoria.Select(a => new AuditoriaResponseDTO {
                        Id = a.Id,
                        CursoId = a.CursoId,
                        AuditorId = a.AuditorId,
                        NomeAuditor = a.Auditor.Nome ?? string.Empty,
                        TituloCurso = c.Titulo,
                        DataAuditoria = a.DataAuditoria,
                        Resultado = a.Resultado,
                        ObservacaoAuditor = a.ObservacaoAuditor
                    }).ToList(),
                    Avaliacoes = c.Avaliacoes.Select(av => new AvaliacaoResponseDTO {
                        Id = av.Id,
                        CursoId = av.CursoId,
                        UsuarioId = av.UsuarioId,
                        NotaEntrega = av.NotaEntrega,
                        NotaSuporte = av.NotaSuporte,
                        Comentario = av.Comentario,
                        Data = av.Data,
                        StatusComprovante = av.StatusComprovante,
                        IsCompraVerificada = av.IsCompraVerificada,
                    }).ToList(),
                    Denuncia = c.Denuncia.Select(d => new DenunciaResponseDTO {
                        Id = d.Id,
                        CursoId = d.CursoId,
                        UsuarioId = d.UsuarioId,
                        Data = d.Data,
                        Categoria = d.Categoria,
                        RelatoDetalhado = d.RelatoDetalhado,
                        Status = d.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (curso == null)
                throw new Exception("Curso não encontrado.");

            return curso;
        }

        public async Task<IEnumerable<CursoResponseDTO>> ObterCursoCategoriasAsync(Guid categoriaId) {
            var cursos = await _context.Cursos
                .AsNoTracking()
                .Where(a => a.CategoriaId == categoriaId)
                .Select(c => new CursoResponseDTO {
                    Id = c.Id,
                    CategoriaId = c.CategoriaId,
                    ProdutorId = c.ProdutorId,
                    UsuarioId = c.UsuarioId,
                    DescricaoOriginal = c.DescricaoOriginal ?? string.Empty,
                    Titulo = c.Titulo,
                    PlataformaHospedagem = c.PlataformaHospedagem,
                    TrustScore = c.TrustScore,
                    StatusAuditoria = c.StatusAuditoria,
                    ResumoReputacao = c.ResumoReputacao,
                    PromessaCursos = c.PromessaCursos.Select(p => new PromessaCursoResponseDTO {
                        Id = p.Id,
                        CursoId = p.CursoId,
                        Descricao = p.Descricao,
                        CumpridaNaAuditoria = p.CumpridaNaAuditoria
                    }).ToList(),
                    Auditoria = c.Auditoria.Select(a => new AuditoriaResponseDTO {
                        Id = a.Id,
                        CursoId = a.CursoId,
                        AuditorId = a.AuditorId,
                        NomeAuditor = a.Auditor.Nome ?? string.Empty,
                        TituloCurso = c.Titulo,
                        DataAuditoria = a.DataAuditoria,
                        Resultado = a.Resultado,
                        ObservacaoAuditor = a.ObservacaoAuditor
                    }).ToList(),
                    Avaliacoes = c.Avaliacoes.Select(av => new AvaliacaoResponseDTO {
                        Id = av.Id,
                        CursoId = av.CursoId,
                        UsuarioId = av.UsuarioId,
                        NotaEntrega = av.NotaEntrega,
                        NotaSuporte = av.NotaSuporte,
                        Comentario = av.Comentario,
                        Data = av.Data,
                        StatusComprovante = av.StatusComprovante,
                        IsCompraVerificada = av.IsCompraVerificada,
                    }).ToList(),
                    Denuncia = c.Denuncia.Select(d => new DenunciaResponseDTO {
                        Id = d.Id,
                        CursoId = d.CursoId,
                        UsuarioId = d.UsuarioId,
                        Data = d.Data,
                        Categoria = d.Categoria,
                        RelatoDetalhado = d.RelatoDetalhado,
                        Status = d.Status
                    }).ToList()
                })
                .ToListAsync();

            if (!cursos.Any())
                throw new Exception("Nenhum curso foi encontrado para essa categoria.");

            return cursos;
        }

        public async Task<IEnumerable<CursoResponseDTO>> ObterCursoProdutorAsync(Guid produtorId) {
            var cursos = await _context.Cursos
                .AsNoTracking()
                .Where(a => a.ProdutorId == produtorId)
                .Select(c => new CursoResponseDTO {
                    Id = c.Id,
                    CategoriaId = c.CategoriaId,
                    ProdutorId = c.ProdutorId,
                    UsuarioId = c.UsuarioId,
                    DescricaoOriginal = c.DescricaoOriginal ?? string.Empty,
                    Titulo = c.Titulo,
                    PlataformaHospedagem = c.PlataformaHospedagem,
                    TrustScore = c.TrustScore,
                    StatusAuditoria = c.StatusAuditoria,
                    ResumoReputacao = c.ResumoReputacao,
                    PromessaCursos = c.PromessaCursos.Select(p => new PromessaCursoResponseDTO {
                        Id = p.Id,
                        CursoId = p.CursoId,
                        Descricao = p.Descricao,
                        CumpridaNaAuditoria = p.CumpridaNaAuditoria
                    }).ToList(),
                    Auditoria = c.Auditoria.Select(a => new AuditoriaResponseDTO {
                        Id = a.Id,
                        CursoId = a.CursoId,
                        AuditorId = a.AuditorId,
                        NomeAuditor = a.Auditor.Nome ?? string.Empty,
                        TituloCurso = c.Titulo,
                        DataAuditoria = a.DataAuditoria,
                        Resultado = a.Resultado,
                        ObservacaoAuditor = a.ObservacaoAuditor
                    }).ToList(),
                    Avaliacoes = c.Avaliacoes.Select(av => new AvaliacaoResponseDTO {
                        Id = av.Id,
                        CursoId = av.CursoId,
                        UsuarioId = av.UsuarioId,
                        NotaEntrega = av.NotaEntrega,
                        NotaSuporte = av.NotaSuporte,
                        Comentario = av.Comentario,
                        Data = av.Data,
                        StatusComprovante = av.StatusComprovante,
                        IsCompraVerificada = av.IsCompraVerificada,
                    }).ToList(),
                    Denuncia = c.Denuncia.Select(d => new DenunciaResponseDTO {
                        Id = d.Id,
                        CursoId = d.CursoId,
                        UsuarioId = d.UsuarioId,
                        Data = d.Data,
                        Categoria = d.Categoria,
                        RelatoDetalhado = d.RelatoDetalhado,
                        Status = d.Status
                    }).ToList()
                })
                .ToListAsync();

            if (!cursos.Any())
                throw new Exception("Nenhum curso desse produtor foi encontrado.");

            return cursos;
        }

        public async Task<IEnumerable<CursoResponseDTO>> ObterCursoStatusAuditoriaAsync(EStatusAuditoria status) {
            var cursos = await _context.Cursos
                .AsNoTracking()
                .Where(a => a.StatusAuditoria == status)
                .Select(c => new CursoResponseDTO {
                    Id = c.Id,
                    CategoriaId = c.CategoriaId,
                    ProdutorId = c.ProdutorId,
                    UsuarioId = c.UsuarioId,
                    DescricaoOriginal = c.DescricaoOriginal ?? string.Empty,
                    Titulo = c.Titulo,
                    PlataformaHospedagem = c.PlataformaHospedagem,
                    TrustScore = c.TrustScore,
                    StatusAuditoria = c.StatusAuditoria,
                    ResumoReputacao = c.ResumoReputacao,
                    PromessaCursos = c.PromessaCursos.Select(p => new PromessaCursoResponseDTO {
                        Id = p.Id,
                        CursoId = p.CursoId,
                        Descricao = p.Descricao,
                        CumpridaNaAuditoria = p.CumpridaNaAuditoria
                    }).ToList(),
                    Auditoria = c.Auditoria.Select(a => new AuditoriaResponseDTO {
                        Id = a.Id,
                        CursoId = a.CursoId,
                        AuditorId = a.AuditorId,
                        NomeAuditor = a.Auditor.Nome ?? string.Empty,
                        TituloCurso = c.Titulo,
                        DataAuditoria = a.DataAuditoria,
                        Resultado = a.Resultado,
                        ObservacaoAuditor = a.ObservacaoAuditor
                    }).ToList(),
                    Avaliacoes = c.Avaliacoes.Select(av => new AvaliacaoResponseDTO {
                        Id = av.Id,
                        CursoId = av.CursoId,
                        UsuarioId = av.UsuarioId,
                        NotaEntrega = av.NotaEntrega,
                        NotaSuporte = av.NotaSuporte,
                        Comentario = av.Comentario,
                        Data = av.Data,
                        StatusComprovante = av.StatusComprovante,
                        IsCompraVerificada = av.IsCompraVerificada,
                    }).ToList(),
                    Denuncia = c.Denuncia.Select(d => new DenunciaResponseDTO {
                        Id = d.Id,
                        CursoId = d.CursoId,
                        UsuarioId = d.UsuarioId,
                        Data = d.Data,
                        Categoria = d.Categoria,
                        RelatoDetalhado = d.RelatoDetalhado,
                        Status = d.Status
                    }).ToList()
                })
                .ToListAsync();

            if (!cursos.Any())
                throw new Exception("Nenhum curso com esse status foi encontrado.");

            return cursos;
        }

        public async Task<CursoResponseDTO> AlterarCursoAsync(Guid id, Guid usuarioId, CursoUpdateDTO cursoDto) {
            var cursoExistente = await _context.Cursos
                .Include(c => c.PromessaCursos)
                .Include(c => c.Auditoria)
                    .ThenInclude(a => a.Auditor)
                .Include(c => c.Avaliacoes)
                .Include(c => c.Denuncia)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (cursoExistente == null)
                throw new Exception("Curso não encontrado.");

            if (cursoExistente.UsuarioId != usuarioId)
                throw new Exception("Você não tem permissão para alterar este curso.");

            cursoExistente.Titulo = cursoDto.Titulo;
            cursoExistente.DescricaoOriginal = cursoDto.DescricaoOriginal;
            cursoExistente.CategoriaId = cursoDto.CategoriaId;
            cursoExistente.PlataformaHospedagem = cursoDto.PlataformaHospedagem;

            _context.RemoveRange(cursoExistente.PromessaCursos);

            cursoExistente.PromessaCursos = cursoDto.PromessaCursos.Select(p => new PromessaCurso {
                Id = Guid.NewGuid(),
                CursoId = cursoExistente.Id,
                Descricao = p.Descricao
            }).ToList();

            await _context.SaveChangesAsync();

            return new CursoResponseDTO {
                Id = cursoExistente.Id,
                CategoriaId = cursoExistente.CategoriaId,
                ProdutorId = cursoExistente.ProdutorId,
                UsuarioId = cursoExistente.UsuarioId,
                DescricaoOriginal = cursoExistente.DescricaoOriginal,
                Titulo = cursoExistente.Titulo,
                PlataformaHospedagem = cursoExistente.PlataformaHospedagem,
                TrustScore = cursoExistente.TrustScore,
                StatusAuditoria = cursoExistente.StatusAuditoria,
                ResumoReputacao = cursoExistente.ResumoReputacao,
                PromessaCursos = cursoExistente.PromessaCursos.Select(p => new PromessaCursoResponseDTO {
                    Id = p.Id,
                    CursoId = p.CursoId,
                    Descricao = p.Descricao,
                    CumpridaNaAuditoria = p.CumpridaNaAuditoria
                }).ToList(),
                Auditoria = cursoExistente.Auditoria.Select(a => new AuditoriaResponseDTO {
                    Id = a.Id,
                    CursoId = a.CursoId,
                    AuditorId = a.AuditorId,
                    NomeAuditor = a.Auditor?.Nome ?? string.Empty,
                    TituloCurso = cursoExistente.Titulo,
                    DataAuditoria = a.DataAuditoria,
                    Resultado = a.Resultado,
                    ObservacaoAuditor = a.ObservacaoAuditor
                }).ToList(),
                Avaliacoes = cursoExistente.Avaliacoes.Select(av => new AvaliacaoResponseDTO {
                    Id = av.Id,
                    CursoId = av.CursoId,
                    UsuarioId = av.UsuarioId,
                    NotaEntrega = av.NotaEntrega,
                    NotaSuporte = av.NotaSuporte,
                    Comentario = av.Comentario,
                    Data = av.Data,
                    StatusComprovante = av.StatusComprovante,
                    IsCompraVerificada = av.IsCompraVerificada,
                }).ToList(),
                Denuncia = cursoExistente.Denuncia.Select(d => new DenunciaResponseDTO {
                    Id = d.Id,
                    CursoId = d.CursoId,
                    UsuarioId =d.UsuarioId,
                    Data = d.Data,
                    Categoria = d.Categoria,
                    RelatoDetalhado = d.RelatoDetalhado,
                    Status = d.Status
                }).ToList()
            };

        }

        public async Task<CursoResponseDTO> AlterarCursoAdminAsync(Guid id, Guid usuarioId, CursoUpdateAdminDTO cursoDto) {
            var cursoExistente = await _context.Cursos
                .Include(c => c.PromessaCursos)
                .Include(c => c.Auditoria)
                .ThenInclude(a => a.Auditor)
                .Include(c => c.Avaliacoes)
                .Include(c => c.Denuncia)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (cursoExistente == null)
                throw new Exception("Curso não encontrado.");

            var usuarioRequisicao = await _context.Usuarios.FindAsync(usuarioId);

            if (usuarioRequisicao == null || usuarioRequisicao.Role != ERoleUsuario.Admin)
                throw new Exception("Você não tem permissão de administrador para alterar este curso.");

            cursoExistente.Titulo = cursoDto.Titulo;
            cursoExistente.DescricaoOriginal = cursoDto.DescricaoOriginal;
            cursoExistente.CategoriaId = cursoDto.CategoriaId;
            cursoExistente.PlataformaHospedagem = cursoDto.PlataformaHospedagem;
            cursoExistente.StatusAuditoria = cursoDto.StatusAuditoria;
            cursoExistente.TrustScore = cursoDto.TrustScore;
            cursoExistente.ResumoReputacao = cursoDto.ResumoReputacao;
            cursoExistente.DataUltimaAnaliseIa = cursoDto.DataUltimaAnaliseIa;

            _context.RemoveRange(cursoExistente.PromessaCursos);

            cursoExistente.PromessaCursos = cursoDto.PromessaCursos.Select(p => new PromessaCurso {
                Id = Guid.NewGuid(),
                CursoId = cursoExistente.Id,
                Descricao = p.Descricao
            }).ToList();

            await _context.SaveChangesAsync();

            return new CursoResponseDTO {
                Id = cursoExistente.Id,
                CategoriaId = cursoExistente.CategoriaId,
                ProdutorId = cursoExistente.ProdutorId,
                UsuarioId = cursoExistente.UsuarioId,
                DescricaoOriginal = cursoExistente.DescricaoOriginal,
                Titulo = cursoExistente.Titulo,
                PlataformaHospedagem = cursoExistente.PlataformaHospedagem,
                TrustScore = cursoExistente.TrustScore,
                StatusAuditoria = cursoExistente.StatusAuditoria,
                ResumoReputacao = cursoExistente.ResumoReputacao,
                PromessaCursos = cursoExistente.PromessaCursos.Select(p => new PromessaCursoResponseDTO {
                    Id = p.Id,
                    CursoId = p.CursoId,
                    Descricao = p.Descricao,
                    CumpridaNaAuditoria = p.CumpridaNaAuditoria
                }).ToList(),
                Auditoria = cursoExistente.Auditoria.Select(a => new AuditoriaResponseDTO {
                    Id = a.Id,
                    CursoId = a.CursoId,
                    AuditorId = a.AuditorId,
                    NomeAuditor = a.Auditor?.Nome ?? string.Empty,
                    TituloCurso = cursoExistente.Titulo,
                    DataAuditoria = a.DataAuditoria,
                    Resultado = a.Resultado,
                    ObservacaoAuditor = a.ObservacaoAuditor
                }).ToList(),
                Avaliacoes = cursoExistente.Avaliacoes.Select(av => new AvaliacaoResponseDTO {
                    Id = av.Id,
                    CursoId = av.CursoId,
                    UsuarioId = av.UsuarioId,
                    NotaEntrega = av.NotaEntrega,
                    NotaSuporte = av.NotaSuporte,
                    Comentario = av.Comentario,
                    Data = av.Data,
                    StatusComprovante = av.StatusComprovante,
                    IsCompraVerificada = av.IsCompraVerificada,
                }).ToList(),
                Denuncia = cursoExistente.Denuncia.Select(d => new DenunciaResponseDTO {
                    Id = d.Id,
                    CursoId = d.CursoId,
                    UsuarioId = d.UsuarioId,
                    Data = d.Data,
                    Categoria = d.Categoria,
                    RelatoDetalhado = d.RelatoDetalhado,
                    Status = d.Status
                }).ToList()
            };
        }

        public async Task<bool> ExcluirCursoAsync(Guid id, Guid usuarioId) {
            try {
                var cursoExistente = await _context.Cursos
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (cursoExistente == null)
                    throw new Exception("Curso não encontrado.");

                var usuarioRequisicao = await _context.Usuarios.FindAsync(usuarioId);

                bool isDono = cursoExistente.UsuarioId == usuarioId;
                bool isAdmin = usuarioRequisicao != null && usuarioRequisicao.Role == ERoleUsuario.Admin;

                if (!isDono && !isAdmin)
                    throw new Exception("Você não tem permissão para excluir este curso.");

                _context.Cursos.Remove(cursoExistente);
                await _context.SaveChangesAsync();

                return true;
            } 
            catch (Exception ex) {
                Console.WriteLine("CursoService.ExcluirCursoAsync - Erro inesperado ocorreu " + ex.ToString());
                return false;
            }
        }
    }
}