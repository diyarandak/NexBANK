<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
    CreditCard, Landmark, CheckCircle2, 
    AlertCircle, Loader2, ShieldCheck, Zap
} from 'lucide-vue-next';

const toast = useToastStore();
const accounts = ref<any[]>([]);
const creditCards = ref<any[]>([]);
const selectedAccountId = ref('');
const selectedCardId = ref('');
const paymentAmount = ref<number>(0);
const loading = ref(false);
const showSuccess = ref(false);

const fetchData = async () => {
    try {
        const [accRes, cardRes] = await Promise.all([
            apiClient.get('/accounts'),
            apiClient.get('/creditcards')
        ]);
        accounts.value = accRes.data;
        creditCards.value = cardRes.data;
        
        if (accounts.value.length > 0) selectedAccountId.value = accounts.value[0].id;
        if (creditCards.value.length > 0) {
            selectedCardId.value = creditCards.value[0].id;
            paymentAmount.value = creditCards.value[0].usedAmount;
        }
    } catch (err) {
        console.error(err);
        toast.error("Veriler yüklenemedi.");
    }
};

const handlePayment = async () => {
    if (!selectedCardId.value || !selectedAccountId.value || paymentAmount.value <= 0) {
        toast.error("Lütfen tüm alanları geçerli şekilde doldurun.");
        return;
    }

    const account = accounts.value.find(a => a.id === selectedAccountId.value);
    if (account && account.balance < paymentAmount.value) {
        toast.error("Hesap bakiyesi yetersiz.");
        return;
    }

    loading.value = true;
    try {
        await apiClient.post('/creditcards/pay', {
            creditCardId: selectedCardId.value,
            accountId: selectedAccountId.value,
            amount: paymentAmount.value
        });
        
        showSuccess.value = true;
        toast.success("Ödeme başarıyla tamamlandı.");
        fetchData();
    } catch (err: any) {
        toast.error(err.response?.data?.message || "Ödeme işlemi başarısız.");
    } finally {
        loading.value = false;
    }
};

onMounted(fetchData);

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};
</script>

