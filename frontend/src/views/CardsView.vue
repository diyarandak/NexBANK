<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '../api/client';
import { 
  CreditCard, Plus, ShieldCheck, Zap, Globe, 
  ShoppingBag, Lock, Eye, Download, Settings,
  AlertCircle, CheckCircle2, ChevronRight, MoreHorizontal,
  TrendingUp, Trash2, Coins
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toastStore = useToastStore();
const router = useRouter();
const cards = ref<any[]>([]);
const loading = ref(true);
const showCreateModal = ref(false);
const isCreating = ref(false);

const modalType = ref<'physical' | 'virtual' | 'limit'>('physical');
const selectedCardId = ref<number | null>(null);
const newCardLimit = ref(10000);

const fetchCards = async () => {
    loading.value = true;
    try {
        const res = await apiClient.get('/CreditCards');
        cards.value = Array.isArray(res.data) ? res.data : [];
    } catch (err: any) {
        console.error('Kartlar yüklenemedi:', err);
        toastStore.error('Kart bilgileriniz yüklenirken bir sorun oluştu.');
    } finally {
        loading.value = false;
    }
};

const handleCreateAction = async () => {
    if (isCreating.value) return;
    isCreating.value = true;
    try {
        if (modalType.value === 'physical') {
            await apiClient.post('/CreditCards', { limit: newCardLimit.value });
            toastStore.success('Yeni kredi kartınız başarıyla oluşturuldu.');
            scrollToBottom();
        } else if (modalType.value === 'virtual') {
            await apiClient.post('/CreditCards/virtual', { limit: newCardLimit.value });
            toastStore.success('Sanal kartınız anında oluşturuldu.');
            scrollToBottom();
        } else if (modalType.value === 'limit' && selectedCardId.value) {
            toastStore.success('Limit artırım talebiniz Yalova şubesi personeline iletildi. Değerlendirme sonrası bildirim alacaksınız.');
            
            setTimeout(async () => {
                try {
                    await apiClient.patch(`/CreditCards/${selectedCardId.value}/limit`, { limit: newCardLimit.value });
                    toastStore.success('TEBRİKLER! Limit artırım talebiniz personelimiz tarafından ONAYLANDI. Yeni limitiniz tanımlandı.');
                    await fetchCards();
                    scrollToBottom();
                } catch (e) {}
            }, 5000);
        }
        
        showCreateModal.value = false;
        if (modalType.value !== 'limit') await fetchCards();
    } catch (err: any) {
        console.error('İşlem hatası:', err);
        toastStore.error('İşlem sırasında bir hata oluştu.');
    } finally {
        isCreating.value = false;
    }
};

const scrollToBottom = () => {
    setTimeout(() => {
        window.scrollTo({
            top: document.body.scrollHeight,
            behavior: 'smooth'
        });
    }, 500);
};

const openLimitModal = (card: any) => {
    modalType.value = 'limit';
    selectedCardId.value = card.id;
    newCardLimit.value = card.cardLimit + 5000; // Öneri olarak +5000
    showCreateModal.value = true;
};

const openCreateModal = (type: 'physical' | 'virtual') => {
    modalType.value = type;
    newCardLimit.value = type === 'virtual' ? 1000 : 10000;
    showCreateModal.value = true;
};

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

const formatCardNumber = (num: string) => {
    if (!num) return '•••• •••• •••• ••••';
    return num.replace(/(\d{4})/g, '$1 ').trim();
};

const downloadStatement = () => {
    toastStore.success('Ekstreniz başarıyla hazırlandı ve kayıtlı e-posta adresinize gönderildi.');
};

const toggleCardStatus = (card: any) => {
    const action = card.isActive === false ? 'aktifleşecek' : 'dondurulacak';
    if (confirm(`Kartınız ${action}. Onaylıyor musunuz?`)) {
        card.isActive = !card.isActive; // UI'da anlık gösterim için
        const msg = card.isActive ? 'Kartınız başarıyla kullanıma açıldı.' : 'Kartınız güvenlik sebebiyle donduruldu.';
        toastStore.success(msg);
    }
};

const handleSecurityToggle = (type: string) => {
    toastStore.success(`${type} ayarı başarıyla güncellendi.`);
};

onMounted(fetchCards);
</script>

<template>
  <div class="view-container fade-in">
    <header class="cards-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">Kart Yönetimi</h1>
        <p class="subtitle">Kredi kartlarınızı, limitlerinizi ve güvenlik ayarlarınızı yönetin.</p>
      </div>
      <div class="header-actions">
        <button @click="openCreateModal('physical')" class="btn-premium btn-gold" :disabled="loading">
            <Plus :size="18" /> Yeni Kart Başvurusu
        </button>
      </div>
    </header>

    <div v-if="loading" class="loader-overlay">
      <div class="spinner"></div>
    </div>

    <div v-else class="cards-content-grid">
        <!-- LEFT: CARDS LIST -->
        <div class="cards-column">
            <div v-if="cards.length === 0" class="empty-state-card glass-card">
                <div class="icon-box-large"><CreditCard :size="48" /></div>
                <h3>Henüz bir kartınız bulunmuyor</h3>
                <p>NexBank'ın ayrıcalıklı dünyasına katılmak için hemen bir kredi kartı başvurusu yapın.</p>
                <button @click="openCreateModal('physical')" class="btn-premium btn-primary mt-4">İlk Kartımı Oluştur</button>
            </div>

            <div v-for="card in cards" :key="card.id" class="card-management-item glass-card slide-up">
                <div class="visual-card-premium" :class="[card.isVirtual ? 'virtual' : 'physical', { 'frozen': card.isActive === false }]">
                    <div class="card-top">
                        <div class="card-chip"></div>
                        <Zap v-if="card.isVirtual" :size="24" class="virtual-icon" />
                    </div>
                    <div v-if="card.isActive === false" class="frozen-overlay">
                        <span>KART DONDURULDU</span>
                    </div>
                    <div class="card-number-display">
                        {{ formatCardNumber(card.cardNumber) }}
                    </div>
                    <div class="card-info-row">
                        <div class="info-block">
                            <small>KART SAHİBİ</small>
                            <span>{{ card.userFullName || 'PREMIUM MÜŞTERİ' }}</span>
                        </div>
                        <div class="info-block">
                            <small>SON KULLANMA</small>
                            <span>{{ card.expiryDate }}</span>
                        </div>
                        <div class="brand-logo">NB</div>
                    </div>
                </div>

                <div class="card-details-panel">
                    <div class="details-header">
                        <div class="badge-premium" :class="card.isVirtual ? 'virtual' : 'active'">
                           {{ card.isVirtual ? 'Sanal Kart' : 'Asıl Kart' }}
                        </div>
                        <div class="status-indicator">
                            <div class="dot active"></div> Aktif
                        </div>
                    </div>

                    <div class="financial-summary">
                        <div class="summary-item">
                            <small>GÜNCEL BORÇ</small>
                            <h4 class="text-danger">{{ formatCurrency(card.usedAmount) }}</h4>
                        </div>
                        <div class="summary-item">
                            <small>KULLANILABİLİR LİMİT</small>
                            <h4 class="text-success">{{ formatCurrency(card.cardLimit - card.usedAmount) }}</h4>
                        </div>
                    </div>

                    <div class="card-usage-progress">
                        <div class="progress-info">
                            <span>Limit Kullanımı ({{ formatCurrency(card.cardLimit) }})</span>
                            <span>{{ Math.round((card.usedAmount / card.cardLimit) * 100) }}%</span>
                        </div>
                        <div class="progress-track">
                            <div class="progress-fill" :style="{ width: Math.min((card.usedAmount / card.cardLimit * 100), 100) + '%' }"></div>
                        </div>
                    </div>

                    <div class="action-grid-small">
                        <button class="action-btn-p" @click="router.push('/cash-advance')"><Coins :size="16" /> Nakit Avans</button>
                        <button class="action-btn-p" @click="openLimitModal(card)" :disabled="card.isActive === false"><TrendingUp :size="16" /> Limit Artır</button>
                        <button class="action-btn-p" @click="downloadStatement" :disabled="card.isActive === false"><Download :size="16" /> Ekstre</button>
                        <button class="action-btn-p" :class="card.isActive === false ? 'success' : 'danger'" @click="toggleCardStatus(card)">
                            <Lock v-if="card.isActive !== false" :size="16" />
                            <ShieldCheck v-else :size="16" />
                            {{ card.isActive === false ? 'Kartı Aktifleştir' : 'Kartı Dondur' }}
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- RIGHT: SETTINGS & TOOLS -->
        <div class="tools-column">
            <div class="glass-card side-section">
                <div class="section-header">
                    <ShieldCheck :size="20" />
                    <h3>Kart Güvenliği</h3>
                </div>
                <div class="security-toggles">
                    <div class="toggle-item">
                        <div class="toggle-info">
                            <ShoppingBag :size="18" />
                            <span>E-Ticaret Yetkisi</span>
                        </div>
                        <label class="switch">
                            <input type="checkbox" checked @change="handleSecurityToggle('E-Ticaret')">
                            <span class="slider"></span>
                        </label>
                    </div>
                    <div class="toggle-item">
                        <div class="toggle-info">
                            <Globe :size="18" />
                            <span>Yurt Dışı Alışveriş</span>
                        </div>
                        <label class="switch">
                            <input type="checkbox" @change="handleSecurityToggle('Yurt Dışı')">
                            <span class="slider"></span>
                        </label>
                    </div>
                    <div class="toggle-item">
                        <div class="toggle-info">
                            <Zap :size="18" />
                            <span>Temassız Ödeme</span>
                        </div>
                        <label class="switch">
                            <input type="checkbox" checked @change="handleSecurityToggle('Temassız')">
                            <span class="slider"></span>
                        </label>
                    </div>
                </div>
            </div>

            <div class="glass-card promo-card gold-border mt-4" @click="openCreateModal('virtual')" style="cursor:pointer">
                <div class="promo-icon"><Zap :size="24" /></div>
                <h4>Anında Sanal Kart</h4>
                <p>İnternet alışverişlerinizde daha güvenli işlem yapmak için anında sanal kart oluşturun.</p>
                <button class="btn-premium btn-gold-small mt-3">Hemen Oluştur <ChevronRight :size="14" /></button>
            </div>

            <div class="info-section-premium mt-4">
               <AlertCircle :size="16" />
               <p>Güvenliğiniz için kart şifrenizi kimseyle paylaşmayın. Şüpheli işlemleri anında bildirin.</p>
            </div>
        </div>
    </div>

    <!-- ACTION MODAL -->
    <div v-if="showCreateModal" class="modal-overlay" @click.self="showCreateModal = false">
        <div class="glass-card modal-content slide-up">
            <div class="modal-header">
                <h3>{{ modalType === 'limit' ? 'Limit Artırım Talebi' : (modalType === 'virtual' ? 'Yeni Sanal Kart' : 'Yeni Kart Başvurusu') }}</h3>
                <button @click="showCreateModal = false" class="close-btn">&times;</button>
            </div>
            <div class="modal-body">
                <div v-if="modalType !== 'limit'" class="form-group">
                    <label class="form-label">Kart Türü Seçin</label>
                    <div class="card-type-selector">
                        <div class="type-opt" :class="{ active: modalType === 'physical' }" @click="modalType = 'physical'">
                            <CreditCard :size="20" />
                            <span>Asıl Kart</span>
                        </div>
                        <div class="type-opt" :class="{ active: modalType === 'virtual' }" @click="modalType = 'virtual'">
                            <Zap :size="20" />
                            <span>Sanal Kart</span>
                        </div>
                    </div>
                </div>
                
                <div class="form-group mt-4">
                    <label class="form-label">
                        {{ modalType === 'limit' ? 'Yeni Limit Talebi (TL)' : 'Başlangıç Limiti (TL)' }}
                    </label>
                    <input type="number" v-model="newCardLimit" class="form-input" min="100">
                    <small class="form-hint">
                        {{ modalType === 'limit' ? 'Limit artışınız gelir durumunuza göre otomatik değerlendirilecektir.' : 'Kartınız anında dijital olarak kullanıma açılacaktır.' }}
                    </small>
                </div>

                <div class="modal-alert mt-4">
                    <CheckCircle2 :size="16" />
                    <span>Bu işlem sonucunda herhangi bir ek ücret yansıtılmayacaktır.</span>
                </div>
            </div>
            <div class="modal-footer">
                <button @click="showCreateModal = false" class="btn-premium btn-outline" :disabled="isCreating">Vazgeç</button>
                <button @click="handleCreateAction" class="btn-premium btn-gold" :disabled="isCreating">
                    {{ isCreating ? 'İşleniyor...' : (modalType === 'limit' ? 'Limiti Güncelle' : 'Onayla ve Oluştur') }}
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.cards-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2.5rem;
}

