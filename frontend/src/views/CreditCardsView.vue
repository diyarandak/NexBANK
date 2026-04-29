<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';

const cards = ref<any[]>([]);
const loading = ref(true);
const showVirtualModal = ref(false);

const virtualForm = ref({
    limit: 1000,
    label: 'İnternet Alışverişim'
});

const fetchCards = async () => {
  loading.value = true;
  try {
    const res = await apiClient.get('/creditcards');
    cards.value = res.data;
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
};

onMounted(fetchCards);

const createVirtualCard = async () => {
    try {
        // Backend API desteği varsayımıyla (Hali hazırda modelde CreditCard var)
        await apiClient.post('/creditcards/virtual', virtualForm.value);
        showVirtualModal.value = false;
        fetchCards();
    } catch (err) {
        alert('Sanal kart oluşturulamadı.');
    }
};

const formatCardNumber = (num: string) => {
    return num.replace(/\d{4}(?=.)/g, '$& ');
};
</script>

<template>
  <div class="cards-page view-animate">
    <header class="header">
      <div class="header-content">
        <h1 class="text-gradient">Kartlarım</h1>
        <p class="subtitle">Tüm kredi ve sanal kartlarını buradan yönetebilirsin.</p>
      </div>
      <button @click="showVirtualModal = true" class="premium-button">
        <span>✨</span> Sanal Kart Oluştur
      </button>
    </header>

    <div v-if="loading" class="loader">Kartlarınız yükleniyor...</div>

    <div v-else class="cards-grid">
      <div v-for="card in cards" :key="card.id" :class="['card-item', card.isVirtual ? 'virtual' : 'physical']">
        <div class="card-chip"></div>
        <div class="card-logo">NexBank <span :class="card.isVirtual ? 'v-badge' : 'p-badge'">{{ card.isVirtual ? 'VIRTUAL' : 'METAL' }}</span></div>
        
        <div class="card-number">{{ formatCardNumber(card.cardNumber) }}</div>
        
        <div class="card-details">
            <div class="detail-group">
                <small>KART SAHİBİ</small>
                <p>{{ card.holderName || 'DEĞERLİ MÜŞTERİMİZ' }}</p>
            </div>
            <div class="detail-group">
                <small>SKT</small>
                <p>{{ card.expiryDate }}</p>
            </div>
            <div class="card-brand">VISA</div>
        </div>

        <div class="card-footer">
            <div class="limit-info">
                <span>Kullanılabilir Limit: <strong>{{ card.availableLimit.toLocaleString() }} ₺</strong></span>
                <div class="limit-bar">
                    <div class="limit-fill" :style="{ width: (card.usedAmount / card.cardLimit * 100) + '%' }"></div>
                </div>
            </div>
        </div>
      </div>

      <!-- Sanal Kart Modalı -->
      <div v-if="showVirtualModal" class="modal-overlay">
          <div class="modal glass-card">
              <h3>Üst Segment Sanal Kart</h3>
              <p>İnternet harcamaların için anında limit tanımla.</p>
              <div class="form-group mt-4">
                  <label>Başlangıç Limiti (₺)</label>
                  <input v-model="virtualForm.limit" type="number" class="premium-input" />
              </div>
              <div class="modal-actions">
                  <button @click="showVirtualModal = false" class="demo-btn">Vazgeç</button>
                  <button @click="createVirtualCard" class="premium-button">Oluştur</button>
              </div>
          </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: 30px;
  margin-top: 30px;
}

.card-item {
  height: 240px;
  border-radius: 24px;
  padding: 30px;
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  color: white;
  overflow: hidden;
  box-shadow: 0 20px 40px rgba(0,0,0,0.2);
  transition: transform 0.3s;
}
.card-item:hover { transform: translateY(-10px) rotate(1deg); }

/* FİZİKSEL KART (METAL BLACK) */
.card-item.physical {
  background: linear-gradient(135deg, #0C1B33 0%, #1a2f4c 100%);
  border: 1px solid rgba(197, 160, 89, 0.4);
}

/* SANAL KART (GOLDEN-BLUE) */
.card-item.virtual {
  background: linear-gradient(135deg, #C5A059 0%, #E0AA3E 100%);
  color: #0C1B33;
}

.card-chip {
  width: 45px;
  height: 35px;
  background: linear-gradient(135deg, #F1F5F9 0%, #CBD5E1 100%);
  border-radius: 8px;
}

.card-logo {
  position: absolute;
  top: 30px;
  right: 30px;
  font-family: 'Playfair Display', serif;
  font-weight: 900;
  font-size: 1.2rem;
}

.v-badge { font-size: 0.6rem; background: #0C1B33; color: #C5A059; padding: 2px 6px; border-radius: 4px; }
.p-badge { font-size: 0.6rem; background: #C5A059; color: white; padding: 2px 6px; border-radius: 4px; }

.card-number {
  font-family: 'Courier New', Courier, monospace;
  font-size: 1.6rem;
  letter-spacing: 2px;
  margin-top: 40px;
}

.card-details { display: flex; gap: 40px; align-items: flex-end; }
.detail-group small { display: block; font-size: 0.6rem; opacity: 0.7; margin-bottom: 4px; }
.detail-group p { font-size: 0.9rem; font-weight: 700; letter-spacing: 1px; }

.card-brand { margin-left: auto; font-size: 1.4rem; font-style: italic; font-weight: 900; }

.card-footer { margin-top: 15px; }
.limit-info { font-size: 0.8rem; }
.limit-bar { height: 4px; background: rgba(255,255,255,0.1); border-radius: 2px; margin-top: 8px; overflow: hidden; }
.limit-fill { height: 100%; background: #C5A059; }

.card-item.virtual .limit-fill { background: #0C1B33; }
.card-item.virtual .limit-bar { background: rgba(0,0,0,0.05); }

.modal-overlay { position: fixed; inset: 0; background: rgba(12, 27, 51, 0.4); backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal { width: 450px; padding: 40px; }
</style>
