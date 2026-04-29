<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../store/auth';
import { useRouter } from 'vue-router';
import Logo from './Logo.vue';
import { 
  LayoutDashboard, CreditCard, Wallet, Send, QrCode, Receipt, 
  History, Calendar, Briefcase, Landmark, TrendingUp, BarChart3, 
  ArrowLeftRight, Building2, PieChart, Bell, FileText, Settings, 
  LifeBuoy, Activity, CheckSquare, Zap, AlertTriangle, Users, 
  UserCheck, FileSpreadsheet, ShieldCheck, ClipboardList, Tag, 
  Gift, Megaphone, LogOut, ChevronDown, Coins, Camera, Gamepad2,
  ShoppingBag, ShieldAlert
} from 'lucide-vue-next';

const authStore = useAuthStore();
const router = useRouter();

const handleLogout = () => {
  authStore.logout();
  router.push('/login');
};

const openCategories = ref({
    // Müşteri Kategorileri
    main: true,
    transfers: false,
    credit: false,
    invest: false,
    support: false,
    // Personel Kategorileri
    ops: true,
    crm: false,
    finance: false,
    system: false,
    product: false
});

const toggle = (cat: string) => {
    // @ts-ignore
    openCategories.value[cat] = !openCategories.value[cat];
}
</script>

