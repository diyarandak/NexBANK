# 🏦 NexBank — Premium Dijital Bankacılık Platformu

> **Yazılım Tasarım Desenleri** dersi projesi kapsamında geliştirilen, ileri düzey mimariler ve gerçek zamanlı teknolojilerle donatılmış full-stack bankacılık simülasyonu.

## 🏗️ Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| **Backend** | .NET 9, ASP.NET Core Web API, **MediatR**, **SignalR**, **MemoryCache** |
| **Frontend** | Vue 3 + TypeScript + Vite + **SignalR Client** |
| **Veritabanı** | SQLite (nexbank.db — Kalıcı) |
| **Mimari** | Clean Architecture, CQRS (Mediator), Repository & Service Pattern |

## 🎨 Uygulanan Tasarım Desenleri (Design Patterns)

Bu proje, akademik gereksinimlerin ötesinde profesyonel yazılım geliştirme standartlarında tasarlanmıştır:

1.  **Mediator Pattern (MediatR):** Controller'lar ve Busines Logic arasındaki bağı (Coupling) tamamen koparır. Tüm istekler `_mediator.Send()` ile merkezi olarak yönetilir.
2.  **Decorator Pattern:** `CachedStockRepository` ile borsa verilerine "Caching" yeteneği eklenmiştir. Mevcut repository kodu değiştirilmeden (Open/Closed) performans artışı sağlanmıştır.
3.  **State Pattern:** Kredi süreçleri (Pending, Approved, Completed) durum sınıflarıyla yönetilir. `ILoanState` arayüzü sayesinde her durumun kendi iş mantığı (onaylama, ödeme vb.) kapsüllenmiştir.
4.  **Chain of Responsibility:** Para transferleri öncesindeki kontroller (Bakiye -> Limit -> Hesap Durumu) bir doğrulama zinciri üzerinden akıtılır.
5.  **Strategy Pattern:** Farklı transfer türleri (EFT, Havale, SWIFT) için değişen komisyon hesaplama mantığı strateji nesneleriyle soyutlanmıştır.
6.  **Command Pattern:** Finansal işlemler komut nesneleri olarak saklanır, bu sayede "İşlemi Geri Al" (Undo) özelliği desteklenir.
7.  **Factory Pattern:** Farklı hesap türleri (Individual, Gold, Corporate) merkezi bir fabrika sınıfı üzerinden tutarlı şekilde üretilir.
8.  **Observer Pattern:** 
    *   **Klasik:** İşlem sonuçları SMS ve E-posta gözlemcilerine dağıtılır.
    *   **Modern (Real-time):** SignalR Hub yapısı ile borsa fiyatları tüm kullanıcılara anlık (push) olarak basılır.
9.  **Repository Pattern:** Veri erişim katmanı soyutlanmış, veritabanı bağımlılığı minimize edilmiştir.

---

## ✅ TAMAMLANAN GELİŞMİŞ ÖZELLİKLER

- [x] **Real-time Borsa İstanbul** — SignalR ile saniyelik fiyat güncellemeleri.
- [x] **İleri Düzey Kredi Sistemi** — State pattern ile yönetilen onay ve taksit ödeme süreçleri.
- [x] **Güvenli Transfer Zinciri** — Chain of Responsibility ile fraud ve limit denetimi.
- [x] **Akıllı Caching** — Decorator pattern ile borsa verilerinin bellekten (MemoryCache) okunması.
- [x] **Modern UI/UX** — Premium Light Theme, TopHeader, Kategorize edilmiş Sidebar.
- [x] **Hisse Senedi Portföyü** — Anlık kar/zarar takibi ve lot yönetimi.
- [x] **Full IBAN Desteği** — TR standartlarında otomatik IBAN üretimi ve transferi.
- [x] **Kredi Kartı & Nakit Avans** — Limit yönetimi ve borç ödeme sistemi.

## 🚀 Çalıştırma Talimatları

### Backend
```bash
cd backend/NexBank.API
dotnet run
```
> API & SignalR Hub: `http://localhost:5157`

### Frontend
```bash
cd frontend
npm install
npm run dev
```
> UI: `http://localhost:5173`

---
*Bu çalışma Yazılım Tasarım Desenleri dersi için "Teknik Mükemmellik" hedefiyle geliştirilmiştir.*
