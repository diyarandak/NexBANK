<script setup lang="ts">
import { ref } from 'vue';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const summary = ref({
  totalInflow: 1250000.00,
  totalOutflow: 980000.00,
  cashInVault: 2500000.00,
  status: 'Denk'
});

const runReconciliation = () => {
  toast.success('Gün sonu mutabakatı başarıyla tamamlandı. Kasa: Denk.');
};
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient" style="font-size: 2.2rem;">Gün Sonu Mutabakat</h1>
      <p class="subtitle">Şube kasası ve nakit akışı raporları.</p>
    </header>

    <div class="glass-card mb-4 text-center">
      <div class="row">
        <div class="col-md-3">
          <p class="text-muted mb-1">Giren Para</p>
          <h3 class="text-success">+{{ summary.totalInflow.toLocaleString('tr-TR') }} TL</h3>
        </div>
        <div class="col-md-3">
          <p class="text-muted mb-1">Çıkan Para</p>
          <h3 class="text-danger">-{{ summary.totalOutflow.toLocaleString('tr-TR') }} TL</h3>
        </div>
        <div class="col-md-3">
          <p class="text-muted mb-1">Kasa Mevcudu</p>
          <h3 class="text-primary">{{ summary.cashInVault.toLocaleString('tr-TR') }} TL</h3>
        </div>
        <div class="col-md-3">
          <p class="text-muted mb-1">Durum</p>
          <h3><span class="badge bg-success">{{ summary.status }}</span></h3>
        </div>
      </div>
    </div>

    <button class="btn btn-primary px-5 py-3" @click="runReconciliation">Gün Sonu Kapat</button>
  </div>
</template>
