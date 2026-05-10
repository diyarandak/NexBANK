<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';
import { useToastStore } from '../store/toast';
import { 
  Send, User, Search, History, Star, 
  Trash2, Landmark, ArrowRight, Zap, 
  ShieldCheck, AlertCircle, CheckCircle2,
  ChevronRight, Phone, Info
} from 'lucide-vue-next';

const authStore = useAuthStore();
const toast = useToastStore();
const accounts = ref<any[]>([]);
const favorites = ref<any[]>([]);
const loading = ref(false);
const loadingRecipients = ref(true);
const ibanStatus = ref<'idle' | 'validating' | 'valid' | 'invalid'>('idle');

const form = ref({
  fromAccountId: '',
  toIban: '',
  recipientName: '',
  amount: '',
  paymentMethod: 'Havale',
  saveToFavorites: false
});

const fetchRecipients = async () => {
    loadingRecipients.value = true;
    try {
        const res = await apiClient.get('/Recipients');
        favorites.value = Array.isArray(res.data) ? res.data : [];
    } catch (err) {
        console.error('Rehber yüklenemedi:', err);
    } finally {
        loadingRecipients.value = false;
    }
};

const fetchAccounts = async () => {
  try {
    const res = await apiClient.get('/Accounts');
    accounts.value = res.data;
    if (accounts.value.length > 0 && !form.value.fromAccountId) {
        form.value.fromAccountId = accounts.value[0].id.toString();
    }
  } catch (err) {
    console.error('Hesaplar yüklenemedi:', err);
  }
};

const selectFavorite = (fav: any) => {
    form.value.toIban = fav.recipientIban;
    form.value.recipientName = fav.recipientName;
    ibanStatus.value = 'valid';
    toast.info(`${fav.recipientName} rehberden seçildi.`);
};

const deleteFavorite = async (id: number) => {
    try {
        await apiClient.delete(`/Recipients/${id}`);
        toast.success('Alıcı rehberden silindi.');
        fetchRecipients();
    } catch (err) {
        toast.error('Silme işlemi başarısız.');
    }
};

const validateIban = async () => {
    if (form.value.toIban.length < 26) {
        ibanStatus.value = 'idle';
        return;
    }
    
    ibanStatus.value = 'validating';
    // Simüle edilmiş IBAN kontrolü (Gerçekte backend'e bir HEAD isteği atılabilir)
    setTimeout(() => {
        if (form.value.toIban.startsWith('TR')) {
            ibanStatus.value = 'valid';
        } else {
            ibanStatus.value = 'invalid';
        }
    }, 800);
};

watch(() => form.value.toIban, validateIban);

const handleTransfer = async () => {
  if (!form.value.fromAccountId || !form.value.toIban || !form.value.amount) {
    toast.error('Lütfen tüm zorunlu alanları doldurun.');
    return;
  }

  if (parseFloat(form.value.amount) <= 0) {
      toast.error('Geçersiz tutar girdiniz.');
      return;
  }

  loading.value = true;
  try {
    const payload = {
      FromAccountId: parseInt(form.value.fromAccountId),
      ToIban: form.value.toIban,
      Amount: parseFloat(form.value.amount),
      PaymentMethod: form.value.paymentMethod
    };

    const res = await apiClient.post('/Transactions/transfer', payload);
    toast.success(res.data.message || 'Transfer başarıyla gerçekleştirildi.');
    
    if (form.value.saveToFavorites) {
        try {
            await apiClient.post('/Recipients', {
                RecipientName: form.value.recipientName || 'İsimsiz Alıcı',
                RecipientIban: form.value.toIban,
                Label: 'Müşteri'
            });
            fetchRecipients();
        } catch (fErr) { console.error('Favori kaydı hatası:', fErr); }
    }

    // Reset form
    form.value.amount = '';
    form.value.toIban = '';
    form.value.recipientName = '';
    form.value.saveToFavorites = false;
    ibanStatus.value = 'idle';
    fetchAccounts(); // Bakiyeleri tazele
  } catch (err: any) {
    console.error('Transfer hatası:', err);
    toast.error(err.response?.data?.message || 'İşlem sırasında bir hata oluştu.');
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
    fetchAccounts();
    fetchRecipients();
});

