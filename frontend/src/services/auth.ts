const TOKEN_KEY = '@EduqPlus:token';
const USER_KEY = '@EduqPlus:user';

export interface AuthUser {
  id: string;
  nome: string;
  email: string;
  role: number | string;
  Role?: number | string;
  Id?: string;
  [key: string]: any;
}

// Limpa qualquer dado residual no localStorage para evitar que sessões antigas
// persistam indefinidamente entre reinicializações do navegador.
try {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
} catch {
  // Ignora possíveis restrições de ambiente
}

export const getAuthToken = (): string | null => {
  return sessionStorage.getItem(TOKEN_KEY);
};

export const getAuthUser = (): AuthUser | null => {
  const userStr = sessionStorage.getItem(USER_KEY);
  if (!userStr) return null;
  try {
    return JSON.parse(userStr);
  } catch {
    return null;
  }
};

export const setAuthSession = (token: string, user: any): void => {
  sessionStorage.setItem(TOKEN_KEY, token);
  sessionStorage.setItem(USER_KEY, JSON.stringify(user));
  // Limpa também do localStorage caso exista resíduo
  try {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  } catch {
    // Ignora
  }
};

export const clearAuthSession = (): void => {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
  try {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  } catch {
    // Ignora
  }
};

export const isAuthenticated = (): boolean => {
  return !!getAuthToken();
};

const getRoleFromToken = (token: string | null): string | null => {
  if (!token) return null;
  try {
    const rawToken = token.startsWith('Bearer ') ? token.slice(7) : token;
    const parts = rawToken.split('.');
    if (parts.length < 2) return null;
    const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
    return payload['role'] || payload['Role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
  } catch {
    return null;
  }
};

export const isUserAdmin = (user?: AuthUser | null): boolean => {
  const target = user || getAuthUser();
  if (target) {
    const role = target.role ?? target.Role;
    if (role !== undefined && role !== null) {
      if (typeof role === 'number') return role === 2;
      const roleStr = String(role).trim().toLowerCase();
      if (roleStr === '2' || roleStr === 'admin') return true;
    }
  }

  const token = getAuthToken();
  const tokenRole = getRoleFromToken(token);
  if (tokenRole) {
    const tokenRoleStr = String(tokenRole).trim().toLowerCase();
    if (tokenRoleStr === '2' || tokenRoleStr === 'admin') return true;
  }

  return false;
};
