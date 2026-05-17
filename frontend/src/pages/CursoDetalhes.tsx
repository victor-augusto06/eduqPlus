import React, { useState, useEffect } from 'react';
import { 
  Box, Container, Typography, Button, Paper, CircularProgress, 
  Grid, Card, CardContent, Chip, Divider, AppBar, Toolbar, IconButton, Rating, Avatar
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import VerifiedIcon from '@mui/icons-material/Verified';
import api from '../services/api';
import { type Curso, EStatusAuditoria } from '../types/Curso';
import NovaAvaliacaoDialog from './NovaAvaliacaoDialog';
import NovaDenunciaDialog from './NovaDenunciaDialog';

const CursoDetalhes = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [curso, setCurso] = useState<Curso | null>(null);
  const [loading, setLoading] = useState(true);
  const [modalAvaliacaoAberta, setModalAvaliacaoAberta] = useState(false);
  const [modalDenunciaAberta, setModalDenunciaAberta] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [nomeProdutor, setNomeProdutor] = useState<string>('Carregando...');

  const carregarDetalhesCurso = async () => {
    setLoading(true);
    try {
      const response = await api.get(`/Curso/${id}`);
      const dadosCurso = response.data;
      setCurso(dadosCurso);

      try {
        const prodResponse = await api.get(`/Produtor/${dadosCurso.produtorId}`);
        setNomeProdutor(prodResponse.data.nome);
      } catch (prodError) {
        console.error("Erro ao carregar produtor", prodError);
        setNomeProdutor('Produtor Desconhecido');
      }

    } catch (error) {
      console.error("Erro ao carregar detalhes do curso", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregarDetalhesCurso();
    
    const token = localStorage.getItem('@EduqPlus:token');
    if (token) {
      setIsLoggedIn(true);
    }
  }, [id]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', backgroundColor: '#f9fafb' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!curso) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mt: 10 }}>
        <Typography variant="h5" color="text.secondary" gutterBottom>
          Curso não encontrado.
        </Typography>
        <Button variant="contained" onClick={() => navigate('/')}>
          Voltar para o Catálogo
        </Button>
      </Box>
    );
  }

  const getStatusComprovanteChip = (status: number) => {
    switch (status) {
      case 2:
        return { label: 'Avaliação Comprovada', color: 'success' as const };
      case 3:
        return { label: 'Avaliação Rejeitada', color: 'error' as const };
      case 1: 
      default:
        return { label: 'Avaliação em Análise', color: 'warning' as const };
    }
  };

  // Mapeia o status da denúncia vindo do backend para a tag visual
  const getDenunciaStatusChip = (status: string | number) => {
    const statusStr = String(status).toLowerCase();
    switch (statusStr) {
      case 'resolvida':
      case '2':
        return { label: 'Resolvida', color: 'success' as const };
      case 'rejeitada':
      case 'arquivada':
      case '3':
        return { label: 'Arquivada', color: 'error' as const };
      case 'emanalise':
      case '1':
      default:
        return { label: 'Em Análise', color: 'warning' as const };
    }
  };

  // Converte o valor numérico do Enum de denúncia para texto legível
  const getDenunciaCategoriaLabel = (cat: string | number) => {
    switch (Number(cat)) {
      case 1: return 'Conteúdo Enganoso / Propaganda Falsa';
      case 2: return 'Plágio / Direitos Autorais';
      case 3: return 'Qualidade Baixa / Inassistível';
      case 4: return 'Conteúdo Ofensivo / Inadequado';
      case 5: return 'Outros Motivos';
      default: return String(cat); 
    }
  };

  const renderResumoInteligente = (texto: string) => {
    const partes = texto.split(/Pontos fortes:|Pontos de atenção:/i);
    const resumoGeral = partes[0] ? partes[0].replace(/Resumo:/i, '').trim() : texto;
    
    const pontosFortes = partes[1] ? partes[1].split('*').map(s => s.trim()).filter(Boolean) : [];
    const pontosAtencao = partes[2] ? partes[2].split('*').map(s => s.trim()).filter(Boolean) : [];

    return (
      <Box>
        <Typography variant="body2" color="text.primary" sx={{ mb: 2, textAlign: 'justify' }}>
          {resumoGeral}
        </Typography>
        
        {pontosFortes.length > 0 && (
          <Box sx={{ mb: 2 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 'bold', color: '#15803d' }}>
              Pontos Fortes:
            </Typography>
            <Box component="ul" sx={{ pl: 2, m: 0, color: '#475569' }}>
              {pontosFortes.map((pt, i) => (
                <Typography component="li" variant="body2" key={`pf-${i}`}>{pt}</Typography>
              ))}
            </Box>
          </Box>
        )}

        {pontosAtencao.length > 0 && (
          <Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 'bold', color: '#b91c1c' }}>
              Pontos de Atenção:
            </Typography>
            <Box component="ul" sx={{ pl: 2, m: 0, color: '#475569' }}>
              {pontosAtencao.map((pt, i) => (
                <Typography component="li" variant="body2" key={`pa-${i}`}>{pt}</Typography>
              ))}
            </Box>
          </Box>
        )}
      </Box>
    );
  };

  const trustScore = curso.trustScore || 0;

  return (
    <Box sx={{ flexGrow: 1, backgroundColor: '#f9fafb', minHeight: '100vh', pb: 6 }}>
      {/* Cabeçalho */}
      <AppBar position="static" sx={{ mb: 4, backgroundColor: '#1976d2' }}>
        <Toolbar>
          <IconButton edge="start" color="inherit" onClick={() => navigate(-1)} sx={{ mr: 2 }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 'bold' }}>
            Detalhes do Curso
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg">
        <Grid container spacing={4}>
          
          <Grid size={{ xs: 12, md: 8 }}>
            <Paper elevation={2} sx={{ p: 4, mb: 4, borderRadius: 2 }}>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
                <Chip 
                  label={trustScore > 0 ? `Nota: ${trustScore.toFixed(1)}` : 'Sem avaliações'} 
                  color={trustScore === 0 ? "default" : trustScore >= 4 ? "success" : trustScore >= 3 ? "warning" : "error"}
                  sx={{ fontWeight: 'bold' }}
                />
                {curso.statusAuditoria === EStatusAuditoria.Aprovado && (
                  <Chip label="Auditado" color="success" variant="outlined" icon={<VerifiedIcon />} />
                )}
                {curso.statusAuditoria === EStatusAuditoria.Reprovado && (
                  <Chip 
                    label="Reprovado em Auditoria" 
                    color="error" 
                    variant="outlined" 
                    icon={<VerifiedIcon />} 
                  />
                )}
                {curso.statusAuditoria === EStatusAuditoria.NaoAuditado && (
                  <Chip 
                    label="Não Auditado" 
                    color="default" 
                    variant="outlined" 
                  />
                )}
              </Box>

              <Typography variant="h3" sx={{ fontWeight: 'bold', mb: 2, color: '#1e293b' }}>
                {curso.titulo}
              </Typography>

              <Typography variant="subtitle1" color="text.secondary" sx={{ mb: 2, fontStyle: 'italic' }}>
                Por: {nomeProdutor}
              </Typography>
              
              <Typography variant="h6" color="text.secondary" sx={{ mb: 4 }}>
                Plataforma: {curso.plataformaHospedagem || 'Não informada'}
              </Typography>

              <Divider sx={{ mb: 4 }} />

              <Typography variant="h5" sx={{ fontWeight: 'bold', mb: 2 }}>
                Sobre o Curso
              </Typography>
              <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap', color: '#475569', lineHeight: 1.8 }}>
                {curso.descricaoOriginal}
              </Typography>

              {/* Se houver IA ResumoReputacao */}
              {curso.resumoReputacao && (
                <Box sx={{ mt: 4, p: 3, backgroundColor: '#e0f2fe', borderRadius: 2 }}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 'bold', color: '#0369a1', mb: 1 }}>
                    Resumo Inteligente da Reputação (IA)
                  </Typography>
                  {renderResumoInteligente(curso.resumoReputacao)}
                </Box>
              )}
            </Paper>

            {/* Seção de Avaliações */}
            <Typography variant="h5" sx={{ fontWeight: 'bold', mb: 3, mt: 6 }}>
              Avaliações dos Alunos
            </Typography>
            
            {curso.avaliacoes && curso.avaliacoes.length > 0 ? (
              curso.avaliacoes.map((avaliacao: any) => (
                <Card key={avaliacao.id} sx={{ mb: 2, borderRadius: 2 }} elevation={1}>
                  <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', mb: 2, gap: 2 }}>
                      <Avatar sx={{ bgcolor: '#1976d2' }}>
                        {avaliacao.nomeUsuario ? avaliacao.nomeUsuario.charAt(0).toUpperCase() : 'A'}
                      </Avatar>
                      <Box>
                        <Typography variant="subtitle2" sx={{ fontWeight: 'bold' }}>
                          {avaliacao.nomeUsuario ? avaliacao.nomeUsuario.split(' ')[0] : 'Aluno'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {new Date(avaliacao.data).toLocaleDateString('pt-BR')}
                        </Typography>
                      </Box>
                      {avaliacao.statusComprovante && (
                        <Chip 
                          size="small" 
                          label={getStatusComprovanteChip(avaliacao.statusComprovante).label} 
                          color={getStatusComprovanteChip(avaliacao.statusComprovante).color} 
                          variant="outlined" 
                          sx={{ ml: 'auto' }} 
                        />
                      )}
                    </Box>
                    
                    <Box sx={{ display: 'flex', gap: 4, mb: 2 }}>
                      <Box>
                        <Typography variant="caption" color="text.secondary">Conteúdo</Typography>
                        <Rating value={avaliacao.notaEntrega} readOnly size="small" />
                      </Box>
                      <Box>
                        <Typography variant="caption" color="text.secondary">Suporte</Typography>
                        <Rating value={avaliacao.notaSuporte} readOnly size="small" />
                      </Box>
                    </Box>

                    {avaliacao.comentario && (
                      <Typography variant="body2" color="text.primary" sx={{ fontStyle: 'italic' }}>
                        "{avaliacao.comentario}"
                      </Typography>
                    )}
                  </CardContent>
                </Card>
              ))
            ) : (
              <Typography variant="body1" color="text.secondary">
                Este curso ainda não possui avaliações.
              </Typography>
            )}

            {/* NOVA SEÇÃO: Histórico de Denúncias */}
            <Typography variant="h5" sx={{ fontWeight: 'bold', mb: 3, mt: 6, color: '#b91c1c' }}>
              Denúncias Registradas
            </Typography>

            {curso.denuncia && curso.denuncia.length > 0 ? (
              curso.denuncia.map((denuncia: any) => (
                <Card key={denuncia.id} sx={{ mb: 2, borderRadius: 2, borderLeft: '4px solid #b91c1c' }} elevation={1}>
                  <CardContent>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
                      <Chip 
                        size="small" 
                        label={getDenunciaCategoriaLabel(denuncia.categoria)} 
                        sx={{ fontWeight: 'bold', backgroundColor: '#ffeeec', color: '#b91c1c' }} 
                      />
                      <Typography variant="caption" color="text.secondary">
                        {new Date(denuncia.data).toLocaleDateString('pt-BR')}
                      </Typography>
                      {denuncia.status && (
                        <Chip 
                          size="small" 
                          label={getDenunciaStatusChip(denuncia.status).label} 
                          color={getDenunciaStatusChip(denuncia.status).color} 
                          variant="outlined" 
                          sx={{ ml: 'auto' }} 
                        />
                      )}
                    </Box>
                    <Typography variant="body2" color="text.primary" sx={{ mt: 2, pl: 0.5, lineHeight: 1.6 }}>
                      {denuncia.relatoDetalhado}
                    </Typography>
                  </CardContent>
                </Card>
              ))
            ) : (
              <Typography variant="body1" color="text.secondary">
                Nenhuma denúncia registrada para este curso.
              </Typography>
            )}
          </Grid>

          {/* Coluna Lateral: Ações e Promessas */}
          <Grid size={{ xs: 12, md: 4 }}>
            <Paper elevation={3} sx={{ p: 3, borderRadius: 2, position: 'sticky', top: 20 }}>
              <Typography variant="h6" sx={{ fontWeight: 'bold', mb: 3, textAlign: 'center' }}>
                O que você acha deste curso?
              </Typography>

              {isLoggedIn ? (
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <Button 
                    variant="contained" 
                    size="large" 
                    fullWidth 
                    onClick={() => setModalAvaliacaoAberta(true)}
                    sx={{ py: 1.5, fontWeight: 'bold' }}
                  >
                    Avaliar este Curso
                  </Button>
                  
                  <Button 
                    variant="outlined" 
                    color="error"
                    size="medium" 
                    fullWidth 
                    onClick={() => setModalDenunciaAberta(true)}
                    sx={{ fontWeight: 'bold' }}
                  >
                    Denunciar Curso
                  </Button>
                </Box>
              ) : (
                <Button 
                  variant="outlined" 
                  size="large" 
                  fullWidth 
                  onClick={() => navigate('/login')}
                >
                  Faça login para interagir
                </Button>
              )}

              {/* Box de Promessas do Curso (se houver) */}
              {curso.promessaCursos && curso.promessaCursos.length > 0 && (
                <Box sx={{ mt: 5 }}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 'bold', mb: 2 }}>
                    Promessas do Produtor:
                  </Typography>
                  <Box component="ul" sx={{ pl: 2, margin: 0, color: '#475569' }}>
                    {curso.promessaCursos.map((promessa: any) => (
                      <Typography component="li" variant="body2" key={promessa.id} sx={{ mb: 1 }}>
                        {promessa.descricao}
                      </Typography>
                    ))}
                  </Box>
                </Box>
              )}
            </Paper>
          </Grid>

        </Grid>
      </Container>

      {/* Modal de Avaliação */}
      <NovaAvaliacaoDialog 
        open={modalAvaliacaoAberta}
        onClose={() => setModalAvaliacaoAberta(false)}
        cursoId={curso.id}
        onSuccess={carregarDetalhesCurso} 
      />

      {/* Modal de Denúncia */}
      <NovaDenunciaDialog 
        open={modalDenunciaAberta}
        onClose={() => setModalDenunciaAberta(false)}
        cursoId={curso.id}
        onSuccess={carregarDetalhesCurso} 
      />
    </Box>
  );
};

export default CursoDetalhes;