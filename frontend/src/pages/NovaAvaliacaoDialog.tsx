import React, { useState } from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions, 
  Button, TextField, Typography, Box, Rating, Alert
} from '@mui/material';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import api from '../services/api';

interface NovaAvaliacaoDialogProps {
  open: boolean;
  onClose: () => void;
  cursoId: string;
  onSuccess: () => void; 
}

const NovaAvaliacaoDialog: React.FC<NovaAvaliacaoDialogProps> = ({ open, onClose, cursoId, onSuccess }) => {
  const [notaEntrega, setNotaEntrega] = useState<number | null>(0);
  const [notaSuporte, setNotaSuporte] = useState<number | null>(0);
  const [comentario, setComentario] = useState('');
  const [arquivo, setArquivo] = useState<File | null>(null);
  
  const [erro, setErro] = useState('');
  const [loading, setLoading] = useState(false);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const file = e.target.files[0];
      
      if (file.size > 5 * 1024 * 1024) {
        setErro("O arquivo deve ter no máximo 5MB.");
        setArquivo(null);
        return;
      }
      
      setArquivo(file);
      setErro('');
    }
  };

  const handleSubmit = async () => {
    if (!notaEntrega || !notaSuporte) {
      setErro("Por favor, preencha ambas as notas.");
      return;
    }

    const formData = new FormData();
    formData.append('cursoId', cursoId);
    
    const userString = localStorage.getItem('@EduqPlus:user');
    if (userString) {
      const user = JSON.parse(userString);
      formData.append('usuarioId', user.id); 
    }

    formData.append('notaEntrega', notaEntrega.toString());
    formData.append('notaSuporte', notaSuporte.toString());
    formData.append('comentario', comentario);
    
    if (arquivo) {
      formData.append('UrlComprovante', arquivo); 
    }

    setLoading(true);
    try {
      await api.post('/Avaliacao', formData);
      onSuccess(); 
      onClose();   
    } catch (error: any) {
      console.error(error);
      setErro(error.response?.data || "Erro ao enviar a avaliação.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ fontWeight: 'bold' }}>Avaliar Curso</DialogTitle>
      
      <DialogContent dividers>
        {erro && <Alert severity="error" sx={{ mb: 2 }}>{erro}</Alert>}

        {/* Esta é a Box principal que agrupa todo o formulário */}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          
          <Box>
            <Typography component="legend" sx={{ mb: 1 }}>Nota do Conteúdo/Entrega</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Rating
                value={notaEntrega}
                precision={0.1}
                onChange={(_, newValue) => setNotaEntrega(newValue)}
                size="large"
              />
              <TextField
                type="number"
                size="small"
                sx={{ width: 80 }}
                slotProps={{
                  htmlInput: { min: 0, max: 5, step: 0.1 }
                }}
                value={notaEntrega ?? ''}
                onChange={(e) => {
                  const val = parseFloat(e.target.value);
                  if (!isNaN(val)) setNotaEntrega(Math.min(5, Math.max(0, val)));
                  else setNotaEntrega(null);
                }}
              />
            </Box>
          </Box>

          <Box>
            <Typography component="legend" sx={{ mb: 1 }}>Nota do Suporte do Produtor</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Rating
                value={notaSuporte}
                precision={0.1}
                onChange={(_, newValue) => setNotaSuporte(newValue)}
                size="large"
              />
              <TextField
                type="number"
                size="small"
                sx={{ width: 80 }}
                slotProps={{
                  htmlInput: { min: 0, max: 5, step: 0.1 }
                }}
                value={notaSuporte ?? ''}
                onChange={(e) => {
                  const val = parseFloat(e.target.value);
                  if (!isNaN(val)) setNotaSuporte(Math.min(5, Math.max(0, val)));
                  else setNotaSuporte(null);
                }}
              />
            </Box>
          </Box>

          <TextField
            label="Comentário (Opcional)"
            multiline
            rows={4}
            value={comentario}
            onChange={(e) => setComentario(e.target.value)}
            fullWidth
            placeholder="Conte aos outros alunos o que achou do curso..."
          />

          <Box sx={{ mt: 1 }}>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>Comprovante de Compra (PDF ou Imagem)</Typography>
            <Button
              component="label"
              variant="outlined"
              startIcon={<CloudUploadIcon />}
              fullWidth
            >
              {arquivo ? arquivo.name : "Anexar Comprovante"}
              <input
                type="file"
                hidden
                accept=".pdf, image/jpeg, image/png"
                onChange={handleFileChange}
              />
            </Button>
          </Box>

        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} color="inherit" disabled={loading}>Cancelar</Button>
        <Button onClick={handleSubmit} variant="contained" disabled={loading}>
          {loading ? 'Enviando...' : 'Enviar Avaliação'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default NovaAvaliacaoDialog;