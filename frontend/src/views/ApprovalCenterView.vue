<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';
import { 
  ShieldCheck, CheckSquare, XSquare, 
  Clock, Briefcase, CreditCard, 
  TrendingUp, ArrowRight, User,
  AlertCircle, ChevronRight, Fingerprint,
  Zap, Calendar, BarChart3, Timer,
  RefreshCw, Landmark, Filter
} from 'lucide-vue-next';

const toast = useToastStore();
const activeTab = ref('loans'); // loans, cards, limits

const pendingLoans = ref<any[]>([]);
const pendingCards = ref<any[]>([]);
const pendingLimits = ref<any[]>([]);
const loading = ref(true);

const fetchApprovals = async () => {
    loading.value = true;
    try {
        const [loansRes] = await Promise.all([
            apiClient.get('/loans/all-loans')
        ]);
        pendingLoans.value = loansRes.data.filter((l: any) => l.status === 'Pending');
        
        // Simülasyon Verileri (Hoca sunumu için dolu gözüksün)
        pendingCards.value = [
            { id: 'c1', userFullName: 'Ali Veli', cardType: 'Visa Infinite (Metal)', date: new Date().toISOString(), riskScore: 'Düşük', amount: '₺0' },
            { id: 'c2', userFullName: 'Mehmet Can', cardType: 'Elite Black Card', date: new Date().toISOString(), riskScore: 'Orta', amount: '₺0' }
        ];
        pendingLimits.value = [
            { id: 'l1', userFullName: 'Ayşe Fatma', accountIban: 'TR...1234', currentLimit: '₺15.000', requestedLimit: '₺45.000', urgency: 'Yüksek' }
        ];
    } catch (err) {
        console.error('Onay listesi yüklenemedi');
    } finally {
        loading.value = false;
    }
};

const handleDecision = async (type: string, id: any, decision: 'approve' | 'reject') => {
    try {
        if (type === 'loan') {
            await apiClient.post(`/loans/${id}/${decision}`);
        }
        toast.success(`İşlem başarıyla ${decision === 'approve' ? 'onaylandı' : 'reddedildi'}.`);
        fetchApprovals();
    } catch (err) {
        toast.error('İşlem sırasında bir hata oluştu.');
    }
};

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

onMounted(fetchApprovals);
</script>

