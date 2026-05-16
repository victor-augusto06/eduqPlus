import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { CssBaseline } from '@mui/material';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import CriarCurso from './pages/CriarCurso';
import CursoDetalhes from './pages/CursoDetalhes';

function App() {
  return (
    <>
      <CssBaseline /> 
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/login" element={<Login />} />
          <Route path="/cursos/:id" element={<CursoDetalhes />} />
          <Route path="/curso/novo" element={<CriarCurso />} />
        </Routes>
      </BrowserRouter>
    </>
  );
}

export default App;