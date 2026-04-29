<script setup lang="ts">
import { ref } from 'vue';
import { 
  Plus, Calendar, Clock, ArrowRight, 
  Pause, Play, AlertCircle, CheckCircle2,
  Home, Globe, Zap, Music
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const showAddModal = ref(false);
const isProcessing = ref(false);

const schedules = ref([
    { id: 1, title: 'Ev Kirası Ödemesi', amount: 15500, date: 'Her ayın 15. günü', status: 'Aktif', color: '#10B981', icon: Home },
    { id: 2, title: 'İnternet Faturası (Otomatik)', amount: 299.90, date: 'Her ayın 2. günü', status: 'Aktif', color: '#10B981', icon: Globe },
    { id: 3, title: 'Abonelik: NexMusic', amount: 49.90, date: 'Her ayın 10. günü', status: 'Duraklatıldı', color: '#EF4444', icon: Music }
]);

const newSchedule = ref({
    title: '',
    amount: 0,
    day: 1
});

const handleToggle = (id: number) => {
    const item = schedules.value.find(s => s.id === id);
    if (item) {
        item.status = item.status === 'Aktif' ? 'Duraklatıldı' : 'Aktif';
        item.color = item.status === 'Aktif' ? '#10B981' : '#EF4444';
        toast.success(`Talimat ${item.status === 'Aktif' ? 'aktif edildi' : 'duraklatıldı'}.`);
    }
};

const addNewSchedule = () => {
    if (!newSchedule.value.title || newSchedule.value.amount <= 0) {
        toast.error("Lütfen geçerli bilgiler girin.");
        return;
    }
    
    isProcessing.value = true;
    setTimeout(() => {
        const newItem = {
            id: Date.now(),
            title: newSchedule.value.title,
            amount: newSchedule.value.amount,
            date: `Her ayın ${newSchedule.value.day}. günü`,
            status: 'Aktif',
            color: '#10B981',
            icon: Zap
        };
        schedules.value.push(newItem);
        toast.success("Yeni ödeme talimatınız başarıyla oluşturuldu.");
        showAddModal.value = false;
        isProcessing.value = false;
        
        // Temizle
        newSchedule.value = { title: '', amount: 0, day: 1 };
        
        // Kaydır
        setTimeout(() => {
            window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
        }, 500);
    }, 1000);
};

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};
</script>

