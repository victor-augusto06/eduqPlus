import React, { useState } from 'react';
import { 
  Box, Container, Typography, TextField, Button, Paper, Alert, Link, CircularProgress 
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

const Cadastro = () => {
  const navigate = useNavigate();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confirmarSenha, setConfirmarSenha] = useState('');
  
  const [erro, setErro] = useState('');
  const [loading, setLoading] = useState(false);

  const handleCadastro = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro('');

    if (senha !== confirmarSenha) {
      setErro('As senhas não coincidem.');
      return;
    }

    setLoading(true);
    try {
      await api.post('/Usuario/registrar', { 
        nome,
        email,
        senha,
        role: 'Comum'
      });
      
      try {
        const loginResponse = await api.post('/Usuario/login', { 
          email, 
          senha 
        });
        
        localStorage.setItem('@EduqPlus:token', loginResponse.data.token);
        localStorage.setItem('@EduqPlus:user', JSON.stringify(loginResponse.data.usuario || loginResponse.data));
        
        navigate('/dashboard');
        
      } catch (loginError) {
        navigate('/login');
      }

    } catch (error: any) {
      console.error(error);
      setErro(error.response?.data?.mensagem || 'Erro ao realizar o cadastro. Verifique os dados.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ flexGrow: 1, backgroundColor: '#f9fafb', minHeight: '100vh', display: 'flex', alignItems: 'center' }}>
      <Container maxWidth="xs">
        <Paper elevation={3} sx={{ p: 4, borderRadius: 2 }}>
          <Typography variant="h4" component="h1" align="center" sx={{ fontWeight: 'bold', color: '#1976d2', mb: 1 }}>
            Eduq+
          </Typography>
          <Typography variant="subtitle1" align="center" color="text.secondary" sx={{ mb: 3 }}>
            Crie sua conta para começar a aprender
          </Typography>

          {erro && <Alert severity="error" sx={{ mb: 3 }}>{erro}</Alert>}

          <Box component="form" onSubmit={handleCadastro}>
            <TextField
              fullWidth
              label="Nome Completo"
              variant="outlined"
              margin="normal"
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              required
            />
            
            <TextField
              fullWidth
              type="email"
              label="E-mail"
              variant="outlined"
              margin="normal"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />

            <TextField
              fullWidth
              type="password"
              label="Senha"
              variant="outlined"
              margin="normal"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
            />

            <TextField
              fullWidth
              type="password"
              label="Confirmar Senha"
              variant="outlined"
              margin="normal"
              value={confirmarSenha}
              onChange={(e) => setConfirmarSenha(e.target.value)}
              required
            />

            <Button 
              type="submit" 
              fullWidth 
              variant="contained" 
              size="large" 
              disabled={loading}
              sx={{ mt: 3, mb: 2, py: 1.5, fontWeight: 'bold' }}
            >
              {loading ? <CircularProgress size={24} color="inherit" /> : 'Cadastrar'}
            </Button>

            <Typography variant="body2" align="center">
              Já possui uma conta?{' '}
              <Link 
                component="button" 
                type="button"
                variant="body2" 
                onClick={() => navigate('/login')}
                sx={{ fontWeight: 'bold' }}
              >
                Faça login
              </Link>
            </Typography>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default Cadastro;