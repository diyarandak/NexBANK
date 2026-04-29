<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../store/auth';
import { useRouter } from 'vue-router';
import { 
  User, Mail, Fingerprint, Lock, 
  MapPin, ArrowRight, ShieldCheck, 
  CheckCircle2, AlertCircle, Loader2
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const authStore = useAuthStore();
const toast = useToastStore();
const router = useRouter();

const form = ref({
  fullName: '',
  tcKimlik: '',
  email: '',
  password: '',
  branch: 'Yalova Merkez'
});

const localError = ref('');
const isSubmitting = ref(false);

const handleRegister = async () => {
  localError.value = '';
  isSubmitting.value = true;

  // Frontend Validations
  if (!form.value.fullName.trim()) {
    localError.value = "Lütfen ad soyad giriniz.";
    isSubmitting.value = false;
    return;
  }

  if (form.value.tcKimlik.length !== 11 || !/^\d{11}$/.test(form.value.tcKimlik)) {
    localError.value = "TC Kimlik No 11 haneli rakamlardan oluşmalıdır.";
    isSubmitting.value = false;
    return;
  }

  if (!/^(?=.*[A-Z])(?=.*\d).{8,}$/.test(form.value.password)) {
    localError.value = "Şifre en az 8 karakter, bir büyük harf ve bir rakam içermelidir.";
    isSubmitting.value = false;
    return;
  }

  try {
    const success = await authStore.registerCustomer({
      FullName: form.value.fullName,
      TcKimlikNo: form.value.tcKimlik,
      Email: form.value.email,
      Password: form.value.password,
      Branch: form.value.branch
    });
    
    if (success) {
      toast.success("Müşteri kaydınız başarıyla tamamlandı! Giriş yapabilirsiniz.");
      router.push('/login');
    } else {
      // Backend'den gelen spesifik hata mesajını göster
      localError.value = authStore.error || "Kayıt sırasında bir hata oluştu. Bilgileri kontrol edin.";
    }
  } catch (err: any) {
    localError.value = "Sunucuya ulaşılamıyor. Lütfen internet bağlantınızı ve sunucuyu kontrol edin.";
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="auth-page fade-in">
    <!-- SOL: BRANDING & INFO -->
    <div class="auth-promo">
        <div class="promo-content">
            <div class="promo-logo">NB</div>
            <h1 class="promo-title">Dijital Bankacılığın <br><span>Zirvesine Hoş Geldiniz.</span></h1>
            <p class="promo-desc">Dakikalar içinde NexBank müşterisi olun, premium finansal avantajlardan yararlanmaya başlayın.</p>
            
            <div class="promo-features mt-5">
                <div class="p-feat">
                    <ShieldCheck :size="20" />
                    <span>Uçtan Uca Güvenli Altyapı</span>
                </div>
                <div class="p-feat">
                    <CheckCircle2 :size="20" />
                    <span>Anlık İşlem Bildirimleri</span>
                </div>
            </div>
        </div>
        <div class="promo-footer">
            <p>© 2026 NexBank A.Ş. Tüm hakları saklıdır.</p>
        </div>
    </div>

    <!-- SAĞ: REGISTER FORM -->
    <div class="auth-form-container">
        <div class="form-card-p">
            <div class="form-header">
                <h2>Müşteri Ol</h2>
                <p>Kişisel bilgilerinizi girerek hesabınızı hemen oluşturun.</p>
            </div>

            <form @submit.prevent="handleRegister" class="mt-5 register-form-p">
                <div class="form-grid">
                    <div class="p-field-v2">
                        <label><User :size="14" /> Ad Soyad</label>
                        <input v-model="form.fullName" type="text" placeholder="Ad Soyad" required />
                    </div>

                    <div class="p-field-v2">
                        <label><Fingerprint :size="14" /> TC Kimlik No</label>
                        <input v-model="form.tcKimlik" type="text" maxlength="11" placeholder="11 Haneli TC No" required />
                    </div>
                </div>

                <div class="p-field-v2 mt-3">
                    <label><Mail :size="14" /> E-Posta Adresi</label>
                    <input v-model="form.email" type="email" placeholder="ornek@mail.com" required />
                </div>

                <div class="p-field-v2 mt-3">
                    <label><Lock :size="14" /> Güçlü Şifre</label>
                    <input v-model="form.password" type="password" placeholder="••••••••" required />
                    <small class="hint">En az 8 karakter, 1 Büyük harf, 1 Rakam</small>
                </div>

                <div class="p-field-v2 mt-3">
                    <label><MapPin :size="14" /> Bağlı Olacağınız Şube</label>
                    <select v-model="form.branch" class="p-select-v2">
                        <option value="Yalova Merkez">Yalova Merkez Şubesi</option>
                        <option value="Ankara Şubesi">Ankara Şubesi</option>
                        <option value="İstanbul Şubesi">İstanbul Şubesi</option>
                        <option value="İzmir Şubesi">İzmir Şubesi</option>
                    </select>
                </div>

                <!-- ERROR ALERT (DAHA BELİRGİN) -->
                <div v-if="localError" class="auth-error-alert mt-4 slide-up">
                    <AlertCircle :size="18" />
                    <span>{{ localError }}</span>
                </div>

                <button type="submit" class="btn-auth-premium mt-4" :disabled="isSubmitting">
                    <Loader2 v-if="isSubmitting" class="spin" :size="20" />
                    <span v-else>Hesabımı Oluştur</span>
                    <ArrowRight v-if="!isSubmitting" :size="20" />
                </button>
            </form>

            <div class="form-footer mt-5">
                <span>Zaten NexBank müşterisi misiniz?</span>
                <router-link to="/login" class="auth-link">Giriş Yapın</router-link>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.auth-page {
    display: grid;
    grid-template-columns: 450px 1fr;
    min-height: 100vh;
    background: white;
}

/* LEFT PROMO */
.auth-promo {
    background: linear-gradient(135deg, #0C1B33 0%, #1A2F4B 100%);
    padding: 60px;
    color: white;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    position: relative;
    overflow: hidden;
}

.promo-logo {
    font-size: 2.5rem; font-weight: 900; font-family: 'Outfit'; color: var(--gold); margin-bottom: 40px;
}
.promo-title { font-size: 2.5rem; font-weight: 800; line-height: 1.2; }
.promo-title span { color: var(--gold); }
.promo-desc { margin-top: 20px; font-size: 1.1rem; opacity: 0.8; line-height: 1.6; }

.p-feat { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; font-weight: 700; font-size: 0.9rem; opacity: 0.9; }
.p-feat svg { color: var(--gold); }

.promo-footer { font-size: 0.8rem; opacity: 0.5; }

/* RIGHT FORM */
.auth-form-container {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 60px;
    background: #FAFBFC;
}

.form-card-p { width: 100%; max-width: 500px; }
.form-header h2 { font-size: 2.2rem; font-weight: 800; color: #0C1B33; margin: 0; }
.form-header p { color: #64748B; font-weight: 600; margin-top: 8px; }

.form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }

.p-field-v2 label { display: flex; align-items: center; gap: 8px; font-size: 0.8rem; font-weight: 800; color: #0C1B33; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
.p-field-v2 input, .p-select-v2 {
    width: 100%; padding: 14px 20px; border-radius: 12px; border: 1.5px solid #E2E8F0;
    font-size: 1rem; font-weight: 600; outline: none; transition: 0.2s; background: white;
}
.p-field-v2 input:focus, .p-select-v2:focus { border-color: var(--primary); box-shadow: 0 0 0 4px rgba(12, 27, 51, 0.05); }

.hint { display: block; font-size: 0.75rem; color: #94A3B8; margin-top: 6px; font-weight: 600; }

.auth-error-alert {
    background: #FEE2E2; color: #991B1B; padding: 14px 20px; border-radius: 12px;
    display: flex; align-items: center; gap: 12px; font-weight: 700; font-size: 0.9rem;
    border: 1px solid #FCA5A5;
}

.btn-auth-premium {
    width: 100%; padding: 18px; background: #0C1B33; color: white; border: none;
    border-radius: 14px; font-weight: 800; font-size: 1.1rem; cursor: pointer;
    display: flex; align-items: center; justify-content: center; gap: 15px; transition: 0.3s;
}
.btn-auth-premium:hover { background: var(--gold-dark); color: #0C1B33; transform: translateY(-2px); box-shadow: 0 10px 25px rgba(12, 27, 51, 0.15); }
.btn-auth-premium:disabled { background: #E2E8F0; color: #94A3B8; cursor: not-allowed; transform: none; }

.form-footer { text-align: center; font-weight: 600; color: #64748B; font-size: 0.95rem; }
.auth-link { color: #0C1B33; font-weight: 800; text-decoration: none; margin-left: 8px; border-bottom: 2px solid var(--gold); }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

@media (max-width: 900px) {
    .auth-page { grid-template-columns: 1fr; }
    .auth-promo { display: none; }
}
</style>
