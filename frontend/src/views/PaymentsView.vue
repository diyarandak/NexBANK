<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
    Search, Landmark, Receipt, CreditCard, 
    CheckCircle2, AlertCircle, Loader2, History,
    ShieldCheck, Zap, Droplets, Flame, Globe, Phone
} from 'lucide-vue-next';

const toast = useToastStore();
const accounts = ref<any[]>([]);
const selectedAccountId = ref('');
const loading = ref(false);
const inquiryLoading = ref(false);
const showSuccess = ref(false);

const form = ref({
    institution: 'Elektrik',
    subscriberNo: ''
});

const queriedBill = ref<any>(null);

const institutions = [
    { name: 'Elektrik', providers: ['CK Boğaziçi', 'EnerjiSA', 'Aydem'], icon: Zap, color: '#FFD700' },
    { name: 'Su', providers: ['İSKİ', 'ASKİ', 'İZSU'], icon: Droplets, color: '#3B82F6' },
    { name: 'Doğalgaz', providers: ['İGDAŞ', 'Başkentgaz', 'Aksa'], icon: Flame, color: '#EF4444' },
    { name: 'İnternet', providers: ['Türk Telekom', 'Turkcell Superonline', 'Türksat Kablo'], icon: Globe, color: '#8B5CF6' },
    { name: 'Telefon', providers: ['Türk Telekom Sabit', 'Vodafone Ev', 'Millenicom'], icon: Phone, color: '#10B981' }
];

const fetchAccounts = async () => {
    try {
        const res = await apiClient.get('/accounts');
        accounts.value = res.data;
        if (accounts.value.length > 0) selectedAccountId.value = accounts.value[0].id;
    } catch (err) { console.error(err); }
};

const handleInquiry = () => {
    if (form.value.subscriberNo.length < 5) {
        toast.error("Lütfen geçerli bir abone numarası girin.");
        return;
    }

    inquiryLoading.value = true;
    queriedBill.value = null;
    showSuccess.value = false;

    setTimeout(() => {
        const inst = institutions.find(i => i.name === form.value.institution);
        const randomProvider = inst?.providers[Math.floor(Math.random() * inst.providers.length)];
        
        queriedBill.value = {
            id: Date.now(),
            type: form.value.institution,
            provider: randomProvider,
            subscriberNo: form.value.subscriberNo,
            amount: +(Math.random() * 1200 + 150).toFixed(2),
            dueDate: new Date(Date.now() + 1000 * 60 * 60 * 24 * 5).toLocaleDateString('tr-TR'),
            icon: inst?.icon
        };
        inquiryLoading.value = false;
        toast.success("Borç bilgileri başarıyla çekildi.");
    }, 1500);
};

const handlePay = async () => {
    if (!selectedAccountId.value) {
        toast.error("Ödeme yapılacak hesabı seçiniz.");
        return;
    }
    
    const activeAcc = accounts.value.find(a => a.id === selectedAccountId.value);
    if (activeAcc && activeAcc.balance < queriedBill.value.amount) {
        toast.error("Yetersiz bakiye.");
        return;
    }

    loading.value = true;
    try {
        const userStr = localStorage.getItem('user');
        const userId = userStr ? JSON.parse(userStr).id : null;
        
        const newTransaction = {
            id: 'bill-' + Date.now(),
            type: 'Withdrawal',
            description: `${queriedBill.value.type} Faturası: ${queriedBill.value.provider}`,
            amount: queriedBill.value.amount,
            createdAt: new Date().toISOString()
        };

        if (userId) {
            const storageKey = `manual_transactions_${userId}`;
            const manualTx = JSON.parse(localStorage.getItem(storageKey) || '[]');
            manualTx.unshift(newTransaction);
            localStorage.setItem(storageKey, JSON.stringify(manualTx));
        }

        if (activeAcc) activeAcc.balance -= queriedBill.value.amount;
        
        showSuccess.value = true;
        queriedBill.value = null;
        form.value.subscriberNo = '';
        toast.success("Ödeme işlemi tamamlandı.");
    } catch (err: any) {
        toast.error('Hata oluştu.');
    } finally {
        loading.value = false;
    }
};

onMounted(fetchAccounts);
</script>

