<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
  TrendingUp, TrendingDown, ShieldCheck, 
  PieChart, Activity, Wallet, ArrowRight,
  Info, ChevronRight, Filter, Search
} from 'lucide-vue-next';

const toast = useToastStore();
const loading = ref(true);
const userAccounts = ref<any[]>([]);
const activeTab = ref('all'); // all, tech, gold, money

const funds = ref([
  { id: 1, code: 'NXB-TEC', name: 'NexBank Teknoloji Fonu', price: 145.67, dailyReturn: 2.34, risk: 6, category: 'tech', desc: 'Yapay zeka ve global teknoloji devlerine odaklanır.' },
  { id: 2, code: 'NXB-GLD', name: 'NexBank Altın Fonu', price: 890.12, dailyReturn: -0.45, risk: 4, category: 'gold', desc: 'Fiziki altına dayalı, düşük riskli emtia fonu.' },
  { id: 3, code: 'NXB-PAR', name: 'Para Piyasası Fonu', price: 10.55, dailyReturn: 0.05, risk: 1, category: 'money', desc: 'Gecelik faiz ve repo piyasasında likit getiri.' },
  { id: 4, code: 'NXB-BRK', name: 'Birleşik Getiri Fonu', price: 45.20, dailyReturn: 1.12, risk: 5, category: 'all', desc: 'Dengeli hisse ve tahvil karma portföyü.' },
  { id: 5, code: 'NXB-SUS', name: 'Sürdürülebilir Enerji', price: 88.90, dailyReturn: 3.15, risk: 6, category: 'tech', desc: 'Yeşil enerji ve ESG uyumlu global şirketler.' },
]);

// MODAL STATE
const showTradeModal = ref(false);
const selectedFund = ref<any>(null);
const tradeType = ref<'buy' | 'sell'>('buy');
const tradeForm = ref({
    accountId: '',
    amount: 0
});

const fetchData = async () => {
    try {
        const accRes = await apiClient.get('/accounts');
        userAccounts.value = accRes.data;
        if (userAccounts.value.length > 0) {
            tradeForm.value.accountId = userAccounts.value[0].id;
        }
    } catch (err) {
        console.error('Veri yükleme hatası');
    } finally {
        loading.value = false;
    }
};

const filteredFunds = computed(() => {
    if (activeTab.value === 'all') return funds.value;
    return funds.value.filter(f => f.category === activeTab.value);
});

const openTrade = (fund: any, type: 'buy' | 'sell') => {
    selectedFund.value = fund;
    tradeType.value = type;
    showTradeModal.value = true;
};

const handleTrade = async () => {
    if (tradeForm.value.amount <= 0) {
        toast.error('Lütfen geçerli bir tutar girin.');
        return;
    }

    try {
        // Fon alım/satım işlemi simülasyonu veya API çağrısı
        // await apiClient.post('/funds/trade', { ... });
        toast.success(`${selectedFund.value.code} fonu için ${tradeType.value === 'buy' ? 'alım' : 'satım'} talebiniz alındı.`);
        showTradeModal.value = false;
        tradeForm.value.amount = 0;
    } catch (err: any) {
        toast.error('İşlem gerçekleştirilemedi.');
    }
};

const getRiskColor = (risk: number) => {
    if (risk <= 2) return '#10B981'; // Düşük
    if (risk <= 4) return '#F59E0B'; // Orta
    return '#EF4444'; // Yüksek
};

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

onMounted(fetchData);
</script>

