<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
  Calculator, History, CheckCircle2, 
  AlertCircle, ShieldCheck, TrendingUp, Info,
  ChevronRight, ArrowRight, DollarSign, Clock,
  FileText, Zap, Wallet
} from 'lucide-vue-next';

const toast = useToastStore();
const accounts = ref<any[]>([]);
const loans = ref<any[]>([]);
const loading = ref(true);
const isApplying = ref(false);

const form = ref({
  accountId: '',
  amount: 25000,
  term: 12,
  loanType: 'Personal' // Varsayılan backend değeri
});

const loanTypeDisplay = ref('İhtiyaç Kredisi');

const loanTypeMap: Record<string, string> = {
    'İhtiyaç Kredisi': 'Personal',
    'Konut Kredisi': 'Home',
    'Taşıt Kredisi': 'Car'
};

const liveRate = ref(0.035);

const fetchLoansData = async () => {
    try {
        const [loanRes, rateRes] = await Promise.all([
            apiClient.get('/Loans'),
            apiClient.get('/BankSettings/public-rates')
        ]);
        loans.value = Array.isArray(loanRes.data) ? loanRes.data : [];
        if (rateRes.data && rateRes.data.loanRate) {
            liveRate.value = rateRes.data.loanRate / 100;
        }
    } catch (err) { console.error(err); }
};

onMounted(async () => {
  loading.value = true;
  try {
    const res = await apiClient.get('/Accounts');
    accounts.value = Array.isArray(res.data) ? res.data : [];
    if (accounts.value.length > 0) form.value.accountId = accounts.value[0].id.toString();
    await fetchLoansData();
  } catch (err) { 
    toast.error('Veriler yüklenirken bir sorun oluştu.');
  } finally {
    loading.value = false;
  }
});

const monthlyInstallment = computed(() => {
    const p = form.value.amount || 0;
    const n = form.value.term;
    const i = liveRate.value;
    if (p <= 0) return 0;
    const installment = (p * i * Math.pow(1 + i, n)) / (Math.pow(1 + i, n) - 1);
    return isNaN(installment) ? 0 : installment;
});

const totalPayback = computed(() => monthlyInstallment.value * form.value.term);

const handleApply = async () => {
  if (loans.value.some(l => l.status === 'Pending')) {
      toast.error('Halihazırda bekleyen bir kredi başvurunuz bulunmaktadır.');
      return;
  }
  if (form.value.amount < 5000) {
      toast.error('Minimum kredi tutarı 5.000 ₺ olmalıdır.');
      return;
  }

  isApplying.value = true;
  try {
    await apiClient.post('/Loans/apply', {
      AccountId: parseInt(form.value.accountId),
      Amount: form.value.amount,
      Term: form.value.term,
      LoanType: loanTypeMap[loanTypeDisplay.value] || 'Personal'
    });
    toast.success('Kredi başvurunuz başarıyla alındı.');
    await fetchLoansData();
  } catch (err: any) {
    toast.error(err.response?.data?.message || 'Başvuru hatası.');
  } finally {
    isApplying.value = false;
  }
};

const getStatusLabel = (status: string) => {
    const labels: any = { 'Pending': 'İnceleniyor', 'Approved': 'Onaylandı', 'Rejected': 'Reddedildi' };
    return labels[status] || status;
};

// Sayı girişini temizlemek için watcher
watch(() => form.value.amount, (val) => {
    if (val > 2000000) form.value.amount = 2000000;
});
</script>

