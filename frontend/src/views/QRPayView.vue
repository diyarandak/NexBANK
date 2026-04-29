<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useToastStore } from '../store/toast';
import { 
  QrCode, Camera, Zap, ShieldCheck, 
  X, Landmark, ShoppingBag, ArrowRight,
  Maximize, CheckCircle2, AlertCircle
} from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import apiClient from '../api/client';

const toast = useToastStore();
const router = useRouter();

const scanState = ref<'idle' | 'scanning' | 'confirmed' | 'success'>('idle');
const activeTab = ref<'pay' | 'withdraw'>('pay');
const scannedData = ref<any>(null);
const amount = ref<number | string>('');
const selectedAccount = ref<any>(null);
const accounts = ref<any[]>([]);

const videoRef = ref<HTMLVideoElement | null>(null);
const stream = ref<MediaStream | null>(null);

const startScan = async () => {
    scanState.value = 'scanning';
    
    try {
        // GERÇEK KAMERA ERİŞİMİ
        stream.value = await navigator.mediaDevices.getUserMedia({ 
            video: { facingMode: 'environment' } 
        });
        if (videoRef.value) {
            videoRef.value.srcObject = stream.value;
        }
    } catch (err) {
        console.error('Kamera erişim hatası:', err);
        toast.error('Kameraya erişilemedi. Simülasyon modunda devam ediliyor.');
    }

    // 3 saniye sonra "tarama tamamlandı" simülasyonu
    setTimeout(() => {
        stopCamera();
        if (activeTab.value === 'withdraw') {
            scannedData.value = {
                type: 'ATM',
                name: 'NexBank ATM #042 - Beşiktaş',
                location: 'İstanbul / Beşiktaş',
                fee: 0
            };
        } else {
            scannedData.value = {
                type: 'POS',
                name: 'Premium Coffee Co.',
                location: 'Nişantaşı Şubesi',
                fee: 0.50
            };
        }
        scanState.value = 'confirmed';
    }, 3500);
};

const stopCamera = () => {
    if (stream.value) {
        stream.value.getTracks().forEach(track => track.stop());
        stream.value = null;
    }
};

onUnmounted(() => {
    stopCamera();
});

const handleTransaction = async () => {
    if (!amount.value || parseFloat(amount.value.toString()) <= 0) {
        toast.error('Lütfen geçerli bir tutar girin.');
        return;
    }

    if (!selectedAccount.value) {
        toast.error('Lütfen işlem yapılacak hesabı seçin.');
        return;
    }

    try {
        // Simüle edilmiş QR işlem kaydı
        const payload = {
            accountId: selectedAccount.value.id,
            amount: parseFloat(amount.value.toString()),
            description: `QR ${activeTab.value === 'pay' ? 'Ödeme' : 'Para Çekme'}: ${scannedData.value.name}`
        };
        
        await apiClient.post(`/Transactions/${activeTab.value === 'pay' ? 'withdraw' : 'withdraw'}`, payload);
        
        scanState.value = 'success';
        toast.success('İşleminiz başarıyla tamamlandı.');
        
        setTimeout(() => {
            router.push('/dashboard');
        }, 3000);
    } catch (err) {
        toast.error('İşlem sırasında bir hata oluştu. Lütfen bakiyenizi kontrol edin.');
    }
};

onMounted(async () => {
    try {
        const res = await apiClient.get('/Accounts');
        accounts.value = res.data;
        if (accounts.value.length > 0) selectedAccount.value = accounts.value[0];
    } catch (err) {
        console.error('Hesaplar yüklenemedi');
    }
});
</script>