<template>
  <aside class="sidebar-premium">
    <!-- LOGO SECTION -->
    <div class="logo-section">
      <Logo :scale="0.8" />
      <div class="logo-text">
        <h1 class="font-outfit">NexBank</h1>
        <span class="badge-premium">{{ authStore.isAdmin ? 'STAFF' : 'PREMIUM' }}</span>
      </div>
    </div>

    <div class="nav-content">
        <!-- 👤 MÜŞTERİ PANELİ (Sadece Müşteriler) -->
        <template v-if="!authStore.isAdmin">
            <div class="category">
                <div class="cat-header" @click="toggle('main')">
                    <span>ANA SAYFA & HESAPLAR</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.main }" :size="14" />
                </div>
                <div v-if="openCategories.main" class="cat-items">
                    <router-link to="/dashboard" class="nav-item">
                        <LayoutDashboard :size="18" /> <span>Ana Sayfa</span>
                    </router-link>
                    <router-link to="/accounts" class="nav-item">
                        <Landmark :size="18" /> <span>Hesaplarım</span>
                    </router-link>
                    <router-link to="/cards" class="nav-item">
                        <CreditCard :size="18" /> <span>Kart Yönetimi</span>
                    </router-link>
                    <router-link to="/cash-advance" class="nav-item">
                        <Coins :size="18" /> <span>Nakit Avans</span>
                    </router-link>
                    <router-link to="/receipt-scanner" class="nav-item">
                        <Camera :size="18" /> <span>NexVision AI</span>
                    </router-link>
                    <router-link to="/wallet" class="nav-item">
                        <Wallet :size="18" /> <span>Dijital Cüzdan</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('transfers')">
                    <span>İŞLEMLER</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.transfers }" :size="14" />
                </div>
                <div v-if="openCategories.transfers" class="cat-items">
                    <router-link to="/transfer" class="nav-item">
                        <Send :size="18" /> <span>Para Transferi</span>
                    </router-link>
                    <router-link to="/qr-pay" class="nav-item">
                        <QrCode :size="18" /> <span>QR ile Öde</span>
                    </router-link>
                    <router-link to="/payments" class="nav-item">
                        <Receipt :size="18" /> <span>Fatura Öde</span>
                    </router-link>
                    <router-link to="/transactions" class="nav-item">
                        <History :size="18" /> <span>İşlem Geçmişi</span>
                    </router-link>
                    <router-link to="/scheduled-payments" class="nav-item">
                        <Calendar :size="18" /> <span>Zamanlanmış İşlemler</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('credit')">
                    <span>KREDİ & FİNANSMAN</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.credit }" :size="14" />
                </div>
                <div v-if="openCategories.credit" class="cat-items">
                    <router-link to="/loans" class="nav-item">
                        <Briefcase :size="18" /> <span>Kredi Başvurusu</span>
                    </router-link>
                    <router-link to="/my-applications" class="nav-item">
                        <ClipboardList :size="18" /> <span>Başvurularım</span>
                    </router-link>
                    <router-link to="/limit-request" class="nav-item">
                        <TrendingUp :size="18" /> <span>Limit Artırım</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('invest')">
                    <span>YATIRIM & BORSA</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.invest }" :size="14" />
                </div>
                <div v-if="openCategories.invest" class="cat-items">
                    <router-link to="/stocks" class="nav-item">
                        <BarChart3 :size="18" /> <span>Borsa İstanbul</span>
                    </router-link>
                    <router-link to="/exchange" class="nav-item">
                        <ArrowLeftRight :size="18" /> <span>Döviz & Altın</span>
                    </router-link>
                    <router-link to="/funds" class="nav-item">
                        <PieChart :size="18" /> <span>Fon Yönetimi</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('support')">
                    <span>DESTEK & DİĞER</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.support }" :size="14" />
                </div>
                <div v-if="openCategories.support" class="cat-items">
                    <router-link to="/notifications" class="nav-item">
                        <Bell :size="18" /> <span>Bildirimler</span>
                    </router-link>
                    <router-link to="/documents" class="nav-item">
                        <FileText :size="18" /> <span>Belgelerim</span>
                    </router-link>
                    <router-link to="/settings" class="nav-item">
                        <Settings :size="18" /> <span>Güvenlik & Ayarlar</span>
                    </router-link>
                    <router-link to="/support" class="nav-item">
                        <LifeBuoy :size="18" /> <span>Canlı Yardım</span>
                    </router-link>
                    <router-link to="/rewards" class="nav-item">
                        <Gamepad2 :size="18" /> <span>Eğlence & Ödüller</span>
                    </router-link>
                    <router-link to="/market" class="nav-item">
                        <ShoppingBag :size="18" /> <span>NexMarket</span>
                    </router-link>
                </div>
            </div>
        </template>

        <!-- 🏢 PERSONEL PANELİ -->
        <template v-else>
            <div class="category">
                <div class="cat-header" @click="toggle('ops')">
                    <span>OPERASYON</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.ops }" :size="14" />
                </div>
                <div v-if="openCategories.ops" class="cat-items">
                    <router-link to="/dashboard" class="nav-item">
                        <Activity :size="18" /> <span>Canlı Metrikler</span>
                    </router-link>
                    <router-link to="/approval-center" class="nav-item">
                        <CheckSquare :size="18" /> <span>Onay Merkezi</span>
                    </router-link>
                    <router-link to="/staff-transactions" class="nav-item">
                        <Zap :size="18" /> <span>İşlem Akışı</span>
                    </router-link>
                    <router-link to="/fraud-alerts" class="nav-item">
                        <AlertTriangle :size="18" /> <span>Şüpheli Uyarılar</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('crm')">
                    <span>MÜŞTERİ YÖNETİMİ</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.crm }" :size="14" />
                </div>
                <div v-if="openCategories.crm" class="cat-items">
                    <router-link to="/customers" class="nav-item">
                        <Users :size="18" /> <span>Müşterilerim</span>
                    </router-link>
                    <router-link to="/kyc-verification" class="nav-item">
                        <UserCheck :size="18" /> <span>KYC Onayları</span>
                    </router-link>
                    <router-link to="/staff-loans" class="nav-item">
                        <Briefcase :size="18" /> <span>Kredi Başvuruları</span>
                    </router-link>
                    <router-link to="/support" class="nav-item">
                        <LifeBuoy :size="18" /> <span>Canlı Destek</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('finance')">
                    <span>FİNANS & RAPORLAMA</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.finance }" :size="14" />
                </div>
                <div v-if="openCategories.finance" class="cat-items">
                    <router-link to="/daily-report" class="nav-item">
                        <FileSpreadsheet :size="18" /> <span>Günlük Rapor</span>
                    </router-link>
                    <router-link to="/analytics" class="nav-item">
                        <BarChart3 :size="18" /> <span>Analitik Panel</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('system')">
                    <span>SİSTEM & GÜVENLİK</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.system }" :size="14" />
                </div>
                <div v-if="openCategories.system" class="cat-items">
                    <router-link to="/audit-logs" class="nav-item">
                        <ShieldCheck :size="18" /> <span>Audit Log</span>
                    </router-link>
                    <router-link to="/staff-management" class="nav-item">
                        <Users :size="18" /> <span>Personel Yönetimi</span>
                    </router-link>
                    <router-link to="/system-logs" class="nav-item">
                        <ClipboardList :size="18" /> <span>Sistem Logları</span>
                    </router-link>
                </div>
            </div>

            <div class="category">
                <div class="cat-header" @click="toggle('product')">
                    <span>ÜRÜN & AYARLAR</span>
                    <ChevronDown class="chevron" :class="{ rotated: !openCategories.product }" :size="14" />
                </div>
                <div v-if="openCategories.product" class="cat-items">
                    <router-link to="/bank-settings" class="nav-item">
                        <Tag :size="18" /> <span>Faiz & Komisyon</span>
                    </router-link>
                    <router-link to="/campaign-management" class="nav-item">
                        <Gift :size="18" /> <span>Kampanyalar</span>
                    </router-link>
                    <router-link to="/broadcast" class="nav-item">
                        <Megaphone :size="18" /> <span>Duyuru Gönder</span>
                    </router-link>
                </div>
            </div>
        </template>
    </div>

    <!-- FOOTER SECTION -->
    <div class="sidebar-footer">
        <div class="user-pill-premium">
            <div class="avatar-box">{{ authStore.user?.fullName.charAt(0) }}</div>
            <div class="u-info">
                <p class="u-name">{{ authStore.user?.fullName }}</p>
                <p class="u-branch">{{ authStore.user?.branch }}</p>
            </div>
            <button @click="handleLogout" class="lg-btn" title="Çıkış">
                <LogOut :size="18" color="#EF4444" />
            </button>
        </div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar-premium {
  width: var(--sidebar-width);
  height: 100vh;
  background: var(--bg-sidebar);
  display: flex;
  flex-direction: column;
  position: fixed;
  left: 0;
  top: 0;
  z-index: 1000;
  box-shadow: 10px 0 30px rgba(0,0,0,0.15);
}

