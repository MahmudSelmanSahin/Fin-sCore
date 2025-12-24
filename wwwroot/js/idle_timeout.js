/**
 * Idle Timeout Handler
 * Kullanıcı belirli süre işlem yapmazsa otomatik logout yapar
 * 
 * Kullanım: Bu script authenticated sayfalarda (_Layout.cshtml) yüklenir
 */
$(document).ready(function() {
    'use strict';
    
    // ========== CONFIGURATION ==========
    var IDLE_TIMEOUT_MS = 60 * 1000; // 1 dakika (60 saniye)
    var WARNING_BEFORE_MS = 15 * 1000; // Timeout'tan 15 saniye önce uyarı göster
    var CHECK_INTERVAL_MS = 1000; // Her saniye kontrol et
    
    // ========== STATE ==========
    var idle_timer = null;
    var warning_timer = null;
    var countdown_interval = null;
    var last_activity_time = Date.now();
    var is_warning_shown = false;
    var $warning_modal = null;
    var $countdown_element = null;
    
    // ========== ACTIVITY EVENTS ==========
    var activity_events = [
        'mousemove',
        'mousedown',
        'keydown',
        'keypress',
        'touchstart',
        'touchmove',
        'scroll',
        'wheel',
        'click'
    ];
    
    // ========== FUNCTIONS ==========
    
    /**
     * Kullanıcı aktivitesini kaydet
     */
    function record_activity() {
        last_activity_time = Date.now();
        
        // Uyarı modal açıksa kapat
        if (is_warning_shown) {
            hide_warning_modal();
        }
        
        // Timer'ları resetle
        reset_timers();
    }
    
    /**
     * Timer'ları sıfırla
     */
    function reset_timers() {
        // Mevcut timer'ları temizle
        if (idle_timer) {
            clearTimeout(idle_timer);
        }
        if (warning_timer) {
            clearTimeout(warning_timer);
        }
        if (countdown_interval) {
            clearInterval(countdown_interval);
        }
        
        // Uyarı timer'ı (timeout - warning_before süre sonra)
        var warning_delay = IDLE_TIMEOUT_MS - WARNING_BEFORE_MS;
        if (warning_delay > 0) {
            warning_timer = setTimeout(show_warning_modal, warning_delay);
        }
        
        // Logout timer'ı
        idle_timer = setTimeout(perform_logout, IDLE_TIMEOUT_MS);
    }
    
    /**
     * Uyarı modal'ını göster
     */
    function show_warning_modal() {
        is_warning_shown = true;
        
        // Modal yoksa oluştur
        if (!$warning_modal) {
            create_warning_modal();
        }
        
        // Modal'ı göster
        $warning_modal.addClass('show');
        $('body').css('overflow', 'hidden');
        
        // Geri sayım başlat
        var remaining_seconds = Math.ceil(WARNING_BEFORE_MS / 1000);
        update_countdown(remaining_seconds);
        
        countdown_interval = setInterval(function() {
            remaining_seconds--;
            if (remaining_seconds <= 0) {
                clearInterval(countdown_interval);
            } else {
                update_countdown(remaining_seconds);
            }
        }, 1000);
    }
    
    /**
     * Geri sayım değerini güncelle
     */
    function update_countdown(seconds) {
        if ($countdown_element) {
            $countdown_element.text(seconds);
        }
    }
    
    /**
     * Uyarı modal'ını gizle
     */
    function hide_warning_modal() {
        is_warning_shown = false;
        
        if ($warning_modal) {
            $warning_modal.removeClass('show');
            $('body').css('overflow', '');
        }
        
        if (countdown_interval) {
            clearInterval(countdown_interval);
        }
    }
    
    /**
     * Uyarı modal'ını oluştur
     */
    function create_warning_modal() {
        var modal_html = [
            '<div class="idle_timeout_modal" id="idleTimeoutModal">',
            '    <div class="idle_timeout_modal__overlay"></div>',
            '    <div class="idle_timeout_modal__content">',
            '        <div class="idle_timeout_modal__icon">',
            '            <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">',
            '                <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>',
            '                <path d="M12 6v6l4 2" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>',
            '            </svg>',
            '        </div>',
            '        <h3 class="idle_timeout_modal__title">Oturum Zaman Aşımı</h3>',
            '        <p class="idle_timeout_modal__message">',
            '            Uzun süredir işlem yapmadığınız için oturumunuz',
            '            <span class="idle_timeout_modal__countdown" id="idleCountdown">15</span>',
            '            saniye içinde sonlandırılacak.',
            '        </p>',
            '        <div class="idle_timeout_modal__actions">',
            '            <button type="button" class="btn btn_primary" id="idleStayBtn">',
            '                Oturumu Devam Ettir',
            '            </button>',
            '            <button type="button" class="btn btn_secondary" id="idleLogoutBtn">',
            '                Çıkış Yap',
            '            </button>',
            '        </div>',
            '    </div>',
            '</div>'
        ].join('\n');
        
        $warning_modal = $(modal_html);
        $('body').append($warning_modal);
        
        $countdown_element = $('#idleCountdown');
        
        // Event handlers
        $('#idleStayBtn').on('click', function() {
            record_activity();
        });
        
        $('#idleLogoutBtn').on('click', function() {
            perform_logout();
        });
    }
    
    /**
     * Logout işlemini gerçekleştir
     */
    function perform_logout() {
        // Timer'ları temizle
        if (idle_timer) clearTimeout(idle_timer);
        if (warning_timer) clearTimeout(warning_timer);
        if (countdown_interval) clearInterval(countdown_interval);
        
        // Event listener'ları kaldır
        $.each(activity_events, function(index, event_name) {
            $(document).off(event_name + '.idleTimeout');
        });
        
        // Modal'ı güncelle - logout yapılıyor mesajı
        if ($warning_modal) {
            $warning_modal.find('.idle_timeout_modal__content').html([
                '<div class="idle_timeout_modal__icon idle_timeout_modal__icon--logout">',
                '    <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">',
                '        <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>',
                '        <polyline points="16,17 21,12 16,7" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>',
                '        <line x1="21" y1="12" x2="9" y2="12" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>',
                '    </svg>',
                '</div>',
                '<h3 class="idle_timeout_modal__title">Oturum Sonlandırılıyor</h3>',
                '<p class="idle_timeout_modal__message">',
                '    Lütfen bekleyin...',
                '</p>'
            ].join('\n'));
            
            $warning_modal.addClass('show');
        }
        
        // AJAX logout
        $.ajax({
            url: '/?handler=Logout',
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    window.location.href = response.redirectUrl || '/';
                } else {
                    // Fallback: direkt yönlendir
                    window.location.href = '/';
                }
            },
            error: function() {
                // Hata durumunda da login sayfasına yönlendir
                window.location.href = '/';
            }
        });
    }
    
    // ========== INITIALIZATION ==========
    
    /**
     * Idle timeout sistemini başlat
     */
    function init_idle_timeout() {
        // Activity event'lerini dinle
        $.each(activity_events, function(index, event_name) {
            $(document).on(event_name + '.idleTimeout', function() {
                record_activity();
            });
        });
        
        // İlk timer'ları başlat
        reset_timers();
        
        console.log('[IdleTimeout] Başlatıldı. Timeout: ' + (IDLE_TIMEOUT_MS / 1000) + ' saniye');
    }
    
    // Sistem başlat
    init_idle_timeout();
});

