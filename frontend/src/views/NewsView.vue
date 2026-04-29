<script setup lang="ts">
import { ref } from 'vue';

const newsItems = ref([
  {
    id: 1,
    title: "Merkez Bankası Faiz Kararını Açıkladı",
    summary: "Para Politikası Kurulu, politika faizini piyasa beklentileri doğrultusunda sabit tuttuğunu duyurdu.",
    category: "Ekonomi",
    time: "10 dk önce",
    impact: "high"
  },
  {
    id: 2,
    title: "Teknoloji Hisselerinde Ralli Sürüyor",
    summary: "Yapay zeka odaklı şirketlerin bilançoları sonrası borsada teknoloji endeksi rekor tazeledi.",
    category: "Borsa",
    time: "45 dk önce",
    impact: "medium"
  },
  {
    id: 3,
    title: "NexBank'tan Genç Girişimcilere Destek",
    summary: "Yeni 'Girişimci Paketi' ile düşük faizli kredi ve ücretsiz danışmanlık hizmeti sunulmaya başlandı.",
    category: "Banka",
    time: "2 saat önce",
    impact: "low"
  },
  {
    id: 4,
    title: "Döviz Piyasasında Sakin Seyir",
    summary: "Haftanın ilk işlem gününde dolar ve euro kurunda sınırlı bantta hareket gözleniyor.",
    category: "Döviz",
    time: "3 saat önce",
    impact: "medium"
  }
]);
</script>

<template>
  <div class="news-page">
    <header class="header">
      <h1 class="text-gradient">Finansal Gündem</h1>
      <p class="subtitle">Piyasalardaki son gelişmeleri ve NexBank duyurularını takip edin.</p>
    </header>

    <div class="news-grid">
      <div v-for="news in newsItems" :key="news.id" class="news-card glass-card">
        <div class="news-meta">
          <span class="category">{{ news.category }}</span>
          <span class="time">{{ news.time }}</span>
        </div>
        <h3 class="news-title">{{ news.title }}</h3>
        <p class="news-summary">{{ news.summary }}</p>
        <div class="news-footer">
          <span :class="['impact-tag', news.impact]">Etki: {{ news.impact === 'high' ? 'Yüksek' : (news.impact === 'medium' ? 'Orta' : 'Düşük') }}</span>
          <button class="read-more">Devamını Oku →</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.header { margin-bottom: 40px; }
.news-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 24px;
}
.news-card {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  transition: transform 0.3s;
}
.news-card:hover { transform: translateY(-5px); }
.news-meta {
  display: flex;
  justify-content: space-between;
  font-size: 0.75rem;
  font-weight: 600;
}
.category { color: var(--primary); text-transform: uppercase; letter-spacing: 1px; }
.time { color: var(--text-muted); }
.news-title { font-size: 1.2rem; color: #EEE; line-height: 1.4; }
.news-summary { font-size: 0.9rem; color: var(--text-muted); line-height: 1.6; }
.news-footer {
  margin-top: auto;
  padding-top: 15px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.impact-tag {
  font-size: 0.7rem;
  padding: 4px 8px;
  border-radius: 6px;
  font-weight: bold;
}
.impact-tag.high { background: rgba(239, 68, 68, 0.1); color: #EF4444; }
.impact-tag.medium { background: rgba(245, 158, 11, 0.1); color: #F59E0B; }
.impact-tag.low { background: rgba(16, 185, 129, 0.1); color: #10B981; }
.read-more {
  background: transparent;
  border: none;
  color: var(--primary);
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
}
</style>
