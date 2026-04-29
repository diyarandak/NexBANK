<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore();
const activeTab = ref('profile'); // profile, notifications, tickets

const profileForm = ref({ fullName: '', email: '' });
const notifications = ref<any[]>([]);
const tickets = ref<any[]>([]);
const loading = ref(true);

const fetchProfileData = async () => {
    loading.value = true;
    try {
        const [profRes, notifRes, tickRes] = await Promise.all([
            apiClient.get('/profile'),
            apiClient.get('/profile/notifications'),
            apiClient.get('/profile/tickets')
        ]);
        profileForm.value = { fullName: profRes.data.fullName, email: profRes.data.email };
        notifications.value = notifRes.data;
        tickets.value = tickRes.data;
    } catch (err) {
        console.error(err);
    } finally {
        loading.value = false;
    }
};

onMounted(fetchProfileData);

const updateProfile = async () => {
    try {
        await apiClient.put('/profile', profileForm.value);
        alert('Profiliniz güncellendi.');
        if (authStore.user) {
            authStore.user.fullName = profileForm.value.fullName;
        }
    } catch (err) {
        alert('Güncelleme başarısız.');
    }
};

const markAsRead = async (id: number) => {
    try {
        await apiClient.post(`/profile/notifications/${id}/read`);
        const item = notifications.value.find(n => n.id === id);
        if (item) item.isRead = true;
    } catch (err) { /* ignore */ }
};

const newTicket = ref({ subject: '', message: '' });
const submitTicket = async () => {
    try {
        await apiClient.post('/profile/tickets', newTicket.value);
        alert('Destek talebiniz alındı.');
        newTicket.value = { subject: '', message: '' };
        await fetchProfileData();
    } catch (err) {
        alert('Talep gönderilemedi.');
    }
};
</script>

<template>
  <div class="profile-view">
    <header class="view-header">
      <h2 class="text-gradient">Hesap ve Güvenlik</h2>
      <p>Kişisel bilgilerinizi güncelleyin, bildirimlerinizi kontrol edin ve destek alın.</p>
    </header>

    <div class="tabs glass-card">
      <button :class="['tab-btn', { active: activeTab === 'profile' }]" @click="activeTab = 'profile'">👤 Profil</button>
      <button :class="['tab-btn', { active: activeTab === 'notifications' }]" @click="activeTab = 'notifications'">
        🔔 Bildirimler <span v-if="notifications.filter(n=>!n.isRead).length" class="count-badge">{{ notifications.filter(n=>!n.isRead).length }}</span>
      </button>
      <button :class="['tab-btn', { active: activeTab === 'tickets' }]" @click="activeTab = 'tickets'">🆘 Destek</button>
    </div>

    <div v-if="loading" class="loader">Yükleniyor...</div>
    <div v-else class="tab-content">
      
      <!-- PROFIL GÜNCELLEME -->
      <div v-if="activeTab === 'profile'" class="tab-pane animate-fade">
        <div class="glass-card profile-card">
          <h3>Profil Bilgileri</h3>
          <form @submit.prevent="updateProfile" class="form">
            <div class="form-group">
              <label>TC Kimlik No</label>
              <input :value="authStore.user?.identityNumber" disabled class="disabled-input" />
            </div>
            <div class="form-group">
              <label>Ad Soyad</label>
              <input v-model="profileForm.fullName" required />
            </div>
            <div class="form-group">
              <label>E-Posta</label>
              <input v-model="profileForm.email" type="email" required />
            </div>
            <button type="submit" class="premium-button w-full mt-4">Değişiklikleri Kaydet</button>
          </form>
        </div>
      </div>

      <!-- BİLDİRİMLER -->
      <div v-if="activeTab === 'notifications'" class="tab-pane animate-fade">
        <div class="notifications-list">
          <div v-for="n in notifications" :key="n.id" :class="['notif-item glass-card', { unread: !n.isRead }]">
            <div class="notif-content">
              <h4>{{ n.title }}</h4>
              <p>{{ n.message }}</p>
              <small>{{ new Date(n.createdAt).toLocaleString() }}</small>
            </div>
            <button v-if="!n.isRead" @click="markAsRead(n.id)" class="read-btn">Okundu İşaretle</button>
          </div>
          <p v-if="notifications.length === 0" class="empty-state">Henüz bildiriminiz yok.</p>
        </div>
      </div>

      <!-- DESTEK TALEPLERİ -->
      <div v-if="activeTab === 'tickets'" class="tab-pane animate-fade">
        <div class="support-layout">
          <div class="glass-card ticket-form-pane">
            <h3>Yeni Destek Talebi</h3>
            <form @submit.prevent="submitTicket">
              <div class="form-group">
                <label>Konu</label>
                <input v-model="newTicket.subject" required placeholder="İşlem hatası, Kart şifresi vb." />
              </div>
              <div class="form-group">
                <label>İletiniz</label>
                <textarea v-model="newTicket.message" required rows="5" placeholder="Lütfen detaylı bilgi verin..."></textarea>
              </div>
              <button type="submit" class="premium-button w-full">Gönder</button>
            </form>
          </div>
          <div class="glass-card ticket-list-pane">
            <h3>Geçmiş Taleplerim</h3>
            <div class="tickets">
              <div v-for="t in tickets" :key="t.id" class="ticket-item">
                <div class="t-main">
                  <strong>{{ t.subject }}</strong>
                  <span :class="['status-badge', t.status.toLowerCase()]">{{ t.status }}</span>
                </div>
                <p class="t-msg">{{ t.message }}</p>
                <small>{{ new Date(t.createdAt).toLocaleDateString() }}</small>
              </div>
              <p v-if="tickets.length === 0" class="empty-state">Geçmiş talebiniz bulunmuyor.</p>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
