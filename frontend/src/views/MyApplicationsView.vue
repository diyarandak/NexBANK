<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';
import { 
  ClipboardList, Briefcase, CreditCard, 
  TrendingUp, Clock, CheckCircle2, 
  XCircle, AlertCircle, ChevronRight,
  Filter, Search, RefreshCw
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const loading = ref(true);
const activeFilter = ref('All');

// Başvurular listesi
const applications = ref<any[]>([]);

const fetchApplications = async () => {
    loading.value = true;
    try {
        // Kredi başvurularını çek
        const loanRes = await apiClient.get('/Loans');
        const loans = (Array.isArray(loanRes.data) ? loanRes.data : []).map(l => ({
            ...l,
            category: 'Loan',
            title: 'Kredi Başvurusu',
            subtitle: l.loanType,
            amount: l.amount,
            date: l.createdAt
        }));

        // Limit taleplerini çek (Mock/Simülasyon - Backend desteğine göre güncellenebilir)
        const mockLimitRequests = [
            { id: 'lr1', category: 'Limit', title: 'Limit Artırım Talebi', subtitle: 'Visa Platinum', amount: 50000, status: 'Pending', date: new Date().toISOString() }
        ];

        // Kart başvurularını çek
        const mockCardApps = [
            { id: 'ca1', category: 'Card', title: 'Yeni Kart Başvurusu', subtitle: 'NexGold Mastercard', amount: 0, status: 'Approved', date: new Date(Date.now() - 86400000).toISOString() }
        ];

        applications.value = [...loans, ...mockLimitRequests, ...mockCardApps].sort((a, b) => 
            new Date(b.date).getTime() - new Date(a.date).getTime()
        );
    } catch (err) {
        console.error('Başvurular yüklenemedi:', err);
        toast.error('Başvuru geçmişi şu an alınamıyor.');
    } finally {
        loading.value = false;
    }
};

const getStatusDetails = (status: string) => {
    switch (status) {
        case 'Pending': return { label: 'İnceleniyor', color: '#F59E0B', icon: Clock, bg: 'rgba(245, 158, 11, 0.1)' };
        case 'Approved': return { label: 'Onaylandı', color: '#10B981', icon: CheckCircle2, bg: 'rgba(16, 185, 129, 0.1)' };
        case 'Rejected': return { label: 'Reddedildi', color: '#EF4444', icon: XCircle, bg: 'rgba(239, 68, 68, 0.1)' };
        default: return { label: status, color: '#64748B', icon: AlertCircle, bg: 'rgba(100, 116, 139, 0.1)' };
    }
};

const getCategoryIcon = (cat: string) => {
    switch (cat) {
        case 'Loan': return Briefcase;
        case 'Card': return CreditCard;
        case 'Limit': return TrendingUp;
        default: return ClipboardList;
    }
};

const filteredApps = computed(() => {
    if (activeFilter.value === 'All') return applications.value;
    return applications.value.filter(a => a.category === activeFilter.value);
});

onMounted(fetchApplications);

import { computed } from 'vue';
</script>

<template>
  <div class="view-container fade-in">
    <header class="view-header-premium">
      <div class="header-main">
        <h1 class="view-title text-gradient">Başvurularım</h1>
        <p class="subtitle">Tüm finansal taleplerinizin güncel durumunu buradan takip edebilirsiniz.</p>
      </div>
      <div class="header-actions">
        <button @click="fetchApplications" class="btn-refresh" :class="{ spinning: loading }">
            <RefreshCw :size="18" />
        </button>
      </div>
    </header>

    <!-- FILTER BAR -->
    <div class="filter-bar-v2 mb-4">
        <button 
            v-for="f in [{id:'All', n:'Tümü'}, {id:'Loan', n:'Krediler'}, {id:'Card', n:'Kartlar'}, {id:'Limit', n:'Limit Artırım'}]" 
            :key="f.id"
            @click="activeFilter = f.id"
            :class="{ active: activeFilter === f.id }"
            class="filter-pill"
        >
            {{ f.n }}
        </button>
    </div>

    <div v-if="loading" class="loader-wrap">
        <div class="premium-spinner"></div>
        <p>Talepleriniz kontrol ediliyor...</p>
    </div>

    <div v-else class="applications-stack">
        <div v-for="app in filteredApps" :key="app.id" class="app-card-premium view-animate">
            <div class="app-icon-side">
                <div class="icon-circle" :style="{ backgroundColor: getStatusDetails(app.status).bg, color: getStatusDetails(app.status).color }">
                    <component :is="getCategoryIcon(app.category)" :size="22" />
                </div>
            </div>
            
            <div class="app-main-info">
                <div class="info-top">
                    <h3>{{ app.title }}</h3>
                    <div class="status-badge" :style="{ color: getStatusDetails(app.status).color, backgroundColor: getStatusDetails(app.status).bg }">
                        <component :is="getStatusDetails(app.status).icon" :size="14" />
                        <span>{{ getStatusDetails(app.status).label }}</span>
                    </div>
                </div>
                <div class="info-mid">
                    <span class="subtitle">{{ app.subtitle }}</span>
                    <span class="dot-sep">•</span>
                    <span class="date">{{ new Date(app.date).toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' }) }}</span>
                </div>
                <div v-if="app.amount > 0" class="info-amount">
                    <strong>{{ app.amount.toLocaleString('tr-TR') }} ₺</strong>
                </div>
            </div>

            <div class="app-action-side">
                <button class="btn-detail">
                    Detay <ChevronRight :size="16" />
                </button>
            </div>
        </div>

        <div v-if="filteredApps.length === 0" class="empty-state-v2">
            <ClipboardList :size="64" class="empty-icon" />
            <h3>Henüz bir başvurunuz bulunmuyor</h3>
            <p>Kredi, kart veya limit artırım talepleriniz burada listelenecektir.</p>
            <div class="empty-actions mt-4">
                <router-link to="/loans" class="btn-premium btn-gold-small">Kredi Başvurusu Yap</router-link>
            </div>
        </div>
    </div>

    <!-- INFO BOX -->
    <div class="glass-card mt-5 bg-premium-info">
        <div class="info-flex">
            <AlertCircle :size="20" class="text-primary" />
            <div class="info-text">
                <strong>Değerlendirme Süreci</strong>
                <p>Başvurularınız mesai saatleri içerisinde personelimiz tarafından incelenmektedir. Sonuçlandığında size SMS ve bildirim yoluyla haber verilecektir.</p>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.view-header-premium { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2.5rem; }
.btn-refresh { background: white; border: 1px solid #E2E8F0; padding: 10px; border-radius: 12px; cursor: pointer; transition: 0.3s; color: #64748B; }
.btn-refresh:hover { border-color: var(--primary); color: var(--primary); transform: rotate(180deg); }
.btn-refresh.spinning { animation: spin 1s linear infinite; }

.filter-bar-v2 { display: flex; gap: 12px; }
.filter-pill { 
    background: white; border: 1.5px solid #E2E8F0; padding: 8px 20px; 
    border-radius: 100px; font-size: 0.85rem; font-weight: 700; color: #64748B; 
    cursor: pointer; transition: 0.2s;
}
.filter-pill:hover { border-color: var(--primary); color: var(--primary); }
.filter-pill.active { background: var(--primary-dark); color: var(--gold); border-color: var(--primary-dark); }

.applications-stack { display: flex; flex-direction: column; gap: 16px; }

.app-card-premium {
    background: white; border: 1px solid rgba(0,0,0,0.05); padding: 24px 32px; 
    border-radius: 24px; display: flex; align-items: center; gap: 24px;
    transition: 0.3s; box-shadow: 0 4px 15px rgba(0,0,0,0.02);
}
.app-card-premium:hover { transform: translateY(-3px); box-shadow: 0 12px 30px rgba(0,0,0,0.06); border-color: var(--primary-light); }

.icon-circle { width: 56px; height: 56px; border-radius: 18px; display: flex; align-items: center; justify-content: center; }

.app-main-info { flex: 1; }
.info-top { display: flex; align-items: center; gap: 16px; margin-bottom: 4px; }
.info-top h3 { margin: 0; font-size: 1.1rem; color: #0F172A; font-weight: 800; }

.status-badge { display: flex; align-items: center; gap: 6px; padding: 4px 12px; border-radius: 100px; font-size: 0.7rem; font-weight: 800; }

.info-mid { display: flex; align-items: center; gap: 8px; color: #64748B; font-size: 0.8rem; font-weight: 600; margin-bottom: 8px; }
.dot-sep { opacity: 0.3; }

.info-amount { font-size: 1.25rem; color: #0F172A; font-family: 'Outfit'; font-weight: 800; }

.btn-detail { background: #F8FAFC; border: 1px solid #E2E8F0; padding: 8px 16px; border-radius: 10px; font-size: 0.75rem; font-weight: 800; color: #64748B; display: flex; align-items: center; gap: 4px; cursor: pointer; transition: 0.2s; }
.btn-detail:hover { background: var(--primary-dark); color: white; border-color: var(--primary-dark); }

.empty-state-v2 { text-align: center; padding: 80px 40px; background: white; border-radius: 24px; border: 2px dashed #E2E8F0; }
.empty-icon { color: #CBD5E1; margin-bottom: 20px; }
.empty-state-v2 h3 { color: #0F172A; font-weight: 800; }
.empty-state-v2 p { color: #64748B; max-width: 300px; margin: 10px auto; font-weight: 600; }

.bg-premium-info { background: #F8FAFC !important; border-left: 4px solid var(--primary); padding: 24px; border-radius: 20px; }
.info-flex { display: flex; gap: 16px; align-items: center; }
.info-text strong { display: block; font-size: 0.9rem; color: #0F172A; }
.info-text p { font-size: 0.8rem; color: #64748B; margin: 4px 0 0 0; }

@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
