<script setup lang="ts">
import { ref } from 'vue';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const requestAmount = ref<number | null>(null);
const reason = ref('');

const submitRequest = () => {
  if (!requestAmount.value || requestAmount.value <= 0) {
    toast.error('Lütfen geçerli bir tutar giriniz.');
    return;
  }
  toast.success('Limit artırım talebiniz başarıyla alınmıştır. Değerlendirme sonucunda SMS ile bilgilendirileceksiniz.');
  requestAmount.value = null;
  reason.value = '';
};
</script>

<template>
  <div class="view-container view-animate">
    <header class="mb-5">
      <h1 class="text-gradient" style="font-size: 2.2rem;">Limit Artırım Talebi</h1>
      <p class="subtitle">Kredi kartı veya ek hesap limitinizi anında yükseltin.</p>
    </header>

    <div class="glass-card p-4 mx-auto" style="max-width: 600px;">
      <h3 class="mb-4">Yeni Limit Başvurusu</h3>
      
      <div class="form-group mb-4 text-start">
        <label>Talep Edilen Toplam Limit (TL)</label>
        <input type="number" class="form-control" v-model="requestAmount" placeholder="Örn: 50000" />
      </div>

      <div class="form-group mb-4 text-start">
        <label>Artırım Sebebi</label>
        <select class="form-control" v-model="reason">
          <option value="" disabled>Lütfen seçiniz</option>
          <option value="alisveris">Yüklü Alışveriş</option>
          <option value="gelir_artisi">Gelir Artışı</option>
          <option value="diger">Diğer</option>
        </select>
      </div>

      <button class="btn btn-primary w-100" @click="submitRequest">Talebi Gönder</button>
      <p class="text-muted text-center mt-3 text-sm">Başvurunuz anında skorlama sistemimize gönderilecektir.</p>
    </div>
  </div>
</template>
