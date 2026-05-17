import { useState, useEffect } from 'react';
import { 
  Box, Container, Typography, Button, Paper, CircularProgress, 
  Grid, Card, CardContent, Chip, Divider, AppBar, Toolbar, IconButton, Rating, Avatar,
  Dialog, DialogTitle, DialogContent, DialogActions, TextField, MenuItem
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import VerifiedIcon from '@mui/icons-material/Verified';
import ReceiptIcon from '@mui/icons-material/Receipt';
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
  const [produtorInfo, setProdutorInfo] = useState<{nome: string, nicho?: string, links?: string}>({ nome: 'Carregando...' });

  const [isAdmin, setIsAdmin] = useState(false);
  const [currentUserId, setCurrentUserId] = useState('');

  const [openEditAvaliacao, setOpenEditAvaliacao] = useState(false);
  const [selectedAvaliacaoId, setSelectedAvaliacaoId] = useState('');
  const [notaEntrega, setNotaEntrega] = useState(0);
  const [notaSuporte, setNotaSuporte] = useState(0);
  const [comentarioAvaliacao, setComentarioAvaliacao] = useState('');
  const [statusComprovanteAvaliacao, setStatusComprovanteAvaliacao] = useState<number | string>(1);
  const [urlComprovante, setUrlComprovante] = useState('');

  const [openDeleteAvaliacaoDialog, setOpenDeleteAvaliacaoDialog] = useState(false);
  const [avaliacaoIdParaExcluir, setAvaliacaoIdParaExcluir] = useState('');

  const [openEditDenuncia, setOpenEditDenuncia] = useState(false);
  const [selectedDenunciaId, setSelectedDenunciaId] = useState('');
  const [relatoDetalhado, setRelatoDetalhado] = useState('');
  const [categoriaDenuncia, setCategoriaDenuncia] = useState<number | string>('');
  const [statusDenuncia, setStatusDenuncia] = useState<number | string>(1);

  const [openDeleteDenunciaDialog, setOpenDeleteDenunciaDialog] = useState(false);
  const [denunciaIdParaExcluir, setDenunciaIdParaExcluir] = useState('');

  const carregarDetalhesCurso = async () => {
    setLoading(true);
    try {
      const response = await api.get(`/Curso/${id}`);
      const dadosCurso = response.data;
      setCurso(dadosCurso);

      try {
        const prodResponse = await api.get(`/Produtor/${dadosCurso.produtorId}`);
        setProdutorInfo({
          nome: prodResponse.data.nome,
          nicho: prodResponse.data.nichoPrincipal,
          links: prodResponse.data.linksSociais
        });
      } catch (prodError) {
        console.error("Erro ao carregar produtor", prodError);
        setProdutorInfo({ nome: 'Produtor Desconhecido' });
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

    const userStr = localStorage.getItem('@EduqPlus:user');
    if (userStr) {
      const user = JSON.parse(userStr);
      setIsAdmin(user.role === 2 || user.Role === 2);
      setCurrentUserId(user.id || user.Id || '');
    }
  }, [id]);

  // --- AÇÕES DE AVALIAÇÃO ---
  const handleIniciarEdicaoAvaliacao = (av: any) => {
    setSelectedAvaliacaoId(av.id);
    setNotaEntrega(av.notaEntrega);
    setNotaSuporte(av.notaSuporte);
    setComentarioAvaliacao(av.comentario || '');
    setStatusComprovanteAvaliacao(av.statusComprovante || 1);
    setUrlComprovante(av.comprovanteUrl || av.caminhoComprovante || av.urlComprovante || ''); // Ajuste aqui para a propriedade correta do seu backend
    setOpenEditAvaliacao(true);
  };

  const handleSalvarEdicaoAvaliacao = async () => {
    try {
      const payload: any = {
        notaEntrega,
        notaSuporte,
        comentario: comentarioAvaliacao
      };

      if (isAdmin) {
        payload.statusComprovante = Number(statusComprovanteAvaliacao);
      }

      await api.put(`/Avaliacao/${selectedAvaliacaoId}`, payload);
      setOpenEditAvaliacao(false);
      carregarDetalhesCurso();
    } catch (err) {
      console.error('Erro ao editar avaliação.', err);
    }
  };

  const handleIniciarExcluirAvaliacao = (avaliacaoId: string) => {
    setAvaliacaoIdParaExcluir(avaliacaoId);
    setOpenDeleteAvaliacaoDialog(true);
  };

  const handleConfirmarExcluirAvaliacao = async () => {
    try {
      await api.delete(`/Avaliacao/${avaliacaoIdParaExcluir}`);
      setOpenDeleteAvaliacaoDialog(false);
      setAvaliacaoIdParaExcluir('');
      carregarDetalhesCurso();
    } catch (err) {
      console.error('Erro ao excluir avaliação.', err);
    }
  };

  // --- AÇÕES DE DENÚNCIA ---
  const handleIniciarEdicaoDenuncia = (d: any) => {
    setSelectedDenunciaId(d.id);
    setCategoriaDenuncia(Number(d.categoria));
    setRelatoDetalhado(d.relatoDetalhado);
    
    // Converte status de texto (se vier) para o número equivalente para o Select
    let statusNum = 1;
    const s = String(d.status).toLowerCase();
    if (s === 'resolvida' || s === '2') statusNum = 2;
    else if (s === 'arquivada' || s === 'rejeitada' || s === '3') statusNum = 3;
    setStatusDenuncia(statusNum);

    setOpenEditDenuncia(true);
  };

  const handleSalvarEdicaoDenuncia = async () => {
    try {
      const payload: any = {
        categoria: Number(categoriaDenuncia),
        relatoDetalhado: relatoDetalhado
      };

      if (isAdmin) {
        payload.status = Number(statusDenuncia);
      }

      await api.put(`/Denuncia/${selectedDenunciaId}`, payload);
      setOpenEditDenuncia(false);
      carregarDetalhesCurso();
    } catch (err) {
      console.error('Erro ao editar denúncia.', err);
    }
  };

  const handleIniciarExcluirDenuncia = (denunciaId: string) => {
    setDenunciaIdParaExcluir(denunciaId);
    setOpenDeleteDenunciaDialog(true);
  };

  const handleConfirmarExcluirDenuncia = async () => {
    try {
      await api.delete(`/Denuncia/${denunciaIdParaExcluir}`);
      setOpenDeleteDenunciaDialog(false);
      setDenunciaIdParaExcluir('');
      carregarDetalhesCurso();
    } catch (err) {
      console.error('Erro ao excluir denúncia.', err);
    }
  };

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

              <Box sx={{ mb: 2 }}>
                <Typography variant="subtitle1" color="text.secondary" sx={{ fontStyle: 'italic' }}>
                  Por: <strong>{produtorInfo.nome}</strong> 
                  {produtorInfo.nicho && ` | Nicho: ${produtorInfo.nicho}`}
                </Typography>
                
                {produtorInfo.links && (
                  <Box sx={{ mt: 1, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    {produtorInfo.links.split(';').map((link, index) => {
                      const url = link.trim();
                      if (!url) return null;
                      return (
                        <Chip 
                          key={index}
                          label={url.replace(/^https?:\/\//, '').replace(/\/$/, '')}
                          component="a"
                          href={url.startsWith('http') ? url : `https://${url}`}
                          target="_blank"
                          clickable
                          size="small"
                          color="primary"
                          variant="outlined"
                        />
                      );
                    })}
                  </Box>
                )}
              </Box>
              
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
                        <Rating value={avaliacao.notaEntrega} readOnly size="small" precision={0.5} />
                      </Box>
                      <Box>
                        <Typography variant="caption" color="text.secondary">Suporte</Typography>
                        <Rating value={avaliacao.notaSuporte} readOnly size="small" precision={0.5} />
                      </Box>
                    </Box>

                    {avaliacao.comentario && (
                      <Typography variant="body2" color="text.primary" sx={{ fontStyle: 'italic' }}>
                        "{avaliacao.comentario}"
                      </Typography>
                    )}

                    {/* Botões de Ação Dinâmicos */}
                    {(avaliacao.usuarioId === currentUserId || isAdmin) && (
                      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mt: 2 }}>
                        <Button size="small" variant="outlined" onClick={() => handleIniciarEdicaoAvaliacao(avaliacao)}>
                          Editar
                        </Button>
                        <Button size="small" variant="outlined" color="error" onClick={() => handleIniciarExcluirAvaliacao(avaliacao.id)}>
                          Excluir
                        </Button>
                      </Box>
                    )}
                  </CardContent>
                </Card>
              ))
            ) : (
              <Typography variant="body1" color="text.secondary">
                Este curso ainda não possui avaliações.
              </Typography>
            )}

            {/* Histórico de Denúncias */}
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

                    {/* Botões de Ação Dinâmicos */}
                    {(denuncia.usuarioId === currentUserId || isAdmin) && (
                      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mt: 2 }}>
                        <Button size="small" variant="outlined" onClick={() => handleIniciarEdicaoDenuncia(denuncia)}>
                          Editar
                        </Button>
                        <Button size="small" variant="outlined" color="error" onClick={() => handleIniciarExcluirDenuncia(denuncia.id)}>
                          Excluir
                        </Button>
                      </Box>
                    )}
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

      {/* DIALOG EDITAR AVALIAÇÃO */}
      <Dialog open={openEditAvaliacao} onClose={() => setOpenEditAvaliacao(false)} fullWidth maxWidth="sm">
        <DialogTitle sx={{ fontWeight: 'bold' }}>Editar Avaliação</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 2 }}>
            {isAdmin && (
              <Box sx={{ mb: 3, p: 2, backgroundColor: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: 2 }}>
                <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 2, color: '#0f172a' }}>
                  Área do Administrador
                </Typography>
                
                {urlComprovante ? (
                  <Button 
                    variant="contained" 
                    color="info" 
                    startIcon={<ReceiptIcon />}
                    onClick={() => {
                      const backendUrl = api.defaults.baseURL?.replace('/api', '') || 'http://localhost:5000';
                      const urlCompleta = urlComprovante.startsWith('http') ? urlComprovante : `${backendUrl}${urlComprovante}`;
                      
                      window.open(urlCompleta, '_blank');
                    }}
                    sx={{ mb: 3, width: '100%' }}
                  >
                    Visualizar Comprovante Anexado
                  </Button>
                ) : (
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2, fontStyle: 'italic' }}>
                    Nenhum comprovante anexado a esta avaliação.
                  </Typography>
                )}

                <TextField
                  select fullWidth label="Status do Comprovante" variant="outlined" size="small"
                  value={statusComprovanteAvaliacao} onChange={(e) => setStatusComprovanteAvaliacao(e.target.value)}
                >
                  <MenuItem value={1}>Em Análise</MenuItem>
                  <MenuItem value={2}>Comprovada</MenuItem>
                  <MenuItem value={3}>Rejeitada</MenuItem>
                </TextField>
              </Box>
            )}

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
              <Typography variant="body2" sx={{ minWidth: 180 }}>
                Nota de Entrega/Conteúdo
              </Typography>
              <Rating 
                value={notaEntrega} 
                precision={0.5}
                onChange={(_, newValue) => setNotaEntrega(newValue || 0)} 
              />
              <TextField
                type="number"
                size="small"
                value={notaEntrega}
                onChange={(e) => setNotaEntrega(Number(e.target.value))}
                slotProps={{ htmlInput: { min: 0, max: 5, step: 0.5 } }}
                sx={{ width: 80 }}
              />
            </Box>
            
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
              <Typography variant="body2" sx={{ minWidth: 180 }}>
                Nota de Suporte
              </Typography>
              <Rating 
                value={notaSuporte} 
                precision={0.5}
                onChange={(_, newValue) => setNotaSuporte(newValue || 0)} 
              />
              <TextField
                type="number"
                size="small"
                value={notaSuporte}
                onChange={(e) => setNotaSuporte(Number(e.target.value))}
                slotProps={{ htmlInput: { min: 0, max: 5, step: 0.5 } }}
                sx={{ width: 80 }}
              />
            </Box>

            <TextField
              fullWidth label="Comentário" multiline rows={3} margin="normal" variant="outlined"
              value={comentarioAvaliacao} onChange={(e) => setComentarioAvaliacao(e.target.value)}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenEditAvaliacao(false)}>Cancelar</Button>
          <Button variant="contained" color="primary" onClick={handleSalvarEdicaoAvaliacao}>
            Salvar Alterações
          </Button>
        </DialogActions>
      </Dialog>

      {/* DIALOG EDITAR DENÚNCIA */}
      <Dialog open={openEditDenuncia} onClose={() => setOpenEditDenuncia(false)} fullWidth maxWidth="sm">
        <DialogTitle sx={{ fontWeight: 'bold' }}>Editar Denúncia</DialogTitle>
        <DialogContent>
          <Box sx={{ mt: 1 }}>
            {isAdmin && (
              <TextField
                select fullWidth label="Status da Denúncia (Admin)" margin="normal" variant="outlined"
                value={statusDenuncia} onChange={(e) => setStatusDenuncia(e.target.value)}
                sx={{ mb: 2 }}
              >
                <MenuItem value={1}>Em Análise</MenuItem>
                <MenuItem value={2}>Resolvida</MenuItem>
                <MenuItem value={3}>Arquivada</MenuItem>
              </TextField>
            )}

            <TextField
              select fullWidth label="Categoria/Motivo" margin="normal" variant="outlined"
              value={categoriaDenuncia} onChange={(e) => setCategoriaDenuncia(e.target.value)}
            >
              <MenuItem value={1}>Conteúdo Enganoso / Propaganda Falsa</MenuItem>
              <MenuItem value={2}>Plágio / Direitos Autorais</MenuItem>
              <MenuItem value={3}>Qualidade Baixa / Inassistível</MenuItem>
              <MenuItem value={4}>Conteúdo Ofensivo / Inadequado</MenuItem>
              <MenuItem value={5}>Outros Motivos</MenuItem>
            </TextField>
            <TextField
              fullWidth label="Relato Detalhado" multiline rows={4} margin="normal" variant="outlined"
              value={relatoDetalhado} onChange={(e) => setRelatoDetalhado(e.target.value)}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenEditDenuncia(false)}>Cancelar</Button>
          <Button variant="contained" color="primary" onClick={handleSalvarEdicaoDenuncia}>
            Salvar Alterações
          </Button>
        </DialogActions>
      </Dialog>

      {/* DIALOG CONFIRMAR EXCLUSÃO DE AVALIAÇÃO */}
      <Dialog 
        open={openDeleteAvaliacaoDialog} 
        onClose={() => setOpenDeleteAvaliacaoDialog(false)}
        fullWidth 
        maxWidth="xs"
      >
        <DialogTitle sx={{ fontWeight: 'bold', color: '#b91c1c' }}>
          Confirmar Exclusão
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1">
            Tem certeza que deseja remover esta avaliação? Esta ação não poderá ser desfeita.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenDeleteAvaliacaoDialog(false)}>
            Cancelar
          </Button>
          <Button 
            variant="contained" 
            color="error" 
            onClick={handleConfirmarExcluirAvaliacao}
          >
            Excluir
          </Button>
        </DialogActions>
      </Dialog>

      {/* DIALOG CONFIRMAR EXCLUSÃO DE DENÚNCIA */}
      <Dialog 
        open={openDeleteDenunciaDialog} 
        onClose={() => setOpenDeleteDenunciaDialog(false)}
        fullWidth 
        maxWidth="xs"
      >
        <DialogTitle sx={{ fontWeight: 'bold', color: '#b91c1c' }}>
          Confirmar Exclusão
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1">
            Tem certeza que deseja remover esta denúncia? Esta ação não poderá ser desfeita.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenDeleteDenunciaDialog(false)}>
            Cancelar
          </Button>
          <Button 
            variant="contained" 
            color="error" 
            onClick={handleConfirmarExcluirDenuncia}
          >
            Excluir
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CursoDetalhes;