// Navbar Fixed Scroll Behavior
// .cursorrules uyumlu: snake_case isimlendirme

$(document).ready(function() {
    var $navbar = $('#mainNavbar');
    var scroll_threshold = 20;
    var last_scroll_position = 0;
    
    /**
     * Scroll event handler - navbar'ı scroll ile küçült
     */
    $(window).on('scroll', function() {
        var scroll_position = $(window).scrollTop();
        
        // Aşağı scroll yaparken navbar'ı küçült
        if (scroll_position > scroll_threshold) {
            $navbar.addClass('scrolled');
        } else {
            $navbar.removeClass('scrolled');
        }
        
        last_scroll_position = scroll_position;
    });
    
    /**
     * Smooth scroll to sections
     */
    $('a[href^="#"]').on('click', function(e) {
        var target = $(this).attr('href');
        
        if (target !== '#' && $(target).length) {
            e.preventDefault();
            
            var navbar_height = $navbar.outerHeight();
            var target_position = $(target).offset().top - navbar_height - 20;
            
            $('html, body').animate({
                scrollTop: target_position
            }, 600);
        }
    });
    
    /**
     * Sayfa yüklendiğinde navbar yüksekliğini hesapla
     */
    function update_navbar_spacer() {
        var navbar_height = $navbar.outerHeight();
        $('.dashboard__navbar_spacer').css('height', navbar_height + 'px');
    }
    
    // Initial call
    update_navbar_spacer();
    
    // Window resize'da yeniden hesapla
    $(window).on('resize', function() {
        update_navbar_spacer();
    });
});

