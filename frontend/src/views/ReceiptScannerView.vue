<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { 
  Camera, Upload, FileText, Check, 
  RotateCcw, Sparkles, Loader2, AlertCircle,
  ShoppingBag, Calendar, CreditCard, ArrowRight,
  Image, Plus, CheckCircle2
} from 'lucide-vue-next';
import { createWorker } from 'tesseract.js';
import { useToastStore } from '../store/toast';
import apiClient from '../api/client';

const toast = useToastStore();
const imageSrc = ref<string | null>(null);
const isProcessing = ref(false);
const progress = ref(0);
const step = ref<'upload' | 'scanning' | 'result' | 'camera'>('upload');
const videoRef = ref<HTMLVideoElement | null>(null);
const stream = ref<MediaStream | null>(null);

const resultData = ref({
    merchant: '',
    total: 0,
    date: new Date().toLocaleDateString('tr-TR'),
    category: 'Market & Gıda'
});

const handleFileUpload = (event: Event) => {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            imageSrc.value = e.target?.result as string;
            processReceipt();
        };
        reader.readAsDataURL(file);
    }
};

const processReceipt = async () => {
    if (!imageSrc.value) return;
    
    isProcessing.value = true;
    step.value = 'scanning';
    progress.value = 0;

    try {
        const worker = await createWorker('tur', 1, {
            logger: m => {
                if (m.status === 'recognizing text') {
                    progress.value = Math.round(m.progress * 100);
                }
            }
        });

        const { data: { text } } = await worker.recognize(imageSrc.value);
        await worker.terminate();

        console.log("OCR RAW TEXT:", text);
        parseReceiptData(text);
        
        step.value = 'result';
        toast.success("Fiş başarıyla tarandı!");
    } catch (err) {
        console.error(err);
        toast.error("Fiş okuma hatası. Lütfen daha net bir fotoğraf deneyin.");
        step.value = 'upload';
    } finally {
        isProcessing.value = false;
    }
};

const aiInsight = ref({
    title: 'Analiz Bekleniyor',
    desc: 'Veriler işlendiğinde finansal zekâ devreye girecek.',
    severity: 'neutral'
});

const parseReceiptData = (text: string) => {
    const lines = text.split('\n');
    
    // 1. Tutar Bul
    const priceRegex = /(TOPLAM|TOP|TOTAL|TUTAR)[:\s]*([\d,.]+)/i;
    const match = text.match(priceRegex);
    if (match && match[2]) {
        resultData.value.total = parseFloat(match[2].replace(',', '.'));
    } else {
        const allPrices = text.match(/\d+[,.]\d{2}/g);
        if (allPrices) {
            const numericPrices = allPrices.map(p => parseFloat(p.replace(',', '.')));
            resultData.value.total = Math.max(...numericPrices);
        }
    }

    // 2. Market Bul
    const validLines = lines.filter(l => l.trim().length > 3 && !l.includes('*'));
    resultData.value.merchant = validLines.length > 0 ? validLines[0].trim().toUpperCase() : "Bilinmeyen İşletme";

    // 3. Zekâ Analizi Oluştur
    if (resultData.value.total > 1500) {
        aiInsight.value = {
            title: 'Yüksek Harcama Uyarısı',
            desc: `Bu ${resultData.value.total} TL'lik harcama, günlük limitinizin %40'ını oluşturuyor. Bütçenizi korumak için bir sonraki harcamayı ertelemeyi düşünebilirsiniz.`,
            severity: 'danger'
        };
    } else if (resultData.value.total > 500) {
        aiInsight.value = {
            title: 'Bütçe Planlama',
            desc: `Harcamanız gıda bütçenizle uyumlu görünüyor. NexCard kullanarak bu işlemden 15.00 NexPuan kazanabilirsiniz!`,
            severity: 'warning'
        };
    } else {
        aiInsight.value = {
            title: 'Mükemmel Yönetim',
            desc: "Küçük ama etkili bir harcama! Tasarruf hedeflerine sadık kaldığın için NexBank sana teşekkür eder.",
            severity: 'success'
        };
    }
};

