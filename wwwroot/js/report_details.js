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
    var $invoices_content = $('#invoices_content');

    // Invoice data cache
    var invoices_data = null;
    var current_invoice_type = null;

    // Reports data cache
    var reports_data = null;

    // ==============================================
    // INITIALIZATION
    // ==============================================
    
    console.log('Report Details JS Loaded'); // Debug
    load_reports_data();
    load_invoices_data();

    // ==============================================
    // URL PARAMETER HANDLING
    // ==============================================
    var urlParams = new URLSearchParams(window.location.search);
    var tabParam = urlParams.get('tab');
    if (tabParam) {
        var $targetTab = $('.report_tab[data-tab="' + tabParam + '"]');
        if ($targetTab.length) {
            // Wait a bit for other inits
            setTimeout(function() {
                $targetTab.click();
            }, 100);
        }
    }

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
        
        // Toggle full-width mode for credit offers and invoices tabs
        if (target_tab === 'credit_offers' || target_tab === 'invoices') {
            $report_page_layout.addClass('report_page_layout--full_width');
        } else {
            $report_page_layout.removeClass('report_page_layout--full_width');
        }
        
        // Reset invoices tab to default state when clicked directly
        if (target_tab === 'invoices') {
            $('#invoice_info_default').show();
            $('#invoices_content').empty();
            current_invoice_type = null;
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
        
        console.log('Seçilen fatura türü:', invoice_type);
        
        // Faturalar sekmesine geç
        $('.report_tab').removeClass('report_tab--active');
        $('[data-tab="invoices"]').addClass('report_tab--active');
        
        $('.report_tab_content').removeClass('report_tab_content--active');
        $('#invoices_tab').addClass('report_tab_content--active');
        
        // Full width modu aktifleştir
        $report_page_layout.addClass('report_page_layout--full_width');
        
        // Fatura içeriğini yükle
        current_invoice_type = invoice_type;
        render_invoice_content(invoice_type);
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
     * Rapor verilerini JSON dosyasından yükle
     */
    function load_reports_data() {
        $report_list_loading.show();
        $report_list.empty();

        $.ajax({
            url: '/data/reports.json',
            method: 'GET',
            dataType: 'json',
            success: function (data) {
                reports_data = data;
                $report_list_loading.hide();
                render_report_list(data.reports || []);
            },
            error: function (xhr, status, error) {
                $report_list_loading.hide();
                console.error('Rapor verileri yüklenemedi:', error);
                $report_list.html('<li class="report_loading">Rapor listesi yüklenemedi.</li>');
            }
        });
    }

    /**
     * Rapor detayını JSON'dan yükle
     */
    function load_report_detail(report_id) {
        // Show panel on mobile
        $report_detail_panel.addClass('active');
        
        // Show loading
        $report_detail_placeholder.hide();
        $report_detail_loading.show();
        $report_detail_content.removeClass('active').empty();

        // Veriler henüz yüklenmediyse bekle
        if (!reports_data) {
            setTimeout(function() {
                load_report_detail(report_id);
            }, 500);
            return;
        }

        // JSON'dan rapor detayını bul
        var report_detail = reports_data.details && reports_data.details[report_id];
        
        if (report_detail) {
                $report_detail_loading.hide();
            render_report_detail(report_detail);
        } else {
                $report_detail_loading.hide();
            console.error('Rapor detayı bulunamadı:', report_id);
                $report_detail_content.addClass('active').html(
                '<p class="report_detail_placeholder">Rapor detayı bulunamadı.</p>'
                );
            }
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

        var html = '';
        
        // Rapor Başlık Bilgileri
        html += '<div class="detail_section">';
        html += '<h3 class="detail_section__title">Rapor Bilgileri</h3>';
        html += '<div class="detail_row">';
        html += '  <span class="detail_row__label">Rapor Adı</span>';
        html += '  <span class="detail_row__value">' + escape_html(detail.name || '-') + '</span>';
        html += '</div>';
        html += '<div class="detail_row">';
        html += '  <span class="detail_row__label">Rapor Türü</span>';
        html += '  <span class="detail_row__value">' + escape_html(detail.type || '-') + '</span>';
        html += '</div>';
        html += '<div class="detail_row">';
        html += '  <span class="detail_row__label">Tarih</span>';
        html += '  <span class="detail_row__value">' + escape_html(detail.date || '-') + '</span>';
        html += '</div>';
        html += '<div class="detail_row">';
        html += '  <span class="detail_row__label">Durum</span>';
        html += '  <span class="detail_row__value">' + escape_html(detail.status || '-') + '</span>';
        html += '</div>';
        if (detail.summary) {
            html += '<div class="detail_row detail_row--full">';
            html += '  <span class="detail_row__label">Özet</span>';
            html += '  <span class="detail_row__value">' + escape_html(detail.summary) + '</span>';
            html += '</div>';
        }
        html += '</div>';

        // Rapor Türüne Göre Özel Detaylar
        if (detail.reportPeriod) {
            html += '<div class="detail_section">';
            html += '<h3 class="detail_section__title">Rapor Dönemi</h3>';
            html += '<div class="detail_row">';
            html += '  <span class="detail_row__label">Dönem</span>';
            html += '  <span class="detail_row__value">' + escape_html(detail.reportPeriod) + '</span>';
            html += '</div>';
            html += '</div>';
        }

        // Kredi Geçmişi Raporu için özel render
        if (detail.creditHistory && Array.isArray(detail.creditHistory)) {
            html += '<div class="detail_section">';
            html += '<h3 class="detail_section__title">Kredi Geçmişi</h3>';
            for (var i = 0; i < detail.creditHistory.length; i++) {
                var credit = detail.creditHistory[i];
                html += '<div class="detail_card">';
                html += '  <div class="detail_card__header">';
                html += '    <strong>' + escape_html(credit.bankName) + '</strong>';
                html += '    <span class="detail_card__badge">' + escape_html(credit.status) + '</span>';
                html += '  </div>';
                html += '  <div class="detail_card__body">';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Kredi Türü</span>';
                html += '      <span class="detail_row__value">' + escape_html(credit.creditType) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Tutar</span>';
                html += '      <span class="detail_row__value">' + format_currency(credit.amount) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Aylık Taksit</span>';
                html += '      <span class="detail_row__value">' + format_currency(credit.monthlyPayment) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Kalan Taksit</span>';
                html += '      <span class="detail_row__value">' + credit.remainingInstallments + ' ay</span>';
                html += '    </div>';
                html += '  </div>';
                html += '</div>';
            }
            html += '</div>';
        }

        // Ödeme Performans Raporu için özel render
        if (detail.paymentHistory && Array.isArray(detail.paymentHistory)) {
            html += '<div class="detail_section">';
            html += '<h3 class="detail_section__title">Ödeme Geçmişi</h3>';
            for (var i = 0; i < detail.paymentHistory.length; i++) {
                var payment = detail.paymentHistory[i];
                html += '<div class="detail_card">';
                html += '  <div class="detail_card__header">';
                html += '    <strong>' + escape_html(payment.month) + '</strong>';
                html += '    <span class="detail_card__badge detail_card__badge--success">' + escape_html(payment.status) + '</span>';
                html += '  </div>';
                html += '  <div class="detail_card__body">';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Tutar</span>';
                html += '      <span class="detail_row__value">' + format_currency(payment.amount) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Vade Tarihi</span>';
                html += '      <span class="detail_row__value">' + format_date(payment.dueDate) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Ödeme Tarihi</span>';
                html += '      <span class="detail_row__value">' + format_date(payment.paymentDate) + '</span>';
                html += '    </div>';
                if (payment.daysEarly !== undefined) {
                    html += '    <div class="detail_row">';
                    html += '      <span class="detail_row__label">Erken Ödeme</span>';
                    html += '      <span class="detail_row__value">' + payment.daysEarly + ' gün önce</span>';
                    html += '    </div>';
                }
                html += '  </div>';
                html += '</div>';
            }
            html += '</div>';
        }

        // Kredi Skoru Değişim Raporu için özel render
        if (detail.scoreHistory && Array.isArray(detail.scoreHistory)) {
            html += '<div class="detail_section">';
            html += '<h3 class="detail_section__title">Skor Geçmişi</h3>';
            for (var i = 0; i < detail.scoreHistory.length; i++) {
                var score = detail.scoreHistory[i];
                var change_class = score.change > 0 ? 'detail_row__value--positive' : score.change < 0 ? 'detail_row__value--negative' : '';
                var change_text = score.change > 0 ? '+' + score.change : score.change.toString();
                html += '<div class="detail_row">';
                html += '  <span class="detail_row__label">' + escape_html(score.month) + '</span>';
                html += '  <span class="detail_row__value ' + change_class + '">' + score.score + ' (' + change_text + ')</span>';
                html += '</div>';
            }
            html += '</div>';
        }

        // Borç Özet Raporu için özel render
        if (detail.credits && Array.isArray(detail.credits)) {
            html += '<div class="detail_section">';
            html += '<h3 class="detail_section__title">Aktif Krediler</h3>';
            for (var i = 0; i < detail.credits.length; i++) {
                var credit = detail.credits[i];
                html += '<div class="detail_card">';
                html += '  <div class="detail_card__header">';
                html += '    <strong>' + escape_html(credit.bankName) + '</strong>';
                html += '    <span class="detail_card__badge">%' + credit.interestRate.toFixed(2) + '</span>';
                html += '  </div>';
                html += '  <div class="detail_card__body">';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Orijinal Tutar</span>';
                html += '      <span class="detail_row__value">' + format_currency(credit.originalAmount) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Kalan Borç</span>';
                html += '      <span class="detail_row__value">' + format_currency(credit.remainingAmount) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Aylık Taksit</span>';
                html += '      <span class="detail_row__value">' + format_currency(credit.monthlyPayment) + '</span>';
                html += '    </div>';
                html += '    <div class="detail_row">';
                html += '      <span class="detail_row__label">Tamamlanma</span>';
                html += '      <span class="detail_row__value">%' + credit.completionPercentage.toFixed(1) + '</span>';
                html += '    </div>';
                html += '  </div>';
                html += '</div>';
            }
            html += '</div>';
        }

        // Genel sayısal değerler
        var numeric_fields = ['totalCredits', 'activeCredits', 'completedCredits', 'totalBorrowed', 'totalPaid', 'remainingDebt', 
                             'averageInterestRate', 'onTimePaymentRate', 'creditScore', 'totalPayments', 'onTimePayments', 
                             'latePayments', 'totalDebt', 'totalMonthlyPayment', 'debtToIncomeRatio', 'currentScore', 
                             'previousScore', 'scoreChange', 'totalIncome', 'totalExpenses', 'savings', 'savingsRate',
                             'totalApplications', 'approvedApplications', 'rejectedApplications', 'pendingApplications',
                             'approvalRate', 'riskScore'];
        
        var has_numeric_data = false;
        for (var j = 0; j < numeric_fields.length; j++) {
            if (detail[numeric_fields[j]] !== undefined) {
                has_numeric_data = true;
                break;
            }
        }

        if (has_numeric_data) {
            html += '<div class="detail_section">';
            html += '<h3 class="detail_section__title">İstatistikler</h3>';
            for (var key in detail) {
                if (detail.hasOwnProperty(key) && numeric_fields.indexOf(key) !== -1) {
                    var display_key = format_key_name(key);
                    var display_value = format_numeric_value(key, detail[key]);
                html += '<div class="detail_row">';
                html += '  <span class="detail_row__label">' + escape_html(display_key) + '</span>';
                html += '  <span class="detail_row__value">' + escape_html(display_value) + '</span>';
                html += '</div>';
            }
            }
            html += '</div>';
        }

        // Öneriler
        if (detail.recommendations && Array.isArray(detail.recommendations)) {
            html += '<div class="detail_section">';
            html += '<h3 class="detail_section__title">Öneriler</h3>';
            html += '<ul class="detail_list">';
            for (var k = 0; k < detail.recommendations.length; k++) {
                html += '<li class="detail_list__item">' + escape_html(detail.recommendations[k]) + '</li>';
            }
            html += '</ul>';
        html += '</div>';
        }
        
        $report_detail_content.addClass('active').html(html);
    }

    // ==============================================
    // INVOICE FUNCTIONS
    // ==============================================
    
    /**
     * Fatura verilerini JSON'dan yükle
     */
    function load_invoices_data() {
        $.ajax({
            url: '/data/invoices.json',
            method: 'GET',
            dataType: 'json',
            success: function (data) {
                invoices_data = data;
                console.log('Fatura verileri yüklendi');
            },
            error: function (xhr, status, error) {
                console.error('Fatura verileri yüklenemedi:', error);
            }
        });
    }

    /**
     * Seçilen fatura türüne göre içeriği render et
     */
    function render_invoice_content(invoice_type) {
        var $content = $('#invoices_content');
        var $default_info = $('#invoice_info_default');
        
        // Varsayılan bilgi kutusunu gizle
        $default_info.hide();
        
        if (!invoices_data) {
            $content.html('<div class="invoice_loading"><div class="spinner"></div><span>Faturalar yükleniyor...</span></div>');
            // Veriler henüz yüklenmediyse bekle
            setTimeout(function() {
                render_invoice_content(invoice_type);
            }, 500);
            return;
        }
        
        var invoice_group = invoices_data[invoice_type];
        
        if (!invoice_group) {
            $content.html('<div class="invoice_empty_state"><svg viewBox="0 0 24 24" fill="none"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg><p>Bu fatura türü için veri bulunamadı.</p></div>');
            return;
        }
        
        var html = '';
        
        // Başlık
        html += '<div class="invoice_section_header">';
        html += '  <div class="invoice_section_header__icon">' + get_invoice_icon(invoice_type) + '</div>';
        html += '  <div class="invoice_section_header__info">';
        html += '    <h2 class="invoice_section_header__title">' + escape_html(invoice_group.title) + '</h2>';
        html += '    <span class="invoice_section_header__count">' + invoice_group.items.length + ' fatura bulundu</span>';
        html += '  </div>';
        html += '</div>';
        
        if (invoice_group.items.length === 0) {
            html += '<div class="invoice_empty_state">';
            html += '  <svg viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/><path d="M8 14s1.5 2 4 2 4-2 4-2M9 9h.01M15 9h.01" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';
            html += '  <p>Bu kategoride bekleyen faturanız bulunmamaktadır.</p>';
            html += '</div>';
        } else {
            html += '<div class="invoice_cards_grid">';
            
            for (var i = 0; i < invoice_group.items.length; i++) {
                var item = invoice_group.items[i];
                var status_class = get_status_class(item.status);
                var status_text = get_status_text(item.status);
                var formatted_amount = format_currency(item.amount);
                var is_actionable = item.status === 'pending' || item.status === 'overdue';
                
                html += '<div class="invoice_card invoice_card--' + item.status + '">';
                html += '  <div class="invoice_card__header">';
                html += '    <span class="invoice_card__provider">' + escape_html(item.provider) + '</span>';
                html += '    <span class="invoice_card__status ' + status_class + '">' + status_text + '</span>';
                html += '  </div>';
                html += '  <div class="invoice_card__body">';
                html += '    <div class="invoice_card__amount">' + formatted_amount + '</div>';
                html += '    <div class="invoice_card__period">' + escape_html(item.period) + '</div>';
                html += '  </div>';
                html += '  <div class="invoice_card__details">';
                html += '    <div class="invoice_card__detail">';
                html += '      <span>Abone No</span>';
                html += '      <strong>' + escape_html(item.subscriber_no) + '</strong>';
                html += '    </div>';
                html += '    <div class="invoice_card__detail">';
                html += '      <span>Tüketim</span>';
                html += '      <strong>' + escape_html(item.consumption) + '</strong>';
                html += '    </div>';
                html += '    <div class="invoice_card__detail">';
                html += '      <span>Son Ödeme</span>';
                html += '      <strong>' + format_date(item.due_date) + '</strong>';
                html += '    </div>';
                html += '  </div>';
                
                if (is_actionable) {
                    html += '  <button class="invoice_card__pay_btn" data-invoice-id="' + item.id + '">';
                    html += '    <svg viewBox="0 0 24 24" fill="none"><rect x="1" y="4" width="22" height="16" rx="2" stroke="currentColor" stroke-width="2"/><path d="M1 10h22" stroke="currentColor" stroke-width="2"/></svg>';
                    html += '    <span>Ödeme Yap</span>';
                    html += '  </button>';
                } else {
                    html += '  <div class="invoice_card__paid_badge">';
                    html += '    <svg viewBox="0 0 24 24" fill="none"><path d="M20 6L9 17l-5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>';
                    html += '    <span>Ödendi</span>';
                    html += '  </div>';
                }
                
                html += '</div>';
            }
            
            html += '</div>';
        }
        
        $content.html(html);
    }

    /**
     * Fatura türüne göre ikon SVG döndür
     */
    function get_invoice_icon(type) {
        var icons = {
            'elektrik': '<svg viewBox="0 0 24 24" fill="none"><path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'su': '<svg viewBox="0 0 24 24" fill="none"><path d="M12 2.69l5.66 5.66a8 8 0 1 1-11.31 0z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'dogalgaz': '<svg viewBox="0 0 24 24" fill="none"><path d="M12 12c-3 0-4 3-4 5s2 4 4 4 4-2 4-4-1-5-4-5z" stroke="currentColor" stroke-width="2"/><path d="M12 2v4M12 8c-2.5 0-3.5 2-3.5 4" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>',
            'gsm': '<svg viewBox="0 0 24 24" fill="none"><rect x="5" y="2" width="14" height="20" rx="2" stroke="currentColor" stroke-width="2"/><path d="M12 18h.01" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>',
            'internet': '<svg viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/><path d="M2 12h20M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" stroke="currentColor" stroke-width="2"/></svg>',
            'tv': '<svg viewBox="0 0 24 24" fill="none"><rect x="2" y="7" width="20" height="15" rx="2" stroke="currentColor" stroke-width="2"/><path d="M17 2l-5 5-5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'sabit_telefon': '<svg viewBox="0 0 24 24" fill="none"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z" stroke="currentColor" stroke-width="2"/></svg>',
            'mtv': '<svg viewBox="0 0 24 24" fill="none"><path d="M5 17h14v-4.172a2 2 0 0 0-.586-1.414l-1.828-1.828A2 2 0 0 0 15.172 9H8.828a2 2 0 0 0-1.414.586l-1.828 1.828A2 2 0 0 0 5 12.828V17z" stroke="currentColor" stroke-width="2"/><circle cx="7.5" cy="17.5" r="1.5" stroke="currentColor" stroke-width="2"/><circle cx="16.5" cy="17.5" r="1.5" stroke="currentColor" stroke-width="2"/></svg>',
            'trafik_cezasi': '<svg viewBox="0 0 24 24" fill="none"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" stroke="currentColor" stroke-width="2"/><path d="M12 9v4M12 17h.01" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>',
            'sgk': '<svg viewBox="0 0 24 24" fill="none"><path d="M3 21h18M3 10h18M5 6l7-3 7 3M4 10v11M20 10v11M8 14v3M12 14v3M16 14v3" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'vergi': '<svg viewBox="0 0 24 24" fill="none"><path d="M3 21h18M3 10h18M5 6l7-3 7 3M4 10v11M20 10v11M8 14v3M12 14v3M16 14v3" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'universite': '<svg viewBox="0 0 24 24" fill="none"><path d="M22 10v6M2 10l10-5 10 5-10 5z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/><path d="M6 12v5c3 3 9 3 12 0v-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'ozel_okul': '<svg viewBox="0 0 24 24" fill="none"><path d="M22 10v6M2 10l10-5 10 5-10 5z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/><path d="M6 12v5c3 3 9 3 12 0v-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'aidat': '<svg viewBox="0 0 24 24" fill="none"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/><path d="M9 22V12h6v10" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'bagis': '<svg viewBox="0 0 24 24" fill="none"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'sigorta': '<svg viewBox="0 0 24 24" fill="none"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
            'belediye': '<svg viewBox="0 0 24 24" fill="none"><path d="M3 21h18M3 10h18M5 6l7-3 7 3M4 10v11M20 10v11M8 14v3M12 14v3M16 14v3" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>'
        };
        return icons[type] || icons['elektrik'];
    }

    /**
     * Durum sınıfı döndür
     */
    function get_status_class(status) {
        var classes = {
            'pending': 'invoice_card__status--pending',
            'paid': 'invoice_card__status--paid',
            'overdue': 'invoice_card__status--overdue'
        };
        return classes[status] || classes['pending'];
    }

    /**
     * Durum metni döndür
     */
    function get_status_text(status) {
        var texts = {
            'pending': 'Bekliyor',
            'paid': 'Ödendi',
            'overdue': 'Gecikmiş'
        };
        return texts[status] || 'Bekliyor';
    }

    /**
     * Para formatla
     */
    function format_currency(amount) {
        if (amount === 0) return 'Ücretsiz';
        return amount.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ₺';
    }

    /**
     * Tarih formatla
     */
    function format_date(date_str) {
        if (!date_str || date_str === '-') return '-';
        var parts = date_str.split('-');
        if (parts.length !== 3) return date_str;
        return parts[2] + '.' + parts[1] + '.' + parts[0];
    }

    // ==============================================
    // INVOICE PAY BUTTON
    // ==============================================
    
    $(document).on('click', '.invoice_card__pay_btn', function (e) {
        e.preventDefault();
        var $btn = $(this);
        var invoice_id = $btn.data('invoice-id');
        
        // Button disable for double click prevention
        $btn.prop('disabled', true).html('<div class="spinner spinner--small"></div><span>İşleniyor...</span>');
        
        // Simulated payment redirect
        setTimeout(function() {
            alert('Fatura ID: ' + invoice_id + '\n\nÖdeme sayfasına yönlendiriliyorsunuz...');
            $btn.prop('disabled', false).html('<svg viewBox="0 0 24 24" fill="none"><rect x="1" y="4" width="22" height="16" rx="2" stroke="currentColor" stroke-width="2"/><path d="M1 10h22" stroke="currentColor" stroke-width="2"/></svg><span>Ödeme Yap</span>');
        }, 800);
    });

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

    function format_numeric_value(key, value) {
        if (value === null || value === undefined) return '-';
        
        // Para birimi gerektiren alanlar
        var currency_fields = ['totalBorrowed', 'totalPaid', 'remainingDebt', 'totalPaidAmount', 
                              'totalDebt', 'totalMonthlyPayment', 'originalAmount', 'remainingAmount',
                              'monthlyPayment', 'amount', 'totalIncome', 'totalExpenses', 'savings'];
        if (currency_fields.indexOf(key) !== -1) {
            return format_currency(value);
        }
        
        // Yüzde gerektiren alanlar
        var percentage_fields = ['averageInterestRate', 'onTimePaymentRate', 'savingsRate', 
                                'approvalRate', 'debtToIncomeRatio', 'completionPercentage', 'interestRate'];
        if (percentage_fields.indexOf(key) !== -1) {
            return '%' + parseFloat(value).toFixed(2);
        }
        
        // Skor değişimi
        if (key === 'scoreChange' || key === 'creditScoreChange') {
            var change = parseFloat(value);
            return change > 0 ? '+' + change : change.toString();
        }
        
        return String(value);
    }
});