<template>
  <div class="view-container view-animate">
    <header class="view-header-wrap">
      <h1 class="view-title text-gradient">Zamanlanmış İşlemler</h1>
      <p class="subtitle">Otomatik ödeme talimatlarınızı ve periyodik transferlerinizi yönetin.</p>
    </header>

    <div class="glass-card mb-5 p-0 overflow-hidden">
        <div class="p-4 border-bottom d-flex justify-content-between align-items-center">
            <div class="d-flex align-items-center gap-2">
                <Clock :size="20" class="text-primary" />
                <h3 class="m-0" style="font-size: 1.1rem; font-weight: 800;">Aktif Talimatlarım</h3>
            </div>
            <button @click="showAddModal = true" class="btn-premium btn-gold-small">
                <Plus :size="16" /> Yeni Talimat Ekle
            </button>
        </div>
        <div class="schedule-list">
            <div v-for="s in schedules" :key="s.id" class="schedule-item-v2">
                <div class="s-main">
                    <div class="s-icon-box-v2" :style="{ backgroundColor: s.color + '15', color: s.color }">
                        <component :is="s.icon" :size="20" />
                    </div>
                    <div class="s-details">
                        <h4>{{ s.title }}</h4>
                        <div class="s-meta">
                            <Calendar :size="12" />
                            <span>{{ s.date }}</span>
                        </div>
                    </div>
                </div>
                <div class="s-price-v2">
                    <div class="amount">{{ formatCurrency(s.amount) }}</div>
                    <div class="status-badge" :style="{ color: s.color }">
                        <div class="dot" :style="{ backgroundColor: s.color }"></div>
                        {{ s.status }}
                    </div>
                </div>
                <div class="s-actions-v2">
                    <button @click="handleToggle(s.id)" class="action-toggle-btn" :class="{ 'is-active': s.status === 'Aktif' }">
                        <Pause v-if="s.status === 'Aktif'" :size="16" />
                        <Play v-else :size="16" />
                        {{ s.status === 'Aktif' ? 'Durdur' : 'Başlat' }}
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- YENİ TALİMAT MODAL -->
    <div v-if="showAddModal" class="modal-overlay" @click.self="showAddModal = false">
        <div class="glass-card modal-content-v2 slide-up">
            <div class="modal-header">
                <h3>Yeni Ödeme Talimatı</h3>
                <button @click="showAddModal = false" class="close-btn">&times;</button>
            </div>
            <div class="modal-body">
                <div class="form-group">
                    <label class="form-label">Talimat Başlığı</label>
                    <input type="text" v-model="newSchedule.title" class="form-input" placeholder="Örn: Netflix Aboneliği">
                </div>
                <div class="form-row mt-3">
                    <div class="form-group flex-fill">
                        <label class="form-label">Tutar (TL)</label>
                        <input type="number" v-model="newSchedule.amount" class="form-input">
                    </div>
                    <div class="form-group flex-fill">
                        <label class="form-label">Her Ayın Günü</label>
                        <select v-model="newSchedule.day" class="form-input">
                            <option v-for="d in 28" :key="d" :value="d">{{ d }}. Günü</option>
                        </select>
                    </div>
                </div>
                <div class="modal-alert-p mt-4">
                    <AlertCircle :size="16" />
                    <span>Talimat onaylandığı an her ay otomatik çekim yapılacaktır.</span>
                </div>
            </div>
            <div class="modal-footer">
                <button @click="showAddModal = false" class="btn-premium btn-outline" :disabled="isProcessing">Vazgeç</button>
                <button @click="addNewSchedule" class="btn-premium btn-gold" :disabled="isProcessing">
                    {{ isProcessing ? 'Oluşturuluyor...' : 'Talimatı Onayla' }}
                </button>
            </div>
        </div>
    </div>

    <!-- BİLGİ KARTI -->
    <div class="glass-card bg-light-info">
        <div class="d-flex gap-3 align-items-center">
            <div class="i-icon">💡</div>
            <div class="i-content">
                <strong>Zamanlanmış Ödeme Esnekliği</strong>
                <p class="m-0 text-muted">Ödemeleriniz gerçekleşmeden 24 saat önce size haber veriyoruz. Dilediğiniz zaman duraklatabilir veya tutar güncellemesi yapabilirsiniz.</p>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.schedule-item-v2 { 
    display: flex; align-items: center; justify-content: space-between; 
    padding: 24px 32px; border-bottom: 1px solid #F1F5F9; transition: all 0.2s;
}
.schedule-item-v2:hover { background: rgba(12, 27, 51, 0.02); }

.s-main { display: flex; align-items: center; gap: 20px; flex: 1.5; }
.s-icon-box-v2 { width: 52px; height: 52px; border-radius: 16px; display: flex; align-items: center; justify-content: center; }

.s-details h4 { margin: 0; font-size: 1rem; color: #0F172A; font-weight: 700; }
.s-meta { display: flex; align-items: center; gap: 6px; font-size: 0.75rem; color: #64748B; font-weight: 600; margin-top: 4px; }

.s-price-v2 { flex: 1; text-align: right; margin-right: 40px; }
.s-price-v2 .amount { font-size: 1.15rem; font-weight: 800; color: #0F172A; font-family: 'Outfit'; }
.status-badge { display: flex; align-items: center; justify-content: flex-end; gap: 6px; font-size: 0.7rem; font-weight: 800; margin-top: 4px; }
.status-badge .dot { width: 6px; height: 6px; border-radius: 50%; }

.s-actions-v2 { width: 120px; text-align: right; }
.action-toggle-btn { 
    width: 100%; padding: 10px; border-radius: 10px; border: 1.5px solid #E2E8F0; 
    background: white; font-size: 0.75rem; font-weight: 800; display: flex; 
    align-items: center; justify-content: center; gap: 8px; cursor: pointer; transition: 0.2s;
}
.action-toggle-btn.is-active:hover { border-color: #EF4444; color: #EF4444; }
.action-toggle-btn:not(.is-active):hover { border-color: #10B981; color: #10B981; }

.modal-content-v2 { width: 500px; padding: 32px; }
.modal-alert-p { background: #F8FAFC; padding: 16px; border-radius: 12px; display: flex; gap: 12px; font-size: 0.8rem; color: #64748B; font-weight: 600; }

.bg-light-info { border-left: 4px solid var(--primary); background: #F8FAFC !important; padding: 24px; border-radius: 20px; }
</style>