const saveExpense = async () => {
    try {
        // Dashboard entegrasyonu simülasyonu
        const newTransaction = {
            id: 'scan-' + Date.now(),
            type: 'Withdrawal',
            description: `NexVision: ${resultData.value.merchant}`,
            amount: resultData.value.total,
            createdAt: new Date().toISOString()
        };
        
        // LocalStorage'a ekle ki Dashboard'dan okuyabilelim (Demo için)
        const scannedExpenses = JSON.parse(localStorage.getItem('scanned_expenses') || '[]');
        scannedExpenses.unshift(newTransaction);
        localStorage.setItem('scanned_expenses', JSON.stringify(scannedExpenses));

        toast.success("Harcama başarıyla bütçenize entegre edildi!");
        
        // 1 saniye sonra ana sayfaya yönlendir
        setTimeout(() => {
            router.push('/dashboard');
        }, 1000);
    } catch (err) {
        toast.error("Kaydedilemedi.");
    }
};

const startCamera = async () => {
    try {
        step.value = 'camera';
        stream.value = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } });
        if (videoRef.value) {
            videoRef.value.srcObject = stream.value;
        }
    } catch (err) {
        toast.error("Kamera erişimi reddedildi veya bulunamadı.");
        step.value = 'upload';
    }
};

const capturePhoto = () => {
    if (videoRef.value) {
        const canvas = document.createElement('canvas');
        canvas.width = videoRef.value.videoWidth;
        canvas.height = videoRef.value.videoHeight;
        const ctx = canvas.getContext('2d');
        ctx?.drawImage(videoRef.value, 0, 0);
        imageSrc.value = canvas.toDataURL('image/jpeg');
        stopCamera();
        processReceipt();
    }
};

const stopCamera = () => {
    if (stream.value) {
        stream.value.getTracks().forEach(track => track.stop());
        stream.value = null;
    }
};

const reset = () => {
    stopCamera();
    imageSrc.value = null;
    step.value = 'upload';
    progress.value = 0;
};

onUnmounted(() => {
    stopCamera();
});
</script>

