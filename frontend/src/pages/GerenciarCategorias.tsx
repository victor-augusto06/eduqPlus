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

interface Categoria {
  id: string;
  nome: string;
}

const GerenciarCategorias = () => {
  const navigate = useNavigate();
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);

  const [openModal, setOpenModal] = useState(false);
  const [categoriaAtual, setCategoriaAtual] = useState<Categoria>({ id: '', nome: '' });
  const [isEditMode, setIsEditMode] = useState(false);

  const [openDeleteModal, setOpenDeleteModal] = useState(false);
  const [idParaExcluir, setIdParaExcluir] = useState('');
  
  const [openErrorModal, setOpenErrorModal] = useState(false);

  const carregarCategorias = async () => {
    setLoading(true);
    try {
      const response = await api.get('/Categoria');
      setCategorias(response.data);
    } catch (err) {
      console.error('Erro ao carregar categorias', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregarCategorias();
  }, []);

  const handleAbrirCriar = () => {
    setCategoriaAtual({ id: '', nome: '' });
    setIsEditMode(false);
    setOpenModal(true);
  };

  const handleAbrirEditar = (cat: Categoria) => {
    setCategoriaAtual({ id: cat.id, nome: cat.nome });
    setIsEditMode(true);
    setOpenModal(true);
  };

  const handleSalvar = async () => {
    if (!categoriaAtual.nome.trim()) return;

    try {
      if (isEditMode) {
        await api.put(`/Categoria/${categoriaAtual.id}`, { nome: categoriaAtual.nome });
      } else {
        await api.post('/Categoria', { nome: categoriaAtual.nome });
      }
      setOpenModal(false);
      carregarCategorias();
    } catch (err) {
      console.error('Erro ao salvar categoria', err);
    }
  };

  const handleAbrirExcluir = (id: string) => {
    setIdParaExcluir(id);
    setOpenDeleteModal(true);
  };

  const handleConfirmarExcluir = async () => {
    try {
      await api.delete(`/Categoria/${idParaExcluir}`);
      setOpenDeleteModal(false);
      carregarCategorias();
    } catch (err) {
      console.error('Erro ao excluir categoria', err);
      setOpenDeleteModal(false);
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
            Gerenciar Categorias
          </Typography>
        </Toolbar>
      </AppBar>

      <Container maxWidth="md">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h5" sx={{ fontWeight: 'bold' }}>
            Categorias do Sistema
          </Typography>
          <Button variant="contained" color="primary" startIcon={<AddIcon />} onClick={handleAbrirCriar}>
            Nova Categoria
          </Button>
        </Box>

        <TableContainer component={Paper} elevation={2}>
          <Table>
            <TableHead sx={{ backgroundColor: '#f1f5f9' }}>
              <TableRow>
                <TableCell sx={{ fontWeight: 'bold' }}>ID</TableCell>
                <TableCell sx={{ fontWeight: 'bold' }}>Nome da Categoria</TableCell>
                <TableCell sx={{ fontWeight: 'bold', textAlign: 'right' }}>Ações</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {categorias.map((cat) => (
                <TableRow key={cat.id} hover>
                  <TableCell sx={{ color: 'text.secondary', fontSize: '0.8rem' }}>{cat.id.substring(0,8)}...</TableCell>
                  <TableCell sx={{ fontWeight: 'bold' }}>{cat.nome}</TableCell>
                  <TableCell sx={{ textAlign: 'right' }}>
                    <IconButton color="primary" onClick={() => handleAbrirEditar(cat)}>
                      <EditIcon />
                    </IconButton>
                    <IconButton color="error" onClick={() => handleAbrirExcluir(cat.id)}>
                      <DeleteIcon />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
              {categorias.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3} sx={{ textAlign: 'center', py: 3, color: 'text.secondary' }}>
                    Nenhuma categoria encontrada.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Container>

      <Dialog open={openModal} onClose={() => setOpenModal(false)} fullWidth maxWidth="xs">
        <DialogTitle sx={{ fontWeight: 'bold' }}>
          {isEditMode ? 'Editar Categoria' : 'Nova Categoria'}
        </DialogTitle>
        <DialogContent>
          <TextField
            autoFocus margin="dense" label="Nome da Categoria" fullWidth variant="outlined"
            value={categoriaAtual.nome} onChange={(e) => setCategoriaAtual({ ...categoriaAtual, nome: e.target.value })}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenModal(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleSalvar} disabled={!categoriaAtual.nome.trim()}>
            Salvar
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openDeleteModal} onClose={() => setOpenDeleteModal(false)} fullWidth maxWidth="xs">
        <DialogTitle sx={{ fontWeight: 'bold', color: '#b91c1c' }}>Confirmar Exclusão</DialogTitle>
        <DialogContent>
          <Typography>Tem certeza que deseja excluir esta categoria?</Typography>
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
          Ação Bloqueada
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1">
            Não é possível excluir esta categoria porque <strong>existem cursos vinculados a ela</strong> no sistema.
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            Para excluir, você precisa primeiro alterar a categoria de todos os cursos que a utilizam.
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

export default GerenciarCategorias;