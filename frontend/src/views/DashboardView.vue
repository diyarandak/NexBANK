<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useAuthStore } from '../store/auth';
import apiClient from '../api/client';
import { useRouter } from 'vue-router';
import { 
  Send, CreditCard, ArrowLeftRight, QrCode, Receipt, 
  Landmark, ArrowDownLeft, ArrowUpRight, TrendingUp,
  Wallet, Coins, MoreHorizontal, ChevronRight
} from 'lucide-vue-next';

const authStore = useAuthStore();
const router = useRouter();
const summary = ref<any>(null);
const loading = ref(true);
let refreshInterval: any = null;

const fakeTransactions = [
  { id: 'f1', type: 'Withdrawal', description: 'Netflix Aboneliği', amount: 149.99, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 30).toISOString() },
  { id: 'f2', type: 'Deposit', description: 'Maaş Ödemesi', amount: 45000.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 33).toISOString() },
  { id: 'f3', type: 'Withdrawal', description: 'Market Alışverişi', amount: 845.50, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 34).toISOString() },
  { id: 'f4', type: 'Withdrawal', description: 'Elektrik Faturası', amount: 420.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 35).toISOString() },
  { id: 'f5', type: 'Deposit', description: 'Kira Geliri', amount: 12000.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 37).toISOString() },
  { id: 'f6', type: 'Withdrawal', description: 'Starbucks Kahve', amount: 85.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 38).toISOString() },
  { id: 'f7', type: 'Withdrawal', description: 'Hepsiburada Alışveriş', amount: 2450.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 39).toISOString() },
  { id: 'f8', type: 'Withdrawal', description: 'Shell Akaryakıt', amount: 1250.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 40).toISOString() },
  { id: 'f9', type: 'Deposit', description: 'EFT Gelen Para', amount: 3500.00, createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 42).toISOString() }
];

const showAnalysisMenu = ref(false);

const combinedTransactions = computed(() => {
    let real = summary.value?.recentTransactions || [];
    const userId = authStore.user?.id;
    
    // Taranan fişleri ve Manuel işlemleri ekle (Kullanıcıya özel)
    const scannedExpenses = userId ? JSON.parse(localStorage.getItem(`scanned_expenses_${userId}`) || '[]') : [];
    const manualTx = userId ? JSON.parse(localStorage.getItem(`manual_transactions_${userId}`) || '[]') : [];
    let combined = [...scannedExpenses, ...manualTx, ...real];
    
    // Yalnızca demo hesabı ise fake verileri göster
    if (authStore.user?.email === 'nexbank@gmail.com') {
        combined = [...combined, ...fakeTransactions];
    }
    
    combined.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
    return combined.filter(t => t.status !== 'Rejected').slice(0, 10);
});

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

const fetchDashboard = async () => {
    try {
        const res = await apiClient.get('/dashboard/summary');
        summary.value = res.data;
    } catch (err) {
        console.error('Veri çekme hatası');
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    fetchDashboard();
    refreshInterval = setInterval(fetchDashboard, 5000);
});

onUnmounted(() => {
    if (refreshInterval) clearInterval(refreshInterval);
});
</script>

