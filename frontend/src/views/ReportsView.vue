<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import apiClient from '../api/client';
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore();
const activeTab = ref('statement');

const userAccounts = ref<any[]>([]);
const selectedAccountId = ref('');

// Ekstre
const statementList = ref<any[]>([]);
const loadingStatement = ref(false);

// Analiz
const analysisData = ref<any>(null);
const loadingAnalysis = ref(false);

const fetchData = async () => {
  try {
    const res = await apiClient.get('/accounts');
    userAccounts.value = res.data;
    if (userAccounts.value.length > 0) {
      selectedAccountId.value = userAccounts.value[0].id;
      await fetchStatement(); // ilk hesabı yükle
    }
  } catch(err) { console.error(err); }
};

onMounted(async () => {
  await fetchData();
  await fetchAnalysis(); // Analizi de genel çek
});

const fetchStatement = async () => {
  if (!selectedAccountId.value) return;
  loadingStatement.value = true;
  try {
    const res = await apiClient.get(`/reports/statement/${selectedAccountId.value}`);
    statementList.value = res.data;
  } catch (err) {
    console.error(err);
  } finally {
    loadingStatement.value = false;
  }
};

const fetchAnalysis = async () => {
  loadingAnalysis.value = true;
  try {
    const res = await apiClient.get('/reports/analysis');
    analysisData.value = res.data;
  } catch (err) {
    console.error(err);
  } finally {
    loadingAnalysis.value = false;
  }
};

