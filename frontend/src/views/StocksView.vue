<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
  TrendingUp, TrendingDown, LayoutDashboard, 
  Search, ArrowRight, Wallet, History,
  Info, Newspaper, PieChart, Activity,
  ChevronUp, ChevronDown, Zap, BarChart3
} from 'lucide-vue-next';

const toast = useToastStore();

const stocks = ref([
    { symbol: 'THYAO', name: 'Türk Hava Yolları', price: 284.50, change: 1.2, volume: '1.2B', history: [280, 282, 281, 285, 284] },
    { symbol: 'ASELS', name: 'Aselsan', price: 54.20, change: -0.8, volume: '450M', history: [55, 54, 53, 54, 54.2] },
    { symbol: 'EREGL', name: 'Erdemir Demir Çelik', price: 42.15, change: 2.5, volume: '890M', history: [40, 41, 41.5, 42, 42.15] },
    { symbol: 'SASA', name: 'Sasa Polyester', price: 38.90, change: 0.1, volume: '2.1B', history: [39, 38.5, 38.7, 38.9, 38.9] },
    { symbol: 'KCHOL', name: 'Koç Holding', price: 165.80, change: -1.4, volume: '720M', history: [168, 167, 166, 165, 165.8] },
    { symbol: 'BIMAS', name: 'BİM Mağazalar', price: 320.10, change: 0.8, volume: '500M', history: [315, 318, 320, 319, 320.1] }
]);

const userAccounts = ref<any[]>([]);
const portfolio = ref<any[]>([]);
const loading = ref(true);
const activeTab = ref<'all' | 'watchlist'>('all');
let stockInterval: any = null;

// Trade State
const searchQuery = ref('');
const tradeAmount = ref(1);
const selectedAccountId = ref('');

// Filtrelenmiş hisseler (Arama fonksiyonu)
const filteredStocks = computed(() => {
    if (!searchQuery.value) return stocks.value;
    const q = searchQuery.value.toUpperCase();
    return stocks.value.filter(s => s.symbol.includes(q) || s.name.toUpperCase().includes(q));
});

const fetchData = async () => {
    try {
        const res = await apiClient.get('/Accounts');
        userAccounts.value = res.data;
        if (userAccounts.value.length > 0) selectedAccountId.value = userAccounts.value[0].id.toString();
        // Portföy simülasyonu
        portfolio.value = [
            { symbol: 'THYAO', amount: 10, buyPrice: 275.40 },
            { symbol: 'ASELS', amount: 50, buyPrice: 52.10 }
        ];
    } catch (err) { console.error(err); }
    finally { loading.value = false; }
};

const simulatePrices = () => {
    stocks.value = stocks.value.map(s => {
        const drift = (Math.random() - 0.48) * 1.2; // Hafif yukarı eğilim
        const newPrice = Math.max(1, s.price + drift);
        const newHistory = [...s.history.slice(1), newPrice];
        return { 
            ...s, 
            price: newPrice, 
            change: ((newPrice - s.history[0]) / s.history[0] * 100),
            history: newHistory,
            trend: drift > 0 ? 'up' : 'down' 
        };
    });
};

const handleTrade = async () => {
    if (!selectedStock.value || !selectedAccountId.value) return;
    
    const totalCost = selectedStock.value.price * tradeAmount.value;
    const account = userAccounts.value.find(a => a.id.toString() === selectedAccountId.value);

    if (tradeMode.value === 'buy' && (!account || account.balance < totalCost)) {
        toast.error("Bakiyeniz bu alım için yetersiz.");
        return;
    }

    try {
        if (tradeMode.value === 'buy') {
            // Gerçek hesap bakiyesini kontrol et
            if (account && account.balance < totalCost) {
                toast.error("Bakiyeniz yetersiz.");
                return;
            }
            
            // Portföye ekle
            const existing = portfolio.value.find(p => p.symbol === selectedStock.value.symbol);
            if (existing) {
                existing.amount += tradeAmount.value;
                existing.buyPrice = (existing.buyPrice + selectedStock.value.price) / 2; // Ortalama maliyet
            } else {
                portfolio.value.push({
                    symbol: selectedStock.value.symbol,
                    amount: tradeAmount.value,
                    buyPrice: selectedStock.value.price
                });
            }
            if (account) account.balance -= totalCost;
            toast.success(`${tradeAmount.value} adet ${selectedStock.value.symbol} portföyünüze eklendi.`);
        } else {
            const existing = portfolio.value.find(p => p.symbol === selectedStock.value.symbol);
            if (!existing || existing.amount < tradeAmount.value) {
                toast.error("Elinizde yeterli hisse bulunmuyor.");
                return;
            }
            
            existing.amount -= tradeAmount.value;
            if (existing.amount === 0) {
                portfolio.value = portfolio.value.filter(p => p.symbol !== selectedStock.value.symbol);
            }
            if (account) account.balance += totalCost;
            toast.success(`${tradeAmount.value} adet ${selectedStock.value.symbol} satıldı, kazanç hesabınıza aktarıldı.`);
        }
        
        tradeMode.value = null;
        tradeAmount.value = 1;
    } catch (err) {
        toast.error("İşlem gerçekleştirilemedi.");
    }
};