<template>
  <div class="view-container fade-in">
    <header class="qr-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">QR İşlemleri</h1>
        <p class="subtitle">Kameranızı kullanarak ATM'den para çekin veya anında ödeme yapın.</p>
      </div>
      <div class="header-badges">
        <div class="secure-badge"><ShieldCheck :size="16" /> Şifreli Tarama</div>
      </div>
    </header>

    <div class="qr-main-layout">
        <!-- 📱 SCANNER AREA -->
        <div class="scanner-section glass-card">
            <div class="nav-tabs-premium">
                <button :class="{ active: activeTab === 'pay' }" @click="activeTab = 'pay'; scanState = 'idle'">
                    <ShoppingBag :size="18" /> QR ile Öde
                </button>
                <button :class="{ active: activeTab === 'withdraw' }" @click="activeTab = 'withdraw'; scanState = 'idle'">
                    <Landmark :size="18" /> QR ile Para Çek
                </button>
            </div>

            <div class="scanner-view-container">
                <!-- IDLE STATE -->
                <div v-if="scanState === 'idle'" class="scanner-idle">
                    <div class="qr-placeholder">
                        <QrCode :size="80" class="qr-icon-pulse" />
                    </div>
                    <h3>Taramaya Başla</h3>
                    <p>ATM veya POS cihazındaki QR kodu kadrajın içine alacak şekilde kameranızı tutun.</p>
                    <button @click="startScan" class="btn-premium btn-gold mt-4">
                        <Camera :size="20" /> Kamerayı Aç
                    </button>
                </div>

                <!-- SCANNING STATE -->
                <div v-if="scanState === 'scanning'" class="scanner-active">
                    <div class="camera-simulation">
                        <video ref="videoRef" autoplay playsinline class="camera-video"></video>
                        <div class="scan-overlay">
                            <div class="scan-frame">
                                <div class="corner tl"></div>
                                <div class="corner tr"></div>
                                <div class="corner bl"></div>
                                <div class="corner br"></div>
                                <div class="laser-line"></div>
                            </div>
                        </div>
                    </div>
                    <div class="scan-status mt-4">
                        <div class="spinner-mini"></div>
                        <span>QR Kod Aranıyor...</span>
                    </div>
                    <button @click="scanState = 'idle'" class="btn-cancel mt-3"><X :size="16" /> İptal</button>
                </div>

                <!-- CONFIRMATION STATE -->
                <div v-if="scanState === 'confirmed'" class="scanner-confirmation slide-up">
                    <div class="target-card glass-card">
                        <div class="target-icon">
                            <Landmark v-if="scannedData.type === 'ATM'" :size="32" />
                            <ShoppingBag v-else :size="32" />
                        </div>
                        <div class="target-info">
                            <h4>{{ scannedData.name }}</h4>
                            <p>{{ scannedData.location }}</p>
                        </div>
                    </div>

                    <div class="transaction-form mt-4">
                        <div class="form-group-p">
                            <label>İşlem Yapılacak Hesap</label>
                            <select v-model="selectedAccount" class="p-select">
                                <option v-for="acc in accounts" :key="acc.id" :value="acc">
                                    {{ acc.accountType }} - {{ acc.balance.toLocaleString() }} {{ acc.currency }}
                                </option>
                            </select>
                        </div>
                        <div class="form-group-p mt-3">
                            <label>Tutar (TL)</label>
                            <input type="number" v-model="amount" placeholder="0.00" class="p-input-lg" />
                        </div>

                        <div class="summary-box mt-4">
                            <div class="s-row">
                                <span>İşlem Ücreti</span>
                                <span class="text-success">{{ scannedData.fee > 0 ? scannedData.fee + ' ₺' : 'Ücretsiz' }}</span>
                            </div>
                            <div class="s-row total">
                                <span>Toplam</span>
                                <span>{{ (parseFloat(amount.toString() || '0') + scannedData.fee).toLocaleString() }} ₺</span>
                            </div>
                        </div>

                        <div class="action-buttons mt-4">
                            <button @click="scanState = 'idle'" class="btn-premium btn-outline">Vazgeç</button>
                            <button @click="handleTransaction" class="btn-premium btn-gold">
                                <Zap :size="18" /> Onayla ve Bitir
                            </button>
                        </div>
                    </div>
                </div>

                <!-- SUCCESS STATE -->
                <div v-if="scanState === 'success'" class="scanner-success">
                    <div class="success-anim">
                        <CheckCircle2 :size="80" color="var(--success)" />
                    </div>
                    <h2>İşlem Başarılı!</h2>
                    <p>Referans No: #QR-{{ Math.floor(Math.random()*90000) + 10000 }}</p>
                    <div class="success-details glass-card mt-4">
                        <div class="d-row"><span>Tutar:</span> <strong>{{ amount }} ₺</strong></div>
                        <div class="d-row"><span>Nokta:</span> <strong>{{ scannedData.name }}</strong></div>
                    </div>
                    <button @click="router.push('/dashboard')" class="btn-premium btn-primary mt-4">Ana Sayfaya Dön</button>
                </div>
            </div>
        </div>

        <!-- ℹ️ INFO SIDEBAR -->
        <div class="info-sidebar">
            <div class="glass-card promo-info-v2">
                <div class="p-header">
                    <Zap :size="20" class="gold-icon" />
                    <h3>Neden QR?</h3>
                </div>
                <ul class="p-list mt-3">
                    <li>Kart taşıma zorunluluğunu ortadan kaldırır.</li>
                    <li>Temassız ve hijyenik bir deneyim sunar.</li>
                    <li>Şifre girmeden anında işlem yapmanızı sağlar.</li>
                </ul>
            </div>

            <div class="glass-card security-card-v2 mt-4">
                <div class="p-header">
                    <ShieldCheck :size="20" class="success-icon" />
                    <h3>Güvenlik İpucu</h3>
                </div>
                <p class="mt-2 small">Sadece tanıdığınız POS cihazlarındaki ve bankamıza ait ATM'lerdeki QR kodları okutun.</p>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.qr-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}

.secure-badge {
  background: rgba(16, 185, 129, 0.1);
  color: #10B981;
  padding: 8px 16px;
  border-radius: 100px;
  font-size: 0.8rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.qr-main-layout {
  display: grid;
  grid-template-columns: 1fr 350px;
  gap: 32px;
}

.scanner-section {
  padding: 0;
  overflow: hidden;
  min-height: 600px;
  display: flex;
  flex-direction: column;
}

.nav-tabs-premium {
  display: flex;
  background: var(--bg-app);
  padding: 8px;
  gap: 8px;
}

.nav-tabs-premium button {
  flex: 1;
  padding: 14px;
  border: none;
  border-radius: 12px;
  background: transparent;
  color: var(--text-muted);
  font-weight: 700;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  cursor: pointer;
  transition: all 0.2s;
}

.nav-tabs-premium button.active {
  background: white;
  color: var(--primary-dark);
  box-shadow: var(--shadow-sm);
}

.scanner-view-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px;
  position: relative;
}

