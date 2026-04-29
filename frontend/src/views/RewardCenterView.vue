<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { 
  Gamepad2, Trophy, Ticket, Sparkles, 
  Play, RotateCcw, ChevronRight, Gift,
  Zap, Star, Wallet, Dices, RefreshCw, 
  CheckCircle2, TrendingUp, Coins, 
  Search, ShoppingBag, Coffee, Monitor,
  Gamepad, Brain, LayoutGrid, ArrowUpRight
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const nexPuan = ref(parseInt(localStorage.getItem('nexPuan') || '0'));
const activeTab = ref('games'); // games, shop

const games = [
    { id: 'bird', name: 'NexBird', icon: Zap, desc: 'Engellerden kaç, NexPuanları topla.', color: '#D4AF37' },
    { id: 'snake', name: 'NexSnake', icon: Gamepad, desc: 'Yemleri ye, NexPuanları katla.', color: '#10B981' },
    { id: 'memory', name: 'Memory Master', icon: Brain, desc: 'Kartları eşleştir, hafızanı konuştur.', color: '#3B82F6' },
    { id: 'crypto', name: 'Crypto Predictor', icon: TrendingUp, desc: 'Grafiği tahmin et, NexPuan kazan.', color: '#8B5CF6' },
    { id: 'wheel', name: 'Wheel of Nex', icon: RefreshCw, desc: 'Çevir ve sürpriz ödülü kap!', color: '#F43F5E' }
];

const rewards = [
    { id: 1, brand: 'Starbucks', title: 'Ücretsiz Tall Boy Kahve', cost: 500, icon: Coffee, category: 'Food' },
    { id: 2, brand: 'Amazon', title: '100 TL Hediye Çeki', cost: 2000, icon: ShoppingBag, category: 'Shopping' },
    { id: 3, brand: 'Netflix', title: '1 Aylık Standart Paket', cost: 1500, icon: Monitor, category: 'Digital' },
    { id: 4, brand: 'NexBank', title: '+0.50 Mevduat Faizi', cost: 1000, icon: ArrowUpRight, category: 'Bank' },
    { id: 5, brand: 'Hepsiburada', title: '50 TL İndirim Kuponu', cost: 300, icon: Ticket, category: 'Shopping' }
];

const selectedGame = ref<any>(null);
const isGameRunning = ref(false);

const openGame = (game: any) => {
    selectedGame.value = game;
    isGameRunning.value = true;
    toast.info(`${game.name} başlatılıyor...`);
};

const buyReward = (reward: any) => {
    if (nexPuan.value < reward.cost) {
        toast.error('Yetersiz NexPuan! Biraz daha oyun oynamalısın.');
        return;
    }
    nexPuan.value -= reward.cost;
    localStorage.setItem('nexPuan', nexPuan.value.toString());
    toast.success(`${reward.brand} - ${reward.title} başarıyla alındı. Kodun e-postana gönderildi!`);
};

const earnPoints = (amount: number) => {
    nexPuan.value += amount;
    localStorage.setItem('nexPuan', nexPuan.value.toString());
    toast.success(`Tebrikler! ${amount} NexPuan kazandın.`);
};

// SIMULATE GAME END FOR DEMO
const finishGame = () => {
    isGameRunning.value = false;
    earnPoints(Math.floor(Math.random() * 50) + 20);
};

</script>

<template>
  <div class="view-container fade-in">
    <header class="rewards-header-v2 mb-4">
      <div class="h-left">
        <h1 class="text-gradient-gold">NexLife Ödül Dünyası</h1>
        <p class="subtitle">Oynadıkça NexPuan kazan, harcadıkça gerçek ödülleri topla.</p>
      </div>
      <div class="h-right">
        <div class="nexpuan-balance-card">
            <div class="nbc-icon"><Coins :size="24" /></div>
            <div class="nbc-text">
                <small>TOPLAM BAKİYE</small>
                <strong>{{ nexPuan }} NexPuan</strong>
            </div>
        </div>
      </div>
    </header>

    <!-- TABS -->
    <div class="rewards-nav mb-5">
        <button @click="activeTab = 'games'" :class="{ active: activeTab === 'games' }">
            <Gamepad2 :size="18" /> OYUNLAR
        </button>
        <button @click="activeTab = 'shop'" :class="{ active: activeTab === 'shop' }">
            <ShoppingBag :size="18" /> NEXSHOP
        </button>
    </div>

    <!-- GAMES VIEW -->
    <div v-if="activeTab === 'games'" class="games-grid">
        <div v-for="g in games" :key="g.id" class="game-card-v2" :style="{ '--game-color': g.color }" @click="openGame(g)">
            <div class="gc-icon-box">
                <component :is="g.icon" :size="32" />
            </div>
            <div class="gc-info">
                <h3>{{ g.name }}</h3>
                <p>{{ g.desc }}</p>
                <div class="gc-footer">
                    <span class="puan-tag">Max 500 NP</span>
                    <button class="btn-play">OYNA <Play :size="14" /></button>
                </div>
            </div>
        </div>
    </div>

    <!-- SHOP VIEW -->
    <div v-else class="shop-grid">
        <div v-for="r in rewards" :key="r.id" class="shop-card-v2">
            <div class="sc-header">
                <span class="sc-category">{{ r.category }}</span>
                <div class="sc-brand-icon"><component :is="r.icon" :size="24" /></div>
            </div>
            <div class="sc-body">
                <small>{{ r.brand }}</small>
                <h3>{{ r.title }}</h3>
            </div>
            <div class="sc-footer">
                <div class="sc-cost">
                    <Coins :size="16" />
                    {{ r.cost }}
                </div>
                <button @click="buyReward(r)" :disabled="nexPuan < r.cost" class="btn-buy-v2">
                    AL
                </button>
            </div>
        </div>
    </div>

    <!-- GAME MODAL (SIMULATED) -->
    <div v-if="isGameRunning" class="game-overlay-v2" @click.self="isGameRunning = false">
        <div class="game-window glass-card-elite scale-up">
            <div class="gw-header">
                <h2>{{ selectedGame?.name }}</h2>
                <button @click="isGameRunning = false">&times;</button>
            </div>
            <div class="gw-content">
                <!-- Burası normalde canvas olurdu, demo için statik tutuyoruz -->
                <div class="game-placeholder">
                    <component :is="selectedGame?.icon" :size="80" :style="{ color: selectedGame?.color }" />
                    <h3>Oyun Başladı!</h3>
                    <p>Skorun yükseldikçe NexPuan kazanacaksın.</p>
                    <div class="game-stats-mini mt-4">
                        <div class="gsm-item"><span>SKOR</span><strong>120</strong></div>
                        <div class="gsm-item"><span>NP KAZANCI</span><strong>+15</strong></div>
                    </div>
                    <button @click="finishGame" class="btn-finish-game mt-5">OYUNU BİTİR VE PUANLARI TOPLA</button>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.rewards-header-v2 { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2.2rem; font-weight: 900;
}

.nexpuan-balance-card { 
    background: #0F172A; padding: 16px 24px; border-radius: 20px; 
    display: flex; align-items: center; gap: 20px; color: white; border: 1px solid #D4AF37;
}
.nbc-icon { width: 44px; height: 44px; background: rgba(212, 175, 55, 0.1); color: #D4AF37; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
.nbc-text small { display: block; font-size: 0.65rem; color: rgba(255,255,255,0.4); font-weight: 800; letter-spacing: 1px; }
.nbc-text strong { font-size: 1.2rem; color: #D4AF37; font-family: 'Outfit'; }

.rewards-nav { display: flex; gap: 16px; border-bottom: 1px solid #F1F5F9; padding-bottom: 20px; }
.rewards-nav button { 
    background: none; border: none; padding: 10px 20px; font-size: 0.9rem; font-weight: 800; 
    color: #94A3B8; cursor: pointer; display: flex; align-items: center; gap: 10px; transition: 0.2s;
    border-radius: 12px;
}
.rewards-nav button.active { color: #0F172A; background: #F1F5F9; }

/* GAMES GRID */
.games-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 24px; }
.game-card-v2 { 
    background: white; border-radius: 24px; padding: 24px; border: 1px solid #F1F5F9;
    display: flex; gap: 20px; cursor: pointer; transition: 0.3s; overflow: hidden; position: relative;
}
.game-card-v2:hover { transform: translateY(-5px); border-color: var(--game-color); box-shadow: 0 10px 30px rgba(0,0,0,0.05); }
.game-card-v2::after { content: ''; position: absolute; top: 0; right: 0; width: 40px; height: 40px; background: var(--game-color); opacity: 0.1; border-radius: 0 0 0 40px; }

.gc-icon-box { 
    width: 64px; height: 64px; background: #F8FAFC; border-radius: 18px; 
    display: flex; align-items: center; justify-content: center; color: var(--game-color);
}
.gc-info { flex: 1; }
.gc-info h3 { font-size: 1.1rem; font-weight: 900; color: #0F172A; margin-bottom: 4px; }
.gc-info p { font-size: 0.8rem; color: #64748B; margin-bottom: 16px; line-height: 1.4; }

.gc-footer { display: flex; justify-content: space-between; align-items: center; }
.puan-tag { font-size: 0.65rem; font-weight: 800; color: #10B981; background: rgba(16, 185, 129, 0.1); padding: 4px 10px; border-radius: 6px; }
.btn-play { background: #0F172A; color: #D4AF37; border: none; padding: 6px 14px; border-radius: 8px; font-weight: 800; font-size: 0.75rem; display: flex; align-items: center; gap: 6px; }

/* SHOP GRID */
.shop-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 24px; }
.shop-card-v2 { background: white; border-radius: 20px; padding: 20px; border: 1px solid #F1F5F9; transition: 0.2s; }
.shop-card-v2:hover { border-color: #D4AF37; transform: translateY(-3px); }

.sc-header { display: flex; justify-content: space-between; margin-bottom: 20px; }
.sc-category { font-size: 0.6rem; font-weight: 900; background: #F1F5F9; color: #64748B; padding: 4px 8px; border-radius: 6px; text-transform: uppercase; }
.sc-brand-icon { color: #0F172A; opacity: 0.8; }

.sc-body { margin-bottom: 24px; }
.sc-body small { display: block; font-size: 0.75rem; color: #94A3B8; font-weight: 700; margin-bottom: 4px; }
.sc-body h3 { font-size: 0.95rem; font-weight: 800; color: #0F172A; line-height: 1.4; }

.sc-footer { display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #F8FAFC; pt: 16px; }
.sc-cost { display: flex; align-items: center; gap: 6px; font-weight: 900; color: #D4AF37; font-size: 1rem; font-family: 'Outfit'; }
.btn-buy-v2 { background: #0F172A; color: #D4AF37; border: none; padding: 8px 20px; border-radius: 10px; font-weight: 800; cursor: pointer; }
.btn-buy-v2:disabled { background: #F1F5F9; color: #CBD5E1; cursor: not-allowed; }

/* GAME OVERLAY */
.game-overlay-v2 { 
    position: fixed; inset: 0; background: rgba(15, 23, 42, 0.6); backdrop-filter: blur(10px);
    display: flex; align-items: center; justify-content: center; z-index: 1000;
}
.game-window { width: 100%; max-width: 600px; background: white; border-radius: 32px; padding: 32px; }
.gw-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; }
.gw-header h2 { font-size: 1.5rem; font-weight: 900; }
.gw-header button { background: none; border: none; font-size: 2rem; cursor: pointer; color: #94A3B8; }

.game-placeholder { text-align: center; padding: 40px 0; }
.game-placeholder h3 { font-size: 1.4rem; font-weight: 900; margin-top: 24px; }
.game-placeholder p { color: #64748B; margin-bottom: 32px; }

.game-stats-mini { display: flex; justify-content: center; gap: 40px; }
.gsm-item span { display: block; font-size: 0.7rem; font-weight: 800; color: #94A3B8; margin-bottom: 6px; }
.gsm-item strong { font-size: 1.4rem; font-weight: 900; color: #0F172A; font-family: 'Outfit'; }

.btn-finish-game { 
    background: #0F172A; color: #D4AF37; border: none; padding: 16px 32px; 
    border-radius: 16px; font-weight: 900; cursor: pointer; width: 100%;
}
</style>
