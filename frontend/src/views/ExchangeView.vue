<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
  ArrowDownUp, TrendingUp, TrendingDown, 
  Wallet, Info, AlertCircle, ArrowRight
} from 'lucide-vue-next';

const toast = useToastStore();

const rates = ref<any>({
    'USD': 34.4415, 'EUR': 37.2390, 'GBP': 43.0945, 'XAU': 2559.9957
});
const userAccounts = ref<any[]>([]);
const loading = ref(true);
let rateInterval: any = null;

const form = ref({
    fromAccountId: '',
    toAccountId: '',
    amountToExchange: 0,
    targetCurrency: 'USD'
});

const currencies = ['TRY', 'USD', 'EUR', 'GBP', 'XAU'];

const fetchData = async () => {
    try {
        const [rateRes, accRes] = await Promise.all([
            apiClient.get('/exchange/rates').catch(() => ({ data: rates.value })),
            apiClient.get('/accounts')
        ]);
        if (rateRes.data) rates.value = rateRes.data;
        userAccounts.value = accRes.data;
        
        if (userAccounts.value.length >= 1) {
            form.value.fromAccountId = userAccounts.value[0].id;
            autoSelectToAccount();
        }
    } catch (err) {
        console.error('Veri yükleme hatası');
    } finally {
        loading.value = false;
    }
};

const autoSelectToAccount = () => {
    const matchingAcc = userAccounts.value.find(a => a.currency === form.value.targetCurrency);
    if (matchingAcc) {
        form.value.toAccountId = matchingAcc.id;
    } else {
        // Eğer o dövizde hesabı yoksa, varsayılan olarak bir hesap seçili kalsın veya uyarı verilebilir
        const otherAcc = userAccounts.value.find(a => a.id !== form.value.fromAccountId);
        if (otherAcc) form.value.toAccountId = otherAcc.id;
    }
};

// Döviz türü değiştiğinde hesabı otomatik seç
watch(() => form.value.targetCurrency, () => {
    autoSelectToAccount();
});

const simulateRates = () => {
    Object.keys(rates.value).forEach(k => {
        const change = (Math.random() - 0.5) * 0.005;
        rates.value[k] = parseFloat((rates.value[k] + change).toFixed(4));
    });
};

const getRateTry = (curr: string) => {
    if (curr === 'TRY') return 1;
    return rates.value[curr] || 32.45;
};

const estimatedAmount = computed(() => {
    if (!form.value.amountToExchange) return 0;
    const fromAcc = userAccounts.value.find(a => a.id === form.value.fromAccountId);
    if (!fromAcc) return 0;

    const rateInTry = getRateTry(fromAcc.currency); 
    const targetRateInTry = getRateTry(form.value.targetCurrency);
    
    return (form.value.amountToExchange * rateInTry) / targetRateInTry;
});

const formatCurrency = (val: number, currency: string = 'TRY') => {
    return new Intl.NumberFormat('tr-TR', { 
        style: 'currency', 
        currency: currency,
        minimumFractionDigits: currency === 'TRY' ? 2 : 4 
    }).format(val);
};

const handleSwap = async () => {
    if (form.value.amountToExchange <= 0) {
        toast.error('Lütfen geçerli bir tutar girin.');
        return;
    }
    
    const fromAcc = userAccounts.value.find(a => a.id === form.value.fromAccountId);
    const toAcc = userAccounts.value.find(a => a.id === form.value.toAccountId);

    if (fromAcc && fromAcc.balance < form.value.amountToExchange) {
        toast.error('Yetersiz bakiye.');
        return;
    }

    if (toAcc && toAcc.currency !== form.value.targetCurrency) {
        toast.warning(`Dikkat: Alınacak hesap dövizi (${toAcc.currency}) ile seçilen döviz (${form.value.targetCurrency}) farklı.`);
    }

    try {
        await apiClient.post('/exchange/swap', {
            fromAccountId: form.value.fromAccountId,
            toAccountId: form.value.toAccountId,
            amountToExchange: form.value.amountToExchange,
            targetCurrency: form.value.targetCurrency
        });
        toast.success('Döviz takas işlemi başarıyla gerçekleşti!');
        fetchData();
    } catch (err: any) {
        toast.error(err.response?.data?.message || 'İşlem gerçekleştirilemedi.');
    }
};

onMounted(() => {
    fetchData();
    rateInterval = setInterval(simulateRates, 3000);
});

onUnmounted(() => clearInterval(rateInterval));
</script>

