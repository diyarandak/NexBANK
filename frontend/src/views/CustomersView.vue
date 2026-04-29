<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';
import { 
  Users, UserCheck, TrendingUp, Search, 
  ExternalLink, MoreHorizontal, ShieldCheck,
  CreditCard, Wallet, Activity, Mail, 
  Phone, MapPin, X, ChevronRight,
  Target, Award, Star, AlertCircle
} from 'lucide-vue-next';

const authStore = useAuthStore();
const customers = ref<any[]>([]);
const loading = ref(true);
const searchQuery = ref('');
const selectedCustomer = ref<any>(null);
const showProfile = ref(false);
let refreshInterval: any = null;

const fetchCustomers = async () => {
  try {
    const res = await apiClient.get('/users/customers');
    customers.value = res.data;
  } catch (err) {
    console.error('Müşteri listesi hatası:', err);
  } finally {
    loading.value = false;
  }
};

const viewCustomer360 = async (cust: any) => {
  try {
    const res = await apiClient.get(`/users/customer-360/${cust.id}`);
    selectedCustomer.value = {
        ...cust,
        ...res.data,
        riskScore: Math.floor(Math.random() * (990 - 300) + 300), // Simüle edilmiş FICO skoru
        vipStatus: Math.random() > 0.8 ? 'VIP' : 'Standart'
    };
    showProfile.value = true;
  } catch (err) {
    // Demo verisi oluştur (Eğer API henüz tam hazır değilse)
    selectedCustomer.value = {
        ...cust,
        riskScore: 842,
        vipStatus: 'VIP',
        accounts: [
            { iban: 'TR12 0001 ... 4422', balance: 125000, type: 'Bireysel TRY' },
            { iban: 'TR34 0001 ... 1199', balance: 12450, type: 'Döviz USD' }
        ]
    };
    showProfile.value = true;
  }
};

const filteredCustomers = computed(() => {
  if (!searchQuery.value) return customers.value;
  const q = searchQuery.value.toLowerCase();
  return customers.value.filter(c => 
    c.fullName.toLowerCase().includes(q) || 
    c.email.toLowerCase().includes(q) ||
    c.tcKimlikNo?.includes(q)
  );
});

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

onMounted(() => {
  fetchCustomers();
  refreshInterval = setInterval(fetchCustomers, 20000);
});

onUnmounted(() => {
  if (refreshInterval) clearInterval(refreshInterval);
});
</script>

