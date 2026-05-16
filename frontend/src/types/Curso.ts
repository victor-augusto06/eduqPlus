export const EStatusAuditoria = {
  NaoAuditado: 1,
  Aprovado: 2,
  Reprovado: 3
} as const;

export type TStatusAuditoria = typeof EStatusAuditoria[keyof typeof EStatusAuditoria];

export interface PromessaCurso {
  id: string;
  descricao: string;
  cumpridaNaAuditoria: boolean;
}

export interface Auditoria {
  id: string;
  nomeAuditor: string;
  resultado: string;
  observacaoAuditor: string;
  dataAuditoria: string;
}

export interface Avaliacao {
  id: string;
  notaEntrega: number;
  notaSuporte: number;
  comentario: string;
  data: string;
  isCompraVerificada: boolean;
}

export interface Denuncia {
  id: string;
  categoria: string;
  relatoDetalhado: string;
  status: string;
  data: string;
}

export interface Curso {
  id: string;
  categoriaId: string;
  produtorId: string;
  usuarioId?: string;
  descricaoOriginal: string;
  titulo: string;
  plataformaHospedagem?: string;
  trustScore: number;
  statusAuditoria: TStatusAuditoria;
  resumoReputacao?: string;
  promessaCursos: PromessaCurso[];
  auditoria: Auditoria[];
  avaliacoes: Avaliacao[];
  denuncia: Denuncia[];
}