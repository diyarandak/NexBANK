<script setup lang="ts">
import { ref, onMounted, nextTick, computed } from 'vue';
import { 
  Bot, X, Send, Sparkles, 
  TrendingUp, Wallet, CreditCard,
  BrainCircuit, Loader2, Mic, 
  MessageSquarePlus, History, BarChart,
  ArrowRight, Check, Zap, Gift
} from 'lucide-vue-next';
import { useAuthStore } from '../store/auth';
import { useRouter } from 'vue-router';
import { useToastStore } from '../store/toast';

const authStore = useAuthStore();
const router = useRouter();
const toast = useToastStore();

const isOpen = ref(false);
const isTyping = ref(false);
const newMessage = ref('');
const chatContainer = ref<HTMLElement | null>(null);
const lastContext = ref<string | null>(null);

// Akıllı Aksiyon Kartları (Zeki Asistanın sunduğu seçenekler)
const quickActions = ref<any[]>([]);

const messages = ref([
  { 
    id: 1, 
    type: 'bot', 
    content: `Selam ${authStore.user?.fullName?.split(' ')[0]}! Ben NexOmni AI. Bugün sadece asistanın değil, finansal stratejistinim. Neyi analiz edelim?`,
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    actions: ['Harcama Analizi', 'Piyasa Durumu', 'Hızlı EFT']
  }
]);

const scrollToBottom = () => {
  nextTick(() => {
    if (chatContainer.value) chatContainer.value.scrollTop = chatContainer.value.scrollHeight;
  });
};

const handleSend = async (textOverride?: string) => {
  const text = textOverride || newMessage.value;
  if (!text.trim()) return;

  messages.value.push({
    id: Date.now(),
    type: 'user',
    content: text,
    time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  });

  newMessage.value = '';
  isTyping.value = true;
  quickActions.value = [];
  scrollToBottom();

  // AI MANTIK MOTORU (NexOmni Engine)
  setTimeout(() => {
    let response = "";
    let actions: string[] = [];
    const input = text.toLowerCase().trim();

    if (input.includes("harcama") || input.includes("analiz")) {
        response = "Son 30 günlük verilerini taradım. Market harcamaların %12 artarken, eğlence harcamaların %5 azalmış. Toplamda 4.500 TL tasarruf potansiyelin var. Tasarruf hesabına aktaralım mı?";
        actions = ['Tasarruf Yap', 'Detaylı Rapor'];
    } else if (input.includes("borsa") || input.includes("yatırım") || input.includes("hisse")) {
        response = "BIST100 bugün %1.2 yukarıda. ASELS ve THYAO güçlü seyrediyor. Portföyündeki toplam kazancın bugün 1.250 TL. Yeni bir alım emri girmek ister misin?";
        actions = ['Hisse Al', 'Portföyüm'];
    } else if (input.includes("para") || input.includes("gönder") || input.includes("eft")) {
        response = "Tabii ki! En çok gönderim yaptığın kişileri listeledim. Kime gönderiyoruz?";
        actions = ['Ahmet Yılmaz', 'Mehmet Kaya', 'Yeni Alıcı'];
    } else if (input.includes("nakit") || input.includes("avans")) {
        response = "Acil nakit mi lazım? Kredi kartından 12 taksitle 50.000 TL'ye kadar nakit avans çekebilirsin. Faiz oranı aylık %1.89. Hemen onaylayalım mı?";
        actions = ['Onayla', 'Limitimi Gör'];
    } else if (input.includes("selam") || input.includes("naber") || input.includes("nasılsın")) {
        response = "Harikayım! Senin finansal sağlığın yerinde olduğu sürece ben de çok iyiyim. Bugün bütçeni bir adım öteye taşıyalım mı?";
        actions = ['Hadi Başlayalım', 'Plan Yap'];
    } else if (input.includes("slm") || input.includes("mrb")) {
        response = "Selam! NexOmni AI emrinde. Para transferi, bakiye sorgulama veya yatırım danışmanlığı... Ne istersin?";
    } else if (input.includes("salak") || input.includes("aptal")) {
        response = "Eleştirin için teşekkürler. Kendimi geliştirmem için bana bir şans ver; mesela sana bugün nasıl 500 TL tasarruf ettirebileceğimi anlatayım?";
        actions = ['Nasıl?', 'Özür Dilerim'];
    } else {
        response = "Bu konuda derin bir analiz yapmamı ister misin? Yoksa doğrudan ilgili menüye mi gidelim?";
        actions = ['Analiz Et', 'Menüye Git'];
    }

    messages.value.push({
      id: Date.now() + 1,
      type: 'bot',
      content: response,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      actions: actions
    });

    isTyping.value = false;
    scrollToBottom();
  }, 1200);
};