<template>
  <div class="view-container fade-in">
    <!-- DASHBOARD HEADER -->
    <header class="crm-header mb-5">
      <div class="h-left">
        <h1 class="text-gradient-gold">Şubemdeki Müşteriler</h1>
        <p class="subtitle">{{ authStore.user?.branch || 'Yalova Şubesi' }} Portföy Yönetimi</p>
      </div>
      <div class="h-right">
         <div class="glass-search">
            <Search :size="18" />
            <input v-model="searchQuery" type="text" placeholder="Müşteri ismi, TCKN veya E-posta..." />
         </div>
      </div>
    </header>

    <!-- PORTFOLIO STATS -->
    <div class="portfolio-stats mb-5">
        <div class="stat-card-p">
            <div class="s-icon"><Users :size="20" /></div>
            <div class="s-info">
                <small>TOPLAM MÜŞTERİ</small>
                <strong>{{ customers.length }}</strong>
            </div>
        </div>
        <div class="stat-card-p gold-theme">
            <div class="s-icon"><Award :size="20" /></div>
            <div class="s-info">
                <small>VIP PORTFÖYÜ</small>
                <strong>{{ Math.floor(customers.length * 0.2) }} Kişi</strong>
            </div>
        </div>
        <div class="stat-card-p">
            <div class="s-icon"><TrendingUp :size="20" /></div>
            <div class="s-info">
                <small>AKTİF KULLANIM</small>
                <strong>%94.2</strong>
            </div>
        </div>
    </div>

    <div v-if="loading" class="loader-container"><div class="glow-loader"></div></div>

    <!-- CUSTOMER TABLE -->
    <div v-else class="glass-card-elite overflow-hidden">
       <table class="crm-table">
          <thead>
             <tr>
                <th>MÜŞTERİ BİLGİSİ</th>
                <th>KİMLİK NUMARASI</th>
                <th>SİSTEM KAYDI</th>
                <th>DURUM</th>
                <th>AKSİYON</th>
             </tr>
          </thead>
          <tbody>
             <tr v-for="c in filteredCustomers" :key="c.id" class="table-row-hover">
                <td>
                   <div class="crm-user">
                      <div class="crm-avatar" :class="{ 'vip': Math.random() > 0.8 }">
                          {{ c.fullName.charAt(0) }}
                      </div>
                      <div class="crm-meta">
                         <span class="crm-name">{{ c.fullName }}</span>
                         <span class="crm-email">{{ c.email }}</span>
                      </div>
                   </div>
                </td>
                <td><code class="tc-tag">{{ c.tcKimlikNo || '---' }}</code></td>
                <td>
                    <div class="reg-date">
                        <Activity :size="14" />
                        {{ Math.floor((new Date().getTime() - new Date(c.createdAt).getTime()) / (1000 * 60 * 60 * 24)) }} Gündür NexBank'lı
                    </div>
                </td>
                <td>
                    <span class="status-pill" :class="Math.random() > 0.7 ? 'active' : 'new'">
                        {{ Math.random() > 0.7 ? 'Aktif' : 'Yeni Kayıt' }}
                    </span>
                </td>
                <td>
                   <button @click="viewCustomer360(c)" class="btn-profile-detail">
                       PROFİL DETAYI <ChevronRight :size="14" />
                   </button>
                </td>
             </tr>
          </tbody>
       </table>
       <div v-if="filteredCustomers.length === 0" class="empty-results">
           <Search :size="48" />
           <p>Aradığınız kriterlere uygun müşteri bulunamadı.</p>
       </div>
    </div>

    <!-- 🏦 CUSTOMER 360 PANEL (SLIDE-OVER) -->
    <Transition name="slide-fade">
        <div v-if="showProfile" class="customer-360-overlay" @click.self="showProfile = false">
           <div class="customer-360-panel glass-card-elite">
              <header class="panel-header">
                 <button class="panel-close" @click="showProfile = false"><X :size="20" /></button>
                 <div class="panel-top">
                    <div class="panel-avatar-big" :class="{ 'vip': selectedCustomer?.vipStatus === 'VIP' }">
                        {{ selectedCustomer?.fullName.charAt(0) }}
                    </div>
                    <h2>{{ selectedCustomer?.fullName }}</h2>
                    <div class="vip-badge" v-if="selectedCustomer?.vipStatus === 'VIP'"><Star :size="12" /> VIP MÜŞTERİ</div>
                    <span class="panel-subtitle">NexBank {{ authStore.user?.branch }} Üyesi</span>
                 </div>
              </header>

              <div class="panel-content">
                 <!-- QUICK STATS -->
                 <div class="panel-metrics">
                    <div class="p-metric">
                        <small>RİSK SKORU</small>
                        <div class="score-box" :style="{ color: selectedCustomer?.riskScore > 700 ? '#10B981' : '#F59E0B' }">
                            {{ selectedCustomer?.riskScore }}
                        </div>
                        <div class="score-bar-bg"><div class="score-fill-bar" :style="{ width: (selectedCustomer?.riskScore / 1000 * 100) + '%', background: selectedCustomer?.riskScore > 700 ? '#10B981' : '#F59E0B' }"></div></div>
                    </div>
                    <div class="p-metric">
                        <small>TOPLAM VARLIK</small>
                        <div class="total-asset-val">{{ formatCurrency(selectedCustomer?.accounts?.reduce((acc: any, a: any) => acc + a.balance, 0) || 0) }}</div>
                    </div>
                 </div>

                 <!-- CONTACT INFO -->
                 <div class="contact-section mt-4">
                    <div class="c-item"><Mail :size="16" /> <span>{{ selectedCustomer?.email }}</span></div>
                    <div class="c-item"><Phone :size="16" /> <span>+90 5XX XXX XX XX</span></div>
                    <div class="c-item"><MapPin :size="16" /> <span>{{ selectedCustomer?.branch }} / Türkiye</span></div>
                 </div>

                 <!-- ACCOUNTS -->
                 <div class="accounts-summary mt-5">
                    <div class="as-header">
                        <h4>MÜŞTERİ HESAPLARI</h4>
                        <span class="count-badge">{{ selectedCustomer?.accounts?.length || 0 }}</span>
                    </div>
                    <div class="as-list mt-3">
                        <div v-for="a in selectedCustomer?.accounts" :key="a.iban" class="as-card">
                           <div class="as-icon"><Wallet :size="20" /></div>
                           <div class="as-details">
                              <strong>{{ a.type || (a.accountType === 0 ? 'Bireysel TRY' : 'Döviz') }}</strong>
                              <code>{{ a.iban }}</code>
                           </div>
                           <div class="as-bal">{{ formatCurrency(a.balance) }}</div>
                        </div>
                        <div v-if="!selectedCustomer?.accounts?.length" class="no-accounts">Henüz bir hesap açılmamış.</div>
                    </div>
                 </div>

                 <!-- ACTION HUB -->
                 <div class="panel-actions mt-5">
                    <button class="btn-panel-action primary"><Target :size="18" /> ÖZEL KREDİ TEKLİFİ YAP</button>
                    <button class="btn-panel-action danger"><AlertCircle :size="18" /> HESABI DONDUR (GÜVENLİK)</button>
                 </div>
              </div>
           </div>
        </div>
    </Transition>
  </div>
