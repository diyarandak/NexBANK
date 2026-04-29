<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';
import { 
  ArrowDownLeft, ArrowUpRight, Search, 
  Filter, Calendar, Download, RefreshCw
} from 'lucide-vue-next';

const authStore = useAuthStore();
const transactions = ref<any[]>([]);
const loading = ref(true);
const filterType = ref('All');
const timeRange = ref('30');
const searchQuery = ref('');

const fakeTransactions = [
  { id: 'f1', type: 'Withdrawal', description: 'Netflix Aboneliği', amount: 149.99, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 2).toISOString() },
  { id: 'f2', type: 'Deposit', description: 'Maaş Ödemesi', amount: 45000.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 3).toISOString() },
  { id: 'f3', type: 'Withdrawal', description: 'Market Alışverişi', amount: 845.50, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 4).toISOString() },
  { id: 'f4', type: 'Withdrawal', description: 'Elektrik Faturası', amount: 420.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 5).toISOString() },
  { id: 'f5', type: 'Deposit', description: 'Kira Geliri', amount: 12000.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 7).toISOString() },
  { id: 'f6', type: 'Withdrawal', description: 'Starbucks Kahve', amount: 85.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 8).toISOString() },
  { id: 'f7', type: 'Withdrawal', description: 'Hepsiburada Alışveriş', amount: 2450.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 9).toISOString() },
  { id: 'f8', type: 'Withdrawal', description: 'Shell Akaryakıt', amount: 1250.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 10).toISOString() },
  { id: 'f9', type: 'Deposit', description: 'EFT Gelen Para', amount: 3500.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 12).toISOString() }
];

const fetchTransactions = async () => {
    loading.value = true;
    try {
        const res = await apiClient.get('/Transactions/user/my');
        let real = Array.isArray(res.data) ? res.data : [];
        
        if (authStore.user?.email === 'nexbank@gmail.com') {
            transactions.value = [...real, ...fakeTransactions].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        } else {
            transactions.value = real.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        }
    } catch (err) {
        console.error('İşlemler yüklenemedi', err);
    } finally {
        loading.value = false;
    }
};

const filteredTransactions = computed(() => {
    let list = [...transactions.value];
    
    // Time filter
    const now = new Date();
    const days = parseInt(timeRange.value);
    if (days > 0) {
        const cutoff = new Date(now.getTime() - days * 24 * 60 * 60 * 1000);
        list = list.filter(t => new Date(t.createdAt) >= cutoff);
    }

    if (filterType.value !== 'All') {
        list = list.filter(t => t.type === filterType.value);
    }
    if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase();
        list = list.filter(t => 
            t.description?.toLowerCase().includes(q) || 
            t.amount.toString().includes(q)
        );
    }
    return list;
});

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

onMounted(fetchTransactions);
</script>

