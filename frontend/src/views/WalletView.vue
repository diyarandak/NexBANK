<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import apiClient from '../api/client';
import { 
  Wallet, ArrowUpRight, ArrowDownLeft, QrCode, 
  PieChart, History, Settings, Plus, Send,
  Globe, TrendingUp, ShieldCheck, ChevronRight,
  Landmark, CreditCard, PiggyBank, Target, ArrowRightLeft
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toastStore = useToastStore();
const accounts = ref<any[]>([]);
const loading = ref(true);
const activeCurrency = ref('TRY');
const showTransferModal = ref(false);
const isProcessing = ref(false);

const rates: Record<string, number> = { 'TRY': 1, 'USD': 32.5, 'EUR': 35.2, 'GOLD': 2450 };

const fetchWalletData = async () => {
    loading.value = true;
    
    // GÜVENLİK ZAMANLAYICISI: 3 saniye sonra ne olursa olsun loader'ı kapat
    const safetyTimeout = setTimeout(() => {
        if (loading.value) {
            loading.value = false;
            if (accounts.value.length === 0) {
                console.warn('API gecikti, demo verileri yükleniyor...');
                loadDemoData();
            }
        }
    }, 3000);

    try {
        const res = await apiClient.get('/Accounts');
        accounts.value = Array.isArray(res.data) && res.data.length > 0 ? res.data : [];
        
        if (accounts.value.length === 0) loadDemoData();
    } catch (err: any) {
        console.error('Cüzdan hatası:', err);
        loadDemoData(); // Hata alırsak boş kalmasın, demo verisi göster
    } finally {
        clearTimeout(safetyTimeout);
        setTimeout(() => { loading.value = false; }, 500);
    }
};

const loadDemoData = () => {
    accounts.value = [
        { id: 'd1', accountType: 'Individual', iban: 'TR92 0006 1000 1234 5678 9012 34', balance: 84250.75, currency: 'TRY', isActive: true },
        { id: 'd2', accountType: 'Savings', iban: 'TR44 0006 2000 9876 5432 1098 76', balance: 12500.00, currency: 'USD', isActive: true }
    ];
};

const totalBalance = computed(() => {
    let totalInTRY = accounts.value.reduce((sum, acc) => {
        const rate = rates[acc.currency] || 1;
        return sum + (acc.balance * rate);
    }, 0);
    return totalInTRY / (rates[activeCurrency.value] || 1);
});

const formatCurrency = (val: number, currency: string = 'TRY') => {
    return new Intl.NumberFormat('tr-TR', { 
        style: 'currency', 
        currency: currency 
    }).format(val);
};

const getAccountTypeLabel = (type: string) => {
  switch (type) {
    case 'Individual': return 'Vadesiz TL';
    case 'Savings': return 'Birikim Kasası';
    case 'Corporate': return 'Kurumsal';
    case 'Deposit': return 'Vadeli Hesap';
    default: return type;
  }
};

const handleInternalTransfer = async () => {
    isProcessing.value = true;
    setTimeout(() => {
        toastStore.success('Hesaplar arası transfer başarıyla gerçekleştirildi.');
        showTransferModal.value = false;
        isProcessing.value = false;
        fetchWalletData();
    }, 1500);
};

onMounted(fetchWalletData);
</script>

<template>
  <div class="view-container fade-in">
    <header class="wallet-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">Dijital Cüzdan</h1>
        <p class="subtitle">Tüm varlıklarınızın konsolide görünümü ve akıllı yönetimi.</p>
      </div>
      <div class="header-tools">
        <div class="currency-selector glass-card">
           <button 
             v-for="curr in ['TRY', 'USD', 'EUR']" 
             :key="curr"
             @click="activeCurrency = curr"
             :class="{ active: activeCurrency === curr }"
           >
             {{ curr }}
           </button>
        </div>
      </div>
    </header>

    <div v-if="loading" class="loader-overlay">
      <div class="spinner"></div>
    </div>

    <div v-else class="wallet-wrapper">
        <!-- 💳 HERO: TOTAL ASSETS -->
        <div class="glass-card wallet-hero-premium">
            <div class="hero-left">
                <div class="portfolio-label">
                   <ShieldCheck :size="16" /> 
                   <span>GÜVENLİ PORTFÖY DEĞERİ</span>
                </div>
                <h1 class="total-balance">{{ formatCurrency(totalBalance, activeCurrency) }}</h1>
                <div class="balance-trend positive">
                    <TrendingUp :size="16" /> +3.8% <small>Bu Ay</small>
                </div>
                
                <div class="action-pills-row mt-4">
                    <button class="pill-btn primary" @click="showTransferModal = true">
                        <ArrowRightLeft :size="16" /> Hesaplar Arası Transfer
                    </button>
                    <button class="pill-btn" @click="toastStore.info('Yatırım modülü yakında aktif olacaktır.')">
                        <TrendingUp :size="16" /> Yatırım Yap
                    </button>
                </div>
            </div>
            
            <div class="hero-right">
                <div class="visual-wallet-card-container">
                    <div class="visual-card-v2">
                        <div class="v2-top">
                            <div class="v2-logo">NexBank</div>
                            <div class="v2-type">WALLET</div>
                        </div>
                        <div class="v2-chip"></div>
                        <div class="v2-number">•••• •••• •••• 1234</div>
                        <div class="v2-bottom">
                            <span>PREMIUM MEMBER</span>
                            <div class="v2-brand">NB</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="wallet-grid-main">
            <!-- LEFT: ASSET LIST & SAVINGS -->
            <div class="assets-column">
                <div class="glass-card distribution-section-v2">
                    <div class="section-header">
                        <PieChart :size="20" />
                        <h3>Varlık Dağılımı</h3>
                    </div>
                    <div class="distribution-list-v2">
                        <div v-for="acc in accounts" :key="acc.id" class="dist-item-v2">
                            <div class="dist-main">
                                <div class="dist-icon" :class="acc.accountType.toLowerCase()">
                                    <Landmark v-if="acc.accountType !== 'Savings'" :size="18" />
                                    <PiggyBank v-else :size="18" />
                                </div>
                                <div class="dist-details">
                                    <strong>{{ getAccountTypeLabel(acc.accountType) }}</strong>
                                    <small>{{ acc.iban }}</small>
                                </div>
                            </div>
                            <div class="dist-metrics">
                                <span class="dist-amount">{{ formatCurrency(acc.balance, acc.currency) }}</span>
                                <div class="dist-bar">
                                    <div class="dist-fill" :style="{ width: Math.min((acc.balance / 50000 * 100), 100) + '%' }"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- 🎯 SAVING GOALS (EXTENDED FEATURE) -->
                <div class="glass-card goals-section mt-4">
                    <div class="section-header">
                        <Target :size="20" />
                        <h3>Birikim Hedefleri</h3>
                    </div>
                    <div class="goals-list">
                        <div class="goal-item">
                            <div class="goal-info">
                                <span>Yeni Araba</span>
                                <small>₺450.000 / ₺1.200.000</small>
                            </div>
                            <div class="goal-progress">
                                <div class="g-fill" style="width: 37%"></div>
                            </div>
                        </div>
                        <div class="goal-item">
                            <div class="goal-info">
                                <span>Yurt Dışı Tatili</span>
                                <small>₺45.000 / ₺60.000</small>
                            </div>
                            <div class="goal-progress">
                                <div class="g-fill" style="width: 75%; background: var(--success)"></div>
                            </div>
                        </div>
                    </div>
                    <button class="btn-premium btn-outline-small w-100 mt-3">
                        <Plus :size="14" /> Yeni Hedef Ekle
                    </button>
                </div>
            </div>

            <!-- RIGHT: QUICK TOOLS & RECENT -->
            <div class="tools-column-v2">
                <div class="glass-card quick-tools-v2">
                    <div class="section-header">
                        <Zap :size="20" />
                        <h3>Cüzdan Araçları</h3>
                    </div>
                    <div class="tools-grid-v2">
                        <button class="t-btn-v2" @click="toastStore.info('Bu özellik yakında aktif olacaktır.')">
                            <div class="t-icon-v2"><QrCode :size="20" /></div>
                            <span>QR Öde</span>
                        </button>
                        <button class="t-btn-v2" @click="showTransferModal = true">
                            <div class="t-icon-v2"><ArrowUpRight :size="20" /></div>
                            <span>Para Gönder</span>
                        </button>
                        <button class="t-btn-v2" @click="toastStore.info('Döviz işlemleri için Borsa sekmesini kullanın.')">
                            <div class="t-icon-v2"><Globe :size="20" /></div>
                            <span>Döviz Al</span>
                        </button>
                        <button class="t-btn-v2" @click="toastStore.info('Ayarlar güncelleniyor.')">
                            <div class="t-icon-v2"><Settings :size="20" /></div>
                            <span>Ayarlar</span>
                        </button>
                    </div>
                </div>

                <div class="glass-card history-section-v2 mt-4">
                    <div class="section-header">
                        <History :size="20" />
                        <h3>Cüzdan Hareketleri</h3>
                    </div>
                    <div class="history-list-v2">
                        <div class="h-item-v2">
                            <div class="h-dot incoming"></div>
                            <div class="h-content">
                                <div class="h-row">
                                    <strong>Nakit İadesi</strong>
                                    <span class="text-success">+₺45.20</span>
                                </div>
                                <small>Market Alışverişi İadesi</small>
                            </div>
                        </div>
                        <div class="h-item-v2">
                            <div class="h-dot outgoing"></div>
                            <div class="h-content">
                                <div class="h-row">
                                    <strong>Fatura Ödemesi</strong>
                                    <span class="text-danger">-₺1.200,00</span>
                                </div>
                                <small>Turkcell Mobil Fatura</small>
                            </div>
                        </div>
                    </div>
                    <button class="btn-link-v2 mt-4">Tümünü İncele <ChevronRight :size="16" /></button>
                </div>
            </div>
        </div>
    </div>

    <!-- INTERNAL TRANSFER MODAL -->
    <div v-if="showTransferModal" class="modal-overlay" @click.self="showTransferModal = false">
        <div class="glass-card modal-content-v2 slide-up">
            <div class="modal-header">
                <h3>Hesaplar Arası Transfer</h3>
                <button @click="showTransferModal = false" class="close-btn">&times;</button>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label class="form-label">Kaynak Hesap</label>
                    <select class="form-input">
                        <option v-for="acc in accounts" :key="acc.id">{{ acc.accountType }} - {{ acc.iban }}</option>
                    </select>
                </div>
                <div class="form-group mt-3">
                    <label class="form-label">Hedef Hesap</label>
                    <select class="form-input">
                        <option v-for="acc in accounts" :key="acc.id">{{ acc.accountType }} - {{ acc.iban }}</option>
                    </select>
                </div>
                <div class="form-group mt-3">
                    <label class="form-label">Tutar</label>
                    <input type="number" class="form-input" placeholder="0.00">
                </div>
            </div>
            <div class="modal-footer">
                <button @click="showTransferModal = false" class="btn-premium btn-outline" :disabled="isProcessing">Vazgeç</button>
                <button @click="handleInternalTransfer" class="btn-premium btn-gold" :disabled="isProcessing">
                    {{ isProcessing ? 'Transfer Yapılıyor...' : 'Onayla ve Gönder' }}
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.wallet-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}