const totalPortfolioValue = computed(() => {
    return portfolio.value.reduce((sum, p) => {
        const currentStock = stocks.value.find(s => s.symbol === p.symbol);
        return sum + (p.amount * (currentStock?.price || p.buyPrice));
    }, 0);
});

onMounted(() => {
    fetchData();
    stockInterval = setInterval(simulatePrices, 3000);
});

onUnmounted(() => clearInterval(stockInterval));

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

// SVG Sparkline Chart Generator
const getSparkline = (history: number[]) => {
    const min = Math.min(...history);
    const max = Math.max(...history);
    const range = max - min || 1;
    const width = 100;
    const height = 30;
    
    const points = history.map((val, i) => {
        const x = (i / (history.length - 1)) * width;
        const y = height - ((val - min) / range) * height;
        return `${x},${y}`;
    }).join(' ');
    
    return points;
};
</script>

<template>
  <div class="view-container fade-in">
    <header class="terminal-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">Borsa Terminali</h1>
        <p class="subtitle">Anlık BİST verileri ve profesyonel portföy yönetimi.</p>
      </div>
      <div class="market-status">
        <div class="status-indicator-p pulse"></div>
        <span>BİST 100 <strong>9.142,40</strong> <small class="text-success">+0.85%</small></span>
      </div>
    </header>

    <div class="terminal-layout">
        <!-- 📈 LEFT: LIVE MARKET WATCH -->
        <div class="market-watch glass-card-p">
            <div class="watch-header">
                <div class="tabs-min">
                    <button :class="{ active: activeTab === 'all' }" @click="activeTab = 'all'">Tüm Hisseler</button>
                    <button :class="{ active: activeTab === 'watchlist' }" @click="activeTab = 'watchlist'">Takip Listem</button>
                </div>
                <div class="search-min">
                    <Search :size="16" />
                    <input type="text" v-model="searchQuery" placeholder="Sembol veya hisse adı ara...">
                </div>
            </div>

            <div class="stocks-table-container mt-4">
                <table class="stocks-table-min">
                    <thead>
                        <tr>
                            <th>Sembol</th>
                            <th>Son Fiyat</th>
                            <th>Değişim</th>
                            <th>Grafik</th>
                            <th class="text-right">Hacim</th>
                            <th class="text-right">İşlem</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="s in filteredStocks" :key="s.symbol" class="stock-tr">
                            <td>
                                <div class="s-name-p">
                                    <strong>{{ s.symbol }}</strong>
                                    <small>{{ s.name }}</small>
                                </div>
                            </td>
                            <td><strong class="price-val">{{ s.price.toFixed(2) }} ₺</strong></td>
                            <td>
                                <div class="change-val" :class="s.change >= 0 ? 'up' : 'down'">
                                    <ChevronUp v-if="s.change >= 0" :size="14" />
                                    <ChevronDown v-else :size="14" />
                                    {{ Math.abs(s.change).toFixed(2) }}%
                                </div>
                            </td>
                            <td>
                                <svg class="sparkline" width="80" height="24">
                                    <polyline
                                        fill="none"
                                        :stroke="s.change >= 0 ? '#10B981' : '#EF4444'"
                                        stroke-width="2"
                                        :points="getSparkline(s.history)"
                                    />
                                </svg>
                            </td>
                            <td class="text-right"><small class="text-muted">{{ s.volume }}</small></td>
                            <td class="text-right">
                                <div class="trade-btns">
                                    <button @click="tradeMode = 'buy'; selectedStock = s" class="btn-sm buy">AL</button>
                                    <button @click="tradeMode = 'sell'; selectedStock = s" class="btn-sm sell">SAT</button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <!-- 💼 RIGHT: PORTFOLIO & TRADE -->
        <div class="terminal-sidebar">
            <!-- TRADE PANEL -->
            <div v-if="tradeMode" class="glass-card-p trade-panel-p slide-up">
                <div class="tp-header">
                    <h3>{{ tradeMode === 'buy' ? 'Hisse Alımı' : 'Hisse Satışı' }}</h3>
                    <button @click="tradeMode = null" class="close-btn">&times;</button>
                </div>
                
                <div class="selected-stock-info mt-3">
                    <div class="ss-left">
                        <strong>{{ selectedStock.symbol }}</strong>
                        <span>{{ selectedStock.name }}</span>
                    </div>
                    <div class="ss-right">
                        <strong>{{ selectedStock.price.toFixed(2) }} ₺</strong>
                    </div>
                </div>

                <div class="trade-form-p mt-4">
                    <div class="f-group-p">
                        <label>Ödeme / Tahsilat Hesabı</label>
                        <select v-model="selectedAccountId" class="p-select-v4">
                            <option v-for="a in userAccounts" :key="a.id" :value="a.id.toString()">
                                {{ a.accountType }} ({{ formatCurrency(a.balance) }})
                            </option>
                        </select>
                    </div>
                    <div class="f-group-p mt-3">
                        <label>Hisse Adedi</label>
                        <div class="amount-counter">
                            <button @click="tradeAmount > 1 && tradeAmount--">-</button>
                            <input type="number" v-model.number="tradeAmount">
                            <button @click="tradeAmount++">+</button>
                        </div>
                    </div>

                    <div class="trade-summary-p mt-4">
                        <div class="ts-row">
                            <span>Birim Fiyat</span>
                            <span>{{ selectedStock.price.toFixed(2) }} ₺</span>
                        </div>
                        <div class="ts-row total">
                            <span>Toplam {{ tradeMode === 'buy' ? 'Maliyet' : 'Kazanç' }}</span>
                            <strong>{{ (selectedStock.price * tradeAmount).toLocaleString() }} ₺</strong>
                        </div>
                    </div>

                    <button @click="handleTrade" :class="['btn-confirm-trade mt-4', tradeMode]">
                        {{ tradeMode === 'buy' ? 'ALIM EMRİNİ GÖNDER' : 'SATIŞ EMRİNİ GÖNDER' }}
                    </button>
                </div>
            </div>

            <!-- PORTFOLIO SUMMARY -->
            <div class="glass-card-p portfolio-card-p">
                <div class="pc-header">
                    <PieChart :size="18" />
                    <span>VARLIKLARIM</span>
                </div>
                <div class="pc-total mt-3">
                    <small>Portföy Değeri</small>
                    <h2>{{ formatCurrency(totalPortfolioValue) }}</h2>
                </div>
                
                <div class="pc-list mt-4">
                    <div v-for="p in portfolio" :key="p.symbol" class="pc-item">
                        <div class="pi-left">
                            <strong>{{ p.symbol }}</strong>
                            <small>{{ p.amount }} Adet</small>
                        </div>
                        <div class="pi-right">
                            <strong :class="(stocks.find(s => s.symbol === p.symbol)?.price || 0) >= p.buyPrice ? 'text-success' : 'text-danger'">
                                {{ formatCurrency((stocks.find(s => s.symbol === p.symbol)?.price || p.buyPrice) * p.amount) }}
                            </strong>
                            <small>Maliyet: {{ p.buyPrice }} ₺</small>
                        </div>
                    </div>
                </div>
            </div>

            <!-- NEWS FEED -->
            <div class="glass-card-p news-card-p mt-4">
                <div class="pc-header">
                    <Newspaper :size="18" />
                    <span>BORSA GÜNDEMİ</span>
                </div>
                <div class="news-list mt-3">
                    <div class="news-item">
                        <small>12:05</small>
                        <p>TCMB faiz kararını sabit tuttu, piyasalar olumlu karşıladı.</p>
                    </div>
                    <div class="news-item">
                        <small>11:30</small>
                        <p>THYAO yeni filo genişletme planını açıkladı.</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.terminal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.market-status {
    display: flex;
    align-items: center;
    gap: 12px;
    background: var(--bg-app);
    padding: 10px 20px;
    border-radius: 100px;
    font-size: 0.85rem;
    font-weight: 700;
}

