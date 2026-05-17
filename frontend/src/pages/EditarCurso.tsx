import { EStatusAuditoria } from '../types/Curso';
import React, { useState, useEffect } from 'react';
import { 
  Box, Container, Typography, TextField, Button, Paper, MenuItem, 
  CircularProgress, Alert, Grid, Divider
} from '@mui/material';
import { useNavigate, useParams } from 'react-router-dom';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import api from '../services/api';

interface Categoria {
  id: string;
  nome: string;
}

const EditarCurso = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [isAdmin, setIsAdmin] = useState(false);

  const [titulo, setTitulo] = useState('');
  const [descricaoOriginal, setDescricaoOriginal] = useState('');
  const [plataformaHospedagem, setPlataformaHospedagem] = useState('');
  const [categoriaId, setCategoriaId] = useState('');
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [promessas, setPromessas] = useState<string[]>([]);

  const [statusAuditoria, setStatusAuditoria] = useState<number | string>('');
  const [trustScore, setTrustScore] = useState(0);
  const [resumoReputacao, setResumoReputacao] = useState('');

  useEffect(() => {
    const carregarDados = async () => {
      setLoading(true);
      try {
        const userStr = localStorage.getItem('@EduqPlus:user');
        if (userStr) {
          const user = JSON.parse(userStr);
          setIsAdmin(user.role === 2 || user.Role === 2);
        }

        const [catRes, cursoRes] = await Promise.all([
          api.get('/Categoria'),
          api.get(`/Curso/${id}`)
        ]);

        const curso = cursoRes.data;
        setCategorias(catRes.data);

        setTitulo(curso.titulo);
        setDescricaoOriginal(curso.descricaoOriginal);
        setPlataformaHospedagem(curso.plataformaHospedagem);
        setCategoriaId(curso.categoriaId);
        setPromessas(curso.promessaCursos?.map((p: any) => p.descricao) || ['']);
        
        setStatusAuditoria(curso.statusAuditoria);
        setTrustScore(curso.trustScore);
        setResumoReputacao(curso.resumoReputacao || '');

      } catch (err) {
        setError('Erro ao carregar dados do curso para edição.');
      } finally {
        setLoading(false);
      }
    };

    carregarDados();
  }, [id]);

  const handlePromessaChange = (index: number, value: string) => {
    const novas = [...promessas];
    novas[index] = value;
    setPromessas(novas);
  };

  const handleAdicionarPromessa = () => setPromessas([...promessas, '']);
  
  const handleRemoverPromessa = (index: number) => {
    setPromessas(promessas.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');

    try {
      const payload: any = {
        titulo,
        descricaoOriginal,
        plataformaHospedagem,
        categoriaId,
        promessaCursos: promessas.filter(p => p.trim() !== '').map(p => ({ descricao: p }))
      };

      if (isAdmin) {
        payload.statusAuditoria = statusAuditoria;
        payload.trustScore = trustScore;
        payload.resumoReputacao = resumoReputacao;
        
        await api.put(`/Curso/${id}/admin`, payload);
      } else {
        await api.put(`/Curso/${id}`, payload);
      }

      navigate('/dashboard');
    } catch (err: any) {
      setError(err.response?.data?.mensagem || 'Erro ao salvar alterações.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1, backgroundColor: '#f0f2f5', minHeight: '100vh', py: 5 }}>
      <Container maxWidth="md">
        <Button 
          startIcon={<ArrowBackIcon />} 
          onClick={() => navigate(-1)} 
          sx={{ mb: 2 }}
        >
          Voltar
        </Button>

        <Paper elevation={3} sx={{ p: 4, borderRadius: 2 }}>
          <Typography variant="h5" sx={{ fontWeight: 'bold', mb: 3 }}>
            Editar Curso: {titulo}
          </Typography>

          {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

          <Box component="form" onSubmit={handleSubmit}>
            <Grid container spacing={3}>
              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth label="Título do Curso"
                  value={titulo} onChange={(e) => setTitulo(e.target.value)} required
                />
              </Grid>

              <Grid size={{ xs: 12 }}>
                <TextField
                  fullWidth label="Descrição Original" multiline rows={4}
                  value={descricaoOriginal} onChange={(e) => setDescricaoOriginal(e.target.value)} required
                />
              </Grid>

              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  fullWidth label="Plataforma"
                  value={plataformaHospedagem} onChange={(e) => setPlataformaHospedagem(e.target.value)} required
                />
              </Grid>

              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField
                  select fullWidth label="Categoria"
                  value={categoriaId} onChange={(e) => setCategoriaId(e.target.value)} required
                >
                  {categorias.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>{cat.nome}</MenuItem>
                  ))}
                </TextField>
              </Grid>

              {/* SEÇÃO DE ADMIN */}
              {isAdmin && (
                <Grid size={{ xs: 12 }}>
                  <Box sx={{ mt: 2, p: 2, border: '1px solid #e0e0e0', borderRadius: 2, backgroundColor: '#fafafa' }}>
                    <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 2, color: '#1976d2' }}>
                      Painel de Controle Administrativo
                    </Typography>
                    <Grid container spacing={2}>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                          select fullWidth label="Status da Auditoria" size="small"
                          value={statusAuditoria}
                          onChange={(e) => setStatusAuditoria(Number(e.target.value))}
                        >
                          <MenuItem value={1}>Não Auditado</MenuItem>
                          <MenuItem value={2}>Aprovado</MenuItem>
                          <MenuItem value={3}>Reprovado</MenuItem>
                        </TextField>
                      </Grid>
                      <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                          fullWidth label="Trust Score" type="number" size="small"
                          slotProps={{ htmlInput: { step: 0.1, min: 0, max: 5 } }}
                          value={trustScore} onChange={(e) => setTrustScore(Number(e.target.value))}
                        />
                      </Grid>
                      <Grid size={{ xs: 12 }}>
                         <TextField
                          fullWidth label="Resumo da Reputação (IA)" multiline rows={2} size="small"
                          value={resumoReputacao} onChange={(e) => setResumoReputacao(e.target.value)}
                        />
                      </Grid>
                    </Grid>
                  </Box>
                </Grid>
              )}

              <Grid size={{ xs: 12 }}>
                <Divider sx={{ my: 2 }} />
                <Typography variant="subtitle1" sx={{ fontWeight: 'bold', mb: 1 }}>
                  Promessas do Curso
                </Typography>
                {promessas.map((p, index) => (
                  <Box key={index} sx={{ display: 'flex', gap: 2, mb: 1.5 }}>
                    <TextField
                      fullWidth size="small" label={`Promessa #${index + 1}`}
                      value={p} onChange={(e) => handlePromessaChange(index, e.target.value)}
                    />
                    <Button color="error" onClick={() => handleRemoverPromessa(index)}>Remover</Button>
                  </Box>
                ))}
                <Button variant="outlined" size="small" onClick={handleAdicionarPromessa}>
                  + Adicionar Promessa
                </Button>
              </Grid>

              <Grid size={{ xs: 12 }} sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2, mt: 3 }}>
                <Button onClick={() => navigate('/dashboard')}>Cancelar</Button>
                <Button 
                  type="submit" variant="contained" color="primary" 
                  disabled={submitting} size="large"
                >
                  {submitting ? 'Salvando...' : 'Salvar Alterações'}
                </Button>
              </Grid>
            </Grid>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default EditarCurso;