<template>
  <div class="view-container fade-in">
    <header class="page-header-minimal">
      <div class="header-left">
        <h1 class="page-title">Kredi Merkezi</h1>
        <p class="page-desc">İhtiyaçlarınıza özel finansal çözümler ve anında başvuru.</p>
      </div>
      <div class="header-right">
        <div class="trust-badge">
            <ShieldCheck :size="16" />
            <span>NexBank Güvencesiyle</span>
        </div>
      </div>
    </header>

    <div v-if="loading" class="loader-minimal"><div class="spinner-p"></div></div>

    <div v-else class="loans-content-grid">
        <!-- 🎯 CALCULATOR AREA -->
        <div class="calc-main-card glass-card-premium">
            <div class="section-title">
                <Calculator :size="18" />
                <span>HESAPLAMA ARACI</span>
            </div>

            <div class="loan-options-grid mt-4">
                <div class="input-field">
                    <label>Kredi Türü</label>
                    <select v-model="loanTypeDisplay" class="premium-select-v3">
                        <option value="İhtiyaç Kredisi">İhtiyaç Kredisi</option>
                        <option value="Konut Kredisi">Konut Kredisi</option>
                        <option value="Taşıt Kredisi">Taşıt Kredisi</option>
                    </select>
                </div>
                <div class="input-field">
                    <label>Ödeme Hesabı</label>
                    <div class="select-with-icon">
                        <Wallet :size="16" v-if="accounts.length > 0" />
                        <select v-model="form.accountId" class="premium-select-v2">
                            <option v-for="acc in accounts" :key="acc.id" :value="acc.id.toString()">
                                {{ acc.accountType === 'Individual' ? 'Vadesiz Hesap' : acc.accountType === 'Savings' ? 'Birikim Hesabı' : acc.accountType }} ({{ acc.currency }})
                            </option>
                            <option v-if="accounts.length === 0" disabled>Hesap bulunamadı</option>
                        </select>
                    </div>
                </div>
            </div>

                <div class="amount-entry-box-premium mt-5">
                    <div class="entry-header">
                        <label>Kredi Tutarı</label>
                        <div class="input-wrapper-gold">
                            <input type="number" v-model.number="form.amount" class="amount-input-p" placeholder="0">
                            <span class="currency-tag">₺</span>
                        </div>
                    </div>
                <div class="range-wrapper mt-3">
                    <input type="range" v-model.number="form.amount" min="5000" max="1000000" step="1000" class="elegant-range">
                    <div class="range-steps">
                        <span>5.000 ₺</span>
                        <span>1.000.000 ₺</span>
                    </div>
                </div>
            </div>

            <div class="term-entry-box mt-5">
                <label class="d-block mb-3">Vade (Ay)</label>
                <div class="term-group-minimal">
                    <button v-for="m in [3, 6, 12, 24, 36, 48, 60]" :key="m" 
                            @click="form.term = m"
                            :class="{ active: form.term === m }"
                            class="term-btn-min">
                        {{ m }} Ay
                    </button>
                </div>
            </div>

            <!-- RESULT DISPLAY -->
            <div class="loan-summary-banner mt-5">
                <div class="summary-item">
                    <small>AYLIK TAKSİT</small>
                    <div class="val gold">{{ monthlyInstallment.toLocaleString('tr-TR', { maximumFractionDigits: 0 }) }} <small>₺</small></div>
                </div>
                <div class="summary-item">
                    <small>TOPLAM GERİ ÖDEME</small>
                    <div class="val">{{ totalPayback.toLocaleString('tr-TR', { maximumFractionDigits: 0 }) }} <small>₺</small></div>
                </div>
                <div class="summary-item">
                    <small>FAİZ ORANI</small>
                    <div class="val text-success">%{{ (liveRate * 100).toFixed(2) }}</div>
                </div>
            </div>

            <button @click="handleApply" class="btn-main-apply mt-4" :disabled="isApplying || accounts.length === 0">
                <span>{{ isApplying ? 'İşleminiz Yapılıyor...' : 'Kredi Başvurusunu Tamamla' }}</span>
                <ArrowRight :size="18" />
            </button>
        </div>

        <!-- 📑 SIDEBAR AREA -->
        <div class="loan-sidebar-v2">
            <div class="glass-card-premium history-panel">
                <div class="section-title">
                    <History :size="18" />
                    <span>BAŞVURU GEÇMİŞİ</span>
                </div>
                
                <div class="history-list mt-4">
                    <div v-for="l in loans" :key="l.id" class="history-item-v2">
                        <div class="h-top">
                            <span class="h-type">{{ l.loanType }}</span>
                            <span class="h-status" :class="l.status.toLowerCase()">{{ getStatusLabel(l.status) }}</span>
                        </div>
                        <div class="h-mid">
                            <strong>{{ l.amount.toLocaleString() }} ₺</strong>
                            <span>{{ l.termMonths }} Ay Vade</span>
                        </div>
                        <div class="h-bot">
                            <Clock :size="12" />
                            <span>{{ new Date(l.createdAt).toLocaleDateString('tr-TR') }}</span>
                        </div>
                    </div>

                    <div v-if="loans.length === 0" class="empty-mini">
                        <Info :size="24" />
                        <p>Kayıtlı başvuru bulunamadı.</p>
                    </div>
                </div>
            </div>

            <div class="glass-card-premium info-box-min mt-4">
                <div class="info-header">
                    <Zap :size="16" class="text-gold" />
                    <strong>Hızlı Onay</strong>
                </div>
                <p>Başvurularınız yapay zeka destekli risk modülümüz tarafından saniyeler içinde analiz edilir.</p>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.page-header-minimal {
    display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 2rem;
}
.page-title { font-size: 2rem; font-weight: 800; color: var(--primary-dark); margin: 0; }
.page-desc { color: var(--text-muted); font-weight: 600; margin-top: 5px; }