<template>
  <div class="view-container fade-in">
    <header class="exchange-header mb-4">
      <div class="header-content">
        <h1 class="text-gradient-gold">Yatırım Masası</h1>
        <p class="subtitle">Anlık kurlar üzerinden güvenli takas.</p>
      </div>
      <div class="header-badges">
        <div class="live-status">
            <span class="ping-dot"></span>
            Canlı Piyasa
        </div>
      </div>
    </header>

    <div v-if="loading" class="loader-premium"><div class="spinner-gold"></div></div>

    <div v-else class="exchange-grid-elite">
        <div class="terminal-side">
            <div class="glass-card-dark terminal-card">
                <div class="card-header-v3">
                    <TrendingUp :size="14" />
                    <span>PİYASA VERİLERİ</span>
                </div>
                <div class="rates-list-v3">
                    <div v-for="c in currencies" :key="c" class="rate-item-v3">
                        <div class="ri-flag">
                            <span class="f-emoji">{{ c === 'USD' ? '🇺🇸' : c === 'EUR' ? '🇪🇺' : c === 'GBP' ? '🇬🇧' : c === 'XAU' ? '🪙' : '🇹🇷' }}</span>
                        </div>
                        <div class="ri-names">
                            <strong>{{ c }}</strong>
                            <span>{{ c === 'XAU' ? 'Altın' : 'Döviz' }}</span>
                        </div>
                        <div class="ri-price">
                            <div class="p-val">{{ getRateTry(c).toFixed(4) }} ₺</div>
                            <div class="p-trend" :class="Math.random() > 0.4 ? 'up' : 'down'">
                                <span>{{ (Math.random() * 0.3).toFixed(2) }}%</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="info-banner-gold mt-3">
                <div class="ib-text">
                    <p>Hafta sonu makas aralıkları değişkenlik gösterebilir.</p>
                </div>
            </div>
        </div>

        <div class="swap-side">
            <div class="glass-card-elite swap-card">
                <div class="card-header-v3">
                    <ArrowDownUp :size="14" />
                    <span>DÖVİZ TAKASI</span>
                </div>

                <div class="swap-interface-v3 mt-3">
                    <div class="swap-box-v3 from">
                        <div class="sb-top">
                            <label>GÖNDERİLEN</label>
                            <span class="sb-balance" v-if="form.fromAccountId">
                                <Wallet :size="12" /> {{ formatCurrency(userAccounts.find(a => a.id === form.fromAccountId)?.balance || 0, userAccounts.find(a => a.id === form.fromAccountId)?.currency) }}
                            </span>
                        </div>
                        <div class="sb-input-area">
                            <select v-model="form.fromAccountId" class="sb-select">
                                <option v-for="a in userAccounts" :key="a.id" :value="a.id">{{ a.currency }} Hesabı</option>
                            </select>
                            <input type="number" v-model.number="form.amountToExchange" class="sb-input" placeholder="0.00" />
                        </div>
                    </div>

                    <div class="swap-bridge">
                        <button class="bridge-btn" @click="toast.info('İşlem yönünü kontrol edin.')">
                            <ArrowDownUp :size="16" />
                        </button>
                    </div>

                    <div class="swap-box-v3 to">
                        <div class="sb-top">
                            <label>ALINACAK</label>
                            <select v-model="form.targetCurrency" class="sb-curr-tag">
                                <option v-for="c in currencies" :key="c" :value="c">{{ c }}</option>
                            </select>
                        </div>
                        <div class="sb-input-area">
                            <select v-model="form.toAccountId" class="sb-select">
                                <option v-for="a in userAccounts" :key="a.id" :value="a.id">{{ a.currency }} Hesabı</option>
                            </select>
                            <div class="sb-display">{{ estimatedAmount.toFixed(estimatedAmount < 1 ? 4 : 2) }}</div>
                        </div>
                    </div>
                </div>

                <div class="swap-summary-v3 mt-4">
                    <div class="ss-row">
                        <span>İşlem Kuru (1 {{ userAccounts.find(a => a.id === form.fromAccountId)?.currency || 'TRY' }})</span>
                        <strong>{{ (getRateTry(userAccounts.find(a => a.id === form.fromAccountId)?.currency || 'TRY') / getRateTry(form.targetCurrency)).toFixed(4) }} {{ form.targetCurrency }}</strong>
                    </div>
                    <div class="ss-total mt-2">
                        <span>Net Alınacak</span>
                        <h3 class="text-gradient-gold">{{ estimatedAmount.toFixed(estimatedAmount < 1 ? 4 : 2) }} {{ form.targetCurrency }}</h3>
                    </div>
                </div>

                <button @click="handleSwap" class="btn-elite-gold w-100 mt-4" :disabled="form.amountToExchange <= 0">
                    <span>Onayla ve Takas Et</span>
                    <ArrowRight :size="16" />
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.exchange-header { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 1.5rem; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 1.8rem; font-weight: 900; letter-spacing: -0.5px;
}

