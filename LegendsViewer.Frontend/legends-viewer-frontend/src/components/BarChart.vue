<template>
    <Bar :data="extendedChartData" :options="chartOptions" />
</template>

<script>
import { defineComponent, computed } from 'vue';
import { Bar } from 'vue-chartjs'
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    Title,
    Tooltip,
    Legend,
    Filler,
    BarElement
} from 'chart.js'
import { useEventFilterStore } from '../stores/eventFilterStore';

ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    BarElement,
    Title,
    Tooltip,
    Legend,
    Filler
)

export default defineComponent({
    name: 'BarChart',
    components: { Bar },
    props: {
        chartData: {
            type: Object,
            required: true
        },
        chartOptions: {
            type: Object,
            default: () => {
                return {
                    indexAxis: 'y',
                    elements: {
                        bar: {
                            borderWidth: 1,
                        }
                    },
                    responsive: true,
                    maintainAspectRatio: false,
                }
            }
        },
    },
    setup(props) {
        const filterStore = useEventFilterStore();

        // Using a computed property to extend chartData with default values
        const extendedChartData = computed(() => {
            if (!props.chartData)
                return {
                    labels: ['VueJs', 'EmberJs', 'ReactJs', 'AngularJs'],
                    datasets: [{
                        borderColor: '#41B883',
                        data: [40, 20, 80, 10],
                        fill: false,
                        borderColor: 'rgb(75, 192, 192)',
                        tension: 0.1
                    }]
                }

            const bgColors = (props.chartData.labels || []).map((label) => {
                const match = label.match(/\((.*?)\)/);
                const typeKey = match ? match[1] : label;
                return filterStore.excludedEventTypes.includes(typeKey)
                    ? 'rgba(120, 120, 120, 0.2)'
                    : 'rgba(75, 192, 192, 0.6)';
            });

            const borderColors = (props.chartData.labels || []).map((label) => {
                const match = label.match(/\((.*?)\)/);
                const typeKey = match ? match[1] : label;
                return filterStore.excludedEventTypes.includes(typeKey)
                    ? 'rgba(120, 120, 120, 0.3)'
                    : 'rgb(75, 192, 192)';
            });

            return {
                labels: props.chartData.labels,
                datasets: props.chartData.datasets.map(dataset => ({
                    label: dataset.label,
                    data: dataset.data,
                    fill: true,
                    borderColor: borderColors,
                    backgroundColor: bgColors,
                })),
            };
        });

        return {
            extendedChartData,
        };
    }
})


ChartJS.defaults.color = '#ffffff';  // Default text color
ChartJS.defaults.scale.ticks.color = '#b0bec5';  // Axis tick colors
ChartJS.defaults.scale.grid.color = '#37474f';  // Grid line colors
</script>