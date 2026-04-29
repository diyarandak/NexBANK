<script setup lang="ts">
import { ref, computed } from 'vue';
import { 
  Bell, BellOff, CheckCheck, Info, 
  ArrowDownLeft, ArrowUpRight, AlertTriangle, 
  UserCircle, Calendar, Trash2, ChevronRight,
  ShieldAlert, Megaphone
} from 'lucide-vue-next';

const notifications = ref([
  { id: 1, type: 'transfer', title: 'Para Girişi', message: 'Diyar Andak tarafından hesabınıza 15.000,00 TL gönderildi.', date: 'Bugün, 14:20', read: false, importance: 'high' },
  { id: 2, type: 'staff', title: 'Personel Duyurusu', message: 'Yalova Şubemiz 29 Nisan tarihinde tadilat nedeniyle kapalı olacaktır. İşlemlerinizi dijital kanallardan yapabilirsiniz.', date: 'Bugün, 10:05', read: false, importance: 'normal' },
  { id: 3, type: 'security', title: 'Yeni Cihaz Girişi', message: 'Hesabınıza MacBook Pro üzerinden yeni bir giriş tespit edildi. Siz değilseniz lütfen şifrenizi sıfırlayın.', date: 'Dün, 22:45', read: true, importance: 'critical' },
  { id: 4, type: 'transfer', title: 'Otomatik Ödeme', message: 'Turkcell fatura ödemeniz (450,00 TL) başarıyla gerçekleşti.', date: 'Dün, 09:15', read: true, importance: 'normal' },
  { id: 5, type: 'system', title: 'Sistem Güncellemesi', message: 'NexBank mobil uygulaması artık daha hızlı! Yeni yatırım araçlarını keşfetmek için güncellemeyi unutmayın.', date: '2 gün önce', read: true, importance: 'low' },
]);

const activeFilter = ref('all');

const filteredNotifications = computed(() => {
    if (activeFilter.value === 'all') return notifications.value;
    if (activeFilter.value === 'unread') return notifications.value.filter(n => !n.read);
    return notifications.value.filter(n => n.type === activeFilter.value);
});

const markAllRead = () => {
    notifications.value.forEach(n => n.read = true);
};

const deleteNotification = (id: number) => {
    notifications.value = notifications.value.filter(n => n.id !== id);
};

const getIcon = (type: string) => {
    switch (type) {
        case 'transfer': return ArrowDownLeft;
        case 'staff': return Megaphone;
        case 'security': return ShieldAlert;
        default: return Info;
    }
};

const getTypeLabel = (type: string) => {
    switch (type) {
        case 'transfer': return 'Finansal';
        case 'staff': return 'Duyuru';
        case 'security': return 'Güvenlik';
        default: return 'Sistem';
    }
};
</script>

<template>
  <div class="view-container fade-in">
    <header class="notif-header mb-4">
      <div class="h-left">
        <h1 class="text-gradient-gold">Bildirim Merkezi</h1>
        <p class="subtitle">NexBank dünyasından ve hesabınızdan en son haberler.</p>
      </div>
      <div class="h-right">
        <button @click="markAllRead" class="btn-action">
            <CheckCheck :size="18" />
            Tümünü Okundu Say
        </button>
      </div>
    </header>

    <!-- FILTERS -->
    <div class="notif-filters mb-4">
        <button @click="activeFilter = 'all'" :class="{ active: activeFilter === 'all' }">Tümü</button>
        <button @click="activeFilter = 'unread'" :class="{ active: activeFilter === 'unread' }">
            Okunmamış
            <span class="badge" v-if="notifications.filter(n => !n.read).length > 0">
                {{ notifications.filter(n => !n.read).length }}
            </span>
        </button>
        <button @click="activeFilter = 'transfer'" :class="{ active: activeFilter === 'transfer' }">Finansal</button>
        <button @click="activeFilter = 'staff'" :class="{ active: activeFilter === 'staff' }">Duyurular</button>
    </div>

    <div class="notif-container">
        <div v-if="filteredNotifications.length === 0" class="empty-state">
            <BellOff :size="48" />
            <p>Görüntülenecek bildirim bulunamadı.</p>
        </div>

        <div v-for="n in filteredNotifications" :key="n.id" 
             class="notif-card" :class="[n.type, { unread: !n.read }]">
            <div class="nc-icon-box">
                <component :is="getIcon(n.type)" :size="20" />
            </div>
            
            <div class="nc-content">
                <div class="nc-top">
                    <span class="nc-type">{{ getTypeLabel(n.type) }}</span>
                    <span class="nc-date">{{ n.date }}</span>
                </div>
                <h3 class="nc-title">{{ n.title }}</h3>
                <p class="nc-msg">{{ n.message }}</p>
                
                <div class="nc-actions" v-if="!n.read">
                    <button class="btn-read" @click="n.read = true">Okundu İşaretle</button>
                </div>
            </div>

            <div class="nc-options">
                <button @click="deleteNotification(n.id)" class="btn-delete"><Trash2 :size="16" /></button>
                <ChevronRight :size="18" class="text-muted" />
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.notif-header { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2rem; font-weight: 900;
}

