<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore();
const transactions = ref<any[]>([]);
const loading = ref(true);
const filterType = ref('All');
const searchQuery = ref('');

const fetchTransactions = async () => {
    try {
        const res = await apiClient.get('/transactions/admin/all');
        transactions.value = res.data;
    } catch (err) {
        console.error('İşlemler yüklenemedi', err);
    } finally {
        loading.value = false;
    }
};

const handleApprove = async (id: number) => {
    if (!confirm('Bu işlemi onaylıyor musunuz?')) return;
    try {
        await apiClient.post(`/transactions/approve/${id}`);
        await fetchTransactions();
        alert('İşlem onaylandı.');
    } catch (err) {
        alert('İşlem onaylanamadı.');
    }
};

const handleReject = async (id: number) => {
    if (!confirm('Bu işlemi REDDEDİP parayı iade etmek istiyor musunuz? (Undo Command tetiklenecek)')) return;
    try {
        await apiClient.post(`/transactions/undo/${id}`);
        await fetchTransactions();
        alert('İşlem reddedildi ve tutar iade edildi (Undo başarılı).');
    } catch (err) {
        alert('İşlem geri alınamadı.');
    }
};

const filteredTransactions = computed(() => {
    let list = transactions.value;
    if (filterType.value !== 'All') {
        list = list.filter(t => t.type === filterType.value);
    }
    if (searchQuery.value) {
        const q = searchQuery.value.toLowerCase();
        list = list.filter(t => 
            t.description?.toLowerCase().includes(q) || 
            t.amount.toString().includes(q)
        );
    }
    return list;
});

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

onMounted(fetchTransactions);
</script>

<template>
  <div class="view-container view-animate">
    <header class="d-flex justify-content-between align-items-center mb-5">
      <div>
        <h1 class="text-gradient" style="font-size: 2.2rem;">Merkezi İşlem İzleme</h1>
        <p class="subtitle">Banka genelindeki tüm nakit ve transfer akışı gerçek zamanlı monitörize ediliyor.</p>
      </div>
      <div class="h-actions d-flex gap-3">
         <select v-model="filterType" class="premium-select">
            <option value="All">Tüm İşlem Türleri</option>
            <option value="Transfer">Para Transferleri</option>
            <option value="Deposit">Para Yatırmalar</option>
            <option value="Withdrawal">Para Çekmeler</option>
         </select>
         <div class="modern-search" style="width: 300px;">
            <span class="m-search-icon">🔍</span>
            <input v-model="searchQuery" type="text" placeholder="İşlem veya bakiye ara..." />
         </div>
      </div>
    </header>

    <div v-if="loading" class="loader-container"><div class="glow-loader"></div></div>

    <div v-else class="glass-card overflow-hidden">
        <table class="premium-table">
            <thead>
                <tr>
                    <th>İşlem ID</th>
                    <th>Tür / Açıklama</th>
                    <th>Tarih & Saat</th>
                    <th>Gönderen ID</th>
                    <th>Alıcı ID</th>
                    <th>Miktar</th>
                    <th>Durum</th>
                    <th>İşlemler</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="t in filteredTransactions" :key="t.id" :class="{ 'suspicious-row': t.status === 'Pending' }">
                    <td><span class="id-tag">#{{ t.id }}</span></td>
                    <td>
                        <div class="t-type-cell">
                            <span class="t-type-icon" :class="t.type.toLowerCase()">
                                {{ t.type === 'Deposit' ? '📥' : (t.type === 'Withdrawal' ? '📤' : '💸') }}
                            </span>
                            <div class="t-desc">
                                <strong>{{ t.type === 'Withdrawal' ? 'Para Çekme' : (t.type === 'Deposit' ? 'Para Yatırma' : 'Transfer') }}</strong>
                                <small :class="{ 'fraud-desc': t.description?.includes('ŞÜPHELİ') }">{{ t.description }}</small>
                            </div>
                        </div>
                    </td>
                    <td>{{ new Date(t.createdAt).toLocaleString() }}</td>
                    <td><code>{{ t.fromAccountId || '---' }}</code></td>
                    <td><code>{{ t.toAccountId || '---' }}</code></td>
                    <td><strong :class="[t.status === 'Rejected' ? 'reverted' : (t.type === 'Withdrawal' ? 'text-danger' : 'text-success')]">{{ formatCurrency(t.amount) }}</strong></td>
                    <td>
                        <span class="t-status-pill" :class="t.status.toLowerCase()">
                            {{ t.status === 'Pending' ? 'İncelemede' : (t.status === 'Approved' ? 'Başarılı' : (t.status === 'Rejected' ? 'Reddedildi' : t.status)) }}
                        </span>
                    </td>
                    <td>
                        <div v-if="t.status === 'Pending'" class="d-flex gap-2">
                            <button @click="handleApprove(t.id)" class="btn-action approve">Onayla</button>
                            <button @click="handleReject(t.id)" class="btn-action reject">Reddet / Geri Al</button>
                        </div>
                        <div v-else>
                           <small class="text-muted">İşlem Tamamlandı</small>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
        <div v-if="filteredTransactions.length === 0" class="empty-state p-5 text-center">
            Listelenecek işlem bulunamadı.
        </div>
    </div>
  </div>
