<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, computed } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';
import { useToastStore } from '../store/toast';
import { 
  Send, User, Headset, MessageSquare, 
  Search, Phone, Mail,
  Info, CheckCircle2, 
  Clock, Loader2
} from 'lucide-vue-next';

const authStore = useAuthStore();
const toast = useToastStore();

const customers = ref<any[]>([]);
const chatMessages = ref<any[]>([]);
const newMessage = ref('');
const activeCustomer = ref<any>(null);
const loading = ref(true);
const chatContainer = ref<HTMLElement | null>(null);
const searchQuery = ref('');
const isSending = ref(false);
let pollInterval: any = null;

const faqs = [
    { q: 'Yalova Şubesi çalışma saatleri nedir?', a: 'Şubemiz hafta içi 09:00 - 17:00 saatleri arasında hizmet vermektedir.' },
    { q: 'Kredi kartı limitimi nasıl artırabilirim?', a: 'Kart Yönetimi sekmesinden veya şubemizden limit artış talebinde bulunabilirsiniz.' },
    { q: 'Döviz işlemlerinde komisyon alınıyor mu?', a: 'NexBank Elite üyeleri için döviz takas işlemleri tamamen komisyonsuzdur.' }
];

const filteredCustomers = computed(() => {
    if (!searchQuery.value) return customers.value;
    return customers.value.filter(c => c.fullName.toLowerCase().includes(searchQuery.value.toLowerCase()));
});

const fetchCustomers = async () => {
    if (!authStore.isAdmin) return;
    try {
        const res = await apiClient.get('/Users/customers');
        customers.value = res.data;
    } catch (err: any) { 
        console.error('Müşteri listesi yüklenemedi.');
    }
};

const fetchChatHistory = async (targetUserId: number) => {
    try {
        const res = await apiClient.get(`/Chat/history/${targetUserId}`);
        const oldCount = chatMessages.value.length;
        chatMessages.value = res.data;
        if (chatMessages.value.length > oldCount) {
            scrollToBottom();
        }
    } catch (err) { console.error('Geçmiş yüklenemedi', err); }
};

const scrollToBottom = () => {
    nextTick(() => {
        if (chatContainer.value) {
            chatContainer.value.scrollTop = chatContainer.value.scrollHeight;
        }
    });
};

const selectCustomer = async (customer: any) => {
    activeCustomer.value = customer;
    await fetchChatHistory(customer.id);
    scrollToBottom();
};

const sendMessage = async () => {
    if (!newMessage.value.trim() || isSending.value) return;
    
    isSending.value = true;
    const msgText = newMessage.value;
    
    try {
        const receiverId = authStore.isAdmin 
            ? (activeCustomer.value?.id || 0)
            : 0; // Müşteri mesajı şubeye gidiyor

        await apiClient.post('/Chat/send', {
            receiverId: receiverId,
            message: msgText
        });
        
        // Mesajı yerel olarak ekle
        chatMessages.value.push({ 
            senderId: authStore.user?.id || authStore.user?.Id, 
            content: msgText, 
            sentAt: new Date().toISOString() 
        });
        newMessage.value = '';
        scrollToBottom();
    } catch (err: any) {
        console.error('Mesaj gönderim hatası:', err);
        toast.error('Mesaj gönderilemedi: ' + (err.response?.data?.message || err.message));
    } finally {
        isSending.value = false;
    }
};

// Polling ile yeni mesajları kontrol et
const startPolling = () => {
    pollInterval = setInterval(async () => {
        if (authStore.isAdmin && activeCustomer.value) {
            await fetchChatHistory(activeCustomer.value.id);
        } else if (!authStore.isAdmin) {
            const myId = authStore.user?.id || authStore.user?.Id;
            if (myId) await fetchChatHistory(myId);
        }
    }, 3000); // 3 saniyede bir kontrol
};

