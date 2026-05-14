import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5292/api', 
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('@EduqPlus:token');
    if (token) {
      config.headers.Authorization = `${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default api;