const downloadCsv = async () => {
  if (!selectedAccountId.value) return;
  try {
    // API csv byte dondurur, dosya olarak inmesi lazim
    // Axios response type blob kullanarak indirilir.
    const res = await apiClient.get(`/reports/export/${selectedAccountId.value}`, { responseType: 'blob' });
    const url = window.URL.createObjectURL(new Blob([res.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `ekstre_${selectedAccountId.value}_${new Date().getTime()}.csv`);
    document.body.appendChild(link);
    link.click();
    link.remove();
  } catch(err) {
    alert('İndirilirken hata oluştu.');
  }
};

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

const maxCatAmount = computed(() => {
   if(!analysisData.value || !analysisData.value.categories || analysisData.value.categories.length === 0) return 1;
   return Math.max(...analysisData.value.categories.map((c: any) => c.totalAmount));
});
</script>

<template>
  <div class="reports-view">
    <header class="view-header">
      <div class="header-content">
        <h2 class="text-gradient">Raporlar ve Analizler</h2>
        <p>Hesap dökümleriniz, harcama grafikleri ve hesap özeti (Excel/CSV dışa aktarma).</p>
      </div>
    </header>

    <div class="tabs glass-card">
      <button :class="['tab-btn', { active: activeTab === 'statement' }]" @click="activeTab = 'statement'">📄 Hesap Özeti / Ekstre</button>
      <button :class="['tab-btn', { active: activeTab === 'analysis' }]" @click="activeTab = 'analysis'">📊 Harcama Analizi</button>
    </div>

    <!-- EKSTRE EKRANI -->
    <div v-if="activeTab === 'statement'" class="tab-pane">
      <div class="filter-bar glass-card">
        <div class="f-group">
          <label>Hesap Seçimi</label>
          <select v-model="selectedAccountId" @change="fetchStatement">
            <option v-for="a in userAccounts" :key="a.id" :value="a.id">{{ a.iban }} ({{ a.accountType }})</option>
          </select>
        </div>
        <div class="actions">
           <button @click="fetchStatement" class="premium-button">Getir</button>
           <button @click="downloadCsv" class="demo-btn">↓ CSV / Excel İndir</button>
        </div>
      </div>

      <div class="statement-table-wrapper glass-card">
        <div v-if="loadingStatement" class="loader">Yükleniyor...</div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>Tarih</th>
              <th>İşlem Tipi</th>
              <th>Durum</th>
              <th>Açıklama</th>
              <th>Tutar</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="trx in statementList" :key="trx.id">
              <td>{{ new Date(trx.createdAt).toLocaleString() }}</td>
              <td><span class="badge type-badge">{{ trx.type }}</span></td>
              <td><span :class="['badge', trx.status.toLowerCase()]">{{ trx.status }}</span></td>
              <td>{{ trx.description }}</td>
              <td :class="['amount', trx.fromAccountId == selectedAccountId ? 'out' : 'in']">
                {{ trx.fromAccountId == selectedAccountId ? '-' : '+' }}{{ formatCurrency(trx.amount) }}
              </td>
            </tr>
            <tr v-if="statementList.length === 0">
              <td colspan="5" class="empty-text">Bu hesapta hiç işlem bulunamadı.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ANALIZ EKRANI -->
    <div v-if="activeTab === 'analysis'" class="tab-pane">
      <div v-if="loadingAnalysis" class="loader">Analiz hazırlanıyor...</div>
      <div v-else-if="analysisData" class="analysis-layout">
        
        <!-- OVERVIEW -->
        <div class="overview glass-card">
           <h3>Toplam Giden Transfer ve Harcamalar</h3>
           <h1 class="total-spent">{{ formatCurrency(analysisData.totalSpent) }}</h1>
           <p class="desc">Tüm hesaplarınız üzerinden yapılan toplam giden işlemler.</p>
        </div>

        <!-- CATEGORIES BAR CHART -->
        <div class="categories-bars glass-card">
           <h3>Harcama Tipleri Kategorizasyonu</h3>
           <div class="bars-container">
             <div v-for="cat in analysisData.categories" :key="cat.category" class="bar-row">
               <div class="bar-info">
                 <span class="c-name">{{ cat.category }}</span>
                 <span class="c-amt">{{ formatCurrency(cat.totalAmount) }} ({{ cat.count }} işlem)</span>
               </div>
               <div class="bar-track">
                 <div class="bar-fill" :style="{ width: ((cat.totalAmount / maxCatAmount) * 100) + '%' }"></div>
               </div>
             </div>
             <p v-if="analysisData.categories.length === 0" class="empty-state">Yeterli harcama verisi yok.</p>
           </div>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
.reports-view { animation: fadeIn 0.4s ease; padding-bottom: 50px; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

.view-header { margin-bottom: 24px; }
.view-header h2 { font-size: 2rem; margin-bottom: 8px; }

.tabs { display: flex; gap: 8px; margin-bottom: 24px; padding: 6px; border-radius: 14px; }
.tab-btn { flex: 1; padding: 12px; background: transparent; border: none; color: var(--text-muted); font-weight: 600; border-radius: 10px; cursor: pointer; transition: all 0.3s; }
.tab-btn.active { background: linear-gradient(135deg, var(--primary), var(--accent)); color: white; }

.filter-bar { padding: 20px; border-radius: 16px; margin-bottom: 24px; display: flex; gap: 20px; align-items: flex-end; }
.f-group { flex: 1; }
.f-group label { display: block; margin-bottom: 6px; color: var(--text-muted); font-size: 0.9rem; }
.f-group select { width: 100%; padding: 10px; background: rgba(0,0,0,0.3); border: 1px solid var(--border); border-radius: 8px; color: white; }
.actions { display: flex; gap: 10px; }

.statement-table-wrapper { padding: 20px; border-radius: 16px; overflow-x: auto;}
.data-table { width: 100%; border-collapse: collapse; }
.data-table th { text-align: left; padding: 12px; border-bottom: 1px solid var(--border); color: var(--text-muted); font-weight: 500;}
.data-table td { padding: 12px; border-bottom: 1px solid rgba(255,255,255,0.05); }

.badge { padding: 4px 8px; border-radius: 6px; font-size: 0.8rem; font-weight: 600;}
.badge.approved { background: rgba(16,185,129,0.2); color: #10b981; }
.badge.pending { background: rgba(245,158,11,0.2); color: #f59e0b; }
.badge.rejected { background: rgba(239,68,68,0.2); color: #ef4444; }
.type-badge { background: rgba(59,130,246,0.2); color: #3b82f6; }

.amount { font-weight: bold; font-family: monospace; font-size: 1.1rem; }
.amount.in { color: #10b981; }
.amount.out { color: #ef4444; }

.empty-text { text-align: center; color: var(--text-muted); padding: 30px !important; }

/* Analysis Styles */
.analysis-layout { display: grid; grid-template-columns: 1fr 2fr; gap: 24px; }
@media (max-width: 900px) { .analysis-layout { grid-template-columns: 1fr; } }
.overview { padding: 30px; border-radius: 20px; display: flex; flex-direction: column; justify-content: center; align-items: center; text-align: center; background: linear-gradient(135deg, rgba(239,68,68,0.1), transparent);}
.total-spent { font-size: 3rem; margin: 20px 0; color: #ef4444; text-shadow: 0 0 20px rgba(239,68,68,0.4);}

.categories-bars { padding: 30px; border-radius: 20px; }
.bars-container { display: flex; flex-direction: column; gap: 20px; margin-top: 20px; }
.bar-row { display: flex; flex-direction: column; gap: 6px;}
.bar-info { display: flex; justify-content: space-between; font-size: 0.95rem; }
.c-name { font-weight: 600; }
.c-amt { color: var(--text-muted); }
.bar-track { width: 100%; height: 12px; background: rgba(0,0,0,0.3); border-radius: 10px; overflow: hidden; }
.bar-fill { height: 100%; background: linear-gradient(90deg, #3b82f6, #8b5cf6); border-radius: 10px; transition: width 1s ease-out; }
</style>
