import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { CssBaseline } from '@mui/material';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import CriarCurso from './pages/CriarCurso';
import CursoDetalhes from './pages/CursoDetalhes';
import Cadastro from './pages/CadastroUsuario';
import EditarCurso from './pages/EditarCurso';
import GerenciarCategorias from './pages/GerenciarCategorias';
import GerenciarProdutores from './pages/GerenciarProdutores';

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
          <Route path="/cadastro" element={<Cadastro />} />
          <Route path="/curso/editar/:id" element={<EditarCurso />} />
          <Route path="/admin/categorias" element={<GerenciarCategorias />} />
          <Route path="/admin/produtores" element={<GerenciarProdutores />} />
        </Routes>
      </BrowserRouter>
    </>
  );
}

export default App;