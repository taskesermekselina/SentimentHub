@echo off
echo SentimentHub Baslatiliyor...
echo 1. Python AI Servisi Aciliyor...
start "SentimentHub AI Servisi" cmd /k "cd SentimentHub\SentimentHub.AI && call venv\Scripts\activate && python main.py"

echo 2. Web Arayuzu Aciliyor...
start "SentimentHub Web Arayuzu" cmd /k "cd SentimentHub\SentimentHub.Web && dotnet run"

echo Tamamlandi! Pencereleri kapatmayiniz.
timeout /t 5
