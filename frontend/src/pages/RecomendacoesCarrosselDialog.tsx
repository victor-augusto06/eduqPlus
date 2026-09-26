import React from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions, 
  Button, Typography, Box, Card, CardContent, Chip, IconButton
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import VerifiedIcon from '@mui/icons-material/Verified';
import { type Curso, EStatusAuditoria } from '../types/Curso';
import { useNavigate } from 'react-router-dom';

interface RecomendacoesCarrosselDialogProps {
  open: boolean;
  onClose: () => void;
  cursos: Curso[];
}

const RecomendacoesCarrosselDialog: React.FC<RecomendacoesCarrosselDialogProps> = ({ open, onClose, cursos }) => {
  const navigate = useNavigate();

  if (!cursos || cursos.length === 0) return null;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ m: 0, p: 2, fontWeight: 'bold', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        Você também pode gostar destes cursos...
        <IconButton
          aria-label="close"
          onClick={onClose}
          sx={{
            color: (theme) => theme.palette.grey[500],
          }}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      
      <DialogContent dividers sx={{ p: 3, backgroundColor: '#f9fafb' }}>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
          Baseado na sua avaliação positiva, a nossa Inteligência Artificial encontrou os seguintes cursos semelhantes para você continuar aprendendo:
        </Typography>

        <Box sx={{ 
          display: 'flex', 
          overflowX: 'auto', 
          gap: 3, 
          pb: 2,
          '&::-webkit-scrollbar': {
            height: 8,
          },
          '&::-webkit-scrollbar-thumb': {
            backgroundColor: '#cbd5e1',
            borderRadius: 4,
          }
        }}>
          {cursos.map((curso, index) => {
            const trustScore = curso.trustScore || 0;
            return (
              <Card 
                key={curso.id} 
                elevation={2} 
                sx={{ 
                  minWidth: 280, 
                  maxWidth: 280,
                  display: 'flex', 
                  flexDirection: 'column',
                  cursor: 'pointer',
                  transition: '0.2s',
                  '&:hover': { transform: 'scale(1.02)', boxShadow: 4 }
                }}
                onClick={() => {
                  onClose();
                  navigate(`/cursos/${curso.id}`);
                }}
              >
                <CardContent sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
                  <Typography variant="subtitle2" color="primary" sx={{ mb: 1, fontWeight: 'bold' }}>
                    Recomendação #{index + 1}
                  </Typography>

                  <Typography variant="h6" sx={{ fontWeight: 'bold', mb: 1, fontSize: '1.1rem', lineHeight: 1.3, display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                    {curso.titulo}
                  </Typography>

                  <Typography variant="body2" color="text.primary" sx={{ mb: 2, display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                    {curso.descricaoOriginal}
                  </Typography>
                  
                  <Box sx={{ mt: 'auto', display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                    <Chip 
                      label={trustScore > 0 ? `Nota: ${trustScore.toFixed(1)}` : 'Novo'} 
                      color={trustScore === 0 ? "default" : trustScore >= 4 ? "success" : trustScore >= 3 ? "warning" : "error"}
                      size="small" 
                      sx={{ fontWeight: 'bold' }}
                    />

                    {curso.statusAuditoria === EStatusAuditoria.Aprovado && (
                      <Chip label="Auditado" color="success" size="small" variant="outlined" icon={<VerifiedIcon fontSize="small"/>} />
                    )}
                  </Box>
                </CardContent>
              </Card>
            );
          })}
        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} color="inherit">
          Não, obrigado
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default RecomendacoesCarrosselDialog;
