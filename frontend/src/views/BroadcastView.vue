<script setup lang="ts">
import { ref } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const message = ref('');
const loading = ref(false);

const sendBroadcast = async () => {
    if (!message.value) return;
    loading.value = true;
    try {
        // Backend'e duyuru mesajını gönderiyoruz (API JSON string bekler)
        await apiClient.post('/notifications/broadcast', `"${message.value}"`, {
            headers: { 'Content-Type': 'application/json' }
        });
        toast.success('Duyuru tüm müşterilere başarıyla iletildi! 📣');
        message.value = '';
    } catch (err) {
        toast.error('Duyuru gönderilirken bir hata oluştu.');
    } finally {
        loading.value = false;
    }
};
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient">Toplu Duyuru Merkezi</h1>
      <p class="subtitle">Banka müşterilerine anlık duyurular, kampanya bilgileri veya sistem uyarıları gönderin.</p>
    </header>

    <div class="glass-card max-w-md mx-auto" style="max-width: 600px;">
        <div class="broadcast-icon mb-4">📣</div>
        <h3 class="mb-2">Yeni Duyuru Oluştur</h3>
        <p class="text-muted mb-4">Bu mesaj, kayıtlı tüm aktif müşterilerin hesap paneline anında düşecektir.</p>
        
        <div class="input-group">
            <label>Duyuru Mesajı</label>
            <textarea v-model="message" rows="5" placeholder="Örn: Değerli müşterimiz, Kurban Bayramı'na özel %1.99 faiz oranlı bayram kredisi fırsatlarını kaçırmayın! Hemen başvurun..." class="premium-input"></textarea>
        </div>

        <button @click="sendBroadcast" :disabled="loading || !message" class="btn-action btn-primary w-100 mt-4 broadcast-btn">
            {{ loading ? 'Gönderiliyor...' : '🚀 Tüm Müşterilere Gönder' }}
        </button>
    </div>
  </div>
</template>

<style scoped>
.broadcast-icon { font-size: 4rem; text-align: center; display: block; margin: 0 auto; text-shadow: 0 10px 20px rgba(0,0,0,0.1); animation: float 3s ease-in-out infinite; }
.premium-input { width: 100%; background: #F8FAFC; border: 2px solid #E2E8F0; border-radius: 12px; padding: 15px; font-size: 1rem; color: #0F172A; resize: vertical; transition: all 0.3s; }
.premium-input:focus { outline: none; border-color: var(--primary); background: white; box-shadow: 0 0 0 4px rgba(245, 158, 11, 0.1); }
.broadcast-btn { font-size: 1.1rem; padding: 16px; border-radius: 14px; text-transform: uppercase; letter-spacing: 1px; }

@keyframes float {
    0% { transform: translateY(0px); }
    50% { transform: translateY(-10px); }
    100% { transform: translateY(0px); }
}
</style>
