<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAuthStore } from '../store/auth';
import apiClient from '../api/client';
import { 
  Plus, Landmark, CreditCard, ArrowRightLeft, 
  MoreHorizontal, ChevronRight, CheckCircle2, AlertCircle,
  Copy, ExternalLink, Trash2, Wallet, RefreshCw, X, Info
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const authStore = useAuthStore();
const toastStore = useToastStore();
const accounts = ref<any[]>([]);
const loading = ref(true);
const showCreateModal = ref(false);
const isCreating = ref(false);

const newAccount = ref({
  accountType: 'Individual',
  currency: 'TRY'
});

const fetchAccounts = async () => {
  loading.value = true;
  try {
    const res = await apiClient.get('/Accounts');
    accounts.value = Array.isArray(res.data) ? res.data : [];
    console.log("Hesaplar başarıyla yüklendi:", accounts.value.length);
  } catch (err: any) {
    console.error('Hesaplar yüklenirken hata:', err);
    if (err.response?.status === 401) {
       toastStore.error("Oturum süresi dolmuş, lütfen tekrar giriş yapın.");
       authStore.logout();
       window.location.href = '/login';
    } else {
       toastStore.error("Hesap verileri alınamadı. Lütfen internetinizi kontrol edin.");
    }
  } finally {
    loading.value = false;
  }
};

const createAccount = async () => {
  if (isCreating.value) return;
  
  // Önemli: id hem id hem de Id olarak gelebilir, garantiye alıyoruz
  const userId = authStore.user?.id || authStore.user?.Id;
  if (!userId) {
    toastStore.error('Oturum bilgisi doğrulanamadı. Lütfen tekrar giriş yapın.');
    return;
  }

  isCreating.value = true;
  try {
    // .NET Backend camelCase bekliyor olabilir
    const payload = {
      accountType: newAccount.value.accountType,
      currency: newAccount.value.currency,
      userId: Number(userId)
    };
    
    await apiClient.post('/Accounts', payload);
    toastStore.success('Yeni NexBank hesabınız başarıyla açıldı.');
    showCreateModal.value = false;
    await fetchAccounts();
  } catch (err: any) {
    const msg = err.response?.data?.message || "Hesap açılışı sırasında bir hata oluştu.";
    toastStore.error(msg);
  } finally {
    isCreating.value = false;
  }
};

const copyIban = (iban: string) => {
  if (!iban) return;
  navigator.clipboard.writeText(iban);
  toastStore.success('IBAN kopyalandı.');
};

const formatCurrency = (val: number, currency: string = 'TRY') => {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency }).format(val);
};

const getAccountTypeLabel = (type: string) => {
  const types: any = { 'Individual': 'Vadesiz TL', 'Savings': 'Birikim', 'Corporate': 'Kurumsal', 'Deposit': 'Vadeli' };
  return types[type] || type;
};

onMounted(() => {
  fetchAccounts();
});
</script>