/* IDLE STATE */
.scanner-idle {
  text-align: center;
  max-width: 400px;
}

.qr-placeholder {
  width: 160px;
  height: 160px;
  background: var(--bg-app);
  border-radius: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 2rem;
  color: var(--primary);
}

.qr-icon-pulse {
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { transform: scale(1); opacity: 0.8; }
  50% { transform: scale(1.1); opacity: 1; }
  100% { transform: scale(1); opacity: 0.8; }
}

/* SCANNING STATE */
.camera-simulation {
  width: 320px;
  height: 320px;
  background: #000;
  border-radius: 40px;
  position: relative;
  overflow: hidden;
  box-shadow: 0 20px 50px rgba(0,0,0,0.3);
}

.camera-video {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.scan-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.2);
  display: flex;
  align-items: center;
  justify-content: center;
}

.scan-frame {
  width: 200px;
  height: 200px;
  position: relative;
}

.corner {
  position: absolute;
  width: 30px;
  height: 30px;
  border: 4px solid var(--gold);
}

.tl { top: 0; left: 0; border-right: 0; border-bottom: 0; border-top-left-radius: 12px; }
.tr { top: 0; right: 0; border-left: 0; border-bottom: 0; border-top-right-radius: 12px; }
.bl { bottom: 0; left: 0; border-right: 0; border-top: 0; border-bottom-left-radius: 12px; }
.br { bottom: 0; right: 0; border-left: 0; border-top: 0; border-bottom-right-radius: 12px; }

.laser-line {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 2px;
  background: var(--gold);
  box-shadow: 0 0 15px var(--gold);
  animation: scan 2s linear infinite;
}

@keyframes scan {
  0% { top: 0; }
  50% { top: 100%; }
  100% { top: 0; }
}

.scan-status {
  display: flex;
  align-items: center;
  gap: 12px;
  font-weight: 700;
  color: var(--text-main);
}

.spinner-mini {
  width: 20px; height: 20px; border: 3px solid var(--border-light);
  border-top-color: var(--primary); border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

.btn-cancel {
  background: var(--bg-app);
  border: 1px solid var(--border-light);
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

/* CONFIRMATION STATE */
.scanner-confirmation {
    width: 100%;
    max-width: 480px;
}

.target-card {
    display: flex;
    align-items: center;
    gap: 20px;
    padding: 20px;
    background: var(--primary-dark);
    color: white;
    border: none;
}

.target-icon {
    width: 60px; height: 60px; background: rgba(255,255,255,0.1);
    border-radius: 16px; display: flex; align-items: center; justify-content: center;
}

.target-info h4 { font-size: 1.1rem; margin: 0 0 4px 0; }
.target-info p { font-size: 0.85rem; opacity: 0.7; margin: 0; }

.form-group-p label { display: block; font-size: 0.85rem; font-weight: 700; color: var(--text-muted); margin-bottom: 8px; }
.p-select, .p-input-lg {
    width: 100%; padding: 14px; background: var(--bg-app); border: 1px solid var(--border-light);
    border-radius: 12px; font-weight: 700; font-size: 1rem;
}
.p-input-lg { font-size: 1.5rem; font-family: 'Outfit'; }

.summary-box { background: var(--bg-app); padding: 16px; border-radius: 16px; }
.s-row { display: flex; justify-content: space-between; font-size: 0.9rem; font-weight: 600; margin-bottom: 8px; }
.s-row.total { border-top: 1px solid var(--border-light); padding-top: 12px; margin-top: 12px; font-weight: 800; font-size: 1.1rem; }

.action-buttons { display: flex; gap: 16px; }
.action-buttons button { flex: 1; padding: 16px; }

/* SUCCESS STATE */
.scanner-success { text-align: center; }
.success-anim { margin-bottom: 1.5rem; animation: bounceIn 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55); }
@keyframes bounceIn { from { transform: scale(0); } to { transform: scale(1); } }

.success-details { padding: 20px; width: 300px; margin: 0 auto; }
.d-row { display: flex; justify-content: space-between; font-size: 0.9rem; padding: 8px 0; border-bottom: 1px solid var(--border-light); }
.d-row:last-child { border: none; }

/* SIDEBAR */
.p-header { display: flex; align-items: center; gap: 10px; }
.gold-icon { color: var(--gold); }
.success-icon { color: var(--success); }
.p-list { list-style: none; padding: 0; }
.p-list li { font-size: 0.85rem; color: var(--text-muted); margin-bottom: 10px; display: flex; align-items: flex-start; gap: 8px; line-height: 1.4; }
.p-list li::before { content: '•'; color: var(--gold); font-weight: 800; }
</style>