<template>
  <div class="view-container slide-up">
    <header class="dashboard-header-premium">
      <div class="welcome-text">
        <h1 class="view-title-p text-gradient-gold">Ana Sayfa</h1>
        <p class="subtitle-p">Finansal durumunuzun anlık özeti ve hızlı işlemler.</p>
      </div>
      <div class="header-date">
        <span class="date-pill-p">{{ new Date().toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' }) }}</span>
      </div>

      <div class="header-actions">
          <button @click="router.push('/transfer')" class="btn-premium btn-gold-header">
            <Send :size="18" /> Para Gönder
          </button>
          <button @click="router.push('/cards')" class="btn-premium btn-outline-header">
            <CreditCard :size="18" /> Kartlarım
          </button>
      </div>
    </header>

    <div v-if="loading" class="loader-overlay">
      <div class="spinner"></div>
    </div>

    <div v-else class="dashboard-grid">
        <!-- 🏢 TOP METRICS -->
        <div class="metrics-row">
            <div class="glass-card metric-card asset-card">
                <div class="metric-icon bg-success-light">
                  <Wallet :size="24" color="var(--success)" />
                </div>
                <div class="metric-data">
                  <span class="label">TOPLAM VARLIKLARIM</span>
                  <div class="value">{{ formatCurrency(summary?.totalBalance || 0) }}</div>
                  <div class="trend text-success">
                    <TrendingUp :size="14" /> {{ summary?.activeAccounts || 0 }} Aktif Hesap
                  </div>
                </div>
            </div>

            <!-- 🍰 WEALTH CAKE (Demo Verisi) -->
            <div v-if="authStore.user?.email === 'nexbank@gmail.com'" class="glass-card wealth-cake-card">
                <div class="wc-header">
                    <span class="label">VARLIK DAĞILIMI</span>
                    <div class="wc-total">{{ formatCurrency(summary?.totalBalance || 0) }}</div>
                </div>
                <div class="wc-body">
                    <div class="wc-chart-p">
                        <div class="wc-segment tl" style="--val: 45"></div>
                        <div class="wc-segment gold" style="--val: 25"></div>
                        <div class="wc-segment stocks" style="--val: 20"></div>
                        <div class="wc-segment usd" style="--val: 10"></div>
                        <div class="wc-center">
                            <TrendingUp :size="20" />
                        </div>
                    </div>
                    <div class="wc-legend">
                        <div class="legend-item"><span class="dot tl"></span> <small>TL %45</small></div>
                        <div class="legend-item"><span class="dot gold"></span> <small>Altın %25</small></div>
                        <div class="legend-item"><span class="dot stocks"></span> <small>Borsa %20</small></div>
                        <div class="legend-item"><span class="dot usd"></span> <small>Döviz %10</small></div>
                    </div>
                </div>
            </div>

            <!-- Yeni kullanıcılar için boş hal -->
            <div v-else class="glass-card wealth-cake-card empty-wealth">
                <div class="wc-header"><span class="label">VARLIK DAĞILIMI</span></div>
                <div class="wc-empty-msg">Henüz veri yok</div>
            </div>


            <div class="glass-card metric-card debt-card">
                <div class="metric-icon bg-danger-light">
                  <CreditCard :size="24" color="var(--danger)" />
                </div>
                <div class="metric-data">
                  <span class="label">GÜNCEL KART BORCUM</span>
                  <div class="value text-danger">{{ formatCurrency(summary?.creditCardDebt || 0) }}</div>
                  <div class="trend text-muted">Ekstre Kesimi Yaklaşıyor</div>
                </div>
            </div>
        </div>

        <div class="main-content-layout">
            <!-- Left: Quick Actions & Spending -->
            <div class="side-column">
                <div class="glass-card quick-actions-card">
                    <div class="card-header">
                      <h3>⚡ Hızlı İşlemler</h3>
                    </div>
                    <div class="quick-grid">
                        <div class="quick-item" @click="router.push('/transfer')">
                            <div class="icon-box transfer"><ArrowLeftRight :size="24" /></div>
                            <span>Transfer</span>
                        </div>
                        <div class="quick-item" @click="router.push('/qr-pay')">
                            <div class="icon-box qr"><QrCode :size="24" /></div>
                            <span>QR Öde</span>
                        </div>
                        <div class="quick-item" @click="router.push('/payments')">
                            <div class="icon-box bill"><Receipt :size="24" /></div>
                            <span>Fatura</span>
                        </div>
                        <div class="quick-item" @click="router.push('/loans')">
                            <div class="icon-box loan"><Coins :size="24" /></div>
                            <span>Kredi</span>
                        </div>
                    </div>
                </div>

                <div class="glass-card spending-analysis">
                    <div class="card-header">
                      <h3>📊 Harcama Analizi</h3>
                      <div class="menu-container-p">
                        <button class="btn-icon" @click="showAnalysisMenu = !showAnalysisMenu"><MoreHorizontal :size="18" /></button>
                        <div v-if="showAnalysisMenu" class="dropdown-menu-p shadow-lg slide-up">
                            <button @click="showAnalysisMenu = false">📂 Detaylı Rapor</button>
                            <button @click="showAnalysisMenu = false">📈 Kategori Değiştir</button>
                            <button @click="showAnalysisMenu = false">⚙️ Ayarlar</button>
                        </div>
                      </div>
                    </div>
                    <div v-if="authStore.user?.email === 'nexbank@gmail.com'" class="spending-list">
                        <div class="spending-item">
                            <div class="item-info"><span>Market & Gıda</span> <strong>45%</strong></div>
                            <div class="progress-bar"><div class="progress-fill gold" style="width: 45%;"></div></div>
                        </div>
                        <div class="spending-item">
                            <div class="item-info"><span>Fatura & Kurumlar</span> <strong>25%</strong></div>
                            <div class="progress-bar"><div class="progress-fill dark" style="width: 25%;"></div></div>
                        </div>
                        <div class="spending-item">
                            <div class="item-info"><span>Eğlence & Dijital</span> <strong>15%</strong></div>
                            <div class="progress-bar"><div class="progress-fill accent" style="width: 15%;"></div></div>
                        </div>
                    </div>
                    <div v-else class="text-center p-4">
                        <small class="text-muted">Bu ay henüz bir harcama yapılmadı.</small>
                    </div>
                </div>
            </div>

            <!-- Right: Recent Transactions -->
            <div class="glass-card transactions-card">
                <div class="card-header">
                    <h3 class="m-0">Son İşlemlerim</h3>
                    <router-link to="/transactions" class="btn-link">Tümünü Gör <ChevronRight :size="16" /></router-link>
                </div>
                
                <div class="transaction-list">
                    <div v-for="tx in combinedTransactions" :key="tx.id" class="tx-row">
                        <div class="tx-type-icon" :class="tx.type.toLowerCase()">
                            <ArrowDownLeft v-if="tx.type === 'Deposit'" :size="20" />
                            <ArrowUpRight v-else :size="20" />
                        </div>
                        <div class="tx-info">
                            <strong>{{ tx.description || 'Banka İşlemi' }}</strong>
                            <span class="tx-date">{{ new Date(tx.createdAt).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', hour: '2-digit', minute:'2-digit' }) }}</span>
                        </div>
                        <div class="tx-amount" :class="[
                            tx.status === 'Pending' ? 'pending' : (tx.status === 'Rejected' ? 'reverted' : tx.type.toLowerCase()), 
                            { 'deposit': tx.status === 'Approved' && (tx.type === 'Deposit' || (tx.toAccountId && (summary?.userAccountIds || []).includes(tx.toAccountId))) }
                        ]">
                            {{ tx.status === 'Pending' ? '' : (tx.status === 'Rejected' ? '↺' : ((tx.type === 'Deposit' || (tx.toAccountId && (summary?.userAccountIds || []).includes(tx.toAccountId))) ? '+' : '-')) }}{{ formatCurrency(tx.amount) }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
  padding: 24px;
  background: var(--primary-dark);
  border-radius: var(--radius-lg);
  box-shadow: 0 10px 30px rgba(12, 27, 51, 0.1);
  color: white;
}

.view-title-p {
    font-size: 2.2rem;
    margin: 0;
    font-weight: 800;
}

.text-gradient-gold {
    background: linear-gradient(135deg, #F9F295 0%, #C5A03A 50%, #B88A44 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

.subtitle-p {
  margin: 4px 0 0 0;
  opacity: 0.8;
  font-weight: 600;
  font-size: 0.95rem;
}


.header-actions {
  display: flex;
  gap: 12px;
}

.metrics-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
  margin-bottom: 32px;
}

.metrics-row .glass-card {
    border-top: 3px solid var(--primary);
}

.metric-card {
  display: flex;
  align-items: center;
  gap: 20px;
  padding: 24px;
}

.metric-icon {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.bg-success-light { background: rgba(16, 185, 129, 0.1); }
.bg-danger-light { background: rgba(239, 68, 68, 0.1); }
.bg-warning-light { background: rgba(245, 158, 11, 0.1); }

.metric-data .label {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--text-muted);
  letter-spacing: 0.5px;
}

.metric-data .value {
  font-size: 1.85rem;
  font-weight: 900;
  font-family: 'Outfit';
  margin: 4px 0;
  color: var(--primary-dark);
}

.metric-data .trend {
  font-size: 0.8rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 4px;
}

/* WEALTH CAKE STYLES */
.wealth-cake-card { padding: 18px 24px; display: flex; flex-direction: column; gap: 15px; }
.wc-header .label { font-size: 0.65rem; font-weight: 800; color: var(--text-muted); letter-spacing: 1px; }
.wc-total { font-size: 1.1rem; font-weight: 900; color: var(--primary-dark); font-family: 'Outfit'; }

.wc-body { display: flex; align-items: center; gap: 20px; }
.wc-chart-p { width: 80px; height: 80px; position: relative; border-radius: 50%; background: #F1F5F9; overflow: hidden; display: flex; align-items: center; justify-content: center; }
.wc-center { width: 40px; height: 40px; background: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: var(--primary); z-index: 2; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }

.wc-empty-msg { font-size: 0.8rem; color: var(--text-muted); font-weight: 700; margin-top: 10px; }
.empty-wealth { display: flex; flex-direction: column; justify-content: center; align-items: center; text-align: center; }

.wc-legend { flex: 1; display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
.legend-item { display: flex; align-items: center; gap: 6px; }
.dot { width: 8px; height: 8px; border-radius: 2px; }
.dot.tl { background: var(--primary); }
.dot.gold { background: var(--gold); }
.dot.stocks { background: var(--info); }
.dot.usd { background: var(--success); }
.legend-item small { font-size: 0.7rem; font-weight: 700; color: #64748B; }

.main-content-layout {
  display: grid;
  grid-template-columns: 380px 1fr;
  gap: 32px;
}

.side-column {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.card-header h3 {
  font-size: 1.15rem;
  margin: 0;
}

/* Quick Actions */
.quick-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.quick-item {
  background: var(--bg-app);
  padding: 20px 12px;
  border-radius: var(--radius-md);
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.quick-item:hover {
  background: white;
  border-color: var(--primary);
  transform: translateY(-4px);
  box-shadow: var(--shadow-md);
}

.icon-box {
  width: 48px;
  height: 48px;
  margin: 0 auto 12px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--primary-dark);
  color: var(--primary);
}

.quick-item span {
  font-size: 0.85rem;
  font-weight: 700;
}

/* Spending */
.spending-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.item-info {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
  margin-bottom: 8px;
}

.progress-bar {
  height: 8px;
  background: var(--border-light);
  border-radius: 10px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 10px;
}

/* Transactions */
.transaction-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.tx-row {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border-radius: var(--radius-md);
  background: var(--bg-app);
  transition: all 0.2s;
}

.tx-row:hover {
  background: white;
  box-shadow: var(--shadow-sm);
  transform: translateX(4px);
}

.tx-type-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.tx-type-icon.deposit { background: #DCFCE7; color: #16A34A; }
.tx-type-icon.withdrawal { background: #FEE2E2; color: #DC2626; }

.tx-info { flex: 1; display: flex; flex-direction: column; }
.tx-info strong { font-size: 0.95rem; color: var(--text-main); }
.tx-date { font-size: 0.75rem; color: var(--text-muted); margin-top: 2px; }

.tx-amount { font-weight: 700; font-size: 1.1rem; font-family: 'Outfit'; }
.tx-amount.deposit { color: var(--success); }
.tx-amount.withdrawal { color: var(--danger); }
.tx-amount.reverted { color: #94A3B8; text-decoration: line-through; opacity: 0.7; }
.tx-amount.pending { color: #EAB308; } /* Sarı Renk */

.btn-link {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.875rem;
  font-weight: 700;
  color: var(--primary);
  text-decoration: none;
}

.btn-gold-header {
    background: var(--primary);
    color: var(--primary-dark);
    font-weight: 800;
}

.btn-gold-header:hover {
    background: var(--primary-light);
    transform: translateY(-2px);
}

.btn-outline-header {
    background: transparent;
    border: 1.5px solid rgba(255,255,255,0.3);
    color: white;
}

.btn-outline-header:hover {
    background: rgba(255,255,255,0.1);
    border-color: var(--primary);
}

.spending-analysis {
    border-left: 4px solid var(--primary);
}

.progress-fill.gold { background: linear-gradient(90deg, #C5A03A, #F9F295); }
.progress-fill.dark { background: var(--primary-dark); }
.progress-fill.accent { background: var(--accent); }

.menu-container-p { position: relative; }
.dropdown-menu-p { position: absolute; top: 100%; right: 0; background: white; border: 1px solid var(--border-light); border-radius: 12px; padding: 8px; z-index: 100; min-width: 150px; display: flex; flex-direction: column; gap: 4px; }
.dropdown-menu-p button { background: none; border: none; padding: 10px 16px; text-align: left; font-size: 0.85rem; font-weight: 700; color: var(--primary-dark); cursor: pointer; border-radius: 8px; transition: 0.2s; white-space: nowrap; }
.dropdown-menu-p button:hover { background: var(--bg-app); color: var(--primary); }

.transactions-card {
    height: 100%;
    max-height: 580px;
    overflow-y: auto;
}

.btn-icon {
  background: none;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
}

.btn-icon:hover { background: var(--bg-app); color: var(--text-main); }
</style>

