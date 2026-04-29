<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import Logo from '../components/Logo.vue';

const router = useRouter();
const showIntro = ref(true);
const fadeOut = ref(false);

onMounted(() => {
  // 2.5 saniye sonra dashboard veya login'e yönlendir
  setTimeout(() => {
    fadeOut.value = true;
    setTimeout(() => {
      showIntro.value = false;
      const token = localStorage.getItem('token');
      if (token) {
        router.push('/dashboard');
      } else {
        router.push('/login');
      }
    }, 800); // Fade-out animasyon süresi
  }, 2200);
});
</script>

<template>
  <div v-if="showIntro" class="intro-screen" :class="{ 'fade-out': fadeOut }">
    <div class="intro-content">
      <div class="logo-animation-p">
        <Logo :scale="1.5" />
      </div>
      <div class="brand-reveal">
        <h1 class="brand-name">NexBank</h1>
        <div class="brand-line"></div>
        <p class="brand-tagline">Dijital Finansın Zirvesi</p>
      </div>
    </div>
    
    <div class="intro-footer">
        <div class="loading-bar">
            <div class="bar-progress"></div>
        </div>
        <small>Güvenli bağlantı kuruluyor...</small>
    </div>
  </div>
</template>

<style scoped>
.intro-screen {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: #0C1B33;
  z-index: 10000;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
  transition: opacity 0.8s ease;
}

.fade-out { opacity: 0; pointer-events: none; }

.intro-content {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.logo-animation-p {
  animation: logoPulse 1.5s ease-out forwards;
}

.brand-reveal {
  margin-top: 2rem;
  opacity: 0;
  animation: revealText 1s ease-out 0.5s forwards;
}

.brand-name {
  font-family: 'Outfit', sans-serif;
  font-size: 3rem;
  font-weight: 800;
  letter-spacing: 4px;
  text-transform: uppercase;
  margin: 0;
  color: white;
}

.brand-line {
  height: 2px;
  width: 0;
  background: linear-gradient(90deg, transparent, #C5A059, transparent);
  margin: 10px auto;
  animation: growLine 1.2s ease-out 0.8s forwards;
}

.brand-tagline {
  font-size: 0.9rem;
  font-weight: 600;
  letter-spacing: 3px;
  color: #C5A059;
  text-transform: uppercase;
  opacity: 0.7;
}

.intro-footer {
    position: absolute;
    bottom: 60px;
    text-align: center;
    width: 200px;
}

.loading-bar {
    height: 2px;
    background: rgba(255,255,255,0.1);
    border-radius: 10px;
    overflow: hidden;
    margin-bottom: 10px;
}

.bar-progress {
    height: 100%;
    width: 0;
    background: #C5A059;
    animation: progressLoad 2s linear forwards;
}

.intro-footer small {
    font-size: 0.65rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 1px;
    opacity: 0.5;
}

@keyframes logoPulse {
  0% { transform: scale(0.5); opacity: 0; }
  100% { transform: scale(1); opacity: 1; }
}

@keyframes revealText {
  0% { transform: translateY(20px); opacity: 0; }
  100% { transform: translateY(0); opacity: 1; }
}

@keyframes growLine {
  0% { width: 0; }
  100% { width: 100%; }
}

@keyframes progressLoad {
  0% { width: 0; }
  100% { width: 100%; }
}
</style>
