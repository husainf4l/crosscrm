// Main JavaScript file for Cross CRM

document.addEventListener('DOMContentLoaded', function() {
    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
    
    // Confirm delete actions
    const deleteButtons = document.querySelectorAll('[data-confirm-delete]');
    deleteButtons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this item?')) {
                e.preventDefault();
            }
        });
    });
    
    // Close sidebar when clicking outside on mobile
    if (window.innerWidth <= 768) {
        document.addEventListener('click', function(e) {
            const sidebar = document.getElementById('sidebar');
            const toggle = document.querySelector('.mobile-menu-toggle');
            if (sidebar && sidebar.classList.contains('show') && 
                !sidebar.contains(e.target) && 
                !toggle.contains(e.target)) {
                toggleSidebar();
            }
        });
    }
    
    // Handle window resize
    window.addEventListener('resize', function() {
        const sidebar = document.getElementById('sidebar');
        if (window.innerWidth > 768 && sidebar) {
            sidebar.classList.remove('show');
        }
    });
});

// Toggle sidebar on mobile
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const closeBtn = document.getElementById('close-sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (sidebar) {
        sidebar.classList.toggle('show');
        if (closeBtn) {
            closeBtn.style.display = sidebar.classList.contains('show') ? 'block' : 'none';
        }
        if (overlay) {
            overlay.style.display = sidebar.classList.contains('show') ? 'block' : 'none';
        }
    }
}

// Format currency
function formatCurrency(amount, currency = 'USD') {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: currency
    }).format(amount);
}

// Format date
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