<template>
  <div class="view-container slide-up">
    <header class="premium-header">
      <div class="header-content">
        <div class="title-wrap">
            <span class="top-tag">FİNANSAL İŞLEMLER</span>
            <h1 class="view-title text-gold">Fatura Ödeme Merkezi</h1>
            <p class="subtitle">NexBank güvencesiyle faturalarınızı sorgulayın ve anında ödeyin.</p>
        </div>
        <div class="security-seal">
            <ShieldCheck :size="20" />
            <span>NexSecure™ Korumalı</span>
        </div>
      </div>
    </header>

    <div class="payments-main-grid">
        <!-- ⚡ LEFT: SELECTION & INQUIRY -->
        <div class="glass-panel inquiry-side">
            <div class="panel-header">
                <Search :size="18" class="text-gold" />
                <h3>Kurum ve Abone Bilgileri</h3>
            </div>

            <div class="panel-body mt-4">
                <div class="field-label">KURUM TİPİ SEÇİNİZ</div>
                <div class="institution-grid">
                    <div 
                        v-for="inst in institutions" 
                        :key="inst.name"
                        class="inst-card"
                        :class="{ active: form.institution === inst.name }"
                        @click="form.institution = inst.name"
                    >
                        <component :is="inst.icon" :size="24" class="inst-icon" :style="{ color: form.institution === inst.name ? 'var(--primary-dark)' : inst.color }" />
                        <span>{{ inst.name }}</span>
                        <div class="active-dot" v-if="form.institution === inst.name"></div>
                    </div>
                </div>

                <div class="subscriber-field mt-5">
                    <label>ABONE / SÖZLEŞME NUMARASI</label>
                    <div class="premium-input-wrap">
                        <Receipt :size="20" class="input-icon" />
                        <input 
                            v-model="form.subscriberNo" 
                            type="text" 
                            placeholder="Müşteri No giriniz..."
                            @keyup.enter="handleInquiry"
                        />
                        <button 
                            @click="handleInquiry" 
                            class="inline-query-btn"
                            :disabled="inquiryLoading"
                        >
                            <Loader2 v-if="inquiryLoading" class="spin" :size="18" />
                            <span v-else>SORGULA</span>
                        </button>
                    </div>
                    <div class="hint-text mt-3">
                        <AlertCircle :size="12" />
                        <span>Sorgulama işlemi için güncel abone numaranızı kullanın.</span>
                    </div>
                </div>
            </div>
        </div>

        <!-- 💎 RIGHT: RESULTS & ACTION -->
        <div class="glass-panel result-side">
            <!-- IDLE STATE -->
            <div v-if="!queriedBill && !inquiryLoading && !showSuccess" class="state-container idle">
                <div class="idle-illustration">
                    <div class="pulse-ring"></div>
                    <Receipt :size="48" class="text-gold" />
                </div>
                <h4>Sorgulama Bekleniyor</h4>
                <p>Lütfen sol taraftan kurum seçimi yaparak abone numaranızı giriniz.</p>
            </div>

            <!-- LOADING STATE -->
            <div v-else-if="inquiryLoading" class="state-container loading">
                <div class="loader-visual">
                    <div class="bar"></div>
                    <div class="bar"></div>
                    <div class="bar"></div>
                </div>
                <h4>Veriler İşleniyor</h4>
                <p>Merkezi fatura sistemine bağlanılıyor, lütfen bekleyiniz...</p>
            </div>

            <!-- SUCCESS STATE -->
            <div v-else-if="showSuccess" class="state-container success fade-in">
                <div class="success-glow">
                    <CheckCircle2 :size="72" />
                </div>
                <h3 class="text-gold">Ödeme Tamamlandı</h3>
                <p>İşleminiz başarıyla gerçekleştirildi. Dekontunuz hazırlandı.</p>
                <div class="success-actions mt-5">
                    <button @click="showSuccess = false" class="btn-premium btn-gold w-full">YENİ İŞLEM YAP</button>
                    <button class="btn-premium btn-outline w-full mt-3">DEKONT İNDİR (PDF)</button>
                </div>
            </div>

            <!-- BILL DETAILS -->
            <div v-else-if="queriedBill" class="bill-details-premium fade-in">
                <div class="bill-header-premium">
                    <div class="bh-main">
                        <div class="bh-icon-box">
                            <component :is="queriedBill.icon" :size="28" />
                        </div>
                        <div class="bh-info">
                            <h3>{{ queriedBill.provider }}</h3>
                            <span class="tag-gold">{{ queriedBill.type }} Ödemesi</span>
                        </div>
                    </div>
                    <div class="bh-amount">
                        <span class="label">ÖDENECEK TUTAR</span>
                        <div class="amount-val">{{ queriedBill.amount.toFixed(2) }} <span>₺</span></div>
                    </div>
                </div>

                <div class="details-grid-premium mt-4">
                    <div class="det-item">
                        <label>ABONE NO</label>
                        <span>{{ queriedBill.subscriberNo }}</span>
                    </div>
                    <div class="det-item">
                        <label>SON ÖDEME</label>
                        <span class="text-danger">{{ queriedBill.dueDate }}</span>
                    </div>
                    <div class="det-item">
                        <label>DÖNEM</label>
                        <span>Mayıs 2026</span>
                    </div>
                    <div class="det-item">
                        <label>DURUM</label>
                        <span class="text-success">Ödenmemiş</span>
                    </div>
                </div>

                <div class="account-selector-premium mt-5">
                    <label>ÖDEME YAPILACAK HESAP</label>
                    <div class="custom-select-wrap">
                        <CreditCard :size="18" class="select-icon" />
                        <select v-model="selectedAccountId">
                            <option v-for="a in accounts" :key="a.id" :value="a.id">
                                {{ a.iban }} ({{ a.balance.toLocaleString() }} {{ a.currency }})
                            </option>
                        </select>
                    </div>
                </div>

                <button @click="handlePay" class="btn-confirm-payment mt-5" :disabled="loading">
                    <Loader2 v-if="loading" class="spin" :size="20" />
                    <span v-else>ÖDEMEYİ GÜVENLE ONAYLA</span>
                </button>
                
                <p class="security-hint mt-3">
                    <ShieldCheck :size="12" />
                    Bu işlem 256-bit SSL ile uçtan uca şifrelenmiştir.
                </p>
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