.btn-action { 
    background: #0F172A; color: #D4AF37; border: none; padding: 10px 18px; 
    border-radius: 12px; font-size: 0.85rem; font-weight: 800; display: flex; 
    align-items: center; gap: 8px; cursor: pointer; transition: 0.2s;
}
.btn-action:hover { transform: translateY(-2px); background: #000; }

.notif-filters { display: flex; gap: 10px; }
.notif-filters button { 
    background: white; border: 1px solid #F1F5F9; padding: 8px 16px; 
    border-radius: 10px; font-size: 0.8rem; font-weight: 700; color: #64748B; 
    cursor: pointer; display: flex; align-items: center; gap: 8px; transition: 0.2s;
}
.notif-filters button.active { background: #F1F5F9; color: #0F172A; border-color: #0F172A; }
.badge { background: #EF4444; color: white; font-size: 0.65rem; padding: 2px 6px; border-radius: 6px; }

.notif-container { display: flex; flex-direction: column; gap: 12px; }

.notif-card { 
    background: white; border-radius: 20px; padding: 20px; 
    border: 1px solid #F1F5F9; display: flex; gap: 20px; 
    transition: 0.2s; position: relative; overflow: hidden;
}
.notif-card:hover { border-color: #D4AF37; transform: translateX(5px); }
.notif-card.unread { background: #F8FAFC; border-left: 4px solid #D4AF37; }

.nc-icon-box { 
    width: 44px; height: 44px; border-radius: 12px; display: flex; 
    align-items: center; justify-content: center; flex-shrink: 0;
}
.transfer .nc-icon-box { background: #DCFCE7; color: #15803D; }
.staff .nc-icon-box { background: #E0F2FE; color: #0369A1; }
.security .nc-icon-box { background: #FEE2E2; color: #DC2626; }
.system .nc-icon-box { background: #F1F5F9; color: #64748B; }

.nc-content { flex: 1; }
.nc-top { display: flex; justify-content: space-between; margin-bottom: 4px; }
.nc-type { font-size: 0.65rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.5px; opacity: 0.6; }
.nc-date { font-size: 0.7rem; color: #94A3B8; font-weight: 600; }
.nc-title { font-size: 1rem; font-weight: 800; color: #0F172A; margin-bottom: 4px; }
.nc-msg { font-size: 0.85rem; color: #64748B; line-height: 1.5; }

.nc-actions { margin-top: 12px; }
.btn-read { background: none; border: none; color: #D4AF37; font-size: 0.75rem; font-weight: 800; cursor: pointer; padding: 0; }
.btn-read:hover { text-decoration: underline; }

.nc-options { display: flex; flex-direction: column; justify-content: space-between; align-items: flex-end; }
.btn-delete { background: none; border: none; color: #CBD5E1; cursor: pointer; padding: 4px; border-radius: 4px; transition: 0.2s; }
.btn-delete:hover { color: #EF4444; background: #FEF2F2; }

.empty-state { padding: 80px 0; text-align: center; color: #94A3B8; }
.empty-state p { margin-top: 12px; font-weight: 700; }
</style>
