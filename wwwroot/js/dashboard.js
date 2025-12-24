// Dashboard JavaScript
// Tüm .cursorrules standartlarına uygun: snake_case, DOM caching, event delegation

$(document).ready(function() {
    // DOM Elementlerini Cache'le (Performance)
    var $notification_btn = $('#notificationBtn');
    var $credit_score_fills = $('.credit_card__score_fill');
    var $progress_fills = $('.progress_bar__fill');
    var $profile_dropdown = $('#profileDropdown');
    var $profile_dropdown_btn = $('#profileDropdownBtn');
    var $logout_btn = $('#logoutBtn');
    
    // Animasyonları Başlat
    init_animations();
    init_notification_system();
    init_interactive_cards();
    init_profile_dropdown();
    
    /**
     * Sayfa yüklendiğinde animasyonları başlatır
     */
    function init_animations() {
        // Kredi skoru animasyonu
        $credit_score_fills.each(function() {
            var $this = $(this);
            var score = parseInt($this.attr('data-score'), 10);
            
            if (score) {
                // Skoru 100 üzerinden yüzdeye çevir (max skor 1800 kabul edelim)
                var percentage = Math.min((score / 1800) * 100, 100);
                $this.css('--score-width', percentage);
            }
        });
        
        // Progress bar animasyonları
        $progress_fills.each(function() {
            var $this = $(this);
            var progress = parseInt($this.attr('data-progress'), 10);
            
            if (progress >= 0) {
                $this.css('--progress-width', progress);
            }
        });
        
        // Kartların sıralı fade-in animasyonu
        animate_cards_sequential();
    }
    
    /**
     * Kartları sırayla gösterir (daha yumuşak UX)
     */
    function animate_cards_sequential() {
        var $cards = $('.credit_card, .quick_action_card, .loan_card, .help_center_card');
        
        $cards.each(function(index) {
            var $card = $(this);
            $card.css({
                'opacity': '0',
                'transform': 'translateY(20px)'
            });
            
            setTimeout(function() {
                $card.css({
                    'opacity': '1',
                    'transform': 'translateY(0)',
                    'transition': 'all 0.5s ease'
                });
            }, index * 50); // Her kart 50ms arayla
        });
    }
    
    /**
     * Bildirim sistemi
     */
    function init_notification_system() {
        $notification_btn.on('click', function() {
            show_notification_dropdown();
        });
        
        // Dropdown dışına tıklanınca kapat
        $(document).on('click', function(e) {
            if (!$(e.target).closest('.btn_notification, .notification_dropdown').length) {
                close_notification_dropdown();
            }
        });
    }
    
    /**
     * Bildirim dropdown'ını gösterir
     */
    function show_notification_dropdown() {
        // TODO: Backend'den gerçek bildirimler çekilecek
        var notifications = [
            {
                title: 'Kredi Başvurunuz Onaylandı',
                message: '25.000 TL tutarındaki kredi başvurunuz onaylanmıştır.',
                time: '5 dakika önce',
                type: 'success'
            },
            {
                title: 'Ödeme Hatırlatması',
                message: 'Konut kredinizin taksit ödeme tarihi yaklaşıyor.',
                time: '2 saat önce',
                type: 'warning'
            },
            {
                title: 'Yeni Kampanya',
                message: 'Size özel %0.99 faiz oranı fırsatı!',
                time: '1 gün önce',
                type: 'info'
            }
        ];
        
        // Dropdown HTML'i oluştur
        var dropdown_html = '<div class="notification_dropdown">';
        dropdown_html += '<div class="notification_dropdown__header">';
        dropdown_html += '<h3 class="notification_dropdown__title">Bildirimler</h3>';
        dropdown_html += '<button type="button" class="notification_dropdown__close" id="closeNotifications">';
        dropdown_html += '<svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">';
        dropdown_html += '<path d="M18 6L6 18M6 6l12 12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>';
        dropdown_html += '</svg>';
        dropdown_html += '</button>';
        dropdown_html += '</div>';
        dropdown_html += '<div class="notification_dropdown__list">';
        
        notifications.forEach(function(notification) {
            dropdown_html += '<div class="notification_item notification_item--' + notification.type + '">';
            dropdown_html += '<div class="notification_item__content">';
            dropdown_html += '<h4 class="notification_item__title">' + notification.title + '</h4>';
            dropdown_html += '<p class="notification_item__message">' + notification.message + '</p>';
            dropdown_html += '<span class="notification_item__time">' + notification.time + '</span>';
            dropdown_html += '</div>';
            dropdown_html += '</div>';
        });
        
        dropdown_html += '</div>';
        dropdown_html += '<div class="notification_dropdown__footer">';
        dropdown_html += '<a href="/notifications" class="notification_dropdown__link">Tümünü Gör</a>';
        dropdown_html += '</div>';
        dropdown_html += '</div>';
        
        // Varsa eskisini kaldır
        $('.notification_dropdown').remove();
        
        // Header'a ekle
        $notification_btn.closest('.dashboard__user_actions').append(dropdown_html);
        
        // Close button event
        $('#closeNotifications').on('click', function() {
            close_notification_dropdown();
        });
    }
    
    /**
     * Bildirim dropdown'ını kapatır
     */
    function close_notification_dropdown() {
        $('.notification_dropdown').fadeOut(200, function() {
            $(this).remove();
        });
    }
    
    /**
     * İnteraktif kart özellikleri
     */
    function init_interactive_cards() {
        // Kartlara hover efekti ekle
        var $interactive_cards = $('.quick_action_card, .loan_card, .help_center_card');
        
        $interactive_cards.on('mouseenter', function() {
            $(this).addClass('card_hover');
        });
        
        $interactive_cards.on('mouseleave', function() {
            $(this).removeClass('card_hover');
        });
        
        // Kart tıklama analytics (TODO: Backend'e gönderilecek)
        $interactive_cards.on('click', function() {
            var card_type = $(this).attr('class').split(' ')[0];
            var card_title = $(this).find('h3').text();
            
            log_card_click(card_type, card_title);
        });
    }
    
    /**
     * Kart tıklamalarını loglar (Analytics için)
     */
    function log_card_click(card_type, card_title) {
        // TODO: Backend API'ye analytics verisi gönderilecek
        console.log('Card Clicked:', {
            type: card_type,
            title: card_title,
            timestamp: new Date().toISOString()
        });
    }
    
    /**
     * Kredi kartı progress bar güncelleme
     */
    function update_credit_usage() {
        // TODO: API'den gerçek zamanlı veri çekilecek
        // Bu fonksiyon periyodik olarak çağrılabilir
    }
    
    /**
     * Sayfa görünürlük değişikliğini takip et
     * Kullanıcı sayfaya döndüğünde verileri yenile
     */
    document.addEventListener('visibilitychange', function() {
        if (!document.hidden) {
            // Sayfa aktif olduğunda verileri yenile
            refresh_dashboard_data();
        }
    });
    
    /**
     * Dashboard verilerini yeniler
     */
    function refresh_dashboard_data() {
        // TODO: API çağrıları ile güncel verileri çek
        console.log('Dashboard verileri yenileniyor...');
    }
    
    /**
     * Responsive menü toggle (mobil için)
     */
    function init_mobile_menu() {
        var $mobile_menu_btn = $('.mobile_menu_toggle');
        
        $mobile_menu_btn.on('click', function() {
            $(this).toggleClass('active');
            $('.dashboard__sidebar').toggleClass('active');
        });
    }
    
    /**
     * Profil dropdown menüsü
     */
    function init_profile_dropdown() {
        // Dropdown toggle
        $profile_dropdown_btn.on('click', function(e) {
            e.stopPropagation();
            $profile_dropdown.toggleClass('active');
        });
        
        // Dışarı tıklanınca kapat
        $(document).on('click', function(e) {
            if (!$(e.target).closest('.profile_dropdown').length) {
                $profile_dropdown.removeClass('active');
            }
        });
        
        // ESC tuşu ile kapat
        $(document).on('keydown', function(e) {
            if (e.key === 'Escape') {
                $profile_dropdown.removeClass('active');
            }
        });
        
        // Logout işlemi
        $logout_btn.on('click', function() {
            window.location.href = '/?action=logout';
        });
    }
    
    // Sayfa yüklendiğinde bir kere çalışacak init fonksiyonları
    init_mobile_menu();
    init_faq_accordion();
    
    /**
     * SSS (FAQ) Accordion fonksiyonalitesi
     */
    function init_faq_accordion() {
        var $faq_items = $('.faq_item');
        
        $faq_items.find('.faq_item__header').on('click', function() {
            var $parent = $(this).closest('.faq_item');
            var is_open = $parent.hasClass('is-open');
            
            // Diğerlerini kapat (accordion davranışı)
            $faq_items.removeClass('is-open');
            
            // Tıklananı toggle et
            if (!is_open) {
                $parent.addClass('is-open');
            }
        });
    }
    
    // Scroll animasyonları için intersection observer
    if ('IntersectionObserver' in window) {
        var observer = new IntersectionObserver(function(entries) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    $(entry.target).addClass('visible');
                }
            });
        }, {
            threshold: 0.1
        });
        
        // Tüm section'ları gözlemle
        $('.dashboard__quick_actions, .dashboard__active_loans, .dashboard__help_center, .dashboard__faq_section').each(function() {
            observer.observe(this);
        });
    }
});