.trust-badge {
    display: flex; align-items: center; gap: 8px; background: rgba(15, 23, 42, 0.05);
    padding: 8px 16px; border-radius: 100px; color: var(--text-muted); font-size: 0.8rem; font-weight: 700;
}

.loans-content-grid {
    display: grid; grid-template-columns: 1fr 340px; gap: 24px;
}

/* CALCULATOR CARD */
.calc-main-card { padding: 32px; border-radius: 24px; border: 1px solid rgba(0,0,0,0.03); }
.section-title {
    display: flex; align-items: center; gap: 10px; font-size: 0.75rem; font-weight: 800;
    color: var(--text-muted); letter-spacing: 1px;
}

.loan-options-grid { display: grid; grid-template-columns: 1fr 1.2fr; gap: 20px; }
.input-field label { display: block; font-size: 0.8rem; font-weight: 700; color: var(--text-muted); margin-bottom: 10px; }

.premium-select-v3 {
    width: 100%; padding: 14px 18px; background: rgba(255,255,255,0.6); 
    border: 1px solid rgba(15, 23, 42, 0.1); border-radius: 16px; 
    font-weight: 700; color: var(--primary-dark); outline: none; transition: 0.3s;
    backdrop-filter: blur(10px);
}
.premium-select-v3:focus { border-color: var(--primary); background: white; box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1); }

.select-with-icon { position: relative; display: flex; align-items: center; }
.select-with-icon svg { position: absolute; left: 14px; color: var(--primary); }
.select-with-icon select { padding-left: 40px; }

/* AMOUNT ENTRY */
.amount-entry-box .entry-header { display: flex; justify-content: space-between; align-items: center; }
.input-wrapper-gold {
    display: flex; align-items: center; gap: 10px; background: var(--primary-dark);
    padding: 8px 18px; border-radius: 14px;
}
.amount-input-p {
    background: transparent; border: none; color: var(--gold); font-size: 1.5rem;
    font-weight: 900; font-family: 'Outfit'; width: 140px; text-align: right; outline: none;
}
.amount-input-p::-webkit-inner-spin-button { -webkit-appearance: none; }
.currency-tag { color: white; font-weight: 800; font-size: 0.9rem; opacity: 0.7; }

.elegant-range {
    width: 100%; height: 4px; background: var(--border-light); border-radius: 2px;
    -webkit-appearance: none; outline: none;
}
.elegant-range::-webkit-slider-thumb {
    -webkit-appearance: none; width: 22px; height: 22px; background: white;
    border: 5px solid var(--primary-dark); border-radius: 50%; cursor: pointer;
    box-shadow: 0 4px 10px rgba(0,0,0,0.1); transition: 0.2s;
}
.elegant-range::-webkit-slider-thumb:hover { transform: scale(1.15); border-color: var(--gold); }