<template>
  <div class="view-container fade-in">
    <header class="funds-header mb-4">
      <div class="h-left">
        <h1 class="text-gradient-gold">Fon Yönetimi</h1>
        <p class="subtitle">NexBank Asset Management ile birikimlerinizi profesyonellere emanet edin.</p>
      </div>
      <div class="h-right">
        <div class="portfolio-mini-card">
            <div class="pm-info">
                <span>PORTFÖY DEĞERİ</span>
                <strong>{{ formatCurrency(124500.75) }}</strong>
            </div>
            <div class="pm-trend up">
                <TrendingUp :size="14" />
                +4.2%
            </div>
        </div>
      </div>
    </header>

    <!-- TABS -->
    <div class="funds-nav mb-4">
        <button v-for="t in ['all', 'tech', 'gold', 'money']" :key="t" 
                @click="activeTab = t" 
                :class="{ active: activeTab === t }">
            {{ t === 'all' ? 'Tüm Fonlar' : t === 'tech' ? 'Teknoloji' : t === 'gold' ? 'Altın/Emtia' : 'Para Piyasası' }}
        </button>
    </div>

    <div v-if="loading" class="loader-premium"><div class="spinner-gold"></div></div>

    <div v-else class="funds-grid">
        <div v-for="fund in filteredFunds" :key="fund.id" class="fund-card-premium">
            <div class="fc-header">
                <div class="fc-code-box">
                    <span class="fc-code">{{ fund.code }}</span>
                    <span class="fc-category" :class="fund.category">{{ fund.category.toUpperCase() }}</span>
                </div>
                <div class="fc-risk">
                    <div class="risk-dots">
                        <span v-for="i in 7" :key="i" :style="{ background: i <= fund.risk ? getRiskColor(fund.risk) : '#E2E8F0' }"></span>
                    </div>
                    <small>Risk: {{ fund.risk }}/7</small>
                </div>
            </div>
            
            <div class="fc-body">
                <h3>{{ fund.name }}</h3>
                <p class="fc-desc">{{ fund.desc }}</p>
                
                <div class="fc-stats">
                    <div class="fs-item">
                        <small>PAY FİYATI</small>
                        <strong>{{ fund.price.toFixed(4) }} ₺</strong>
                    </div>
                    <div class="fs-item">
                        <small>GÜNLÜK GETİRİ</small>
                        <strong :class="fund.dailyReturn >= 0 ? 'text-success' : 'text-danger'">
                            {{ fund.dailyReturn > 0 ? '+' : '' }}{{ fund.dailyReturn }}%
                        </strong>
                    </div>
                </div>
            </div>

            <div class="fc-footer">
                <button @click="openTrade(fund, 'buy')" class="btn-fund buy">AL</button>
                <button @click="openTrade(fund, 'sell')" class="btn-fund sell">SAT</button>
            </div>
        </div>
    </div>

    <!-- TRADE MODAL -->
    <div v-if="showTradeModal" class="modal-overlay" @click.self="showTradeModal = false">
        <div class="trade-modal glass-card-elite scale-up">
            <div class="tm-header">
                <h2>{{ tradeType === 'buy' ? 'Fon Alımı' : 'Fon Satışı' }}</h2>
                <button class="close-btn" @click="showTradeModal = false">&times;</button>
            </div>
            
            <div class="tm-fund-info">
                <div class="tmf-left">
                    <strong>{{ selectedFund.code }}</strong>
                    <span>{{ selectedFund.name }}</span>
                </div>
                <div class="tmf-right">
                    <small>Birim Fiyat</small>
                    <strong>{{ selectedFund.price.toFixed(4) }} ₺</strong>
                </div>
            </div>

            <div class="tm-form mt-4">
                <div class="input-group-premium">
                    <label>HESAP SEÇİMİ</label>
                    <select v-model="tradeForm.accountId">
                        <option v-for="a in userAccounts" :key="a.id" :value="a.id">
                            {{ a.currency }} {{ a.accountType === 'Individual' ? 'Vadesiz' : 'Birikim' }} ({{ formatCurrency(a.balance) }})
                        </option>
                    </select>
                </div>

                <div class="input-group-premium mt-3">
                    <label>İŞLEM TUTARI (TL)</label>
                    <div class="amount-input-wrapper">
                        <input type="number" v-model.number="tradeForm.amount" placeholder="0.00" />
                        <span>TL</span>
                    </div>
                </div>

                <div class="tm-summary mt-4">
                    <div class="tms-row">
                        <span>Tahmini Pay Adedi</span>
                        <strong>{{ (tradeForm.amount / selectedFund.price).toFixed(2) }} Adet</strong>
                    </div>
                    <div class="tms-row">
                        <span>Komisyon</span>
                        <span class="text-success">Ücretsiz</span>
                    </div>
                </div>

                <button @click="handleTrade" :class="['btn-elite-gold w-100 mt-4', tradeType]">
                    {{ tradeType === 'buy' ? 'Alımı Onayla' : 'Satışı Onayla' }}
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.funds-header { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2rem; font-weight: 900;
}

