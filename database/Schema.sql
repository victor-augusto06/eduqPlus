CREATE DATABASE IF NOT EXISTS EduqPlus;
USE EduqPlus;

CREATE TABLE Usuario (
    Id CHAR(36) PRIMARY KEY,
    Nome VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    SenhaHash VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'Comum'
);

CREATE TABLE Produtor (
    Id CHAR(36) PRIMARY KEY,
    UsuarioId CHAR(36) NOT NULL,
    Nome VARCHAR(255) NOT NULL,
    NichoPrincipal VARCHAR(100),
    LinksSociais TEXT,
    CONSTRAINT FK_Produtor_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id) ON DELETE CASCADE
);

CREATE TABLE Categorias (
    Id CHAR(36) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    PRIMARY KEY (Id)
);

CREATE TABLE Curso (
    Id CHAR(36) PRIMARY KEY,
    UsuarioId CHAR(36) NULL, 
    CategoriaId CHAR(36) NOT NULL,
    ProdutorId CHAR(36) NOT NULL,
    Titulo VARCHAR(255) NOT NULL,
    DescricaoOriginal TEXT,
    PlataformaHospedagem VARCHAR(100),
    StatusAuditoria VARCHAR(50) NOT NULL DEFAULT 'EmAnalise',
    TrustScore INT DEFAULT 0,
    ResumoReputacao TEXT,
    DataUltimaAnaliseIA DATETIME NULL,
    VetorSemantico JSON NULL,
    CONSTRAINT FK_Curso_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Curso_Produtor FOREIGN KEY (ProdutorId) REFERENCES Produtor(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Curso_Categoria FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE RESTRICT
);

CREATE TABLE PromessaCurso (
    Id CHAR(36) PRIMARY KEY,
    CursoId CHAR(36) NOT NULL,
    Descricao TEXT NOT NULL,
    CumpridaNaAuditoria BOOLEAN NULL,
    CONSTRAINT FK_PromessaCurso_Curso FOREIGN KEY (CursoId) REFERENCES Curso(Id) ON DELETE CASCADE
);

CREATE TABLE Avaliacao (
    Id CHAR(36) PRIMARY KEY,
    UsuarioId CHAR(36) NOT NULL,
    CursoId CHAR(36) NOT NULL,
    Data DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    NotaEntrega INT NOT NULL CHECK (NotaEntrega BETWEEN 1 AND 5),
    NotaSuporte INT NOT NULL CHECK (NotaSuporte BETWEEN 1 AND 5),
    Comentario TEXT,
    UrlComprovante VARCHAR(500) NULL,
    StatusComprovante VARCHAR(50) NOT NULL DEFAULT 'Pendente', 
    IsCompraVerificada BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT FK_Avaliacao_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Avaliacao_Curso FOREIGN KEY (CursoId) REFERENCES Curso(Id) ON DELETE CASCADE
);

CREATE TABLE Denuncia (
    Id CHAR(36) PRIMARY KEY,
    UsuarioId CHAR(36) NOT NULL,
    CursoId CHAR(36) NOT NULL,
    Data DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Categoria VARCHAR(100) NOT NULL, 
    RelatoDetalhado TEXT NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'EmAnalise', 
    CONSTRAINT FK_Denuncia_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Denuncia_Curso FOREIGN KEY (CursoId) REFERENCES Curso(Id) ON DELETE CASCADE
);

CREATE TABLE Auditoria (
    Id CHAR(36) PRIMARY KEY,
    CursoId CHAR(36) NOT NULL,
    AuditorId CHAR(36) NOT NULL,
    DataAuditoria DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CriterioAnalisado VARCHAR(100) NOT NULL, 
    Resultado VARCHAR(50) NOT NULL, 
    ObservacaoAuditor TEXT,
    CONSTRAINT FK_Auditoria_Curso FOREIGN KEY (CursoId) REFERENCES Curso(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Auditoria_Auditor FOREIGN KEY (AuditorId) REFERENCES Usuario(Id) ON DELETE RESTRICT
);