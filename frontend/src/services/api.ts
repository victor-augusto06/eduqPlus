import axios from 'axios';
import { getAuthToken, clearAuthSession } from './auth';

const api = axios.create({
  baseURL: 'http://localhost:5292/api', 
});

api.interceptors.request.use(
  (config) => {
    const token = getAuthToken();
    if (token) {
      config.headers.Authorization = `${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      clearAuthSession();
    }
    return Promise.reject(error);
  }
);

export default api;
