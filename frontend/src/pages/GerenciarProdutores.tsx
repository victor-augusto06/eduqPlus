import { useState, useEffect } from 'react';
import { 
  Box, Container, Typography, Button, Paper, CircularProgress, 
  AppBar, Toolbar, IconButton, Dialog, DialogTitle, DialogContent, 
  DialogActions, TextField, Table, TableBody, TableCell, TableContainer, 
  TableHead, TableRow 
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

interface Produtor {
  id: string;
  nome: string;
  nichoPrincipal?: string;
  linksSociais?: string;
}

const GerenciarProdutores = () => {
  const navigate = useNavigate();
  const [produtores, setProdutores] = useState<Produtor[]>([]);
  const [loading, setLoading] = useState(true);
  const [isAdmin, setIsAdmin] = useState(false);

  const [openModal, setOpenModal] = useState(false);
  const [produtorAtual, setProdutorAtual] = useState<Produtor>({ id: '', nome: '' });
  const [isEditMode, setIsEditMode] = useState(false);

  const [openDeleteModal, setOpenDeleteModal] = useState(false);
  const [idParaExcluir, setIdParaExcluir] = useState('');

  const [openErrorModal, setOpenErrorModal] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const carregarProdutores = async () => {
    setLoading(true);
    try {
      const response = await api.get('/Produtor');
      setProdutores(response.data);
    } catch (err) {
      console.error('Erro ao carregar produtores', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const userStr = localStorage.getItem('@EduqPlus:user');
    if (userStr) {
      const user = JSON.parse(userStr);
      setIsAdmin(user.role === 2 || user.Role === 2);
    }
    carregarProdutores();
  }, []);

const handleAbrirCriar = () => {
    setProdutorAtual({ id: '', nome: '', nichoPrincipal: '', linksSociais: '' });
    setIsEditMode(false);
    setOpenModal(true);
  };

  const handleAbrirEditar = (prod: Produtor) => {
    setProdutorAtual({ 
      id: prod.id, 
      nome: prod.nome,
      nichoPrincipal: prod.nichoPrincipal || '',
      linksSociais: prod.linksSociais || ''
    });
    setIsEditMode(true);
    setOpenModal(true);
  };

const handleSalvar = async () => {
    if (!produtorAtual.nome.trim()) return;

    const payload = {
      nome: produtorAtual.nome,
      nichoPrincipal: produtorAtual.nichoPrincipal,
      linksSociais: produtorAtual.linksSociais
    };

    try {
      if (isEditMode) {
        if (isAdmin) {
          await api.put(`/Produtor/${produtorAtual.id}/admin`, payload);
        } else {
          await api.put(`/Produtor/${produtorAtual.id}`, payload);
        }
      } else {
        const userStr = localStorage.getItem('@EduqPlus:user');
        const usuarioId = userStr ? JSON.parse(userStr).id : '';
        
        await api.post('/Produtor', { ...payload, usuarioId });
      }
      setOpenModal(false);
      carregarProdutores();
    } catch (err: any) {
      console.error('Erro ao salvar produtor', err);
      setErrorMessage(err.response?.data?.mensagem || 'Ocorreu um erro ao salvar o produtor.');
      setOpenErrorModal(true);
    }
  };

  const handleAbrirExcluir = (id: string) => {
    setIdParaExcluir(id);
    setOpenDeleteModal(true);
  };

const handleConfirmarExcluir = async () => {
    try {
      await api.delete(`/Produtor/${idParaExcluir}`);
      setOpenDeleteModal(false);
      carregarProdutores();
    } catch (err) {
      console.error('Erro ao excluir produtor', err);
      setOpenDeleteModal(false);
      setErrorMessage('Não é possível excluir este produtor pois há cursos vinculados a ele. Altere os cursos primeiro.');
      setOpenErrorModal(true);
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
    <Box sx={{ flexGrow: 1, backgroundColor: '#f9fafb', minHeight: '100vh', pb: 6 }}>
      <AppBar position="static" sx={{ mb: 4, backgroundColor: '#1976d2' }}>
        <Toolbar>
          <IconButton edge="start" color="inherit" onClick={() => navigate(-1)} sx={{ mr: 2 }}>
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 'bold' }}>
            Gerenciar Produtores
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="md">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h5" sx={{ fontWeight: 'bold' }}>
            Produtores do Sistema
          </Typography>
          <Button variant="contained" color="primary" startIcon={<AddIcon />} onClick={handleAbrirCriar}>
            Novo Produtor
          </Button>
        </Box>

        <TableContainer component={Paper} elevation={2}>
          <Table>
            <TableHead sx={{ backgroundColor: '#f1f5f9' }}>
              <TableRow>
                <TableCell sx={{ fontWeight: 'bold' }}>ID</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Nome do Produtor</TableCell>
                <TableCell sx={{ fontWeight: 'bold', textAlign: 'right' }}>Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {produtores.map((prod) => (
                <TableRow key={prod.id} hover>
                  <TableCell sx={{ color: 'text.secondary', fontSize: '0.8rem' }}>{prod.id.substring(0,8)}...</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>{prod.nome}</TableCell>
                  <TableCell sx={{ textAlign: 'right' }}>
                    <IconButton color="primary" onClick={() => handleAbrirEditar(prod)}>
                      <EditIcon />
                    </IconButton>
                    <IconButton color="error" onClick={() => handleAbrirExcluir(prod.id)}>
                      <DeleteIcon />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {produtores.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3} sx={{ textAlign: 'center', py: 3, color: 'text.secondary' }}>
                    Nenhum produtor encontrado.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Container>

      <Dialog open={openModal} onClose={() => setOpenModal(false)} fullWidth maxWidth="xs">
        <DialogTitle sx={{ fontWeight: 'bold' }}>
          {isEditMode ? 'Editar Produtor' : 'Novo Produtor'}
        </DialogTitle>
        <DialogContent>
          <TextField
            autoFocus margin="dense" label="Nome do Produtor" fullWidth variant="outlined"
            value={produtorAtual.nome} onChange={(e) => setProdutorAtual({ ...produtorAtual, nome: e.target.value })}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense" label="Nicho Principal" fullWidth variant="outlined"
            value={produtorAtual.nichoPrincipal} onChange={(e) => setProdutorAtual({ ...produtorAtual, nichoPrincipal: e.target.value })}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense" label="Links Sociais" fullWidth variant="outlined" multiline rows={2}
            value={produtorAtual.linksSociais} onChange={(e) => setProdutorAtual({ ...produtorAtual, linksSociais: e.target.value })}
            helperText="Você pode adicionar mais de um link separando-os por ponto e vírgula (;)"
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenModal(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleSalvar} disabled={!produtorAtual.nome.trim()}>
            Salvar
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openDeleteModal} onClose={() => setOpenDeleteModal(false)} fullWidth maxWidth="xs">
        <DialogTitle sx={{ fontWeight: 'bold', color: '#b91c1c' }}>Confirmar Exclusão</DialogTitle>
        <DialogContent>
          <Typography>Tem certeza que deseja excluir este produtor? Todos os cursos vinculados a ele serão excluídos em conjunto!</Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenDeleteModal(false)}>Cancelar</Button>
          <Button variant="contained" color="error" onClick={handleConfirmarExcluir}>
            Excluir
          </Button>
        </DialogActions>
      </Dialog>
      
      <Dialog open={openErrorModal} onClose={() => setOpenErrorModal(false)} fullWidth maxWidth="xs">
        <DialogTitle sx={{ fontWeight: 'bold', color: '#f59e0b' }}>
          Aviso
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1">
            {errorMessage}
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button variant="contained" color="warning" onClick={() => setOpenErrorModal(false)}>
            Entendido
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default GerenciarProdutores;