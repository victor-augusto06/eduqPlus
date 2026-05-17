import React, { useState, useEffect } from 'react';
import { 
  Box, Container, Typography, TextField, Button, Paper, MenuItem, 
  CircularProgress, Alert, Dialog, DialogTitle, DialogContent, DialogActions 
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

interface Categoria {
  id: string;
  nome: string;
}

interface Produtor {
  id: string;
  nome: string;
}

const CriarCurso = () => {
  const navigate = useNavigate();

  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [produtores, setProdutores] = useState<Produtor[]>([]);
  const [loadingInit, setLoadingInit] = useState(true);
  const [loadingSubmit, setLoadingSubmit] = useState(false);
  const [error, setError] = useState('');
  const [promessas, setPromessas] = useState<string[]>(['']);

  const [titulo, setTitulo] = useState('');
  const [descricaoOriginal, setDescricaoOriginal] = useState('');
  const [plataformaHospedagem, setPlataformaHospedagem] = useState('');
  const [categoriaId, setCategoriaId] = useState('');
  const [produtorId, setProdutorId] = useState('');

  const [openCategoriaDialog, setOpenCategoriaDialog] = useState(false);
  const [novaCategoriaNome, setNovaCategoriaNome] = useState('');
  const [loadingCategoria, setLoadingCategoria] = useState(false);

  const [openProdutorDialog, setOpenProdutorDialog] = useState(false);
  const [novoProdutorNome, setNovoProdutorNome] = useState('');
  const [novoProdutorNicho, setNovoProdutorNicho] = useState('');
  const [novoProdutorLinks, setNovoProdutorLinks] = useState('');
  const [loadingProdutor, setLoadingProdutor] = useState(false);

  const carregarDependencias = async () => {
    try {
      const [catRes, prodRes] = await Promise.all([
        api.get('/Categoria'),
        api.get('/Produtor')
      ]);
      setCategorias(Array.isArray(catRes.data) ? catRes.data : []);
      setProdutores(Array.isArray(prodRes.data) ? prodRes.data : []);
      setError('');
    } catch (err) {
      setError('Erro ao carregar categorias e produtores.');
    } finally {
      setLoadingInit(false);
    }
  };

  useEffect(() => {
    carregarDependencias();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoadingSubmit(true);

    try {
      const userStr = localStorage.getItem('@EduqPlus:user');
      const usuarioId = userStr ? JSON.parse(userStr).id : '';

      const payload = {
        titulo,
        descricaoOriginal,
        plataformaHospedagem,
        categoriaId,
        produtorId,
        usuarioId,
        promessaCursos: promessas
          .filter(p => p.trim() !== '')
          .map(p => ({ descricao: p }))
      };

      await api.post('/Curso', payload);
      navigate('/dashboard');
    } catch (err) {
      setError('Erro ao salvar o curso. Verifique os dados e tente novamente.');
    } finally {
      setLoadingSubmit(false);
    }
  };

  const handlePromessaChange = (index: number, value: string) => {
    const novasPromessas = [...promessas];
    novasPromessas[index] = value;
    setPromessas(novasPromessas);
  };

  const handleAdicionarPromessa = () => {
    setPromessas([...promessas, '']);
  };

  const handleRemoverPromessa = (index: number) => {
    if (promessas.length > 1) {
      setPromessas(promessas.filter((_, i) => i !== index));
    } else {
      setPromessas(['']); // Se for o único, apenas limpa
    }
  };

  const handleCriarCategoria = async () => {
    if (!novaCategoriaNome.trim()) return;
    setLoadingCategoria(true);
    try {
      const response = await api.post('/Categoria', { nome: novaCategoriaNome });
      await carregarDependencias();
      setCategoriaId(response.data.id);
      setOpenCategoriaDialog(false);
      setNovaCategoriaNome('');
    } catch (err) {
      setError('Erro ao criar categoria.');
    } finally {
      setLoadingCategoria(false);
    }
  };

  const handleCriarProdutor = async () => {
    if (!novoProdutorNome.trim()) return;
    setLoadingProdutor(true);
    try {
      const userStr = localStorage.getItem('@EduqPlus:user');
      const usuarioId = userStr ? JSON.parse(userStr).id : '';

      const payload = {
        usuarioId,
        nome: novoProdutorNome,
        nichoPrincipal: novoProdutorNicho,
        linksSociais: novoProdutorLinks
      };

      const response = await api.post('/Produtor', payload);
      await carregarDependencias();
      setProdutorId(response.data.id);
      setOpenProdutorDialog(false);
      setNovoProdutorNome('');
      setNovoProdutorNicho('');
      setNovoProdutorLinks('');
    } catch (err) {
      setError('Erro ao criar produtor.');
    } finally {
      setLoadingProdutor(false);
    }
  };

  if (loadingInit) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 10 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1, backgroundColor: '#f0f2f5', minHeight: '100vh', py: 5 }}>
      <Container maxWidth="md">
        <Paper elevation={3} sx={{ p: 4, borderRadius: 2 }}>
          <Typography variant="h5" sx={{ fontWeight: 'bold' }} gutterBottom>
            Cadastrar Novo Curso
          </Typography>

          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              fullWidth
              label="Título do Curso"
              variant="outlined"
              margin="normal"
              value={titulo}
              onChange={(e) => setTitulo(e.target.value)}
              required
            />

            <TextField
              fullWidth
              label="Descrição Original"
              variant="outlined"
              margin="normal"
              multiline
              rows={4}
              value={descricaoOriginal}
              onChange={(e) => setDescricaoOriginal(e.target.value)}
              required
            />

            <TextField
              fullWidth
              label="Plataforma de Hospedagem"
              variant="outlined"
              margin="normal"
              value={plataformaHospedagem}
              onChange={(e) => setPlataformaHospedagem(e.target.value)}
              required
            />

            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mt: 2, mb: 1 }}>
              <TextField
                select
                fullWidth
                label="Categoria"
                variant="outlined"
                value={categoriaId}
                onChange={(e) => setCategoriaId(e.target.value)}
                required
              >
                {categorias.map((cat) => (
                  <MenuItem key={cat.id} value={cat.id}>
                    {cat.nome}
                  </MenuItem>
                ))}
              </TextField>
              <Button 
                type="button"
                variant="outlined" 
                sx={{ height: 56, minWidth: 100 }} 
                onClick={() => setOpenCategoriaDialog(true)}
              >
                Nova
              </Button>
            </Box>

            <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mt: 2, mb: 1 }}>
              <TextField
                select
                fullWidth
                label="Produtor"
                variant="outlined"
                value={produtorId}
                onChange={(e) => setProdutorId(e.target.value)}
                required
              >
                {produtores.map((prod) => (
                  <MenuItem key={prod.id} value={prod.id}>
                    {prod.nome}
                  </MenuItem>
                ))}
              </TextField>
              <Button 
                type="button"
                variant="outlined" 
                sx={{ height: 56, minWidth: 100 }} 
                onClick={() => setOpenProdutorDialog(true)}
              >
                Novo
              </Button>
            </Box>
            
            {/* Seção Dinâmica de Promessas */}
            <Typography variant="subtitle1" sx={{ fontWeight: 'bold', mt: 3, mb: 1, color: '#475569' }}>
              Promessas do Produtor (Ex: "Acesso Vitalício", "Suporte 24/7")
            </Typography>
            
            {promessas.map((promessa, index) => (
              <Box key={index} sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 1.5 }}>
                <TextField
                  fullWidth
                  label={`Promessa #${index + 1}`}
                  variant="outlined"
                  size="small"
                  value={promessa}
                  onChange={(e) => handlePromessaChange(index, e.target.value)}
                />
                <Button 
                  type="button"
                  variant="outlined" 
                  color="error" 
                  onClick={() => handleRemoverPromessa(index)}
                  sx={{ height: 40 }}
                >
                  Remover
                </Button>
              </Box>
            ))}
            
            <Button 
              type="button"
              variant="outlined" 
              color="secondary" 
              onClick={handleAdicionarPromessa}
              sx={{ mt: 1, mb: 2, borderStyle: 'dashed' }}
            >
              + Adicionar Outra Promessa
            </Button>

            <Box sx={{ mt: 4, display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button type="button" variant="outlined" onClick={() => navigate('/dashboard')}>
                Cancelar
              </Button>
              <Button 
                type="submit" 
                variant="contained" 
                color="primary"
                disabled={loadingSubmit}
              >
                {loadingSubmit ? 'Salvando...' : 'Salvar Curso'}
              </Button>
            </Box>
          </Box>
        </Paper>
      </Container>

      <Dialog open={openCategoriaDialog} onClose={() => setOpenCategoriaDialog(false)} fullWidth maxWidth="sm">
        <DialogTitle sx={{ fontWeight: 'bold' }}>Nova Categoria</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Nome da Categoria"
            fullWidth
            variant="outlined"
            value={novaCategoriaNome}
            onChange={(e) => setNovaCategoriaNome(e.target.value)}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenCategoriaDialog(false)}>Cancelar</Button>
          <Button 
            type="button"
            onClick={handleCriarCategoria} 
            variant="contained" 
            disabled={loadingCategoria || !novaCategoriaNome.trim()}
          >
            {loadingCategoria ? 'Salvando...' : 'Salvar'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={openProdutorDialog} onClose={() => setOpenProdutorDialog(false)} fullWidth maxWidth="sm">
        <DialogTitle sx={{ fontWeight: 'bold' }}>Novo Produtor</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Nome do Produtor"
            fullWidth
            variant="outlined"
            value={novoProdutorNome}
            onChange={(e) => setNovoProdutorNome(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label="Nicho Principal"
            fullWidth
            variant="outlined"
            value={novoProdutorNicho}
            onChange={(e) => setNovoProdutorNicho(e.target.value)}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label="Links Sociais"
            fullWidth
            variant="outlined"
            multiline
            rows={2}
            value={novoProdutorLinks}
            onChange={(e) => setNovoProdutorLinks(e.target.value)}
            helperText="Você pode adicionar mais de um link separando-os por ponto e vírgula (;)"
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setOpenProdutorDialog(false)}>Cancelar</Button>
          <Button 
            type="button"
            onClick={handleCriarProdutor} 
            variant="contained" 
            disabled={loadingProdutor || !novoProdutorNome.trim()}
          >
            {loadingProdutor ? 'Salvando...' : 'Salvar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CriarCurso;