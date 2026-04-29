<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../store/auth';
import { useRouter } from 'vue-router';
import { 
  Mail, Lock, ArrowRight, ShieldCheck, 
  AlertCircle, Loader2, Landmark
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';
import Logo from '../components/Logo.vue';

const authStore = useAuthStore();
const toast = useToastStore();
const router = useRouter();

const email = ref('');
const password = ref('');
const localError = ref('');
const isSubmitting = ref(false);

const handleStaffLogin = async () => {
  if (!email.value || !password.value) return;
  
  localError.value = '';
  isSubmitting.value = true;
  
  try {
    const success = await authStore.staffLogin({
      email: email.value,
      password: password.value
    });
    
    if (success) {
      toast.success("Personel paneline hoş geldiniz.");
      router.push('/approval-center'); // Veya personel dashboardu
    } else {
      localError.value = authStore.error || "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.";
    }
  } catch (err) {
    localError.value = "Sunucu bağlantı hatası.";
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="auth-page staff-mode fade-in">
    <!-- SOL: STAFF BRANDING -->
    <div class="auth-promo staff-promo">
        <div class="promo-content">
            <div class="staff-tag">PERSONEL ERİŞİMİ</div>
            <Logo :scale="0.8" />
            <h1 class="promo-title mt-4">Güçlü Yönetim, <br><span>Mükemmel Hizmet.</span></h1>
            <p class="promo-desc">NexBank personel terminaline hoş geldiniz. Tüm operasyonları buradan yönetebilirsiniz.</p>
            
            <div class="promo-features mt-5">
                <div class="p-feat">
                    <ShieldCheck :size="20" />
                    <span>Güvenli Terminal Bağlantısı</span>
                </div>
                <div class="p-feat">
                    <Landmark :size="20" />
                    <span>Bankacılık Operasyon Merkezi</span>
                </div>
            </div>
        </div>
        <div class="promo-footer">
            <p>Yalnızca yetkili personel erişimi içindir. Tüm işlemler kayıt altına alınmaktadır.</p>
        </div>
    </div>

    <!-- SAĞ: STAFF LOGIN FORM -->
    <div class="auth-form-container">
        <div class="form-card-p">
            <div class="form-header">
                <h2>Terminal Girişi</h2>
                <p>Lütfen kurumsal kimlik bilgilerinizle oturum açın.</p>
            </div>

            <form @submit.prevent="handleStaffLogin" class="mt-5">
                <div class="p-field-v2">
                    <label><Mail :size="14" /> Kurumsal E-Posta</label>
                    <input v-model="email" type="email" placeholder="ad.soyad@nexbank.com" required />
                </div>

                <div class="p-field-v2 mt-4">
                    <label><Lock :size="14" /> Sistem Şifresi</label>
                    <input v-model="password" type="password" placeholder="••••••••" required />
                </div>

                <!-- ERROR ALERT -->
                <div v-if="localError" class="auth-error-alert mt-4 slide-up">
                    <AlertCircle :size="18" />
                    <span>{{ localError }}</span>
                </div>

                <button type="submit" class="btn-staff-auth mt-5" :disabled="isSubmitting">
                    <Loader2 v-if="isSubmitting" class="spin" :size="20" />
                    <span v-else>Sisteme Bağlan</span>
                    <ArrowRight v-if="!isSubmitting" :size="20" />
                </button>
            </form>

            <div class="form-footer mt-5">
                <router-link to="/login" class="auth-link">Müşteri Girişine Dön</router-link>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.auth-page { display: grid; grid-template-columns: 450px 1fr; min-height: 100vh; background: white; }

.staff-promo { background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%) !important; }
.staff-tag { display: inline-block; background: var(--gold); color: #0C1B33; padding: 4px 12px; border-radius: 6px; font-size: 0.7rem; font-weight: 900; letter-spacing: 1px; margin-bottom: 24px; }

.auth-promo { padding: 60px; color: white; display: flex; flex-direction: column; justify-content: space-between; position: relative; overflow: hidden; }
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

.auth-error-alert { background: #FEE2E2; color: #991B1B; padding: 14px 20px; border-radius: 12px; display: flex; align-items: center; gap: 12px; font-weight: 700; font-size: 0.9rem; }

.btn-staff-auth { width: 100%; padding: 18px; background: var(--gold-dark); color: #0C1B33; border: none; border-radius: 14px; font-weight: 900; font-size: 1.1rem; cursor: pointer; display: flex; align-items: center; justify-content: center; gap: 15px; transition: 0.3s; }
.btn-staff-auth:hover { background: #0C1B33; color: white; transform: translateY(-2px); box-shadow: 0 10px 25px rgba(0,0,0,0.1); }

.form-footer { text-align: center; font-weight: 600; color: #64748B; font-size: 0.95rem; }
.auth-link { color: #0C1B33; font-weight: 800; text-decoration: none; border-bottom: 2px solid var(--gold); }

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

@media (max-width: 900px) {
    .auth-page { grid-template-columns: 1fr; }
    .auth-promo { display: none; }
}
</style>
