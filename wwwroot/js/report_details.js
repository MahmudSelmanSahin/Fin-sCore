/**
 * Report Details Page Script
 * Ana site tasarımına uyumlu
 */

$(document).ready(function () {
    // ==============================================
    // API CONFIGURATION (Loaded from external config file)
    // ==============================================
    
    // API_CONFIG is loaded from wwwroot/js/config/api_config.js
    var api_config = typeof API_CONFIG !== 'undefined' ? API_CONFIG : {
        report_list: '',
        report_detail: ''
    };

    // ==============================================
    // DOM CACHE
    // ==============================================
    
    var $report_list = $('#report_list');
    var $report_list_loading = $('#report_list_loading');
    var $report_detail_panel = $('#report_detail_panel');
    var $report_detail_content = $('#report_detail_content');
    var $report_detail_loading = $('#report_detail_loading');
    var $report_detail_placeholder = $('#report_detail_placeholder');
    var $close_panel_btn = $('#close_panel_btn');
    var $report_page_layout = $('#report_page_layout');

    // ==============================================
    // INITIALIZATION
    // ==============================================
    
    console.log('Report Details JS Loaded'); // Debug
    load_report_list();

    // ==============================================
    // TAB NAVIGATION (Event Delegation)
    // ==============================================
    
    $(document).on('click', '.report_tab', function (e) {
        e.preventDefault();
        e.stopPropagation();
        
        var $clicked_tab = $(this);
        var target_tab = $clicked_tab.data('tab');
        
        console.log('Tab clicked:', target_tab); // Debug log
        
        // Update tab states
        $('.report_tab').removeClass('report_tab--active');
        $clicked_tab.addClass('report_tab--active');
        
        // Update content visibility - only use classes
        $('.report_tab_content').removeClass('report_tab_content--active');
        var $target_content = $('#' + target_tab + '_tab');
        $target_content.addClass('report_tab_content--active');
        
        console.log('Target content found:', $target_content.length, 'ID:', target_tab + '_tab'); // Debug log
        
        // Hide detail panel when switching tabs
        hide_detail_panel();
        
        // Toggle full-width mode for credit offers tab
        if (target_tab === 'credit_offers') {
            $report_page_layout.addClass('report_page_layout--full_width');
        } else {
            $report_page_layout.removeClass('report_page_layout--full_width');
        }
    });

    // ==============================================
    // REPORT LIST CLICK (Event Delegation)
    // ==============================================
    
    $(document).on('click', '.report_list__item', function () {
        var report_id = $(this).data('report-id');
        
        // Update active state
        $('.report_list__item').removeClass('report_list__item--active');
        $(this).addClass('report_list__item--active');
        
        // Load report detail
        load_report_detail(report_id);
    });

    // ==============================================
    // CLOSE PANEL
    // ==============================================
    
    $close_panel_btn.on('click', function () {
        hide_detail_panel();
    });

    // ==============================================
    // INVOICE DROPDOWN - Fatura Seçimi
    // ==============================================
    
    $(document).on('click', '.invoice_dropdown__list a', function (e) {
        e.preventDefault();
        
        var invoice_type = $(this).data('invoice');
        var invoice_name = $(this).text();
        
        // Faturalar sekmesine geç
        $report_tabs.removeClass('report_tab--active');
        $('[data-tab="invoices"]').addClass('report_tab--active');
        
        $tab_contents.removeClass('report_tab_content--active').hide();
        $('#invoices_tab').addClass('report_tab_content--active').show();
        
        // Burada seçilen faturaya göre işlem yapılabilir
        console.log('Seçilen fatura türü:', invoice_type);
        alert('Seçilen fatura: ' + invoice_name + '\n\nÖdeme sayfasına yönlendiriliyorsunuz...');
    });

    // ==============================================
    // CREDIT ROW TOGGLE (Accordion)
    // ==============================================
    
    $(document).on('click', '.credit_row__header', function () {
        var $credit_row = $(this).closest('.credit_row');
        
        // Toggle collapsed class
        $credit_row.toggleClass('collapsed');
    });

    // ==============================================
    // BANK CARD APPLY BUTTON
    // ==============================================
    
    $(document).on('click', '.bank_card__btn', function (e) {
        e.stopPropagation(); // Prevent row toggle
        
        var $card = $(this).closest('.bank_card');
        var bank_name = $card.find('.bank_card__name').text();
        var credit_type = $card.closest('.credit_row').find('.credit_row__title').text();
        var rate = $card.find('.bank_card__rate').text();
        
        // Button disable for double click prevention
        var $btn = $(this);
        $btn.prop('disabled', true).text('Yönlendiriliyor...');
        
        // Simulated redirect (in real app, this would be an API call or redirect)
        setTimeout(function() {
            alert('Başvuru: ' + credit_type + '\nBanka: ' + bank_name + '\nFaiz Oranı: ' + rate + '\n\nBanka başvuru sayfasına yönlendiriliyorsunuz...');
            $btn.prop('disabled', false).text('Başvur');
        }, 500);
    });

    // ==============================================
    // API FUNCTIONS
    // ==============================================
    
    /**
     * Rapor listesini API'den yükle
     */
    function load_report_list() {
        $report_list_loading.show();
        $report_list.empty();

        $.ajax({
            url: api_config.report_list,
            method: 'GET',
            dataType: 'json',
            success: function (data) {
                $report_list_loading.hide();
                render_report_list(data);
            },
            error: function (xhr, status, error) {
                $report_list_loading.hide();
                console.error('Rapor listesi hatası:', error);
                $report_list.html('<li class="report_loading">Rapor listesi yüklenemedi.</li>');
            }
        });
    }

    /**
     * Rapor detayını API'den yükle
     */
    function load_report_detail(report_id) {
        // Show panel on mobile
        $report_detail_panel.addClass('active');
        
        // Show loading
        $report_detail_placeholder.hide();
        $report_detail_loading.show();
        $report_detail_content.removeClass('active').empty();

        var detail_url = api_config.report_detail + '&reportId=' + encodeURIComponent(report_id);

        $.ajax({
            url: detail_url,
            method: 'GET',
            dataType: 'json',
            success: function (data) {
                $report_detail_loading.hide();
                render_report_detail(data);
            },
            error: function (xhr, status, error) {
                $report_detail_loading.hide();
                console.error('Rapor detay hatası:', error);
                $report_detail_content.addClass('active').html(
                    '<p class="report_detail_placeholder">Rapor detayı yüklenemedi.</p>'
                );
            }
        });
    }

    // ==============================================
    // RENDER FUNCTIONS
    // ==============================================
    
    /**
     * Rapor listesini render et
     */
    function render_report_list(reports) {
        if (!reports || reports.length === 0) {
            $report_list.html('<li class="report_loading">Rapor bulunamadı.</li>');
            return;
        }

        var html = '';
        
        for (var i = 0; i < reports.length; i++) {
            var report = reports[i];
            var report_id = report.id || report.reportId || (i + 1);
            var report_name = report.name || report.reportName || report.baslik || 'Rapor ' + (i + 1);
            var report_date = report.date || report.createdDate || report.tarih || '-';
            var report_type = report.type || report.reportType || report.tur || 'Genel';

            html += '<li class="report_list__item" data-report-id="' + report_id + '">';
            html += '  <div class="report_list__info">';
            html += '    <p class="report_list__name">' + escape_html(report_name) + '</p>';
            html += '    <span class="report_list__meta">' + escape_html(report_type) + ' • ' + escape_html(report_date) + '</span>';
            html += '  </div>';
            html += '  <span class="report_list__arrow">→</span>';
            html += '</li>';
        }

        $report_list.html(html);
    }

    /**
     * Rapor detayını render et
     */
    function render_report_detail(detail) {
        if (!detail) {
            $report_detail_content.addClass('active').html(
                '<p class="report_detail_placeholder">Rapor detayı bulunamadı.</p>'
            );
            return;
        }

        var html = '<div class="detail_section">';
        html += '<h3 class="detail_section__title">Rapor Bilgileri</h3>';

        for (var key in detail) {
            if (detail.hasOwnProperty(key)) {
                var value = detail[key];
                
                // Array'leri atla
                if (Array.isArray(value)) continue;
                
                var display_key = format_key_name(key);
                var display_value = format_value(value);
                
                html += '<div class="detail_row">';
                html += '  <span class="detail_row__label">' + escape_html(display_key) + '</span>';
                html += '  <span class="detail_row__value">' + escape_html(display_value) + '</span>';
                html += '</div>';
            }
        }

        html += '</div>';
        
        $report_detail_content.addClass('active').html(html);
    }

    // ==============================================
    // HELPER FUNCTIONS
    // ==============================================
    
    function hide_detail_panel() {
        $report_detail_panel.removeClass('active');
        $('.report_list__item').removeClass('report_list__item--active');
        $report_detail_content.removeClass('active').empty();
        $report_detail_placeholder.show();
    }

    function escape_html(str) {
        if (str === null || str === undefined) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    function format_key_name(key) {
        return key
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, function(str) { return str.toUpperCase(); })
            .trim();
    }

    function format_value(value) {
        if (value === null || value === undefined) return '-';
        if (typeof value === 'boolean') return value ? 'Evet' : 'Hayır';
        if (typeof value === 'object') return JSON.stringify(value);
        return String(value);
    }
});
