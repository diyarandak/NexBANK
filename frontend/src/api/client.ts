import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5157/api', // Backend URL güncellendi
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor: Her isteğe Token ekle
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default apiClient;
