//Globals
let chart;
let BarGraphiclist = [];
let doughnutGraphiclist = [];

function startApp() {

    generateGraphic();

}

async function generateGraphic() {

    //De primera instancia carga siempre de primero el grafico de barras
    barGraph = await barGraphic();

    const barGraphicOption = document.getElementById("barGraphicOption");
    barGraphicOption.addEventListener('click', async () => {
        const existingCanvas = document.getElementById('doughnutGraphic');

        if (existingCanvas) {
            existingCanvas.remove();
        }
        await barGraphic();
    })

    const doughnutGraphicOption = document.getElementById("doughnutGraphicOption");
    doughnutGraphicOption.addEventListener('click', async () => {
        const existingCanvas = document.getElementById('barGraphic');

        if (existingCanvas) {
            existingCanvas.remove();
        }
        await doughnutGraphic();
    })
}




async function barGraphic() {
    try {
        const canvasBarContent = document.getElementById('canvasBarContent');
        const existingCanvas = document.getElementById('barGraphic');

        if (existingCanvas) {
            existingCanvas.remove(); 
        }


        const newCanvas = document.createElement('canvas');
        newCanvas.id = 'barGraphic';
        canvasBarContent.appendChild(newCanvas); 

        const barGraphic = document.getElementById('barGraphic');


        canvasBarContent.style.display = "flex";
        canvasBarContent.style.justifyContent = "center";
        canvasBarContent.style.alignItems = "center";
        barGraphic.style.width = "1000px";
        barGraphic.style.height = "800px";

        const response = await fetch("/Metrics/GraphicData");
        const data = await response.json();
        console.log(data)
        let { graphicData } = data;

        let labels = graphicData.map(item => item.name);
        console.log(labels)
        let values = graphicData.map(item => item.hoursBooked);
        console.log(values)

        const average = (labels, values) => {
            const totals = {};

            labels.forEach((label, index) => {
                if (totals[label]) {
                    totals[label] += values[index] / values.length;
                } else {
                    totals[label] = values[index];
                }
            });

                return totals;
            
        }

        console.log(average(labels, values));

        const convertToChart = (average) => {
            return {
                labels: Object.keys(average),
                values: Object.values(average)
            }
        }

        const chartData = convertToChart(average(labels, values));
        console.log(chartData);


        const ctx = barGraphic.getContext('2d');


        if (chart) {
            chart.destroy();
        }


        chart = new Chart(ctx, {
            type: 'bar', 
            data: {
                labels: chartData.labels,
                datasets: [{
                    label: 'Promedio de Horas Reservadas Por Sala',
                    data: chartData.values,
                    backgroundColor: ['#FF5733', '#33FF57'],
                    borderColor: 'black',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });

    } catch (error) {
        console.log("Error al obtener los datos: ", error);
    }
}

async function doughnutGraphic() {
    try {
        const canvasDoughnutContent = document.getElementById('canvasDoughnutContent');

        // Verificar si el gráfico ya existe, y eliminarlo si es necesario
        const existingCanvas = document.getElementById('doughnutGraphic');
        if (existingCanvas) {
            existingCanvas.remove(); // Eliminar el canvas existente
        }

        // Crear un nuevo canvas para el gráfico de pie
        const newCanvas = document.createElement('canvas');
        newCanvas.id = 'doughnutGraphic';
        canvasDoughnutContent.appendChild(newCanvas); // Agregar el canvas al contenedor

        const doughnutGraphic = document.getElementById('doughnutGraphic');

        // Configurar el contenedor y el tamaño del canvas
        canvasDoughnutContent.style.display = "flex";
        canvasDoughnutContent.style.justifyContent = "center";
        canvasDoughnutContent.style.alignItems = "center";
        doughnutGraphic.style.width = "1000px";
        doughnutGraphic.style.height = "800px";

        const response = await fetch("/Metrics/GraphicData");
        const data = await response.json();
        let { graphicData } = data;

        const labels = graphicData.map(item => item.name);
        const hours = graphicData.map(item => {
            return Array.isArray(item.hoursBooked) ? item.hoursBooked : [item.hoursBooked];
        });

        console.log(labels)
        console.log(hours)

        const topHoursPerRoom = (labels, hours) => {

            let groupedData = {};

            labels.forEach((label, index) => {
                const roomsHours = hours[index]

                if (!groupedData[label]) {
                    groupedData[label] = {};
                }

                // Contamos las horas por cada sala
                roomsHours.forEach(hour => {
                    groupedData[label][hour] = (groupedData[label][hour] || 0) + 1;
                });

          

            });


            return groupedData;
        };

        const result = topHoursPerRoom(labels, hours);

        console.log(result);


        const convertToChart = (object) => {
            const labels = [];
            const data = [];

            Object.keys(object).forEach(label => {
                labels.push(label);

                const hourCounts = Object.values(object[label]);
                data.push(hourCounts);
            });

            return {labels, data}
        };

        const chartData = convertToChart(result);

        console.log(chartData);


        const ctx = doughnutGraphic.getContext('2d');

        // Si ya existe un gráfico, destruirlo
        if (chart) {
            chart.destroy();
        }

        //Crear el gráfico de pie
        chart = new Chart(ctx, {
            type: 'doughnut', // Tipo de gráfico
            data: {
                labels: chartData.labels,
                datasets: [{
                    label: `Reservaciones de hora en ${chartData.labels.map(label => label.split(" ").join("- "))}`,
                    data: chartData.data,
                    backgroundColor: ['#FF5733', '#33FF57'],
                    borderColor: 'black',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });

    } catch (error) {
        console.log("Error al obtener los datos: ", error);
    }
}



window.onload = startApp;