onMounted(async () => {
    loading.value = true;
    if (authStore.isAdmin) {
        await fetchCustomers();
    } else {
        const myId = authStore.user?.id || authStore.user?.Id;
        if (myId) {
            activeCustomer.value = { fullName: 'Yalova Şubesi Temsilcisi', id: myId };
            await fetchChatHistory(myId);
        }
    }
    loading.value = false;
    startPolling();
});

onUnmounted(() => {
    if (pollInterval) clearInterval(pollInterval);
});
</script>

<template>
  <div class="view-container fade-in">
    <header class="support-header-v5 mb-4">
      <div class="h-left">
        <h1 class="text-gradient-gold">Destek Merkezi</h1>
        <p class="subtitle">{{ authStore.isAdmin ? 'Müşterilerinizle anlık iletişim kurun.' : 'Yalova Şubesi personeli ile iletişimdesiniz.' }}</p>
      </div>
      <div class="h-right">
        <div class="connection-status-pill active">
            <div class="pulse-dot"></div>
            <span>AKTİF</span>
        </div>
      </div>
    </header>

    <div class="support-grid-v5" :class="{ 'is-staff': authStore.isAdmin }">
        <!-- CHAT TERMINAL -->
        <div class="chat-terminal-v5 glass-card-elite">
            <div class="terminal-layout">
                <!-- STAFF SIDEBAR -->
                <aside v-if="authStore.isAdmin" class="staff-sidebar">
                    <div class="ss-header">
                        <MessageSquare :size="16" />
                        <span>MÜŞTERİLER</span>
                    </div>
                    <div class="ss-search">
                        <Search :size="14" />
                        <input type="text" v-model="searchQuery" placeholder="Müşteri ara..." />
                    </div>
                    <div class="ss-list">
                        <div v-for="c in filteredCustomers" :key="c.id" 
                             @click="selectCustomer(c)"
                             :class="['ss-item', { active: activeCustomer?.id === c.id }]">
                            <div class="ss-avatar">{{ c.fullName.charAt(0) }}</div>
                            <div class="ss-meta">
                                <strong>{{ c.fullName }}</strong>
                                <small>{{ c.branch || 'Yalova Şubesi' }}</small>
                            </div>
                        </div>
                    </div>
                </aside>

                <!-- MAIN CHAT AREA -->
                <main class="chat-main">
                    <div v-if="authStore.isAdmin && !activeCustomer" class="chat-placeholder">
                        <Headset :size="48" class="text-muted mb-3" />
                        <h3>NexBank Destek Paneli</h3>
                        <p>Lütfen görüşmeye başlamak için bir müşteri seçin.</p>
                    </div>

                    <div v-else class="chat-active-v5">
                        <header class="chat-header-v5">
                            <div class="ch-user">
                                <div class="ch-avatar">{{ activeCustomer?.fullName?.charAt(0) }}</div>
                                <div class="ch-info">
                                    <strong>{{ activeCustomer?.fullName }}</strong>
                                    <span class="ch-status"><span class="dot"></span> Çevrimiçi</span>
                                </div>
                            </div>
                        </header>

                        <div class="chat-messages-v5" ref="chatContainer">
                            <div v-for="(m, i) in chatMessages" :key="i" 
                                 :class="['msg-v5', m.senderId === (authStore.user?.id || authStore.user?.Id) ? 'sent' : 'received']">
                                <div class="msg-bubble-v5">
                                    {{ m.content }}
                                    <span class="msg-time-v5">{{ new Date(m.sentAt).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'}) }}</span>
                                </div>
                            </div>
                            <div v-if="chatMessages.length === 0" class="chat-start-hint">
                                {{ authStore.isAdmin ? 'Müşteriye mesaj gönderin.' : 'Yalova şubesi temsilcimiz size yardımcı olmak için hazır. Mesajınızı yazın.' }}
                            </div>
                        </div>

                        <footer class="chat-input-v5">
                            <div class="input-container-v5">
                                <input v-model="newMessage" @keyup.enter="sendMessage" 
                                       placeholder="Mesajınızı buraya yazın..." 
                                       :disabled="isSending" />
                                <button @click="sendMessage" class="btn-send-v5" :disabled="!newMessage.trim() || isSending">
                                    <Loader2 v-if="isSending" :size="18" class="spin" />
                                    <Send v-else :size="18" />
                                </button>
                            </div>
                        </footer>
                    </div>
                </main>
            </div>
        </div>

        <!-- INFO SIDEBAR (FOR CUSTOMERS) -->
        <div v-if="!authStore.isAdmin" class="info-sidebar-v5">
            <div class="glass-card-elite info-box-v5">
                <div class="ib-header"><Info :size="16" /> <span>HIZLI YARDIM</span></div>
                <div class="ib-faqs mt-3">
                    <details v-for="(f, i) in faqs" :key="i" class="faq-row-v5">
                        <summary>{{ f.q }}</summary>
                        <p>{{ f.a }}</p>
                    </details>
                </div>
            </div>

            <div class="glass-card-elite info-box-v5 mt-3">
                <div class="ib-header"><Phone :size="16" /> <span>DİĞER KANALLAR</span></div>
                <div class="contact-rows mt-3">
                    <div class="cr-item">
                        <div class="cr-icon"><Phone :size="14" /></div>
                        <div><strong>0850 222 0 444</strong><small>Müşteri Merkezi</small></div>
                    </div>
                    <div class="cr-item">
                        <div class="cr-icon"><Mail :size="14" /></div>
                        <div><strong>yalova@nexbank.com</strong><small>Şube E-Posta</small></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.support-header-v5 { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2rem; font-weight: 900;
}

.connection-status-pill { 
    background: #F1F5F9; padding: 8px 16px; border-radius: 100px; 
    display: flex; align-items: center; gap: 8px; font-size: 0.7rem; 
    font-weight: 800; color: #64748B; transition: 0.3s;
}
.connection-status-pill.active { background: rgba(16, 185, 129, 0.1); color: #10B981; }

.pulse-dot { width: 8px; height: 8px; background: currentColor; border-radius: 50%; animation: pulse 2s infinite; }
@keyframes pulse { 0% { transform: scale(1); opacity: 1; } 50% { transform: scale(1.5); opacity: 0.5; } 100% { transform: scale(1); opacity: 1; } }

.support-grid-v5 { display: grid; grid-template-columns: 1fr 300px; gap: 24px; height: 600px; }
.is-staff { grid-template-columns: 1fr; }

.chat-terminal-v5 { padding: 0; overflow: hidden; border: 1px solid #F1F5F9; }
.terminal-layout { display: flex; height: 100%; }

/* STAFF SIDEBAR */
.staff-sidebar { width: 280px; background: #F8FAFC; border-right: 1px solid #F1F5F9; display: flex; flex-direction: column; }
.ss-header { padding: 20px; font-weight: 900; font-size: 0.75rem; color: #0F172A; display: flex; align-items: center; gap: 8px; border-bottom: 1px solid #F1F5F9; }
.ss-search { padding: 12px 20px; position: relative; }
.ss-search svg { position: absolute; left: 32px; top: 22px; color: #94A3B8; }
.ss-search input { width: 100%; padding: 8px 12px 8px 32px; border-radius: 10px; border: 1px solid #E2E8F0; outline: none; font-size: 0.8rem; }

.ss-list { flex: 1; overflow-y: auto; }
.ss-item { padding: 16px 20px; display: flex; align-items: center; gap: 12px; cursor: pointer; transition: 0.2s; border-bottom: 1px solid #F1F5F9; }
.ss-item:hover { background: white; }
.ss-item.active { background: white; border-left: 4px solid #D4AF37; }
.ss-avatar { width: 36px; height: 36px; background: #0F172A; color: #D4AF37; border-radius: 8px; display: flex; align-items: center; justify-content: center; font-weight: 800; }
.ss-meta strong { display: block; font-size: 0.85rem; color: #0F172A; }
.ss-meta small { font-size: 0.7rem; color: #94A3B8; font-weight: 700; }

/* CHAT MAIN */
.chat-main { flex: 1; display: flex; flex-direction: column; background: white; }
.chat-placeholder { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #94A3B8; }

.chat-active-v5 { display: flex; flex-direction: column; height: 100%; }
.chat-header-v5 { padding: 16px 24px; border-bottom: 1px solid #F1F5F9; }
.ch-user { display: flex; align-items: center; gap: 12px; }
.ch-avatar { width: 40px; height: 40px; background: #0F172A; color: #D4AF37; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-weight: 900; }
.ch-info strong { display: block; font-size: 0.95rem; color: #0F172A; }
.ch-status { font-size: 0.7rem; font-weight: 800; color: #10B981; display: flex; align-items: center; gap: 6px; }
.ch-status .dot { width: 6px; height: 6px; background: currentColor; border-radius: 50%; }

.chat-messages-v5 { flex: 1; padding: 24px; overflow-y: auto; display: flex; flex-direction: column; gap: 12px; background: #F8FAFC; }
.msg-v5 { max-width: 80%; display: flex; flex-direction: column; }
.msg-v5.sent { align-self: flex-end; }
.msg-v5.received { align-self: flex-start; }

.msg-bubble-v5 { padding: 12px 18px; border-radius: 18px; font-size: 0.9rem; line-height: 1.5; position: relative; }
.sent .msg-bubble-v5 { background: #0F172A; color: white; border-bottom-right-radius: 4px; }
.received .msg-bubble-v5 { background: white; color: #0F172A; border: 1px solid #F1F5F9; border-bottom-left-radius: 4px; }
.msg-time-v5 { font-size: 0.65rem; display: block; margin-top: 4px; opacity: 0.5; text-align: right; }

.chat-start-hint { text-align: center; font-size: 0.75rem; color: #94A3B8; font-style: italic; margin-top: 20px; }

.chat-input-v5 { padding: 20px; border-top: 1px solid #F1F5F9; }
.input-container-v5 { display: flex; gap: 12px; background: #F1F5F9; padding: 8px 8px 8px 20px; border-radius: 100px; }
.input-container-v5 input { flex: 1; border: none; background: transparent; outline: none; font-size: 0.95rem; font-weight: 600; color: #0F172A; }
.btn-send-v5 { width: 42px; height: 42px; border-radius: 50%; border: none; background: #0F172A; color: #D4AF37; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: 0.2s; }
.btn-send-v5:hover:not(:disabled) { transform: scale(1.05); background: #000; }
.btn-send-v5:disabled { opacity: 0.4; cursor: not-allowed; }

/* INFO BOXES */
.info-box-v5 { padding: 20px; }
.ib-header { display: flex; align-items: center; gap: 8px; font-size: 0.7rem; font-weight: 900; color: #94A3B8; letter-spacing: 1px; }
.faq-row-v5 { margin-top: 12px; cursor: pointer; }
.faq-row-v5 summary { font-size: 0.85rem; font-weight: 700; color: #0F172A; margin-bottom: 6px; list-style: none; }
.faq-row-v5 p { font-size: 0.8rem; color: #64748B; line-height: 1.5; padding-left: 10px; border-left: 2px solid #D4AF37; }

.contact-rows { display: flex; flex-direction: column; gap: 16px; }
.cr-item { display: flex; align-items: center; gap: 12px; }
.cr-icon { width: 32px; height: 32px; background: #F8FAFC; border-radius: 8px; display: flex; align-items: center; justify-content: center; color: #0F172A; }
.cr-item strong { display: block; font-size: 0.85rem; color: #0F172A; }
.cr-item small { font-size: 0.7rem; color: #94A3B8; font-weight: 700; }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
