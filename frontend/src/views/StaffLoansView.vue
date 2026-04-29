<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';
import { useToastStore } from '../store/toast';
import { 
  Landmark, Briefcase, TrendingUp, 
  BarChart3, Clock, AlertTriangle, 
  CheckCircle2, XCircle, User,
  DollarSign, PieChart, Timer,
  RefreshCw, ChevronRight, Scale
} from 'lucide-vue-next';

const authStore = useAuthStore();
const toast = useToastStore();
const loans = ref<any[]>([]);
const loading = ref(true);
let pollInterval: any = null;

const fetchLoans = async () => {
    try {
        const res = await apiClient.get('/loans/pending');
        loans.value = res.data;
    } catch (err) {
        console.error('Kredi listesi hatası');
    } finally {
        loading.value = false;
    }
};

const handleAction = async (id: number, action: 'approve' | 'reject') => {
    try {
        await apiClient.post(`/loans/${id}/${action}`);
        toast.success(`Kredi başvurusu başarıyla ${action === 'approve' ? 'onaylandı' : 'reddedildi'}.`);
        fetchLoans();
    } catch (err: any) {
        toast.error('İşlem başarısız: ' + (err.response?.data?.message || 'Bir hata oluştu.'));
    }
};

onMounted(() => {
    fetchLoans();
    pollInterval = setInterval(fetchLoans, 8000);
});

onUnmounted(() => {
    if (pollInterval) clearInterval(pollInterval);
});

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

// Toplam Bekleyen Hacim Hesaplama
const totalPendingVolume = computed(() => {
    return loans.value.reduce((acc, l) => acc + l.amount, 0);
});
</script>

<template>
  <div class="view-container fade-in page-wrapper">
    <div class="content-limiter">
        <!-- HEADER -->
        <header class="loan-header mb-5">
            <div class="h-left">
                <h1 class="text-gradient-gold">Kredi Karar Destek Merkezi</h1>
                <p class="subtitle">Banka risk politikalarına göre başvuruları anlık olarak değerlendirin.</p>
            </div>
            <div class="h-right">
                <div class="live-status-pill" :class="{ 'has-items': loans.length > 0 }">
                    <div class="pulse-dot"></div>
                    <span>{{ loans.length > 0 ? 'YENİ BAŞVURULAR VAR' : 'KUYRUK GÜNCEL' }}</span>
                </div>
            </div>
        </header>

        <!-- ANALYTICS STRIP -->
        <div class="loan-metrics mb-5">
            <div class="l-metric-card">
                <div class="lm-icon"><DollarSign :size="20" /></div>
                <div class="lm-data">
                    <small>BEKLEYEN HACİM</small>
                    <strong>{{ formatCurrency(totalPendingVolume) }}</strong>
                </div>
            </div>
            <div class="l-metric-card highlight">
                <div class="lm-icon"><Scale :size="20" /></div>
                <div class="lm-data">
                    <small>ORTALAMA ONAY ORANI</small>
                    <strong>%68.4</strong>
                </div>
            </div>
            <div class="l-metric-card">
                <div class="lm-icon"><PieChart :size="20" /></div>
                <div class="lm-data">
                    <small>RİSK DAĞILIMI</small>
                    <strong>Stabil</strong>
                </div>
            </div>
        </div>

        <div v-if="loading" class="loader-container-min"><div class="glow-loader"></div></div>

        <div v-else class="loan-content">
            <!-- EMPTY STATE -->
            <div v-if="loans.length === 0" class="glass-card-elite empty-state-v7">
                <div class="empty-visual">
                    <CheckCircle2 :size="64" />
                    <div class="wave-ring"></div>
                </div>
                <h3>Tüm Talepler Karara Bağlandı</h3>
                <p>Şu an onay bekleyen herhangi bir kredi başvurusu bulunmuyor. Müşteriler başvurduğunda bildirim alacaksınız.</p>
                <button @click="fetchLoans" class="btn-refresh-v7 mt-4">
                    <RefreshCw :size="16" /> GÜNCELLE
                </button>
            </div>

            <!-- LOAN TICKET LIST -->
            <div v-else class="loan-tickets-grid">
                <div v-for="l in loans" :key="l.id" class="glass-card-elite loan-ticket-card scale-up">
                    <div class="ltc-header">
                        <div class="ltc-user">
                            <div class="ltc-avatar">{{ l.user?.fullName.charAt(0) }}</div>
                            <div class="ltc-user-info">
                                <strong>{{ l.user?.fullName }}</strong>
                                <span>IBAN: {{ l.account?.iban }}</span>
                            </div>
                        </div>
                        <div class="ltc-type-badge">{{ l.loanType }}</div>
                    </div>

                    <div class="ltc-body">
                        <div class="ltc-stats">
                            <div class="ltc-stat-box">
                                <small>TALEP TUTARI</small>
                                <strong>{{ formatCurrency(l.amount) }}</strong>
                            </div>
                            <div class="ltc-stat-box">
                                <small>VADE / FAİZ</small>
                                <strong>{{ l.termMonths }} Ay / %{{ (l.interestRate * 100).toFixed(2) }}</strong>
                            </div>
                            <div class="ltc-stat-box">
                                <small>AYLIK TAKSİT</small>
                                <strong class="text-gold">{{ formatCurrency(l.monthlyPayment) }}</strong>
                            </div>
                            <div class="ltc-stat-box">
                                <small>RİSK SKORU</small>
                                <div class="risk-indicator" :class="l.amount > 50000 ? 'medium' : 'low'">
                                    {{ l.amount > 50000 ? 'ORTA' : 'DÜŞÜK' }}
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="ltc-footer">
                        <button @click="handleAction(l.id, 'reject')" class="btn-ltc reject">
                            <XCircle :size="16" /> TALEBİ REDDET
                        </button>
                        <button @click="handleAction(l.id, 'approve')" class="btn-ltc approve">
                            <CheckCircle2 :size="16" /> KREDİYİ ONAYLA
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.page-wrapper { display: flex; justify-content: center; padding: 20px 40px; }
.content-limiter { width: 100%; max-width: 1200px; }

