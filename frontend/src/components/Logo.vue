<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  scale?: number;
  light?: boolean;
}>();

const logoStyle = computed(() => ({
  transform: `scale(${props.scale || 1})`,
  display: 'inline-flex',
  alignItems: 'center',
  gap: '12px',
  cursor: 'pointer'
}));
</script>

<template>
  <div class="nex-logo-premium" :style="logoStyle" @click="$router.push('/')">
    <div class="logo-emblem">
      <div class="emblem-inner">NB</div>
      <div class="emblem-ring"></div>
    </div>
    <div v-if="scale && scale > 1.2" class="logo-text-p">
       <span class="l-nex">Nex</span><span class="l-bank">Bank</span>
    </div>
  </div>
</template>

<style scoped>
.nex-logo-premium {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  user-select: none;
}

.logo-emblem {
  position: relative;
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.emblem-inner {
  background: #0C1B33;
  color: #C5A059;
  width: 100%;
  height: 100%;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'Outfit', sans-serif;
  font-weight: 900;
  font-size: 1.4rem;
  letter-spacing: -0.5px;
  box-shadow: 0 10px 20px rgba(12, 27, 51, 0.2);
  z-index: 2;
  border: 1px solid rgba(197, 160, 89, 0.6);
}

.emblem-ring {
  position: absolute;
  top: -4px; left: -4px; right: -4px; bottom: -4px;
  border: 2px solid #C5A059;
  border-radius: 18px;
  opacity: 0.5;
  z-index: 1;
  transition: opacity 0.3s;
}

.logo-text-p {
  display: flex;
  font-family: 'Outfit', sans-serif;
  font-size: 1.8rem;
  font-weight: 800;
  letter-spacing: -0.5px;
}

.l-nex { color: #0C1B33; }
.l-bank { color: #C5A059; }

/* Koyu arka planlar için (Intro gibi) */
.intro-screen .l-nex { color: white; }

.nex-logo-premium:hover {
  transform: scale(1.05) translateY(-2px) !important;
}

.nex-logo-premium:hover .emblem-ring {
  opacity: 1;
  transform: scale(1.1);
  transition: 0.4s;
}
</style>
