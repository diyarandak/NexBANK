<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useAuthStore } from '../store/auth';
import apiClient from '../api/client';
import { Search, Bell, CheckCircle2 } from 'lucide-vue-next';

const authStore = useAuthStore();
const showNotifications = ref(false);
const notifications = ref<any[]>([]);
const unreadCount = ref(0);
let pollInterval: any = null;

const fetchNotifications = async () => {
    if (!authStore.isAuthenticated) return;
    try {
        const res = await apiClient.get('/notifications');
        notifications.value = res.data;
        unreadCount.value = notifications.value.filter(n => !n.isRead).length;
    } catch (err) { /* Sessiz hata */ }
};

onMounted(() => {
    fetchNotifications();
    pollInterval = setInterval(fetchNotifications, 15000);
});

onUnmounted(() => {
    if (pollInterval) clearInterval(pollInterval);
});

const toggleNotifications = () => {
    showNotifications.value = !showNotifications.value;
};
</script>

<template>
  <header class="top-header">
    <div class="header-container">
      <!-- 🔍 ARAMA BÖLÜMÜ -->
      <div class="search-wrapper">
        <Search class="search-icon" :size="18" />
        <input type="text" placeholder="Hesap, işlem veya özellik ara..." class="search-input" />
      </div>

      <!-- 🔔 AKSİYON BÖLÜMÜ -->
      <div class="header-actions">
        <div class="notif-target" @click="toggleNotifications">
          <div class="bell-icon-wrap" :class="{ 'has-new': unreadCount > 0 }">
            <Bell :size="20" />
            <span v-if="unreadCount > 0" class="notif-badge">{{ unreadCount }}</span>
          </div>

          <!-- BİLDİRİM PANELİ -->
          <div v-if="showNotifications" class="dropdown-panel slide-up">
            <div class="panel-header">
              <h3>Bildirimler</h3>
              <button @click.stop="unreadCount = 0" class="read-all">
                <CheckCircle2 :size="14" /> Hepsini Oku
              </button>
            </div>
            <div class="panel-body">
              <div v-for="n in notifications.slice(0, 5)" :key="n.id" class="dropdown-item">
                <div class="item-dot"></div>
                <div class="item-content">
                  <p>{{ n.message }}</p>
                  <small>Şimdi</small>
                </div>
              </div>
              <div v-if="notifications.length === 0" class="panel-empty">Bildirim bulunmuyor.</div>
            </div>
            <router-link to="/notifications" class="panel-footer" @click="showNotifications = false">
              Tümünü Gör
            </router-link>
          </div>
        </div>

        <!-- 👤 PROFİL BÖLÜMÜ -->
        <div class="profile-wrap">
          <div class="profile-labels">
            <span class="p-welcome">Hoş Geldin,</span>
            <span class="p-name">{{ authStore.user?.fullName.split(' ')[0] }}</span>
          </div>
          <div class="p-avatar">
            {{ authStore.user?.fullName[0] }}
            <div class="p-online"></div>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped>
.top-header {
  height: var(--header-height);
  position: fixed;
  top: 0;
  right: 0;
  left: var(--sidebar-width);
  background: var(--glass);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--border);
  z-index: 999;
  padding: 0 40px;
}

.header-container {
  height: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* Search Wrapper */
.search-wrapper {
  background: var(--bg-app);
  border-radius: 14px;
  width: 400px;
  padding: 10px 18px;
  display: flex;
  align-items: center;
  gap: 12px;
  border: 1px solid transparent;
  transition: all 0.2s;
}

.search-wrapper:focus-within {
  background: white;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px rgba(197,160,58,0.05);
}

.search-icon { color: var(--text-muted); }

.search-input {
  border: none !important;
  background: transparent !important;
  padding: 0 !important;
  font-size: 0.9rem !important;
  box-shadow: none !important;
  font-weight: 500 !important;
}

/* Actions Section */
.header-actions {
  display: flex;
  align-items: center;
  gap: 24px;
}

.notif-target { position: relative; cursor: pointer; }

.bell-icon-wrap {
  position: relative;
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-app);
  border-radius: 12px;
  transition: all 0.2s;
  color: var(--text-main);
}

.bell-icon-wrap:hover { 
  background: white; 
  color: var(--primary);
  box-shadow: var(--shadow-sm);
}

.notif-badge {
  position: absolute; top: -4px; right: -4px;
  background: var(--danger); color: white;
  font-size: 0.65rem; font-weight: 800;
  padding: 2px 6px; border-radius: 50%;
  border: 3px solid white;
}

/* Profile Section */
.profile-wrap { display: flex; align-items: center; gap: 16px; }
.profile-labels { display: flex; flex-direction: column; text-align: right; }
.p-welcome { font-size: 0.7rem; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.05em; }
.p-name { font-size: 0.95rem; font-weight: 800; color: var(--primary-dark); font-family: 'Outfit'; }

.p-avatar {
  width: 44px; height: 44px;
  background: var(--primary-dark); color: var(--primary);
  border-radius: 12px; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 1.1rem; position: relative;
  box-shadow: var(--shadow-sm);
}

.p-online {
  position: absolute; bottom: -2px; right: -2px;
  width: 12px; height: 12px; background: var(--success);
  border-radius: 50%; border: 2.5px solid white;
}

/* Dropdown Panel */
.dropdown-panel {
  position: absolute; top: 60px; right: 0;
  width: 340px; background: white; border-radius: 20px;
  box-shadow: var(--shadow-lg);
  border: 1px solid var(--border); overflow: hidden;
}

.panel-header {
  padding: 20px 24px; border-bottom: 1px solid var(--border-light);
  display: flex; justify-content: space-between; align-items: center;
}
.panel-header h3 { font-size: 1rem; margin: 0; font-family: 'Outfit'; }
.read-all { 
  border: none; background: transparent; color: var(--primary); 
  font-weight: 700; font-size: 0.75rem; cursor: pointer;
  display: flex; align-items: center; gap: 4px;
}

.panel-body { max-height: 320px; overflow-y: auto; }
.dropdown-item {
  padding: 16px 24px; display: flex; gap: 14px; border-bottom: 1px solid var(--border-light);
  transition: background 0.2s;
}
.dropdown-item:hover { background: var(--bg-app); }
.item-dot { width: 8px; height: 8px; background: var(--primary); border-radius: 50%; margin-top: 6px; flex-shrink: 0; }
.item-content p { font-size: 0.85rem; font-weight: 600; margin: 0; line-height: 1.4; color: var(--text-main); }
.item-content small { font-size: 0.7rem; color: var(--text-muted); font-weight: 600; margin-top: 4px; display: block; }

.panel-empty { padding: 40px; text-align: center; color: var(--text-muted); font-size: 0.85rem; }

.panel-footer {
  display: block; padding: 14px; text-align: center;
  background: var(--bg-app); color: var(--primary-dark);
  font-size: 0.8rem; font-weight: 700; text-decoration: none;
  transition: all 0.2s;
}
.panel-footer:hover { background: var(--border-light); color: var(--primary); }
</style>

