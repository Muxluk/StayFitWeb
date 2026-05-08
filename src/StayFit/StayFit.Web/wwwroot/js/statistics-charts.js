document.addEventListener('DOMContentLoaded', function () {
    var canvas = document.getElementById('progressChart');
    if (!canvas) return;

    var ctx = canvas.getContext('2d');
    
    try {
        var labels = JSON.parse(canvas.dataset.labels || '[]');
        var caloriesData = JSON.parse(canvas.dataset.calories || '[]');
        var goalValue = parseFloat(canvas.dataset.goal || '0');
        var totalDays = parseInt(canvas.dataset.days || '0');
        
        var goalLine = Array(totalDays).fill(goalValue);

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Спожито калорій',
                        data: caloriesData,
                        backgroundColor: 'rgba(54, 162, 235, 0.6)',
                        borderColor: 'rgba(54, 162, 235, 1)',
                        borderWidth: 1
                    },
                    {
                        label: 'Ціль калорій',
                        data: goalLine,
                        type: 'line',
                        backgroundColor: 'rgba(255, 99, 132, 1)',
                        borderColor: 'rgba(255, 99, 132, 1)',
                        borderWidth: 2,
                        fill: false,
                        pointRadius: 0
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    } catch (e) {
        console.error('Failed to parse chart data', e);
    }
});
