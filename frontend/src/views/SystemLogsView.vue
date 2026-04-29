<script setup lang="ts">
import { ref } from 'vue';

const logs = ref([
  { id: 101, type: 'INFO', action: 'Kullanıcı Girişi', detail: 'user0@nexbank.com IP: 192.168.1.1', time: '10:45:12' },
  { id: 102, type: 'WARN', action: 'Hatalı Şifre', detail: 'mehmet@nexbank.com 3. deneme', time: '11:20:05' },
  { id: 103, type: 'ERROR', action: 'API Timeout', detail: 'TCMB Kur Servisi yanıt vermiyor', time: '11:45:00' },
  { id: 104, type: 'INFO', action: 'Gün Sonu', detail: 'Mutabakat başlatıldı.', time: '17:00:00' },
]);
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient" style="font-size: 2.2rem;">Sistem Logları</h1>
      <p class="subtitle">Bankacılık çekirdeği teknik izleme ve denetim kayıtları.</p>
    </header>

    <div class="glass-card p-0 overflow-hidden">
      <table class="data-table w-100 text-start m-0">
        <thead class="bg-light">
          <tr>
            <th>Saat</th>
            <th>Tür</th>
            <th>İşlem</th>
            <th>Detay</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="l in logs" :key="l.id">
            <td class="text-muted">{{ l.time }}</td>
            <td>
              <span class="badge" :class="l.type === 'INFO' ? 'bg-info' : l.type === 'WARN' ? 'bg-warning' : 'bg-danger'">
                {{ l.type }}
              </span>
            </td>
            <td><strong>{{ l.action }}</strong></td>
            <td class="text-sm font-monospace">{{ l.detail }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
