// Sidebar Toggle
document.addEventListener('DOMContentLoaded', function () {
    const sidebarToggle = document.getElementById("sidebarToggle");
    const wrapper = document.getElementById("wrapper");

    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", function (e) {
            e.preventDefault();
            wrapper.classList.toggle("toggled");
            console.log("Sidebar toggled:", wrapper.classList.contains("toggled"));
        });
    } else {
        console.error("Sidebar Toggle button not found!");
    }

    // Modal Analysis Logic - In-Modal Circular Progress
    const analysisForm = document.getElementById('analysisForm');
    const loadingState = document.getElementById('analysisLoadingState');
    const progressText = document.getElementById('progressPercent');
    const progressBar = document.querySelector('.progress-ring__bar');

    // Reset state when modal opens
    const modalEl = document.getElementById('analysisModal');
    if (modalEl) {
        modalEl.addEventListener('show.bs.modal', function () {
            if (analysisForm) analysisForm.classList.remove('d-none');
            if (loadingState) loadingState.classList.add('d-none');
            // Reset Progress
            if (progressText) progressText.innerText = '0%';
            if (progressBar) {
                const circumference = 52 * 2 * Math.PI;
                progressBar.style.strokeDashoffset = circumference;
            }
        });
    }

    if (analysisForm) {
        analysisForm.addEventListener('submit', function (e) {
            // e.preventDefault(); // allow submit

            // 1. Hide Form
            analysisForm.classList.add('d-none');

            // 2. Show Loading
            if (loadingState) {
                loadingState.classList.remove('d-none');
                loadingState.classList.add('d-flex');
            }

            // 3. Simulate Progress (90% over 5s)
            if (progressBar && progressText) {
                let percent = 0;
                const circumference = 52 * 2 * Math.PI;

                const interval = setInterval(() => {
                    percent += Math.floor(Math.random() * 5) + 1; // Random jump
                    if (percent > 95) percent = 95; // Cap at 95 until page reload (actual completion)

                    // Update Text
                    progressText.innerText = percent + '%';

                    // Update Circle
                    const offset = circumference - (percent / 100) * circumference;
                    progressBar.style.strokeDashoffset = offset;

                    if (percent >= 95) clearInterval(interval);
                }, 300); // Update every 300ms
            }
        });
    }
});
