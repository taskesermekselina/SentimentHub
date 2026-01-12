import os
import json
import os
import json
from dotenv import load_dotenv

load_dotenv()

# Optional Imports (Handle missing libraries gracefully)
genai = None
OpenAI = None

try:
    import google.generativeai as genai
except ImportError as e:
    print(f"Warning: google-generativeai library not found. Gemini API will be unavailable. Error: {e}")

try:
    from openai import OpenAI
except ImportError:
    print("Warning: openai library not found. OpenAI API will be unavailable.")

# Initialize Local Models (Fallback)
sentiment_pipeline = None
try:
    from transformers import pipeline
    sentiment_pipeline = pipeline("sentiment-analysis", model="savasy/bert-base-turkish-sentiment-cased")
except Exception as e:
    print(f"Local AI Model skipped (not installed or compatible): {e}")
    sentiment_pipeline = None

def analyze_sentiment_with_api(text_list):
    """
    Analyzes a list of reviews using Google Gemini (Priority) or OpenAI.
    Returns a dictionary mapping text -> {sentiment, score, aspects}.
    """
    
    # 1. Try Google Gemini (Free Tier is great)
    google_key = os.getenv("GOOGLE_API_KEY")
    if google_key:
        try:
            genai.configure(api_key=google_key)
            model = genai.GenerativeModel('gemini-pro')
            
            prompt = """
            Aşağıdaki ürün/hizmet yorumlarını detaylıca analiz et. Çıktı olarak SADECE geçerli bir JSON listesi ver.
            Listedeki sıralamayı ve sayıyı bozma. Her yorum için şu yapıyı kullan:
            {
                "sentiment": "Positive" | "Negative" | "Neutral",
                "score": 0.95, (0.0 ile 1.0 arası güven skoru)
                "aspects": [ 
                    {"aspect": "Kargo", "sentiment": "Positive", "confidence": 0.9},
                    {"aspect": "Kalite", "sentiment": "Negative", "confidence": 0.8},
                    {"aspect": "Fiyat", "sentiment": "Neutral", "confidence": 0.6}
                ]
            }
            
            Kullanabileceğin Standart Aspect (Konu) Etiketleri: 
            ["Kargo", "Paketleme", "Kalite", "Fiyat", "Müşteri Hizmetleri", "Kullanım Kolaylığı", "Tasarım", "Orijinallik"]
            
            Eğer yorumda belirgin bir konu yoksa 'aspects' listesi boş olabilir.
            
            Yorumlar Listesi:
            """ + json.dumps(text_list, ensure_ascii=False)
            
            response = model.generate_content(prompt)
            # Clean generic json markdown
            clean_text = response.text.replace("```json", "").replace("```", "").strip()
            return json.loads(clean_text)
        except Exception as e:
            print(f"Gemini Error: {e}")

    # 2. Try OpenAI
    openai_key = os.getenv("OPENAI_API_KEY")
    if openai_key:
        try:
            client = OpenAI(api_key=openai_key)
            prompt = f"""
            Analyze these reviews and return a JSON array. For each review:
            {{
                "text_snippet": "start of review...",
                "sentiment": "Positive" | "Negative" | "Neutral",
                "score": 0.95,
                "aspects": [ {{"aspect": "service", "sentiment": "Negative", "confidence": 0.9}} ]
            }}
            
            Reviews: {json.dumps(text_list, ensure_ascii=False)}
            """
            
            completion = client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "user", "content": prompt}],
                response_format={ "type": "json_object" }
            )
            return json.loads(completion.choices[0].message.content).get("reviews", [])
        except Exception as e:
            print(f"OpenAI Error: {e}")
            
    return None

def analyze_sentiment(text):
    # Fallback to local
    if sentiment_pipeline:
        result = sentiment_pipeline(text[:512])[0]
        label_map = {
            "positive": "Positive",
            "negative": "Negative",
            "neutral": "Neutral"
        }
        return {
            "sentiment": label_map.get(result['label'], "Neutral"),
            "score": result['score']
        }
    return {"sentiment": "Neutral", "score": 0.5}