.payments-main-grid { display: grid; grid-template-columns: 1fr 480px; gap: 32px; }

.glass-panel { background: rgba(255, 255, 255, 0.03); border: 1px solid rgba(255, 255, 255, 0.05); border-radius: 32px; backdrop-filter: blur(10px); padding: 32px; min-height: 550px; }
.panel-header { display: flex; align-items: center; gap: 12px; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 20px; }
.panel-header h3 { margin: 0; font-size: 1.1rem; font-weight: 800; color: white; }

.field-label { font-size: 0.75rem; font-weight: 800; color: #94A3B8; margin-bottom: 20px; }

/* Institution Grid */
.institution-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
.inst-card { background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05); border-radius: 20px; padding: 24px 16px; display: flex; flex-direction: column; align-items: center; gap: 12px; cursor: pointer; transition: 0.3s; position: relative; overflow: hidden; }
.inst-card span { font-size: 0.8rem; font-weight: 700; color: #94A3B8; }
.inst-card:hover { background: rgba(255,255,255,0.05); transform: translateY(-5px); border-color: rgba(197, 160, 58, 0.3); }
.inst-card.active { background: linear-gradient(135deg, #F9F295 0%, #C5A03A 100%); border: none; }
.inst-card.active span { color: var(--primary-dark); font-weight: 900; }
.active-dot { position: absolute; bottom: 8px; width: 4px; height: 4px; background: var(--primary-dark); border-radius: 50%; }

/* Input Styles */
.premium-input-wrap { position: relative; background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05); border-radius: 18px; display: flex; align-items: center; padding: 4px 4px 4px 20px; transition: 0.3s; }
.premium-input-wrap:focus-within { border-color: var(--gold); background: rgba(255,255,255,0.04); box-shadow: 0 0 20px rgba(197, 160, 58, 0.1); }
.input-icon { color: var(--gold); opacity: 0.6; }
.premium-input-wrap input { background: white; border: none; flex: 1; padding: 16px 12px; color: var(--primary-dark); font-weight: 700; outline: none; font-size: 1rem; border-radius: 12px; }
.premium-input-wrap input::placeholder { color: #94A3B8; }

.inline-query-btn { background: #0C1B33; color: var(--gold); border: 1px solid var(--gold); padding: 12px 24px; border-radius: 14px; font-weight: 800; font-size: 0.85rem; cursor: pointer; transition: 0.2s; }
.inline-query-btn:hover { background: var(--gold); color: var(--primary-dark); }

.hint-text { display: flex; align-items: center; gap: 8px; color: #64748B; font-size: 0.7rem; font-weight: 700; }

/* Result States */
.state-container { height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; }
.idle-illustration { position: relative; width: 100px; height: 100px; display: flex; align-items: center; justify-content: center; margin-bottom: 24px; }
.pulse-ring { position: absolute; width: 100%; height: 100%; border: 2px solid var(--gold); border-radius: 50%; animation: ring-pulse 2s infinite; opacity: 0.3; }
@keyframes ring-pulse { 0% { transform: scale(0.6); opacity: 0.5; } 100% { transform: scale(1.4); opacity: 0; } }

.state-container h4 { color: white; margin-bottom: 12px; font-weight: 800; font-size: 1.2rem; }
.state-container p { color: #94A3B8; font-weight: 600; font-size: 0.9rem; max-width: 280px; }

/* Loading Visual */
.loader-visual { display: flex; gap: 6px; margin-bottom: 24px; }
.loader-visual .bar { width: 4px; height: 24px; background: var(--gold); border-radius: 10px; animation: bar-load 1s infinite alternate; }
.loader-visual .bar:nth-child(2) { animation-delay: 0.2s; }
.loader-visual .bar:nth-child(3) { animation-delay: 0.4s; }
@keyframes bar-load { 0% { transform: scaleY(0.5); opacity: 0.3; } 100% { transform: scaleY(1.5); opacity: 1; } }

/* Bill Details Premium */
.bill-header-premium { background: rgba(255,255,255,0.03); border-radius: 24px; padding: 24px; border: 1px solid rgba(255,255,255,0.05); }
.bh-main { display: flex; align-items: center; gap: 16px; margin-bottom: 20px; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 20px; }
.bh-icon-box { width: 60px; height: 60px; background: #0C1B33; border: 1px solid var(--gold); border-radius: 18px; display: flex; align-items: center; justify-content: center; color: var(--gold); }
.bh-info h3 { margin: 0; font-size: 1.3rem; font-weight: 900; color: white; }
.tag-gold { font-size: 0.65rem; font-weight: 800; color: var(--gold); background: rgba(197, 160, 58, 0.1); padding: 4px 10px; border-radius: 6px; }

.bh-amount { text-align: center; }
.bh-amount .label { font-size: 0.7rem; font-weight: 800; color: #94A3B8; letter-spacing: 1px; }
.amount-val { font-size: 2.2rem; font-weight: 900; color: white; font-family: 'Outfit'; }
.amount-val span { font-size: 1.2rem; color: var(--gold); }

.details-grid-premium { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.det-item { background: rgba(255,255,255,0.02); padding: 16px; border-radius: 16px; border: 1px solid rgba(255,255,255,0.03); }
.det-item label { display: block; font-size: 0.65rem; font-weight: 800; color: #64748B; margin-bottom: 4px; }
.det-item span { font-size: 0.9rem; font-weight: 800; color: white; }

.custom-select-wrap { position: relative; background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.05); border-radius: 16px; padding: 4px 16px; display: flex; align-items: center; gap: 12px; }
.select-icon { color: var(--gold); }
.custom-select-wrap select { background: transparent; border: none; flex: 1; padding: 12px 0; color: white; font-weight: 700; outline: none; cursor: pointer; }
.custom-select-wrap select option { background: #0C1B33; color: white; }

.btn-confirm-payment { width: 100%; padding: 20px; background: linear-gradient(135deg, #10B981, #059669); color: white; border: none; border-radius: 20px; font-weight: 900; font-size: 1.1rem; cursor: pointer; transition: 0.3s; box-shadow: 0 10px 30px rgba(16, 185, 129, 0.2); }
.btn-confirm-payment:hover { transform: translateY(-5px); box-shadow: 0 15px 40px rgba(16, 185, 129, 0.3); }

.security-hint { font-size: 0.65rem; font-weight: 700; color: #4B5563; display: flex; align-items: center; justify-content: center; gap: 6px; }

.success-glow { width: 120px; height: 120px; background: rgba(16, 185, 129, 0.1); border-radius: 50%; display: flex; align-items: center; justify-content: center; color: #10B981; margin-bottom: 32px; box-shadow: 0 0 50px rgba(16, 185, 129, 0.2); animation: success-pulse 2s infinite; }
@keyframes success-pulse { 0% { transform: scale(1); box-shadow: 0 0 30px rgba(16, 185, 129, 0.2); } 50% { transform: scale(1.05); box-shadow: 0 0 60px rgba(16, 185, 129, 0.4); } 100% { transform: scale(1); box-shadow: 0 0 30px rgba(16, 185, 129, 0.2); } }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