const handleAction = (action: string) => {
    if (action === 'Harcama Analizi') handleSend("Harcamalarımı analiz et");
    else if (action === 'Piyasa Durumu') handleSend("Borsa ne durumda?");
    else if (action === 'Yeni Alıcı') router.push('/transfer');
    else {
        toast.info(`${action} işlemi başlatılıyor...`);
        handleSend(action);
    }
};
</script>

<template>
  <div class="nexomni-container">
    <!-- TOGGLE BUTTON -->
    <button @click="isOpen = !isOpen" class="nexomni-toggle" :class="{ 'btn-open': isOpen }">
      <X v-if="isOpen" :size="24" />
      <div v-else class="bot-icon-p">
        <div class="bot-ring"></div>
        <BrainCircuit :size="30" />
        <div class="bot-status-p"></div>
      </div>
    </button>

    <!-- CHAT WINDOW -->
    <div v-if="isOpen" class="nexomni-window glass-card-v6 slide-up-p">
      <header class="nexomni-h">
        <div class="h-meta">
            <div class="h-icon-box">
                <Zap :size="20" class="text-gold pulse" />
            </div>
            <div class="h-text">
                <h3>NexOmni Intelligence</h3>
                <div class="status-wrap">
                    <div class="dot green"></div>
                    <span>Diyar'ın Finansal Stratejisti</span>
                </div>
            </div>
        </div>
        <div class="h-actions">
            <button class="h-btn"><History :size="16" /></button>
            <button class="h-btn"><BarChart :size="16" /></button>
        </div>
      </header>

      <main class="nexomni-b" ref="chatContainer">
        <div v-for="m in messages" :key="m.id" :class="['m-wrap-p', m.type]">
            <div v-if="m.type === 'bot'" class="m-avatar-p">
                <BrainCircuit :size="14" />
            </div>
            <div class="m-content-wrap">
                <div class="m-bubble-p">
                    {{ m.content }}
                    <span class="m-time-p">{{ m.time }}</span>
                </div>
                <!-- AKILLI BUTONLAR -->
                <div v-if="m.actions && m.actions.length > 0" class="m-actions-p">
                    <button v-for="act in m.actions" :key="act" @click="handleAction(act)" class="action-btn-v6">
                        {{ act }} <ChevronRight :size="12" />
                    </button>
                </div>
            </div>
        </div>
        
        <div v-if="isTyping" class="m-wrap-p bot">
            <div class="m-avatar-p"><Loader2 :size="14" class="spin" /></div>
            <div class="m-bubble-p typing">
                <div class="typing-indicator"><span></span><span></span><span></span></div>
            </div>
        </div>
      </main>

      <footer class="nexomni-f">
        <div class="f-inner">
            <button class="btn-voice"><Mic :size="20" /></button>
            <div class="f-input-wrap">
                <input v-model="newMessage" @keyup.enter="handleSend()" placeholder="NexOmni'ye bir emir ver..." />
                <button @click="handleSend()" class="btn-send-v6" :disabled="isTyping">
                    <Send :size="18" />
                </button>
            </div>
        </div>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.nexomni-container { position: fixed; bottom: 30px; right: 30px; z-index: 10000; }