.logo-section { 
    padding: 32px 24px; 
    display: flex; 
    flex-direction: column;
    align-items: center; 
    gap: 12px; 
    border-bottom: 1px solid rgba(255,255,255,0.05); 
    text-align: center;
}

.logo-text h1 { 
  font-size: 1.5rem; 
  color: white; 
  margin: 0; 
  letter-spacing: 1px; 
  font-weight: 800; 
}

.badge-premium { 
    font-size: 0.65rem; 
    background: var(--primary); 
    color: var(--primary-dark); 
    padding: 3px 10px; 
    border-radius: 6px; 
    font-weight: 900;
    text-transform: uppercase;
}

.nav-content { 
  flex: 1; 
  overflow-y: auto; 
  padding: 24px 16px; 
  scrollbar-width: none; 
}
.nav-content::-webkit-scrollbar { display: none; }

.category { margin-bottom: 20px; }

.cat-header { 
    display: flex; 
    justify-content: space-between; 
    align-items: center;
    padding: 12px 12px; 
    font-size: 0.7rem; 
    font-weight: 800; 
    color: rgba(255,255,255,0.4); 
    letter-spacing: 1.2px; 
    cursor: pointer;
    transition: all 0.2s;
}
.cat-header:hover { color: var(--primary); }

.chevron { transition: transform 0.3s; color: rgba(255,255,255,0.3); }
.chevron.rotated { transform: rotate(-90deg); }

.cat-items { 
  display: flex; 
  flex-direction: column; 
  gap: 4px; 
  margin-top: 8px; 
}

.nav-item {
  display: flex; 
  align-items: center; 
  gap: 14px; 
  padding: 12px 16px;
  color: rgba(255,255,255,0.6); 
  text-decoration: none; 
  font-size: 0.875rem; 
  font-weight: 500;
  border-radius: 12px; 
  transition: all 0.2s;
}

.nav-item:hover { 
  background: rgba(255,255,255,0.05); 
  color: white; 
}

.nav-item.router-link-active { 
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: var(--primary-dark); 
  font-weight: 700;
  box-shadow: 0 4px 12px rgba(197, 160, 58, 0.2);
}

.sidebar-footer { 
  padding: 20px; 
  border-top: 1px solid rgba(255,255,255,0.05); 
}

.user-pill-premium { 
  display: flex; 
  align-items: center; 
  gap: 12px; 
  padding: 12px; 
  background: rgba(255,255,255,0.03); 
  border-radius: 16px; 
  border: 1px solid rgba(255,255,255,0.05);
}

.avatar-box { 
  width: 38px; 
  height: 38px; 
  background: var(--primary); 
  color: var(--primary-dark); 
  border-radius: 10px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-weight: 800; 
}

.u-info { flex: 1; min-width: 0; }
.u-name { 
  font-size: 0.85rem; 
  font-weight: 600; 
  color: white; 
  white-space: nowrap; 
  overflow: hidden; 
  text-overflow: ellipsis; 
  margin: 0; 
}
.u-branch { font-size: 0.7rem; color: rgba(255,255,255,0.4); margin: 0; }

.lg-btn { 
  background: transparent; 
  border: none; 
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer; 
  transition: transform 0.2s; 
  padding: 8px;
  border-radius: 8px;
}
.lg-btn:hover { 
  transform: scale(1.1); 
  background: rgba(239, 68, 68, 0.1);
}
</style>

