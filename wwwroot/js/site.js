// Global site JavaScript
$(document).ready(function() {
    // Add smooth scroll behavior
    $('a[href^="#"]').on('click', function(e) {
        const href = this.getAttribute('href');
        
        // Skip if href is just "#" or empty
        if (!href || href === '#') {
            return;
        }
        
        const target = $(href);
        if (target.length) {
            e.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 20
            }, 600);
        }
    });

    // Add loading states to forms
    $('form').on('submit', function() {
        const $form = $(this);
        const $submitBtn = $form.find('button[type="submit"]');
        
        if (!$submitBtn.hasClass('loading')) {
            $submitBtn.prop('disabled', true);
        }
    });

    // Input focus animations
    $('.form-input, .code-input').on('focus', function() {
        $(this).parent().addClass('focused');
    }).on('blur', function() {
        $(this).parent().removeClass('focused');
    });

    // Auto-format phone numbers
    $('input[type="text"]').on('input', function() {
        const $input = $(this);
        const value = $input.val();
        
        // Format Turkish phone numbers
        if ($input.attr('placeholder') && $input.attr('placeholder').includes('05XX')) {
            const cleaned = value.replace(/\D/g, '');
            if (cleaned.length > 0 && !cleaned.startsWith('0')) {
                $input.val('0' + cleaned);
            }
        }
    });
});