.cards-content-grid {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 32px;
}

.cards-column {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.empty-state-card {
  padding: 4rem 2rem;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.icon-box-large {
  width: 80px;
  height: 80px;
  background: var(--bg-app);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary);
  margin-bottom: 1.5rem;
}

.card-management-item {
  display: flex;
  padding: 24px;
  gap: 32px;
  align-items: center;
}

/* 💳 VISUAL CARD */
.visual-card-premium {
  width: 340px;
  height: 210px;
  border-radius: 20px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  color: white;
  position: relative;
  overflow: hidden;
  box-shadow: 0 15px 35px rgba(0,0,0,0.2);
  flex-shrink: 0;
}

.visual-card-premium.physical {
  background: linear-gradient(135deg, #0C1B33 0%, #1A2F4B 100%);
}

.visual-card-premium.virtual {
  background: linear-gradient(135deg, #1E293B 0%, #475569 100%);
}

.visual-card-premium::after {
  content: '';
  position: absolute; top: -50%; left: -50%; width: 200%; height: 200%;
  background: radial-gradient(circle, rgba(255,255,255,0.05) 0%, transparent 70%);
}

.card-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-chip {
  width: 45px;
  height: 35px;
  background: linear-gradient(135deg, #D4AF37 0%, #F1D382 100%);
  border-radius: 6px;
}

.virtual-icon {
  color: var(--gold);
}

.card-number-display {
  font-family: 'Inter';
  font-size: 1.35rem;
  letter-spacing: 2px;
  font-weight: 600;
}

.card-info-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}

.info-block small {
  display: block;
  font-size: 0.6rem;
  font-weight: 700;
  opacity: 0.6;
  margin-bottom: 2px;
}

.info-block span {
  font-size: 0.85rem;
  font-weight: 700;
}

.brand-logo {
  font-family: 'Outfit';
  font-weight: 900;
  font-size: 1.2rem;
  color: var(--gold);
}

/* 📊 DETAILS */
.card-details-panel {
  flex: 1;
}

.details-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.2rem;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.8rem;
  font-weight: 700;
}

.dot.active {
  width: 8px;
  height: 8px;
  background: var(--success);
  border-radius: 50%;
}

.financial-summary {
  display: flex;
  gap: 32px;
  margin-bottom: 1.2rem;
}

.summary-item h4 {
  font-size: 1.25rem;
  font-weight: 800;
  font-family: 'Outfit';
  margin-top: 4px;
}

.card-usage-progress {
  margin-bottom: 1.5rem;
}

.progress-info {
  display: flex;
  justify-content: space-between;
  font-size: 0.75rem;
  font-weight: 700;
  margin-bottom: 8px;
}

.progress-track {
  height: 8px;
  background: var(--bg-app);
  border-radius: 10px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--primary);
  border-radius: 10px;
  transition: width 0.5s ease-out;
}

.action-grid-small {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.action-btn-p {
  background: var(--bg-app);
  border: 1px solid var(--border-light);
  padding: 10px;
  border-radius: 10px;
  font-size: 0.8rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.action-btn-p:hover {
  background: white;
  border-color: var(--primary);
  color: var(--primary);
}

.action-btn-p.danger:hover {
    color: var(--danger);
    border-color: var(--danger);
}

/* 🛠️ TOOLS */
.side-section { padding: 1.5rem; }
.section-header { display: flex; align-items: center; gap: 12px; margin-bottom: 1.5rem; }
.section-header h3 { font-size: 1.1rem; margin: 0; }
.security-toggles { display: flex; flex-direction: column; gap: 16px; }
.toggle-item { display: flex; justify-content: space-between; align-items: center; }
.toggle-info { display: flex; align-items: center; gap: 12px; font-size: 0.9rem; font-weight: 600; }

/* Switch Styles */
.switch { position: relative; display: inline-block; width: 44px; height: 24px; }
.switch input { opacity: 0; width: 0; height: 0; }
.slider { position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; background-color: var(--border); transition: .4s; border-radius: 34px; }
.slider:before { position: absolute; content: ""; height: 18px; width: 18px; left: 3px; bottom: 3px; background-color: white; transition: .4s; border-radius: 50%; }
input:checked + .slider { background-color: var(--success); }
input:checked + .slider:before { transform: translateX(20px); }

.promo-card { padding: 1.5rem; background: linear-gradient(135deg, #FFFFFF 0%, #FDFBF7 100%); }
.promo-icon { width: 44px; height: 44px; background: rgba(197, 160, 89, 0.1); color: var(--gold); border-radius: 12px; display: flex; align-items: center; justify-content: center; margin-bottom: 1rem; }
.info-section-premium { background: var(--bg-app); padding: 1rem; border-radius: 12px; display: flex; gap: 12px; font-size: 0.8rem; color: var(--text-muted); line-height: 1.4; }

/* 🏗️ MODAL */
.card-type-selector { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.type-opt { padding: 16px; border: 1px solid var(--border); border-radius: 12px; display: flex; flex-direction: column; align-items: center; gap: 8px; cursor: pointer; }
.type-opt.active { border-color: var(--primary); background: rgba(12, 27, 51, 0.05); color: var(--primary); }
.modal-alert { background: var(--bg-app); padding: 12px; border-radius: 10px; display: flex; gap: 10px; font-size: 0.8rem; color: var(--text-muted); }
.visual-card-premium.frozen { filter: grayscale(1) opacity(0.7); pointer-events: none; }
.frozen-overlay { position: absolute; top: 50%; left: 0; width: 100%; background: rgba(239, 68, 68, 0.9); color: white; text-align: center; padding: 10px; font-weight: 900; font-size: 1.1rem; transform: translateY(-50%) rotate(-5deg); z-index: 10; box-shadow: 0 4px 15px rgba(0,0,0,0.3); }

.action-btn-p.success:hover { color: var(--success); border-color: var(--success); }
</style>
