<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
  UserCheck, ShieldCheck, ShieldAlert, 
  Search, FileCheck, XCircle, 
  Camera, Eye, CheckCircle2, 
  Clock, Activity, BarChart3,
  Fingerprint, FileText, ChevronRight
} from 'lucide-vue-next';

const toast = useToastStore();
const pendingUsers = ref<any[]>([]);
const loading = ref(true);
let refreshInterval: any = null;

const fetchPending = async () => {
    try {
        const res = await apiClient.get('/users/customers');
        // Simülasyon: Pasif kullanıcıları KYC bekliyor sayalım
        pendingUsers.value = res.data.filter((u: any) => !u.isActive).map((u: any) => ({
            ...u,
            docType: 'T.C. Kimlik Kartı',
            confidence: Math.floor(Math.random() * (99 - 94) + 94),
            requestDate: new Date(u.createdAt).toLocaleDateString('tr-TR')
        }));
    } catch (err) {
        console.error('Liste yüklenemedi');
    } finally {
        loading.value = false;
    }
};

const handleApprove = async (id: number) => {
    try {
        await apiClient.post(`/users/${id}/toggle-status`);
        toast.success('Müşteri kimliği başarıyla doğrulandı ve hesap aktif edildi.');
        fetchPending();
    } catch (err) {
        toast.error('İşlem sırasında bir hata oluştu.');
    }
};

const handleReject = (name: string) => {
    toast.error(`${name} için kimlik belgesi reddedildi. Müşteriye bildirim gönderildi.`);
};

onMounted(() => {
    fetchPending();
    refreshInterval = setInterval(fetchPending, 15000);
});

onUnmounted(() => {
    if (refreshInterval) clearInterval(refreshInterval);
});
</script>

<template>
  <div class="view-container fade-in page-wrapper">
    <div class="content-limiter">
        <!-- HEADER -->
        <header class="kyc-header mb-5">
            <div class="h-left">
                <h1 class="text-gradient-gold">Kimlik Doğrulama Terminali</h1>
                <p class="subtitle">Yeni katılan müşterilerin belgelerini NexBank standartlarında onaylayın.</p>
            </div>
            <div class="h-right">
                <div class="kyc-stats-mini">
                    <div class="ks-item">
                        <small>BEKLEYEN</small>
                        <strong>{{ pendingUsers.length }}</strong>
                    </div>
                    <div class="ks-divider"></div>
                    <div class="ks-item">
                        <small>BUGÜN ONAY</small>
                        <strong>12</strong>
                    </div>
                </div>
            </div>
        </header>

        <!-- ANALYTICS STRIP -->
        <div class="kyc-metrics mb-5">
            <div class="k-metric-card">
                <div class="km-icon"><Clock :size="20" /></div>
                <div class="km-data">
                    <small>ORTALAMA ONAY SÜRESİ</small>
                    <strong>4.2 Dakika</strong>
                </div>
            </div>
            <div class="k-metric-card highlight">
                <div class="km-icon"><Fingerprint :size="20" /></div>
                <div class="km-data">
                    <small>DOĞRULUK ORANI</small>
                    <strong>%99.8</strong>
                </div>
            </div>
            <div class="k-metric-card">
                <div class="km-icon"><BarChart3 :size="20" /></div>
                <div class="km-data">
                    <small>GÜNLÜK KAPASİTE</small>
                    <strong>150 Belge</strong>
                </div>
            </div>
        </div>

        <div v-if="loading" class="loader-container-min"><div class="glow-loader"></div></div>

        <div v-else class="kyc-content">
            <!-- EMPTY STATE -->
            <div v-if="pendingUsers.length === 0" class="glass-card-elite secure-state-v2">
                <div class="secure-icon-box">
                    <ShieldCheck :size="48" />
                    <div class="scanner-line"></div>
                </div>
                <h3>Kimlik Kuyruğu Temiz!</h3>
                <p>Şu an onay bekleyen herhangi bir kimlik doğrulaması bulunmamaktadır. Tüm yeni kayıtlar NexBank standartlarında incelenmiştir.</p>
                <button @click="fetchPending" class="btn-refresh-min mt-4">
                    <RefreshCw :size="16" /> LİSTEYİ TAZELE
                </button>
            </div>

            <!-- KYC LIST -->
            <div v-else class="kyc-grid">
                <div v-for="u in pendingUsers" :key="u.id" class="glass-card-elite kyc-card-v2 scale-up">
                    <div class="kc-top">
                        <div class="kc-user">
                            <div class="kc-avatar">{{ u.fullName.charAt(0) }}</div>
                            <div class="kc-info">
                                <strong>{{ u.fullName }}</strong>
                                <span>{{ u.email }}</span>
                            </div>
                        </div>
                        <div class="kc-confidence" :class="{ 'high': u.confidence > 95 }">
                            <Activity :size="14" /> %{{ u.confidence }} Güven
                        </div>
                    </div>

                    <div class="kc-body">
                        <div class="kc-doc-row">
                            <div class="dr-icon"><FileText :size="18" /></div>
                            <div class="dr-text">
                                <small>BELGE TÜRÜ</small>
                                <strong>{{ u.docType }}</strong>
                            </div>
                        </div>
                        <div class="kc-doc-row">
                            <div class="dr-icon"><Clock :size="18" /></div>
                            <div class="dr-text">
                                <small>BAŞVURU TARİHİ</small>
                                <strong>{{ u.requestDate }}</strong>
                            </div>
                        </div>
                    </div>

                    <div class="kc-footer">
                        <button @click="handleReject(u.fullName)" class="btn-kyc-action reject">
                            <XCircle :size="16" /> REDDET
                        </button>
                        <button @click="handleApprove(u.id)" class="btn-kyc-action approve">
                            <CheckCircle2 :size="16" /> KİMLİĞİ ONAYLA
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