<template>
  <div class="view-container fade-in">
    <header class="accounts-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">Hesap Yönetimi</h1>
        <p class="subtitle">NexBank bünyesindeki tüm varlıklarınızı anlık takip edin.</p>
      </div>
      <div class="header-actions-p">
          <button @click="fetchAccounts" class="btn-icon-refresh" :disabled="loading"><RefreshCw :size="18" :class="{ 'spin': loading }" /></button>
          <button @click="showCreateModal = true" class="btn-premium btn-gold"><Plus :size="18" /> Yeni Hesap Aç</button>
      </div>
    </header>

    <!-- Loading State -->
    <div v-if="loading" class="loader-line-p">
        <div class="line-fill"></div>
    </div>

    <!-- MAIN CONTENT -->
    <div class="accounts-grid-p mt-4">
        <div v-for="acc in accounts" :key="acc.id" class="glass-card account-card-v3">
            <div class="ac-top">
                <div class="ac-type-pill">{{ getAccountTypeLabel(acc.accountType) }}</div>
                <div class="ac-logo-min">NB</div>
            </div>
            
            <div class="ac-balance-section">
                <small>BAKİYE</small>
                <h2>{{ formatCurrency(acc.balance, acc.currency) }}</h2>
            </div>

            <div class="ac-iban-section mt-4">
                <div class="iban-label-row">
                    <span>IBAN</span>
                    <button @click="copyIban(acc.iban)" class="copy-link"><Copy :size="12" /> Kopyala</button>
                </div>
                <code>{{ acc.iban }}</code>
            </div>

            <div class="ac-actions-row mt-4">
                <button class="btn-ac-main" @click="$router.push('/transfer')">
                    <ArrowRightLeft :size="14" /> Transfer Yap
                </button>
                <button class="btn-ac-more"><MoreHorizontal :size="16" /></button>
            </div>
        </div>

        <!-- ADD PLACEHOLDER -->
        <div @click="showCreateModal = true" class="add-account-card-v3">
            <div class="add-icon-p"><Plus :size="32" /></div>
            <strong>Yeni Hesap Aç</strong>
            <p>İhtiyaçlarınıza uygun yeni bir hesap tanımlayın.</p>
        </div>
    </div>

    <!-- TABLE LIST -->
    <div class="glass-card mt-5 table-panel-p">
        <div class="tp-header">
            <h3>Varlık Detayları</h3>
            <span class="tp-count">{{ accounts.length }} Aktif Hesap</span>
        </div>
        <div class="table-responsive">
            <table class="table-p">
                <thead>
                    <tr>
                        <th>HESAP TÜRÜ</th>
                        <th>IBAN / HESAP NO</th>
                        <th>BAKİYE</th>
                        <th>DURUM</th>
                        <th class="text-right">AKSİYON</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="acc in accounts" :key="acc.id">
                        <td>
                            <div class="t-acc-info">
                                <div class="t-icon-box"><Wallet :size="16" /></div>
                                <div class="t-text">
                                    <strong>{{ getAccountTypeLabel(acc.accountType) }}</strong>
                                    <small>{{ acc.currency }}</small>
                                </div>
                            </div>
                        </td>
                        <td><code class="t-iban">{{ acc.iban }}</code></td>
                        <td><strong class="t-price">{{ formatCurrency(acc.balance, acc.currency) }}</strong></td>
                        <td>
                            <span class="t-status" :class="{ active: acc.isActive }">
                                {{ acc.isActive ? 'Aktif' : 'Pasif' }}
                            </span>
                        </td>
                        <td class="text-right">
                            <button class="btn-t-action"><ExternalLink :size="16" /></button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <!-- CREATE MODAL -->
    <div v-if="showCreateModal" class="modal-overlay-p" @click.self="showCreateModal = false">
        <div class="glass-card modal-p slide-up">
            <div class="modal-h">
                <h3>Yeni Hesap Başvurusu</h3>
                <button @click="showCreateModal = false" class="m-close"><X :size="20" /></button>
            </div>
            <div class="modal-b">
                <div class="m-field mt-3">
                    <label>Hesap Türü Seçin</label>
                    <select v-model="newAccount.accountType" class="p-select-v4">
                        <option value="Individual">Vadesiz TL Hesabı</option>
                        <option value="Savings">Birikim Hesabı</option>
                        <option value="Corporate">Kurumsal Hesap</option>
                        <option value="Deposit">Vadeli Hesap</option>
                    </select>
                </div>
                <div class="m-field mt-4">
                    <label>Para Birimi</label>
                    <div class="currency-pills mt-2">
                        <div v-for="c in ['TRY', 'USD', 'EUR', 'GOLD']" :key="c" 
                             @click="newAccount.currency = c"
                             :class="{ active: newAccount.currency === c }"
                             class="c-pill">
                            {{ c }}
                        </div>
                    </div>
                </div>
                <div class="m-info-box mt-4">
                    <Info :size="16" />
                    <span>Yeni hesabınız anında tanımlanacak ve IBAN atanacaktır.</span>
                </div>
            </div>
            <div class="modal-f mt-4">
                <button @click="createAccount" class="btn-premium btn-gold btn-block" :disabled="isCreating">
                    {{ isCreating ? 'İşleminiz Yapılıyor...' : 'Hesabı Hemen Aç' }}
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.accounts-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 2.5rem; }
.header-actions-p { display: flex; gap: 12px; align-items: center; }

.btn-icon-refresh { background: var(--bg-app); border: 1px solid var(--border-light); width: 44px; height: 44px; border-radius: 12px; cursor: pointer; display: flex; align-items: center; justify-content: center; color: var(--text-muted); transition: 0.2s; }
.btn-icon-refresh:hover { color: var(--primary); border-color: var(--primary); }

.accounts-grid-p { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 24px; }

.account-card-v3 { padding: 24px; border-radius: 24px; position: relative; border: 1px solid rgba(0,0,0,0.03); }
.ac-top { display: flex; justify-content: space-between; align-items: center; }
.ac-type-pill { background: var(--bg-app); padding: 4px 12px; border-radius: 100px; font-size: 0.7rem; font-weight: 800; color: var(--primary-dark); }
.ac-logo-min { font-family: 'Outfit'; font-weight: 900; color: var(--primary); opacity: 0.5; }

.ac-balance-section { margin-top: 24px; }
.ac-balance-section small { font-size: 0.65rem; font-weight: 800; color: var(--text-muted); letter-spacing: 1px; }
.ac-balance-section h2 { margin: 4px 0 0 0; font-size: 2rem; font-weight: 900; font-family: 'Outfit'; color: var(--primary-dark); }

.ac-iban-section .iban-label-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.ac-iban-section span { font-size: 0.7rem; font-weight: 800; color: var(--text-muted); }
.copy-link { background: none; border: none; font-size: 0.7rem; font-weight: 800; color: var(--primary); cursor: pointer; display: flex; align-items: center; gap: 4px; }
.ac-iban-section code { display: block; background: var(--bg-app); padding: 12px; border-radius: 10px; font-family: 'Inter'; font-size: 0.8rem; font-weight: 700; color: var(--primary-dark); }

