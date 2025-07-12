// Fuel Efficiency Frontend Integration Example
// This shows how to integrate the fuel efficiency API with your frontend

class FuelEfficiencyManager {
    constructor(baseUrl, vehicleId) {
        this.baseUrl = baseUrl;
        this.vehicleId = vehicleId;
    }

    // Add new fuel record
    async addFuel(fuelAmount, date, notes = '') {
        try {
            const response = await fetch(`${this.baseUrl}/api/FuelEfficiency`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    vehicleId: this.vehicleId,
                    fuelAmount: parseFloat(fuelAmount),
                    date: date,
                    notes: notes
                })
            });

            if (!response.ok) {
                const error = await response.text();
                throw new Error(error);
            }

            const result = await response.json();
            console.log('Fuel added successfully:', result);
            return result;
        } catch (error) {
            console.error('Error adding fuel:', error);
            throw error;
        }
    }

    // Get chart data for the current year
    async getChartData(year = new Date().getFullYear()) {
        try {
            const response = await fetch(`${this.baseUrl}/api/FuelEfficiency/vehicle/${this.vehicleId}/chart/${year}`);
            
            if (!response.ok) {
                throw new Error('Failed to fetch chart data');
            }

            const data = await response.json();
            
            // Transform data for chart libraries (like Chart.js)
            return {
                labels: data.map(month => month.monthName),
                datasets: [{
                    label: 'Monthly Fuel Usage (Liters)',
                    data: data.map(month => month.totalFuelAmount),
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1
                }]
            };
        } catch (error) {
            console.error('Error fetching chart data:', error);
            throw error;
        }
    }

    // Get fuel summary with statistics
    async getFuelSummary(year = new Date().getFullYear()) {
        try {
            const response = await fetch(`${this.baseUrl}/api/FuelEfficiency/vehicle/${this.vehicleId}/summary?year=${year}`);
            
            if (!response.ok) {
                throw new Error('Failed to fetch fuel summary');
            }

            return await response.json();
        } catch (error) {
            console.error('Error fetching fuel summary:', error);
            throw error;
        }
    }

    // Get all fuel records for a vehicle
    async getAllFuelRecords() {
        try {
            const response = await fetch(`${this.baseUrl}/api/FuelEfficiency/vehicle/${this.vehicleId}`);
            
            if (!response.ok) {
                throw new Error('Failed to fetch fuel records');
            }

            return await response.json();
        } catch (error) {
            console.error('Error fetching fuel records:', error);
            throw error;
        }
    }

    // Delete a fuel record
    async deleteFuelRecord(fuelRecordId) {
        try {
            const response = await fetch(`${this.baseUrl}/api/FuelEfficiency/${fuelRecordId}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                throw new Error('Failed to delete fuel record');
            }

            console.log('Fuel record deleted successfully');
            return true;
        } catch (error) {
            console.error('Error deleting fuel record:', error);
            throw error;
        }
    }
}

// Example usage
const fuelManager = new FuelEfficiencyManager('https://localhost:5001', 1);

// Function to handle fuel addition form
async function handleAddFuel() {
    const fuelAmount = document.getElementById('fuelAmount').value;
    const date = document.getElementById('fuelDate').value;
    const notes = document.getElementById('fuelNotes').value;

    try {
        await fuelManager.addFuel(fuelAmount, date, notes);
        
        // Clear form
        document.getElementById('fuelAmount').value = '';
        document.getElementById('fuelDate').value = '';
        document.getElementById('fuelNotes').value = '';
        
        // Refresh chart
        await refreshChart();
        
        // Show success message
        showMessage('Fuel record added successfully!', 'success');
    } catch (error) {
        showMessage('Error adding fuel record: ' + error.message, 'error');
    }
}

// Function to create/update the chart
async function refreshChart() {
    try {
        const chartData = await fuelManager.getChartData();
        
        // Assuming you're using Chart.js
        if (window.fuelChart) {
            window.fuelChart.destroy();
        }
        
        const ctx = document.getElementById('fuelChart').getContext('2d');
        window.fuelChart = new Chart(ctx, {
            type: 'bar',
            data: chartData,
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Fuel Amount (Liters)'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Month'
                        }
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'Monthly Fuel Usage'
                    }
                }
            }
        });
    } catch (error) {
        console.error('Error refreshing chart:', error);
        showMessage('Error loading chart data', 'error');
    }
}

// Function to display fuel summary
async function displayFuelSummary() {
    try {
        const summary = await fuelManager.getFuelSummary();
        
        document.getElementById('totalFuelYear').textContent = summary.totalFuelThisYear + ' L';
        document.getElementById('averageMonthly').textContent = summary.averageMonthlyFuel.toFixed(1) + ' L';
        document.getElementById('vehicleReg').textContent = summary.vehicleRegistrationNumber;
        
    } catch (error) {
        console.error('Error displaying summary:', error);
    }
}

// Utility function to show messages
function showMessage(message, type) {
    const messageDiv = document.getElementById('message');
    messageDiv.textContent = message;
    messageDiv.className = `message ${type}`;
    messageDiv.style.display = 'block';
    
    setTimeout(() => {
        messageDiv.style.display = 'none';
    }, 3000);
}

// Initialize the page
document.addEventListener('DOMContentLoaded', async function() {
    // Set today's date as default
    document.getElementById('fuelDate').value = new Date().toISOString().split('T')[0];
    
    // Load initial data
    await refreshChart();
    await displayFuelSummary();
});

// HTML structure example:
/*
<!DOCTYPE html>
<html>
<head>
    <title>Fuel Efficiency Tracker</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .message { padding: 10px; margin: 10px 0; border-radius: 4px; }
        .message.success { background-color: #d4edda; color: #155724; }
        .message.error { background-color: #f8d7da; color: #721c24; }
        .form-group { margin: 10px 0; }
        .form-group label { display: block; margin-bottom: 5px; }
        .form-group input, .form-group textarea { width: 100%; padding: 8px; }
        .chart-container { width: 100%; height: 400px; margin: 20px 0; }
        .summary { display: flex; gap: 20px; margin: 20px 0; }
        .summary-item { background: #f8f9fa; padding: 15px; border-radius: 5px; }
    </style>
</head>
<body>
    <h1>Fuel Efficiency Tracker</h1>
    
    <div id="message" style="display: none;"></div>
    
    <div class="summary">
        <div class="summary-item">
            <h3>Vehicle: <span id="vehicleReg">-</span></h3>
        </div>
        <div class="summary-item">
            <h3>Total This Year</h3>
            <p id="totalFuelYear">0 L</p>
        </div>
        <div class="summary-item">
            <h3>Average Monthly</h3>
            <p id="averageMonthly">0 L</p>
        </div>
    </div>
    
    <form onsubmit="event.preventDefault(); handleAddFuel();">
        <div class="form-group">
            <label for="fuelAmount">Fuel Amount (Liters):</label>
            <input type="number" id="fuelAmount" step="0.1" min="0.1" required>
        </div>
        
        <div class="form-group">
            <label for="fuelDate">Date:</label>
            <input type="date" id="fuelDate" required>
        </div>
        
        <div class="form-group">
            <label for="fuelNotes">Notes (Optional):</label>
            <textarea id="fuelNotes" rows="3"></textarea>
        </div>
        
        <button type="submit">Add Fuel</button>
    </form>
    
    <div class="chart-container">
        <canvas id="fuelChart"></canvas>
    </div>
    
    <script src="fuel-efficiency-frontend.js"></script>
</body>
</html>
*/