const formatCurrency = (val: number | string, currency: string = 'TRY') => {
    const num = typeof val === 'string' ? parseFloat(val) : val;
    if (currency === 'GOLD') {
        return num.toLocaleString('tr-TR') + ' gr Altın';
    }
    try {
        return new Intl.NumberFormat('tr-TR', { style: 'currency', currency }).format(num || 0);
    } catch (e) {
        return num.toLocaleString('tr-TR') + ' ' + currency;
    }
};

const calculateTransactionFee = () => {
    const amt = parseFloat(form.value.amount || '0');
    if (amt <= 0) return 0;

    switch(form.value.paymentMethod) {
        case 'Havale': return 0;
        case 'EFT': {
            const calculated = (amt * 0.004) - 80;
            return calculated < 15 ? 15 : calculated;
        }
        case 'FAST': {
            const calculated = (amt * 0.001) + 2;
            return calculated < 5 ? 5 : (calculated > 100 ? 100 : calculated);
        }
        case 'SWIFT': {
            const calculated = (amt * 0.015) + 250;
            return calculated < 500 ? 500 : calculated;
        }
        case 'QRTransfer': return amt <= 1000 ? 0 : 2;
        default: return 0;
    }
};

const selectedAccount = computed(() => {
    return accounts.value.find(a => a.id.toString() === form.value.fromAccountId);
});
</script>

