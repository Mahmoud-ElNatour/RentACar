document.addEventListener('DOMContentLoaded', () => {
    const tocItems = document.querySelectorAll('.toc-item');
    const sections = document.querySelectorAll('section');
    // Smooth scroll functionality for ToC clicks
    tocItems.forEach(item => {
        item.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = item.getAttribute('href');
            const targetSection = document.querySelector(targetId);
            if (targetSection) {
                // Calculate offset position for sticky headers/padding
                const offset = 100;
                const bodyRect = document.body.getBoundingClientRect().top;
                const elementRect = targetSection.getBoundingClientRect().top;
                const elementPosition = elementRect - bodyRect;
                const offsetPosition = elementPosition - offset;
                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
    // Intersection Observer to highlight active ToC item on scroll
    const observerOptions = {
        root: null,
        rootMargin: '-20% 0px -60% 0px', // Trigger when section is in the upper part of view
        threshold: 0
    };
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const id = entry.target.getAttribute('id');
                tocItems.forEach(item => {
                    item.classList.remove('active');
                    // Add active class if href matches the section ID
                    if (item.getAttribute('href') === `#${id}`) {
                        item.classList.add('active');
                    }
                });
            }
        });
    }, observerOptions);
    sections.forEach(section => {
        observer.observe(section);
    });
});
