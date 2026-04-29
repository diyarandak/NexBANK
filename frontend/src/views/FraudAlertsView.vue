<script setup lang="ts">
import { ref, onMounted, computed, onUnmounted } from 'vue';
import apiClient from '../api/client';
import { 
  ShieldAlert, ShieldCheck, Activity, 
  Terminal, AlertTriangle, Lock, 
  PhoneCall, Zap, Search, 
  BarChart3, RefreshCw, Eye,
  Navigation, Globe, Crosshair
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const transactions = ref<any[]>([]);
const loading = ref(true);
const systemStatus = ref('GÜVENLİ'); // GÜVENLİ, UYARI, TEHLİKE
const liveLogs = ref<string[]>([]);
const logInterval = ref<any>(null);

const fetchTransactions = async () => {
    try {
        const res = await apiClient.get('/transactions/admin/all');
        transactions.value = res.data;
    } catch (err) {
        console.error('İşlemler yüklenemedi');
    } finally {
        loading.value = false;
    }
};

const fraudAlerts = computed(() => {
    const alerts = transactions.value.filter(t => t.description && t.description.includes('ŞÜPHELİ İŞLEM'));
    if (alerts.length > 0) systemStatus.value = 'UYARI';
    return alerts;
});

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

const initiateAction = (action: string, id: number) => {
    toast.info(`İşlem #${id} için '${action}' süreci başlatıldı.`);
};

// CANLI LOG SİMÜLASYONU
const startLiveMonitor = () => {
    const messages = [
        "Analiz Ediliyor: TR...8822 EFT işlemi",
        "Gözlemci: 145.250 TL bakiye kontrolü",
        "Güvenlik Katmanı: IP 94.12.XXX.XX doğrulandı",
        "Risk Analizi: Yurtdışı harcama kontrolü",
        "Sistem: Veritabanı bütünlük kontrolü OK",
        "Uyarı: Beklenmeyen yüksek tutarlı işlem denemesi!",
        "Gözlemci: Yeni abone işlemi izleniyor"
    ];
    logInterval.value = setInterval(() => {
        const msg = `[${new Date().toLocaleTimeString()}] ${messages[Math.floor(Math.random() * messages.length)]}`;
        liveLogs.value.unshift(msg);
        if (liveLogs.value.length > 8) liveLogs.value.pop();
    }, 4000);
};

onMounted(() => {
    fetchTransactions();
    startLiveMonitor();
});

onUnmounted(() => {
    if (logInterval.value) clearInterval(logInterval.value);
});
</script>

<template>
  <div class="view-container fade-in page-wrapper">
    <div class="content-limiter">
        <!-- CYBER HEADER -->
        <header class="cyber-header mb-4">
        <div class="h-left">
            <div class="header-tag">SİSTEM VERSİYON v2.4</div>
            <h1 class="text-gradient-red">Siber Gözcü (Şüpheli İşlem Takibi)</h1>
            <p class="subtitle text-muted">Gözlemci (Observer) deseni ile 7/24 canlı risk analizi.</p>
        </div>
        <div class="h-right">
            <div class="system-status-box" :class="systemStatus.toLowerCase()">
                <div class="status-indicator">
                    <div class="dot"></div>
                    <span>SİSTEM DURUMU: {{ systemStatus }}</span>
                </div>
                <div class="threat-level-bar">
                    <div class="level-fill" :style="{ width: fraudAlerts.length > 0 ? '65%' : '10%' }"></div>
                </div>
            </div>
        </div>
        </header>

        <div class="cyber-grid">
            <!-- LEFT: MAIN ALERTS -->
            <div class="cyber-main">
                <div v-if="loading" class="loader-container-min"><div class="glow-loader"></div></div>
                
                <div v-else>
                    <!-- SECURE STATE -->
                    <div v-if="fraudAlerts.length === 0" class="glass-card-elite secure-state">
                        <div class="secure-icon-box">
                            <ShieldCheck :size="48" />
                            <div class="scanner-line"></div>
                        </div>
                        <h3>Sistem Güvenliği En Üst Seviyede</h3>
                        <p>Şu an için Gözlemci tarafından yakalanan şüpheli bir işlem bulunmamaktadır. Tüm finansal akışlar stabil ve güvenli.</p>
                        <div class="secure-stats mt-4">
                            <div class="s-stat"><span>ANALİZ EDİLEN</span><strong>1.452</strong></div>
                            <div class="s-stat"><span>REDDEDİLEN</span><strong>0</strong></div>
                            <div class="s-stat"><span>GÜVEN PUANI</span><strong>%99.9</strong></div>
                        </div>
                    </div>

                    <!-- ALERT LIST -->
                    <div v-else class="fraud-alerts-list">
                        <div v-for="alert in fraudAlerts" :key="alert.id" class="glass-card-elite fraud-alert-card scale-up">
                            <div class="fac-header">
                                <div class="fac-id">
                                    <AlertTriangle :size="18" />
                                    <span>DOSYA-#{{ alert.id }}</span>
                                </div>
                                <div class="fac-risk-label">KRİTİK RİSK</div>
                            </div>
                            
                            <div class="fac-body">
                                <div class="fac-info">
                                    <h3>{{ alert.description }}</h3>
                                    <div class="fac-meta">
                                        <span><Navigation :size="14" /> IP: 94.12.112.XX</span>
                                        <span><Calendar :size="14" /> {{ new Date(alert.createdAt).toLocaleString() }}</span>
                                    </div>
                                </div>
                                <div class="fac-amount">
                                    <small>İŞLEM TUTARI</small>
                                    <strong>{{ formatCurrency(alert.amount) }}</strong>
                                </div>
                            </div>

                            <div class="fac-footer">
                                <button @click="initiateAction('Dondur', alert.id)" class="btn-cyber btn-freeze">İŞLEMİ DONDUR</button>
                                <button @click="initiateAction('Arama', alert.id)" class="btn-cyber btn-call">MÜŞTERİYİ ARA</button>
                                <button @click="initiateAction('İncele', alert.id)" class="btn-cyber btn-analyze">DETAYLI ANALİZ</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- RIGHT: SIDEBAR -->
            <aside class="cyber-sidebar">
                <div class="glass-card-elite monitor-box">
                    <div class="mb-header">
                        <Terminal :size="14" />
                        <span>CANLI GÖZLEMCİ LOGLARI</span>
                    </div>
                    <div class="log-stream">
                        <div v-for="(log, i) in liveLogs" :key="i" class="log-item fade-in">
                            {{ log }}
                        </div>
                        <div v-if="liveLogs.length === 0" class="log-placeholder">Log akışı bekleniyor...</div>
                    </div>
                </div>

                <div class="glass-card-elite stats-box mt-3">
                    <div class="mb-header">
                        <Activity :size="14" />
                        <span>GÜVENLİK VERİLERİ</span>
                    </div>
                    <div class="stats-list mt-2">
                        <div class="stat-row">
                            <span>Aktif Gözlemciler</span>
                            <strong>4</strong>
                        </div>
                        <div class="stat-row">
                            <span>Fraud Önleme Oranı</span>
                            <strong class="text-success">%100</strong>
                        </div>
                    </div>
                </div>

                <div class="cyber-action-box mt-3">
                    <button class="btn-full-scan">
                        <Crosshair :size="16" /> TAM TARAMA BAŞLAT
                    </button>
                </div>
            </aside>
        </div>
    </div>
  </div>
</template>

<style scoped>
.page-wrapper { display: flex; justify-content: center; padding: 20px 40px; }
.content-limiter { width: 100%; max-width: 1200px; }

.cyber-header { display: flex; justify-content: space-between; align-items: flex-end; border-bottom: 1px solid #F1F5F9; padding-bottom: 24px; }
.header-tag { font-size: 0.55rem; font-weight: 900; background: #0F172A; color: #D4AF37; padding: 2px 8px; border-radius: 4px; width: fit-content; margin-bottom: 6px; }
.text-gradient-red { 
    background: linear-gradient(135deg, #EF4444 0%, #B91C1C 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 1.8rem; font-weight: 900;
}

.system-status-box { width: 220px; padding: 10px; border-radius: 12px; background: white; border: 1px solid #E2E8F0; }
.system-status-box.güvenli { border-color: #10B981; }
.system-status-box.uyarı { border-color: #F59E0B; background: #FFFBEB; }

.status-indicator { display: flex; align-items: center; gap: 8px; font-size: 0.6rem; font-weight: 800; color: #64748B; margin-bottom: 6px; }
.status-indicator .dot { width: 6px; height: 6px; background: #10B981; border-radius: 50%; }
.uyarı .status-indicator .dot { background: #F59E0B; animation: pulse 1s infinite; }

.threat-level-bar { height: 4px; background: #E2E8F0; border-radius: 10px; overflow: hidden; }
.level-fill { height: 100%; background: #10B981; transition: 1s; }
.uyarı .level-fill { background: #F59E0B; }

/* GRID */
.cyber-grid { display: grid; grid-template-columns: 1fr 300px; gap: 24px; margin-top: 32px; }

/* SECURE STATE */
.secure-state { padding: 60px 40px; text-align: center; background: white; border: 1px solid #F1F5F9; }
.secure-icon-box { width: 90px; height: 90px; background: #F0FDF4; color: #10B981; border-radius: 24px; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; position: relative; overflow: hidden; }
.scanner-line { position: absolute; top: 0; left: 0; right: 0; height: 2px; background: rgba(16, 185, 129, 0.5); box-shadow: 0 0 10px #10B981; animation: scan 3s infinite linear; }

.secure-state h3 { font-size: 1.3rem; font-weight: 900; color: #0F172A; margin-bottom: 12px; }
.secure-state p { color: #64748B; font-size: 0.9rem; max-width: 500px; margin: 0 auto; line-height: 1.6; }

.secure-stats { display: flex; justify-content: center; gap: 40px; border-top: 1px solid #F1F5F9; padding-top: 24px; }
.s-stat span { display: block; font-size: 0.6rem; font-weight: 800; color: #94A3B8; margin-bottom: 4px; }
.s-stat strong { font-size: 1rem; font-weight: 900; color: #0F172A; }

/* ALERT CARDS */
.fraud-alerts-list { display: flex; flex-direction: column; gap: 16px; }
.fraud-alert-card { padding: 20px; border: 1px solid #FEE2E2; background: white; border-left: 5px solid #EF4444; }
.fac-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
.fac-id { display: flex; align-items: center; gap: 8px; color: #EF4444; font-weight: 900; font-size: 0.75rem; }
.fac-risk-label { background: #EF4444; color: white; padding: 3px 8px; border-radius: 4px; font-size: 0.6rem; font-weight: 900; }

.fac-body { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 20px; }
.fac-info h3 { font-size: 1.1rem; font-weight: 900; color: #0F172A; margin-bottom: 10px; }
.fac-meta { display: flex; gap: 16px; }
.fac-meta span { display: flex; align-items: center; gap: 6px; font-size: 0.7rem; color: #64748B; font-weight: 700; }

.fac-amount { text-align: right; }
.fac-amount small { display: block; font-size: 0.6rem; font-weight: 800; color: #94A3B8; margin-bottom: 2px; }
.fac-amount strong { font-size: 1.5rem; font-weight: 900; color: #EF4444; font-family: 'Outfit'; }

.fac-footer { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; border-top: 1px solid #F1F5F9; padding-top: 16px; }
.btn-cyber { padding: 10px; border: none; border-radius: 8px; font-size: 0.65rem; font-weight: 900; cursor: pointer; transition: 0.2s; }
.btn-freeze { background: #0F172A; color: white; }
.btn-call { background: #FEF2F2; color: #EF4444; }
.btn-analyze { background: #F1F5F9; color: #64748B; }

/* SIDEBAR */
.monitor-box { background: #0F172A; color: #10B981; padding: 16px; height: 280px; display: flex; flex-direction: column; }
.mb-header { display: flex; align-items: center; gap: 8px; font-size: 0.65rem; font-weight: 900; color: rgba(255,255,255,0.4); border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 10px; }
.log-stream { flex: 1; overflow-y: auto; font-family: 'Courier New', Courier, monospace; font-size: 0.65rem; margin-top: 10px; display: flex; flex-direction: column; gap: 6px; }
.log-item { color: #10B981; border-left: 2px solid rgba(16, 185, 129, 0.2); padding-left: 8px; }

.stats-box { padding: 16px; background: white; border: 1px solid #F1F5F9; }
.stat-row { display: flex; justify-content: space-between; font-size: 0.75rem; margin-bottom: 8px; }
.stat-row span { color: #64748B; font-weight: 600; }
.stat-row strong { color: #0F172A; font-weight: 900; }

.btn-full-scan { 
    width: 100%; padding: 12px; border: none; border-radius: 12px; 
    background: #0F172A; color: #D4AF37; font-weight: 900; font-size: 0.7rem;
    cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 8px;
    transition: 0.3s;
}

@keyframes scan { from { top: 0; } to { top: 100%; } }
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }

.loader-container-min { height: 300px; display: flex; align-items: center; justify-content: center; }
</style>