</template>

<style scoped>
.premium-select { background: white; border: 1px solid var(--border); padding: 10px 20px; border-radius: 100px; font-weight: 700; color: #0F172A; outline: none; }
.id-tag { background: #F1F5F9; color: #64748B; font-weight: 800; font-size: 0.75rem; padding: 4px 8px; border-radius: 6px; }

.t-type-cell { display: flex; align-items: center; gap: 12px; }
.t-type-icon { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.1rem; }
.t-type-icon.deposit { background: #E0F2FE; }
.t-type-icon.withdrawal { background: #FEE2E2; }
.t-type-icon.transfer { background: #FEF3C7; }

.t-desc { display: flex; flex-direction: column; }
.t-desc strong { font-size: 0.9rem; color: #0F172A; }
.t-desc small { font-size: 0.75rem; color: #64748B; }

.t-status-pill { padding: 4px 10px; border-radius: 6px; font-weight: 800; font-size: 0.7rem; text-transform: uppercase; }
.t-status-pill.approved { background: #DCFCE7; color: #16A34A; }
.t-status-pill.pending { background: #FEF3C7; color: #D97706; }
.t-status-pill.rejected { background: #FEE2E2; color: #DC2626; }

.suspicious-row { background: rgba(239, 68, 68, 0.05); }
.fraud-desc { color: #DC2626 !important; font-weight: 800; }

.modern-search { background: white; border: 1px solid var(--border); border-radius: 100px; padding: 0 15px; display: flex; align-items: center; }
.modern-search input { border: none; outline: none; flex: 1; padding: 10px; font-weight: 600; font-size: 0.9rem; }

.premium-table { width: 100%; border-collapse: collapse; }
.premium-table th { text-align: left; padding: 20px; background: #F8FAFC; color: #64748B; font-size: 0.75rem; font-weight: 800; text-transform: uppercase; }
.premium-table td { padding: 20px; border-bottom: 1px solid #F1F5F9; font-size: 0.85rem; }

.btn-action {
    padding: 6px 12px;
    border-radius: 8px;
    font-size: 0.75rem;
    font-weight: 700;
    cursor: pointer;
    border: none;
    transition: 0.2s;
}
.btn-action.approve {
    background: #16A34A;
    color: white;
}
.btn-action.approve:hover {
    background: #15803D;
    box-shadow: 0 4px 12px rgba(22, 163, 74, 0.2);
}
.btn-action.reject {
    background: #DC2626;
    color: white;
}
.btn-action.reject:hover {
    background: #B91C1C;
    box-shadow: 0 4px 12px rgba(220, 38, 38, 0.2);
}
.reverted { color: #94A3B8; text-decoration: line-through; opacity: 0.7; }
.text-success { color: #16A34A; }
.text-danger { color: #DC2626; }
</style>
