<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { 
  ShieldCheck, Smartphone, Globe, Lock, 
  MapPin, Clock, AlertTriangle, ShieldAlert,
  ChevronRight, Fingerprint, Eye, EyeOff,
  BellRing, Cpu, RotateCcw, Power
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const showSensibleData = ref(false);
const securityScore = ref(84);

const sessions = ref([
  { id: 1, device: 'MacBook Pro 14"', location: 'İstanbul, TR', time: 'Şu an aktif', ip: '192.168.1.45', current: true },
  { id: 2, device: 'iPhone 15 Pro', location: 'Ankara, TR', time: '2 saat önce', ip: '176.234.5.12', current: false },
  { id: 3, device: 'Chrome Windows', location: 'Londra, UK', time: 'Dün', ip: '82.145.67.8', current: false }
]);

const securitySettings = ref([
  { id: 'bio', title: 'Biyometrik Giriş', desc: 'FaceID veya Parmak izi ile giriş.', active: true, icon: Fingerprint },
  { id: '2fa', title: 'İki Faktörlü Doğrulama', desc: 'SMS veya Authenticator desteği.', active: true, icon: ShieldCheck },
  { id: 'geo', title: 'Konum Bazlı Güvenlik', desc: 'Sadece bulunduğunuz şehirde işlem yapın.', active: false, icon: MapPin },
  { id: 'freeze', title: 'Hesabı Geçici Dondur', desc: 'Kaybolma/Çalınma durumunda tek tıkla kilitle.', active: false, icon: Power }
]);

const toggleSetting = (id: string) => {
    const setting = securitySettings.value.find(s => s.id === id);
    if (setting) {
        setting.active = !setting.active;
        toast.success(`${setting.title} ayarı güncellendi.`);
    }
};

const terminateSession = (id: number) => {
    sessions.value = sessions.value.filter(s => s.id !== id);
    toast.warning("Oturum sonlandırıldı.");
};
</script>

<template>
  <div class="view-container fade-in">
    <header class="security-header">
      <div class="welcome-text">
        <h1 class="view-title text-gradient">Siber Güvenlik Merkezi</h1>
        <p class="subtitle">NexBank dijital kalesindesiniz. Varlıklarınız en üst düzey protokollerle korunmaktadır.</p>
      </div>
      <div class="security-badge-p" :class="{ 'high': securityScore > 80 }">
          <div class="score-circle">
              <svg viewBox="0 0 36 36" class="circular-chart">
                <path class="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                <path class="circle" :style="{ strokeDasharray: securityScore + ', 100' }" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
              </svg>
              <div class="score-text">
                  <strong>{{ securityScore }}</strong>
                  <span>PUAN</span>
              </div>
          </div>
          <div class="score-label">Güvenlik Durumu: <strong>MÜKEMMEL</strong></div>
      </div>
    </header>

    <div class="security-grid-p">
        <!-- 🛡️ CORE SETTINGS -->
        <div class="glass-card-v5 settings-panel">
            <div class="p-header">
                <Lock :size="20" />
                <h3>Güvenlik Kontrolleri</h3>
            </div>
            <div class="settings-list-p mt-4">
                <div v-for="s in securitySettings" :key="s.id" class="setting-item-p" :class="{ 'danger': s.id === 'freeze' }">
                    <div class="si-left">
                        <div class="si-icon"><component :is="s.icon" :size="22" /></div>
                        <div class="si-info">
                            <strong>{{ s.title }}</strong>
                            <p>{{ s.desc }}</p>
                        </div>
                    </div>
                    <div class="si-right">
                        <label class="switch-p">
                            <input type="checkbox" :checked="s.active" @change="toggleSetting(s.id)">
                            <span class="slider-p"></span>
                        </label>
                    </div>
                </div>
            </div>

            <div class="security-tip mt-5">
                <ShieldAlert :size="24" />
                <div>
                    <strong>Siber Güvenlik İpucu</strong>
                    <p>NexBank asla sizden şifrenizi e-posta veya SMS yoluyla istemez.</p>
                </div>
            </div>
        </div>

        <!-- 📱 ACTIVE SESSIONS -->
        <div class="sessions-panel">
            <div class="glass-card-v5 s-card">
                <div class="p-header">
                    <Smartphone :size="20" />
                    <h3>Aktif Oturumlar</h3>
                    <span class="badge-count">{{ sessions.length }} Cihaz</span>
                </div>
                <div class="session-list mt-4">
                    <div v-for="sess in sessions" :key="sess.id" class="session-item">
                        <div class="sess-info">
                            <div class="sess-icon" :class="{ current: sess.current }">
                                <component :is="sess.device.includes('iPhone') ? Smartphone : Cpu" :size="18" />
                            </div>
                            <div class="sess-text">
                                <strong>{{ sess.device }}</strong>
                                <small>{{ sess.location }} • {{ sess.ip }}</small>
                                <span class="sess-time">{{ sess.time }}</span>
                            </div>
                        </div>
                        <button v-if="!sess.current" @click="terminateSession(sess.id)" class="btn-terminate">Sonlandır</button>
                    </div>
                </div>
                <button class="btn-terminate-all mt-4">Tüm Diğer Oturumları Kapat</button>
            </div>

            <div class="glass-card-v5 alert-card mt-4">
                <div class="p-header">
                    <BellRing :size="20" />
                    <h3>Son Uyarılar</h3>
                </div>
                <div class="alert-list-mini mt-3">
                    <div class="alert-mini-item">
                        <MapPin :size="14" />
                        <span>Yeni bir konumdan (Londra) giriş denemesi engellendi.</span>
                        <small>2s önce</small>
                    </div>
                    <div class="alert-mini-item">
                        <Lock :size="14" />
                        <span>Şifreniz 45 gün önce değiştirildi.</span>
                        <small>Dün</small>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- 📊 PRIVACY & DATA SECTION -->
    <div class="privacy-section-p mt-5">
        <div class="glass-card-v5 privacy-card">
            <div class="pc-left">
                <div class="pc-icon-box"><EyeOff :size="32" /></div>
                <div class="pc-text">
                    <h3>Gizlilik Modu</h3>
                    <p>Bakiye ve kart bilgilerinizi ana ekranda gizlemek ister misiniz?</p>
                </div>
            </div>
            <div class="pc-right">
                <button @click="showSensibleData = !showSensibleData" class="btn-privacy-toggle" :class="{ active: showSensibleData }">
                    <component :is="showSensibleData ? EyeOff : Eye" :size="20" />
                    {{ showSensibleData ? 'Hassas Verileri Gizle' : 'Verileri Göster' }}
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.security-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2.5rem; }