<template>
  <div class="view-container slide-up">
    <header class="page-header-premium">
      <div class="header-left">
        <h1 class="view-title text-gradient-gold">Hesap Hareketleri</h1>
        <p class="subtitle">Tüm hesaplarınızdaki para giriş ve çıkışlarını detaylıca inceleyin.</p>
      </div>
      <div class="header-actions">
          <button @click="fetchTransactions" class="btn-secondary-p" :disabled="loading">
            <RefreshCw :size="18" :class="{ 'spin': loading }" />
          </button>
          <button class="btn-premium btn-gold">
            <Download :size="18" /> Dekont Al
          </button>
      </div>
    </header>

    <!-- FILTERS -->
    <div class="filters-bar glass-card mb-4">
        <div class="search-box-p">
            <Search :size="18" class="search-icon" />
            <input v-model="searchQuery" type="text" placeholder="İşlem açıklaması veya tutar ara..." />
        </div>
        <div class="filter-group">
            <div class="filter-item">
                <Filter :size="16" />
                <select v-model="filterType" class="premium-select-v5">
                    <option value="All">Tüm İşlemler</option>
                    <option value="Transfer">Transferler</option>
                    <option value="Deposit">Yatırılanlar</option>
                    <option value="Withdrawal">Çekilenler</option>
                </select>
            </div>
            <div class="filter-item">
                <Calendar :size="16" />
                <select v-model="timeRange" class="premium-select-v5">
                    <option value="1">Bugün</option>
                    <option value="7">Son 1 Hafta</option>
                    <option value="30">Son 30 Gün</option>
                    <option value="0">Tüm Zamanlar</option>
                </select>
            </div>
        </div>
    </div>

    <div v-if="loading" class="loader-overlay-inline mt-5"><div class="spinner"></div></div>

    <div v-else class="transactions-container">
        <div v-if="filteredTransactions.length > 0" class="glass-card table-wrapper-p">
            <table class="premium-table-v2">
                <thead>
                    <tr>
                        <th>Tarih</th>
                        <th>İşlem Detayı</th>
                        <th>Tür</th>
                        <th class="text-right">Tutar</th>
                        <th class="text-right">Durum</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="t in filteredTransactions" :key="t.id" class="tx-row-v2">
                        <td class="date-cell">
                            <div class="d-day">{{ new Date(t.createdAt).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short' }) }}</div>
                            <div class="d-time">{{ new Date(t.createdAt).toLocaleDateString('tr-TR', { hour: '2-digit', minute: '2-digit' }).split(',')[1] }}</div>
                        </td>
                        <td class="desc-cell">
                            <strong>{{ t.description || 'Banka İşlemi' }}</strong>
                            <small>İşlem No: #{{ t.id }}</small>
                        </td>
                        <td>
                            <span class="type-badge" :class="t.type.toLowerCase()">
                                {{ t.type === 'Deposit' ? 'Para Girişi' : (t.type === 'Withdrawal' ? 'Para Çıkışı' : 'Transfer') }}
                            </span>
                        </td>
                        <td class="amount-cell text-right" :class="t.type.toLowerCase()">
                            {{ t.type === 'Deposit' ? '+' : '-' }}{{ formatCurrency(t.amount) }}
                        </td>
                        <td class="text-right">
                            <span class="status-pill-p active">Başarılı</span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div v-else class="empty-state-p glass-card">
            <div class="empty-icon">📂</div>
            <h3>İşlem Bulunamadı</h3>
            <p>Aradığınız kriterlere uygun herhangi bir hesap hareketi kaydı bulunmuyor.</p>
        </div>
    </div>
  </div>
</template>

<style scoped>
.page-header-premium {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 2rem;
}

.text-gradient-gold {
    background: linear-gradient(135deg, #0C1B33 0%, #C5A03A 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

.filters-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px 24px;
    gap: 20px;
}

.search-box-p {
    flex: 1;
    position: relative;
    display: flex;
    align-items: center;
}

.search-icon {
    position: absolute;
    left: 16px;
    color: var(--text-muted);
}

.search-box-p input {
    width: 100%;
    padding: 12px 12px 12px 48px;
    background: var(--bg-app);
    border: 1.5px solid var(--border-light);
    border-radius: 12px;
    font-weight: 600;
}

.filter-group {
    display: flex;
    gap: 16px;
}

.filter-item {
    display: flex;
    align-items: center;
    gap: 10px;
    background: var(--bg-app);
    padding: 8px 16px;
    border-radius: 10px;
    color: var(--text-muted);
    font-weight: 700;
    font-size: 0.85rem;
    border: 1.5px solid var(--border-light);
}

.premium-select-v5 {
    background: none;
    border: none;
    font-weight: 800;
    color: var(--primary-dark);
    outline: none;
    cursor: pointer;
    padding: 0;
}

/* TABLE */
.table-wrapper-p { padding: 0; overflow: hidden; border-radius: 20px; }
.premium-table-v2 { width: 100%; border-collapse: collapse; }
.premium-table-v2 th { background: #F8FAFC; padding: 18px 24px; text-align: left; font-size: 0.75rem; font-weight: 800; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; }
.premium-table-v2 td { padding: 20px 24px; border-bottom: 1px solid #F1F5F9; }

.tx-row-v2:hover { background: rgba(197, 160, 58, 0.02); }

.date-cell .d-day { font-weight: 800; color: var(--primary-dark); font-size: 0.95rem; }
.date-cell .d-time { font-size: 0.75rem; color: var(--text-muted); font-weight: 600; }

.desc-cell strong { display: block; font-size: 0.95rem; color: var(--primary-dark); }
.desc-cell small { font-size: 0.7rem; color: var(--text-muted); font-weight: 600; }

.type-badge { font-size: 0.7rem; font-weight: 800; padding: 4px 10px; border-radius: 6px; text-transform: uppercase; }
.type-badge.deposit { background: #DCFCE7; color: #16A34A; }
.type-badge.withdrawal { background: #FEE2E2; color: #DC2626; }
.type-badge.transfer { background: #FEF3C7; color: #B45309; }

.amount-cell { font-family: 'Outfit'; font-size: 1.15rem; font-weight: 800; }
.amount-cell.deposit { color: var(--success); }
.amount-cell.withdrawal { color: var(--danger); }
.amount-cell.transfer { color: var(--primary-dark); }

.status-pill-p { font-size: 0.7rem; font-weight: 800; padding: 4px 10px; border-radius: 100px; }
.status-pill-p.active { background: #E0F2FE; color: #0369A1; }

.empty-state-p { text-align: center; padding: 80px 40px; }
.empty-icon { font-size: 4rem; margin-bottom: 20px; }
.empty-state-p h3 { font-size: 1.5rem; margin-bottom: 10px; color: var(--primary-dark); }
.empty-state-p p { color: var(--text-muted); font-weight: 600; }

.btn-secondary-p { width: 44px; height: 44px; border-radius: 12px; background: white; border: 1.5px solid var(--border-light); color: var(--text-muted); cursor: pointer; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.btn-secondary-p:hover { border-color: var(--primary); color: var(--primary); }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
