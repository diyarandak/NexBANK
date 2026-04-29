<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../store/auth';
import { useRouter } from 'vue-router';
import { 
  Fingerprint, Lock, ArrowRight, ShieldCheck, 
  CheckCircle2, AlertCircle, Loader2, UserCircle2
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const authStore = useAuthStore();
const toast = useToastStore();
const router = useRouter();

const tcKimlik = ref('');
const password = ref('');
const localError = ref('');
const isSubmitting = ref(false);

const handleLogin = async () => {
  if (!tcKimlik.value || !password.value) return;
  
  localError.value = '';
  isSubmitting.value = true;
  
  try {
    const success = await authStore.customerLogin({
      tcKimlikNo: tcKimlik.value,
      password: password.value
    });
    
    if (success) {
      toast.success("Başarıyla giriş yapıldı. Hoş geldiniz!");
      router.push('/dashboard');
    } else {
      localError.value = authStore.error || "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.";
    }
  } catch (err) {
    localError.value = "Bağlantı hatası oluştu.";
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="auth-page fade-in">
    <!-- SOL: BRANDING (Aynı Register'daki gibi) -->
    <div class="auth-promo">
        <div class="promo-content">
            <div class="promo-logo">NB</div>
            <h1 class="promo-title">Güvenliğiniz <br><span>Bizim Önceliğimizdir.</span></h1>
            <p class="promo-desc">NexBank'in biyometrik ve çift faktörlü koruma sistemleriyle varlıklarınız her zaman güvende.</p>
            
            <div class="promo-features mt-5">
                <div class="p-feat">
                    <ShieldCheck :size="20" />
                    <span>256-bit SSL Şifreleme</span>
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

    <!-- SAĞ: LOGIN FORM -->
    <div class="auth-form-container">
        <div class="form-card-p">
            <div class="form-header">
                <h2>Hoş Geldiniz</h2>
                <p>NexBank Premium dünyasına giriş yapmak için bilgilerinizi girin.</p>
            </div>

            <form @submit.prevent="handleLogin" class="mt-5 register-form-p">
                <div class="p-field-v2">
                    <label><Fingerprint :size="14" /> TC Kimlik No</label>
                    <input v-model="tcKimlik" type="text" maxlength="11" placeholder="11 Haneli TC No" required />
                </div>

                <div class="p-field-v2 mt-4">
                    <div class="label-row">
                        <label><Lock :size="14" /> Parola</label>
                        <a href="#" class="forgot-pass">Şifremi Unuttum</a>
                    </div>
                    <input v-model="password" type="password" placeholder="••••••••" required />
                </div>

                <!-- ERROR ALERT -->
                <div v-if="localError" class="auth-error-alert mt-4 slide-up">
                    <AlertCircle :size="18" />
                    <span>{{ localError }}</span>
                </div>

                <button type="submit" class="btn-auth-premium mt-5" :disabled="isSubmitting">
                    <Loader2 v-if="isSubmitting" class="spin" :size="20" />
                    <span v-else>Güvenli Giriş Yap</span>
                    <ArrowRight v-if="!isSubmitting" :size="20" />
                </button>

                <div class="staff-login-p mt-4">
                    <router-link to="/login-staff" class="staff-link">
                        <UserCircle2 :size="16" />
                        Personel Girişi
                    </router-link>
                </div>
            </form>

            <div class="form-footer mt-5">
                <span>Henüz NexBank müşterisi değil misiniz?</span>
                <router-link to="/register" class="auth-link">Hemen Kaydolun</router-link>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
/* Register ile aynı baz stiller kullanılacak */
.auth-page { display: grid; grid-template-columns: 450px 1fr; min-height: 100vh; background: white; }
.auth-promo { background: linear-gradient(135deg, #0C1B33 0%, #1A2F4B 100%); padding: 60px; color: white; display: flex; flex-direction: column; justify-content: space-between; position: relative; overflow: hidden; }
.promo-logo { font-size: 2.5rem; font-weight: 900; font-family: 'Outfit'; color: var(--gold); margin-bottom: 40px; }
.promo-title { font-size: 2.5rem; font-weight: 800; line-height: 1.2; }
.promo-title span { color: var(--gold); }
.promo-desc { margin-top: 20px; font-size: 1.1rem; opacity: 0.8; line-height: 1.6; }
.p-feat { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; font-weight: 700; font-size: 0.9rem; opacity: 0.9; }
.p-feat svg { color: var(--gold); }
.promo-footer { font-size: 0.8rem; opacity: 0.5; }

.auth-form-container { display: flex; align-items: center; justify-content: center; padding: 60px; background: #FAFBFC; }
.form-card-p { width: 100%; max-width: 450px; }
.form-header h2 { font-size: 2.2rem; font-weight: 800; color: #0C1B33; margin: 0; }
.form-header p { color: #64748B; font-weight: 600; margin-top: 8px; }

.p-field-v2 label { display: flex; align-items: center; gap: 8px; font-size: 0.8rem; font-weight: 800; color: #0C1B33; margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
.p-field-v2 input { width: 100%; padding: 14px 20px; border-radius: 12px; border: 1.5px solid #E2E8F0; font-size: 1rem; font-weight: 600; outline: none; transition: 0.2s; background: white; }
.p-field-v2 input:focus { border-color: var(--primary); box-shadow: 0 0 0 4px rgba(12, 27, 51, 0.05); }

.label-row { display: flex; justify-content: space-between; align-items: center; }
.forgot-pass { font-size: 0.8rem; font-weight: 700; color: #64748B; text-decoration: none; }
.forgot-pass:hover { color: var(--primary); }

.auth-error-alert { background: #FEE2E2; color: #991B1B; padding: 14px 20px; border-radius: 12px; display: flex; align-items: center; gap: 12px; font-weight: 700; font-size: 0.9rem; }

.btn-auth-premium { width: 100%; padding: 18px; background: #0C1B33; color: white; border: none; border-radius: 14px; font-weight: 800; font-size: 1.1rem; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 15px; transition: 0.3s; }
.btn-auth-premium:hover { background: var(--gold-dark); color: #0C1B33; transform: translateY(-2px); box-shadow: 0 10px 25px rgba(12, 27, 51, 0.15); }

.staff-login-p { text-align: center; }
.staff-link { display: inline-flex; align-items: center; gap: 8px; color: #64748B; font-size: 0.85rem; font-weight: 700; text-decoration: none; padding: 8px 16px; border-radius: 80px; transition: 0.2s; }
.staff-link:hover { background: #E2E8F0; color: #0C1B33; }

.form-footer { text-align: center; font-weight: 600; color: #64748B; font-size: 0.95rem; }
.auth-link { color: #0C1B33; font-weight: 800; text-decoration: none; margin-left: 8px; border-bottom: 2px solid var(--gold); }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

@media (max-width: 900px) {
    .auth-page { grid-template-columns: 1fr; }
    .auth-promo { display: none; }
}
</style>