/* CIRCULAR SCORE */
.security-badge-p { display: flex; align-items: center; gap: 20px; }
.score-circle { position: relative; width: 70px; height: 70px; }
.circular-chart { display: block; margin: 10px auto; max-width: 100%; max-height: 100%; }
.circle-bg { fill: none; stroke: #eee; stroke-width: 3.8; }
.circle { fill: none; stroke-width: 2.8; stroke-linecap: round; transition: stroke-dasharray 1s ease; stroke: #F44336; }
.high .circle { stroke: #10B981; }

.score-text { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; }
.score-text strong { font-size: 1.2rem; line-height: 1; color: var(--primary-dark); }
.score-text span { font-size: 0.5rem; font-weight: 800; opacity: 0.5; }
.score-label { font-size: 0.8rem; font-weight: 700; color: #64748B; }
.score-label strong { color: #10B981; }

.security-grid-p { display: grid; grid-template-columns: 1fr 400px; gap: 30px; }

.p-header { display: flex; align-items: center; gap: 12px; margin-bottom: 15px; }
.p-header h3 { margin: 0; font-size: 1.1rem; color: var(--primary-dark); font-weight: 800; }
.badge-count { background: #F1F5F9; font-size: 0.7rem; font-weight: 800; padding: 4px 10px; border-radius: 100px; color: #64748B; margin-left: auto; }

/* SETTINGS */
.setting-item-p { display: flex; justify-content: space-between; align-items: center; padding: 20px; background: #F8FAFC; border-radius: 16px; margin-bottom: 12px; transition: 0.2s; }
.setting-item-p:hover { background: white; box-shadow: 0 4px 15px rgba(0,0,0,0.05); }
.si-left { display: flex; gap: 16px; align-items: center; }
.si-icon { width: 44px; height: 44px; background: white; color: var(--primary); border-radius: 12px; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }
.si-info strong { display: block; font-size: 0.95rem; color: var(--primary-dark); }
.si-info p { font-size: 0.8rem; color: #64748B; margin: 4px 0 0 0; font-weight: 600; }

.setting-item-p.danger .si-icon { color: #F44336; }

/* SESSIONS */
.session-item { padding: 16px; border-bottom: 1px solid #F1F5F9; display: flex; justify-content: space-between; align-items: center; }
.sess-info { display: flex; gap: 14px; align-items: center; }
.sess-icon { width: 36px; height: 36px; background: #F1F5F9; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: #94A3B8; }
.sess-icon.current { background: rgba(16, 185, 129, 0.1); color: #10B981; }
.sess-text strong { display: block; font-size: 0.85rem; color: var(--primary-dark); }
.sess-text small { display: block; font-size: 0.7rem; color: #94A3B8; font-weight: 600; margin-top: 2px; }
.sess-time { font-size: 0.65rem; font-weight: 800; color: #10B981; margin-top: 4px; display: block; }

.btn-terminate { background: #FEE2E2; color: #DC2626; border: none; padding: 6px 12px; border-radius: 8px; font-size: 0.7rem; font-weight: 800; cursor: pointer; transition: 0.2s; }
.btn-terminate:hover { background: #DC2626; color: white; }

.btn-terminate-all { width: 100%; padding: 14px; background: #0C1B33; color: white; border: none; border-radius: 12px; font-weight: 800; font-size: 0.9rem; cursor: pointer; }

/* SWITCH */
.switch-p { position: relative; display: inline-block; width: 44px; height: 24px; }
.switch-p input { opacity: 0; width: 0; height: 0; }
.slider-p { position: absolute; cursor: pointer; inset: 0; background-color: #E2E8F0; transition: .4s; border-radius: 34px; }
.slider-p:before { position: absolute; content: ""; height: 18px; width: 18px; left: 3px; bottom: 3px; background-color: white; transition: .4s; border-radius: 50%; }
input:checked + .slider-p { background-color: #10B981; }
input:checked + .slider-p:before { transform: translateX(20px); }

.security-tip { background: #0C1B33; color: white; padding: 24px; border-radius: 20px; display: flex; gap: 20px; align-items: center; }
.security-tip svg { color: var(--gold); }
.security-tip strong { display: block; font-size: 1rem; color: var(--gold); }
.security-tip p { margin: 4px 0 0 0; font-size: 0.85rem; opacity: 0.8; line-height: 1.5; }

/* PRIVACY SECTION */
.privacy-card { display: flex; justify-content: space-between; align-items: center; padding: 32px; border: 1.5px solid rgba(0,0,0,0.03); }
.pc-left { display: flex; gap: 24px; align-items: center; }
.pc-icon-box { width: 64px; height: 64px; background: #F8FAFC; color: var(--primary); border-radius: 20px; display: flex; align-items: center; justify-content: center; }
.pc-text h3 { margin: 0; font-size: 1.2rem; color: var(--primary-dark); font-weight: 900; }
.pc-text p { margin: 6px 0 0 0; font-size: 0.9rem; color: #64748B; font-weight: 600; }

.btn-privacy-toggle { display: flex; align-items: center; gap: 10px; padding: 14px 24px; border-radius: 12px; border: 1.5px solid #E2E8F0; background: white; font-weight: 800; font-size: 0.9rem; color: #1E293B; cursor: pointer; transition: 0.2s; }
.btn-privacy-toggle.active { background: #0C1B33; color: white; border-color: #0C1B33; }

.alert-mini-item { display: flex; gap: 10px; padding: 12px; border-radius: 10px; background: #F8FAFC; margin-bottom: 8px; font-size: 0.8rem; font-weight: 600; color: #64748B; position: relative; }
.alert-mini-item small { position: absolute; right: 12px; top: 12px; font-size: 0.65rem; opacity: 0.5; }
.alert-mini-item svg { color: #F44336; }
</style>
