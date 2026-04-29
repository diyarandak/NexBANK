<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const pageName = computed(() => {
    return route.name?.toString().replace(/-/g, ' ').toUpperCase() || 'ÖZELLİK';
});
</script>

<template>
  <div class="view-container view-animate text-center p-5">
    <div class="glass-card p-5 mt-5">
        <div class="placeholder-icon">🚀</div>
        <h1 class="text-gradient mb-3">{{ pageName }} Çok Yakında!</h1>
        <p class="subtitle mx-auto" style="max-width: 600px;">
            NexBank 2.0 vizyonumuzun bir parçası olarak <strong>{{ pageName }}</strong> modülü şu an uzman ekibimiz tarafından geliştiriliyor. 
            Güvenli, hızlı ve premium bir deneyim için bizi takip etmeye devam edin.
        </p>
        <div class="mt-5 progress-box">
            <div class="p-label d-flex justify-content-between mb-2">
                <span>Geliştirme Süreci</span>
                <span>%75</span>
            </div>
            <div class="p-bar"><div class="p-fill" style="width: 75%"></div></div>
        </div>
        <button @click="$router.back()" class="btn-action mt-5 mx-auto">← Geri Dön</button>
    </div>
  </div>
</template>

<style scoped>
.placeholder-icon { font-size: 5rem; margin-bottom: 30px; animation: bounce 2s infinite; }
@keyframes bounce { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-15px); } }

.progress-box { max-width: 400px; margin: 0 auto; }
.p-label { font-size: 0.8rem; font-weight: 800; color: #64748B; }
.p-bar { height: 8px; background: #E2E8F0; border-radius: 4px; overflow: hidden; }
.p-fill { height: 100%; background: linear-gradient(90deg, #F59E0B, #D97706); border-radius: 4px; }
</style>
