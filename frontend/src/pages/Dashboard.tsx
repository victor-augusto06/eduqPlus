import React, { useState, useEffect } from 'react';
import { 
  Box, Container, Typography, TextField, Button, Grid as Grid, 
  Card, CardContent, CardActions, AppBar, Toolbar, Chip, CircularProgress 
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

interface Curso {
  id: string;
  titulo: string;
  plataformaHospedagem: string;
  trustScore: number;
  resumoReputacao: string;
}

const Dashboard = () => {
  const [cursos, setCursos] = useState<Curso[]>([]);
  const [busca, setBusca] = useState('');
  const [loading, setLoading] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('@EduqPlus:token');
    setIsLoggedIn(!!token);
    carregarCursos();
  }, []);

  const carregarCursos = async () => {
    setLoading(true);
    try {
      const endpoint = busca.trim() 
        ? `/Curso/buscar?query=${encodeURIComponent(busca)}` 
        : `/Curso`; 
        
      const response = await api.get(endpoint);
      setCursos(Array.isArray(response.data) ? response.data : []); 
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
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
    <Box sx={{ flexGrow: 1, backgroundColor: '#f0f2f5', minHeight: '100vh', pb: 5 }}>
      <AppBar position="static" sx={{ mb: 4, backgroundColor: '#1976d2' }}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 'bold' }}>
            Eduq+ Dashboard
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
        <Box sx={{ display: 'flex', gap: 2, mb: 4, backgroundColor: '#fff', p: 2, borderRadius: 2, boxShadow: 1 }}>
          <TextField
            fullWidth
            label="Busca Semântica (ex: 'Me traga um curso de investimentos bom')"
            variant="outlined"
            value={busca}
            onChange={(e) => setBusca(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && carregarCursos()}
          />
          <Button 
            variant="contained" 
            color="primary" 
            onClick={carregarCursos}
            sx={{ px: 4, fontWeight: 'bold' }}
            disabled={loading}
          >
            Pesquisar
          </Button>
        </Box>

        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', my: 4 }}>
            <CircularProgress />
          </Box>
        )}

        <Grid container spacing={3}>
          {!loading && cursos.length === 0 && (
            <Typography variant="body1" sx={{ mt: 4, width: '100%', textAlign: 'center', color: '#666' }}>
              Nenhum curso encontrado.
            </Typography>
          )}

          {cursos.map((curso) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={curso.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', boxShadow: 3 }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Typography variant="h6" gutterBottom sx={{ fontWeight: 'bold' }}>
                    {curso.titulo}
                  </Typography>
                  <Chip 
                    label={`Score: ${curso.trustScore}`} 
                    color={curso.trustScore >= 3 ? "success" : "error"} 
                    size="small" 
                    sx={{ mb: 2, fontWeight: 'bold' }} 
                  />
                  <Typography variant="body2" color="text.secondary">
                    <Box component="span" sx={{ fontWeight: 'bold' }}>Plataforma:</Box> {curso.plataformaHospedagem || 'Não informada'}
                  </Typography>
                </CardContent>
                <CardActions>
                  <Button 
                    size="small" 
                    variant="outlined" 
                    fullWidth 
                    onClick={() => navigate(`/curso/${curso.id}`)}
                  >
                    Ver Detalhes e Avaliações
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  );
};

export default Dashboard;