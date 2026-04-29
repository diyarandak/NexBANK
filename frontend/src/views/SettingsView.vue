<script setup lang="ts">
import { ref } from 'vue';
import { 
  User, Mail, Phone, MapPin, 
  ShieldCheck, Lock, Bell, Smartphone,
  Save, ChevronRight, Camera, CreditCard,
  UserCheck, Fingerprint
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();

const profile = ref({
    fullName: 'Diyar Andak',
    email: 'diyar@nexbank.com',
    phone: '+90 555 123 45 67',
    tcNo: '123******89',
    birthDate: '15.06.1995',
    address: 'Yalova, Türkiye - Nex Tower No: 42',
    memberSince: 'Ocak 2024',
    accountLevel: 'Premium Elite'
});

const settings = ref({
    smsNotif: true,
    emailNotif: true,
    twoFactor: true,
    biometric: false
});

const saveSettings = () => {
    toast.success('Profil bilgileriniz ve ayarlarınız başarıyla güncellendi.');
};
</script>

<template>
  <div class="view-container fade-in">
    <header class="settings-header mb-4">
      <div class="h-left">
        <h1 class="text-gradient-gold">Profil & Güvenlik</h1>
        <p class="subtitle">Kişisel verilerinizi ve hesap güvenliğinizi en üst düzeyde yönetin.</p>
      </div>
      <div class="h-right">
        <div class="status-badge">
            <ShieldCheck :size="14" />
            Hesap Durumu: Güvenli
        </div>
      </div>
    </header>

    <div class="settings-grid">
        <!-- PROFILE CARD -->
        <div class="profile-summary-side">
            <div class="glass-card-elite profile-card">
                <div class="pc-avatar">
                    <div class="avatar-circle">
                        <span>DA</span>
                    </div>
                    <button class="btn-camera"><Camera :size="14" /></button>
                </div>
                <div class="pc-info text-center mt-3">
                    <h3>{{ profile.fullName }}</h3>
                    <p>{{ profile.accountLevel }} Üye</p>
                    <span class="member-tag">Müşteri No: 9872541</span>
                </div>
                <div class="pc-stats mt-4">
                    <div class="pcs-item">
                        <small>ÜYELİK</small>
                        <strong>{{ profile.memberSince }}</strong>
                    </div>
                    <div class="pcs-divider"></div>
                    <div class="pcs-item">
                        <small>ŞUBE</small>
                        <strong>Yalova Şubesi</strong>
                    </div>
                </div>
            </div>

            <div class="glass-card-elite mt-3 p-4">
                <div class="security-level">
                    <div class="sl-header">
                        <Fingerprint :size="20" class="text-gold" />
                        <strong>Güvenlik Seviyesi</strong>
                    </div>
                    <div class="sl-bar">
                        <div class="sl-progress" style="width: 85%"></div>
                    </div>
                    <p class="sl-desc">Hesabınız %85 oranında korunuyor. 2FA aktif.</p>
                </div>
            </div>
        </div>

        <!-- SETTINGS FORM -->
        <div class="settings-form-side">
            <div class="glass-card-elite p-4">
                <div class="section-title mb-4">
                    <User :size="18" />
                    <span>KİŞİSEL BİLGİLER</span>
                </div>
                
                <div class="form-row-premium">
                    <div class="input-group-v2">
                        <label>TAM AD SOYAD</label>
                        <div class="input-wrapper">
                            <User :size="16" />
                            <input type="text" v-model="profile.fullName" />
                        </div>
                    </div>
                    <div class="input-group-v2">
                        <label>T.C. KİMLİK NO</label>
                        <div class="input-wrapper disabled">
                            <UserCheck :size="16" />
                            <input type="text" v-model="profile.tcNo" disabled />
                        </div>
                    </div>
                </div>

                <div class="form-row-premium mt-3">
                    <div class="input-group-v2">
                        <label>E-POSTA ADRESİ</label>
                        <div class="input-wrapper">
                            <Mail :size="16" />
                            <input type="email" v-model="profile.email" />
                        </div>
                    </div>
                    <div class="input-group-v2">
                        <label>TELEFON NUMARASI</label>
                        <div class="input-wrapper">
                            <Phone :size="16" />
                            <input type="text" v-model="profile.phone" />
                        </div>
                    </div>
                </div>

                <div class="input-group-v2 mt-3">
                    <label>EV / İŞ ADRESİ</label>
                    <div class="input-wrapper">
                        <MapPin :size="16" />
                        <input type="text" v-model="profile.address" />
                    </div>
                </div>

                <div class="section-title mt-5 mb-4">
                    <Lock :size="18" />
                    <span>GÜVENLİK VE TERCİHLER</span>
                </div>

                <div class="preference-list">
                    <div class="pref-item">
                        <div class="pi-info">
                            <div class="pi-icon"><Bell :size="18" /></div>
                            <div>
                                <strong>Bildirim Tercihleri</strong>
                                <span>SMS ve E-Posta ile bilgilendiril</span>
                            </div>
                        </div>
                        <div class="pi-action">
                            <label class="switch">
                                <input type="checkbox" v-model="settings.smsNotif">
                                <span class="slider"></span>
                            </label>
                        </div>
                    </div>

                    <div class="pref-item">
                        <div class="pi-info">
                            <div class="pi-icon"><Smartphone :size="18" /></div>
                            <div>
                                <strong>İki Faktörlü Doğrulama (2FA)</strong>
                                <span>Girişlerde mobil onay kullan</span>
                            </div>
                        </div>
                        <div class="pi-action">
                            <label class="switch">
                                <input type="checkbox" v-model="settings.twoFactor">
                                <span class="slider"></span>
                            </label>
                        </div>
                    </div>

                    <div class="pref-item clickable" @click="toast.info('Şifre değiştirme ekranına yönlendiriliyorsunuz...')">
                        <div class="pi-info">
                            <div class="pi-icon"><Lock :size="18" /></div>
                            <div>
                                <strong>Giriş Şifresi</strong>
                                <span>Son değişim: 2 ay önce</span>
                            </div>
                        </div>
                        <div class="pi-action">
                            <ChevronRight :size="20" />
                        </div>
                    </div>
                </div>

                <button @click="saveSettings" class="btn-elite-gold w-100 mt-5">
                    <Save :size="18" />
                    <span>Değişiklikleri Kaydet</span>
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.settings-header { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2.2rem; font-weight: 900;
}
.text-gold { color: #D4AF37; }

.status-badge { 
    background: rgba(16, 185, 129, 0.1); color: #10B981; 
    padding: 8px 16px; border-radius: 100px; font-size: 0.75rem; 
    font-weight: 800; display: flex; align-items: center; gap: 8px;
}

.settings-grid { display: grid; grid-template-columns: 1fr 2.2fr; gap: 24px; align-items: start; }

/* PROFILE CARD */
.profile-card { padding: 32px; position: relative; }
.pc-avatar { position: relative; width: 80px; height: 80px; margin: 0 auto; }
.avatar-circle { 
    width: 100%; height: 100%; background: #0F172A; border-radius: 50%; 
    display: flex; align-items: center; justify-content: center;
    color: #D4AF37; font-size: 1.8rem; font-weight: 900; font-family: 'Outfit';
    border: 3px solid #F1F5F9;
}
.btn-camera { 
    position: absolute; bottom: 0; right: 0; background: #D4AF37; 
    color: #0F172A; border: none; width: 28px; height: 28px; 
    border-radius: 50%; cursor: pointer; display: flex; align-items: center; justify-content: center;
}

.pc-info h3 { font-size: 1.3rem; font-weight: 900; color: #0F172A; margin-bottom: 4px; }
.pc-info p { font-size: 0.8rem; color: #D4AF37; font-weight: 800; text-transform: uppercase; letter-spacing: 1px; }
.member-tag { font-size: 0.7rem; color: #94A3B8; font-weight: 700; }

.pc-stats { display: flex; justify-content: center; align-items: center; gap: 20px; }
.pcs-item { text-align: center; }
.pcs-item small { display: block; font-size: 0.6rem; color: #94A3B8; font-weight: 800; margin-bottom: 4px; }
.pcs-item strong { font-size: 0.85rem; color: #0F172A; font-weight: 800; }
.pcs-divider { width: 1px; height: 30px; background: #F1F5F9; }

.security-level { text-align: center; }
.sl-header { display: flex; align-items: center; justify-content: center; gap: 8px; margin-bottom: 12px; }
.sl-bar { height: 6px; background: #F1F5F9; border-radius: 10px; overflow: hidden; margin-bottom: 10px; }
.sl-progress { height: 100%; background: linear-gradient(to right, #D4AF37, #10B981); border-radius: 10px; }
.sl-desc { font-size: 0.7rem; color: #64748B; font-weight: 700; }

/* FORM STYLES */
.section-title { display: flex; align-items: center; gap: 10px; color: #0F172A; font-weight: 900; font-size: 0.85rem; letter-spacing: 1px; border-bottom: 1px solid #F1F5F9; padding-bottom: 12px; }
.form-row-premium { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }

.input-group-v2 label { display: block; font-size: 0.65rem; font-weight: 800; color: #94A3B8; margin-bottom: 8px; letter-spacing: 0.5px; }
.input-wrapper { 
    background: #F8FAFC; border: 1px solid #F1F5F9; border-radius: 14px; 
    padding: 12px 16px; display: flex; align-items: center; gap: 12px; color: #94A3B8;
    transition: 0.2s;
}
.input-wrapper:focus-within { border-color: #D4AF37; background: white; box-shadow: 0 0 0 3px rgba(212, 175, 55, 0.1); }
.input-wrapper input { border: none; background: transparent; outline: none; width: 100%; font-weight: 700; color: #0F172A; font-size: 0.9rem; }
.input-wrapper.disabled { opacity: 0.6; background: #F1F5F9; cursor: not-allowed; }

.preference-list { display: flex; flex-direction: column; gap: 12px; }
.pref-item { 
    display: flex; justify-content: space-between; align-items: center; 
    padding: 16px; background: #F8FAFC; border-radius: 16px; transition: 0.2s;
}
.pref-item.clickable:hover { background: #F1F5F9; cursor: pointer; }
.pi-info { display: flex; align-items: center; gap: 16px; }
.pi-icon { width: 40px; height: 40px; background: white; border-radius: 10px; display: flex; align-items: center; justify-content: center; color: #0F172A; }
.pi-info strong { display: block; font-size: 0.9rem; color: #0F172A; font-weight: 800; }
.pi-info span { font-size: 0.75rem; color: #64748B; font-weight: 600; }

.switch { position: relative; display: inline-block; width: 44px; height: 24px; }
.switch input { opacity: 0; width: 0; height: 0; }
.slider { position: absolute; cursor: pointer; top: 0; left: 0; right: 0; bottom: 0; background-color: #CBD5E1; transition: .4s; border-radius: 34px; }
.slider:before { position: absolute; content: ""; height: 18px; width: 18px; left: 3px; bottom: 3px; background-color: white; transition: .4s; border-radius: 50%; }
input:checked + .slider { background-color: #10B981; }
input:checked + .slider:before { transform: translateX(20px); }

.btn-elite-gold { 
    background: #0F172A; color: #D4AF37; border: none; padding: 18px; 
    border-radius: 18px; font-size: 1.1rem; font-weight: 900; cursor: pointer;
    display: flex; align-items: center; justify-content: center; gap: 12px;
}
.btn-elite-gold:hover { background: #000; transform: translateY(-2px); box-shadow: 0 10px 20px rgba(0,0,0,0.1); }
</style>
