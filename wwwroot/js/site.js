// Global site JavaScript
$(document).ready(function() {
    // Add smooth scroll behavior
    $('a[href^="#"]').on('click', function(e) {
        const target = $(this.getAttribute('href'));
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
        const $submit_btn = $form.find('button[type="submit"]');
        
        if (!$submit_btn.hasClass('loading')) {
            $submit_btn.prop('disabled', true);
        }
    });

    // Input focus animations
    $('.form_input, .code_input').on('focus', function() {
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
