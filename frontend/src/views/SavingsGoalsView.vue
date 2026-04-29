<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';

const goals = ref<any[]>([]);
const loading = ref(true);
const showModal = ref(false);

const newGoal = ref({
    title: '',
    targetAmount: 0,
    category: 'Diğer',
    deadline: new Date().toISOString().split('T')[0]
});

const fetchGoals = async () => {
    loading.value = true;
    try {
        const res = await apiClient.get('/goals');
        goals.value = res.data;
    } catch (err) {
        console.error(err);
    } finally {
        loading.value = false;
    }
};

const createGoal = async () => {
    try {
        await apiClient.post('/goals', newGoal.value);
        showModal.value = false;
        fetchGoals();
    } catch (err) {
        alert('Hedef oluşturulamadı.');
    }
};

const addMoney = async (goalId: number) => {
    const amount = prompt('Biriktirilen tutarı girin (₺):');
    if (!amount || isNaN(Number(amount))) return;
    
    try {
        await apiClient.post(`/goals/${goalId}/add-money`, Number(amount));
        fetchGoals();
    } catch (err) {
        alert('İşlem başarısız.');
    }
};

const deleteGoal = async (goalId: number) => {
    if (!confirm('Bu hedefi silmek istediğinize emin misiniz?')) return;
    try {
        await apiClient.delete(`/goals/${goalId}`);
        fetchGoals();
    } catch (err) {
        alert('Silme başarısız.');
    }
};

onMounted(fetchGoals);

const getProgress = (goal: any) => {
    return Math.min(Math.round((goal.currentAmount / goal.targetAmount) * 100), 100);
};

const getCategoryIcon = (cat: string) => {
    switch (cat) {
        case 'Tatil': return '✈️';
        case 'Araba': return '🚗';
        case 'Ev': return '🏠';
        case 'Teknoloji': return '💻';
        default: return '🎯';
    }
};
</script>

<template>
  <div class="goals-page">
    <header class="header">
      <div class="header-content">
        <h1 class="text-gradient">Birikim Hedeflerim</h1>
        <p class="subtitle">Hayalleriniz için akıllıca para biriktirin.</p>
      </div>
      <button @click="showModal = true" class="premium-button">
        <span>+</span> Yeni Hedef Oluştur
      </button>
    </header>

    <div v-if="loading" class="loader">Hedefler yükleniyor...</div>
    
    <div v-else class="goals-grid">
        <div v-for="goal in goals" :key="goal.id" class="goal-card glass-card" :class="{ completed: goal.isCompleted }">
            <div class="goal-header">
                <span class="icon">{{ getCategoryIcon(goal.category) }}</span>
                <span class="category-name">{{ goal.category }}</span>
                <button @click="deleteGoal(goal.id)" class="delete-btn">×</button>
            </div>
            <h3 class="title">{{ goal.title }}</h3>
            
            <div class="progress-section">
                <div class="progress-info">
                    <span class="percent">%{{ getProgress(goal) }} Tamamlandı</span>
                    <span class="remaining">Kalan: {{ (goal.targetAmount - goal.currentAmount).toLocaleString() }} ₺</span>
                </div>
                <div class="progress-bg">
                    <div class="progress-fill" :style="{ width: getProgress(goal) + '%' }"></div>
                </div>
            </div>

            <div class="goal-amounts">
                <div class="amt">
                    <small>Mevcut</small>
                    <p>{{ goal.currentAmount.toLocaleString() }} ₺</p>
                </div>
                <div class="amt text-right">
                    <small>Hedef</small>
                    <p>{{ goal.targetAmount.toLocaleString() }} ₺</p>
                </div>
            </div>

            <button v-if="!goal.isCompleted" @click="addMoney(goal.id)" class="add-money-btn">
                Miktar Ekle
            </button>
            <div v-else class="completed-msg">🎉 Tebrikler! Hedefe Ulaşıldı.</div>
        </div>

        <div v-if="goals.length === 0" class="empty-state glass-card">
            <p>Henüz bir birikim hedefiniz yok. İlk hedefinizi hemen oluşturun!</p>
        </div>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="modal-overlay">
        <div class="modal glass-card">
            <h3>Hayalindeki Hedefi Belirle</h3>
            <div class="form-group">
                <label>Hedef Başlığı</label>
                <input v-model="newGoal.title" type="text" placeholder="Örn: Yeni Model Araba" class="premium-input" />
            </div>
            <div class="form-group">
                <label>Hedeflenen Tutar (₺)</label>
                <input v-model="newGoal.targetAmount" type="number" class="premium-input" />
            </div>
            <div class="form-group">
                <label>Kategori</label>
                <select v-model="newGoal.category" class="premium-input">
                    <option>Tatil</option>
                    <option>Araba</option>
                    <option>Ev</option>
                    <option>Teknoloji</option>
                    <option>Diğer</option>
                </select>
            </div>
            <div class="modal-actions">
                <button @click="showModal = false" class="demo-btn">Vazgeç</button>
                <button @click="createGoal" class="premium-button">Hedefi Kaydet</button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 40px; }
.goals-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 24px; }
.goal-card { padding: 24px; display: flex; flex-direction: column; gap: 16px; border-top: 4px solid var(--primary); }
.goal-card.completed { border-top-color: var(--success); }
.goal-header { display: flex; align-items: center; gap: 10px; position: relative; }
.category-name { font-size: 0.75rem; font-weight: 700; color: var(--text-muted); text-transform: uppercase; }
.delete-btn { position: absolute; right: -10px; top: -10px; background: transparent; border: none; color: var(--text-muted); font-size: 1.2rem; cursor: pointer; }

.progress-info { display: flex; justify-content: space-between; font-size: 0.75rem; margin-bottom: 6px; }
.progress-bg { height: 8px; background: rgba(255,255,255,0.05); border-radius: 4px; overflow: hidden; }
.progress-fill { height: 100%; background: linear-gradient(90deg, var(--primary), var(--accent)); border-radius: 4px; transition: width 0.5s ease; }

.goal-amounts { display: flex; justify-content: space-between; }
.amt small { display: block; font-size: 0.65rem; color: var(--text-muted); text-transform: uppercase; }
.amt p { font-weight: 700; color: #EEE; }
.text-right { text-align: right; }

.add-money-btn { width: 100%; padding: 10px; border: 1px dashed var(--primary); background: transparent; color: var(--primary); border-radius: 10px; font-weight: 600; cursor: pointer; transition: all 0.3s; }
.add-money-btn:hover { background: var(--primary-soft); }
.completed-msg { text-align: center; color: var(--success); font-weight: 700; font-size: 0.9rem; padding: 10px; background: rgba(16, 185, 129, 0.1); border-radius: 10px; }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.8); backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal { width: 400px; padding: 30px; }
.form-group { margin-bottom: 20px; }
.modal-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 20px; }
</style>
