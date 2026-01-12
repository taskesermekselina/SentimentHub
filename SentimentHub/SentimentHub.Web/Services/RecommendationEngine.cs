using System.Collections.Generic;
using System.Globalization;

namespace SentimentHub.Web.Services;

public static class RecommendationEngine
{
    public static List<string> Generate(List<string> weaknesses)
    {
        var recommendations = new List<string>();
        if (weaknesses == null) return recommendations;

        foreach (var w in weaknesses)
        {
            string lowerW = w.ToLower(new CultureInfo("tr-TR"));
            string rec = "Bu konuda detaylı inceleme yapılması ve müşteri geri bildirimlerinin takip edilmesi önerilir.";

            if (lowerW.Contains("kargo") || lowerW.Contains("teslimat") || lowerW.Contains("paket") || lowerW.Contains("lojistik"))
                rec = "Kargo operasyonlarınıza hız katmak için lojistik partnerinizi değerlendirin veya paketleme standartlarını yükseltin. Hızlı teslimat, müşteri memnuniyetini doğrudan etkiler.";
            else if (lowerW.Contains("fiyat") || lowerW.Contains("pahalı") || lowerW.Contains("ücret") || lowerW.Contains("değer"))
                rec = "Fiyat/performans algısını artırmak için dönemsel kampanyalar, kuponlar veya çoklu alım indirimleri düzenleyin. Rakip fiyat analizlerini güncel tutun.";
            else if (lowerW.Contains("kalite") || lowerW.Contains("malzeme") || lowerW.Contains("bozuk") || lowerW.Contains("ömür") || lowerW.Contains("dayanık"))
                rec = "Ürün kalite kontrol süreçlerini sıkılaştırın. Üretim veya tedarik zincirindeki kalite standartlarını gözden geçirin. Kalite sorunları marka sadakatini zedeler.";
            else if (lowerW.Contains("iletişim") || lowerW.Contains("cevap") || lowerW.Contains("ilgi") || lowerW.Contains("muhatap"))
                rec = "Müşteri sorularına dönüş hızınızı artırmak için otomatik yanıt sistemleri kullanın veya destek ekibinizin kapasitesini artırın. İletişim, güvenin temelidir.";
            else if (lowerW.Contains("beden") || lowerW.Contains("kalıp") || lowerW.Contains("uymuyor") || lowerW.Contains("büyük") || lowerW.Contains("küçük"))
                rec = "Ürün açıklamalarına detaylı beden tablosu ve ölçü görselleri ekleyin. 'Tam kalıp' veya 'Bir beden büyük alınmalı' gibi net yönlendirmeler iadeleri azaltır.";
            else if (lowerW.Contains("iade") || lowerW.Contains("değişim"))
                rec = "İade ve değişim politikalarınızı müşteriler için daha şeffaf ve kolay hale getirin. Süreçteki zorluklar negatif yorumların ana kaynağıdır.";
            else if (lowerW.Contains("paketleme") || lowerW.Contains("kutu") || lowerW.Contains("özensiz"))
                 rec = "Kargolama sırasında ürünün zarar görmemesi için paketleme materyallerini güçlendirin (balonlu naylon, koruyucu kutu vb.).";
            else if (lowerW.Contains("yanlış") || lowerW.Contains("farklı") || lowerW.Contains("eksik"))
                 rec = "Depo ve sevkiyat süreçlerinde barkod kontrol sistemlerini iyileştirerek yanlış veya eksik ürün gönderimlerini minimize edin.";

            recommendations.Add(rec);
        }
        
        // Ensure strictly non-empty if input was non-empty? Logic above guarantees 1-to-1 mapping.
        return recommendations;
    }
}