<template>
  <div class="view-container fade-in">
    <header class="transfer-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">Para Transferi</h1>
        <p class="subtitle">NexBank güvencesiyle 7/24 kesintisiz ve hızlı transfer deneyimi.</p>
      </div>
      <div class="header-badges">
        <div class="secure-badge"><ShieldCheck :size="16" /> Güvenli İşlem</div>
      </div>
    </header>

    <div class="transfer-grid">
      <!-- LEFT: TRANSFER FORM -->
      <div class="form-column">
        <div class="glass-card transfer-card-premium">
          <form @submit.prevent="handleTransfer" class="transfer-form-v2">
            
            <!-- STEP 1: SOURCE -->
            <div class="form-section-v2">
              <div class="section-title">
                <div class="step-num">1</div>
                <h3>Gönderici Hesap</h3>
              </div>
              <div class="account-selector-v2 mt-3">
                <div v-for="acc in accounts" :key="acc.id" 
                     @click="form.fromAccountId = acc.id.toString()"
                     class="account-opt" :class="{ active: form.fromAccountId === acc.id.toString() }">
                  <div class="acc-top">
                    <span class="acc-type">{{ acc.accountType === 'Individual' ? 'Vadesiz TL' : 'Birikim' }}</span>
                    <span class="acc-balance">{{ formatCurrency(acc.balance, acc.currency) }}</span>
                  </div>
                  <div class="acc-bottom">
                    <small>{{ acc.iban }}</small>
                    <CheckCircle2 v-if="form.fromAccountId === acc.id.toString()" :size="16" />
                  </div>
                </div>
              </div>
            </div>

            <!-- STEP 2: RECIPIENT -->
            <div class="form-section-v2 mt-5">
              <div class="section-title">
                <div class="step-num">2</div>
                <h3>Alıcı Bilgileri</h3>
              </div>
              <div class="form-grid-v2 mt-3">
                <div class="form-group-v2">
                  <label>Alıcı Adı Soyadı</label>
                  <div class="input-with-icon">
                    <User :size="18" class="icon" />
                    <input type="text" v-model="form.recipientName" placeholder="Örn: Mehmet Öz" class="p-input">
                  </div>
                </div>
                <div class="form-group-v2">
                  <label>Alıcı IBAN</label>
                  <div class="input-with-icon" :class="ibanStatus">
                    <Landmark :size="18" class="icon" />
                    <input type="text" v-model="form.toIban" placeholder="TR..." maxlength="26" class="p-input">
                    <div v-if="ibanStatus === 'validating'" class="spinner-mini"></div>
                    <CheckCircle2 v-if="ibanStatus === 'valid'" :size="18" class="status-icon success" />
                    <AlertCircle v-if="ibanStatus === 'invalid'" :size="18" class="status-icon error" />
                  </div>
                  <small v-if="ibanStatus === 'invalid'" class="error-text">Geçersiz IBAN formatı</small>
                </div>
              </div>
              <div class="checkbox-v2 mt-3">
                <label class="custom-cb">
                  <input type="checkbox" v-model="form.saveToFavorites">
                  <span class="checkmark"></span>
                  Bu kişiyi rehberime kaydet
                </label>
              </div>
            </div>

            <!-- STEP 3: DETAILS -->
            <div class="form-section-v2 mt-5">
              <div class="section-title">
                <div class="step-num">3</div>
                <h3>Transfer Detayları</h3>
              </div>
              <div class="form-grid-v2 mt-3">
                <div class="form-group-v2">
                  <label>Gönderilecek Tutar</label>
                  <div class="input-with-icon amount-input">
                    <span class="currency-tag">₺</span>
                    <input type="number" v-model="form.amount" placeholder="0.00" class="p-input lg">
                  </div>
                </div>
                <div class="form-group-v2">
                  <label>İşlem Tipi</label>
                  <div class="payment-method-selector">
                    <div class="method-opt" :class="{ active: form.paymentMethod === 'Havale' }" @click="form.paymentMethod = 'Havale'">
                      <Zap :size="16" /> Havale
                    </div>
                    <div class="method-opt" :class="{ active: form.paymentMethod === 'FAST' }" @click="form.paymentMethod = 'FAST'">
                      <Zap :size="16" /> FAST
                    </div>
                    <div class="method-opt" :class="{ active: form.paymentMethod === 'EFT' }" @click="form.paymentMethod = 'EFT'">
                      <History :size="16" /> EFT
                    </div>
                    <div class="method-opt" :class="{ active: form.paymentMethod === 'SWIFT' }" @click="form.paymentMethod = 'SWIFT'">
                      <Landmark :size="16" /> SWIFT
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <button type="submit" class="btn-premium btn-gold btn-block mt-5" :disabled="loading || (selectedAccount && selectedAccount.balance < parseFloat(form.amount || '0'))">
              <span v-if="!loading">
                Transferi Onayla 
                {{ calculateTransactionFee() > 0 ? `(İşlem Ücreti: ${formatCurrency(calculateTransactionFee())})` : '' }}
              </span>
              <span v-else>İşlem Gerçekleştiriliyor...</span>
              <ArrowRight v-if="!loading" :size="20" />
            </button>
            
            <p v-if="selectedAccount && parseFloat(form.amount || '0') > selectedAccount.balance" class="insufficient-warning mt-3">
              <AlertCircle :size="14" /> Yetersiz bakiye. Lütfen tutarı kontrol edin.
            </p>
          </form>
        </div>
      </div>

      <!-- RIGHT: RECIPIENTS & INFO -->
      <div class="sidebar-column">
        <div class="glass-card recipients-panel slide-up">
          <div class="side-header">
            <div class="header-left">
                <Star :size="20" class="star-icon" />
                <h3>Banka Rehberim</h3>
            </div>
            <span class="count-badge">{{ favorites.length }} Kayıtlı</span>
          </div>

          <div v-if="loadingRecipients" class="loader-mini mt-4">
             <div class="spinner-s"></div>
          </div>

          <div v-else class="recipients-scroll mt-4">
            <div v-for="fav in favorites" :key="fav.id" class="recipient-item-p" @click="selectFavorite(fav)">
                <div class="item-left">
                    <div class="avatar-p">{{ fav.recipientName.charAt(0) }}</div>
                    <div class="item-info-p">
                        <strong>{{ fav.recipientName }}</strong>
                        <small>{{ fav.recipientIban.slice(0, 15) }}...</small>
                    </div>
                </div>
                <button @click.stop="deleteFavorite(fav.id)" class="delete-btn-p"><Trash2 :size="16" /></button>
            </div>
            
            <div v-if="favorites.length === 0" class="empty-state-side">
                <User :size="32" />
                <p>Henüz kayıtlı alıcı bulunmuyor.</p>
            </div>
          </div>

          <button class="btn-premium btn-outline-small w-100 mt-4" @click="toast.info('Manuel kayıt yakında eklenecektir.')">
            <Plus :size="14" /> Yeni Kişi Ekle
          </button>
        </div>

        <div class="glass-card promo-info-card mt-4 gold-border">
          <div class="promo-header">
            <Zap :size="20" />
            <h4>Ücretsiz Havale</h4>
          </div>
          <p>NexBank hesapları arasındaki tüm transferler 7/24 ücretsizdir. Limitlere takılmadan özgürce gönderin.</p>
          <div class="info-tip">
             <Info :size="14" />
             <span>FAST ile diğer bankalara 20.000 TL'ye kadar anında gönderim yapabilirsiniz.</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.transfer-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}

