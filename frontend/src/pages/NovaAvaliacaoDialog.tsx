import React, { useState } from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions, 
  Button, TextField, Typography, Box, Rating, Alert,
  LinearProgress, CircularProgress
} from '@mui/material';
import CloudUploadIcon from '@mui/icons-material/CloudUpload';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import api from '../services/api';
import { getAuthUser } from '../services/auth';

interface NovaAvaliacaoDialogProps {
  open: boolean;
  onClose: () => void;
  cursoId: string;
  onSuccess: (recomendacoes?: any[]) => void; 
}

const NovaAvaliacaoDialog: React.FC<NovaAvaliacaoDialogProps> = ({ open, onClose, cursoId, onSuccess }) => {
  const [notaEntrega, setNotaEntrega] = useState<number | null>(0);
  const [notaSuporte, setNotaSuporte] = useState<number | null>(0);
  const [comentario, setComentario] = useState('');

  const [arquivos, setArquivos] = useState<File[]>([]);
  
  const [erro, setErro] = useState('');
  const [loading, setLoading] = useState(false);
  const [statusMensagem, setStatusMensagem] = useState('');

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const filesArray = Array.from(e.target.files);
      const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
      const validFiles: File[] = [];

      for (const file of filesArray) {
        if (!allowedTypes.includes(file.type)) {
          setErro(`Formato inválido no arquivo "${file.name}". Envie apenas imagens (JPG, PNG) ou PDF.`);
          setArquivos([]);
          return;
        }
        
        if (file.size > 5 * 1024 * 1024) {
          setErro(`O arquivo "${file.name}" excede o máximo de 5MB.`);
          setArquivos([]); 
          return;
        }
        
        validFiles.push(file);
      }
      
      setArquivos(validFiles);
      setErro('');
    }
  };

  const resetForm = () => {
    setNotaEntrega(0);
    setNotaSuporte(0);
    setComentario('');
    setArquivos([]);
    setErro('');
    setStatusMensagem('');
  };

  const handleClose = () => {
    if (loading) return; // evita fechar enquanto processa
    resetForm();
    onClose();
  };

  const handleSubmit = async () => {
    if (!notaEntrega || !notaSuporte) {
      setErro("Por favor, preencha ambas as notas.");
      return;
    }

    const formData = new FormData();
    formData.append('cursoId', cursoId);
    
    const user = getAuthUser();
    if (user) {

      formData.append('usuarioId', user.id); 
    }

    formData.append('notaEntrega', notaEntrega.toString());
    formData.append('notaSuporte', notaSuporte.toString());
    formData.append('comentario', comentario);
    
    if (arquivos.length > 0) {
      arquivos.forEach((file) => {
        formData.append('comprovantes', file); 
      });
    }

    const isAltaAvaliacao = ((notaEntrega + notaSuporte) / 2) >= 4.0;

    setLoading(true);
    setStatusMensagem(arquivos.length > 0 
      ? "Enviando avaliação e analisando comprovante com IA..." 
      : "Registrando sua avaliação com segurança...");

    // Dispara a busca de recomendações em paralelo à submissão da avaliação
    const promessaRecomendacoes = isAltaAvaliacao
      ? api.get(`/Curso/${cursoId}/recomendacoes?k=3`)
          .then(res => (res.data || []).slice(0, 3))
          .catch(recError => {
            console.warn("Aviso ao buscar recomendações em paralelo:", recError);
            return [];
          })
      : Promise.resolve([]);

    try {
      await api.post('/Avaliacao', formData);
      
      if (isAltaAvaliacao) {
        setStatusMensagem("Personalizando suas recomendações de cursos...");
      }

      // Aguarda as recomendações que já estavam sendo processadas em paralelo
      const recomendacoes = await promessaRecomendacoes;

      onSuccess(recomendacoes.length > 0 ? recomendacoes : undefined); 
      resetForm();
      onClose();   
    } catch (error: any) {
      console.error(error);
      setErro(error.response?.data?.mensagem || "Erro ao enviar a avaliação.");
    } finally {
      setLoading(false);
      setStatusMensagem('');
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ fontWeight: 'bold' }}>Avaliar Curso</DialogTitle>
      
      <DialogContent dividers>
        {erro && <Alert severity="error" sx={{ mb: 2 }}>{erro}</Alert>}

        {loading && (
          <Box sx={{ mb: 3, p: 2, bgcolor: '#f0fdf4', border: '1px solid #bbf7d0', borderRadius: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1 }}>
              <AutoAwesomeIcon color="primary" sx={{ animation: 'pulse 1.5s infinite' }} />
              <Typography variant="body2" sx={{ fontWeight: 600, color: '#166534' }}>
                {statusMensagem || "Processando avaliação..."}
              </Typography>
            </Box>
            <LinearProgress sx={{ borderRadius: 1, height: 6 }} />
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
              Nossos algoritmos inteligentes estão processando os dados para você. Isso leva apenas alguns instantes!
            </Typography>
          </Box>
        )}

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, opacity: loading ? 0.6 : 1, pointerEvents: loading ? 'none' : 'auto' }}>
          
          <Box>
            <Typography component="legend" sx={{ mb: 1 }}>Nota do Conteúdo/Entrega</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Rating
                value={notaEntrega}
                precision={0.1}
                onChange={(_, newValue) => setNotaEntrega(newValue)}
                size="large"
                disabled={loading}
              />
              <TextField
                type="number"
                size="small"
                sx={{ width: 80 }}
                slotProps={{
                  htmlInput: { min: 0, max: 5, step: 0.1 }
                }}
                disabled={loading}
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
                disabled={loading}
              />
              <TextField
                type="number"
                size="small"
                sx={{ width: 80 }}
                slotProps={{
                  htmlInput: { min: 0, max: 5, step: 0.1 }
                }}
                disabled={loading}
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
            disabled={loading}
            placeholder="Conte aos outros alunos o que achou do curso..."
          />

          <Box sx={{ mt: 1 }}>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>Comprovantes de Compra (PDF ou Imagens)</Typography>
            <Button
              component="label"
              variant="outlined"
              startIcon={<CloudUploadIcon />}
              fullWidth
              disabled={loading}
            >
              {arquivos.length > 0 
                ? (arquivos.length === 1 ? arquivos[0].name : `${arquivos.length} arquivo(s) selecionado(s)`) 
                : "Anexar Comprovantes"}
              <input
                type="file"
                hidden
                multiple
                accept=".pdf, image/jpeg, image/png"
                onChange={handleFileChange}
                disabled={loading}
              />
            </Button>
          </Box>

        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={handleClose} color="inherit" disabled={loading}>
          Cancelar
        </Button>
        <Button 
          onClick={handleSubmit} 
          variant="contained" 
          disabled={loading}
          startIcon={loading ? <CircularProgress size={18} color="inherit" /> : null}
        >
          {loading ? 'Processando...' : 'Enviar Avaliação'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default NovaAvaliacaoDialog;
