<script setup lang="ts">
import { ref, computed } from 'vue';
import { 
  ShoppingBag, Search, Ticket, Gift, 
  Gamepad, Coffee, Music, Play, 
  ChevronRight, Star, Sparkles, Zap,
  ShoppingBasket, CreditCard, Wallet,
  Smartphone, Monitor, Tag
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const searchQuery = ref('');
const activeCategory = ref('Tümü');
const nexPoints = ref(12450);

const categories = [
  { name: 'Tümü', icon: ShoppingBag },
  { name: 'Oyun', icon: Gamepad },
  { name: 'Eğlence', icon: Play },
  { name: 'Yiyecek', icon: Coffee },
  { name: 'Teknoloji', icon: Monitor }
];

const products = ref([
  { id: 1, name: 'Steam 100 TL Cüzdan Kodu', brand: 'Steam', price: 100, points: 5000, cat: 'Oyun', img: '🎮', color: '#171a21' },
  { id: 2, name: 'Netflix 1 Aylık Standart', brand: 'Netflix', price: 149, points: 7500, cat: 'Eğlence', img: '🎬', color: '#e50914' },
  { id: 3, name: 'Starbucks 50 TL Bakiye', brand: 'Starbucks', price: 50, points: 2500, cat: 'Yiyecek', img: '☕', color: '#00704a' },
  { id: 4, name: 'Spotify 3 Aylık Premium', brand: 'Spotify', price: 119, points: 6000, cat: 'Eğlence', img: '🎵', color: '#1db954' },
  { id: 5, name: 'Google Play 50 TL Kod', brand: 'Google', price: 50, points: 2500, cat: 'Oyun', img: '📱', color: '#4285f4' },
  { id: 6, name: 'Amazon 200 TL Hediye Çeki', brand: 'Amazon', price: 200, points: 10000, cat: 'Tümü', img: '📦', color: '#ff9900' }
]);

const filteredProducts = computed(() => {
    return products.value.filter(p => {
        const matchesSearch = p.name.toLowerCase().includes(searchQuery.value.toLowerCase());
        const matchesCat = activeCategory.value === 'Tümü' || p.cat === activeCategory.value;
        return matchesSearch && matchesCat;
    });
});

const buyWithPoints = (p: any) => {
    if (nexPoints.value >= p.points) {
        nexPoints.value -= p.points;
        toast.success(`${p.name} başarıyla alındı! Kodunuz SMS ile gönderildi.`);
    } else {
        toast.error("Yetersiz NexPuan! NexBird oynayarak puan kazanabilirsin.");
    }
};

const buyWithMoney = (p: any) => {
    toast.success(`${p.name} satın alma işlemi başlatıldı. Lütfen hesap seçin.`);
    // Buradan bir ödeme modalına yönlendirilebilir
};
</script>

<template>
  <div class="view-container fade-in">
    <header class="market-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">NexMarket</h1>
        <p class="subtitle">NexPuanlarınızı harcayın veya ayrıcalıklı fiyatlarla hediye kartları alın.</p>
      </div>
      <div class="points-badge-p">
          <div class="p-icon-box"><Sparkles :size="18" /></div>
          <div class="p-text">
              <small>NEXPUAN BAKİYE</small>
              <strong>{{ nexPoints.toLocaleString() }}</strong>
          </div>
          <button class="btn-earn" @click="$router.push('/rewards')">PUAN KAZAN</button>
      </div>
    </header>

    <!-- SEARCH & CATEGORIES -->
    <div class="market-nav mt-4">
        <div class="search-bar-v5">
            <Search :size="18" />
            <input v-model="searchQuery" type="text" placeholder="Marka veya ürün ara..." />
        </div>
        <div class="categories-v5">
            <button v-for="cat in categories" :key="cat.name" 
                    @click="activeCategory = cat.name"
                    :class="['cat-btn-v5', { active: activeCategory === cat.name }]">
                <component :is="cat.icon" :size="16" />
                <span>{{ cat.name }}</span>
            </button>
        </div>
    </div>

    <!-- PRODUCTS GRID -->
    <div class="products-grid-p mt-5">
        <div v-for="p in filteredProducts" :key="p.id" class="glass-card-v5 product-card-p">
            <div class="p-brand-tag" :style="{ background: p.color }">{{ p.brand }}</div>
            <div class="p-visual" :style="{ background: p.color + '10' }">
                <span class="p-emoji">{{ p.img }}</span>
                <div class="p-glow" :style="{ background: p.color }"></div>
            </div>
            <div class="p-info-p">
                <h3>{{ p.name }}</h3>
                <div class="p-tags">
                    <span class="p-tag"><Tag :size="12" /> {{ p.cat }}</span>
                    <span class="p-tag"><Zap :size="12" /> Anında Teslim</span>
                </div>
                
                <div class="p-pricing mt-4">
                    <div class="price-money">
                        <small>NAKİT</small>
                        <strong>{{ p.price }} TL</strong>
                    </div>
                    <div class="price-divider"></div>
                    <div class="price-points">
                        <small>NEXPUAN</small>
                        <strong>{{ p.points.toLocaleString() }}</strong>
                    </div>
                </div>

                <div class="p-actions mt-4">
                    <button @click="buyWithPoints(p)" class="btn-p-buy points">Puanla Al</button>
                    <button @click="buyWithMoney(p)" class="btn-p-buy cash">Satın Al</button>
                </div>
            </div>
        </div>
    </div>

    <!-- MY CODES SECTION -->
    <div class="my-codes-panel mt-5">
        <div class="glass-card-v5 codes-card-p">
            <div class="cc-header">
                <div class="cch-left">
                    <Ticket :size="24" />
                    <h3>Dijital Kodlarım</h3>
                </div>
                <button class="btn-link">Tümünü Gör <ChevronRight :size="16" /></button>
            </div>
            <div class="codes-empty mt-4">
                <div class="ce-icon"><Gift :size="32" /></div>
                <p>Henüz aktif bir dijital kodunuz bulunmuyor.</p>
                <span>Satın aldığınız tüm kodlar burada güvenle saklanır.</span>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.market-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }

