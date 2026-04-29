<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';

const accounts = ref<any[]>([]);
const selectedAccountId = ref('');
const amount = ref('');
const transactionType = ref('withdraw');
const loading = ref(false);
const showQR = ref(false);
const message = ref('');
const error = ref('');

const fetchAccounts = async () => {
  try {
    const res = await apiClient.get('/accounts');
    accounts.value = res.data;
    if (accounts.value.length > 0 && !selectedAccountId.value) {
      selectedAccountId.value = accounts.value[0].id.toString();
    }
  } catch (err) { console.error(err); }
};

onMounted(fetchAccounts);

const handleAction = async () => {
    if (transactionType.value === 'withdraw' && !showQR) {
        showQR.value = true;
        return;
    }
    await executeTransaction();
};

const executeTransaction = async () => {
    message.value = '';
    error.value = '';
    loading.value = true;
    try {
        const endpoint = transactionType.value === 'deposit' ? '/transactions/deposit' : '/transactions/withdraw';
        await apiClient.post(endpoint, {
            accountId: parseInt(selectedAccountId.value),
            amount: parseFloat(amount.value)
        });
        message.value = `İşlem başarıyla tamamlandı. Bakiyeniz güncellendi.`;
        showQR.value = false;
        fetchAccounts();
    } catch (err: any) {
        error.value = err.response?.data?.message || 'İşlem hatası.';
    } finally {
        loading.value = false;
    }
};
</script>

<template>
  <div class="cash-page view-animate">
    <header class="header">
      <h1 class="text-gradient">ATM İşlemleri</h1>
      <p class="subtitle">Kartsız, hızlı ve güvenli ATM deneyimi. QR ile anında nakit çekin.</p>
    </header>

    <div class="cash-layout">
        <div class="cash-card glass-card">
            <div class="tabs">
                <button :class="['tab-btn', {active: transactionType === 'withdraw'}]" @click="transactionType = 'withdraw'">📤 Para Çek</button>
                <button :class="['tab-btn', {active: transactionType === 'deposit'}]" @click="transactionType = 'deposit'">📥 Para Yatır</button>
            </div>

            <div v-if="!showQR" class="form-content">
                <div v-if="error" class="alert error">{{ error }}</div>
                <div v-if="message" class="alert success">{{ message }}</div>

                <div class="form-group mb-4">
                    <label>Hesap Seçin</label>
                    <select v-model="selectedAccountId" class="premium-input">
                        <option v-for="acc in accounts" :key="acc.id" :value="acc.id">
                            {{ acc.iban }} (Bakiye: {{ acc.balance.toLocaleString() }} {{ acc.currency }})
                        </option>
                    </select>
                </div>

                <div class="form-group">
                    <label>Tutar (₺)</label>
                    <div class="amount-presets">
                        <button v-for="p in [100, 200, 500, 1000]" :key="p" @click="amount = p.toString()" class="preset-btn">{{ p }} ₺</button>
                    </div>
                    <input type="number" v-model="amount" placeholder="Özel Tutar Girin" class="premium-input mt-3" />
                </div>

                <button @click="handleAction" class="main-btn mt-4">
                    {{ transactionType === 'withdraw' ? 'QR Kod Al' : 'Para Yatır' }}
                </button>
            </div>

            <!-- QR SİMUYLASYONU -->
            <div v-else class="qr-simulation animate-fade">
                <h3>QR ile Para Çekme</h3>
                <p>Lütfen ATM ekranındaki kodu taratın.</p>
                <div class="qr-box">
                    <div class="qr-scanner-line"></div>
                    <img src="https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=NexBank_ATM" alt="QR Code" />
                </div>
                <p class="timer">Kalan Süre: <strong>49sn</strong></p>
                
                <div class="qr-actions mt-4">
                    <button @click="showQR = false" class="demo-btn">İptal Et</button>
                    <button @click="executeTransaction" class="premium-button">Telefondan Onayla (Simüle)</button>
                </div>
            </div>
        </div>

        <div class="info-sidebar">
            <div class="info-card glass-card">
                <h4>🏦 Yakındaki ATM'ler</h4>
                <div class="atm-item">
                    <span>📍 Yalova Merkez Şube Önü</span>
                    <small>Müsait</small>
                </div>
                <div class="atm-item mt-2">
                    <span>📍 Çınarcık Meydan ATM</span>
                    <small>Müsait</small>
                </div>
            </div>

            <div class="info-card glass-card mt-3">
                <h4>⚠️ Güvenlik Uyarısı</h4>
                <p>QR kodunuzu kimseyle paylaşmayın. İşleminiz sadece sizin cihazınızdan onaylanabilir.</p>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.cash-layout { display: grid; grid-template-columns: 1.5fr 1fr; gap: 30px; margin-top: 30px; }
.cash-card { padding: 40px; }
.tabs { display: flex; gap: 10px; margin-bottom: 30px; background: #f8fafc; padding: 6px; border-radius: 15px; }
.tab-btn { flex: 1; padding: 12px; border: none; background: transparent; color: #64748b; font-weight: 700; cursor: pointer; border-radius: 10px; }
.tab-btn.active { background: #0C1B33; color: #C5A059; shadow: 0 4px 10px rgba(0,0,0,0.1); }

.amount-presets { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; }
.preset-btn { 
    padding: 10px; background: white; border: 1px solid #C5A059; color: #0C1B33; 
    border-radius: 10px; cursor: pointer; font-weight: 700; transition: all 0.2s;
}
.preset-btn:hover { background: #0C1B33; color: #C5A059; }

.main-btn {
    width: 100%; padding: 18px; border: none; border-radius: 15px;
    background: #0C1B33; color: white; font-weight: 700; font-size: 1.1rem; cursor: pointer; transition: all 0.3s;
}
.main-btn:hover { transform: translateY(-2px); box-shadow: 0 10px 20px rgba(12, 27, 51, 0.2); }

/* QR SIMULATION */
.qr-simulation { text-align: center; }
.qr-box { 
    width: 220px; height: 220px; margin: 30px auto; background: white; padding: 10px; 
    border: 2px solid #C5A059; border-radius: 20px; position: relative; overflow: hidden;
}
.qr-scanner-line { 
    position: absolute; left: 0; right: 0; height: 3px; background: #C5A059; 
    box-shadow: 0 0 15px #C5A059; animation: scan 2s linear infinite;
}
@keyframes scan { 0% { top: 0; } 100% { top: 100%; } }
.timer { color: #ef4444; font-size: 0.9rem; }

/* SIDEBAR */
.info-card { padding: 25px; }
.info-card h4 { margin-bottom: 15px; color: #0C1B33; font-size: 1rem; border-bottom: 1px solid #f1f5f9; padding-bottom: 10px; }
.atm-item { display: flex; justify-content: space-between; align-items: center; font-size: 0.85rem; }
.atm-item small { color: #10b981; font-weight: 700; }
.info-card p { font-size: 0.85rem; color: #64748b; line-height: 1.5; }

.alert { padding: 15px; border-radius: 12px; margin-bottom: 25px; font-weight: 700; text-align: center; font-size: 0.9rem; }
.alert.error { background: #fef2f2; color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.1); }
.alert.success { background: #f0fdf4; color: #10b981; border: 1px solid rgba(16, 185, 129, 0.1); }

.mt-2 { margin-top: 10px; }
.mt-3 { margin-top: 15px; }
.mt-4 { margin-top: 24px; }
.mb-4 { margin-bottom: 24px; }
</style>
