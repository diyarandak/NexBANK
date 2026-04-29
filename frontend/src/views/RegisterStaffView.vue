<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../store/auth';
import { useRouter } from 'vue-router';

const fullName = ref('');
const email = ref('');
const password = ref('');
const authStore = useAuthStore();
const router = useRouter();
const localError = ref('');

const handleRegister = async () => {
  localError.value = '';
  authStore.error = null;

  if (!/^(?=.*[A-Z])(?=.*\d).{8,}$/.test(password.value)) {
    localError.value = "Şifre en az 8 karakter, bir büyük harf ve bir rakam içermelidir.";
    return;
  }

  const success = await authStore.registerStaff({
    fullName: fullName.value,
    email: email.value,
    password: password.value
  });
  
  if (success) {
    alert("Personel hesabı başarıyla oluşturuldu! Artık kurumsal e-posta ve şifrenizle giriş yapabilirsiniz.");
    router.push('/login');
  }
};
</script>

<template>
  <div class="login-page">
    <div class="blob blob-1"></div>
    <div class="blob blob-2"></div>

    <div class="login-card glass-card">
      <div class="login-header">
        <div class="logo-staff">🏢</div>
        <h1 class="text-gradient">Personel Kayıt</h1>
        <p>NexBank çalışan hesabı oluşturun</p>
      </div>

      <form @submit.prevent="handleRegister" class="login-form">
        <div class="input-group">
          <label>Ad Soyad</label>
          <input v-model="fullName" type="text" required class="premium-input" placeholder="Adınız Soyadınız" />
        </div>

        <div class="input-group">
          <label>Kurumsal E-posta</label>
          <input v-model="email" type="email" required class="premium-input" placeholder="personel@nexbank.com" />
        </div>

        <div class="input-group">
          <label>Şifre</label>
          <input v-model="password" type="password" required class="premium-input" placeholder="Min 8 karakter, 1 büyük harf, 1 rakam" />
        </div>

        <p v-if="localError" class="error-msg">{{ localError }}</p>
        <p v-if="authStore.error && !localError" class="error-msg">{{ authStore.error }}</p>

        <button type="submit" class="premium-button w-full" :disabled="authStore.loading">
          {{ authStore.loading ? 'Kaydediliyor...' : 'Personel Hesabı Oluştur' }}
        </button>
      </form>

      <div class="switch-helper">
        <p>Zaten hesabınız var mı? <router-link to="/login" class="link">Giriş Yap</router-link></p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  width: 100vw; height: 100vh;
  display: flex; align-items: center; justify-content: center;
  background-color: var(--bg-dark);
  position: relative; overflow: hidden;
}
.login-card { width: 460px; padding: 40px; z-index: 10; }
.login-header { text-align: center; margin-bottom: 30px; }
.logo-staff {
  width: 60px; height: 60px;
  background: linear-gradient(135deg, #F59E0B, #EF4444);
  border-radius: 15px; display: flex; align-items: center; justify-content: center;
  margin: 0 auto 16px; font-size: 1.8rem;
}
.login-header p { color: var(--text-muted); font-size: 0.9rem; margin-top: 8px; }
.login-form { display: flex; flex-direction: column; gap: 18px; }
.input-group label { display: block; font-size: 0.85rem; margin-bottom: 8px; color: var(--text-muted); }
.w-full { width: 100%; }
.error-msg {
  color: var(--danger); font-size: 0.8rem; text-align: center;
  background: rgba(239,68,68,0.08); padding: 10px; border-radius: 8px;
  border: 1px solid rgba(239,68,68,0.15);
}
.switch-helper { margin-top: 24px; text-align: center; font-size: 0.85rem; color: var(--text-muted); }
.switch-helper .link { color: var(--primary); text-decoration: none; font-weight: 600; }
.switch-helper .link:hover { color: var(--accent); }
.blob { position: absolute; border-radius: 50%; filter: blur(80px); z-index: 1; }
.blob-1 { width: 300px; height: 300px; background: rgba(245,158,11,0.2); top: -100px; right: -100px; }
.blob-2 { width: 400px; height: 400px; background: rgba(239,68,68,0.1); bottom: -150px; left: -150px; }
</style>
