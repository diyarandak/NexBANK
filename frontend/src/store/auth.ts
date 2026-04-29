import { defineStore } from 'pinia';
import apiClient from '../api/client';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    token: localStorage.getItem('token') || null,
    loading: false,
    error: null as string | null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.user?.role === 'Admin' || state.user?.role === 'Staff',
  },
  actions: {
    // Müşteri Girişi (TC + Şifre)
    async customerLogin(credentials: { tcKimlikNo: string; password: string }) {
      this.loading = true;
      this.error = null;
      try {
        const response = await apiClient.post('/auth/customer-login', credentials);
        this.token = response.data.token;
        this.user = response.data.user;
        localStorage.setItem('token', this.token!);
        localStorage.setItem('user', JSON.stringify(this.user));
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Giriş yapılamadı. TC Kimlik No veya şifrenizi kontrol edin.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    // Personel Girişi (E-posta + Şifre)
    async staffLogin(credentials: { email: string; password: string }) {
      this.loading = true;
      this.error = null;
      try {
        const response = await apiClient.post('/auth/staff-login', credentials);
        this.token = response.data.token;
        this.user = response.data.user;
        localStorage.setItem('token', this.token!);
        localStorage.setItem('user', JSON.stringify(this.user));
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Giriş yapılamadı. E-posta veya şifrenizi kontrol edin.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    // Müşteri Kaydı
    async registerCustomer(userData: any) {
      this.loading = true;
      this.error = null;
      try {
        await apiClient.post('/auth/register', userData);
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Kayıt sırasında bir hata oluştu.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    // Personel Kaydı
    async registerStaff(userData: any) {
      this.loading = true;
      this.error = null;
      try {
        await apiClient.post('/auth/register-staff', userData);
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Personel kaydı sırasında bir hata oluştu.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
  }
});