.range-steps { display: flex; justify-content: space-between; margin-top: 10px; font-size: 0.75rem; font-weight: 800; color: var(--text-muted); }

.amount-entry-box-premium {
    background: linear-gradient(135deg, rgba(15, 23, 42, 0.02) 0%, rgba(15, 23, 42, 0.05) 100%);
    padding: 30px;
    border-radius: 20px;
    border: 1px solid rgba(0,0,0,0.05);
}

/* TERM MINIMAL */
.term-group-minimal { display: flex; gap: 8px; flex-wrap: wrap; }
.term-btn-min {
    padding: 10px 20px; border-radius: 12px; background: var(--bg-app); border: 1px solid transparent;
    color: var(--text-muted); font-weight: 700; font-size: 0.85rem; cursor: pointer; transition: 0.2s;
}
.term-btn-min:hover { background: white; border-color: var(--border-light); }
.term-btn-min.active { background: var(--primary-dark); color: var(--gold); border-color: var(--gold); }

/* SUMMARY BANNER */
.loan-summary-banner {
    background: #F8FAFC; padding: 24px; border-radius: 20px; display: flex; justify-content: space-around;
}
.summary-item { text-align: center; }
.summary-item small { display: block; font-size: 0.65rem; font-weight: 800; color: var(--text-muted); letter-spacing: 0.5px; margin-bottom: 6px; }
.summary-item .val { font-size: 1.5rem; font-weight: 900; color: var(--primary-dark); font-family: 'Outfit'; }
.summary-item .val.gold { color: var(--gold-dark); }
.summary-item .val small { display: inline; font-size: 1rem; opacity: 0.6; }

.btn-main-apply {
    width: 100%; padding: 20px; background: var(--primary-dark); color: white; border: none;
    border-radius: 16px; font-weight: 800; font-size: 1.1rem; cursor: pointer;
    display: flex; align-items: center; justify-content: center; gap: 12px; transition: 0.3s;
}
.btn-main-apply:hover { background: var(--gold-dark); color: var(--primary-dark); transform: translateY(-2px); box-shadow: 0 10px 25px rgba(0,0,0,0.1); }
.btn-main-apply:disabled { background: var(--border-light); cursor: not-allowed; transform: none; }

/* SIDEBAR HISTORY */
.history-panel { padding: 24px; }
.history-list { display: flex; flex-direction: column; gap: 12px; }
.history-item-v2 {
    background: var(--bg-app); padding: 16px; border-radius: 16px; border: 1px solid transparent; transition: 0.2s;
}
.history-item-v2:hover { background: white; border-color: var(--primary-light); transform: scale(1.02); }

.h-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.h-type { font-size: 0.75rem; font-weight: 800; color: var(--primary-dark); }
.h-status { font-size: 0.65rem; font-weight: 800; padding: 4px 8px; border-radius: 6px; text-transform: uppercase; }
.h-status.pending { background: #FEF3C7; color: #92400E; }
.h-status.approved { background: #DCFCE7; color: #166534; }
.h-status.rejected { background: #FEE2E2; color: #991B1B; }

.h-mid { display: flex; justify-content: space-between; align-items: center; }
.h-mid strong { font-size: 1rem; color: var(--primary-dark); }
.h-mid span { font-size: 0.75rem; color: var(--text-muted); font-weight: 700; }

.h-bot { display: flex; align-items: center; gap: 6px; margin-top: 8px; font-size: 0.7rem; color: var(--text-muted); font-weight: 600; }

.info-box-min { padding: 20px; }
.info-header { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
.info-box-min p { font-size: 0.8rem; color: var(--text-muted); line-height: 1.5; margin: 0; }

.empty-mini { text-align: center; padding: 40px 0; color: var(--text-muted); }
.empty-mini p { font-size: 0.8rem; margin-top: 10px; }

.spinner-p { width: 40px; height: 40px; border: 4px solid var(--bg-app); border-top-color: var(--primary); border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
