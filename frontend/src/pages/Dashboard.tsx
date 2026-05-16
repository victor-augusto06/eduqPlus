import React, { useState, useEffect } from 'react';
import { 
  Box, Container, Typography, TextField, Button, Paper, CircularProgress, 
  Grid, Card, CardContent, CardActions, Chip, AppBar, Toolbar, InputAdornment, IconButton, Pagination,
  Select, MenuItem, FormControl, InputLabel
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import api from '../services/api';
import { type Curso, EStatusAuditoria } from '../types/Curso';


interface Categoria {
  id: string;
  nome: string;
}

interface Produtor {
  id: string;
  nome: string;
}

const Dashboard = () => {
  const navigate = useNavigate();
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  const [cursos, setCursos] = useState<Curso[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [produtores, setProdutores] = useState<Produtor[]>([]);

  const [loading, setLoading] = useState(true);
  
  const [busca, setBusca] = useState('');
  const [isSearching, setIsSearching] = useState(false);
  
  const [filtroCategoria, setFiltroCategoria] = useState('');
  const [filtroProdutor, setFiltroProdutor] = useState('');
  const [filtroStatus, setFiltroStatus] = useState<string>('');

  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const carregarDependencias = async () => {
    try {
      const [catRes, prodRes] = await Promise.all([
        api.get('/Categoria'),
        api.get('/Produtor')
      ]);
      setCategorias(Array.isArray(catRes.data) ? catRes.data : []);
      setProdutores(Array.isArray(prodRes.data) ? prodRes.data : []);
    } catch (error) {
      console.error("Erro ao carregar dependências", error);
    }
  };

  useEffect(() => {
    carregarDependencias();
  }, []);

  const getNomeProdutor = (id: string) => {
    const prod = produtores.find(p => p.id === id);
    return prod ? prod.nome : 'Produtor Desconhecido';
  };

  const carregarCursosBase = async (paginaAtual: number) => {
    setLoading(true);
    try {
      const response = await api.get(`/Curso?pageNumber=${paginaAtual}&pageSize=10`);
      
      const data = response.data;
      if (Array.isArray(data)) {
        setCursos(data);
        setTotalPages(1);
      } else if (data.itens) {
        setCursos(data.itens);
        setTotalPages(data.totalPaginas || 1); 
      }
    } catch (error) {
      console.error("Erro ao carregar cursos", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const token = localStorage.getItem('@EduqPlus:token');
    if (token) {
      setIsLoggedIn(true);
    }
  }, []);

  useEffect(() => {
    if (!isSearching) {
      carregarCursosBase(page);
    }
  }, [page, isSearching]);

  const handleSearch = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    
    if (!busca.trim()) {
      setIsSearching(false);
      setPage(1);
      return;
    }

    setLoading(true);
    setIsSearching(true);
    try {
      const response = await api.get(`/Curso/buscar?query=${encodeURIComponent(busca)}`);
      const data = response.data;
      setCursos(Array.isArray(data) ? data : data.itens || []);
    } catch (error) {
      console.error("Erro na busca inteligente", error);
    } finally {
      setLoading(false);
    }
  };

  const limparBusca = () => {
    setBusca('');
    setIsSearching(false);
    setPage(1);
  };

  const aplicarFiltro = async (tipoFiltro: 'categoria' | 'produtor' | 'status', valorId: string | number) => {
    setLoading(true);
    setIsSearching(true); // Tratamos filtros como uma busca ativa para parar a paginação normal
    try {
      let response;
      if (tipoFiltro === 'categoria') {
        response = await api.get(`/Curso/categoria/${valorId}`);
      } else if (tipoFiltro === 'produtor') {
        response = await api.get(`/Curso/produtor/${valorId}`);
      } else if (tipoFiltro === 'status') {
        response = await api.get(`/Curso/status/${valorId}`);
      }
      
      const data = response?.data;
      setCursos(Array.isArray(data) ? data : data?.itens || []);
      
    } catch (error) {
      console.error(`Erro ao filtrar por ${tipoFiltro}`, error);
      setCursos([]); // Limpa se der erro (ex: 404 Nenhum curso encontrado)
    } finally {
      setLoading(false);
    }
  };

  const limparFiltros = () => {
    setFiltroCategoria('');
    setFiltroProdutor('');
    setFiltroStatus('');
    limparBusca();
  };


  const getNomeCategoria = (id: string) => {
    const cat = categorias.find(c => c.id === id);
    return cat ? cat.nome : 'Outros';
  };


  const cursosAgrupados = cursos.reduce((acc, curso) => {
    const catNome = getNomeCategoria(curso.categoriaId);
    if (!acc[catNome]) {
      acc[catNome] = [];
    }
    acc[catNome].push(curso);
    return acc;
  }, {} as Record<string, Curso[]>);


  const renderCardCurso = (curso: Curso) => {
    const trustScore = curso.trustScore || 0;

    return (
      <Card elevation={2} sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <CardContent sx={{ flexGrow: 1 }}>
          <Typography variant="h6" sx={{ fontWeight: 'bold', mb: 0.5 }}>
            {curso.titulo}
          </Typography>

          <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic', mb: 1.5 }}>
            Por: {getNomeProdutor(curso.produtorId)}
          </Typography>

          {/* Descrição do curso */}
          <Typography variant="body2" color="text.primary" sx={{ mb: 2, display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
            {curso.descricaoOriginal}
          </Typography>
          
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 'auto' }}>
            <Chip 
              label={trustScore > 0 ? `Nota: ${trustScore.toFixed(1)}` : 'Curso ainda não possui avaliações'} 
              color={trustScore === 0 ? "default" : trustScore >= 4 ? "success" : trustScore >= 3 ? "warning" : "error"}
              size="small" 
              sx={{ fontWeight: 'bold' }}
            />

            {/* Tag de Auditoria */}
            {curso.statusAuditoria === EStatusAuditoria.Aprovado && (
              <Chip label="Auditado" color="success" size="small" variant="outlined" />
            )}
            {curso.statusAuditoria === EStatusAuditoria.Reprovado && (
              <Chip label="Reprovado na Auditoria" color="error" size="small" variant="outlined" />
            )}

            {/* Tag de Denúncia */}
            {curso.denuncia && curso.denuncia.length > 0 && (
              <Chip label={`${curso.denuncia.length} Denúncia(s)`} color="warning" size="small" variant="outlined" />
            )}
          </Box>
        </CardContent>
        <CardActions>
          <Button size="small" onClick={() => navigate(`/cursos/${curso.id}`)}>
            Ver Detalhes
          </Button>
        </CardActions>
      </Card>
    );
  };

    const handleAuthAction = () => {
    if (isLoggedIn) {
      localStorage.removeItem('@EduqPlus:token');
      localStorage.removeItem('@EduqPlus:user');
      setIsLoggedIn(false);
      navigate('/login');
    } else {
      navigate('/login');
    }
  };

  return (
    <Box sx={{flexGrow: 1, backgroundColor: '#f9fafb', minHeight: '100vh', pb: 4 }}>
      <AppBar position="static" sx={{ mb: 4, backgroundColor: '#1976d2' }}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 'bold' }}>
            Eduq+
          </Typography>
          
          {isLoggedIn && (
            <Button 
              color="inherit" 
              onClick={() => navigate('/curso/novo')} 
              sx={{ mr: 2, fontWeight: 'bold' }}
            >
              Novo Curso
            </Button>
          )}

          <Button color="inherit" onClick={handleAuthAction}>
            {isLoggedIn ? 'Sair' : 'Entrar'}
          </Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg">
        
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#1e293b' }}>
            Catálogo de Cursos
          </Typography>
          <Typography variant="subtitle1" color="text.secondary">
            Explore, pesquise e avalie os melhores conteúdos da plataforma Eduq+
          </Typography>
        </Box>

        <Paper elevation={1} sx={{ p: 2, mb: 1, display: 'flex', gap: 2 }}>
          <TextField
            fullWidth
            variant="outlined"
            placeholder="O que você quer aprender? (ex: cursos de investimentos bem avaliados)"
            value={busca}
            onChange={(e) => setBusca(e.target.value)}
            onKeyPress={(e) => {
              if (e.key === 'Enter') {
                handleSearch();
              }
            }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon color="action" />
                  </InputAdornment>
                ),
                endAdornment: busca ? (
                  <InputAdornment position="end">
                    <IconButton onClick={limparBusca} edge="end">
                      <ClearIcon />
                    </IconButton>
                  </InputAdornment>
                ) : null,
              },
            }}
          />
          <Button variant="contained" onClick={() => handleSearch()} disabled={loading} sx={{ minWidth: 120 }}>
            {loading ? <CircularProgress size={24} color="inherit" /> : 'Pesquisar'}
          </Button>
        </Paper>
        
        <Paper elevation={0} sx={{ p: 2, mb: 2, display: 'flex', gap: 2, flexWrap: 'wrap', backgroundColor: 'transparent' }}>
          
          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel>Categoria</InputLabel>
            <Select
              value={filtroCategoria}
              label="Categoria"
              onChange={(e) => {
                setFiltroCategoria(e.target.value);
                setFiltroProdutor(''); setFiltroStatus(''); // Limpa os outros
                aplicarFiltro('categoria', e.target.value);
              }}
            >
              {categorias.map((cat) => (
                <MenuItem key={cat.id} value={cat.id}>{cat.nome}</MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel>Produtor</InputLabel>
            <Select
              value={filtroProdutor}
              label="Produtor"
              onChange={(e) => {
                setFiltroProdutor(e.target.value);
                setFiltroCategoria(''); setFiltroStatus(''); 
                aplicarFiltro('produtor', e.target.value);
              }}
            >
              {produtores.map((prod) => (
                <MenuItem key={prod.id} value={prod.id}>{prod.nome}</MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel>Status Auditoria</InputLabel>
            <Select
              value={filtroStatus}
              label="Status Auditoria"
              onChange={(e) => {
                const value = e.target.value;
                setFiltroStatus(value);
                setFiltroCategoria(''); setFiltroProdutor(''); 
                aplicarFiltro('status', value);
              }}
            >
              <MenuItem value={EStatusAuditoria.NaoAuditado}>Não Auditado</MenuItem>
              <MenuItem value={EStatusAuditoria.Aprovado}>Aprovado</MenuItem>
              <MenuItem value={EStatusAuditoria.Reprovado}>Reprovado</MenuItem>
            </Select>
          </FormControl>

          {(filtroCategoria || filtroProdutor || filtroStatus !== '') && (
             <Button variant="text" onClick={limparFiltros} sx={{ mt: 'auto', mb: 'auto' }}>
               Limpar Filtros
             </Button>
          )}

        </Paper>

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 10 }}>
            <CircularProgress />
          </Box>
        ) : cursos.length === 0 ? (
          <Typography variant="h6" align="center" color="text.secondary" sx={{ mt: 5 }}>
            Nenhum curso encontrado.
          </Typography>
        ) : (
          <>
            {/* Renderização Condicional: Agrupado vs Lista Linear */}
            {!isSearching ? (
              Object.keys(cursosAgrupados).map(categoriaNome => (
                <Box key={categoriaNome} sx={{ mb: 5 }}>
                  <Typography variant="h5" sx={{ fontWeight: 'bold', mb: 2, color: '#333', borderBottom: '2px solid #e0e0e0', pb: 1 }}>
                    {categoriaNome}
                  </Typography>
                  <Grid container spacing={3}>
                    {cursosAgrupados[categoriaNome].map(curso => (
                      <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={curso.id}>
                        {renderCardCurso(curso)}
                      </Grid>
                    ))}
                  </Grid>
                </Box>
              ))
            ) : (
              <Box>
                <Typography variant="h5" sx={{ mb: 3, fontWeight: 'bold', color: '#1976d2' }}>
                  Resultados da Busca
                </Typography>
                <Grid container spacing={3}>
                  {cursos.map(curso => (
                    <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={curso.id}>
                      {renderCardCurso(curso)}
                    </Grid>
                  ))}
                </Grid>
              </Box>
            )}

            {/* Paginação (Visível apenas no modo normal) */}
            {!isSearching && totalPages > 1 && (
              <Box sx={{ display: 'flex', justifyContent: 'center', mt: 6 }}>
                <Pagination 
                  count={totalPages} 
                  page={page} 
                  onChange={(_, value) => setPage(value)} 
                  color="primary" 
                  size="large"
                />
              </Box>
            )}
          </>
        )}
      </Container>
    </Box>
  );
};

export default Dashboard;