.points-badge-p { background: #0C1B33; padding: 8px 8px 8px 20px; border-radius: 100px; display: flex; align-items: center; gap: 15px; color: white; border: 1.5px solid var(--gold); }
.p-icon-box { color: var(--gold); }
.p-text small { display: block; font-size: 0.6rem; font-weight: 800; opacity: 0.6; letter-spacing: 1px; }
.p-text strong { font-size: 1.2rem; font-family: 'Outfit'; color: var(--gold); }
.btn-earn { background: var(--gold); color: #0C1B33; border: none; padding: 10px 18px; border-radius: 100px; font-weight: 900; font-size: 0.75rem; cursor: pointer; transition: 0.2s; }
.btn-earn:hover { transform: scale(1.05); }

.market-nav { display: flex; justify-content: space-between; align-items: center; gap: 24px; }
.search-bar-v5 { flex: 1; background: white; border: 1.5px solid #E2E8F0; border-radius: 14px; padding: 12px 20px; display: flex; align-items: center; gap: 12px; }
.search-bar-v5 input { flex: 1; border: none; outline: none; font-weight: 600; color: var(--primary-dark); }
.search-bar-v5 svg { color: #94A3B8; }

.categories-v5 { display: flex; gap: 10px; }
.cat-btn-v5 { background: white; border: 1.5px solid #E2E8F0; padding: 10px 20px; border-radius: 14px; display: flex; align-items: center; gap: 10px; font-weight: 800; font-size: 0.85rem; color: #64748B; cursor: pointer; transition: 0.2s; }
.cat-btn-v5.active { background: #0C1B33; color: white; border-color: #0C1B33; }
.cat-btn-v5:hover:not(.active) { border-color: var(--primary); color: var(--primary); }

.products-grid-p { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 24px; }

.product-card-p { padding: 0; overflow: hidden; border: 1.5px solid rgba(0,0,0,0.03); display: flex; flex-direction: column; transition: 0.3s; }
.product-card-p:hover { transform: translateY(-8px); box-shadow: var(--shadow-lg); border-color: rgba(197, 160, 89, 0.2); }

.p-brand-tag { position: absolute; top: 12px; left: 12px; padding: 4px 12px; border-radius: 100px; color: white; font-size: 0.65rem; font-weight: 900; z-index: 2; text-transform: uppercase; }

.p-visual { height: 160px; display: flex; align-items: center; justify-content: center; position: relative; overflow: hidden; }
.p-emoji { font-size: 4rem; z-index: 1; }
.p-glow { position: absolute; width: 100px; height: 100px; filter: blur(50px); opacity: 0.2; }

.p-info-p { padding: 24px; flex: 1; display: flex; flex-direction: column; }
.p-info-p h3 { margin: 0; font-size: 1rem; color: var(--primary-dark); font-weight: 800; line-height: 1.4; height: 2.8rem; overflow: hidden; }
.p-tags { display: flex; gap: 8px; margin-top: 12px; }
.p-tag { font-size: 0.65rem; font-weight: 800; color: #94A3B8; background: #F1F5F9; padding: 4px 10px; border-radius: 6px; display: flex; align-items: center; gap: 4px; }

.p-pricing { display: flex; align-items: center; background: #F8FAFC; border-radius: 12px; padding: 12px; gap: 15px; }
.price-money, .price-points { flex: 1; text-align: center; }
.p-pricing small { display: block; font-size: 0.55rem; font-weight: 800; color: #94A3B8; letter-spacing: 0.5px; }
.p-pricing strong { font-size: 1rem; color: var(--primary-dark); font-family: 'Outfit'; }
.price-divider { width: 1px; height: 20px; background: #E2E8F0; }
.price-points strong { color: var(--gold-dark); }

.p-actions { display: flex; gap: 10px; }
.btn-p-buy { flex: 1; padding: 12px; border-radius: 10px; border: none; font-weight: 800; font-size: 0.8rem; cursor: pointer; transition: 0.2s; }
.btn-p-buy.points { background: #0C1B33; color: var(--gold); }
.btn-p-buy.cash { background: #F1F5F9; color: var(--primary-dark); }
.btn-p-buy:hover { transform: translateY(-2px); filter: brightness(1.1); }

/* MY CODES */
.codes-card-p { padding: 32px; border: 1.5px dashed #E2E8F0; background: rgba(255,255,255,0.3); }
.cc-header { display: flex; justify-content: space-between; align-items: center; }
.cch-left { display: flex; align-items: center; gap: 16px; color: var(--primary); }
.cch-left h3 { margin: 0; font-size: 1.2rem; color: var(--primary-dark); font-weight: 800; }
.btn-link { background: none; border: none; color: var(--primary); font-weight: 800; font-size: 0.85rem; cursor: pointer; display: flex; align-items: center; gap: 4px; }

.codes-empty { text-align: center; padding: 20px 0; }
.ce-icon { color: #CBD5E1; margin-bottom: 16px; }
.codes-empty p { font-size: 1rem; font-weight: 700; color: #64748B; margin: 0; }
.codes-empty span { font-size: 0.8rem; color: #94A3B8; font-weight: 600; margin-top: 4px; display: block; }
</style>
