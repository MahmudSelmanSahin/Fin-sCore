// Dashboard JavaScript
$(document).ready(function() {
    // Cache selectors
    var $dashboard_cards = $('.dashboard_card');

    // Add hover effect enhancement
    $dashboard_cards.on('mouseenter', function() {
        var $card = $(this);
        $card.addClass('is_hovered');
    });

    $dashboard_cards.on('mouseleave', function() {
        var $card = $(this);
        $card.removeClass('is_hovered');
    });

    // Card click with confetti effect
    $dashboard_cards.on('click', function(e) {
        var $card = $(this);
        var card_title = $card.find('.dashboard_card__title').text();
        
        // Get card position for confetti origin
        var card_rect = this.getBoundingClientRect();
        var origin_x = card_rect.left + card_rect.width / 2;
        var origin_y = card_rect.top + card_rect.height / 2;
        
        // Trigger confetti
        create_confetti(origin_x, origin_y);
        
        // Add success flash effect
        $card.addClass('card_clicked');
        setTimeout(function() {
            $card.removeClass('card_clicked');
        }, 600);
        
        console.log('Card clicked: ' + card_title);
    });
});

// Confetti Animation System
function create_confetti(origin_x, origin_y) {
    var confetti_count = 50;
    var colors = ['#2E6DF8', '#0056B3', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899'];
    var confetti_elements = [];
    
    for (var i = 0; i < confetti_count; i++) {
        var confetti = document.createElement('div');
        confetti.className = 'confetti_piece';
        
        // Random color
        var random_color = colors[Math.floor(Math.random() * colors.length)];
        confetti.style.backgroundColor = random_color;
        
        // Random size
        var size = Math.random() * 8 + 4;
        confetti.style.width = size + 'px';
        confetti.style.height = size + 'px';
        
        // Starting position
        confetti.style.left = origin_x + 'px';
        confetti.style.top = origin_y + 'px';
        
        // Random angle and velocity
        var angle = Math.random() * Math.PI * 2;
        var velocity = Math.random() * 300 + 150;
        var velocity_x = Math.cos(angle) * velocity;
        var velocity_y = Math.sin(angle) * velocity - 200;
        
        // Random rotation
        var rotation = Math.random() * 360;
        var rotation_speed = Math.random() * 600 - 300;
        
        confetti.setAttribute('data-vx', velocity_x);
        confetti.setAttribute('data-vy', velocity_y);
        confetti.setAttribute('data-rotation', rotation);
        confetti.setAttribute('data-rotation-speed', rotation_speed);
        
        document.body.appendChild(confetti);
        confetti_elements.push(confetti);
    }
    
    // Animate confetti
    animate_confetti(confetti_elements);
}

function animate_confetti(confetti_elements) {
    var start_time = Date.now();
    var duration = 2000;
    var gravity = 800;
    
    function update() {
        var elapsed = Date.now() - start_time;
        var progress = elapsed / duration;
        
        if (progress >= 1) {
            // Remove all confetti
            confetti_elements.forEach(function(confetti) {
                if (confetti.parentNode) {
                    confetti.parentNode.removeChild(confetti);
                }
            });
            return;
        }
        
        confetti_elements.forEach(function(confetti) {
            var velocity_x = parseFloat(confetti.getAttribute('data-vx'));
            var velocity_y = parseFloat(confetti.getAttribute('data-vy'));
            var rotation = parseFloat(confetti.getAttribute('data-rotation'));
            var rotation_speed = parseFloat(confetti.getAttribute('data-rotation-speed'));
            
            // Update velocity (gravity effect)
            velocity_y += gravity * 0.016;
            confetti.setAttribute('data-vy', velocity_y);
            
            // Update position
            var current_x = parseFloat(confetti.style.left);
            var current_y = parseFloat(confetti.style.top);
            confetti.style.left = (current_x + velocity_x * 0.016) + 'px';
            confetti.style.top = (current_y + velocity_y * 0.016) + 'px';
            
            // Update rotation
            rotation += rotation_speed * 0.016;
            confetti.setAttribute('data-rotation', rotation);
            confetti.style.transform = 'rotate(' + rotation + 'deg)';
            
            // Fade out
            confetti.style.opacity = 1 - progress;
        });
        
        requestAnimationFrame(update);
    }
    
    requestAnimationFrame(update);
}

