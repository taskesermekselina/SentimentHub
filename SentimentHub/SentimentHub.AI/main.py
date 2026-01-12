from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional
import scraper
import sentiment
import uvicorn

app = FastAPI(title="SentimentHub AI Service")

class AnalyzeRequest(BaseModel):
    url: str
    limit: int = 50

class AspectResult(BaseModel):
    aspect: str
    sentiment: str
    confidence: float

class ReviewResult(BaseModel):
    author: str
    text: str
    rating: int
    date: str
    sentiment: str
    confidence: float
    aspects: List[AspectResult]

class SummaryResult(BaseModel):
    strengths: List[str]
    weaknesses: List[str]
    advice: List[str]

class AnalyzeResponse(BaseModel):
    reviews: List[ReviewResult]
    total_reviews: int
    overall_sentiment_score: float
    business_name: Optional[str] = None
    summary: Optional[SummaryResult] = None

@app.post("/analyze", response_model=AnalyzeResponse)
def analyze_param_url(request: AnalyzeRequest):
    try:
        # Scrape
        print(f"Scraping {request.url} with limit {request.limit}")
        raw_reviews, business_name = scraper.scrape_trendyol_reviews(request.url, request.limit)
        
        results = []
        total_score = 0
        
        # Analyze
        review_texts = [r['text'] for r in raw_reviews if r['text']]
        analyzed_results_map = {} 
        
        # 1. Detail Analysis (Existing batch logic)
        try:
            batch_size = 20
            for i in range(0, len(review_texts), batch_size):
                batch = review_texts[i:i+batch_size]
                if not batch: continue
                api_responses = sentiment.analyze_sentiment_with_api(batch)
                if api_responses and isinstance(api_responses, list):
                    for j, res in enumerate(api_responses):
                        if j < len(batch):
                            analyzed_results_map[batch[j]] = res
        except Exception as e:
            print(f"Detail API Analysis failed: {e}")

        # 2. Executive Summary (New)
        strategic_summary = None
        if review_texts:
             print("Generating Executive Summary...")
             raw_summary = sentiment.generate_business_summary(review_texts)
             if raw_summary:
                 strategic_summary = SummaryResult(
                     strengths=raw_summary.get('strengths', []),
                     weaknesses=raw_summary.get('weaknesses', []),
                     advice=raw_summary.get('advice', [])
                 )

        print(f"Finalizing {len(raw_reviews)} reviews")
        
        for r in raw_reviews:
            # Check if we have API result
            api_res = analyzed_results_map.get(r['text'])
            
            if api_res:
                final_sentiment = api_res.get('sentiment', 'Neutral')
                final_score = float(api_res.get('score', 0.8))
                raw_aspects = api_res.get('aspects', [])
                aspects_obj = []
                for a in raw_aspects:
                    aspects_obj.append(AspectResult(
                        aspect=str(a.get('aspect') or a.get('feature') or "General"),
                        sentiment=str(a.get('sentiment') or final_sentiment),
                        confidence=float(a.get('score') or a.get('confidence') or 0.8)
                    ))
            else:
                sent_res = sentiment.analyze_sentiment(r['text'])
                final_sentiment = sent_res['sentiment']
                final_score = float(sent_res['score'])
                aspects_obj = [] # Basic aspect removed for clarity on fallback

            if final_sentiment == 'Positive':
                 total_score += 1.0
            elif final_sentiment == 'Neutral':
                 total_score += 0.5
            
            results.append(ReviewResult(
                author=r['author'],
                text=r['text'],
                rating=r['rating'],
                date=r['date'],
                sentiment=final_sentiment,
                confidence=final_score,
                aspects=aspects_obj
            ))
                        
        avg_score = total_score / len(results) if results else 0
        
        return AnalyzeResponse(
            reviews=results,
            total_reviews=len(results),
            overall_sentiment_score=avg_score,
            business_name=business_name,
            summary=strategic_summary
        )

    except Exception as e:
        print(f"Error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8001)
