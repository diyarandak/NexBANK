<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const settings = ref({
    loanRate: 3.50,
    transferFee: 1.00,
    savingRate: 45.0
});
const loading = ref(true);
const saving = ref(false);

const fetchSettings = async () => {
    try {
        const res = await apiClient.get('/banksettings');
        settings.value = res.data;
    } catch (err) { console.error(err); }
    finally { loading.value = false; }
};

const handleSave = async () => {
    saving.value = true;
    try {
        await apiClient.post('/banksettings/update', settings.value);
        toast.success('Banka parametreleri başarıyla güncellendi! Tüm müşterilere anlık yansıtıldı.');
    } catch (err) {
        toast.error('Kaydetme sırasında bir hata oluştu.');
    } finally { saving.value = false; }
};

onMounted(fetchSettings);
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient">Banka Sistem Ayarları</h1>
      <p class="subtitle">NexBank global finansal parametrelerini ve şube kurallarını bu ekrandan yönetin.</p>
    </header>

    <div v-if="loading" class="loader-container"><div class="glow-loader"></div></div>

    <div v-else class="settings-layout">
        <!-- ⚙️ PARAMETRE KARTLARI -->
        <div class="glass-card p-5">
            <h3 class="mb-5">🏦 Genel Faiz & Komisyon Oranları</h3>
            
            <div class="settings-form">
                <div class="s-row-group">
                    <div class="s-field">
                        <label>Kredi Faiz Oranı (%)</label>
                        <div class="input-with-label">
                            <input type="number" v-model="settings.loanRate" step="0.01" />
                            <span>%</span>
                        </div>
                        <small>Müşterilerin tüm kredi başvurularında baz alacağı aylık oran.</small>
                    </div>

                    <div class="s-field">
                        <label>EFT/Havale Ücreti (₺)</label>
                        <div class="input-with-label">
                            <input type="number" v-model="settings.transferFee" step="0.5" />
                            <span>₺</span>
                        </div>
                        <small>İşlem başına alınacak sabit komisyon tutarı.</small>
                    </div>
                </div>

                <div class="s-field mt-4">
                    <label>Vadeli Mevduat Faizi (Yıllık %)</label>
                    <div class="input-with-label">
                        <input type="number" v-model="settings.savingRate" step="0.5" />
                        <span>%</span>
                    </div>
                    <small>Tasarruf hesapları için uygulanan yıllık brüt faiz oranı.</small>
                </div>

                <button @click="handleSave" :disabled="saving" class="btn-action btn-primary w-100 mt-5 p-3">
                    {{ saving ? 'Güncelleniyor...' : 'Tüm Sistemi Güncelle' }}
                </button>
            </div>
        </div>

        <!-- 🛡️ SİSTEM DURUMU -->
        <div class="sidebar-info">
            <div class="glass-card mb-4 status-box">
                <h4>Sistem Durumu</h4>
                <div class="status-item"><span class="label">API Durumu:</span> <span class="tag-green">AKTİF</span></div>
                <div class="status-item"><span class="label">Şube Sayısı:</span> <strong>14</strong></div>
                <div class="status-item"><span class="label">Operasyon Sürümü:</span> <strong>v2.0.4-Gold</strong></div>
            </div>

            <div class="glass-card info-box">
                <h4>💡 Yönetici Notu</h4>
                <p>Faiz oranlarındaki değişiklikler hali hazırda onaylanmış kredileri etkilemez, yalnızca yeni başvuruları kapsar.</p>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.settings-layout { display: grid; grid-template-columns: 2fr 1fr; gap: 32px; }

.s-row-group { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
.s-field label { font-size: 0.85rem; font-weight: 800; color: var(--primary-dark); margin-bottom: 10px; display: block; }
.s-field small { font-size: 0.75rem; color: var(--text-muted); font-weight: 600; margin-top: 8px; display: block; }

.input-with-label { position: relative; display: flex; align-items: center; }
.input-with-label input { padding-right: 40px; }
.input-with-label span { position: absolute; right: 15px; font-weight: 800; color: var(--text-muted); }

.status-box h4 { margin-bottom: 20px; }
.status-item { display: flex; justify-content: space-between; padding: 12px 0; border-bottom: 1px solid #F1F5F9; font-size: 0.85rem; }
.status-item .label { color: var(--text-muted); font-weight: 600; }
.tag-green { background: #DCFCE7; color: #166534; padding: 2px 8px; border-radius: 6px; font-weight: 800; font-size: 0.7rem; }

.info-box p { font-size: 0.85rem; color: var(--text-muted); margin-top: 10px; line-height: 1.5; }
</style>
