<script setup lang="ts">
import { ref, computed } from 'vue';
import { 
  FileText, Download, Search, Filter, 
  FileCheck, FileClock, ShieldCheck, 
  ChevronRight, Calendar, HardDrive
} from 'lucide-vue-next';
import { useToastStore } from '../store/toast';

const toast = useToastStore();
const searchQuery = ref('');

const docs = ref([
  { id: 1, type: 'statement', name: 'Nisan 2024 Hesap Özeti', filename: 'Nisan_2024_Hesap_Ozeti.pdf', date: '01.04.2024', size: '1.2 MB', status: 'ready' },
  { id: 2, type: 'contract', name: 'Bireysel Bankacılık Sözleşmesi', filename: 'NexBank_Bireysel_Sozlesme.pdf', date: '15.01.2024', size: '3.4 MB', status: 'signed' },
  { id: 3, type: 'receipt', name: 'Dekont - TR9800045612', filename: 'Dekont_TR98000.pdf', date: '10.03.2024', size: '0.8 MB', status: 'ready' },
  { id: 4, type: 'statement', name: 'Mart 2024 Hesap Özeti', filename: 'Mart_2024_Hesap_Ozeti.pdf', date: '01.03.2024', size: '1.1 MB', status: 'ready' },
  { id: 5, type: 'contract', name: 'Kredi Kartı Üyelik Formu', filename: 'Kredi_Karti_Sozlesmesi.pdf', date: '20.02.2024', size: '2.1 MB', status: 'signed' },
]);

const filteredDocs = computed(() => {
    return docs.value.filter(d => 
        d.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    );
});

