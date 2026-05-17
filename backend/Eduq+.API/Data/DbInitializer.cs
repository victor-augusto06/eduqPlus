using EduqPlus.API.Models;
using EduqPlus.API.Enums;
using EduqPlus.API.DTOs; // Garanta que suas DTOs de criação estão mapeadas aqui
using EduqPlus.API.Interfaces; // Namespace das suas interfaces de serviço
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduqPlus.API.Data {
    public static class DbInitializer {
        public static async Task SeedAsync(IServiceProvider serviceProvider) {
            // Resolve o contexto para checagem e migração
            var context = serviceProvider.GetRequiredService<EduqPlusContext>();
            await context.Database.MigrateAsync();

            // Evita duplicidade
            if (await context.Usuarios.AnyAsync() || await context.Categorias.AnyAsync()) {
                return;
            }

            Console.WriteLine("[SEED] Iniciando a geração da base via Services (Gatilhos de IA ativos)...");

            // Resolve os serviços do container de Injeção de Dependência
            var produtorService = serviceProvider.GetRequiredService<IProdutorService>();
            var cursoService = serviceProvider.GetRequiredService<ICursoService>();
            // Se possuir serviços criados para Avaliação e Denúncia, resolva-os aqui:
            // var avaliacaoService = serviceProvider.GetRequiredService<IAvaliacaoService>();
            // var denunciaService = serviceProvider.GetRequiredService<IDenunciaService>();

            // 1. USUÁRIOS (Podem ser inseridos via Context pois a lógica de hash já está aqui)
            var adminId = Guid.NewGuid();
            var usuarioAtivoId = Guid.NewGuid();
            var usuarioLimpoId = Guid.NewGuid();

            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    Id = adminId,
                    Nome = "Administrador do Sistema",
                    Email = "admin@eduqplus.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    Role = ERoleUsuario.Admin
                },
                new Usuario
                {
                    Id = usuarioAtivoId,
                    Nome = "Carlos Eduardo Souza",
                    Email = "carlos.ativo@gmail.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("User123"),
                    Role = ERoleUsuario.Comum
                },
                new Usuario
                {
                    Id = usuarioLimpoId,
                    Nome = "Mariana Silva Costa",
                    Email = "mariana.limpa@gmail.com",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("User123"),
                    Role = ERoleUsuario.Comum
                }
            };
            await context.Usuarios.AddRangeAsync(usuarios);

            // 2. CATEGORIAS (Inserção direta via Context)
            var catProgramacaoId = Guid.NewGuid();
            var catIaDataId = Guid.NewGuid();
            var catFinancasId = Guid.NewGuid();

            var categorias = new List<Categoria>
            {
                new Categoria { Id = catProgramacaoId, Nome = "Desenvolvimento de Software e Programação" },
                new Categoria { Id = catIaDataId, Nome = "Inteligência Artificial, Machine Learning e Ciência de Dados" },
                new Categoria { Id = catFinancasId, Nome = "Finanças, Investimentos e Mercado Financeiro" }
            };
            await context.Categorias.AddRangeAsync(categorias);
            await context.SaveChangesAsync(); // Salva base estrutural necessária para os relacionamentos dos Services

            // 3. PRODUTORES (Via Service -> Dispara regras de negócio)
            var prodTechDto = new ProdutorCreateDTO {
                UsuarioId = usuarioAtivoId,
                Nome = "Guilherme Silveira (CodeAcademy)",
                NichoPrincipal = "Backend, Arquitetura de Sistemas e Engenharia de Software",
                LinksSociais = "https://github.com/codeacademy;https://linkedin.com/in/guilhermetech"
            };
            var prodTechRes = await produtorService.CriarProdutorAsync(prodTechDto);

            var prodIaDto = new ProdutorCreateDTO {
                UsuarioId = adminId,
                Nome = "Dra. Sandra Santos (DataScience Pro)",
                NichoPrincipal = "Deep Learning, Redes Neurais e Modelos de Linguagem (LLMs)",
                LinksSociais = "https://datasciencepro.com.br;https://linkedin.com/in/sandradata"
            };
            var prodIaRes = await produtorService.CriarProdutorAsync(prodIaDto);

            var prodFinDto = new ProdutorCreateDTO {
                UsuarioId = adminId,
                Nome = "Roberto Perfil (Finanças do Zero)",
                NichoPrincipal = "Renda Fixa, Alocação de Ativos, Macroeconomia e Tesouro Direto",
                LinksSociais = "https://instagram.com/financasdozero"
            };
            var prodFinRes = await produtorService.CriarProdutorAsync(prodFinDto);

            var cursoCsharpDto = new CursoCreateDTO {
                CategoriaId = catProgramacaoId,
                ProdutorId = prodTechRes.Id,
                UsuarioId = usuarioAtivoId,
                Titulo = "Formação Avançada em C# e Arquitetura Clean Architecture",
                PlataformaHospedagem = "Hotmart",
                DescricaoOriginal = "Um treinamento profundo focado no desenvolvimento backend utilizando .NET 8 e C#. O curso aborda a implementação prática dos padrões da Clean Architecture (Arquitetura Limpa), separando responsabilidades com Domain-Driven Design (DDD). Inclui módulos avançados sobre persistência de dados com Entity Framework Core (EF Core), manipulação assíncrona com Async/Await, estruturação de controllers enxutos via Expression-Bodied Members, WebSockets em tempo real com SignalR, caching distribuído com Redis e criação de APIs REST robustas e performáticas prontas para escalabilidade em nuvem.",
                PromessaCursos = new List<PromessaCursoCreateDTO>
                {
                    new PromessaCursoCreateDTO { Descricao = "Acesso vitalício ao conteúdo e atualizações de novas versões do .NET." },
                    new PromessaCursoCreateDTO { Descricao = "Suporte técnico direto com os instrutores em menos de 24 horas." },
                    new PromessaCursoCreateDTO { Descricao = "Projeto final prático pronto para portfólio no GitHub." }
                }
            };
            var cursoCsharpRes = await cursoService.CriarCursoAsync(cursoCsharpDto);

            var cursoIaDto = new CursoCreateDTO {
                CategoriaId = catIaDataId,
                ProdutorId = prodIaRes.Id,
                UsuarioId = adminId,
                Titulo = "Masterclass de IA e Engenharia de Prompt com Python",
                PlataformaHospedagem = "Kiwify",
                DescricaoOriginal = "Este curso foca na aplicação prática de Inteligência Artificial sem apenas consumir APIs prontas. Você aprenderá os fundamentos de Machine Learning de ponta a ponta, engenharia de prompts avançada, criação de pipelines de dados, processamento de linguagem natural (PLN) e integração com Grandes Modelos de Linguagem (LLMs) como Llama 3 e GPT. O treinamento utiliza bibliotecas robustas do ecossistema Python, incluindo PyTorch, Scikit-Learn, Pandas e Hugging Face, culminando no desenvolvimento de um agente inteligente proprietário e modelos preditivos aplicados ao mercado real.",
                PromessaCursos = new List<PromessaCursoCreateDTO>
                {
                    new PromessaCursoCreateDTO { Descricao = "Material didático complementar com notebooks Jupyter exclusivos." },
                    new PromessaCursoCreateDTO { Descricao = "Comunidade fechada no Discord para networking e soluções de bugs." }
                }
            };
            var cursoIaRes = await cursoService.CriarCursoAsync(cursoIaDto);

            var cursoFinancasDto = new CursoCreateDTO {
                CategoriaId = catFinancasId,
                ProdutorId = prodFinRes.Id,
                UsuarioId = adminId,
                Titulo = "Alocação Estratégica: Do Tesouro Direto às Criptomoedas",
                PlataformaHospedagem = "Eduzz",
                DescricaoOriginal = "Um guia completo sobre planejamento financeiro, investimentos e montagem de carteiras diversificadas resilientes a ciclos de inflação. O treinamento ensina a calcular rentabilidade real corrigida pelo IPCA, analisar títulos de renda fixa prefixados e pós-fixados, operar na bolsa de valores com foco em dividendos e introduz conceitos de gerenciamento de risco em ativos de alta volatilidade, como fundos imobiliários, ativos internacionais e criptomoedas. Foco total em independência financeira através de aportes constantes e rebalanceamento dinâmico de portfólio.",
                PromessaCursos = new List<PromessaCursoCreateDTO>
                {
                    new PromessaCursoCreateDTO { Descricao = "Planilha automatizada de controle de gastos e projeção de juros compostos." },
                    new PromessaCursoCreateDTO { Descricao = "Mentorias mensais ao vivo para tirar dúvidas sobre macroeconomia." }
                }
            };
            var cursoFinancasRes = await cursoService.CriarCursoAsync(cursoFinancasDto);

            var c1 = await context.Cursos.FindAsync(cursoCsharpRes.Id);
            if (c1 != null) { c1.TrustScore = 4.5; c1.StatusAuditoria = EStatusAuditoria.Aprovado; }

            var c2 = await context.Cursos.FindAsync(cursoIaRes.Id);
            if (c2 != null) { c2.TrustScore = 3.0; c2.StatusAuditoria = EStatusAuditoria.NaoAuditado; }

            var c3 = await context.Cursos.FindAsync(cursoFinancasRes.Id);
            if (c3 != null) { c3.TrustScore = 1.5; c3.StatusAuditoria = EStatusAuditoria.Reprovado; }
            await context.SaveChangesAsync();

            var avaliacoes = new List<Avaliacao>
            {
                new Avaliacao
                {
                    Id = Guid.NewGuid(),
                    CursoId = cursoCsharpRes.Id,
                    UsuarioId = usuarioAtivoId,
                    NotaEntrega = 5.0,
                    NotaSuporte = 4.5,
                    Comentario = "O curso cumpre perfeitamente tudo o que promete. As explicações sobre Clean Architecture e DDD abriram minha mente sobre como estruturar projetos corporativos escaláveis.",
                    Data = DateTime.Now.AddDays(-10),
                    StatusComprovante = EStatusComprovante.Aprovado,
                    IsCompraVerificada = true,
                    UrlComprovante = "/files/comprovantes/seeder-csharp.pdf"
                },
                new Avaliacao
                {
                    Id = Guid.NewGuid(),
                    CursoId = cursoIaRes.Id,
                    UsuarioId = usuarioAtivoId,
                    NotaEntrega = 3.0,
                    NotaSuporte = 3.0,
                    Comentario = "O conteúdo sobre Python e manipulação de dados com Pandas é decente, mas a parte de Engenharia de Prompt e integração com LLMs ficou muito superficial.",
                    Data = DateTime.Now.AddDays(-5),
                    StatusComprovante = EStatusComprovante.Pendente,
                    IsCompraVerificada = false,
                    UrlComprovante = "/files/comprovantes/seeder-ia.jpg"
                }
            };
            await context.Avaliacoes.AddRangeAsync(avaliacoes);

            var denuncias = new List<Denuncia>
            {
                new Denuncia
                {
                    Id = Guid.NewGuid(),
                    CursoId = cursoFinancasRes.Id,
                    UsuarioId = usuarioAtivoId,
                    Categoria = ETipoDenuncia.FalsaPromessaDeGanhos,
                    RelatoDetalhado = "O produtor prometeu na página de vendas que o método garantiria retornos de 5% ao mês garantidos sem risco através de operações de arbitragem. Isso é impossível no mercado financeiro.",
                    Data = DateTime.Now.AddDays(-2),
                    Status = EStatusDenuncia.EmAnalise
                }
            };
            await context.Denuncia.AddRangeAsync(denuncias);
            await context.SaveChangesAsync();

            Console.WriteLine("[SEED] Base de dados gerada com sucesso! Gatilhos executados com êxito.");
        }
    }
}