<template>
  <div class="view-container slide-up">
    <header class="premium-header">
      <div class="header-content">
        <div class="title-wrap">
            <span class="top-tag">KART İŞLEMLERİ</span>
            <h1 class="view-title text-gold">Kredi Kartı Borç Ödeme</h1>
            <p class="subtitle">Kart borçlarınızı vadesi geçmeden kolayca ödeyin, limitinizi anında açın.</p>
        </div>
        <div class="security-seal">
            <ShieldCheck :size="20" />
            <span>NexSecure™ Korumalı</span>
        </div>
      </div>
    </header>

    <div class="payment-grid">
        <!-- 💳 LEFT: CARD SELECTION -->
        <div class="glass-panel selection-side">
            <div class="panel-header">
                <CreditCard :size="18" class="text-gold" />
                <h3>Kart ve Ödeme Bilgileri</h3>
            </div>

            <div class="panel-body mt-4">
                <div class="f-group">
                    <label>ÖDENECEK KART SEÇİMİ</label>
                    <div class="card-selector-v2">
                        <div 
                            v-for="card in creditCards" 
                            :key="card.id"
                            class="card-option"
                            :class="{ active: selectedCardId === card.id }"
                            @click="selectedCardId = card.id; paymentAmount = card.usedAmount"
                        >
                            <div class="co-main">
                                <Zap :size="20" />
                                <div class="co-info">
                                    <strong>{{ card.cardNumber.slice(-4) }}</strong>
                                    <span>{{ card.isVirtual ? 'Sanal Kart' : 'Asıl Kart' }}</span>
                                </div>
                            </div>
                            <div class="co-debt">
                                <small>Toplam Borç</small>
                                <strong>{{ formatCurrency(card.usedAmount) }}</strong>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="f-group mt-5">
                    <label>ÖDEME YAPILACAK HESAP</label>
                    <select v-model="selectedAccountId" class="premium-select">
                        <option v-for="a in accounts" :key="a.id" :value="a.id">
                            {{ a.iban }} ({{ formatCurrency(a.balance) }})
                        </option>
                    </select>
                </div>

                <div class="f-group mt-4">
                    <label>ÖDEME TUTARI (₺)</label>
                    <div class="amount-input-wrap">
                        <input v-model.number="paymentAmount" type="number" step="0.01" />
                        <button @click="paymentAmount = creditCards.find(c => c.id === selectedCardId)?.usedAmount || 0" class="btn-all">BORCUN TAMAMI</button>
                    </div>
                </div>
            </div>
        </div>

        <!-- 💎 RIGHT: CONFIRMATION -->
        <div class="glass-panel summary-side">
            <div v-if="showSuccess" class="state-container success fade-in">
                <div class="success-glow">
                    <CheckCircle2 :size="72" />
                </div>
                <h3 class="text-gold">Ödeme Başarılı</h3>
                <p>Kredi kartı borcunuz başarıyla ödendi. Limitiniz güncellenmiştir.</p>
                <button @click="showSuccess = false" class="btn-premium btn-gold w-full mt-5">YENİ ÖDEME YAP</button>
            </div>

            <div v-else class="payment-summary">
                <div class="summary-header">
                    <h3>İşlem Özeti</h3>
                </div>
                
                <div class="summary-rows mt-4">
                    <div class="s-row">
                        <span>İşlem Tipi</span>
                        <span>Kart Borç Ödeme</span>
                    </div>
                    <div class="s-row">
                        <span>Seçili Kart</span>
                        <span>**** {{ creditCards.find(c => c.id === selectedCardId)?.cardNumber.slice(-4) }}</span>
                    </div>
                    <div class="s-row">
                        <span>Kaynak Hesap</span>
                        <span>{{ accounts.find(a => a.id === selectedAccountId)?.accountType }}</span>
                    </div>
                    <div class="s-row total mt-4">
                        <span>TOPLAM ÖDEME</span>
                        <strong>{{ formatCurrency(paymentAmount) }}</strong>
                    </div>
                </div>

                <div class="info-box mt-5">
                    <AlertCircle :size="16" />
                    <p>Ödeme işlemi gerçekleştikten sonra kart limitiniz anında güncellenecektir.</p>
                </div>

                <button @click="handlePayment" class="btn-confirm-payment mt-5" :disabled="loading || paymentAmount <= 0">
                    <Loader2 v-if="loading" class="spin" :size="20" />
                    <span v-else>ÖDEMEYİ ONAYLA VE BİTİR</span>
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.premium-header { margin-bottom: 3rem; }
.top-tag { font-size: 0.7rem; font-weight: 800; color: var(--gold); letter-spacing: 2px; display: block; margin-bottom: 8px; }
.text-gold { background: linear-gradient(135deg, #F9F295 0%, #C5A03A 50%, #B88A44 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; font-weight: 900; }
.header-content { display: flex; justify-content: space-between; align-items: flex-end; }
.security-seal { display: flex; align-items: center; gap: 8px; background: rgba(12, 27, 51, 0.6); border: 1px solid rgba(197, 160, 58, 0.3); padding: 8px 16px; border-radius: 12px; color: var(--gold); font-size: 0.75rem; font-weight: 700; }

.payment-grid { display: grid; grid-template-columns: 1fr 420px; gap: 32px; }
.glass-panel { background: rgba(255, 255, 255, 0.03); border: 1px solid rgba(255, 255, 255, 0.05); border-radius: 32px; backdrop-filter: blur(10px); padding: 32px; min-height: 500px; }
.panel-header { display: flex; align-items: center; gap: 12px; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 20px; }
.panel-header h3 { margin: 0; font-size: 1.1rem; font-weight: 800; color: white; }

.f-group label { display: block; font-size: 0.75rem; font-weight: 800; color: #94A3B8; margin-bottom: 12px; }

.card-selector-v2 { display: flex; flex-direction: column; gap: 12px; }
.card-option { background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05); padding: 16px 20px; border-radius: 16px; display: flex; justify-content: space-between; align-items: center; cursor: pointer; transition: 0.3s; }
.card-option:hover { background: rgba(255,255,255,0.05); border-color: var(--gold); }
.card-option.active { background: var(--primary-dark); border-color: var(--gold); }
.co-main { display: flex; align-items: center; gap: 16px; color: var(--gold); }
.co-info strong { display: block; color: white; font-size: 1rem; }
.co-info span { font-size: 0.75rem; color: #94A3B8; font-weight: 600; }
.co-debt { text-align: right; }
.co-debt small { display: block; font-size: 0.65rem; color: #94A3B8; font-weight: 700; }
.co-debt strong { color: white; font-size: 0.95rem; }

.premium-select { width: 100%; padding: 16px; background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05); border-radius: 16px; color: white; font-weight: 700; outline: none; }
.premium-select option { background: #0C1B33; color: white; }

.amount-input-wrap { display: flex; background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05); border-radius: 16px; padding: 4px; }
.amount-input-wrap input { flex: 1; background: transparent; border: none; padding: 12px 16px; color: white; font-weight: 800; font-size: 1.2rem; outline: none; }
.btn-all { background: #0C1B33; color: var(--gold); border: 1px solid var(--gold); padding: 0 16px; border-radius: 12px; font-weight: 800; font-size: 0.7rem; cursor: pointer; }

.s-row { display: flex; justify-content: space-between; font-weight: 700; font-size: 0.9rem; color: #94A3B8; margin-bottom: 12px; }
.s-row strong { color: white; font-size: 1.4rem; font-family: 'Outfit'; }
.s-row.total { border-top: 1px solid rgba(255,255,255,0.05); padding-top: 20px; color: var(--gold); }

.info-box { background: rgba(197, 160, 58, 0.05); border: 1px solid rgba(197, 160, 58, 0.2); padding: 16px; border-radius: 16px; display: flex; gap: 12px; align-items: flex-start; }
.info-box p { font-size: 0.75rem; color: #94A3B8; font-weight: 600; margin: 0; line-height: 1.4; }

.btn-confirm-payment { width: 100%; padding: 20px; background: var(--primary); color: var(--primary-dark); border: none; border-radius: 20px; font-weight: 900; font-size: 1.1rem; cursor: pointer; transition: 0.3s; box-shadow: 0 10px 30px rgba(197, 160, 58, 0.2); }
.btn-confirm-payment:hover:not(:disabled) { transform: translateY(-5px); box-shadow: 0 15px 40px rgba(197, 160, 58, 0.3); }

.success-glow { width: 120px; height: 120px; background: rgba(16, 185, 129, 0.1); border-radius: 50%; display: flex; align-items: center; justify-content: center; color: #10B981; margin-bottom: 32px; box-shadow: 0 0 50px rgba(16, 185, 129, 0.2); }
.state-container { display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; height: 100%; }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