.profile-view { animation: fadeIn 0.4s ease; padding-bottom: 50px; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

.view-header { margin-bottom: 24px; }
.view-header h2 { font-size: 2rem; margin-bottom: 8px; }

.tabs { display: flex; gap: 8px; margin-bottom: 24px; padding: 6px; border-radius: 14px; position: relative; }
.tab-btn { flex: 1; padding: 12px; background: transparent; border: none; color: var(--text-muted); font-weight: 600; border-radius: 10px; cursor: pointer; transition: all 0.3s; display: flex; align-items: center; justify-content: center; gap: 8px;}
.tab-btn.active { background: linear-gradient(135deg, var(--primary), var(--accent)); color: white; }

.count-badge { background: #ef4444; color: white; padding: 2px 6px; border-radius: 50%; font-size: 0.7rem; }

.profile-card { max-width: 500px; margin: 0 auto; padding: 30px; border-radius: 20px; }
.form-group { margin-bottom: 16px; }
.form-group label { display: block; margin-bottom: 8px; color: var(--text-muted); font-size: 0.9rem; }
.form-group input, .form-group textarea { width: 100%; padding: 12px; background: rgba(0,0,0,0.3); border: 1px solid var(--border); border-radius: 8px; color: #fff; }
.disabled-input { opacity: 0.6; cursor: not-allowed; }

.notifications-list { display: flex; flex-direction: column; gap: 15px; max-width: 800px; margin: 0 auto; }
.notif-item { padding: 20px; border-radius: 16px; display: flex; justify-content: space-between; align-items: center; border-left: 4px solid transparent; }
.notif-item.unread { border-left-color: var(--primary); background: rgba(255,255,255,0.05); }
.notif-content h4 { margin-bottom: 4px; }
.notif-content p { color: var(--text-muted); font-size: 0.95rem; }
.read-btn { background: rgba(59,130,246,0.1); color: #3b82f6; border: 1px solid #3b82f6; padding: 6px 12px; border-radius: 8px; cursor: pointer; font-size: 0.8rem; }

.support-layout { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
@media (max-width: 800px) { .support-layout { grid-template-columns: 1fr; } }
.ticket-form-pane, .ticket-list-pane { padding: 24px; border-radius: 20px; }

.ticket-item { padding: 16px; border-bottom: 1px solid var(--border); }
.ticket-item:last-child { border-bottom: none; }
.t-main { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.t-msg { font-size: 0.9rem; color: var(--text-muted); margin-bottom: 8px; }
.status-badge { font-size: 0.7rem; padding: 2px 8px; border-radius: 10px; text-transform: uppercase; }
.status-badge.açık { background: #10b981; color: white; }
.status-badge.kapalı { background: #333; color: #aaa; }

.w-full { width: 100%; }
.mt-4 { margin-top: 16px; }
.animate-fade { animation: fadeIn 0.3s ease; }
</style>
