// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sidebar toggle functionality
document.addEventListener('DOMContentLoaded', function() {
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    const sidebarCloseBtn = document.getElementById('sidebarCloseBtn');
    const avatarBtn = document.getElementById('avatarBtn');
    const notificationBtn = document.getElementById('notificationBtn');

    // Function to close sidebar
    const closeSidebar = () => {
        sidebar.classList.remove('open');
        sidebarOverlay.classList.remove('show');
    };

    // Toggle sidebar
    if (menuToggle && sidebar && sidebarOverlay) {
        menuToggle.addEventListener('click', () => {
            sidebar.classList.toggle('open');
            sidebarOverlay.classList.toggle('show');
        });

        // Close button functionality
        if (sidebarCloseBtn) {
            sidebarCloseBtn.addEventListener('click', closeSidebar);
        }

        sidebarOverlay.addEventListener('click', closeSidebar);
    }

    // Avatar click to navigate to account
    if (avatarBtn) {
        avatarBtn.addEventListener('click', () => {
            window.location.href = '/MiCuenta';
        });
    }

    // Notification button click
    if (notificationBtn) {
        notificationBtn.addEventListener('click', function() {
            alert('Notificaciones: 3 avisos pendientes');
        });
    }
});