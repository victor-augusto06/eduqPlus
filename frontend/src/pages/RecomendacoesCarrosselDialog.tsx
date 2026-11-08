import React from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions, 
  Button, Typography, Box, Card, CardContent, Chip, IconButton,
  Skeleton
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import VerifiedIcon from '@mui/icons-material/Verified';
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome';
import { type Curso, EStatusAuditoria } from '../types/Curso';
import { useNavigate } from 'react-router-dom';

interface RecomendacoesCarrosselDialogProps {
  open: boolean;
  onClose: () => void;
  cursos: Curso[];
  loading?: boolean;
}

const RecomendacoesCarrosselDialog: React.FC<RecomendacoesCarrosselDialogProps> = ({ 
  open, 
  onClose, 
  cursos,
  loading = false
}) => {
  const navigate = useNavigate();

  if (!open) return null;
  if (!loading && (!cursos || cursos.length === 0)) return null;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle sx={{ m: 0, p: 2, fontWeight: 'bold', display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AutoAwesomeIcon color="primary" />
          <span>Cursos Recomendados para Você</span>
        </Box>
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
          {loading 
            ? "Nossa Inteligência Artificial está buscando os cursos mais relevantes com base na sua avaliação..."
            : "Com base na sua experiência positiva, nossa IA selecionou estes cursos semelhantes para acelerar seu aprendizado:"}
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
          {loading ? (
            // Exibe 3 cards esqueleto enquanto carrega
            [1, 2, 3].map((item) => (
              <Card 
                key={item} 
                elevation={1} 
                sx={{ 
                  minWidth: 280, 
                  maxWidth: 280, 
                  p: 2, 
                  borderRadius: 2,
                  bgcolor: '#ffffff'
                }}
              >
                <Skeleton variant="text" width="40%" height={24} sx={{ mb: 1 }} />
                <Skeleton variant="text" width="90%" height={32} sx={{ mb: 1 }} />
                <Skeleton variant="rectangular" height={60} sx={{ mb: 2, borderRadius: 1 }} />
                <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                  <Skeleton variant="rounded" width={70} height={24} />
                  <Skeleton variant="rounded" width={70} height={24} />
                </Box>
              </Card>
            ))
          ) : (
            cursos.map((curso, index) => {
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
                    borderRadius: 2,
                    transition: 'all 0.2s ease-in-out',
                    border: '1px solid #e2e8f0',
                    '&:hover': { 
                      transform: 'translateY(-4px)', 
                      boxShadow: 6,
                      borderColor: 'primary.main'
                    }
                  }}
                  onClick={() => {
                    onClose();
                    navigate(`/cursos/${curso.id}`);
                  }}
                >
                  <CardContent sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                      <Chip 
                        label={`Top #${index + 1}`} 
                        color="primary" 
                        size="small" 
                        sx={{ fontWeight: 'bold', fontSize: '0.75rem', height: 22 }} 
                      />
                      <AutoAwesomeIcon sx={{ fontSize: 16, color: '#9333ea' }} />
                    </Box>

                    <Typography 
                      variant="h6" 
                      sx={{ 
                        fontWeight: 'bold', 
                        mb: 1, 
                        fontSize: '1.05rem', 
                        lineHeight: 1.3, 
                        display: '-webkit-box', 
                        WebkitLineClamp: 2, 
                        WebkitBoxOrient: 'vertical', 
                        overflow: 'hidden' 
                      }}
                    >
                      {curso.titulo}
                    </Typography>

                    <Typography 
                      variant="body2" 
                      color="text.secondary" 
                      sx={{ 
                        mb: 2, 
                        display: '-webkit-box', 
                        WebkitLineClamp: 3, 
                        WebkitBoxOrient: 'vertical', 
                        overflow: 'hidden',
                        fontSize: '0.875rem'
                      }}
                    >
                      {curso.descricaoOriginal || "Sem descrição disponível."}
                    </Typography>
                    
                    <Box sx={{ mt: 'auto', display: 'flex', gap: 1, flexWrap: 'wrap', alignItems: 'center' }}>
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
            })
          )}
        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2, justifyContent: 'space-between' }}>
        <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
          Recomendações baseadas em similaridade de conteúdo e índice de confiança.
        </Typography>
        <Button onClick={onClose} color="inherit">
          Concluir
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default RecomendacoesCarrosselDialog;