.status-indicator-p {
    width: 8px; height: 8px; background: #10B981; border-radius: 50%;
}

.terminal-layout {
    display: grid;
    grid-template-columns: 1fr 360px;
    gap: 24px;
}

.glass-card-p {
    background: white;
    border-radius: 24px;
    border: 1px solid rgba(0,0,0,0.03);
    padding: 24px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.02);
}

/* MARKET WATCH */
.watch-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.tabs-min { display: flex; gap: 8px; }
.tabs-min button {
    padding: 8px 16px; border: none; border-radius: 10px; background: transparent;
    font-size: 0.85rem; font-weight: 700; color: var(--text-muted); cursor: pointer;
}
.tabs-min button.active { background: var(--primary-dark); color: var(--gold); }

.search-min {
    display: flex; align-items: center; gap: 10px; background: var(--bg-app);
    padding: 8px 16px; border-radius: 10px;
}
.search-min input { background: transparent; border: none; outline: none; font-size: 0.85rem; font-weight: 600; width: 120px; }

.stocks-table-min { width: 100%; border-collapse: collapse; }
.stocks-table-min th { text-align: left; padding: 12px; font-size: 0.75rem; color: var(--text-muted); text-transform: uppercase; border-bottom: 1px solid var(--border-light); }
.stock-tr { border-bottom: 1px solid var(--bg-app); transition: 0.2s; }
.stock-tr:hover { background: #F8FAFC; }
.stock-tr td { padding: 16px 12px; }

.s-name-p strong { display: block; font-size: 1rem; color: var(--primary-dark); }
.s-name-p small { font-size: 0.7rem; color: var(--text-muted); font-weight: 600; }

.price-val { font-family: 'Outfit'; font-size: 1.05rem; color: var(--primary-dark); }

.change-val { display: flex; align-items: center; gap: 4px; font-weight: 800; font-size: 0.85rem; }
.change-val.up { color: #10B981; }
.change-val.down { color: #EF4444; }

.sparkline { overflow: visible; }

.trade-btns { display: flex; gap: 6px; }
.btn-sm {
    border: none; padding: 6px 12px; border-radius: 8px; font-weight: 800; font-size: 0.7rem; cursor: pointer; transition: 0.2s;
}
.btn-sm.buy { background: rgba(16, 185, 129, 0.1); color: #10B981; }
.btn-sm.sell { background: rgba(239, 68, 68, 0.1); color: #EF4444; }
.btn-sm:hover { transform: scale(1.05); }

/* SIDEBAR */
.pc-header { display: flex; align-items: center; gap: 10px; font-size: 0.75rem; font-weight: 800; color: var(--text-muted); letter-spacing: 1px; }
.pc-total h2 { margin: 8px 0 0 0; font-size: 2rem; font-weight: 900; color: var(--primary-dark); font-family: 'Outfit'; }

.pc-item { display: flex; justify-content: space-between; padding: 14px 0; border-bottom: 1px solid var(--bg-app); }
.pi-left strong { display: block; font-size: 0.95rem; }
.pi-left small { font-size: 0.7rem; color: var(--text-muted); font-weight: 700; }
.pi-right { text-align: right; }
.pi-right strong { display: block; font-size: 1rem; }
.pi-right small { font-size: 0.7rem; color: var(--text-muted); font-weight: 600; }

/* TRADE PANEL */
.trade-panel-p { border: 2px solid var(--primary); margin-bottom: 24px; position: relative; }
.tp-header { display: flex; justify-content: space-between; align-items: center; }
.close-btn { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: var(--text-muted); }

.selected-stock-info { display: flex; justify-content: space-between; background: var(--bg-app); padding: 12px; border-radius: 12px; }
.ss-left strong { display: block; font-size: 1.1rem; }
.ss-left span { font-size: 0.75rem; color: var(--text-muted); font-weight: 600; }
.ss-right strong { font-size: 1.25rem; font-family: 'Outfit'; color: var(--primary-dark); }

.f-group-p label { display: block; font-size: 0.8rem; font-weight: 700; color: var(--text-muted); margin-bottom: 8px; }
.p-select-v4 { width: 100%; padding: 10px; border-radius: 10px; border: 1px solid var(--border-light); font-weight: 700; font-size: 0.85rem; }

.amount-counter { display: flex; background: var(--bg-app); border-radius: 10px; overflow: hidden; border: 1px solid var(--border-light); }
.amount-counter button { width: 40px; border: none; background: transparent; font-size: 1.2rem; cursor: pointer; }
.amount-counter input { flex: 1; border: none; background: transparent; text-align: center; font-weight: 800; outline: none; padding: 10px; }

.trade-summary-p { background: #F8FAFC; padding: 16px; border-radius: 12px; }
.ts-row { display: flex; justify-content: space-between; font-size: 0.85rem; font-weight: 700; margin-bottom: 6px; }
.ts-row.total { border-top: 1px solid var(--border-light); padding-top: 10px; margin-top: 10px; font-size: 1rem; }

.btn-confirm-trade { width: 100%; padding: 16px; border: none; border-radius: 12px; font-weight: 900; cursor: pointer; transition: 0.2s; }
.btn-confirm-trade.buy { background: #10B981; color: white; }
.btn-confirm-trade.sell { background: #EF4444; color: white; }
.btn-confirm-trade:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(0,0,0,0.1); }

/* NEWS */
.news-item { padding: 10px 0; border-bottom: 1px solid var(--bg-app); }
.news-item small { font-weight: 800; color: var(--gold-dark); }
.news-item p { margin: 4px 0 0 0; font-size: 0.8rem; font-weight: 600; color: var(--text-main); line-height: 1.4; }

.pulse { animation: statusPulse 2s infinite; }
@keyframes statusPulse { 0% { transform: scale(1); opacity: 1; } 50% { transform: scale(1.5); opacity: 0.5; } 100% { transform: scale(1); opacity: 1; } }
</style>
