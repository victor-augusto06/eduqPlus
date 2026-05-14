using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.Services;

namespace EduqPlus.API.Services {
    public class CursoService : ICursoService {
        
        private readonly EduqPlusContext _context;
        private readonly IIaService _iaService;

        public CursoService(EduqPlusContext context, IIaService iaService) {
            _context = context;
            _iaService = iaService;
        }

        public async Task<PagedResultDTO<CursoResponseDTO>> ObterTodosPaginadoAsync(int pagina, int tamanho) {
            var query = _context.Cursos.AsNoTracking();

            var totalItens = await query.CountAsync();

            var itens = await query
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(c => new CursoResponseDTO {
                    Id = c.Id,
                    Titulo = c.Titulo,
                    TrustScore = c.TrustScore,
                    StatusAuditoria = c.StatusAuditoria ?? EStatusAuditoria.NaoAuditado
                })
                .ToListAsync();

            return new PagedResultDTO<CursoResponseDTO> {
                Itens = itens,
                TotalItens = totalItens,
                PaginaAtual = pagina,
                TamanhoPagina = tamanho
            };
        }

        public async Task<CursoResponseDTO> CriarCursoAsync(CursoCreateDTO cursoDto) {
            var cursoId = Guid.NewGuid();

            string textoParaVetor = $"{cursoDto.Titulo}. {cursoDto.DescricaoOriginal}";
            var vetor = await _iaService.GerarEmbeddingAsync(textoParaVetor);

            var novoCurso = new Curso {
                Id = cursoId,
                ProdutorId = cursoDto.ProdutorId,
                CategoriaId = cursoDto.CategoriaId,
                UsuarioId = cursoDto.UsuarioId,
                Titulo = cursoDto.Titulo,
                DescricaoOriginal = cursoDto.DescricaoOriginal,
                PlataformaHospedagem = cursoDto.PlataformaHospedagem,
                VetorSemantico = vetor,
                StatusAuditoria = EStatusAuditoria.NaoAuditado,
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
                StatusAuditoria = novoCurso.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
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
                    StatusAuditoria = c.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
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
                    StatusAuditoria = c.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
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
                    StatusAuditoria = c.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
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
                    StatusAuditoria = c.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
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
                StatusAuditoria = cursoExistente.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
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
                StatusAuditoria = cursoExistente.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
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

        public async Task VerificarEAtualizarResumoIaAsync(Guid cursoId) {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null) return;

            int limiteNovosRegistros = 8;

            var queryAvaliacoes = _context.Avaliacoes.Where(a => a.CursoId == cursoId);
            var queryDenuncias = _context.Denuncia.Where(d => d.CursoId == cursoId);

            if (curso.DataUltimaAnaliseIa.HasValue) {
                queryAvaliacoes = queryAvaliacoes.Where(a => a.Data > curso.DataUltimaAnaliseIa.Value);
                queryDenuncias = queryDenuncias.Where(d => d.Data > curso.DataUltimaAnaliseIa.Value);
            }

            int totalNovos = await queryAvaliacoes.CountAsync() + await queryDenuncias.CountAsync();

            if (totalNovos >= limiteNovosRegistros || !curso.DataUltimaAnaliseIa.HasValue) {

                var textosAvaliacoes = await _context.Avaliacoes
                    .Where(a => a.CursoId == cursoId && !string.IsNullOrEmpty(a.Comentario))
                    .Select(a => $"[Avaliação - Nota {a.NotaEntrega}]: {a.Comentario}")
                    .ToListAsync();

                var textosDenuncias = await _context.Denuncia
                    .Where(d => d.CursoId == cursoId && !string.IsNullOrEmpty(d.RelatoDetalhado))
                    .Select(d => $"[Denúncia - {d.Categoria}]: {d.RelatoDetalhado}")
                    .ToListAsync();

                var todosComentarios = textosAvaliacoes.Concat(textosDenuncias).ToList();

                if (todosComentarios.Any()) {
                    var resumo = await _iaService.GerarResumoReputacaoAsync(todosComentarios);

                    curso.ResumoReputacao = resumo;
                    curso.DataUltimaAnaliseIa = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<CursoResponseDTO>> BuscarCursosInteligenteAsync(string termoBusca) {
            var vetorBusca = await _iaService.GerarEmbeddingAsync(termoBusca);

            var cursosNoBanco = await _context.Cursos
                .Include(c => c.PromessaCursos)
                .Include(c => c.Auditoria)
                    .ThenInclude(a => a.Auditor)
                .Include(c => c.Avaliacoes)
                .Include(c => c.Denuncia)
                .AsNoTracking()
                .Where(c => c.VetorSemantico != null)
                .ToListAsync();

            var ranking = cursosNoBanco.Select(curso => {
                double mediaNotas = curso.Avaliacoes.Any()
                    ? curso.Avaliacoes.Average(a => (a.NotaEntrega + a.NotaSuporte) / 2.0)
                    : 0;

                float similiaridade = MathUtils.CalcularSimilaridadeCosseno(vetorBusca, curso.VetorSemantico!);

                double scoreFinal = (similiaridade * 0.5) + ((mediaNotas / 5.0) * 0.5);

                return new {
                    Curso = curso,
                    ScoreFinal = scoreFinal,
                    Similaridade = similiaridade,
                    MediaReal = mediaNotas
                };
            })
                .Where(res => res.Similaridade > 0.70 && (res.MediaReal >= 3.0 || res.Curso.Avaliacoes.Count == 0))
                .OrderByDescending(res => res.ScoreFinal)
                .ThenByDescending(res => res.Curso.TrustScore)
                .Take(10)
                .ToList();

            return ranking.Select(r => new CursoResponseDTO {
                Id = r.Curso.Id,
                CategoriaId = r.Curso.CategoriaId,
                ProdutorId = r.Curso.ProdutorId,
                UsuarioId = r.Curso.UsuarioId,
                DescricaoOriginal = r.Curso.DescricaoOriginal ?? string.Empty,
                Titulo = r.Curso.Titulo,
                PlataformaHospedagem = r.Curso.PlataformaHospedagem,
                TrustScore = r.Curso.TrustScore,
                StatusAuditoria = r.Curso.StatusAuditoria ?? EStatusAuditoria.NaoAuditado,
                ResumoReputacao = r.Curso.ResumoReputacao,
                PromessaCursos = r.Curso.PromessaCursos?.Select(p => new PromessaCursoResponseDTO {
                    Id = p.Id,
                    CursoId = p.CursoId,
                    Descricao = p.Descricao,
                    CumpridaNaAuditoria = p.CumpridaNaAuditoria
                }).ToList() ?? new List<PromessaCursoResponseDTO>(),
                Auditoria = r.Curso.Auditoria?.Select(a => new AuditoriaResponseDTO {
                    Id = a.Id,
                    CursoId = a.CursoId,
                    AuditorId = a.AuditorId,
                    NomeAuditor = a.Auditor?.Nome ?? string.Empty,
                    TituloCurso = r.Curso.Titulo,
                    DataAuditoria = a.DataAuditoria,
                    Resultado = a.Resultado,
                    ObservacaoAuditor = a.ObservacaoAuditor
                }).ToList() ?? new List<AuditoriaResponseDTO>(),
                Avaliacoes = r.Curso.Avaliacoes?.Select(av => new AvaliacaoResponseDTO {
                    Id = av.Id,
                    CursoId = av.CursoId,
                    UsuarioId = av.UsuarioId,
                    NotaEntrega = av.NotaEntrega,
                    NotaSuporte = av.NotaSuporte,
                    Comentario = av.Comentario,
                    Data = av.Data,
                    StatusComprovante = av.StatusComprovante,
                    IsCompraVerificada = av.IsCompraVerificada,
                }).ToList() ?? new List<AvaliacaoResponseDTO>(),
                Denuncia = r.Curso.Denuncia?.Select(d => new DenunciaResponseDTO {
                    Id = d.Id,
                    CursoId = d.CursoId,
                    UsuarioId = d.UsuarioId,
                    Data = d.Data,
                    Categoria = d.Categoria,
                    RelatoDetalhado = d.RelatoDetalhado,
                    Status = d.Status
                }).ToList() ?? new List<DenunciaResponseDTO>()
            });
        }
    }
}