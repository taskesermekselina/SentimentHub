import time
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.chrome.options import Options

def scrape_trendyol_reviews(url: str, max_reviews: int = 100):
    options = Options()
    options.add_argument("--headless")
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument("--disable-blink-features=AutomationControlled")
    options.add_argument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36")

    driver = webdriver.Chrome(service=Service(ChromeDriverManager().install()), options=options)
    
    reviews_data = []
    business_name = "Trendyol Product"
    
    try:
        # Pre-process URL: Ensure we are on the reviews page
        if "/yorumlar" not in url:
            # Check if it has query params
            if "?" in url:
                 base = url.split("?")[0]
                 query = url.split("?")[1]
                 target_url = f"{base}/yorumlar?{query}"
            else:
                 target_url = f"{url}/yorumlar"
        else:
            target_url = url
            
        print(f"Navigating to: {target_url}")
        driver.get(target_url)
        time.sleep(5) # Wait for initial load

        # 1. Extract Product Name (Business Name)
        try:
            # Trendyol header usually has class 'pr-new-br' or checking h1
            h1 = driver.find_element(By.TAG_NAME, "h1")
            business_name = h1.text
            if not business_name:
                 # Try brand name + product name
                 brand = driver.find_element(By.CLASS_NAME, "brand-name").text
                 prod = driver.find_element(By.CLASS_NAME, "product-name").text
                 business_name = f"{brand} {prod}"
            print(f"Found Product Name: {business_name}")
        except:
            print("Could not find product name, using default")

        # 2. Scrape Reviews (Trendyol Specific)
        unique_texts = set()
        
        # Trendyol Specific Selectors 2026
        # Container: .rnr-com-w
        # Text: .rnr-com-tx
        # Date: .rnr-com-dt
        
        body = driver.find_element(By.TAG_NAME, "body")
        
        for i in range(max_reviews // 10): # Faster passes
            print(f"--- Pass {i+1} ---")
            
            # Scroll strategy for Trendyol (Infinite scroll)
            for _ in range(3):
                body.send_keys(Keys.PAGE_DOWN)
                time.sleep(0.3)
            
            time.sleep(1.0)
            
            # Find all review containers
            containers = driver.find_elements(By.CLASS_NAME, "rnr-com-w")
            if not containers:
                 # Fallback for mobile view or alternative classes
                 containers = driver.find_elements(By.XPATH, "//div[contains(@class, 'comment')]")
            
            print(f"Found {len(containers)} review containers.")
            
            new_in_this_pass = 0
            
            for index, card in enumerate(containers):
                try:
                    review_text = ""
                    
                    # Strategy 1: Trendyol Specific Classes
                    try:
                        review_text = card.find_element(By.CLASS_NAME, "rnr-com-tx").text
                    except:
                        pass
                        
                    # Strategy 2: Common Alternatives from different layouts
                    if not review_text:
                        try:
                            review_text = card.find_element(By.XPATH, ".//div[contains(@class, 'comment-text')]").text
                        except:
                            pass

                    # Strategy 3: Any Paragraph
                    if not review_text:
                        try:
                            review_text = card.find_element(By.TAG_NAME, "p").text
                        except:
                            pass
                            
                    # Strategy 4: Fallback to full card text (Aggressive)
                    if not review_text:
                        full_content = card.text.split('\n')
                        # Heuristic: the longest line is likely the review
                        if full_content:
                            review_text = max(full_content, key=len)
                    
                    if not review_text or len(review_text) < 3: 
                        if index < 5: print(f"Skipping empty review at index {index}")
                        continue
                        
                    # Check duplication
                    if review_text in unique_texts: continue
                    
                    # Extract Rating
                    rating = 5 # Default
                    try:
                        # Star container: .rnr-com-sr -> .full
                        full_stars = card.find_elements(By.CSS_SELECTOR, ".rnr-com-sr .full")
                        if full_stars:
                            rating = len(full_stars)
                        else:
                             # Fallback: Count gold/orange stars classes
                             stars = card.find_elements(By.CSS_SELECTOR, "i.i-star-orange") # older class?
                             if stars: rating = len(stars)
                             else:
                                 # text based?
                                 pass
                    except:
                        pass

                    # Author
                    author = "Müşteri"
                    try:
                        author = card.find_element(By.CLASS_NAME, "rnr-com-usr").text
                    except:
                        # Try to find from split lines (short line)
                        pass
                        
                    # Date
                    date_val = ""
                    try:
                        date_val = card.find_element(By.CLASS_NAME, "rnr-com-dt").text
                    except:
                        pass

                    reviews_data.append({
                        "author": author,
                        "text": review_text,
                        "rating": rating,
                        "date": date_val
                    })
                    unique_texts.add(review_text)
                    new_in_this_pass += 1
                except Exception as e:
                     if index < 3: print(f"Card parse error: {e}")
                     continue
            
            print(f"Added {new_in_this_pass} new reviews. Total: {len(reviews_data)}")
            
            if len(reviews_data) >= max_reviews:
                break
                
            # Try to find "More" button (if not infinite scroll)
            if new_in_this_pass == 0 and i > 2:
                 pass

    except Exception as e:
        print(f"Global Scraping Error: {e}")
    finally:
        driver.quit()
        
    return reviews_data, business_name


