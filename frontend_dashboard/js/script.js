// Sidebar Toggle
document.addEventListener('DOMContentLoaded', function() {
    const sidebarToggle = document.getElementById("sidebarToggle");
    const wrapper = document.getElementById("wrapper");

    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", function(e) {
            e.preventDefault();
            wrapper.classList.toggle("toggled");
        });
    }

    // Modal Analysis Logic Mock
    const analyzeBtn = document.getElementById('startAnalysisBtn');
    if (analyzeBtn) {
        analyzeBtn.addEventListener('click', function() {
            const btn = this;
            const originalText = btn.innerHTML;
            
            // Show loading state
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Analiz Ediliyor...';
            btn.disabled = true;

            // Simulate delay then redirect
            setTimeout(() => {
                window.location.href = 'report.html';
            }, 2000);
        });
    }
});
