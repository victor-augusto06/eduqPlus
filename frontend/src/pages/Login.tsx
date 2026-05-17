import React, { useState } from 'react';
import { Box, Button, TextField, Typography, Container, Paper, Alert, Link } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

const Login = () => {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      const response = await api.post('/Usuario/login', { email, senha });
      
      localStorage.setItem('@EduqPlus:token', response.data.token);
      localStorage.setItem('@EduqPlus:user', JSON.stringify(response.data.usuario));

      navigate('/dashboard');
    } catch (err: any) {
      setError('Credenciais inválidas ou erro no servidor.');
    }
  };

  return (
    <Box 
      sx={{ 
        height: '100vh', 
        display: 'flex', 
        alignItems: 'center', 
        backgroundColor: '#f5f5f5' 
      }}
    >
      <Container maxWidth="xs">
        <Paper elevation={3} sx={{ p: 4, borderRadius: 2 }}>
          <Typography variant="h4" component="h1" gutterBottom align="center" sx={{ fontWeight: 'bold', color: '#1976d2' }}>
            Eduq+
          </Typography>
          <Typography variant="body2" color="textSecondary" align="center" sx={{ mb: 3 }}>
            Gerenciamento Inteligente de Cursos
          </Typography>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <Box component="form" onSubmit={handleLogin}>
            <TextField
              fullWidth
              label="E-mail"
              variant="outlined"
              margin="normal"
              value={email}
              onChange={(e) => setEmail(e.target.value)} 
              required
            />         
            <TextField
              fullWidth
              label="Senha"
              type="password"
              variant="outlined"
              margin="normal"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
            />
            <Button
              fullWidth
              type="submit"
              variant="contained"
              size="large"
              sx={{ mt: 3, mb: 2, height: 50, fontWeight: 'bold' }}
            >
              Entrar
            </Button>
            <Typography variant="body2" align="center" sx={{ mt: 3 }}>
              Não tem uma conta?{' '}
              <Link 
                component="button" 
                type="button"
                variant="body2" 
                onClick={() => navigate('/cadastro')}
                sx={{ fontWeight: 'bold' }}
              >
                Cadastre-se
              </Link>
            </Typography>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};

export default Login;