.live-status {
    display: flex; align-items: center; gap: 6px; background: rgba(16, 185, 129, 0.1);
    color: #10B981; padding: 6px 12px; border-radius: 100px; font-size: 0.7rem; font-weight: 800;
}
.ping-dot { width: 6px; height: 6px; background: #10B981; border-radius: 50%; animation: pulse 2s infinite; }

.exchange-grid-elite { display: grid; grid-template-columns: 1fr 1.6fr; gap: 24px; }

/* TERMINAL SIDE */
.glass-card-dark { 
    background: #0F172A; border-radius: 20px; padding: 20px; 
    border: 1px solid rgba(255,255,255,0.05);
}
.card-header-v3 { display: flex; align-items: center; gap: 8px; font-size: 0.65rem; font-weight: 800; color: rgba(255,255,255,0.4); letter-spacing: 1px; margin-bottom: 16px; text-transform: uppercase; }

.rates-list-v3 { display: flex; flex-direction: column; gap: 8px; }
.rate-item-v3 { 
    display: flex; align-items: center; gap: 12px; padding: 12px; 
    background: rgba(255,255,255,0.03); border-radius: 14px; transition: 0.2s;
}
.f-emoji { font-size: 1.5rem; }
.ri-names strong { display: block; font-size: 0.9rem; color: white; }
.ri-names span { font-size: 0.65rem; color: rgba(255,255,255,0.3); font-weight: 700; }
.p-val { font-size: 1.1rem; font-weight: 900; color: #D4AF37; font-family: 'Outfit'; }

.info-banner-gold { padding: 12px; background: #FFFBEB; border-radius: 14px; border: 1px solid #FEF3C7; }
.info-banner-gold p { font-size: 0.7rem; color: #B45309; margin: 0; }

/* SWAP SIDE */
.glass-card-elite { background: white; border-radius: 24px; padding: 24px; box-shadow: 0 10px 30px rgba(0,0,0,0.04); }

.swap-box-v3 { background: #F8FAFC; padding: 20px; border-radius: 18px; border: 1px solid #F1F5F9; }
.sb-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.sb-top label { font-size: 0.65rem; font-weight: 800; color: #64748B; }
.sb-balance { font-size: 0.7rem; font-weight: 800; color: var(--primary-dark); display: flex; align-items: center; gap: 4px; }

.sb-input-area { display: flex; justify-content: space-between; align-items: center; gap: 10px; }
.sb-select { background: transparent; border: none; font-size: 0.95rem; font-weight: 800; color: #0F172A; outline: none; }
.sb-input { background: transparent; border: none; text-align: right; font-size: 1.6rem; font-weight: 900; color: #0F172A; font-family: 'Outfit'; width: 100%; outline: none; }
.sb-display { font-size: 1.6rem; font-weight: 900; color: var(--primary-dark); font-family: 'Outfit'; }

.swap-bridge { height: 8px; display: flex; align-items: center; justify-content: center; }
.bridge-btn { 
    width: 36px; height: 36px; border-radius: 50%; background: white; 
    border: 1.5px solid #F1F5F9; display: flex; align-items: center; 
    justify-content: center; color: var(--primary-dark); cursor: pointer; transition: 0.2s;
}

.sb-curr-tag { background: var(--primary-dark); color: #D4AF37; border: none; padding: 4px 10px; border-radius: 8px; font-weight: 900; font-size: 0.75rem; }

.swap-summary-v3 { background: #F8FAFC; padding: 16px; border-radius: 16px; }
.ss-row { display: flex; justify-content: space-between; font-size: 0.8rem; font-weight: 700; color: #64748B; margin-bottom: 6px; }
.ss-total { border-top: 1px solid #F1F5F9; padding-top: 12px; display: flex; justify-content: space-between; align-items: center; }
.ss-total h3 { margin: 0; font-size: 1.4rem; }

.btn-elite-gold { 
    background: #0F172A; color: #D4AF37; border: none; padding: 16px; 
    border-radius: 16px; font-size: 0.95rem; font-weight: 900; cursor: pointer;
    display: flex; align-items: center; justify-content: center; gap: 8px;
}
.btn-elite-gold:hover:not(:disabled) { background: #000; transform: translateY(-2px); }

@keyframes pulse { 0% { opacity: 0.4; } 50% { opacity: 1; } 100% { opacity: 0.4; } }
@keyframes spinner { to { transform: rotate(360deg); } }

.loader-premium { height: 400px; display: flex; align-items: center; justify-content: center; }
.spinner-gold { width: 50px; height: 50px; border: 4px solid rgba(212, 175, 55, 0.1); border-top-color: #D4AF37; border-radius: 50%; animation: spinner 1s linear infinite; }
</style>