// GERÇEK İNDİRME FONKSİYONU
const handleDownload = (doc: any) => {
    toast.info(`${doc.name} hazırlanıyor...`);
    
    // Simüle edilmiş PDF içeriği oluşturma
    const content = `NexBank Digital Archive\nDocument: ${doc.name}\nDate: ${doc.date}\nRef: ${doc.filename}\n\nThis is a simulated document content for NexBank Premium Banking.`;
    const blob = new Blob([content], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = doc.filename;
    document.body.appendChild(link);
    link.click();
    
    // Temizlik
    setTimeout(() => {
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        toast.success(`${doc.name} başarıyla indirildi.`);
    }, 1000);
};

const getIcon = (type: string) => {
    switch (type) {
        case 'statement': return FileClock;
        case 'contract': return FileCheck;
        case 'receipt': return FileText;
        default: return FileText;
    }
};

const getTypeLabel = (type: string) => {
    switch (type) {
        case 'statement': return 'Hesap Özeti';
        case 'contract': return 'Sözleşme';
        case 'receipt': return 'Dekont';
        default: return 'Belge';
    }
};
</script>

<template>
  <div class="view-container fade-in">
    <header class="docs-header mb-4">
      <div class="h-left">
        <h1 class="text-gradient-gold">Dijital Arşiv</h1>
        <p class="subtitle">NexBank güvencesiyle tüm belgeleriniz ve sözleşmeleriniz tek bir noktada.</p>
      </div>
      <div class="h-right">
        <div class="storage-mini-card">
            <HardDrive :size="18" class="text-gold" />
            <div class="sm-info">
                <span>DİJİTAL KASA</span>
                <strong>4.2 GB / 10 GB</strong>
            </div>
        </div>
      </div>
    </header>

    <!-- SEARCH & ACTIONS -->
    <div class="docs-toolbar mb-4">
        <div class="search-box-premium">
            <Search :size="18" />
            <input type="text" v-model="searchQuery" placeholder="Belge ismi ile ara..." />
        </div>
        <div class="toolbar-actions">
            <button class="btn-tool"><Filter :size="18" /> Filtrele</button>
        </div>
    </div>

    <div class="docs-grid">
        <div v-for="d in filteredDocs" :key="d.id" class="doc-card-premium">
            <div class="dc-icon-side">
                <div class="dc-icon-wrapper" :class="d.type">
                    <component :is="getIcon(d.type)" :size="24" />
                </div>
            </div>
            
            <div class="dc-main-side">
                <div class="dc-top">
                    <span class="dc-tag">{{ getTypeLabel(d.type) }}</span>
                    <span class="dc-date">
                        <Calendar :size="12" />
                        {{ d.date }}
                    </span>
                </div>
                <h3 class="dc-title">{{ d.name }}</h3>
                <div class="dc-footer">
                    <span class="dc-size">{{ d.size }}</span>
                    <span class="dc-status" v-if="d.status === 'signed'">
                        <ShieldCheck :size="12" /> İmzalı
                    </span>
                </div>
            </div>

            <div class="dc-action-side">
                <button @click="handleDownload(d)" class="btn-download-premium" title="Bilgisayara İndir">
                    <Download :size="20" />
                    <span>İndir</span>
                </button>
            </div>
        </div>
        
        <div v-if="filteredDocs.length === 0" class="empty-docs">
            <FileText :size="48" />
            <p>Aradığınız belge bulunamadı.</p>
        </div>
    </div>
  </div>
</template>

<style scoped>
.docs-header { display: flex; justify-content: space-between; align-items: flex-end; }
.text-gradient-gold { 
    background: linear-gradient(135deg, #D4AF37 0%, #F1D279 50%, #B8860B 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    font-size: 2.2rem; font-weight: 900;
}
.text-gold { color: #D4AF37; }

.storage-mini-card { 
    background: #0F172A; padding: 12px 20px; border-radius: 16px; 
    display: flex; align-items: center; gap: 16px; color: white;
}
.sm-info span { display: block; font-size: 0.6rem; color: rgba(255,255,255,0.4); font-weight: 800; letter-spacing: 1px; }
.sm-info strong { font-size: 0.9rem; color: #D4AF37; font-family: 'Outfit'; }

.docs-toolbar { display: flex; gap: 16px; align-items: center; }
.search-box-premium { 
    flex: 1; background: white; border: 1px solid #F1F5F9; border-radius: 14px; 
    padding: 12px 20px; display: flex; align-items: center; gap: 12px; color: #94A3B8;
}
.search-box-premium input { border: none; outline: none; width: 100%; font-weight: 600; color: #0F172A; }

.btn-tool { 
    background: white; border: 1px solid #F1F5F9; padding: 12px 20px; 
    border-radius: 14px; font-weight: 700; color: #64748B; cursor: pointer;
    display: flex; align-items: center; gap: 8px; transition: 0.2s;
}
.btn-tool:hover { background: #F8FAFC; border-color: #0F172A; color: #0F172A; }

.docs-grid { display: flex; flex-direction: column; gap: 12px; }

.doc-card-premium { 
    background: white; border-radius: 20px; padding: 20px; 
    border: 1px solid #F1F5F9; display: flex; align-items: center; 
    gap: 20px; transition: 0.3s;
}
.doc-card-premium:hover { transform: scale(1.01); border-color: #D4AF37; box-shadow: 0 10px 30px rgba(0,0,0,0.03); }

.dc-icon-wrapper { 
    width: 56px; height: 56px; border-radius: 16px; display: flex; 
    align-items: center; justify-content: center;
}
.dc-icon-wrapper.statement { background: #E0F2FE; color: #0369A1; }
.dc-icon-wrapper.contract { background: #FEF3C7; color: #B45309; }
.dc-icon-wrapper.receipt { background: #DCFCE7; color: #15803D; }

.dc-main-side { flex: 1; }
.dc-top { display: flex; gap: 12px; align-items: center; margin-bottom: 6px; }
.dc-tag { font-size: 0.65rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.5px; opacity: 0.6; }
.dc-date { font-size: 0.75rem; color: #94A3B8; font-weight: 700; display: flex; align-items: center; gap: 4px; }

.dc-title { font-size: 1.1rem; font-weight: 800; color: #0F172A; margin-bottom: 6px; }

.dc-footer { display: flex; gap: 16px; align-items: center; }
.dc-size { font-size: 0.75rem; color: #94A3B8; font-weight: 800; }
.dc-status { font-size: 0.7rem; color: #10B981; font-weight: 800; display: flex; align-items: center; gap: 4px; }

.btn-download-premium { 
    background: #0F172A; color: #D4AF37; border: none; padding: 12px 24px; 
    border-radius: 14px; font-weight: 900; display: flex; align-items: center; 
    gap: 10px; cursor: pointer; transition: 0.2s;
}
.btn-download-premium:hover { background: #000; transform: translateY(-2px); box-shadow: 0 10px 20px rgba(0,0,0,0.1); }

.empty-docs { padding: 80px 0; text-align: center; color: #94A3B8; }
.empty-docs p { margin-top: 12px; font-weight: 700; }
</style>