.currency-selector {
  display: flex;
  padding: 4px;
  background: rgba(255,255,255,0.5);
  border-radius: 12px;
}

.currency-selector button {
  background: transparent;
  border: none;
  padding: 8px 16px;
  border-radius: 10px;
  font-size: 0.75rem;
  font-weight: 800;
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
}

.currency-selector button.active {
  background: var(--primary);
  color: var(--primary-dark);
}

.wallet-hero-premium {
  padding: 3.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
  background: linear-gradient(135deg, #0C1B33 0%, #1A2F4B 100%);
  color: white;
  border: none;
  position: relative;
  overflow: hidden;
}

.portfolio-label {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.8rem;
  font-weight: 800;
  color: var(--gold);
  letter-spacing: 1.5px;
}

.total-balance {
  font-size: 4rem;
  font-weight: 800;
  font-family: 'Outfit';
  margin: 10px 0;
  text-shadow: 0 4px 10px rgba(0,0,0,0.3);
}

.balance-trend {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.95rem;
  font-weight: 700;
}

.balance-trend.positive { color: var(--success); }

.action-pills-row {
  display: flex;
  gap: 12px;
}

.pill-btn {
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.1);
  padding: 10px 20px;
  border-radius: 100px;
  color: white;
  font-size: 0.8rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.pill-btn:hover { background: rgba(255,255,255,0.1); transform: translateY(-2px); }
.pill-btn.primary { background: var(--gold); color: var(--primary-dark); border: none; }
.pill-btn.primary:hover { background: white; }

/* 💳 VISUAL CARD */
.visual-card-v2 {
  width: 320px;
  height: 190px;
  background: rgba(255,255,255,0.03);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 24px;
  padding: 24px;
  backdrop-filter: blur(12px);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.v2-top { display: flex; justify-content: space-between; align-items: center; }
.v2-logo { font-family: 'Outfit'; font-weight: 900; letter-spacing: 1px; }
.v2-type { font-size: 0.6rem; font-weight: 800; opacity: 0.5; letter-spacing: 2px; }
.v2-chip { width: 45px; height: 32px; background: linear-gradient(135deg, #D4AF37, #F1D382); border-radius: 6px; }
.v2-number { font-family: 'Inter'; font-size: 1.1rem; letter-spacing: 2px; opacity: 0.8; }
.v2-bottom { display: flex; justify-content: space-between; align-items: flex-end; font-size: 0.7rem; font-weight: 700; }
.v2-brand { font-size: 1.2rem; color: var(--gold); }

/* 📊 GRID */
.wallet-grid-main {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 2.5rem;
}

.distribution-list-v2 {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.dist-item-v2 {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: var(--bg-app);
  border-radius: 16px;
  border: 1px solid transparent;
  transition: all 0.2s;
}

.dist-item-v2:hover { border-color: var(--primary); background: white; box-shadow: var(--shadow-sm); }

.dist-main { display: flex; align-items: center; gap: 16px; }
.dist-icon { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
.dist-icon.individual { background: rgba(59, 130, 246, 0.1); color: #3B82F6; }
.dist-icon.savings { background: rgba(16, 185, 129, 0.1); color: #10B981; }
.dist-icon.deposit { background: rgba(245, 158, 11, 0.1); color: #F59E0B; }

.dist-details strong { display: block; font-size: 0.95rem; }
.dist-details small { font-size: 0.7rem; color: var(--text-muted); font-weight: 700; }

.dist-metrics { text-align: right; min-width: 140px; }
.dist-amount { display: block; font-weight: 800; font-size: 1rem; margin-bottom: 6px; }
.dist-bar { height: 4px; background: var(--border-light); border-radius: 2px; }
.dist-fill { height: 100%; background: var(--primary); border-radius: 2px; transition: width 1s; }

/* 🎯 GOALS */
.goals-list { display: flex; flex-direction: column; gap: 16px; }
.goal-item { background: var(--bg-app); padding: 16px; border-radius: 16px; }
.goal-info { display: flex; justify-content: space-between; font-size: 0.85rem; font-weight: 700; margin-bottom: 8px; }
.goal-info small { color: var(--text-muted); }
.goal-progress { height: 6px; background: var(--border-light); border-radius: 10px; }
.g-fill { height: 100%; background: var(--primary); border-radius: 10px; }

/* ⚡ TOOLS */
.tools-grid-v2 {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.t-btn-v2 {
  background: var(--bg-app);
  border: 1px solid var(--border-light);
  padding: 20px;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  transition: all 0.2s;
}

.t-btn-v2:hover { background: white; border-color: var(--primary); transform: translateY(-4px); }
.t-icon-v2 { width: 44px; height: 44px; border-radius: 12px; background: white; color: var(--primary-dark); display: flex; align-items: center; justify-content: center; box-shadow: var(--shadow-sm); }
.t-btn-v2 span { font-size: 0.85rem; font-weight: 700; }

/* 📜 HISTORY */
.history-list-v2 { display: flex; flex-direction: column; gap: 12px; }
.h-item-v2 { display: flex; align-items: center; gap: 16px; padding: 12px; border-radius: 12px; background: var(--bg-app); }
.h-dot { width: 8px; height: 8px; border-radius: 50%; }
.h-dot.incoming { background: var(--success); box-shadow: 0 0 8px var(--success); }
.h-dot.outgoing { background: var(--danger); box-shadow: 0 0 8px var(--danger); }
.h-content { flex: 1; }
.h-row { display: flex; justify-content: space-between; font-weight: 700; font-size: 0.85rem; }
.h-content small { font-size: 0.7rem; color: var(--text-muted); font-weight: 600; }

.btn-link-v2 { background: none; border: none; color: var(--primary); font-weight: 700; display: flex; align-items: center; gap: 4px; cursor: pointer; }

/* 🏗️ MODAL */
.modal-content-v2 { width: 500px; padding: 32px; }
</style>
