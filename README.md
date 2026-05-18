# Eduq+ 🚀

O **Eduq+** é uma plataforma independente de avaliação e transparência de infoprodutos. O ecossistema é projetado para empoderar consumidores no mercado de cursos online por meio de avaliações auditadas com comprovantes, denúncias estruturadas e análise automatizada via Inteligência Artificial integrada de forma local.

Projeto acadêmico (2026).

---

## 🛠️ Stack Tecnológica

Seguindo a modelagem de serviços conteinerizados e a arquitetura cliente-servidor descrita na documentação técnica:

* 
**Camada de Apresentação:** React + TypeScript (SPA) 


* 
**Camada de Aplicação (API):** C# ASP.NET Core & Entity Framework Core 


* 
**Camada de Dados (BD):** MySQL 8.0 


* 
**Camada de Inteligência (IA):** Agente Ollama (Processamento de Linguagem Natural Local) 


* 
**Infraestrutura:** Docker & Docker Compose 



---

## 📦 Como Instalar e Executar a Infraestrutura

Graças à conteinerização total da arquitetura via Docker, você não precisa instalar compiladores, runtimes ou servidores de banco de dados locais em sua máquina hospedeira.

### Pré-requisitos

* [Git](https://git-scm.com) instalado.
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) rodando ativamente na máquina.

### Passo Único de Inicialização

1. Clone o repositório do projeto para a sua máquina.
2. Abra o terminal na pasta raiz do repositório clonado (onde está o arquivo `docker-compose.yml`).
3. Execute o comando de orquestração abaixo:

```bash
docker-compose up -d --build

```

---

## 🔗 Portas e Endereços Disponíveis

Após a inicialização bem-sucedida de todos os contêineres, os serviços estarão acessíveis através dos seguintes mapeamentos de portas:

* **Interface Web do Usuário (Frontend):** `http://localhost:5173`
* **API RESTful (Backend):** `http://localhost:5292`
* **Banco de Dados (MySQL):** `localhost:3306`
* **Serviço de IA (Ollama API):** `http://localhost:11434`

*Nota:* O backend aplica as migrations do Entity Framework Core e executa automaticamente o **Data Seeder** estruturado na primeira inicialização, populando o banco com a massa de dados para testes imediatos.

---

## 👥 Cenários Disponíveis para Teste de Permissões

A massa de dados inserida automaticamente no banco configura as contas abaixo para simular as permissões mapeadas nos diagramas de casos de uso do sistema:

| Perfil / Ator | E-mail de Acesso | Senha Padrão | Funcionalidades Demonstráveis no Cenário |
| --- | --- | --- | --- |
| **Administrador / Auditor** | `admin@eduqplus.com` | `Admin123` | Validação de comprovantes de compra, execução de auditoria técnica cruzando promessas da página de vendas e moderação de denúncias .

 |
| **Usuário Ativo / Consumidor** | `carlos.ativo@gmail.com` | `User123` | Demonstrar controle de propriedade. Ele possui cursos cadastrados, avaliações escritas por ele e denúncias ativas (botões de edição e exclusão aparecem apenas para o dono do registro). |
| **Usuário Limpo** | `mariana.limpa@gmail.com` | `User123` | Experiência limpa de um novo estudante ingressando na plataforma do zero. |

---

## 🧠 Validação Prática dos Recursos de IA

1. **Busca Semântica por Intenção:** Vá ao catálogo de cores e pesquise por conceitos abstratos como *"banco de dados corporativo performático"* ou *"ganhos reais corrigidos pela inflação"*. Os embeddings gerados localmente pela IA farão o cruzamento semântico aproximado mesmo que nenhuma palavra exata conste nos títulos.
2. **Resumo Inteligente de Reputação:** Abra o curso *"Formação Avançada em C#"* ou *"Alocação Estratégica"*. A seção superior exibirá um painel contendo a consolidação automatizada feita pela IA dividida entre "Resumo Geral", "Pontos Fortes" e "Pontos de Atenção" capturados com base nas avaliações reais submetidas no portal .