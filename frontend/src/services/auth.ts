const TOKEN_KEY = '@EduqPlus:token';
const USER_KEY = '@EduqPlus:user';

export interface AuthUser {
  id: string;
  nome: string;
  email: string;
  role: number;
  Role?: number;
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