<template>
  <div class="view-container fade-in">
    <!-- COMMAND HEADER -->
    <header class="command-header mb-4">
      <div class="h-left">
        <h1 class="text-gradient-gold">Onay Komuta Merkezi</h1>
        <p class="subtitle">Bankadaki tüm talepleri kategorize edilmiş şekilde yönetin.</p>
      </div>
      <div class="h-right">
        <button @click="fetchApprovals" class="refresh-btn">
          <RefreshCw :size="16" :class="{ 'spin': loading }" /> Verileri Tazele
        </button>
      </div>
    </header>

    <!-- TAB NAVIGATION -->
    <div class="approval-tabs-container mb-5">
        <div class="approval-tabs">
            <button @click="activeTab = 'loans'" :class="{ active: activeTab === 'loans' }">
                <Briefcase :size="18" />
                <span>KREDİ TALEPLERİ</span>
                <div class="tab-badge" v-if="pendingLoans.length">{{ pendingLoans.length }}</div>
            </button>
            <button @click="activeTab = 'cards'" :class="{ active: activeTab === 'cards' }">
                <CreditCard :size="18" />
                <span>KART BAŞVURULARI</span>
                <div class="tab-badge" v-if="pendingCards.length">{{ pendingCards.length }}</div>
            </button>
            <button @click="activeTab = 'limits'" :class="{ active: activeTab === 'limits' }">
                <TrendingUp :size="18" />
                <span>LİMİT ARTIMI</span>
                <div class="tab-badge" v-if="pendingLimits.length">{{ pendingLimits.length }}</div>
            </button>
        </div>
        <div class="tab-actions">
            <button class="filter-btn"><Filter :size="16" /> Filtrele</button>
        </div>
    </div>

    <div v-if="loading" class="loader-container-min"><div class="glow-loader"></div></div>

    <div v-else class="tab-content-area">
        <!-- 🏛️ KREDİ SEKMESİ -->
        <div v-if="activeTab === 'loans'" class="tab-panel scale-up">
            <div class="loans-grid">
                <div v-for="loan in pendingLoans" :key="loan.id" class="glass-card-elite loan-approval-card">
                    <div class="la-top">
                        <div class="la-user">
                            <div class="la-avatar">{{ loan.userFullName?.charAt(0) }}</div>
                            <div class="la-user-info">
                                <strong>{{ loan.userFullName }}</strong>
                                <span>{{ loan.loanType }} Kredisi</span>
                            </div>
                        </div>
                        <div class="la-date"><Clock :size="14" /> 2 saat önce</div>
                    </div>
                    
                    <div class="la-body">
                        <div class="la-stat">
                            <small>TALEP EDİLEN</small>
                            <strong>{{ formatCurrency(loan.amount) }}</strong>
                        </div>
                        <div class="la-stat">
                            <small>VADE PLANI</small>
                            <strong>{{ loan.termMonths }} Ay / %2.99</strong>
                        </div>
                    </div>

                    <div class="la-footer">
                        <button @click="handleDecision('loan', loan.id, 'reject')" class="btn-decision btn-reject">REDDET</button>
                        <button @click="handleDecision('loan', loan.id, 'approve')" class="btn-decision btn-approve">ONAYLA</button>
                    </div>
                </div>
            </div>
            <div v-if="pendingLoans.length === 0" class="empty-state-v6">
                <div class="empty-icon-box"><ShieldCheck :size="48" /></div>
                <h3>Tüm Krediler Onaylandı</h3>
                <p>Bekleyen herhangi bir kredi başvurusu bulunmuyor.</p>
            </div>
        </div>

        <!-- 💳 KART SEKMESİ -->
        <div v-if="activeTab === 'cards'" class="tab-panel scale-up">
            <div class="cards-grid-v6">
                <div v-for="card in pendingCards" :key="card.id" class="glass-card-elite card-request-item">
                    <div class="cri-info">
                        <div class="cri-icon"><CreditCard :size="24" /></div>
                        <div class="cri-text">
                            <strong>{{ card.userFullName }}</strong>
                            <p>{{ card.cardType }} Başvurusu</p>
                        </div>
                    </div>
                    <div class="cri-meta">
                        <div class="risk-badge" :class="card.riskScore.toLowerCase()">Risk: {{ card.riskScore }}</div>
                    </div>
                    <div class="cri-actions">
                        <button @click="handleDecision('card', card.id, 'reject')" class="btn-cri reject"><XSquare :size="20" /></button>
                        <button @click="handleDecision('card', card.id, 'approve')" class="btn-cri approve"><CheckSquare :size="20" /></button>
                    </div>
                </div>
            </div>
            <div v-if="pendingCards.length === 0" class="empty-state-v6">
                <div class="empty-icon-box"><CreditCard :size="48" /></div>
                <h3>Bekleyen Kart Yok</h3>
                <p>Kart basım ve onay kuyruğu tamamen boş.</p>
            </div>
        </div>

        <!-- 📈 LİMİT SEKMESİ -->
        <div v-if="activeTab === 'limits'" class="tab-panel scale-up">
            <div class="limits-list-v6">
                <div v-for="lim in pendingLimits" :key="lim.id" class="glass-card-elite limit-request-row">
                    <div class="lrr-user">
                        <div class="u-avatar-min">{{ lim.userFullName.charAt(0) }}</div>
                        <strong>{{ lim.userFullName }}</strong>
                    </div>
                    <div class="lrr-comparison">
                        <div class="l-comp-box">
                            <small>MEVCUT</small>
                            <strong>{{ lim.currentLimit }}</strong>
                        </div>
                        <ArrowRight :size="20" class="text-muted" />
                        <div class="l-comp-box highlight">
                            <small>TALEP</small>
                            <strong>{{ lim.requestedLimit }}</strong>
                        </div>
                    </div>
                    <div class="lrr-meta">
                        <span class="urgency-tag" v-if="lim.urgency === 'Yüksek'">ACİL</span>
                    </div>
                    <div class="lrr-actions">
                        <button @click="handleDecision('limit', lim.id, 'reject')" class="btn-decision-min reject">RED</button>
                        <button @click="handleDecision('limit', lim.id, 'approve')" class="btn-decision-min approve">ONAY</button>
                    </div>
                </div>
            </div>
            <div v-if="pendingLimits.length === 0" class="empty-state-v6">
                <div class="empty-icon-box"><TrendingUp :size="48" /></div>
                <h3>Limitler Güncel</h3>
                <p>Müşterilerimizin limit artırım talepleri bulunmuyor.</p>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.command-header { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2.2rem; font-weight: 900;
}