.secure-badge {
  background: rgba(16, 185, 129, 0.1);
  color: #10B981;
  padding: 8px 16px;
  border-radius: 100px;
  font-size: 0.8rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.transfer-grid {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 32px;
}

.transfer-card-premium {
  padding: 40px;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 1.5rem;
}

.step-num {
  width: 28px;
  height: 28px;
  background: var(--primary-dark);
  color: var(--gold);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  font-weight: 800;
}

.section-title h3 {
  font-size: 1.2rem;
  margin: 0;
  color: var(--primary-dark);
}

/* ACCOUNT SELECTOR */
.account-selector-v2 {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.account-opt {
  background: var(--bg-app);
  padding: 16px;
  border-radius: 16px;
  border: 2px solid transparent;
  cursor: pointer;
  transition: all 0.2s;
}

.account-opt:hover { background: white; border-color: var(--primary-light); }
.account-opt.active { background: white; border-color: var(--primary); box-shadow: var(--shadow-sm); }

.acc-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.acc-type { font-weight: 700; font-size: 0.85rem; color: var(--text-main); }
.acc-balance { font-weight: 800; font-size: 0.95rem; color: var(--primary-dark); }
.acc-bottom { display: flex; justify-content: space-between; align-items: center; color: var(--text-muted); }
.acc-bottom small { font-family: monospace; font-size: 0.75rem; letter-spacing: -0.5px; }

/* FORM GROUPS */
.form-grid-v2 { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
.form-group-v2 label { display: block; font-size: 0.85rem; font-weight: 700; color: var(--text-muted); margin-bottom: 8px; }

.input-with-icon {
  position: relative;
  display: flex;
  align-items: center;
}

.input-with-icon .icon {
  position: absolute;
  left: 14px;
  color: var(--text-muted);
}

.input-with-icon.validating .icon { color: var(--primary); }
.input-with-icon.valid .p-input { border-color: var(--success); }
.input-with-icon.invalid .p-input { border-color: var(--danger); }

.p-input {
  width: 100%;
  padding: 12px 14px 12px 42px;
  background: var(--bg-app);
  border: 1px solid var(--border-light);
  border-radius: 12px;
  font-size: 0.9rem;
  font-weight: 600;
  transition: all 0.2s;
}

.p-input:focus { background: white; border-color: var(--primary); outline: none; }
.p-input.lg { font-size: 1.5rem; font-weight: 800; padding: 16px 20px 16px 48px; }

.amount-input .currency-tag {
  position: absolute;
  left: 18px;
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--primary-dark);
}

.status-icon { position: absolute; right: 14px; }
.status-icon.success { color: var(--success); }
.status-icon.error { color: var(--danger); }

.spinner-mini {
    width: 18px; height: 18px; border: 2px solid var(--border-light);
    border-top-color: var(--primary); border-radius: 50%;
    animation: spin 0.8s linear infinite; position: absolute; right: 14px;
}

@keyframes spin { to { transform: rotate(360deg); } }

/* PAYMENT METHOD */
.payment-method-selector {
  display: flex;
  gap: 8px;
  background: var(--bg-app);
  padding: 4px;
  border-radius: 12px;
}

.method-opt {
  flex: 1; padding: 10px; border-radius: 10px; font-size: 0.8rem; font-weight: 700;
  display: flex; align-items: center; justify-content: center; gap: 6px;
  cursor: pointer; transition: all 0.2s;
}

.method-opt.active { background: white; color: var(--primary); box-shadow: var(--shadow-sm); }

/* CHECKBOX */
.custom-cb { display: flex; align-items: center; gap: 10px; font-size: 0.85rem; font-weight: 600; cursor: pointer; color: var(--text-muted); }
.custom-cb input { position: absolute; opacity: 0; cursor: pointer; }
.checkmark { width: 20px; height: 20px; background: var(--bg-app); border: 1px solid var(--border-light); border-radius: 6px; position: relative; }
.custom-cb:hover input ~ .checkmark { border-color: var(--primary); }
.custom-cb input:checked ~ .checkmark { background-color: var(--primary); border-color: var(--primary); }
.checkmark:after { content: ""; position: absolute; display: none; left: 7px; top: 3px; width: 5px; height: 10px; border: solid white; border-width: 0 2px 2px 0; transform: rotate(45deg); }
.custom-cb input:checked ~ .checkmark:after { display: block; }

.btn-block { width: 100%; display: flex; justify-content: center; align-items: center; gap: 12px; padding: 18px; font-size: 1.1rem; }
.btn-gold:disabled { background: var(--border-light); cursor: not-allowed; }

.insufficient-warning { color: var(--danger); font-size: 0.8rem; font-weight: 700; display: flex; align-items: center; gap: 6px; }

/* 👤 SIDEBAR */
.recipients-panel { padding: 24px; }
.side-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.header-left { display: flex; align-items: center; gap: 10px; }
.star-icon { color: var(--gold); }
.count-badge { background: var(--bg-app); padding: 4px 10px; border-radius: 100px; font-size: 0.7rem; font-weight: 800; color: var(--text-muted); }

.recipients-scroll { display: flex; flex-direction: column; gap: 12px; max-height: 420px; overflow-y: auto; padding-right: 4px; }
.recipients-scroll::-webkit-scrollbar { width: 4px; }
.recipients-scroll::-webkit-scrollbar-thumb { background: var(--border-light); border-radius: 10px; }

.recipient-item-p {
    display: flex; justify-content: space-between; align-items: center;
    padding: 12px; background: var(--bg-app); border-radius: 14px;
    cursor: pointer; transition: all 0.2s; border: 1px solid transparent;
}

.recipient-item-p:hover { background: white; border-color: var(--primary-light); transform: translateX(4px); box-shadow: var(--shadow-sm); }

.item-left { display: flex; align-items: center; gap: 12px; }
.avatar-p { width: 38px; height: 38px; background: var(--primary-light); color: var(--primary-dark); border-radius: 10px; display: flex; align-items: center; justify-content: center; font-weight: 900; font-size: 0.9rem; }
.item-info-p strong { display: block; font-size: 0.85rem; color: var(--primary-dark); }
.item-info-p small { font-size: 0.7rem; color: var(--text-muted); font-family: monospace; }

.delete-btn-p { background: transparent; border: none; color: var(--text-muted); padding: 6px; border-radius: 8px; cursor: pointer; transition: all 0.2s; }
.delete-btn-p:hover { background: #FEE2E2; color: #EF4444; }

.empty-state-side { text-align: center; padding: 2rem; color: var(--text-muted); }
.empty-state-side p { font-size: 0.8rem; font-weight: 600; margin-top: 10px; }

.promo-info-card { padding: 1.5rem; }
.promo-header { display: flex; align-items: center; gap: 10px; margin-bottom: 10px; color: var(--gold-dark); }
.promo-header h4 { margin: 0; }
.promo-info-card p { font-size: 0.8rem; color: var(--text-muted); line-height: 1.5; }
.info-tip { display: flex; gap: 10px; background: var(--bg-app); padding: 10px; border-radius: 10px; font-size: 0.75rem; color: var(--text-main); margin-top: 1rem; }

.error-text { color: var(--danger); font-size: 0.7rem; font-weight: 700; margin-top: 4px; display: block; }

.fee-preview {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 0.75rem;
    font-weight: 700;
    color: var(--text-muted);
}
.fee-preview strong { color: var(--danger); }
.fee-preview strong.free { color: var(--success); }

.w-100 { width: 100%; }
</style>
