<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '../api/client';
import { 
  CreditCard, Wallet, Calendar, 
  ArrowRight, Info, Zap, AlertCircle,
  CheckCircle2, Landmark, Coins
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const router = useRouter();
const toast = useToastStore();

const cards = ref<any[]>([]);
const accounts = ref<any[]>([]);
const loading = ref(true);
const isProcessing = ref(false);

const form = ref({
    cardId: '',
    toAccountId: '',
    amount: 5000,
    installments: 6
});

const monthlyPayment = ref(0);
const totalRepayment = ref(0);

const calculateRepayment = () => {
    const interestRate = 0.0189; // %1.89
    const total = form.value.amount * (1 + interestRate);
    totalRepayment.value = total;
    monthlyPayment.value = total / form.value.installments;
};

watch([() => form.value.amount, () => form.value.installments], calculateRepayment);

onMounted(async () => {
    try {
        const [cardRes, accRes] = await Promise.all([
            apiClient.get('/creditcards'),
            apiClient.get('/accounts')
        ]);
        cards.value = cardRes.data;
        accounts.value = accRes.data;
        if (cards.value.length > 0) form.value.cardId = cards.value[0].id.toString();
        if (accounts.value.length > 0) form.value.toAccountId = accounts.value[0].id.toString();
        calculateRepayment();
    } catch (err) { 
        toast.error("Veriler yüklenemedi. Lütfen tekrar deneyin.");
    } finally { 
        loading.value = false; 
    }
});

const handleAdvance = async () => {
    if (cards.value.length === 0) {
        toast.error("Önce Kart Yönetimi'nden bir kredi kartı oluşturmanız gerekiyor.");
        return;
    }
    if (accounts.value.length === 0) {
        toast.error("Nakit avansın yatırılacağı bir vadesiz hesabınız bulunmuyor.");
        return;
    }
    if (!form.value.cardId || !form.value.toAccountId || form.value.amount < 500) {
        toast.warning("Lütfen geçerli bir tutar (min. 500 TL) ve hesap seçin.");
        return;
    }

    isProcessing.value = true;
    try {
        const payload = {
            CreditCardId: parseInt(form.value.cardId),
            ToAccountId: parseInt(form.value.toAccountId),
            Amount: Number(form.value.amount),
            Installments: Number(form.value.installments)
        };
        await apiClient.post('/creditcards/cash-advance', payload);
        toast.success('Nakit Avans başarıyla onaylandı! Tutar hesabınıza aktarıldı.');
        router.push('/dashboard');
    } catch (err: any) {
        const msg = err.response?.data?.message || err.response?.data?.Message || 'İşlem sırasında bir hata oluştu.';
        toast.error(msg);
    } finally {
        isProcessing.value = false;
    }
};
</script>

<template>
  <div class="view-container fade-in">
    <header class="view-header-p">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">Taksitli Nakit Avans</h1>
        <p class="subtitle">Acil nakit ihtiyaçların için kart limitin her an emrinde.</p>
      </div>
      <div class="quick-info-p">
          <Zap :size="18" /> <span>%1.89'dan Başlayan Faiz</span>
      </div>
    </header>

    <div v-if="loading" class="loader-p">
        <div class="spinner"></div>
    </div>

    <div v-else class="advance-grid-v2">
        <!-- 📄 FORM SECTION -->
        <div class="glass-card-v5 advance-main-p">
            <div class="a-section">
                <div class="as-h"><CreditCard :size="20" /> <span>KAYNAK KREDİ KARTI</span></div>
                <div class="card-selector-v2 mt-3">
                    <div v-for="card in cards" :key="card.id" 
                         @click="form.cardId = card.id.toString()"
                         :class="['card-pill-v2', { active: form.cardId === card.id.toString() }]">
                        <div class="cp-meta">
                            <strong>{{ card.cardNumber }}</strong>
                            <small>Limit: {{ card.availableLimit.toLocaleString() }} ₺</small>
                        </div>
                        <div class="cp-check"><CheckCircle2 :size="18" /></div>
                    </div>
                    <div v-if="cards.length === 0" class="no-cards">Aktif kredi kartınız bulunmuyor.</div>
                </div>
            </div>

            <div class="a-section mt-5">
                <div class="as-h"><Coins :size="20" /> <span>TUTAR & TAKSİT</span></div>
                <div class="form-row-p mt-3">
                    <div class="input-group-v2">
                        <label>Çekilecek Tutar</label>
                        <div class="input-wrap-v2">
                            <input type="number" v-model="form.amount" step="100" />
                            <span>₺</span>
                        </div>
                    </div>
                    <div class="input-group-v2">
                        <label>Taksit Sayısı</label>
                        <select v-model="form.installments" class="select-v2">
                            <option v-for="i in [3, 6, 9, 12, 18, 24]" :key="i" :value="i">{{ i }} Taksit</option>
                        </select>
                    </div>
                </div>
                <div class="range-slider-p mt-3">
                    <input type="range" v-model="form.amount" min="500" max="100000" step="100" />
                </div>
            </div>

            <div class="a-section mt-5">
                <div class="as-h"><Landmark :size="20" /> <span>HEDEF HESAP</span></div>
                <select v-model="form.toAccountId" class="select-v2 mt-3">
                    <option v-for="acc in accounts" :key="acc.id" :value="acc.id">
                        {{ acc.iban }} — (Bakiye: {{ acc.balance.toLocaleString() }} ₺)
                    </option>
                </select>
            </div>

            <footer class="advance-footer-p mt-5">
                <button @click="handleAdvance" class="btn-confirm-v3" :disabled="isProcessing">
                    <span v-if="!isProcessing">İşlemi Güvenle Onayla</span>
                    <Loader2 v-else :size="20" class="spin" />
                </button>
            </footer>
        </div>

        <!-- 📊 SUMMARY & INFO SIDEBAR -->
        <aside class="advance-side-p">
            <div class="glass-card-v5 summary-card-v2">
                <div class="sc-h">ÖDEME PLANI</div>
                <div class="sc-body-p">
                    <div class="sc-row">
                        <span>Çekilen Tutar</span>
                        <strong>{{ form.amount.toLocaleString() }} ₺</strong>
                    </div>
                    <div class="sc-row">
                        <span>Faiz Oranı</span>
                        <strong class="text-success">%1.89</strong>
                    </div>
                    <div class="sc-row">
                        <span>Toplam Geri Ödeme</span>
                        <strong>{{ totalRepayment.toLocaleString(undefined, {maximumFractionDigits: 2}) }} ₺</strong>
                    </div>
                    <div class="sc-divider"></div>
                    <div class="sc-total-p">
                        <div class="sc-total-label">AYLIK TAKSİT</div>
                        <div class="sc-total-val">{{ monthlyPayment.toLocaleString(undefined, {maximumFractionDigits: 2}) }} ₺</div>
                    </div>
                </div>
            </div>

            <div class="info-box-v2 mt-4">
                <div class="ib-h"><Info :size="18" /> <span>BİLGİLENDİRME</span></div>
                <p>Nakit avans faizi işlem tarihinden itibaren işlemeye başlar. Taksitli işlemleriniz ekstrelerinize düzenli olarak yansıtılır.</p>
                <div class="ib-alert">
                    <AlertCircle :size="14" />
                    <span>Günlük nakit çekim limiti kartınıza özel tanımlanmıştır.</span>
                </div>
            </div>
        </aside>
    </div>
  </div>
</template>

<style scoped>
.view-header-p { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.quick-info-p { display: flex; align-items: center; gap: 8px; background: rgba(197, 160, 89, 0.1); color: var(--gold-dark); padding: 8px 16px; border-radius: 100px; font-size: 0.75rem; font-weight: 800; }

.advance-grid-v2 { display: grid; grid-template-columns: 1fr 380px; gap: 30px; }

.as-h { display: flex; align-items: center; gap: 10px; font-weight: 800; font-size: 0.85rem; color: var(--primary-dark); letter-spacing: 0.5px; opacity: 0.8; }

/* CARD SELECTOR */
.card-selector-v2 { display: flex; flex-direction: column; gap: 10px; }
.card-pill-v2 { background: #F8FAFC; border: 1.5px solid #E2E8F0; padding: 16px 20px; border-radius: 16px; display: flex; justify-content: space-between; align-items: center; cursor: pointer; transition: 0.2s; }
.card-pill-v2:hover { background: white; border-color: var(--primary); }
.card-pill-v2.active { background: #0C1B33; color: white; border-color: #0C1B33; box-shadow: 0 10px 20px rgba(12, 27, 51, 0.1); }
.cp-meta strong { display: block; font-size: 0.95rem; }
.cp-meta small { font-size: 0.75rem; opacity: 0.6; font-weight: 700; }
.cp-check { opacity: 0; transform: scale(0.5); transition: 0.3s; }
.card-pill-v2.active .cp-check { opacity: 1; transform: scale(1); color: var(--gold); }

/* FORM ELEMENTS */
.form-row-p { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.input-group-v2 label { display: block; font-size: 0.7rem; font-weight: 800; color: #94A3B8; margin-bottom: 8px; }
.input-wrap-v2 { position: relative; }
.input-wrap-v2 input { width: 100%; padding: 14px 20px; border-radius: 12px; border: 1.5px solid #E2E8F0; font-weight: 800; font-size: 1.1rem; color: var(--primary-dark); outline: none; }
.input-wrap-v2 input:focus { border-color: var(--primary); background: #F8FAFC; }
.input-wrap-v2 span { position: absolute; right: 20px; top: 16px; font-weight: 800; color: #94A3B8; }

.select-v2 { width: 100%; padding: 14px 20px; border-radius: 12px; border: 1.5px solid #E2E8F0; font-weight: 800; font-size: 0.95rem; color: var(--primary-dark); outline: none; background: #F8FAFC; }

.range-slider-p input { width: 100%; accent-color: var(--primary-dark); }

.btn-confirm-v3 { width: 100%; padding: 16px; background: #0C1B33; color: white; border: none; border-radius: 14px; font-weight: 700; font-size: 1rem; cursor: pointer; transition: 0.3s; box-shadow: 0 10px 20px rgba(12, 27, 51, 0.1); }
.btn-confirm-v3:hover:not(:disabled) { background: var(--primary); transform: translateY(-2px); box-shadow: 0 15px 30px rgba(197, 160, 58, 0.2); }
.btn-confirm-v3:disabled { opacity: 0.5; cursor: not-allowed; }

/* SUMMARY CARD */
.summary-card-v2 { padding: 30px; background: linear-gradient(135deg, #0C1B33 0%, #1E293B 100%); color: white; border: none; }
.sc-h { font-size: 0.75rem; font-weight: 900; letter-spacing: 1px; color: var(--gold); margin-bottom: 20px; text-align: center; }
.sc-row { display: flex; justify-content: space-between; margin-bottom: 12px; font-size: 0.85rem; font-weight: 600; color: rgba(255,255,255,0.7); }
.sc-row strong { color: white; }
.sc-divider { height: 1px; background: rgba(255,255,255,0.1); margin: 20px 0; }
.sc-total-p { text-align: center; }
.sc-total-label { font-size: 0.7rem; font-weight: 800; color: var(--gold); opacity: 0.8; margin-bottom: 8px; }
.sc-total-val { font-size: 2rem; font-weight: 900; font-family: 'Outfit'; color: white; }

.info-box-v2 { padding: 24px; border: 1.5px dashed #E2E8F0; border-radius: 20px; }
.ib-h { display: flex; align-items: center; gap: 10px; font-weight: 800; font-size: 0.8rem; color: var(--primary-dark); margin-bottom: 12px; }
.info-box-v2 p { font-size: 0.8rem; line-height: 1.6; color: #64748B; font-weight: 600; margin-bottom: 16px; }
.ib-alert { display: flex; gap: 8px; color: #F59E0B; font-size: 0.7rem; font-weight: 800; }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
