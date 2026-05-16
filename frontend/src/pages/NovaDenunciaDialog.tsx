import React, { useState } from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions, 
  Button, TextField, Box, Alert, FormControl, InputLabel, Select, MenuItem
} from '@mui/material';
import api from '../services/api';

interface NovaDenunciaDialogProps {
  open: boolean;
  onClose: () => void;
  cursoId: string;
  onSuccess: () => void; 
}

const NovaDenunciaDialog: React.FC<NovaDenunciaDialogProps> = ({ open, onClose, cursoId, onSuccess }) => {
  const [categoria, setCategoria] = useState('');
  const [relatoDetalhado, setRelatoDetalhado] = useState('');
  const [erro, setErro] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async () => {
    if (!categoria || !relatoDetalhado.trim()) {
      setErro("Por favor, selecione uma categoria e preencha o relato detalhado.");
      return;
    }

    setLoading(true);
    try {
      await api.post('/Denuncia', {
        cursoId,
        categoria,
        relatoDetalhado
      });

      onSuccess(); 
      onClose();   
      setCategoria('');
      setRelatoDetalhado('');
      setErro('');
    } catch (error: any) {
      console.error(error);
      setErro(error.response?.data?.mensagem || "Erro ao enviar a denúncia.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ fontWeight: 'bold', color: '#b91c1c' }}>Denunciar Curso</DialogTitle>
      
      <DialogContent dividers>
        {erro && <Alert severity="error" sx={{ mb: 2 }}>{erro}</Alert>}

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 1 }}>
          <FormControl fullWidth size="small">
            <InputLabel>Motivo da Denúncia</InputLabel>
            <Select
              value={categoria}
              label="Motivo da Denúncia"
              onChange={(e) => setCategoria(e.target.value)}
            >
              <MenuItem value={1}>Conteúdo Enganoso / Propaganda Falsa</MenuItem>
              <MenuItem value={2}>Plágio / Direitos Autorais</MenuItem>
              <MenuItem value={3}>Qualidade Extremamente Baixa / Inassistível</MenuItem>
              <MenuItem value={4}>Conteúdo Ofensivo / Inadequado</MenuItem>
              <MenuItem value={5}>Outros Motivos</MenuItem>
            </Select>
          </FormControl>

          <TextField
            label="Relato Detalhado"
            multiline
            rows={4}
            value={relatoDetalhado}
            onChange={(e) => setRelatoDetalhado(e.target.value)}
            fullWidth
            placeholder="Explique detalhadamente o problema encontrado no curso para que a moderação possa analisar..."
          />
        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} color="inherit" disabled={loading}>Cancelar</Button>
        <Button onClick={handleSubmit} variant="contained" color="error" disabled={loading}>
          {loading ? 'Enviando...' : 'Enviar Denúncia'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default NovaDenunciaDialog;