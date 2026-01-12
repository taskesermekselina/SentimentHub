using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SentimentHub.Web.Models;
using System.Text.Json;
using System.Linq;

namespace SentimentHub.Web.Services;

public interface IPdfService
{
    byte[] GenerateReport(Analysis analysis, string sentimentChartBase64 = null, string aspectChartBase64 = null);
    byte[] GenerateComparisonReport(ComparisonViewModel model);
}

public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateReport(Analysis analysis, string sentimentChartBase64 = null, string aspectChartBase64 = null)
    {
        var summaryDto = !string.IsNullOrEmpty(analysis.SummaryJson) 
            ? JsonSerializer.Deserialize<SentimentHub.Web.DTOs.SummaryResultDto>(analysis.SummaryJson) 
            : null;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                // HEADER
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("SentimentHub AI").FontSize(24).Bold().FontColor(Colors.Indigo.Medium);
                        col.Item().Text("Akıllı E-Ticaret Analiz Raporu").FontSize(14).FontColor(Colors.Grey.Medium);
                    });
                    
                    row.ConstantItem(150).Column(col =>
                    {
                        col.Item().AlignRight().Text($"{DateTime.Now:dd.MM.yyyy}").FontSize(12);
                        col.Item().AlignRight().Text("Rapor No: " + analysis.Id).FontSize(10).FontColor(Colors.Grey.Lighten1);
                    });
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // PRODUCT INFO
                    col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingBottom(10).Row(row => {
                        row.RelativeItem().Column(c => {
                             c.Item().Text(analysis.Business?.Name ?? "Ürün/İşletme").FontSize(18).Bold();
                             c.Item().Text(analysis.Business?.GoogleMapsUrl ?? "").FontSize(10).FontColor(Colors.Blue.Medium);
                        });
                    });

                    col.Item().PaddingVertical(20).Row(row => 
                    {
                        // KPI 1: Overall Score (1-5 Integer)
                        row.RelativeItem().Component(new KpiComponent("Genel Değerlendirme", $"{analysis.OverallScore:F1}/5", Colors.Green.Medium));
                        row.ConstantItem(20);
                        // KPI 2
                        row.RelativeItem().Component(new KpiComponent("Toplam Yorum", analysis.TotalReviews.ToString(), Colors.Blue.Medium));
                        row.ConstantItem(20);
                        // KPI 3
                        row.RelativeItem().Component(new KpiComponent("Analiz Durumu", "Tamamlandı", Colors.Indigo.Medium));
                    });

                    // AI SUMMARY
                    if (summaryDto != null)
                    {
                         col.Item().Background(Colors.Grey.Lighten5).Padding(15).Column(c => {
                             c.Item().Text("Yapay Zeka Değerlendirmesi").FontSize(16).Bold().FontColor(Colors.Indigo.Darken1);
                             
                             // Strengths
                             c.Item().PaddingTop(10).Text("Olumlu Yönler").Bold().FontColor(Colors.Green.Darken1);
                             foreach(var s in summaryDto.Strengths) c.Item().PaddingLeft(10).Text("• " + s);
                             
                             // Weaknesses
                             c.Item().PaddingTop(10).Text("Olumsuz Yönler").Bold().FontColor(Colors.Red.Darken1);
                             foreach(var w in summaryDto.Weaknesses) c.Item().PaddingLeft(10).Text("• " + w);

                             // Recommendations (NEW)
                             if(summaryDto.Recommendations != null && summaryDto.Recommendations.Any())
                             {
                                 c.Item().PaddingTop(10).Text("İşletmeye Yönelik İyileştirici Öneriler").Bold().FontColor(Colors.Orange.Darken2);
                                 foreach(var r in summaryDto.Recommendations) c.Item().PaddingLeft(10).Text("→ " + r).Italic();
                             }
                             
                             // Category Scores Text
                             if(summaryDto.CategoryScores != null)
                             {
                                 c.Item().PaddingTop(10).Text("Kategori Puanları").Bold();
                                 c.Item().Row(r => {
                                     r.RelativeItem().Text($"Kalite: {summaryDto.CategoryScores.ProductQuality}/5");
                                     r.RelativeItem().Text($"Fiyat/Perf: {summaryDto.CategoryScores.PricePerformance}/5");
                                     r.RelativeItem().Text($"Kargo: {summaryDto.CategoryScores.Shipping}/5");
                                     r.RelativeItem().Text($"Satıcı: {summaryDto.CategoryScores.Seller}/5");
                                     r.RelativeItem().Text($"Deneyim: {summaryDto.CategoryScores.UsageExperience}/5");
                                 });
                             }
                         });
                    }

                    col.Item().PaddingVertical(10);

                    // CHARTS (Embedded Images)
                    if (!string.IsNullOrEmpty(sentimentChartBase64) || !string.IsNullOrEmpty(aspectChartBase64))
                    {
                        col.Item().Text("Görsel Analizler").FontSize(16).Bold();
                        col.Item().Row(row => 
                        {
                            if (!string.IsNullOrEmpty(sentimentChartBase64))
                            {
                                try {
                                    var bytes = Convert.FromBase64String(sentimentChartBase64.Split(',')[1]);
                                    row.RelativeItem().Padding(5).Column(c => {
                                        c.Item().Text("Duygu Dağılımı").AlignCenter();
                                        c.Item().Image(bytes).FitArea();
                                    });
                                } catch {}
                            }
                            
                            row.ConstantItem(20);

                            if (!string.IsNullOrEmpty(aspectChartBase64))
                            {
                                try {
                                    var bytes = Convert.FromBase64String(aspectChartBase64.Split(',')[1]);
                                    row.RelativeItem().Padding(5).Column(c => {
                                        c.Item().Text("Kategori Analizi").AlignCenter();
                                        c.Item().Image(bytes).FitArea();
                                    });
                                } catch {}
                            }
                        });
                    }

                    col.Item().PaddingVertical(20);

                    // REVIEWS TABLE
                    col.Item().PaddingBottom(10).Text("Örnek Müşteri Yorumları").FontSize(16).Bold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(70);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Yazar").Bold();
                            header.Cell().Element(CellStyle).Text("Yorum Detayı").Bold();
                            header.Cell().Element(CellStyle).Text("Duygu").Bold();
                            header.Cell().Element(CellStyle).Text("Tarih").Bold();
                            
                            static IContainer CellStyle(IContainer container)
                                => container.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).BorderColor(Colors.Grey.Medium).Padding(5);
                        });

                        foreach (var review in analysis.Reviews.OrderByDescending(r => r.ReviewDate)) // Limit removed as requested
                        {
                            table.Cell().Element(CellStyle).Text(review.AuthorName ?? "Müşteri");
                            
                            var text = review.Text ?? "";
                            if (text.Length > 150) text = text.Substring(0, 150) + "...";
                            table.Cell().Element(CellStyle).Text(text).FontSize(10);
                            
                            var sentimentTr = review.Sentiment == SentimentType.Positive ? "Olumlu" : 
                                              review.Sentiment == SentimentType.Negative ? "Olumsuz" : "Nötr";
                            var color = review.Sentiment == SentimentType.Positive ? Colors.Green.Medium : 
                                        review.Sentiment == SentimentType.Negative ? Colors.Red.Medium : Colors.Orange.Medium;

                            table.Cell().Element(CellStyle).Text(sentimentTr).FontColor(color);
                            table.Cell().Element(CellStyle).Text(review.ReviewDate?.ToShortDateString() ?? "-");

                            static IContainer CellStyle(IContainer container)
                                => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5);
                        }
                    });
                });

                // FOOTER
                page.Footer().AlignCenter().Row(row => {
                    row.RelativeItem().Text("SentimentHub AI Report © 2026").FontSize(10).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignRight().Text(x => {
                        x.Span("Sayfa ");
                        x.CurrentPageNumber();
                    });
                });
            });
        }).GeneratePdf();
    } // Close GenerateReport

    public byte[] GenerateComparisonReport(ComparisonViewModel model)
    {
        return Document.Create(container =>
        { 
            // ... (Rest of the method is fine, just fixing the wrapper)
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                // HEADER
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("SentimentHub AI").FontSize(20).Bold().FontColor(Colors.Indigo.Medium);
                        col.Item().Text("Karşılaştırmalı Analiz Raporu").FontSize(14).FontColor(Colors.Grey.Medium);
                    });
                    
                    row.ConstantItem(150).Column(col =>
                    {
                        col.Item().AlignRight().Text($"{DateTime.Now:dd.MM.yyyy}").FontSize(10);
                        col.Item().AlignRight().Text($"Ürün Sayısı: {model.Products.Count}").FontSize(10);
                    });
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // 1. COMPARISON TABLE Structure
                    col.Item().Table(table =>
                    {
                        // Definition
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(120); // Labels
                            foreach(var p in model.Products) columns.RelativeColumn();
                        });

                        // Header Row (Product Names)
                        table.Header(header =>
                        {
                            header.Cell().Element(BlockHeader).Text("KARŞILAŞTIRMA").Bold();
                            foreach(var p in model.Products)
                            {
                                header.Cell().Element(BlockHeader).Text(p.Name).Bold().FontColor(Colors.Indigo.Darken2);
                            }
                        });

                        // Row: General Score
                        table.Cell().Element(BlockLabel).Text("Genel Puan");
                        foreach(var p in model.Products)
                        {
                            table.Cell().Element(BlockCell).AlignCenter().Text($"{p.OverallScore:F1}/5").FontSize(14).Bold()
                                .FontColor(p.OverallScore >= 4 ? Colors.Green.Medium : p.OverallScore >= 3 ? Colors.Orange.Medium : Colors.Red.Medium);
                        }

                        // Row: Positive/Negative
                        table.Cell().Element(BlockLabel).Text("Memnuniyet Oranı");
                        foreach(var p in model.Products)
                        {
                            table.Cell().Element(BlockCell).Column(c => {
                                c.Item().Row(r => {
                                    r.RelativeItem().Text($"% {p.PositiveRate} Olumlu").FontColor(Colors.Green.Darken1).FontSize(9);
                                });
                                c.Item().Row(r => {
                                    r.RelativeItem().Text($"% {p.NegativeRate} Olumsuz").FontColor(Colors.Red.Darken1).FontSize(9);
                                });
                            });
                        }

                        // Row: Category Scores (Nested Table or Lines)
                        table.Cell().Element(BlockLabel).Text("Kategori Puanları");
                        foreach(var p in model.Products)
                        {
                            table.Cell().Element(BlockCell).Column(c => {
                                c.Item().Text($"Kalite: {p.CategoryScores.ProductQuality}/5");
                                c.Item().Text($"Fiyat: {p.CategoryScores.PricePerformance}/5");
                                c.Item().Text($"Kargo: {p.CategoryScores.Shipping}/5");
                                c.Item().Text($"Satıcı: {p.CategoryScores.Seller}/5");
                                c.Item().Text($"Deneyim: {p.CategoryScores.UsageExperience}/5");
                            });
                        }

                        // Row: Strengths
                        table.Cell().Element(BlockLabel).Text("Güçlü Yönler");
                        foreach(var p in model.Products)
                        {
                            table.Cell().Element(BlockCell).Column(c => {
                                foreach(var s in p.Strengths.Take(5)) c.Item().Text("• " + s).FontSize(9);
                            });
                        }

                        // Row: Weaknesses & Recs
                        table.Cell().Element(BlockLabel).Text("Gelişim Alanları & Öneriler");
                        foreach(var p in model.Products)
                        {
                            table.Cell().Element(BlockCell).Column(c => {
                                int i = 0;
                                foreach(var w in p.Weaknesses.Take(3)) 
                                {
                                    c.Item().Text("• " + w).FontSize(9).FontColor(Colors.Red.Darken1);
                                    if(i < p.Recommendations.Count)
                                    {
                                        c.Item().PaddingLeft(5).Text("→ " + p.Recommendations[i]).FontSize(8).Italic().FontColor(Colors.Grey.Darken2);
                                    }
                                    i++;
                                }
                            });
                        }

                        static IContainer BlockHeader(IContainer container) => container.Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(5);
                        static IContainer BlockLabel(IContainer container) => container.Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(5).AlignMiddle();
                        static IContainer BlockCell(IContainer container) => container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                    });

                    col.Item().PaddingVertical(10);

                    // 2. INSIGHTS SECTION
                    col.Item().Background(Colors.Grey.Lighten5).Padding(10).Column(c => 
                    {
                        c.Item().Text("ANALİTİK DEĞERLENDİRME").Bold().FontSize(12).Underline();
                        
                        // Distinctive Features
                        c.Item().PaddingTop(5).Text("Ayırt Edici Özellikler:").Bold();
                        if(model.DistinctiveFeatures.Any())
                        {
                            foreach(var f in model.DistinctiveFeatures) c.Item().PaddingLeft(10).Text("• " + f);
                        }
                        else 
                        {
                            c.Item().PaddingLeft(10).Text("- Belirgin fark bulunamadı.");
                        }

                        // Profiles
                        c.Item().PaddingTop(5).Text("Kullanıcı Profilleri:").Bold();
                        foreach(var kvp in model.UserProfiles)
                        {
                           c.Item().PaddingLeft(10).Row(r => {
                               r.ConstantItem(200).Text("• " + kvp.Key + ":");
                               r.RelativeItem().Text(kvp.Value).Bold().FontColor(Colors.Indigo.Medium);
                           });
                        }

                        // Decision Support
                        c.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten3).PaddingTop(5).Text("SONUÇ VE TAVSİYE:").Bold();
                        c.Item().Text(model.DecisionSupport).FontSize(11).Italic();
                    });
                });

                page.Footer().AlignCenter().Row(row => {
                    row.RelativeItem().Text("SentimentHub AI - Karşılaştırma Modülü").FontSize(8).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignRight().Text(x => x.CurrentPageNumber());
                });
            });
        }).GeneratePdf();
    }
}

public class KpiComponent : IComponent
{
    private string Title { get; }
    private string Value { get; }
    private string Color { get; }

    public KpiComponent(string title, string value, string color)
    {
        Title = title;
        Value = value;
        Color = color;
    }

    public void Compose(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten3)
            .Background(Colors.Grey.Lighten5)
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text(Title).FontSize(10).FontColor(Colors.Grey.Darken1);
                column.Item().Text(Value).FontSize(24).Bold().FontColor(Color);
            });
    }
}
