<script setup lang="ts">
import { useRouter } from 'vue-router';
const router = useRouter();
</script>

<template>
  <div class="view-container view-animate">
    <div class="coming-soon-wrapper glass-card">
      <div class="icon-pulse">🚀</div>
      <h1 class="text-gradient">Çok Yakında!</h1>
      <p class="subtitle mt-3">
        Sizlere daha iyi bir hizmet sunabilmek için bu özelliği geliştiriyoruz. <br/>
        Lütfen daha sonra tekrar deneyin.
      </p>
      
      <button class="btn btn-primary mt-5 px-5 py-3" @click="router.back()">
        Geri Dön
      </button>
    </div>
  </div>
</template>

<style scoped>
.coming-soon-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  height: 60vh;
  min-height: 400px;
  padding: 40px;
}
.icon-pulse {
  font-size: 5rem;
  margin-bottom: 20px;
  animation: pulse 2s infinite ease-in-out;
}
@keyframes pulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}
.subtitle {
  font-size: 1.1rem;
  line-height: 1.6;
  color: #64748B;
  max-width: 500px;
}
.btn-primary {
  border-radius: 50px;
  font-weight: 600;
  font-size: 1.1rem;
  box-shadow: 0 10px 20px rgba(59, 130, 246, 0.3);
  transition: all 0.3s;
}
.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 15px 25px rgba(59, 130, 246, 0.4);
}
</style>