.loan-header { display: flex; justify-content: space-between; align-items: flex-end; border-bottom: 1px solid #F1F5F9; padding-bottom: 24px; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2rem; font-weight: 900;
}

.live-status-pill { background: #F8FAFC; padding: 8px 16px; border-radius: 100px; display: flex; align-items: center; gap: 10px; font-size: 0.65rem; font-weight: 900; color: #64748B; border: 1px solid #E2E8F0; }
.live-status-pill.has-items { background: #FEF3C7; border-color: #D4AF37; color: #92400E; }
.pulse-dot { width: 8px; height: 8px; background: #CBD5E1; border-radius: 50%; }
.has-items .pulse-dot { background: #F59E0B; animation: pulse 1s infinite; }

/* METRICS */
.loan-metrics { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
.l-metric-card { background: white; padding: 20px; border-radius: 20px; border: 1px solid #F1F5F9; display: flex; align-items: center; gap: 16px; }
.l-metric-card.highlight { border-color: #D4AF37; }
.lm-icon { width: 44px; height: 44px; background: #F8FAFC; border-radius: 12px; display: flex; align-items: center; justify-content: center; color: #0F172A; }
.highlight .lm-icon { background: rgba(212, 175, 55, 0.1); color: #D4AF37; }
.lm-data small { display: block; font-size: 0.65rem; font-weight: 900; color: #94A3B8; letter-spacing: 0.5px; }
.lm-data strong { font-size: 1.1rem; font-weight: 900; color: #0F172A; }

/* LOAN TICKETS */
.loan-tickets-grid { display: grid; grid-template-columns: 1fr; gap: 20px; margin-top: 32px; }
.loan-ticket-card { padding: 30px; border: 1px solid #F1F5F9; border-left: 6px solid #D4AF37; display: flex; flex-direction: column; gap: 24px; }

.ltc-header { display: flex; justify-content: space-between; align-items: flex-start; }
.ltc-user { display: flex; align-items: center; gap: 16px; }
.ltc-avatar { width: 50px; height: 50px; background: #0F172A; color: #D4AF37; border-radius: 14px; display: flex; align-items: center; justify-content: center; font-weight: 900; font-size: 1.2rem; }
.ltc-user-info strong { display: block; font-size: 1.1rem; color: #0F172A; }
.ltc-user-info span { font-size: 0.8rem; color: #94A3B8; font-weight: 600; }
.ltc-type-badge { background: #0F172A; color: #D4AF37; padding: 4px 12px; border-radius: 100px; font-size: 0.65rem; font-weight: 900; text-transform: uppercase; }

.ltc-body { padding: 20px; background: #F8FAFC; border-radius: 20px; }
.ltc-stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 24px; }
.ltc-stat-box small { display: block; font-size: 0.6rem; font-weight: 900; color: #94A3B8; margin-bottom: 6px; letter-spacing: 0.5px; }
.ltc-stat-box strong { font-size: 1rem; color: #0F172A; font-family: 'Outfit'; }
.text-gold { color: #D4AF37 !important; }

.risk-indicator { font-size: 0.65rem; font-weight: 900; padding: 2px 10px; border-radius: 6px; width: fit-content; }
.risk-indicator.low { background: #DCFCE7; color: #166534; }
.risk-indicator.medium { background: #FEF3C7; color: #92400E; }

.ltc-footer { display: flex; justify-content: flex-end; gap: 16px; }
.btn-ltc { padding: 12px 24px; border: none; border-radius: 12px; font-weight: 900; font-size: 0.8rem; cursor: pointer; display: flex; align-items: center; gap: 10px; transition: 0.2s; }
.btn-ltc.reject { background: #FEF2F2; color: #EF4444; }
.btn-ltc.approve { background: #0F172A; color: #D4AF37; box-shadow: 0 10px 20px rgba(15, 23, 42, 0.1); }
.btn-ltc:hover { transform: translateY(-2px); filter: brightness(1.1); }

/* EMPTY STATE */
.empty-state-v7 { padding: 80px 40px; text-align: center; background: white; }
.empty-visual { width: 100px; height: 100px; background: #F0FDF4; color: #10B981; border-radius: 30px; display: flex; align-items: center; justify-content: center; margin: 0 auto 30px; position: relative; }
.wave-ring { position: absolute; inset: -10px; border: 2px solid #10B981; border-radius: 40px; opacity: 0; animation: wave 2s infinite; }

@keyframes wave { 0% { transform: scale(1); opacity: 0.5; } 100% { transform: scale(1.3); opacity: 0; } }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.4; } 100% { opacity: 1; } }

.loader-container-min { height: 300px; display: flex; align-items: center; justify-content: center; }
</style>