.ac-actions-row { display: flex; gap: 10px; }
.btn-ac-main { flex: 1; padding: 10px; background: var(--primary-dark); color: white; border: none; border-radius: 10px; font-weight: 700; font-size: 0.85rem; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px; }
.btn-ac-more { width: 40px; background: var(--bg-app); border: none; border-radius: 10px; cursor: pointer; color: var(--text-muted); }

.add-account-card-v3 { border: 2px dashed var(--border-light); border-radius: 24px; display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 32px; text-align: center; cursor: pointer; transition: 0.3s; background: rgba(255,255,255,0.3); }
.add-account-card-v3:hover { border-color: var(--primary); background: white; transform: translateY(-4px); box-shadow: var(--shadow-lg); }
.add-icon-p { width: 56px; height: 56px; background: var(--bg-app); border-radius: 50%; display: flex; align-items: center; justify-content: center; color: var(--primary); margin-bottom: 16px; }
.add-account-card-v3 strong { display: block; font-size: 1.1rem; color: var(--primary-dark); }
.add-account-card-v3 p { font-size: 0.8rem; color: var(--text-muted); margin-top: 8px; font-weight: 600; }

/* TABLE */
.table-panel-p { padding: 0; overflow: hidden; }
.tp-header { padding: 20px 32px; border-bottom: 1px solid var(--border-light); display: flex; justify-content: space-between; align-items: center; }
.tp-header h3 { margin: 0; font-size: 1.1rem; color: var(--primary-dark); }
.tp-count { font-size: 0.75rem; font-weight: 800; color: var(--text-muted); background: var(--bg-app); padding: 4px 12px; border-radius: 100px; }

.table-p { width: 100%; border-collapse: collapse; }
.table-p th { background: #FAFBFC; padding: 12px 32px; text-align: left; font-size: 0.7rem; color: var(--text-muted); font-weight: 800; border-bottom: 1px solid var(--border-light); }
.table-p td { padding: 20px 32px; border-bottom: 1px solid var(--bg-app); }

.t-acc-info { display: flex; align-items: center; gap: 12px; }
.t-icon-box { width: 36px; height: 36px; background: var(--bg-app); border-radius: 10px; display: flex; align-items: center; justify-content: center; color: var(--primary-dark); }
.t-text strong { display: block; font-size: 0.9rem; }
.t-text small { font-size: 0.7rem; color: var(--text-muted); font-weight: 700; }

.t-iban { font-family: 'Inter'; font-size: 0.8rem; color: var(--text-muted); font-weight: 600; }
.t-price { font-family: 'Outfit'; font-size: 1rem; color: var(--primary-dark); }
.t-status { font-size: 0.7rem; font-weight: 800; padding: 4px 10px; border-radius: 6px; background: #FEE2E2; color: #DC2626; }
.t-status.active { background: #DCFCE7; color: #16A34A; }

/* MODAL */
.modal-overlay-p { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(12, 27, 51, 0.4); backdrop-filter: blur(8px); z-index: 9999; display: flex; align-items: center; justify-content: center; }
.modal-p { width: 100%; max-width: 440px; padding: 32px; }
.modal-h { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.m-close { background: none; border: none; cursor: pointer; color: var(--text-muted); }

.m-field label { display: block; font-size: 0.8rem; font-weight: 800; color: var(--primary-dark); margin-bottom: 10px; text-transform: uppercase; }
.p-select-v4 { width: 100%; padding: 12px; border-radius: 12px; border: 1.5px solid var(--border-light); font-weight: 700; outline: none; }

.currency-pills { display: flex; gap: 8px; }
.c-pill { flex: 1; padding: 10px; text-align: center; background: var(--bg-app); border-radius: 10px; cursor: pointer; font-weight: 800; font-size: 0.8rem; color: var(--text-muted); transition: 0.2s; border: 1.5px solid transparent; }
.c-pill:hover { border-color: var(--primary-light); }
.c-pill.active { background: var(--primary-dark); color: white; border-color: var(--gold); }

.m-info-box { background: rgba(59, 130, 246, 0.05); padding: 14px; border-radius: 12px; display: flex; align-items: center; gap: 12px; color: #3B82F6; font-size: 0.8rem; font-weight: 700; }

.btn-block { width: 100%; padding: 16px; font-size: 1.1rem; }
.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
.loader-line-p { width: 100%; height: 4px; background: #E2E8F0; position: relative; overflow: hidden; border-radius: 2px; margin-bottom: 20px; }
.line-fill { width: 40%; height: 100%; background: var(--primary); position: absolute; animation: slide-loading 1.5s infinite ease-in-out; }
@keyframes slide-loading {
    0% { left: -40%; }
    100% { left: 100%; }
}
</style>
