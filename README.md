<div align="center">

![SentimentHub Banner](docs/screenshots/banner.png)

# SentimentHub

### E-Ticaret Platformlarındaki Kullanıcı Yorumlarının Yapay Zekâ ile Analizi ve Karşılaştırmalı İş Zekâsı Sistemi

<br/>

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_MVC-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-.NET_8-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/tr-tr/dotnet/csharp/)
[![Python](https://img.shields.io/badge/Python-3.11-3776AB?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-Mikro_Servis-009688?style=for-the-badge&logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![SQLite](https://img.shields.io/badge/SQLite-EF_Core-003B57?style=for-the-badge&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Selenium](https://img.shields.io/badge/Selenium-WebDriver-43B02A?style=for-the-badge&logo=selenium&logoColor=white)](https://www.selenium.dev/)
[![LM Studio](https://img.shields.io/badge/LM_Studio-Yerel_LLM-FF6B35?style=for-the-badge)](https://lmstudio.ai/)

<br/>

> **Lisans Tezi Uygulama Çıktısı**  
> Bursa Uludağ Üniversitesi · İnegöl İşletme Fakültesi · Yönetim Bilişim Sistemleri Bölümü · 2026  
> **Geliştirici:** Aynur Taşkeser Mekselina

---

[Problem Tanımı](#problem-tanımı) · [Çözüm](#çözüm) · [Sistem Mimarisi](#sistem-mimarisi) · [Yapay Zekâ Akışı](#yapay-zekâ-akışı) · [Temel Özellikler](#temel-özellikler) · [AnalysisEnrichmentService](#analysisenrichmentservice) · [Karşılaştırma Motoru](#karşılaştırmalı-iş-zekâsı-motoru) · [Kurulum](#kurulum)

</div>

---

## Tez Bilgisi

Bu yazılım sistemi aşağıdaki lisans tezinin uygulama çıktısı olarak geliştirilmiştir:

> **"E-Ticaret Platformlarındaki Kullanıcı Yorumlarının Yapay Zekâ ile Analizi ve Karşılaştırmalı İş Zekâsı Sistemi Geliştirilmesi: SentimentHub Uygulaması"**

| Alan | Bilgi |
|------|-------|
| Üniversite | Bursa Uludağ Üniversitesi |
| Fakülte | İnegöl İşletme Fakültesi |
| Bölüm | Yönetim Bilişim Sistemleri |
| Tür | Lisans Tezi |
| Yıl | 2026 |
| Geliştirici | Aynur Taşkeser Mekselina |

---

## Problem Tanımı

Türkiye'de e-ticaret sektörü hızla büyümekte, Trendyol gibi platformlarda milyonlarca ürün yorumu gün geçtikçe birikmeye devam etmektedir. Bu veri kütlesinin beraberinde getirdiği başlıca zorluklar şunlardır:

- Bir ürün sayfasında yüzlerce hatta binlerce yorum bulunmakta; bunların tamamını manuel olarak okuyup değerlendirmek son kullanıcı için **pratik olarak mümkün değildir.**
- Mevcut yıldız derecelendirme sistemleri yorum içeriğinin nüanslarını yansıtmaktan uzak, tek boyutlu metrikler sunmaktadır.
- Kargo, ürün kalitesi, satıcı davranışı ve fiyat/performans gibi **farklı müşteri kaygıları** tek bir genel puan altında görünmez hâle gelmektedir.
- İşletmeler ve satıcılar için rakip ürünlerle **sistematik karşılaştırma yapabilecek** bir araç piyasada yaygın biçimde mevcut değildir.
- Müşteri memnuniyetindeki **erken bozulma sinyalleri** fark edilemeden geçiştirilmekte, bu durum satış kayıplarına yol açmaktadır.

---

## Çözüm

**SentimentHub**, bu sorunlara bütünleşik bir çözüm sunmak amacıyla tasarlanmış, yapay zekâ destekli bir karar destek sistemidir. Sistem şu temel işlevleri yerine getirmektedir:

1. Trendyol ürün sayfasındaki yorumları **otomatik olarak** toplayıp sisteme aktarır.
2. Her yorumu yerel büyük dil modeli (LLM) aracılığıyla **Olumlu / Olumsuz / Nötr** olarak sınıflandırır ve kategori etiketiyle ilişkilendirir.
3. `AnalysisEnrichmentService` bileşeni aracılığıyla ham analiz verilerini **kök neden analizi, risk erken uyarı sistemi, fiyat algısı analizi, tavsiye etme eğilimi ve müşteri sadakati** gibi katmanlı çıktılara dönüştürür.
4. İki veya üç ürünün analizini **Karşılaştırmalı İş Zekâsı Motoru** ile işleyerek kazananı, karar güven skorunu, kategori dominansını ve alıcı persona uyumunu belirler.
5. Tüm bu bulguları görsel grafikler, yapılandırılmış tablolar ve indirilebilir **PDF raporlar** aracılığıyla sunar.

---

## Temel Özellikler

### Veri Toplama
- Trendyol ürün URL'si üzerinden Selenium WebDriver ile otomatik yorum kazıma
- Başsız (headless) Chrome tarayıcı ile otomasyon tespiti engelleme
- Ürün adı, yorum metni, yazar, tarih ve yıldız puanı alanlarının ayrıştırılması
- URL normalizasyonu: `/yorumlar` yönlendirmesi otomatik uygulanır
- Yinelenen yorum tespiti ve temizleme

### Yapay Zekâ Destekli Duygu Analizi
- LM Studio üzerinde çalışan yerel LLM ile her yorum için **Sentiment (Olumlu/Olumsuz/Nötr)** sınıflandırması
- Düşük token limiti ve `temperature=0.1` ile deterministik çıktı üretimi
- LLM yanıtından Markdown temizleme ve JSON normalize etme
- Yanıt ayrıştırma hatalarında güvenli geri dönüş (`fallback`) mekanizması

### Aspect-Based Sentiment Analysis (ABSA)
Her yorum, içerik analizine göre beş kategoriden biriyle etiketlenir:

| Kategori Kodu | Türkçe Karşılığı |
|---|---|
| `ProductQuality` | Ürün Kalitesi |
| `PricePerformance` | Fiyat / Performans |
| `Delivery` | Kargo ve Teslimat |
| `PackagingSeller` | Satıcı / Paketleme |
| `GeneralSatisfaction` | Genel Memnuniyet |

### Kategori Bazlı Puanlama
LLM'in 5 kategorideki performansı bağımsız olarak puanlandırılır (1.0–5.0 aralığında, ondalıklı). Her kategori için güçlü yönler ve zayıf yönler listesi üretilir.

### Kök Neden Analizi
`AnalysisEnrichmentService` bünyesinde, LLM'in tespit ettiği zayıf yönler kural tabanlı eşleştirme ile işlem yapılabilir iyileştirme önerilerine dönüştürülür.

### Risk Erken Uyarı Sistemi
Geçmiş analizlerle karşılaştırmalı trend takibi yapılarak üç seviyeli risk sinyali üretilir: **Düşük / Orta / Yüksek**.

### Fiyat Algısı Analizi
Fiyat/Performans kategorisindeki yorum yoğunluğuna ve duygu dağılımına dayalı fiyat algısı skoru hesaplanır.

### Tavsiye Etme Eğilimi Analizi
NPS (Net Promoter Score) mantığına dayalı tavsiye etme potansiyeli hesaplanır; destekleyen, kararsız ve olumsuz kullanıcı oranları ayrıştırılır.

### Müşteri Sadakati Analizi
Yorum metinlerinde regex tabanlı tekrar satın alma sinyalleri (`"tekrar aldım"`, `"favorim"`, `"aylardır kullanıyorum"` vb.) taranarak sadakat düzeyi belirlenir.

### Karşılaştırmalı İş Zekâsı Modülü
İki veya üç ürün analizi yan yana karşılaştırılır; kazanan ürün, karar güven skoru, kategori dominansı, alıcı persona uyumu ve geçiş öngörüsü hesaplanır.

### PDF Rapor Oluşturma
Hem tekil analiz raporu hem de karşılaştırma raporu indirilebilir PDF formatında sunulur.

---

## Sistem Mimarisi

SentimentHub, sorumlulukları net biçimde ayrılmış iki katmanlı bir yapı üzerine inşa edilmiştir:

```
┌──────────────────────────────────────────────────────────────────┐
│                          KULLANICI                               │
│                      (Web Tarayıcısı)                            │
└────────────────────────────┬─────────────────────────────────────┘
                             │ HTTPS
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│               SentimentHub.Web  —  ASP.NET Core MVC              │
│                                                                  │
│  ┌─────────────────────┐   ┌──────────────────────────────────┐  │
│  │     Controllers     │   │            Services              │  │
│  │                     │   │                                  │  │
│  │  AccountController  │   │  LmStudioService                 │  │
│  │  AnalysisController │   │  AnalysisEnrichmentService       │  │
│  │  ComparisonControl. │   │  RecommendationEngine            │  │
│  │  HomeController     │   │  PdfService                      │  │
│  └──────────┬──────────┘   │  PythonApiService                │  │
│             │              │  EmailSender                     │  │
│  ┌──────────▼──────────┐   └──────────────────────────────────┘  │
│  │       Views         │                                          │
│  │  (Razor / .cshtml)  │   ┌──────────────────────────────────┐  │
│  │                     │   │         Data Katmanı             │  │
│  │  Login / Register   │   │  Entity Framework Core + SQLite  │  │
│  │  Dashboard          │   │                                  │  │
│  │  Analysis Detail    │   │  ApplicationUser                 │  │
│  │  Comparison Result  │   │  Business                        │  │
│  └─────────────────────┘   │  Analysis                        │  │
│                            │  Review + AspectResult           │  │
│                            │  ComparisonReport                │  │
│                            └──────────────────────────────────┘  │
└──────────────────────────┬───────────────────────────────────────┘
                           │ HTTP — localhost:8001
                           ▼
┌──────────────────────────────────────────────────────────────────┐
│               SentimentHub.AI  —  Python / FastAPI               │
│                                                                  │
│  ┌────────────────────────┐    ┌─────────────────────────────┐  │
│  │      scraper.py        │    │        sentiment.py          │  │
│  │                        │    │                             │  │
│  │  Selenium WebDriver    │    │  OpenAI-compatible Client   │  │
│  │  BeautifulSoup4        │    │  Prompt Engineering         │  │
│  │  Headless Chrome       │    │  JSON Parse & Normalize     │  │
│  └────────────────────────┘    └─────────────────────────────┘  │
│                                                                  │
│                    main.py  (FastAPI Router)                     │
└──────────────────────────────┬───────────────────────────────────┘
                               │ OpenAI-compatible REST API
                               ▼
                  ┌────────────────────────────┐
                  │         LM Studio          │
                  │    Yerel LLM Sunucusu      │
                  │   localhost:1234/v1/        │
                  └────────────────────────────┘
```

---

## Yapay Zekâ Akışı

```
Kullanıcı Trendyol URL'si Girer
           │
           ▼
  ┌─────────────────────┐
  │   Python Scraper    │
  │  Selenium WebDriver │
  │  BeautifulSoup4     │
  │  URL Normalizasyon  │
  └────────┬────────────┘
           │  Ham yorum metinleri (JSON)
           ▼
  ┌─────────────────────┐
  │  Temizleme Katmanı  │
  │  Boş/tekrar yorum   │
  │  tespiti ve eleme   │
  └────────┬────────────┘
           │
           ▼
  ┌──────────────────────────────────────┐
  │         LLM Analiz Döngüsü           │
  │  Her yorum için LmStudioService      │
  │  POST /v1/chat/completions           │
  │  temperature=0.1 (deterministik)     │
  │  Çıktı: sentiment + category + score │
  └────────┬─────────────────────────────┘
           │
           ▼
  ┌──────────────────────────────────────┐
  │    LLM Özet Üretimi (Toplu)          │
  │  GenerateSummaryAsync                │
  │  İlk 50 yorum örneklenir             │
  │  Güçlü yön / zayıf yön              │
  │  5 kategori puanı (1.0–5.0)         │
  └────────┬─────────────────────────────┘
           │
           ▼
  ┌──────────────────────────────────────┐
  │   AnalysisEnrichmentService          │
  │                                      │
  │  1. Root Cause Analysis              │
  │  2. Risk Early Warning System        │
  │  3. Price Perception Analysis        │
  │  4. Recommendation Propensity        │
  │  5. Customer Loyalty Signals         │
  └────────┬─────────────────────────────┘
           │
           ▼
  ┌──────────────────────────────────────┐
  │        Raporlama Katmanı             │
  │  Dashboard Grafikleri (Chart.js)     │
  │  Yorum Listesi + Filtreler           │
  │  PDF Rapor (PdfService)              │
  └──────────────────────────────────────┘
```

---

## AnalysisEnrichmentService

`AnalysisEnrichmentService`, ham LLM çıktısını stratejik karar desteğine dönüştüren merkezi zenginleştirme katmanıdır. Analiz tamamlandıktan sonra aşağıdaki beş modülü sırayla çalıştırır ve sonuçları `SummaryJson` alanına seri hâlde kaydeder.

---

### Kök Neden Analizi (Root Cause Analysis)

LLM'in ürettiği `weaknesses` (zayıf yönler) listesi, `RecommendationEngine` sınıfı aracılığıyla kural tabanlı bir eşleştirme motorundan geçirilir. Her zayıf yön, anahtar kelime analizine göre aşağıdaki kategorilerden biriyle ilişkilendirilir ve işlem yapılabilir bir iyileştirme önerisi üretilir:

| Tespit Edilen Sorun Alanı | Üretilen Öneri Kategorisi |
|---|---|
| Kargo / Teslimat / Lojistik | Lojistik optimizasyonu ve paketleme standartları |
| Fiyat / Pahalı / Değer | Kampanya ve fiyat/performans yönetimi |
| Kalite / Malzeme / Dayanıklılık | Kalite kontrol süreç iyileştirmesi |
| İletişim / Müşteri Hizmetleri | Destek kapasitesi ve yanıt hızı |
| Beden / Kalıp uyumsuzluğu | Ürün açıklaması ve beden rehberi iyileştirmesi |
| İade / Değişim | İade politikası şeffaflığı |
| Yanlış / Eksik ürün | Depo ve sevkiyat doğrulama süreçleri |

Bu yapı, her analizin soyut bir "zayıf yön listesinin" ötesine geçerek **iş birimine doğrudan uygulanabilir** çıktılar üretmesini sağlar.

---

### Risk Erken Uyarı Sistemi (Risk Early Warning System)

Bu modül, mevcut analizi aynı ürünün önceki analiz sonuçlarıyla karşılaştırarak müşteri memnuniyetindeki bozulmayı erken tespit eder.

**Hesaplanan Metrikler:**

| Metrik | Açıklama |
|---|---|
| `negGrowth` | Olumsuz yorum oranının önceki analize göre yüzdesiyle artışı |
| `scoreDrop` | Genel memnuniyet puanındaki düşüş |
| `qualityDecline` | Ürün kalitesi kategorisi puanındaki düşüş |
| `shippingDecline` | Kargo kategorisi puanındaki düşüş |

**Risk Seviyeleri:**

| Seviye | Durum | Tetikleme Koşulu |
|---|---|---|
| Yüksek | Kritik | `scoreDrop ≥ 0.5` veya `negGrowth ≥ 15%` veya kategori düşüşü `≥ 0.8` |
| Orta | Dikkat | `scoreDrop ≥ 0.2` veya `negGrowth ≥ 5%` veya kategori düşüşü `≥ 0.3` |
| Düşük | Takip | Herhangi bir metrikte pozitif yönde sapma |
| Yok | Kararlı | Tüm metrikler stabil veya iyileşme eğiliminde |

İlk analiz durumunda (önceki analiz yoksa) mutlak olumsuz yorum oranı baz alınır:
- `negativeRate ≥ 25%` → Yüksek Risk
- `negativeRate ≥ 10%` → Orta Risk

---

### Fiyat Algısı Analizi (Price Perception Analysis)

`PricePerformance` kategorisindeki yorum oranı ve duygu dağılımı analiz edilerek ürünün fiyat/performans algısı ölçülür. Kategori puanı 1.0–5.0 aralığında hesaplanır. Bu puan, karşılaştırmalı iş zekâsı motorunda rakip ürünle kıyaslanacak bir delta değerine dönüştürülür.

---

### Tavsiye Etme Eğilimi Analizi (Recommendation Propensity Analysis)

NPS (Net Promoter Score) mantığına dayalı olarak üç segment hesaplanır:

```
Destekleyen (Promoters)  = Olumlu yorum oranı (%)
Kararsız (Passives)      = Nötr yorum oranı (%)
Olumsuz (Detractors)     = Olumsuz yorum oranı (%)
Tavsiye Etme Skoru       = max(0, min(100, Promoters%))
```

**Güven Düzeyi:** Yorum sayısına göre belirlenir.
- `≥ 10 yorum` → Yüksek Güven
- `5–9 yorum` → Orta Güven
- `< 5 yorum` → Düşük Güven

**Tavsiye Potansiyeli Seviyeleri:**
- `≥ 75%` → Yüksek
- `45–74%` → Orta
- `< 45%` → Düşük

---

### Müşteri Sadakati Analizi (Customer Loyalty Analysis)

Yorum metinleri üzerinde düzenli ifade (regex) taraması yapılarak tekrar satın alma ve uzun süreli kullanım sinyalleri tespit edilir.

**Tespit Edilen Sadakat Örüntüleri:**

```
"tekrar aldım" | "tekrar sipariş" | "yeniden sipariş" | "ikinci kez"
"2. sipariş" | "3. sipariş" | "favorim" | "uzun süredir"
"aylardır" | "yıllardır" | "sürekli kullanıyorum"
```

**Sadakat Düzeyleri:**

| Düzey | Koşul |
|---|---|
| Güçlü | `≥ 5` sadakat sinyali içeren yorum tespit edildi |
| Orta | `2–4` sadakat sinyali içeren yorum tespit edildi |
| Zayıf | `< 2` — Yeterli veri yok |

---

## Karşılaştırmalı İş Zekâsı Motoru

`ComparisonController` ve `ComparisonViewModel` bileşenlerinden oluşan bu modül, iki veya üç ürünün analizini sistematik biçimde karşılaştırarak iş zekâsı çıktıları üretir.

---

### Kazanan Tespiti (Winner Detection)

Her ürünün `OverallScore` değeri karşılaştırılır. Fark `≥ 0.5` ise **Dominant Win**, `0.2–0.49` aralığında ise **Yakın Rekabet**, `< 0.2` ise **Beraberlik** olarak sınıflandırılır.

### Karar Güven Skoru (Decision Confidence Score)

Karşılaştırma sonucunun güvenilirliğini ölçen bileşik bir metriktir. Üç alt faktörden türetilir:

| Bileşen | Ağırlık | Açıklama |
|---|---|---|
| Skor Farkı Etkisi | %67 | İki ürün arasındaki genel puan farkının normalize edilmiş değeri |
| Kategori Dominansı | %80 | Kazanan ürünün kaç kategoride önde olduğunun oranı |
| Duygu Tutarlılığı | %83 | Kazanan ürünün duygu dağılımının iç tutarlılığı |

### Kategori Dominansı ve Delta Analizi

Her kategori (`ProductQuality`, `PricePerformance`, `Delivery`, `PackagingSeller`, `UsageExperience`) için iki ürün arasındaki puan farkı (Δ) hesaplanır ve ağırlık etiketiyle sunulur:
- `|Δ| ≥ 1.0` → **Ayırt Edici Fark** — ürünleri birbirinden belirgin biçimde ayıran kategori
- `|Δ| ≥ 0.5` → **Yüksek Ağırlık**
- `|Δ| < 0.5` → **Orta Ağırlık**

### Alıcı Persona Uyumu (Persona Matching)

Farklı müşteri profillerinin hangi ürünü tercih edeceği, kategori puanları üzerinden belirlenir:

| Persona | Belirleyici Kategori |
|---|---|
| Premium Alıcı | Ürün Kalitesi + Kullanıcı Deneyimi |
| Fiyat/Performans Alıcısı | Fiyat/Performans |
| Hızlı Kargo Alıcısı | Kargo ve Teslimat |
| Güvenilir Satıcı Alıcısı | Satıcı/Paketleme |
| Uzun Vadeli Kullanıcı | Genel Memnuniyet ortalaması |

### Karar Gerekçesi (Decision Intelligence)

Karşılaştırma sonucu, kazanan ürünün güçlü olduğu kategori sayısı, olumlu duygu farkı ve genel skor üstünlüğü temel alınarak yazılı bir karar özeti olarak üretilir.

### Geçiş Öngörüsü (Switching Prediction)

Kaybeden ürünün kullanıcılarının kazanan ürüne geçtiğinde hangi alanlarda iyileşme elde edeceği, her iki ürünün güçlü/zayıf yönleri karşılaştırılarak listelenir.

---

## Veritabanı Yapısı

Entity Framework Core ile yönetilen, SQLite üzerinde çalışan ilişkisel veri modeli:

```
ApplicationUser  (ASP.NET Identity)
    │
    └── Business  (1:N)
            │  OwnerId → ApplicationUser.Id
            │  Name, ProductUrl, CreatedAt
            │
            └── Analysis  (1:N)
                    │  BusinessId → Business.Id
                    │  Date, Status, TotalReviews, OverallScore
                    │  SummaryJson  ← AnalysisEnrichmentService çıktısı (JSON)
                    │
                    └── Review  (1:N)
                            │  AnalysisId → Analysis.Id
                            │  Text, AuthorName, ReviewDate, Rating
                            │  Sentiment (Positive/Negative/Neutral)
                            │  ConfidenceScore
                            │
                            └── AspectResult  (1:N)
                                    ReviewId → Review.Id
                                    Aspect (AspectType enum)
                                    Sentiment, Confidence

ComparisonReport  (Bağımsız)
    AnalysisIds  ← JSON dizisi: "[1, 5, 8]"
    ResultJson   ← ComparisonViewModel (JSON)
    Name, CreatedAt
```

**Enum Tanımları:**

```csharp
public enum SentimentType  { Positive, Negative, Neutral }

public enum AspectType
{
    ProductQuality,      // Ürün Kalitesi
    PricePerformance,    // Fiyat / Performans
    Delivery,            // Kargo ve Teslimat
    PackagingSeller,     // Satıcı / Paketleme
    GeneralSatisfaction  // Genel Memnuniyet
}

public enum AnalysisStatus { Pending, Processing, Completed, Failed }
```

---

## Proje Klasör Yapısı

```
SentimentHub/
│
├── SentimentHub/
│   │
│   ├── SentimentHub.Web/                  ASP.NET Core MVC Projesi
│   │   ├── Controllers/
│   │   │   ├── AccountController.cs       Kayıt, giriş, e-posta doğrulama
│   │   │   ├── AnalysisController.cs      Analiz başlatma, yönetim, raporlama
│   │   │   ├── ComparisonController.cs    Karşılaştırma motoru ve PDF indirme
│   │   │   └── HomeController.cs          Ana sayfa
│   │   │
│   │   ├── Models/
│   │   │   ├── Analysis.cs                Analiz varlık modeli
│   │   │   ├── Review.cs                  Yorum ve AspectResult modelleri
│   │   │   ├── Business.cs                İşletme/ürün modeli
│   │   │   ├── ComparisonReport.cs        Karşılaştırma raporu modeli
│   │   │   ├── ComparisonViewModel.cs     Karşılaştırma görünüm modeli
│   │   │   ├── ApplicationUser.cs         Identity kullanıcı modeli
│   │   │   └── Enums.cs                   SentimentType, AspectType, AnalysisStatus
│   │   │
│   │   ├── Services/
│   │   │   ├── LmStudioService.cs         LLM API entegrasyonu (OpenAI compat.)
│   │   │   ├── AnalysisEnrichmentService  5 katmanlı zenginleştirme motoru
│   │   │   ├── RecommendationEngine.cs    Kural tabanlı öneri motoru
│   │   │   ├── PdfService.cs              PDF rapor oluşturucu
│   │   │   ├── PythonApiService.cs        Python AI servis istemcisi
│   │   │   └── EmailSender.cs             SMTP e-posta doğrulama servisi
│   │   │
│   │   ├── Views/                         Razor (.cshtml) şablonları
│   │   ├── Data/                          ApplicationDbContext
│   │   ├── DTOs/                          Veri transfer nesneleri
│   │   ├── Migrations/                    EF Core migration dosyaları
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   └── SentimentHub.AI/                   Python FastAPI Mikro Servisi
│       ├── main.py                        FastAPI uygulama giriş noktası
│       ├── scraper.py                     Selenium tabanlı Trendyol kazıyıcı
│       ├── sentiment.py                   LM Studio duygu analizi modülü
│       ├── requirements.txt               Python bağımlılıkları
│       └── .env                           Ortam değişkenleri (git dışında)
│
├── docs/
│   └── screenshots/                       Uygulama ekran görüntüleri
│
├── validation_dataset/                    Model doğrulama veri seti
├── analyze_findings.py                    Doğrulama bulguları analiz scripti
├── generate_validation_dataset.py         Veri seti üretici
├── check_db_data.py                       Veritabanı denetim scripti
├── inspect_db.py                          Veritabanı inceleme scripti
└── baslat.bat                             Windows hızlı başlatma scripti
```

---

## Teknoloji Yığını

### Backend

| Teknoloji | Sürüm | Kullanım Amacı |
|---|---|---|
| C# | .NET 8 | Ana programlama dili |
| ASP.NET Core MVC | 8.0 | Web uygulama çerçevesi |
| Entity Framework Core | 8.0 | ORM ve veritabanı yönetimi |
| ASP.NET Identity | 8.0 | Kimlik doğrulama ve yetkilendirme |
| SQLite | — | Yerleşik ilişkisel veritabanı |
| PdfService (QuestPDF) | — | PDF rapor oluşturma |
| MailKit / SMTP | — | E-posta doğrulama servisi |

### Yapay Zekâ Mikro Servisi

| Teknoloji | Sürüm | Kullanım Amacı |
|---|---|---|
| Python | 3.11 | Ana programlama dili |
| FastAPI | 0.111 | REST API sunucusu |
| Uvicorn | 0.29 | ASGI web sunucusu |
| Selenium WebDriver | 4.21 | Web otomasyon ve veri kazıma |
| BeautifulSoup4 | 4.12 | HTML ayrıştırma |
| OpenAI Python SDK | 1.30 | LM Studio ile iletişim protokolü |
| python-dotenv | 1.0 | Ortam değişkenleri yönetimi |

### Yapay Zekâ Altyapısı

| Araç | Kullanım Amacı |
|---|---|
| LM Studio | Yerel LLM sunucusu (localhost:1234) |
| OpenAI-compatible API | LM Studio iletişim protokolü |
| Prompt Engineering | Deterministik JSON çıktı üretimi (temperature=0.1) |
| BERT (Yedek) | LLM erişimi mümkün olmadığında alternatif sınıflandırıcı |

### Frontend

| Teknoloji | Kullanım Amacı |
|---|---|
| HTML5 / CSS3 | Yapı ve stil katmanı |
| JavaScript | Etkileşim ve dinamik içerik |
| Bootstrap 5 | Duyarlı ızgara sistemi |
| Chart.js | Duygu dağılım grafikleri ve görselleştirmeler |

---

## Uygulama Ekran Görüntüleri

### Giriş ve Kayıt Ekranı
![Giriş Ekranı](docs/screenshots/login.png)
*ASP.NET Identity tabanlı kimlik doğrulama sistemi. E-posta doğrulama akışı ile güvenli oturum yönetimi.*

---

### Ana Dashboard
![Dashboard](docs/screenshots/dashboard.png)
*Kullanıcıya ait tüm analizlerin listelendiği, yeni analiz başlatma ve geçmiş raporlara erişim sağlayan merkezi panel.*

---

### Analiz Detay Sayfası — Yorum Görünümü
![Analiz Detayı](docs/screenshots/analysis.png)
*Her yorumun duygu durumu, aspect etiketi ve yapay zekâ güven skoru ile birlikte sunulduğu detay görünümü. Filtre seçenekleri: Tümü / Olumlu / Olumsuz / Nötr.*

---

### Karşılaştırmalı Analiz Raporu
![Karşılaştırma Raporu](docs/screenshots/report.png)
*İki ürünün Karar Güven Skoru, Kazanma Marjı, Kategori Delta Analizi, Alıcı Persona Uyumu ve Geçiş Öngörüsü ile karşılaştırıldığı iş zekâsı raporu.*

---

### Kayıtlı Analizler
![Kayıtlı Analizler](docs/screenshots/saved.png)
*Kullanıcıya ait geçmiş analizlerin yönetildiği, karşılaştırma seçimi yapılabilen panel.*

---

## Kurulum ve Çalıştırma

### Ön Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Python 3.11](https://www.python.org/downloads/)
- [LM Studio](https://lmstudio.ai/)
- [Google Chrome](https://www.google.com/chrome/) (Selenium için)

---

### 1. Depoyu Klonlayın

```bash
git clone https://github.com/taskesermekselina/SentimentHub.git
cd SentimentHub
```

---

### 2. LM Studio Yapılandırması

1. LM Studio uygulamasını açın.
2. Sol menüden uyumlu bir model indirin *(önerilen: Meta-Llama-3-8B-Instruct veya benzeri instruction-tuned model)*.
3. **Local Server** sekmesine geçin → **Start Server** düğmesine tıklayın.
4. Sunucunun `http://localhost:1234/v1` adresinde erişilebilir olduğunu doğrulayın.

---

### 3. Python AI Servisini Başlatın

```bash
cd SentimentHub/SentimentHub.AI

# Sanal ortam oluşturma
python -m venv venv
venv\Scripts\activate          # Windows
# source venv/bin/activate     # macOS / Linux

# Bağımlılıkların yüklenmesi
pip install -r requirements.txt

# Ortam değişkenlerinin yapılandırılması
copy .env.example .env
# .env dosyasındaki LM_STUDIO_URL değerini kontrol edin

# Servisin başlatılması
uvicorn main:app --host 0.0.0.0 --port 8001 --reload
```

Servis `http://localhost:8001` adresinde hazır olacaktır.

---

### 4. ASP.NET Core Web Uygulamasını Başlatın

```bash
cd SentimentHub/SentimentHub.Web

# Veritabanı migration uygulaması
dotnet ef database update

# Uygulamanın başlatılması
dotnet run
```

Uygulama `https://localhost:5001` adresinde erişilebilir olacaktır.

---

### Hızlı Başlatma (Windows)

```bat
baslat.bat
```

---

## Özgün Katkılar

Bu proje kapsamında geliştirilen özgün akademik ve teknik katkılar şunlardır:

**AnalysisEnrichmentService Mimarisi**
Ham LLM çıktısını beş katmanlı (kök neden analizi, risk erken uyarı, fiyat algısı, tavsiye eğilimi, müşteri sadakati) stratejik çıktıya dönüştüren çok katmanlı servis tasarımı.

**Risk Tahmin Yaklaşımı**
Zamansal analiz karşılaştırması yoluyla müşteri memnuniyetindeki bozulmayı erken tespit eden, üç seviyeli çok boyutlu risk skoru hesaplama yöntemi.

**Yerel LLM Kullanımı**
Veri gizliliğini koruyacak biçimde yerel LLM altyapısının kullanılması; bulut bağımlılığının ortadan kaldırılması ve maliyet optimizasyonu.

**Karşılaştırmalı İş Zekâsı Motoru**
Kategori bazlı delta analizi, alıcı persona uyumu ve geçiş öngörüsünü birleştiren çok boyutlu ürün karşılaştırma ve karar destek sistemi.

**Persona Uyum Analizi**
Farklı müşteri profillerinin (premium alıcı, fiyat/performans odaklı, hızlı kargo öncelikli vb.) hangi ürünü tercih edeceğini kategori puanlarından türeten profil eşleştirme modeli.

---

## Gelecek Çalışmalar

| Alan | Açıklama |
|---|---|
| Çoklu Platform Desteği | Amazon, Hepsiburada, n11 gibi diğer e-ticaret platformlarının entegrasyonu |
| Çoklu Dil Desteği | Türkçe odaklı yapının çok dilli analize genişletilmesi; dil tespiti ve çeviri katmanı |
| Tahminsel Analiz | Geçmiş analizlere dayalı müşteri memnuniyeti trendini öngören zaman serisi modelleri |
| Gelişmiş LLM Entegrasyonları | GPT-4, Claude ve özelleştirilmiş domain modelleri ile karşılaştırmalı değerlendirme |
| Gerçek Zamanlı İzleme | Belirli aralıklarla otomatik yeniden analiz ve anlık bildirim sistemi |
| API Katmanı | Üçüncü taraf sistemlere analiz verisi sağlayan RESTful API altyapısı |

---

## Tanıtım Videosu

[![Proje Tanıtım Videosu](https://img.youtube.com/vi/iRkfqZ7xucA/maxresdefault.jpg)](https://www.youtube.com/watch?v=iRkfqZ7xucA)

[YouTube'da İzlemek İçin Tıklayın](https://www.youtube.com/watch?v=iRkfqZ7xucA)

---

<div align="center">

**SentimentHub** · Lisans Tezi Uygulama Çıktısı · 2026

Bursa Uludağ Üniversitesi · İnegöl İşletme Fakültesi · Yönetim Bilişim Sistemleri

Aynur Taşkeser Mekselina

</div>
