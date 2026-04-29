<script setup lang="ts">
import { ref } from 'vue';

const staff = ref([
  { id: 1, name: 'Diyar Andak', email: 'diyarandak@neetbank.com', role: 'Şube Müdürü', status: 'Aktif' },
  { id: 2, name: 'Ayşe Yılmaz', email: 'ayse.yilmaz@nexbank.com', role: 'Gişe Yetkilisi', status: 'Aktif' },
  { id: 3, name: 'Mehmet Kaya', email: 'mehmet.kaya@nexbank.com', role: 'Bireysel Portföy', status: 'İzinde' },
]);
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient" style="font-size: 2.2rem;">Personel Yönetimi</h1>
      <p class="subtitle">Şube çalışanlarını ve yetkilerini yönetin.</p>
    </header>

    <div class="glass-card">
      <div class="d-flex justify-content-between mb-4">
        <h3>Şube Kadrosu</h3>
        <button class="btn btn-sm btn-primary">+ Yeni Personel</button>
      </div>
      <table class="data-table w-100 text-start">
        <thead>
          <tr>
            <th>Ad Soyad</th>
            <th>E-Posta</th>
            <th>Rol</th>
            <th>Durum</th>
            <th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="s in staff" :key="s.id">
            <td><strong>{{ s.name }}</strong></td>
            <td>{{ s.email }}</td>
            <td>{{ s.role }}</td>
            <td><span class="badge" :class="s.status === 'Aktif' ? 'bg-success' : 'bg-warning'">{{ s.status }}</span></td>
            <td><button class="btn btn-sm btn-outline">Düzenle</button></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