<template>
  <div class="view-container fade-in">
    <header class="receipt-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">NexVision AI</h1>
        <p class="subtitle">Fişini tara, harcamalarını otomatik olarak bütçene işle.</p>
      </div>
      <div class="ai-status-p">
          <Sparkles :size="18" /> <span>Yapay Zekâ Aktif</span>
      </div>
    </header>

    <div class="receipt-grid mt-4">
        <!-- 📸 INPUT PANEL -->
        <div class="glass-card-v5 input-panel-p">
            <div v-if="step === 'upload'" class="upload-zone-p">
                <div class="uz-inner">
                    <div class="uz-icon-wrap">
                        <div class="uz-icon"><Camera :size="48" /></div>
                        <div class="uz-icon-plus"><Plus :size="20" /></div>
                        <div class="uz-icon gallery"><Image :size="48" /></div>
                    </div>
                    <h3>Fişinizi Okutun</h3>
                    <p>Kamerayı kullanın veya galeriden bir fotoğraf seçin.</p>
                    
                    <div class="upload-actions-grid mt-5">
                        <button class="upload-action-btn camera" @click="startCamera">
                            <Camera :size="24" />
                            <span>Kamera</span>
                        </button>
                        <label class="upload-action-btn gallery">
                            <Image :size="24" />
                            <span>Galeri</span>
                            <input type="file" @change="handleFileUpload" accept="image/*" hidden />
                        </label>
                    </div>
                    <div class="drag-drop-hint mt-4">
                        veya dosyayı buraya sürükleyin
                    </div>
                </div>
                <div class="uz-tips">
                    <div class="tip"><Check :size="14" /> Net görünürlük</div>
                    <div class="tip"><Check :size="14" /> Tüm formatlar (JPG, PNG)</div>
                </div>
            </div>
            <div v-if="step === 'camera'" class="camera-zone-p">
                <video ref="videoRef" autoplay playsinline class="camera-video"></video>
                <div class="camera-controls">
                    <button @click="reset" class="btn-cam-cancel"><RotateCcw :size="20" /></button>
                    <button @click="capturePhoto" class="btn-cam-capture">
                        <div class="inner-circle"></div>
                    </button>
                </div>
            </div>

            <div v-if="step === 'scanning'" class="scanning-zone-p">
                <div class="receipt-preview-p">
                    <img :src="imageSrc!" alt="Preview" />
                    <div class="scan-line" :style="{ top: progress + '%' }"></div>
                    <div class="scan-overlay"></div>
                </div>
                <div class="scan-status mt-4">
                    <div class="status-meta">
                        <Loader2 :size="24" class="spin" />
                        <span>Fiş Analiz Ediliyor... %{{ progress }}</span>
                    </div>
                    <div class="progress-bar-p">
                        <div class="pb-fill" :style="{ width: progress + '%' }"></div>
                    </div>
                </div>
            </div>

            <div v-if="step === 'result'" class="result-preview-p">
                <div class="rp-image-small">
                    <img :src="imageSrc!" alt="Final" />
                </div>
                <div class="rp-actions mt-4">
                    <button @click="reset" class="btn-premium btn-outline w-full">
                        <RotateCcw :size="18" /> YENİDEN TARA
                    </button>
                </div>
            </div>
        </div>

        <!-- 🧾 RESULT PANEL -->
        <div class="glass-card-v5 result-panel-p">
            <div class="rp-h">
                <FileText :size="20" />
                <span>TARAMA SONUCU</span>
            </div>

            <div v-if="step !== 'result'" class="rp-empty">
                <div class="rpe-icon"><ArrowRight :size="48" /></div>
                <p>Verileri görmek için bir fiş yükleyin.</p>
            </div>

            <div v-else class="rp-data fade-in">
                <!-- 🏢 CORE DATA -->
                <div class="data-group-v2">
                    <div class="data-item">
                        <label>İŞLETME</label>
                        <input v-model="resultData.merchant" type="text" class="data-input" />
                    </div>
                    <div class="data-row mt-3">
                        <div class="data-item">
                            <label>TOPLAM</label>
                            <div class="price-input-p">
                                <input v-model="resultData.total" type="number" class="data-input" />
                                <span>₺</span>
                            </div>
                        </div>
                        <div class="data-item">
                            <label>KDV (%20)</label>
                            <input :value="(resultData.total * 0.166).toFixed(2)" disabled class="data-input readonly" />
                        </div>
                    </div>
                </div>

                <!-- 📊 DEEP ANALYSIS REPORT -->
                <div class="analysis-report-p mt-4">
                    <div class="ar-h">FİNANSAL ANALİZ RAPORU</div>
                    
                    <div class="ar-metrics mt-3">
                        <div class="ar-metric">
                            <div class="arm-val">{{ resultData.total > 1000 ? 'Yüksek' : 'Normal' }}</div>
                            <div class="arm-label">Harcama Seviyesi</div>
                        </div>
                        <div class="ar-metric">
                            <div class="arm-val text-gold">+{{ (resultData.total * 0.02).toFixed(0) }}</div>
                            <div class="arm-label">NexPuan Kazanç</div>
                        </div>
                        <div class="ar-metric">
                            <div class="arm-val text-success">Uyumlu</div>
                            <div class="arm-label">Bütçe Durumu</div>
                        </div>
                    </div>

                    <div class="ar-insight-box mt-4" :class="aiInsight.severity">
                        <div class="aib-h">
                            <Sparkles :size="16" />
                            <strong>{{ aiInsight.title }}</strong>
                        </div>
                        <p>{{ aiInsight.desc }}</p>
                        <div class="aib-footer mt-2">
                            <div class="tag-p">#{{ resultData.category }}</div>
                            <div class="tag-p">#Temelİhtiyaç</div>
                        </div>
                    </div>

                    <div class="ar-chart-mini mt-4">
                        <div class="chart-label">Aylık Gıda Bütçesi Etkisi</div>
                        <div class="chart-track">
                            <div class="chart-fill" :style="{ width: Math.min((resultData.total / 5000) * 100, 100) + '%' }"></div>
                        </div>
                        <div class="chart-meta">Bu fiş, gıda bütçenizin %{{ ((resultData.total / 5000) * 100).toFixed(1) }}'ini doldurdu.</div>
                    </div>
                </div>

                <button @click="saveExpense" class="btn-save-expense mt-5">
                    <CheckCircle2 :size="20" /> RAPORU ONAYLA VE KAYDET
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.receipt-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.ai-status-p { display: flex; align-items: center; gap: 8px; background: #0C1B33; color: var(--gold); padding: 8px 16px; border-radius: 100px; font-size: 0.75rem; font-weight: 800; border: 1.5px solid var(--gold); }

.receipt-grid { display: grid; grid-template-columns: 1fr 400px; gap: 24px; }

.upload-zone-p { height: 420px; border: 2px dashed #E2E8F0; border-radius: 24px; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; padding: 40px; background: rgba(255,255,255,0.3); transition: 0.3s; }
.uz-icon-wrap { display: flex; align-items: center; gap: 15px; margin-bottom: 25px; color: #CBD5E1; }
.uz-icon-plus { color: #E2E8F0; }
.uz-icon.gallery { transform: rotate(5deg); }

.upload-actions-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; width: 100%; max-width: 400px; }
.upload-action-btn { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 24px; border-radius: 20px; border: 1.5px solid #E2E8F0; background: white; cursor: pointer; transition: 0.2s; }
.upload-action-btn:hover { border-color: var(--primary); transform: translateY(-5px); box-shadow: 0 10px 20px rgba(0,0,0,0.05); }
.upload-action-btn svg { color: var(--primary); }
.upload-action-btn span { font-weight: 800; font-size: 0.9rem; color: var(--primary-dark); }

.drag-drop-hint { font-size: 0.75rem; color: #94A3B8; font-weight: 700; opacity: 0.6; }
.upload-zone-p:hover { border-color: var(--primary); background: white; }
.uz-icon { color: var(--primary); margin-bottom: 20px; opacity: 0.3; }
.uz-inner h3 { margin: 0; font-size: 1.2rem; color: var(--primary-dark); font-weight: 800; }
.uz-inner p { margin: 8px 0 0 0; font-size: 0.9rem; color: #64748B; font-weight: 600; }

.uz-tips { display: flex; gap: 20px; margin-top: 40px; }
.tip { display: flex; align-items: center; gap: 6px; font-size: 0.7rem; font-weight: 800; color: #94A3B8; }
.tip svg { color: #10B981; }

/* SCANNING */
.scanning-zone-p { text-align: center; }
.receipt-preview-p { position: relative; width: 100%; height: 350px; border-radius: 20px; overflow: hidden; background: #000; }
.receipt-preview-p img { width: 100%; height: 100%; object-fit: contain; }
.scan-line { position: absolute; left: 0; right: 0; height: 3px; background: var(--gold); box-shadow: 0 0 15px var(--gold); z-index: 2; transition: top 0.1s linear; }
.scan-overlay { position: absolute; inset: 0; background: linear-gradient(to bottom, transparent, rgba(197, 160, 89, 0.1)); pointer-events: none; }

.status-meta { display: flex; align-items: center; justify-content: center; gap: 12px; font-weight: 800; color: var(--primary-dark); }
.progress-bar-p { height: 8px; background: #F1F5F9; border-radius: 10px; margin-top: 15px; overflow: hidden; }
.pb-fill { height: 100%; background: var(--primary); transition: width 0.3s ease; }

/* RESULT PANEL */
.rp-h { display: flex; align-items: center; gap: 10px; font-weight: 900; font-size: 0.8rem; color: #94A3B8; letter-spacing: 1px; margin-bottom: 24px; }
.rp-empty { text-align: center; padding: 60px 0; color: #CBD5E1; }
.rpe-icon { margin-bottom: 20px; animation: bounceRight 2s infinite; }
@keyframes bounceRight { 0%, 100% { transform: translateX(0); } 50% { transform: translateX(10px); } }

.data-item label { display: block; font-size: 0.65rem; font-weight: 900; color: #94A3B8; margin-bottom: 8px; letter-spacing: 0.5px; }
.data-input { width: 100%; padding: 12px 16px; border-radius: 12px; border: 1.5px solid #F1F5F9; background: #F8FAFC; font-weight: 800; font-size: 0.9rem; color: var(--primary-dark); outline: none; }
.data-input:focus { border-color: var(--primary); background: white; }

.price-input-p { position: relative; }
.price-input-p span { position: absolute; right: 16px; top: 12px; font-weight: 900; color: #94A3B8; }

.data-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }

.data-summary { background: #0C1B33; color: white; padding: 20px; border-radius: 16px; display: flex; gap: 15px; align-items: center; transition: 0.3s; }
.data-input.readonly { opacity: 0.7; background: #F1F5F9; cursor: not-allowed; }

.analysis-report-p { background: #F8FAFC; border: 1.5px solid #E2E8F0; border-radius: 20px; padding: 20px; }
.ar-h { font-size: 0.65rem; font-weight: 900; color: #94A3B8; letter-spacing: 1.5px; border-bottom: 1px solid #E2E8F0; padding-bottom: 10px; text-align: center; }

.ar-metrics { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
.ar-metric { text-align: center; padding: 10px; background: white; border-radius: 12px; border: 1px solid #F1F5F9; }
.arm-val { font-size: 0.9rem; font-weight: 900; color: var(--primary-dark); }
.arm-label { font-size: 0.55rem; font-weight: 800; color: #94A3B8; margin-top: 4px; }

.ar-insight-box { padding: 16px; border-radius: 14px; background: #0C1B33; color: white; }
.aib-h { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; color: var(--gold); }
.ar-insight-box p { font-size: 0.75rem; line-height: 1.5; opacity: 0.8; margin: 0; }
.aib-footer { display: flex; gap: 8px; }
.tag-p { font-size: 0.6rem; font-weight: 800; background: rgba(255,255,255,0.1); padding: 2px 8px; border-radius: 4px; }

.ar-chart-mini { border-top: 1px solid #E2E8F0; padding-top: 15px; }
.chart-label { font-size: 0.65rem; font-weight: 800; color: #64748B; margin-bottom: 8px; }
.chart-track { height: 6px; background: #E2E8F0; border-radius: 10px; overflow: hidden; }
.chart-fill { height: 100%; background: var(--primary); transition: width 1s ease; }
.chart-meta { font-size: 0.6rem; color: #94A3B8; font-weight: 700; margin-top: 8px; }

.data-summary.success { background: rgba(16, 185, 129, 0.1); color: #10B981; border: 1.5px solid #10B981; }
.data-summary.warning { background: rgba(245, 158, 11, 0.1); color: #F59E0B; border: 1.5px solid #F59E0B; }
.data-summary.danger { background: rgba(239, 68, 68, 0.1); color: #EF4444; border: 1.5px solid #EF4444; }
.data-summary.success .ds-icon, .data-summary.warning .ds-icon, .data-summary.danger .ds-icon { color: currentColor; }
.ds-icon { color: var(--gold); }
.ds-text strong { display: block; font-size: 0.9rem; color: var(--gold); }
.data-summary.success strong, .data-summary.warning strong, .data-summary.danger strong { color: currentColor; }
.ds-text p { margin: 2px 0 0 0; font-size: 0.75rem; opacity: 0.8; font-weight: 600; }

.btn-save-expense { width: 100%; padding: 16px; background: #10B981; color: white; border: none; border-radius: 16px; font-weight: 900; font-size: 1rem; cursor: pointer; transition: 0.3s; display: flex; align-items: center; justify-content: center; gap: 12px; }
.btn-save-expense:hover { background: #059669; transform: translateY(-4px); box-shadow: 0 10px 20px rgba(16, 185, 129, 0.2); }

.rp-image-small { width: 100%; height: 200px; border-radius: 16px; overflow: hidden; border: 1px solid #E2E8F0; }
.rp-image-small img { width: 100%; height: 100%; object-fit: cover; }

.camera-zone-p { position: relative; height: 420px; background: #000; border-radius: 24px; overflow: hidden; }
.camera-video { width: 100%; height: 100%; object-fit: cover; }
.camera-controls { position: absolute; bottom: 30px; left: 0; right: 0; display: flex; align-items: center; justify-content: center; gap: 30px; }
.btn-cam-capture { width: 70px; height: 70px; border-radius: 50%; background: white; border: 5px solid rgba(255,255,255,0.3); cursor: pointer; padding: 4px; }
.btn-cam-capture .inner-circle { width: 100%; height: 100%; border-radius: 50%; border: 2px solid #000; background: white; }
.btn-cam-cancel { width: 44px; height: 44px; border-radius: 50%; background: rgba(0,0,0,0.5); color: white; border: none; cursor: pointer; }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