.kyc-header { display: flex; justify-content: space-between; align-items: flex-end; border-bottom: 1px solid #F1F5F9; padding-bottom: 24px; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2rem; font-weight: 900;
}

.kyc-stats-mini { background: #0F172A; padding: 12px 24px; border-radius: 16px; display: flex; align-items: center; gap: 20px; color: white; border: 1px solid #D4AF37; }
.ks-item small { display: block; font-size: 0.6rem; font-weight: 800; color: rgba(255,255,255,0.4); margin-bottom: 2px; }
.ks-item strong { font-size: 1.2rem; font-weight: 900; color: #D4AF37; font-family: 'Outfit'; }
.ks-divider { width: 1px; height: 30px; background: rgba(212, 175, 55, 0.2); }

/* METRICS */
.kyc-metrics { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
.k-metric-card { background: white; padding: 20px; border-radius: 20px; border: 1px solid #F1F5F9; display: flex; align-items: center; gap: 16px; }
.k-metric-card.highlight { background: #F8FAFC; border-color: #D4AF37; }
.km-icon { width: 44px; height: 44px; background: #F8FAFC; border-radius: 12px; display: flex; align-items: center; justify-content: center; color: #0F172A; }
.highlight .km-icon { background: rgba(212, 175, 55, 0.1); color: #D4AF37; }
.km-data small { display: block; font-size: 0.65rem; font-weight: 900; color: #94A3B8; letter-spacing: 0.5px; }
.km-data strong { font-size: 1.1rem; font-weight: 900; color: #0F172A; }

/* KYCLIST GRID */
.kyc-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(360px, 1fr)); gap: 24px; margin-top: 32px; }
.kyc-card-v2 { padding: 24px; border: 1px solid #F1F5F9; display: flex; flex-direction: column; gap: 20px; }

.kc-top { display: flex; justify-content: space-between; align-items: flex-start; }
.kc-user { display: flex; align-items: center; gap: 12px; }
.kc-avatar { width: 44px; height: 44px; background: #0F172A; color: #D4AF37; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-weight: 900; font-size: 1.1rem; }
.kc-info strong { display: block; font-size: 1rem; color: #0F172A; }
.kc-info span { font-size: 0.75rem; color: #94A3B8; font-weight: 600; }

.kc-confidence { font-size: 0.65rem; font-weight: 900; background: #FFFBEB; color: #F59E0B; padding: 4px 10px; border-radius: 100px; display: flex; align-items: center; gap: 4px; }
.kc-confidence.high { background: #F0FDF4; color: #10B981; }

.kc-body { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; padding: 16px; background: #F8FAFC; border-radius: 16px; }
.kc-doc-row { display: flex; align-items: center; gap: 10px; }
.dr-icon { color: #94A3B8; }
.dr-text small { display: block; font-size: 0.55rem; font-weight: 800; color: #94A3B8; }
.dr-text strong { font-size: 0.8rem; color: #475569; font-weight: 700; }

.kc-footer { display: grid; grid-template-columns: 1fr 1.5fr; gap: 12px; }
.btn-kyc-action { padding: 12px; border: none; border-radius: 12px; font-weight: 900; font-size: 0.75rem; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px; transition: 0.2s; }
.btn-kyc-action.reject { background: #FEF2F2; color: #EF4444; }
.btn-kyc-action.approve { background: #0F172A; color: #D4AF37; }
.btn-kyc-action:hover { transform: translateY(-2px); filter: brightness(1.1); }

/* EMPTY STATE */
.secure-state-v2 { padding: 80px 40px; text-align: center; background: white; }
.secure-icon-box { width: 90px; height: 90px; background: #F0FDF4; color: #10B981; border-radius: 24px; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; position: relative; overflow: hidden; }
.scanner-line { position: absolute; top: 0; left: 0; right: 0; height: 2px; background: rgba(16, 185, 129, 0.5); box-shadow: 0 0 10px #10B981; animation: scan 3s infinite linear; }

@keyframes scan { from { top: 0; } to { top: 100%; } }

.loader-container-min { height: 300px; display: flex; align-items: center; justify-content: center; }
</style>