</template>

<style scoped>
.crm-header { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2.2rem; font-weight: 900;
}

.glass-search { 
    background: white; border: 1px solid #F1F5F9; padding: 12px 24px; 
    border-radius: 100px; display: flex; align-items: center; gap: 12px; width: 400px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.03); transition: 0.3s;
}
.glass-search:focus-within { border-color: #D4AF37; transform: translateY(-2px); }
.glass-search input { border: none; outline: none; width: 100%; font-weight: 600; font-size: 0.9rem; }

/* STATS */
.portfolio-stats { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
.stat-card-p { background: white; padding: 20px; border-radius: 20px; border: 1px solid #F1F5F9; display: flex; align-items: center; gap: 16px; }
.stat-card-p.gold-theme { background: #0F172A; color: white; border-color: #D4AF37; }
.s-icon { width: 44px; height: 44px; background: #F8FAFC; border-radius: 12px; display: flex; align-items: center; justify-content: center; color: #0F172A; }
.gold-theme .s-icon { background: rgba(212, 175, 55, 0.1); color: #D4AF37; }
.s-info small { display: block; font-size: 0.65rem; font-weight: 900; color: #94A3B8; letter-spacing: 1px; }
.gold-theme .s-info small { color: rgba(255,255,255,0.5); }
.s-info strong { font-size: 1.3rem; font-weight: 900; font-family: 'Outfit'; }

/* TABLE */
.crm-table { width: 100%; border-collapse: collapse; }
.crm-table th { padding: 20px; background: #F8FAFC; color: #64748B; font-size: 0.7rem; font-weight: 900; text-transform: uppercase; letter-spacing: 1px; text-align: left; }
.crm-table td { padding: 20px; border-bottom: 1px solid #F8FAFC; font-size: 0.9rem; vertical-align: middle; }

.table-row-hover:hover { background: rgba(248, 250, 252, 0.8); }

.crm-user { display: flex; align-items: center; gap: 14px; }
.crm-avatar { 
    width: 44px; height: 44px; border-radius: 12px; background: #0F172A; color: #D4AF37; 
    display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 1.1rem;
    border: 2px solid transparent;
}
.crm-avatar.vip { border-color: #D4AF37; box-shadow: 0 0 10px rgba(212, 175, 55, 0.3); }

.crm-name { display: block; font-weight: 800; color: #0F172A; }
.crm-email { font-size: 0.75rem; color: #94A3B8; font-weight: 600; }

.tc-tag { background: #F1F5F9; padding: 4px 10px; border-radius: 6px; font-weight: 700; font-size: 0.8rem; color: #475569; }

.reg-date { display: flex; align-items: center; gap: 8px; font-size: 0.75rem; color: #64748B; font-weight: 700; }

.status-pill { font-size: 0.65rem; font-weight: 900; padding: 4px 12px; border-radius: 100px; text-transform: uppercase; }
.status-pill.active { background: #DCFCE7; color: #166534; }
.status-pill.new { background: #E0F2FE; color: #0369A1; }

.btn-profile-detail { 
    background: #0F172A; color: #D4AF37; border: none; padding: 10px 16px; 
    border-radius: 10px; font-size: 0.7rem; font-weight: 900; cursor: pointer;
    display: flex; align-items: center; gap: 8px; transition: 0.2s;
}
.btn-profile-detail:hover { background: #000; transform: translateX(3px); }

/* PANEL (360 VIEW) */
.customer-360-overlay { position: fixed; inset: 0; background: rgba(15, 23, 42, 0.4); backdrop-filter: blur(10px); z-index: 1000; display: flex; justify-content: flex-end; }
.customer-360-panel { width: 500px; background: white; height: 100vh; display: flex; flex-direction: column; box-shadow: -20px 0 50px rgba(0,0,0,0.15); border: none; }

.panel-header { padding: 40px; text-align: center; position: relative; border-bottom: 1px solid #F1F5F9; }
.panel-close { position: absolute; top: 20px; right: 20px; background: #F8FAFC; border: none; width: 40px; height: 40px; border-radius: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center; color: #64748B; transition: 0.2s; }
.panel-close:hover { background: #FEE2E2; color: #EF4444; }

.panel-avatar-big { width: 100px; height: 100px; background: #0F172A; color: #D4AF37; border-radius: 30px; display: flex; align-items: center; justify-content: center; font-size: 2.5rem; font-weight: 900; margin: 0 auto 20px; }
.panel-avatar-big.vip { border: 4px solid #D4AF37; }

.vip-badge { background: #0F172A; color: #D4AF37; font-size: 0.65rem; font-weight: 900; padding: 4px 12px; border-radius: 100px; width: fit-content; margin: 0 auto 10px; display: flex; align-items: center; gap: 6px; }
.panel-top h2 { font-size: 1.8rem; font-weight: 900; color: #0F172A; margin: 0 0 8px 0; }
.panel-subtitle { font-size: 0.9rem; color: #94A3B8; font-weight: 600; }

.panel-content { flex: 1; overflow-y: auto; padding: 40px; }

.panel-metrics { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.p-metric { background: #F8FAFC; padding: 20px; border-radius: 20px; text-align: center; }
.p-metric small { display: block; font-size: 0.65rem; font-weight: 900; color: #94A3B8; margin-bottom: 12px; letter-spacing: 0.5px; }
.score-box { font-size: 2.2rem; font-weight: 900; font-family: 'Outfit'; margin-bottom: 12px; }
.score-bar-bg { height: 6px; background: #E2E8F0; border-radius: 10px; }
.score-fill-bar { height: 100%; border-radius: 10px; }
.total-asset-val { font-size: 1.3rem; font-weight: 900; color: #0F172A; font-family: 'Outfit'; margin-top: 10px; }

.contact-section { display: flex; flex-direction: column; gap: 12px; padding: 20px; background: #F8FAFC; border-radius: 20px; }
.c-item { display: flex; align-items: center; gap: 12px; font-size: 0.85rem; color: #475569; font-weight: 600; }

.as-header { display: flex; justify-content: space-between; align-items: center; }
.as-header h4 { font-size: 0.8rem; font-weight: 900; color: #0F172A; margin: 0; letter-spacing: 0.5px; }
.count-badge { background: #F1F5F9; padding: 2px 8px; border-radius: 6px; font-size: 0.7rem; font-weight: 900; }

.as-card { display: flex; align-items: center; gap: 16px; padding: 16px; background: white; border: 1px solid #F1F5F9; border-radius: 16px; margin-bottom: 12px; }
.as-icon { width: 40px; height: 40px; background: #F8FAFC; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: #0F172A; }
.as-details { flex: 1; }
.as-details strong { display: block; font-size: 0.85rem; color: #0F172A; }
.as-details code { font-size: 0.7rem; color: #94A3B8; }
.as-bal { font-weight: 800; color: #0F172A; font-family: 'Outfit'; }

.panel-actions { display: flex; flex-direction: column; gap: 12px; }
.btn-panel-action { padding: 16px; border-radius: 16px; border: none; font-weight: 900; font-size: 0.8rem; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 12px; transition: 0.3s; }
.btn-panel-action.primary { background: #0F172A; color: #D4AF37; box-shadow: 0 10px 20px rgba(15, 23, 42, 0.1); }
.btn-panel-action.danger { background: #FEF2F2; color: #EF4444; }
.btn-panel-action:hover { transform: translateY(-3px); }

/* ANIMATIONS */
.slide-fade-enter-active { transition: all 0.4s ease-out; }
.slide-fade-leave-active { transition: all 0.4s ease-in; }
.slide-fade-enter-from, .slide-fade-leave-to { transform: translateX(500px); opacity: 0; }

.empty-results { text-align: center; padding: 100px; color: #CBD5E1; }
.empty-results p { margin-top: 20px; font-weight: 700; }
</style>
