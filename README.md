# Eduq+ 🚀

O **Eduq+** é uma plataforma independente de avaliação e transparência de infoprodutos. O ecossistema é projetado para empoderar consumidores no mercado de cursos online por meio de avaliações auditadas com comprovantes, denúncias estruturadas e análise automatizada via Inteligência Artificial integrada de forma local.

Projeto acadêmico (2026).

## 👥 Integrantes da Equipe
1. Victor Augusto Farias Ferreira
2. Felipe Alexandre Pereira
3. Lucas Barroso Silvestrini
4. Gustavo Felipe

## 📖 Descrição Detalhada do Projeto
O Eduq+ atua como um portal de auditoria para combater o charlatanismo digital no mercado de cursos online. A plataforma cruza promessas de páginas de vendas com a percepção real dos alunos, calculando um *TrustScore* dos produtos. Para garantir a veracidade das avaliações, o sistema exige a submissão de comprovantes de consumo do curso (como boletos e certificados), os quais passam por um rigoroso processo de validação utilizando Visão Computacional. Além disso, inteligência artificial é empregada de forma ampla no ecossistema para estruturar dados, buscar intenções e recomendar cursos similares com base em filtragem de conteúdo, com um algoritmo de Classificação KNN.

## 🖼️ Pipeline de Processamento de Imagens
O módulo de processamento atua na validação automatizada de comprovantes de compra e certificados, submetidos pelos alunos. O fluxo segue as seguintes etapas integradas no microsserviço:

1. **Pré-processamento da Imagem:** Recebimento do documento (imagem), redimensionamento para redução do custo computacional e conversão para escala de cinza seguida de binarização (preto e branco), otimizando o documento para o reconhecimento de caracteres.
2. **Operações Morfológicas:** Aplicação de erosão e abertura de área. Esta etapa é fundamental para limpar ruídos do fundo da imagem e destacar o texto, melhorando significativamente a precisão da leitura.
3. **Extração de Texto (OCR):** Utilização de ferramentas de Reconhecimento Óptico de Caracteres (como *Tesseract*) para extrair o texto bruto contido no comprovante tratado.
4. **Estruturação Semântica:** O texto bruto extraído é enviado como *prompt* para o agente de IA (Llama3). A inteligência artificial analisa o bloco de texto desestruturado e identifica/retorna as entidades exatas (nome do aluno, data e curso adquirido) para validação no sistema.

## 🛠️ Stack Tecnológica

Seguindo a modelagem de serviços conteinerizados e a arquitetura cliente-servidor descrita na documentação técnica:

* **Camada de Apresentação:** React + TypeScript (SPA)
* **Camada de Aplicação (API):** C# ASP.NET Core & Entity Framework Core
* **Camada de Visão Computacional:** Python (Flask/FastAPI, OpenCV, PyTesseract)
* **Camada de LLM:** Agente Ollama (Processamento de Linguagem Natural Local)
* **Camada de Machining Learning:** Algoritmo de K-Nearest Neighbors (KNN) implementado nativamente no backend (Utilização para recomendação de cursos)
* **Camada de Dados (BD):** MySQL 8.0
* **Infraestrutura:** Docker & Docker Compose

## 📦 Como Instalar e Executar a Infraestrutura

Graças à conteinerização total da arquitetura via Docker, você não precisa instalar compiladores, runtimes ou servidores de banco de dados locais em sua máquina hospedeira.

### Pré-requisitos

* [Git](https://git-scm.com) instalado.
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) rodando ativamente na máquina.

### Passos de Inicialização (Primeira Instalação ou Inicialização)

Para garantir que o banco de dados seja criado corretamente e a Inteligência Artificial provisionada sem falhas de sincronia na primeira execução, siga o fluxo limpo abaixo:

1. Clone o repositório do projeto para a sua máquina.

2. Configure o seu arquivo `docker-compose.yml` seguindo o `docker-compose.yml.example`

3. Abra o terminal na pasta raiz do repositório clonado `...\Eduq+` (onde está o arquivo `docker-compose.yml`).
 
4. **Limpeza de Segurança:** Garanta que não há volumes ou contêineres residuais bloqueando o banco de dados rodando o comando:
   ```bash
   docker compose down -v
   ```
   
5. Subir a Infraestrutura: Execute o comando de orquestração abaixo para construir e subir todos os serviços em segundo plano:
   ````bash
   docker compose up -d --build
   ````

6. Aguardar o Processamento da IA (Data Seeder): O backend iniciará a inserção de dados de testes e a geração local dos embeddings semânticos em segundo plano. Aguarde de 60 a 90 segundos antes de acessar o sistema pela primeira vez para garantir que todos os cursos e avaliações estejam perfeitamente sincronizados no banco de dados.

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
| **Administrador / Auditor** | `admin@eduqplus.com` | `Admin123` | Validação de comprovantes de compra, execução de auditoria técnica cruzando promessas da página de vendas e moderação de denúncias .|
| **Usuário Ativo / Consumidor** | `carlos.ativo@gmail.com` | `User123` | Demonstrar controle de propriedade. Ele possui cursos cadastrados, avaliações escritas por ele e denúncias ativas (botões de edição e exclusão aparecem apenas para o dono do registro). |
| **Usuário Limpo** | `mariana.limpa@gmail.com` | `User123` | Experiência limpa de um novo estudante ingressando na plataforma do zero. |

---

## 🧠 Validação Prática dos Recursos de IA

1. **Busca Semântica por Intenção:** Vá ao catálogo de cores e pesquise por conceitos abstratos como *"banco de dados corporativo performático"* ou *"ganhos reais corrigidos pela inflação"*. Os embeddings gerados localmente pela IA farão o cruzamento semântico aproximado mesmo que nenhuma palavra exata conste nos títulos.
2. **Resumo Inteligente de Reputação:** Abra o curso *"Formação Avançada em C#"* ou *"Alocação Estratégica"*. A seção superior exibirá um painel contendo a consolidação automatizada feita pela IA dividida entre "Resumo Geral", "Pontos Fortes" e "Pontos de Atenção" capturados com base nas avaliações reais submetidas no portal .