.nexomni-toggle { width: 70px; height: 70px; border-radius: 50%; border: none; background: #0C1B33; color: var(--gold); box-shadow: 0 15px 40px rgba(0,0,0,0.4); cursor: pointer; display: flex; align-items: center; justify-content: center; position: relative; transition: 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275); }
.nexomni-toggle:hover { transform: scale(1.1); background: #1e293b; }
.btn-open { background: #F44336 !important; color: white !important; }

.bot-ring { position: absolute; inset: -4px; border: 2px solid var(--gold); border-radius: 50%; animation: orbit 4s linear infinite; opacity: 0.3; }
@keyframes orbit { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

.nexomni-window { position: absolute; bottom: 90px; right: 0; width: 420px; height: 600px; background: white; border-radius: 30px; display: flex; flex-direction: column; overflow: hidden; box-shadow: 0 30px 100px rgba(0,0,0,0.3); border: 1px solid rgba(0,0,0,0.05); }

.nexomni-h { padding: 24px 30px; background: #0C1B33; color: white; display: flex; justify-content: space-between; align-items: center; }
.h-meta { display: flex; align-items: center; gap: 15px; }
.h-icon-box { width: 44px; height: 44px; background: rgba(255,255,255,0.05); border-radius: 14px; display: flex; align-items: center; justify-content: center; border: 1px solid rgba(197, 160, 89, 0.2); }
.h-text h3 { margin: 0; font-size: 1.1rem; font-weight: 800; font-family: 'Outfit'; color: var(--gold); }
.status-wrap { display: flex; align-items: center; gap: 6px; margin-top: 4px; }
.dot { width: 6px; height: 6px; border-radius: 50%; }
.dot.green { background: #10B981; box-shadow: 0 0 10px #10B981; }
.status-wrap span { font-size: 0.65rem; font-weight: 700; opacity: 0.6; text-transform: uppercase; letter-spacing: 1px; }

.h-btn { background: rgba(255,255,255,0.05); border: none; color: white; width: 32px; height: 32px; border-radius: 8px; cursor: pointer; transition: 0.2s; }
.h-btn:hover { background: rgba(255,255,255,0.15); }

.nexomni-b { flex: 1; padding: 25px; overflow-y: auto; background: #F8FAFC; display: flex; flex-direction: column; gap: 20px; }

.m-wrap-p { display: flex; align-items: flex-start; gap: 12px; max-width: 90%; }
.m-wrap-p.user { align-self: flex-end; flex-direction: row-reverse; }

.m-avatar-p { width: 32px; height: 32px; background: #0C1B33; color: var(--gold); border-radius: 10px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }

.m-bubble-p { padding: 15px 20px; border-radius: 20px; font-size: 0.9rem; line-height: 1.5; font-weight: 600; position: relative; }
.user .m-bubble-p { background: #0C1B33; color: white; border-bottom-right-radius: 2px; }
.bot .m-bubble-p { background: white; color: #1E293B; border: 1px solid #E2E8F0; border-bottom-left-radius: 2px; box-shadow: 0 5px 15px rgba(0,0,0,0.03); }

.m-time-p { font-size: 0.6rem; opacity: 0.4; margin-top: 8px; display: block; text-align: right; }

.m-actions-p { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; }
.action-btn-v6 { background: white; border: 1.5px solid #E2E8F0; padding: 8px 16px; border-radius: 100px; font-size: 0.75rem; font-weight: 800; color: #0C1B33; cursor: pointer; transition: 0.2s; display: flex; align-items: center; gap: 6px; }
.action-btn-v6:hover { border-color: var(--gold); background: #0C1B33; color: var(--gold); transform: translateY(-2px); }

.nexomni-f { padding: 20px 25px; background: white; border-top: 1px solid #F1F5F9; }
.f-inner { display: flex; align-items: center; gap: 15px; }
.f-input-wrap { flex: 1; display: flex; align-items: center; background: #F1F5F9; border-radius: 100px; padding: 6px 6px 6px 20px; }
.f-input-wrap input { flex: 1; border: none; background: transparent; outline: none; font-weight: 700; font-size: 0.9rem; }
.btn-voice { width: 44px; height: 44px; border-radius: 50%; border: 1.5px solid #E2E8F0; background: white; color: #64748B; cursor: pointer; transition: 0.2s; }
.btn-voice:hover { color: var(--primary); border-color: var(--primary); background: #F1F5F9; }
.btn-send-v6 { width: 40px; height: 40px; border-radius: 50%; border: none; background: #0C1B33; color: white; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.btn-send-v6:hover:not(:disabled) { background: var(--gold); color: #0C1B33; transform: scale(1.1); }

.typing-indicator { display: flex; gap: 4px; }
.typing-indicator span { width: 6px; height: 6px; background: #94A3B8; border-radius: 50%; animation: blink 1.4s infinite both; }
.typing-indicator span:nth-child(2) { animation-delay: 0.2s; }
.typing-indicator span:nth-child(3) { animation-delay: 0.4s; }

.pulse { animation: pulse 2s infinite; }
@keyframes pulse { 0% { transform: scale(1); } 50% { transform: scale(1.2); } 100% { transform: scale(1); } }
</style>