def analyze_aspects(text):
    # Basic keyword based aspect extraction if API fails
    aspects = []
    keywords = {
        "Kargo": ["kargo", "teslimat", "paket", "ulaşım", "hızlı", "geç", "gün"],
        "Kalite": ["kalite", "kumaş", "sağlam", "bozuk", "yırtık", "dikiş", "materyal"],
        "Beden/Uyum": ["beden", "kalıp", "dar", "bol", "küçük", "büyük", "tam"],
        "Fiyat/Performans": ["fiyat", "pahalı", "ucuz", "değer", "indirim", "performance"],
        "Satıcı": ["satıcı", "ilgi", "cevap", "yanlış", "eksik", "hediye"]
    }
    
    text_lower = text.lower()
    for cat, words in keywords.items():
        if any(w in text_lower for w in words):
            # Simple heuristic: if whole text is neg, aspect is likely neg
            sent = analyze_sentiment(text)['sentiment']
            aspects.append({
                "aspect": cat,
                "sentiment": sent,
                "confidence": 0.8
            })
            
    return aspects

def generate_business_summary(review_texts):
    """
    Generates a high-level strategic summary (Strengths, Weaknesses, Advice)
    using all reviews.
    """
    if not review_texts:
        return None
        
    # Summarize input if too long (take first 50 reviews or random sample)
    sample_text = json.dumps(review_texts[:50], ensure_ascii=False)
    
    prompt = f"""
    Sen uzman bir E-Ticaret Danismanisin. Asagidaki urun yorumlarini analiz ederek satici/ureticiye ozel bir rapor hazirla.
    
    Ciktiyi SADECE asagidaki JSON formatinda ver:
    {{
        "strengths": ["Güçlü yön 1 (Örn: Hızlı kargo)", "Güçlü yön 2"...],
        "weaknesses": ["Zayıf nokta 1 (Örn: Paketleme kötü)", "Zayıf nokta 2"...],
        "advice": ["Tavsiye 1 (Örn: Aras Kargo ile çalışmayı bırakın)", "Tavsiye 2"...]
    }}
    
    Her liste icin en az 3, en fazla 5 madde olsun. Maddeler kisa, net ve e-ticaret odakli olsun.
    
    Yorumlar:
    {sample_text}
    """
    
    # 1. Try Gemini
    google_key = os.getenv("GOOGLE_API_KEY")
    if google_key:
        try:
            genai.configure(api_key=google_key)
            model = genai.GenerativeModel('gemini-pro')
            response = model.generate_content(prompt)
            clean_text = response.text.replace("```json", "").replace("```", "").strip()
            return json.loads(clean_text)
        except Exception as e:
            print(f"Gemini Summary Error: {e}")

    # 2. Try OpenAI
    openai_key = os.getenv("OPENAI_API_KEY")
    if openai_key:
        try:
            client = OpenAI(api_key=openai_key)
            completion = client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "user", "content": prompt}],
                response_format={ "type": "json_object" }
            )
            return json.loads(completion.choices[0].message.content)
        except Exception as e:
            print(f"OpenAI Summary Error: {e}")
            
    # Fallback (Rule Based) if no API
    # Calculate most frequent positive/negative words (Simple heuristic for Turkish)
    
    # Simple word frequency
    all_text = " ".join(review_texts).lower()
    
    # Mock intelligent response based on content
    strengths = []
    weaknesses = []
    
    if "hızlı" in all_text and "kargo" in all_text:
        strengths.append("Kargo süreçleri hızlı işliyor")
    if "kaliteli" in all_text or "sağlam" in all_text:
        strengths.append("Ürün kalitesi beğeniliyor")
    if "hediye" in all_text:
        strengths.append("Hediye gönderimi memnuniyet yaratmış")
        
    if "geç" in all_text and "geldi" in all_text:
        weaknesses.append("Teslimat sürelerinde gecikmeler yaşanıyor")
    if "özensiz" in all_text or "yırtık" in all_text:
        weaknesses.append("Paketleme konusunda şikayetler var")
    if "küçük" in all_text or "dar" in all_text:
        weaknesses.append("Beden/Boyut konusunda uyumsuzluklar var")
        
    # Defaults if empty
    if not strengths: strengths = ["Genel müşteri memnuniyeti yüksek görünüyor"]
    if not weaknesses: weaknesses = ["Belirgin bir sistemsel sorun tespit edilemedi"]
    
    advice = []
    if "paket" in str(weaknesses):
        advice.append("Paketleme standartlarınızı gözden geçirin, daha korunaklı ambalaj kullanın.")
    if "beden" in str(weaknesses):
        advice.append("Ürün açıklamalarına detaylı beden tablosu ekleyin.")
    else:
        advice.append("Mevcut hizmet kalitesini koruyarak kampanyalarla satışı artırın.")

    return {
        "strengths": strengths[:5],
        "weaknesses": weaknesses[:5],
        "advice": advice[:3]
    }
