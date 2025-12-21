/**
 * Loan Calculator JavaScript
 * Real-time calculation with Chart.js visualization
 */

document.addEventListener('DOMContentLoaded', function() {
    // DOM Elements
    const loanAmountInput = document.getElementById('loanAmountInput');
    const loanAmountSlider = document.getElementById('loanAmountSlider');
    const termInput = document.getElementById('termInput');
    const interestRateInput = document.getElementById('interestRateInput');
    
    const monthlyPaymentEl = document.getElementById('monthlyPayment');
    const totalPaymentEl = document.getElementById('totalPayment');
    const totalInterestEl = document.getElementById('totalInterest');
    const principalPercentEl = document.getElementById('principalPercent');
    const interestPercentEl = document.getElementById('interestPercent');
    const amortizationBody = document.getElementById('amortizationBody');
    
    const amortizationToggle = document.getElementById('amortizationToggle');
    const amortizationSection = document.querySelector('.amortization_section');
    
    // Chart instance
    let loanChart = null;
    
    // Initialize Chart
    function initChart() {
        const ctx = document.getElementById('loanChart').getContext('2d');
        loanChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Anapara', 'Faiz'],
                datasets: [{
                    data: [100, 0],
                    backgroundColor: ['#2E6DF8', '#f59e0b'],
                    borderWidth: 0,
                    cutout: '65%'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return context.label + ': ' + context.parsed.toFixed(1) + '%';
                            }
                        }
                    }
                }
            }
        });
    }
    
    // Format number as Turkish currency
    function formatCurrency(number) {
        return new Intl.NumberFormat('tr-TR', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        }).format(number) + ' ₺';
    }
    
    // Parse Turkish formatted number
    function parseCurrency(str) {
        if (!str) return 0;
        return parseFloat(str.replace(/\./g, '').replace(',', '.').replace(/[^0-9.-]/g, '')) || 0;
    }
    
    // Format number with thousand separators
    function formatNumber(number) {
        return new Intl.NumberFormat('tr-TR', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(number);
    }
    
    // Update slider progress visual
    function updateSliderProgress() {
        const min = parseFloat(loanAmountSlider.min);
        const max = parseFloat(loanAmountSlider.max);
        const value = parseFloat(loanAmountSlider.value);
        const progress = ((value - min) / (max - min)) * 100;
        loanAmountSlider.style.setProperty('--slider-progress', progress + '%');
    }
    
    // Calculate loan
    function calculateLoan() {
        const principal = parseCurrency(loanAmountInput.value);
        const months = parseInt(termInput.value) || 1;
        const monthlyRate = parseFloat(interestRateInput.value) / 100;
        
        if (principal <= 0 || months <= 0 || monthlyRate <= 0) {
            resetResults();
            return;
        }
        
        // Monthly payment formula: M = P * [r(1+r)^n] / [(1+r)^n - 1]
        const denominator = Math.pow(1 + monthlyRate, months) - 1;
        const monthlyPayment = principal * (monthlyRate * Math.pow(1 + monthlyRate, months)) / denominator;
        
        const totalPayment = monthlyPayment * months;
        const totalInterest = totalPayment - principal;
        
        // Update display
        monthlyPaymentEl.textContent = formatCurrency(monthlyPayment);
        totalPaymentEl.textContent = formatCurrency(totalPayment);
        totalInterestEl.textContent = formatCurrency(totalInterest);
        
        // Calculate percentages for chart
        const principalPercent = (principal / totalPayment) * 100;
        const interestPercent = (totalInterest / totalPayment) * 100;
        
        principalPercentEl.textContent = principalPercent.toFixed(1) + '%';
        interestPercentEl.textContent = interestPercent.toFixed(1) + '%';
        
        // Update chart
        if (loanChart) {
            loanChart.data.datasets[0].data = [principalPercent, interestPercent];
            loanChart.update();
        }
        
        // Generate amortization schedule
        generateAmortizationTable(principal, monthlyPayment, monthlyRate, months);
    }
    
    // Reset results
    function resetResults() {
        monthlyPaymentEl.textContent = '0 ₺';
        totalPaymentEl.textContent = '0 ₺';
        totalInterestEl.textContent = '0 ₺';
        principalPercentEl.textContent = '0%';
        interestPercentEl.textContent = '0%';
        
        if (loanChart) {
            loanChart.data.datasets[0].data = [100, 0];
            loanChart.update();
        }
        
        amortizationBody.innerHTML = '';
    }
    
    // Generate amortization table
    function generateAmortizationTable(principal, monthlyPayment, monthlyRate, months) {
        let balance = principal;
        let html = '';
        
        for (let month = 1; month <= months; month++) {
            const interestPayment = balance * monthlyRate;
            const principalPayment = monthlyPayment - interestPayment;
            balance = Math.max(0, balance - principalPayment);
            
            // Fix for last month - ensure balance becomes exactly 0
            if (month === months) {
                balance = 0;
            }
            
            html += `
                <tr>
                    <td>${month}</td>
                    <td>${formatCurrency(monthlyPayment)}</td>
                    <td>${formatCurrency(principalPayment)}</td>
                    <td>${formatCurrency(interestPayment)}</td>
                    <td>${formatCurrency(balance)}</td>
                </tr>
            `;
        }
        
        amortizationBody.innerHTML = html;
    }
    
    // Sync slider with input
    function syncSliderToInput() {
        const value = parseCurrency(loanAmountInput.value);
        const clampedValue = Math.max(1000, Math.min(500000, value));
        loanAmountSlider.value = clampedValue;
        updateSliderProgress();
    }
    
    // Sync input with slider
    function syncInputToSlider() {
        const value = parseInt(loanAmountSlider.value);
        loanAmountInput.value = formatNumber(value);
        updateSliderProgress();
    }
    
    // Event Listeners
    
    // Amount input
    loanAmountInput.addEventListener('input', function() {
        syncSliderToInput();
        calculateLoan();
    });
    
    loanAmountInput.addEventListener('blur', function() {
        const value = parseCurrency(this.value);
        const clampedValue = Math.max(1000, Math.min(500000, value));
        this.value = formatNumber(clampedValue);
        loanAmountSlider.value = clampedValue;
        updateSliderProgress();
        calculateLoan();
    });
    
    // Amount slider
    loanAmountSlider.addEventListener('input', function() {
        syncInputToSlider();
        calculateLoan();
    });
    
    // Term input
    termInput.addEventListener('input', calculateLoan);
    
    termInput.addEventListener('blur', function() {
        let value = parseInt(this.value);
        if (isNaN(value) || value < 1) value = 1;
        if (value > 120) value = 120;
        this.value = value;
        calculateLoan();
    });
    
    // Interest rate input
    interestRateInput.addEventListener('input', calculateLoan);
    
    interestRateInput.addEventListener('blur', function() {
        let value = parseFloat(this.value);
        if (isNaN(value) || value < 0.1) value = 0.1;
        if (value > 10) value = 10;
        this.value = value.toFixed(1);
        calculateLoan();
    });
    
    // Amortization toggle
    amortizationToggle.addEventListener('click', function() {
        amortizationSection.classList.toggle('is-open');
    });
    
    // Initialize
    initChart();
    updateSliderProgress();
    calculateLoan();
});
