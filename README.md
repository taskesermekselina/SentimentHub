<div align="center">

![SentimentHub Banner](docs/screenshots/banner.png)

# SentimentHub

**E-ticaret ürün yorumlarını yapay zekâ ile analiz eden, rakip ürünleri karşılaştıran ve işletmelere veri odaklı karar desteği sunan açık kaynaklı iş zekâsı platformu.**

<br/>

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.11-3776AB?style=flat-square&logo=python&logoColor=white)](https://www.python.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-0.111-009688?style=flat-square&logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![SQLite](https://img.shields.io/badge/SQLite-EF_Core-003B57?style=flat-square&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![LM Studio](https://img.shields.io/badge/LM_Studio-Yerel_LLM-FF6B35?style=flat-square)](https://lmstudio.ai/)
[![Selenium](https://img.shields.io/badge/Selenium-4.21-43B02A?style=flat-square&logo=selenium&logoColor=white)](https://www.selenium.dev/)
[![Lisans](https://img.shields.io/badge/Lisans-MIT-22c55e?style=flat-square)](LICENSE)

</div>

---

## Overview

**SentimentHub**, Trendyol platformundaki ürün yorumlarını otomatik olarak toplayıp yerel büyük dil modeli (LLM) ile analiz eden, her yorumu duygu durumuna ve kategori etiketine göre sınıflandıran bir yapay zekâ destekli karar destek sistemidir.

Sistem yalnızca genel bir memnuniyet puanı üretmekle kalmaz; ürün kalitesi, kargo, satıcı davranışı ve fiyat/performans gibi boyutları bağımsız olarak değerlendirir. Rakip ürünleri karşılaştıran **Comparative Business Intelligence Engine** ile hangi ürünün hangi müşteri profiline daha uygun olduğunu belirler ve iş zekâsı raporları üretir.

E-ticaret satıcıları, pazar analistleri ve ürün yöneticileri için tasarlanmıştır. Veri gizliliğini korumak amacıyla tüm yapay zekâ işlemleri **yerel LLM altyapısı** üzerinde gerçekleştirilir; hiçbir yorum verisi dışarıya gönderilmez.

---

## Key Features

| Özellik | Açıklama |
|---|---|
| **AI Sentiment Analysis** | Her yorum LLM ile Olumlu / Olumsuz / Nötr olarak sınıflandırılır |
| **Aspect-Based Analysis** | 5 bağımsız kategori (Kalite, Kargo, Satıcı, Fiyat, Deneyim) ayrı ayrı puanlanır |
| **Root Cause Detection** | Zayıf yönler kural tabanlı motorla işlem yapılabilir önerilere dönüştürülür |
| **Risk Early Warning** | Geçmiş analizlerle kıyaslamalı 3 seviyeli risk uyarısı üretilir |
| **Price Perception Analysis** | Fiyat/Performans kategorisi bazlı müşteri algısı ölçülür |
| **Recommendation Propensity** | NPS mantığıyla tavsiye etme eğilimi hesaplanır |
| **Customer Loyalty Analysis** | Yorum metinlerinde tekrar satın alma sinyalleri taranır |
| **Comparative Business Intelligence** | 2–3 ürün karar güven skoru ve persona uyumuyla karşılaştırılır |
| **PDF Reporting** | Analiz ve karşılaştırma raporları indirilebilir PDF formatında sunulur |

---

## Screenshots

### Giriş Sayfası

![Login](docs/screenshots/login.png)

---

### Dashboard

![Dashboard](docs/screenshots/dashboard.png)

---

### Analiz Raporu

![Analysis](docs/screenshots/analysis.png)

---

### Müşteri Yorumları

![Reviews](docs/screenshots/saved.png)

---

### Karşılaştırmalı İş Zekâsı Motoru

![Comparison](docs/screenshots/report.png)

---

### Persona Uyumu & Karar Zekâsı

> *Ekran görüntüleri eklenecek.*

---

## How It Works

```
  Trendyol Ürün URL'si
          ↓
  Selenium WebDriver ile Otomatik Yorum Kazıma
          ↓
  Temizleme ve Tekrar Yorum Tespiti
          ↓
  LLM ile Duygu Sınıflandırması  (Olumlu / Olumsuz / Nötr)
          ↓
  Aspect Sınıflandırması  (Kalite / Kargo / Satıcı / Fiyat / Deneyim)
          ↓
  AnalysisEnrichmentService
  (Kök Neden · Risk Uyarısı · Fiyat Algısı · Tavsiye · Sadakat)
          ↓
  Karşılaştırmalı İş Zekâsı Motoru
          ↓
  Dashboard · Grafikler · PDF Rapor
```

---

## Technology Stack

### Backend

| Teknoloji | Sürüm | Rol |
|---|---|---|
| C# / ASP.NET Core MVC | .NET 8 | Web uygulama çerçevesi |
| Entity Framework Core | 8.0 | ORM ve veritabanı yönetimi |
| ASP.NET Identity | 8.0 | Kimlik doğrulama |
| SQLite | — | Yerleşik veritabanı |
| MailKit / SMTP | — | E-posta doğrulama |
| PdfService | — | PDF rapor oluşturma |

### AI Mikro Servisi

| Teknoloji | Sürüm | Rol |
|---|---|---|
| Python | 3.11 | Mikro servis dili |
| FastAPI + Uvicorn | 0.111 / 0.29 | REST API sunucusu |
| Selenium WebDriver | 4.21 | Web otomasyon |
| BeautifulSoup4 | 4.12 | HTML ayrıştırma |
| OpenAI Python SDK | 1.30 | LM Studio protokolü |

### Yapay Zekâ

| Araç | Rol |
|---|---|
| LM Studio | Yerel LLM sunucusu (`localhost:1234`) |
| OpenAI-compatible API | LM Studio iletişim katmanı |
| BERT (Yedek) | LLM erişimi olmadığında fallback sınıflandırıcı |

### Frontend

| Teknoloji | Rol |
|---|---|
| HTML5 / CSS3 / Bootstrap 5 | Duyarlı arayüz |
| JavaScript | Etkileşim katmanı |
| Chart.js | Grafik ve görselleştirme |

---

## System Architecture

```
┌──────────────────────────────────────────────────────────┐
│                        KULLANICI                         │
└───────────────────────────┬──────────────────────────────┘
                            │ HTTPS
                            ▼
┌──────────────────────────────────────────────────────────┐
│            SentimentHub.Web  —  ASP.NET Core MVC         │
│                                                          │
│  Controllers          Services              Views        │
│  ─────────────        ────────────          ──────────   │
│  Account              LmStudioService       Razor Pages  │
│  Analysis             EnrichmentService     Dashboard    │
│  Comparison           RecommendationEngine  Reports      │
│  Home                 PdfService            Comparison   │
│                       PythonApiService                   │
│                       EmailSender                        │
│                                                          │
│  ┌────────────────────────────────────────────────────┐  │
│  │         Entity Framework Core  +  SQLite           │  │
│  │  User · Business · Analysis · Review · Comparison  │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────┬───────────────────────────────┘
                           │ HTTP — localhost:8001
                           ▼
┌──────────────────────────────────────────────────────────┐
│           SentimentHub.AI  —  Python / FastAPI           │
│                                                          │
│   scraper.py                  sentiment.py               │
│   Selenium + BS4              OpenAI-compatible Client   │
│   Headless Chrome             Prompt Engineering         │
└──────────────────────────┬───────────────────────────────┘
                           │ OpenAI-compatible REST API
                           ▼
               ┌───────────────────────────┐
               │         LM Studio         │
               │   localhost:1234/v1/       │
               └───────────────────────────┘
```

---

## AI Pipeline

```
1. Yorum Metni Alınır
2. LmStudioService → POST /v1/chat/completions
   temperature=0.1  |  max_tokens=100  |  stream=false
3. Çıktı: { sentiment, category, score }
4. Toplu Özet → GenerateSummaryAsync (ilk 50 yorum)
   Güçlü yönler · Zayıf yönler · 5 kategori puanı
5. AnalysisEnrichmentService devreye girer
6. SummaryJson olarak veritabanına kaydedilir
```

LLM yanıtı Markdown içeriyorsa temizlenir. Ayrıştırma hatasında güvenli `fallback` devreye girer (`Neutral / GeneralSatisfaction / Score: 3`).

---

## AnalysisEnrichmentService

Ham LLM çıktısını beş katmanlı stratejik veriyle zenginleştiren merkezi servis. Analiz tamamlandıktan sonra sırayla çalışır.

---

### Root Cause Analysis

LLM'in ürettiği zayıf yönler, `RecommendationEngine` içindeki kural tabanlı motordan geçirilir. Anahtar kelime eşleştirmesiyle (kargo, fiyat, kalite, iade vb.) her zayıf yöne karşılık **işlem yapılabilir** bir iyileştirme önerisi üretilir.

---

### Risk Early Warning System

Mevcut analiz, aynı ürünün önceki analiz sonuçlarıyla karşılaştırılır.

| Seviye | Tetikleyici |
|---|---|
| **Yüksek / Kritik** | Skor düşüşü ≥ 0.5 veya negatif artış ≥ %15 |
| **Orta / Dikkat** | Skor düşüşü ≥ 0.2 veya negatif artış ≥ %5 |
| **Düşük / Takip** | Herhangi bir metrikte pozitif sapma |
| **Kararlı** | Tüm metrikler stabil |

İlk analizde mutlak olumsuz yorum oranı baz alınır.

---

### Price Perception Analysis

`PricePerformance` kategorisindeki yorum yoğunluğu ve duygu dağılımı analiz edilerek fiyat/performans algı skoru (1.0–5.0) hesaplanır. Karşılaştırma motorunda delta değerine dönüştürülür.

---

### Recommendation Propensity Analysis

NPS mantığıyla üç segment hesaplanır: **Destekleyen / Kararsız / Olumsuz**.  
Tavsiye skoru `max(0, min(100, Promoters%))` formülüyle türetilir.  
Yorum sayısına göre güven düzeyi atanır: Yüksek (≥10) · Orta (5–9) · Düşük (<5).

---

### Customer Loyalty Analysis

Yorum metinlerinde regex taramasıyla tekrar satın alma sinyalleri (`"tekrar aldım"`, `"favorim"`, `"aylardır"` vb.) tespit edilir.

- **Güçlü:** ≥ 5 sinyal
- **Orta:** 2–4 sinyal
- **Zayıf:** < 2 sinyal

---

## Comparative Business Intelligence Engine

2–3 ürün analizini sistemli biçimde karşılaştırarak iş zekâsı çıktıları üretir.

---

### Winner Detection

`OverallScore` karşılaştırması ile kazanan belirlenir.

| Fark | Sonuç |
|---|---|
| ≥ 0.5 | Dominant Win |
| 0.2 – 0.49 | Yakın Rekabet |
| < 0.2 | Beraberlik |

---

### Confidence Score

Üç bileşenden türetilen bileşik güven skoru:
- Skor farkı etkisi
- Kategori dominansı (kaç kategoride önde)
- Duygu tutarlılığı

---

### Category Delta Analysis

Her kategori için iki ürün arasındaki puan farkı (Δ) hesaplanır.

- `|Δ| ≥ 1.0` → Ayırt Edici Fark
- `|Δ| ≥ 0.5` → Yüksek Ağırlık
- `|Δ| < 0.5` → Orta Ağırlık

---

### Persona Matching

| Persona | Belirleyici Kategori |
|---|---|
| Premium Alıcı | Ürün Kalitesi + Kullanıcı Deneyimi |
| Fiyat/Performans Alıcısı | PricePerformance |
| Hızlı Kargo Alıcısı | Delivery |
| Güvenilir Satıcı Alıcısı | PackagingSeller |
| Uzun Vadeli Kullanıcı | Genel Ortalama |

---

### Switching Prediction & Decision Intelligence

Kaybeden ürünün kullanıcılarının kazanana geçişte elde edeceği avantajlar listelenir. Kazanan ürünün güçlü kategorileri, skor üstünlüğü ve duygu farkı temel alınarak yazılı bir **karar gerekçesi** üretilir.

---

## Database Structure

```
ApplicationUser  (ASP.NET Identity)
  │
  └── Business  (1:N)
        │  OwnerId, Name, ProductUrl, CreatedAt
        │
        └── Analysis  (1:N)
              │  Date, Status, TotalReviews, OverallScore
              │  SummaryJson  ←  EnrichmentService çıktısı
              │
              └── Review  (1:N)
                    │  Text, AuthorName, ReviewDate, Rating
                    │  Sentiment  (Positive / Negative / Neutral)
                    │  ConfidenceScore
                    │
                    └── AspectResult  (1:N)
                          Aspect  (ProductQuality / PricePerformance /
                                   Delivery / PackagingSeller /
                                   GeneralSatisfaction)
                          Sentiment, Confidence

ComparisonReport  (Bağımsız)
  AnalysisIds  ←  JSON dizi: "[1, 5, 8]"
  ResultJson   ←  ComparisonViewModel (JSON)
  Name, CreatedAt
```

---

## Project Structure

```
SentimentHub/
├── SentimentHub/
│   ├── SentimentHub.Web/              ASP.NET Core MVC
│   │   ├── Controllers/
│   │   │   ├── AccountController.cs   Kayıt · Giriş · E-posta Doğrulama
│   │   │   ├── AnalysisController.cs  Analiz Yönetimi · Raporlama
│   │   │   ├── ComparisonController   Karşılaştırma Motoru · PDF
│   │   │   └── HomeController.cs
│   │   ├── Models/                    Varlık Modelleri + Enum'lar
│   │   ├── Services/
│   │   │   ├── LmStudioService.cs     LLM API Entegrasyonu
│   │   │   ├── AnalysisEnrichmentService  5 Katmanlı Zenginleştirme
│   │   │   ├── RecommendationEngine   Kural Tabanlı Öneri Motoru
│   │   │   ├── PdfService.cs          PDF Oluşturma
│   │   │   ├── PythonApiService.cs    AI Servis İstemcisi
│   │   │   └── EmailSender.cs         SMTP
│   │   ├── Views/                     Razor Şablonları
│   │   ├── Data/                      DbContext
│   │   └── DTOs/
│   │
│   └── SentimentHub.AI/               Python Mikro Servis
│       ├── main.py                    FastAPI Router
│       ├── scraper.py                 Selenium Kazıyıcı
│       ├── sentiment.py               LM Studio İstemcisi
│       └── requirements.txt
│
├── docs/screenshots/                  Ekran Görüntüleri
├── validation_dataset/                Model Doğrulama Seti
├── analyze_findings.py
├── generate_validation_dataset.py
└── baslat.bat                         Windows Hızlı Başlatma
```

---

## Installation

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Python 3.11](https://www.python.org/downloads/)
- [LM Studio](https://lmstudio.ai/)
- [Google Chrome](https://www.google.com/chrome/)

---

### 1 — Depoyu Klonla

```bash
git clone https://github.com/taskesermekselina/SentimentHub.git
cd SentimentHub
```

### 2 — LM Studio

1. Bir model indir *(önerilen: Llama-3-8B-Instruct veya benzeri)*
2. **Local Server** → **Start Server**
3. `http://localhost:1234/v1` adresinde çalıştığını doğrula

### 3 — Python AI Servisi

```bash
cd SentimentHub/SentimentHub.AI

python -m venv venv
venv\Scripts\activate

pip install -r requirements.txt

uvicorn main:app --host 0.0.0.0 --port 8001 --reload
```

### 4 — Web Uygulaması

```bash
cd SentimentHub/SentimentHub.Web

dotnet ef database update
dotnet run
```

Uygulama `https://localhost:5001` adresinde açılır.

> **Windows için hızlı başlatma:** `baslat.bat`

---

## Future Work

- Çoklu platform desteği (Amazon TR, Hepsiburada, n11)
- Çok dilli analiz ve otomatik dil tespiti
- Zaman serisi tabanlı memnuniyet tahmin modeli
- Gerçek zamanlı izleme ve anlık bildirim sistemi
- Gelişmiş LLM entegrasyonları (GPT-4, Claude, domain modeller)
- Üçüncü taraf sistemler için RESTful API katmanı

---

## Thesis Information

> **"E-Ticaret Platformlarındaki Kullanıcı Yorumlarının Yapay Zekâ ile Analizi ve Karşılaştırmalı İş Zekâsı Sistemi Geliştirilmesi: SentimentHub Uygulaması"**

| | |
|---|---|
| Üniversite | Bursa Uludağ Üniversitesi |
| Fakülte | İnegöl İşletme Fakültesi |
| Bölüm | Yönetim Bilişim Sistemleri |
| Tür | Lisans Tezi |
| Yıl | 2026 |

**Özgün Akademik Katkılar**

- `AnalysisEnrichmentService` — 5 katmanlı zenginleştirme mimarisi
- Geçmiş analiz karşılaştırmasına dayalı çok boyutlu risk tahmin yaklaşımı
- Veri gizliliği korumalı yerel LLM altyapısı kullanımı
- Kategori delta analizi ve persona uyumuyla karşılaştırmalı iş zekâsı motoru
- NPS tabanlı tavsiye etme eğilimi hesaplama yöntemi

---

## Author

**Aynur Taşkeser Mekselina**

Bursa Uludağ Üniversitesi · İnegöl İşletme Fakültesi · Yönetim Bilişim Sistemleri

[![GitHub](https://img.shields.io/badge/GitHub-taskesermekselina-181717?style=flat-square&logo=github)](https://github.com/taskesermekselina)

---

<div align="center">

**SentimentHub** — Yapay Zekâ Destekli E-Ticaret Karar Destek Sistemi

</div>
