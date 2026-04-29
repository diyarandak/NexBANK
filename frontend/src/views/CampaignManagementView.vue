<script setup lang="ts">
import { ref } from 'vue';

const campaigns = ref([
  { id: 1, name: 'Yeni Gelenlere %0 Faiz', type: 'Kredi', status: 'Aktif', audience: 'Yeni Müşteriler' },
  { id: 2, name: 'Market Alışverişine 500TL İade', type: 'Kredi Kartı', status: 'Aktif', audience: 'Tüm Müşteriler' },
  { id: 3, name: 'Emekli Promosyonu 15.000TL', type: 'Maaş', status: 'Taslak', audience: 'Emekliler' },
]);
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient" style="font-size: 2.2rem;">Kampanya Yönetimi</h1>
      <p class="subtitle">Müşterilere sunulacak fırsat ve promosyonları düzenleyin.</p>
    </header>

    <div class="glass-card">
      <div class="d-flex justify-content-between mb-4">
        <h3>Aktif Kampanyalar</h3>
        <button class="btn btn-sm btn-primary">+ Yeni Kampanya</button>
      </div>
      <table class="data-table w-100 text-start">
        <thead>
          <tr>
            <th>Kampanya Adı</th>
            <th>Kategori</th>
            <th>Hedef Kitle</th>
            <th>Durum</th>
            <th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in campaigns" :key="c.id">
            <td><strong>{{ c.name }}</strong></td>
            <td>{{ c.type }}</td>
            <td>{{ c.audience }}</td>
            <td><span class="badge" :class="c.status === 'Aktif' ? 'bg-success' : 'bg-secondary'">{{ c.status }}</span></td>
            <td><button class="btn btn-sm btn-outline">Detay</button></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