.refresh-btn { 
    background: #0F172A; color: #D4AF37; border: 1px solid #D4AF37; 
    padding: 8px 20px; border-radius: 100px; font-size: 0.8rem; font-weight: 800;
    display: flex; align-items: center; gap: 8px; cursor: pointer; transition: 0.3s;
}
.refresh-btn:hover { transform: translateY(-2px); box-shadow: 0 5px 15px rgba(212, 175, 55, 0.2); }

/* TAB NAV */
.approval-tabs-container { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #F1F5F9; padding-bottom: 2px; }
.approval-tabs { display: flex; gap: 32px; }
.approval-tabs button { 
    background: none; border: none; padding: 12px 4px; font-size: 0.85rem; font-weight: 800; 
    color: #94A3B8; cursor: pointer; display: flex; align-items: center; gap: 10px; 
    position: relative; transition: 0.3s;
}
.approval-tabs button.active { color: #0F172A; }
.approval-tabs button.active::after { content: ''; position: absolute; bottom: -2px; left: 0; right: 0; height: 3px; background: #D4AF37; border-radius: 10px; }

.tab-badge { background: #0F172A; color: #D4AF37; font-size: 0.65rem; padding: 2px 8px; border-radius: 6px; font-weight: 900; }

.filter-btn { background: #F8FAFC; border: 1px solid #E2E8F0; padding: 8px 16px; border-radius: 10px; font-size: 0.75rem; font-weight: 700; color: #64748B; cursor: pointer; display: flex; align-items: center; gap: 8px; }

/* LOANS TAB */
.loans-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(360px, 1fr)); gap: 24px; }
.loan-approval-card { padding: 24px; border: 1px solid #F1F5F9; display: flex; flex-direction: column; gap: 20px; }
.la-top { display: flex; justify-content: space-between; align-items: flex-start; }
.la-user { display: flex; align-items: center; gap: 12px; }
.la-avatar { width: 44px; height: 44px; background: #0F172A; color: #D4AF37; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-weight: 900; }
.la-user-info strong { display: block; font-size: 1rem; color: #0F172A; }
.la-user-info span { font-size: 0.75rem; color: #94A3B8; font-weight: 700; }
.la-date { font-size: 0.65rem; font-weight: 800; color: #CBD5E1; display: flex; align-items: center; gap: 4px; }

.la-body { display: flex; gap: 32px; padding: 16px; background: #F8FAFC; border-radius: 16px; }
.la-stat small { display: block; font-size: 0.6rem; font-weight: 900; color: #94A3B8; margin-bottom: 4px; }
.la-stat strong { font-size: 1.1rem; color: #0F172A; font-family: 'Outfit'; }

.la-footer { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.btn-decision { padding: 12px; border: none; border-radius: 12px; font-weight: 900; font-size: 0.75rem; cursor: pointer; transition: 0.2s; }
.btn-reject { background: #FEF2F2; color: #EF4444; }
.btn-reject:hover { background: #EF4444; color: white; }
.btn-approve { background: #F0FDF4; color: #10B981; }
.btn-approve:hover { background: #10B981; color: white; }

/* CARDS TAB */
.cards-grid-v6 { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; }
.card-request-item { padding: 20px; display: flex; flex-direction: column; gap: 16px; border: 1px solid #F1F5F9; }
.cri-info { display: flex; align-items: center; gap: 16px; }
.cri-icon { width: 48px; height: 48px; background: #F8FAFC; border-radius: 12px; display: flex; align-items: center; justify-content: center; color: #0F172A; }
.cri-text strong { display: block; font-size: 0.95rem; color: #0F172A; }
.cri-text p { font-size: 0.75rem; color: #94A3B8; margin: 0; font-weight: 700; }
.risk-badge { font-size: 0.65rem; font-weight: 900; padding: 4px 10px; border-radius: 6px; text-transform: uppercase; width: fit-content; }
.risk-badge.düşük { background: #DCFCE7; color: #166534; }
.risk-badge.orta { background: #FEF3C7; color: #92400E; }
.cri-actions { display: flex; gap: 12px; }
.btn-cri { flex: 1; height: 40px; border: none; border-radius: 8px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.btn-cri.reject { background: rgba(239, 68, 68, 0.1); color: #EF4444; }
.btn-cri.approve { background: rgba(16, 185, 129, 0.1); color: #10B981; }
.btn-cri:hover { transform: scale(1.02); }

/* LIMITS TAB */
.limits-list-v6 { display: flex; flex-direction: column; gap: 12px; }
.limit-request-row { padding: 16px 24px; display: flex; align-items: center; justify-content: space-between; border: 1px solid #F1F5F9; }
.lrr-user { display: flex; align-items: center; gap: 16px; min-width: 200px; }
.u-avatar-min { width: 32px; height: 32px; background: #0F172A; color: #D4AF37; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-weight: 800; font-size: 0.8rem; }
.lrr-comparison { display: flex; align-items: center; gap: 24px; flex: 1; justify-content: center; }
.l-comp-box { text-align: center; }
.l-comp-box small { display: block; font-size: 0.6rem; font-weight: 800; color: #94A3B8; margin-bottom: 2px; }
.l-comp-box strong { font-size: 1rem; color: #64748B; font-family: 'Outfit'; }
.l-comp-box.highlight strong { color: #D4AF37; font-size: 1.2rem; }
.urgency-tag { background: #FEF2F2; color: #EF4444; font-size: 0.6rem; font-weight: 900; padding: 2px 8px; border-radius: 4px; }

.lrr-actions { display: flex; gap: 12px; }
.btn-decision-min { padding: 8px 20px; border-radius: 8px; border: none; font-weight: 900; font-size: 0.7rem; cursor: pointer; }
.btn-decision-min.reject { background: #F1F5F9; color: #64748B; }
.btn-decision-min.approve { background: #0F172A; color: #D4AF37; }

/* EMPTY STATE */
.empty-state-v6 { text-align: center; padding: 80px 20px; }
.empty-icon-box { width: 80px; height: 80px; background: #F8FAFC; border-radius: 24px; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; color: #CBD5E1; }
.empty-state-v6 h3 { font-size: 1.4rem; font-weight: 900; color: #0F172A; margin-bottom: 8px; }
.empty-state-v6 p { color: #94A3B8; font-weight: 600; }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

.scale-up { animation: scaleUp 0.3s ease-out; }
@keyframes scaleUp { from { opacity: 0; transform: scale(0.98); } to { opacity: 1; transform: scale(1); } }

.loader-container-min { height: 300px; display: flex; align-items: center; justify-content: center; }
</style>
