<script setup lang="ts">
import { ref } from 'vue';

const assets = ref([
  { name: 'Vadesiz TL', amount: 45000, color: '#3B82F6' },
  { name: 'Döviz (USD/EUR)', amount: 120000, color: '#10B981' },
  { name: 'Hisse Senedi', amount: 35000, color: '#8B5CF6' },
  { name: 'Yatırım Fonları', amount: 15000, color: '#F59E0B' }
]);
const total = assets.value.reduce((acc, a) => acc + a.amount, 0);
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient" style="font-size: 2.2rem;">Varlıklarım (Portföy)</h1>
      <p class="subtitle">Tüm finansal varlıklarınızın dağılımı.</p>
    </header>

    <div class="glass-card p-4">
      <h2 class="text-center mb-4">Toplam Varlık: <strong>{{ total.toLocaleString('tr-TR') }} TL</strong></h2>
      <div class="portfolio-bars">
        <div v-for="a in assets" :key="a.name" class="mb-4">
          <div class="d-flex justify-content-between mb-1">
            <span>{{ a.name }}</span>
            <strong>{{ a.amount.toLocaleString('tr-TR') }} TL</strong>
          </div>
          <div class="progress" style="height: 12px; border-radius: 6px; background: #E2E8F0;">
            <div class="progress-bar" :style="{ width: (a.amount / total * 100) + '%', background: a.color, borderRadius: '6px' }"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
