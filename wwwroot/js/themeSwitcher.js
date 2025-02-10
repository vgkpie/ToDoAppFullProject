const toggleButton = document.getElementById('theme-toggle');
const currentTheme = localStorage.getItem('theme') || 'light-theme';
document.body.classList.add(currentTheme);

toggleButton.addEventListener('click', () => {
    if (document.body.classList.contains('light-theme')) {
        document.body.classList.replace('light-theme', 'dark-theme');
        localStorage.setItem('theme', 'dark-theme');
    } else {
        document.body.classList.replace('dark-theme', 'light-theme');
        localStorage.setItem('theme', 'light-theme');
    }
});