.portfolio-mini-card { 
    background: #0F172A; padding: 16px 24px; border-radius: 20px; 
    display: flex; align-items: center; gap: 20px; color: white;
}
.pm-info span { display: block; font-size: 0.65rem; color: rgba(255,255,255,0.4); font-weight: 800; letter-spacing: 1px; }
.pm-info strong { font-size: 1.2rem; color: #D4AF37; font-family: 'Outfit'; }
.pm-trend { display: flex; align-items: center; gap: 4px; font-size: 0.75rem; font-weight: 800; padding: 4px 10px; border-radius: 100px; }
.pm-trend.up { background: rgba(16, 185, 129, 0.1); color: #10B981; }

.funds-nav { display: flex; gap: 12px; }
.funds-nav button { 
    background: white; border: 1px solid #F1F5F9; padding: 8px 16px; 
    border-radius: 12px; font-size: 0.85rem; font-weight: 700; color: #64748B; cursor: pointer; transition: 0.2s;
}
.funds-nav button.active { background: #0F172A; color: #D4AF37; border-color: #0F172A; }

.funds-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 24px; }

.fund-card-premium { 
    background: white; border-radius: 24px; padding: 24px; border: 1px solid #F1F5F9;
    box-shadow: 0 10px 20px rgba(0,0,0,0.02); transition: 0.3s; display: flex; flex-direction: column;
}
.fund-card-premium:hover { transform: translateY(-5px); box-shadow: 0 20px 40px rgba(0,0,0,0.05); }

.fc-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
.fc-code-box { display: flex; flex-direction: column; gap: 4px; }
.fc-code { font-size: 0.9rem; font-weight: 900; color: #0F172A; }
.fc-category { font-size: 0.6rem; font-weight: 800; padding: 2px 8px; border-radius: 4px; }
.fc-category.tech { background: #E0F2FE; color: #0369A1; }
.fc-category.gold { background: #FEF3C7; color: #B45309; }
.fc-category.money { background: #DCFCE7; color: #15803D; }

.risk-dots { display: flex; gap: 3px; margin-bottom: 4px; }
.risk-dots span { width: 8px; height: 8px; border-radius: 50%; }
.fc-risk small { font-size: 0.65rem; font-weight: 700; color: #94A3B8; }

.fc-body h3 { font-size: 1.1rem; font-weight: 800; color: #0F172A; margin-bottom: 8px; }
.fc-desc { font-size: 0.8rem; color: #64748B; line-height: 1.5; margin-bottom: 20px; flex: 1; }

.fc-stats { display: flex; gap: 24px; background: #F8FAFC; padding: 12px; border-radius: 16px; }
.fs-item small { display: block; font-size: 0.6rem; font-weight: 800; color: #94A3B8; margin-bottom: 4px; }
.fs-item strong { font-size: 0.95rem; color: #0F172A; font-family: 'Outfit'; }

.fc-footer { display: flex; gap: 12px; margin-top: 24px; }
.btn-fund { flex: 1; padding: 12px; border-radius: 14px; font-weight: 800; font-size: 0.85rem; cursor: pointer; transition: 0.2s; border: none; }
.btn-fund.buy { background: #0F172A; color: #D4AF37; }
.btn-fund.sell { background: #F1F5F9; color: #64748B; }
.btn-fund:hover { transform: translateY(-2px); opacity: 0.9; }

/* MODAL */
.modal-overlay { 
    position: fixed; top: 0; left: 0; width: 100%; height: 100%; 
    background: rgba(15, 23, 42, 0.4); backdrop-filter: blur(8px);
    display: flex; align-items: center; justify-content: center; z-index: 1000;
}
.trade-modal { width: 100%; max-width: 440px; background: white; border-radius: 32px; padding: 32px; }

.tm-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.tm-header h2 { font-size: 1.5rem; font-weight: 900; color: #0F172A; }
.close-btn { background: none; border: none; font-size: 2rem; color: #94A3B8; cursor: pointer; }

.tm-fund-info { background: #F8FAFC; padding: 20px; border-radius: 20px; display: flex; justify-content: space-between; align-items: center; }
.tmf-left strong { display: block; font-size: 1.1rem; color: #0F172A; }
.tmf-left span { font-size: 0.8rem; color: #64748B; }
.tmf-right { text-align: right; }
.tmf-right small { display: block; font-size: 0.65rem; font-weight: 800; color: #94A3B8; }
.tmf-right strong { font-size: 1.1rem; color: #D4AF37; font-family: 'Outfit'; }

.input-group-premium label { display: block; font-size: 0.65rem; font-weight: 800; color: #94A3B8; margin-bottom: 8px; letter-spacing: 1px; }
.input-group-premium select, .input-group-premium input { 
    width: 100%; padding: 16px; border-radius: 16px; border: 1px solid #F1F5F9; 
    background: #F8FAFC; font-size: 0.95rem; font-weight: 700; color: #0F172A; outline: none;
}
.amount-input-wrapper { position: relative; }
.amount-input-wrapper span { position: absolute; right: 16px; top: 50%; transform: translateY(-50%); font-weight: 900; color: #D4AF37; }

.tm-summary { background: #F1F5F9; padding: 16px; border-radius: 16px; }
.tms-row { display: flex; justify-content: space-between; font-size: 0.85rem; font-weight: 700; color: #64748B; margin-bottom: 8px; }
.tms-row strong { color: #0F172A; }

.btn-elite-gold { 
    background: #0F172A; color: #D4AF37; border: none; padding: 18px; 
    border-radius: 18px; font-size: 1.1rem; font-weight: 900; cursor: pointer;
}
.btn-elite-gold.sell { background: #F1F5F9; color: #0F172A; }

@keyframes spinner { to { transform: rotate(360deg); } }
.loader-premium { height: 400px; display: flex; align-items: center; justify-content: center; }
.spinner-gold { width: 50px; height: 50px; border: 4px solid rgba(212, 175, 55, 0.1); border-top-color: #D4AF37; border-radius: 50%; animation: spinner 1s linear infinite; }
</style>
