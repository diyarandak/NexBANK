<script setup lang="ts">
import { useAuthStore } from './store/auth';
import { useToastStore } from './store/toast';
import Sidebar from './components/Sidebar.vue';
import TopHeader from './components/TopHeader.vue';
import ChatBot from './components/ChatBot.vue';

const authStore = useAuthStore();
const toastStore = useToastStore();
</script>

<template>
  <div class="app-container">
    <!-- Sidebar sadece giriş yapılmışsa görünür -->
    <Sidebar v-if="authStore.isAuthenticated" />
    
    <div class="content-wrapper" v-if="authStore.isAuthenticated">
      <TopHeader />
      <main class="main-content">
        <router-view></router-view>
      </main>
      <!-- 🤖 AI CHATBOT (Sadece giriş yapılmışsa) -->
      <ChatBot v-if="authStore.isAuthenticated" />
    </div>

    <main v-else style="width: 100%;">
      <router-view></router-view>
    </main>

    <!-- 🔔 GLOBAL TOAST NOTIFICATIONS -->
    <div class="toast-container">
      <TransitionGroup name="toast-list">
        <div v-for="t in toastStore.toasts" :key="t.id" :class="['toast-item', t.type]">
            <div class="toast-icon">{{ t.type === 'success' ? '✅' : t.type === 'error' ? '❌' : 'ℹ️' }}</div>
            <div class="toast-content">{{ t.message }}</div>
        </div>
      </TransitionGroup>
    </div>
  </div>
</template>

<style>
.app-container {
  display: flex;
  min-height: 100vh;
}

.content-wrapper {
  flex: 1;
  margin-left: var(--sidebar-width); /* Sidebar genişliği kadar sağa it */
  padding-top: var(--header-height); /* Header yüksekliği kadar aşağı it */
  min-width: 0;
  display: flex;
  flex-direction: column;
}

.main-content {
  flex: 1;
  padding: 0; 
  overflow-x: hidden;
}
</style>
