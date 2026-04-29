<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../api/client';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const accounts = ref<any[]>([]);
const selectedAccountId = ref('');
const loading = ref(false);
const showSuccess = ref(false);

const bills = ref([
    { id: 1, type: 'Elektrik', provider: 'CK Boğaziçi', amount: 452.30, dueDate: '12.05.2026', icon: '⚡' },
    { id: 2, type: 'Su', provider: 'İSKİ', amount: 185.10, dueDate: '15.05.2026', icon: '💧' },
    { id: 3, type: 'İnternet', provider: 'Türk Telekom', amount: 299.90, dueDate: '10.05.2026', icon: '🌐' }
]);

const fetchAccounts = async () => {
    try {
        const res = await apiClient.get('/accounts');
        accounts.value = res.data;
        if (accounts.value.length > 0) selectedAccountId.value = accounts.value[0].id;
    } catch (err) { console.error(err); }
};

const handlePay = async (bill: any) => {
    if (!selectedAccountId.value) {
        toast.error("Lütfen bir ödeme hesabı seçin.");
        return;
    }
    
    // GÜVENLİK KONTROLÜ: Bakiyeyi kontrol et (Demo için manuel kontrol ekledik)
    const activeAcc = accounts.value.find(a => a.id === selectedAccountId.value);
    if (activeAcc && activeAcc.balance < bill.amount) {
        toast.error("Seçili hesapta yeterli bakiye bulunmuyor.");
        return;
    }

    loading.value = true;
    try {
        // Sunumda hata almamak için başarılı bir akış simüle ediyoruz
        // Tutar bakiyeden düşer
        if (activeAcc) activeAcc.balance -= bill.amount;
        
        // Dashboard geçmişine ekle (Entegrasyon)
        const newTransaction = {
            id: 'bill-' + Date.now(),
            type: 'Withdrawal',
            description: `${bill.type} Faturası: ${bill.provider}`,
            amount: bill.amount,
            createdAt: new Date().toISOString()
        };
        const manualTx = JSON.parse(localStorage.getItem('manual_transactions') || '[]');
        manualTx.unshift(newTransaction);
        localStorage.setItem('manual_transactions', JSON.stringify(manualTx));

        toast.success(`${bill.type} faturanız başarıyla ödendi.`);
        
        bills.value = bills.value.filter(b => b.id !== bill.id);
        if (bills.value.length === 0) showSuccess.value = true;
    } catch (err: any) {
        toast.error('İşlem sırasında bir hata oluştu, lütfen daha sonra deneyin.');
    } finally {
        loading.value = false;
    }
};

onMounted(fetchAccounts);
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient">Fatura & Kurum Ödemeleri</h1>
      <p class="subtitle">Anlaşmalı kurumlara ait borçlarınızı NexBank hızıyla tek tıkla ödeyin.</p>
    </header>

    <div class="glass-card mb-5 p-4 d-flex align-items-center gap-4">
        <div class="p-3 bg-light rounded-pill">💳</div>
        <div class="flex-fill">
            <small class="text-muted font-weight-bold d-block mb-1">ÖDEME YAPILACAK HESAP</small>
            <select v-model="selectedAccountId" class="premium-select w-100">
                <option v-for="a in accounts" :key="a.id" :value="a.id">
                    {{ a.iban }} - Bakiye: {{ a.balance.toLocaleString() }} {{ a.currency }}
                </option>
            </select>
        </div>
    </div>

    <div v-if="showSuccess" class="success-banner view-animate mb-4">
        🎉 Fatura ödemeniz başarıyla gerçekleştirildi. Dekontunuz belgelerim sekmesine eklendi.
    </div>

    <div class="bills-list-premium">
        <div v-for="b in bills" :key="b.id" class="glass-card bill-item-card">
            <div class="bill-icon">{{ b.icon }}</div>
            <div class="bill-main">
                <div class="b-info">
                    <h4>{{ b.type }} Faturası</h4>
                    <span>{{ b.provider }}</span>
                </div>
                <div class="b-amount">
                    <strong>{{ b.amount.toFixed(2) }} ₺</strong>
                    <small>Son Tarih: {{ b.dueDate }}</small>
                </div>
            </div>
            <button @click="handlePay(b)" :disabled="loading" class="btn-premium btn-gold-small px-5">Borcu Öde</button>
        </div>

        <div v-if="bills.length === 0" class="p-5 text-center glass-card">
            <div class="mb-3" style="font-size: 3rem;">✅</div>
            <h3>Tüm Faturalar Ödendi!</h3>
            <p class="text-muted">Şu anda sistemde adınıza kayıtlı bir borç bulunmuyor.</p>
        </div>
    </div>
  </div>
</template>

<style scoped>
.premium-select { background: transparent; border: none; font-size: 1.1rem; font-weight: 800; color: #0F172A; outline: none; }

.bills-list-premium { display: flex; flex-direction: column; gap: 15px; }
.bill-item-card { display: flex; align-items: center; gap: 20px; padding: 25px 35px; border-radius: 24px; transition: transform 0.2s; }
.bill-item-card:hover { transform: scale(1.01); background: #F8FAFC; }

.bill-icon { width: 50px; height: 50px; background: white; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 1.6rem; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }

.bill-main { flex: 1; display: flex; justify-content: space-between; align-items: center; }
.b-info h4 { margin: 0; font-size: 1.1rem; color: #0F172A; }
.b-info span { color: #64748B; font-weight: 600; font-size: 0.85rem; }

.b-amount { text-align: right; }
.b-amount strong { display: block; font-size: 1.25rem; color: #0F172A; }
.b-amount small { color: #EF4444; font-weight: 800; font-size: 0.75rem; }

.success-banner { background: #E1F7E5; color: #166534; padding: 15px; border-radius: 16px; text-align: center; font-weight: 700; border: 1px solid #10B981; }
</style>
