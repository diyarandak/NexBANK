<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';

const logs = ref<any[]>([]);
const loading = ref(true);

const fetchLogs = async () => {
    try {
        const res = await apiClient.get('/logs');
        logs.value = res.data;
    } catch (err) {
        console.error('Loglar yüklenemedi');
    } finally {
        loading.value = false;
    }
};

onMounted(fetchLogs);
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5 d-flex justify-content-between align-items-center">
      <div>
        <h1 class="text-gradient" style="font-size: 2.2rem;">Sistem Denetim Günlüğü</h1>
        <p class="subtitle">Bankadaki tüm kritik personel ve sistem hareketleri saniye saniye kayıt altına alınır.</p>
      </div>
      <button @click="fetchLogs" class="btn-action">🔄 Yenile</button>
    </header>

    <div v-if="loading" class="loader-container"><div class="glow-loader"></div></div>

    <div v-else class="glass-card p-0 overflow-hidden">
        <div class="log-table-wrap">
            <table class="premium-table">
                <thead>
                    <tr>
                        <th style="width: 100px;">Zaman</th>
                        <th>Olay</th>
                        <th>Kullanıcı</th>
                        <th>Detay</th>
                        <th>Derece</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="l in logs" :key="l.id">
                        <td><span class="l-time">{{ new Date(l.timestamp).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'}) }}</span></td>
                        <td><strong>{{ l.action }}</strong></td>
                        <td class="text-primary font-weight-bold">{{ l.user }}</td>
                        <td><p class="m-0 text-muted" style="font-size: 0.85rem;">{{ l.detail }}</p></td>
                        <td>
                            <span :class="['severity-pill', l.severity.toLowerCase()]">
                                {{ l.severity }}
                            </span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
  </div>
</template>

<style scoped>
.log-table-wrap { min-height: 500px; }
.premium-table { width: 100%; border-collapse: collapse; }
.premium-table th { padding: 20px; background: #F8FAFC; color: #64748B; font-size: 0.75rem; text-transform: uppercase; font-weight: 800; }
.premium-table td { padding: 18px 20px; border-bottom: 1px solid #F1F5F9; font-size: 0.9rem; }

.l-time { background: #F1F5F9; padding: 4px 8px; border-radius: 6px; font-weight: 800; font-size: 0.75rem; color: #0F172A; }

.severity-pill { padding: 4px 12px; border-radius: 100px; font-size: 0.7rem; font-weight: 800; text-transform: uppercase; }
.severity-pill.info { background: #E0F2FE; color: #0369A1; }
.severity-pill.success { background: #E1F7E5; color: #10B981; }
.severity-pill.warning { background: #FEF3C7; color: #D97706; }
.severity-pill.danger { background: #FEE2E2; color: #EF4444; }

tr:hover { background: #F8FAFC; }
</style>
