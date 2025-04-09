<script setup lang="ts">
import { useWorldStore } from '../stores/worldStore';
import { useWorldMapStore } from '../stores/mapStore';
import DoughnutChart from '../components/DoughnutChart.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import CivilizationsCardList from '../components/CivilizationsCardList.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData, LoadItemsOptions, TableHeader } from '../types/legends';
import ExpandableCard from '../components/ExpandableCard.vue';
import LineChart from '../components/LineChart.vue';
import BarChart from '../components/BarChart.vue';

const store = useWorldStore()
const mapStore = useWorldMapStore()

store.loadWorld();
store.loadEventChartData()
store.loadEventTypeChartData()
mapStore.loadWorldMap('Default');

const lists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Heroic Ties', items: store.world?.playerRelatedObjects ?? [], icon: "mdi-compass-outline", subtitle: "Discover the adventurers, factions, and locations tied to your journey" },
]);


const loadEvents = async ({ page, itemsPerPage, sortBy }: LoadItemsOptions) => {
    await store.loadEvents(page, itemsPerPage, sortBy)
}

const loadEventCollections = async ({ page, itemsPerPage, sortBy }: LoadItemsOptions) => {
    await store.loadEventCollections(page, itemsPerPage, sortBy)
}

const eventTableHeaders: TableHeader[] = [
    { title: 'Date', key: 'date' },
    { title: 'Type', key: 'type' },
    { title: 'Event', key: 'html', sortable: false },
]

const eventCollectionTableHeaders: TableHeader[] = [
    { title: 'Start', key: 'startDate', align: 'center' },
    { title: 'End', key: 'endDate', align: 'center' },
    { title: 'Name', key: 'html', align: 'start', sortable: false },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Subtype', key: 'subtype', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
]

</script>

<template>
    <v-row>
        <v-col cols="12">
            <v-card variant="text">
                <v-row align="center" no-gutters>
                    <v-col class="large-icon" cols="auto">
                        <v-icon icon="mdi-earth-box" />
                    </v-col>
                    <v-col>
                        <v-card-title>{{ store.world?.name ?? '' }}</v-card-title>
                        <v-card-subtitle class="multiline-subtitle">
                            {{ store.world?.alternativeName ?? '' }}
                        </v-card-subtitle>
                    </v-col>
                </v-row>
            </v-card>
        </v-col>
    </v-row>
    <v-row>
        <v-col v-if="mapStore?.worldMapMid" cols="12" xl="4" lg="6" md="12">
            <!-- World Map -->
            <v-card title="World Overview Map"
                subtitle="Click to explore the interactive map and dive into the world's locations" height="400"
                variant="text" to="/map">
                <template v-slot:prepend>
                    <v-icon class="mr-2" icon="mdi-map-search-outline" size="32px"></v-icon>
                </template>
                <v-card-text>
                    <v-img width="320" height="320" class="position-relative ml-12 pixelated-image"
                        :src="mapStore.worldMapMid" :cover="false" />
                </v-card-text>
            </v-card>
        </v-col>
        <v-col
            v-if="store.world?.entityPopulationsByRace?.labels != null && store.world?.entityPopulationsByRace?.labels?.length > 0"
            cols="12" xl="4" lg="6" md="12">
            <v-card title="Population by Race"
                subtitle="A demographic breakdown of the population of the main civilizations" height="400"
                variant="text">
                <template v-slot:prepend>
                    <v-icon class="mr-2" icon="mdi-chart-donut" size="32px"></v-icon>
                </template>
                <v-card-text>
                    <DoughnutChart :chart-data="store.world?.entityPopulationsByRace" />
                </v-card-text>
            </v-card>
        </v-col>
        <v-col v-if="store.world?.areaByOverworldRegions?.datasets != null &&
            store.world?.areaByOverworldRegions?.datasets?.length > 0 &&
            store.world?.areaByOverworldRegions?.datasets[0].data?.some(value => value > 0)" cols="12" xl="4" lg="6"
            md="12">
            <v-card title="Area by Overworld Regions"
                subtitle="A comparative view of the land distribution across the world" height="400" variant="text">
                <template v-slot:prepend>
                    <v-icon class="mr-2" icon="mdi-earth" size="32px"></v-icon>
                </template>
                <v-card-text>
                    <DoughnutChart :chart-data="store.world?.areaByOverworldRegions" />
                </v-card-text>
            </v-card>
        </v-col>
    </v-row>
    <v-row>
        <v-col v-if="store.world?.mainCivilizations != null && store.world?.mainCivilizations.length > 0" cols="12"
            xl="6" lg="12">
            <CivilizationsCardList :icon="'mdi-account-multiple'" :title="'Active Civilizations'"
                :subtitle="'The thriving societies that continue to shape the world with their influence and power'"
                :list="store.world?.mainCivilizations ?? []" />
        </v-col>
        <v-col v-if="store.world?.mainCivilizationsLost != null && store.world?.mainCivilizationsLost.length > 0"
            cols="12" xl="6" lg="12">
            <CivilizationsCardList :icon="'mdi-account-multiple-remove-outline'" :title="'Lost Civilizations'"
                :subtitle="'The remnants of once-great societies that have faded into history'"
                :list="store.world?.mainCivilizationsLost ?? []" />
        </v-col>
    </v-row>
    <v-row>
        <v-col>
            <ExpandableCard title="Events" :subtitle="'An overview of events for ' + store.world?.name"
                icon="mdi-calendar-clock" :height="'auto'">
                <template #compact-content>
                    <div class="ml-12">
                        <LineChart v-if="store.objectEventChartData != null" :chart-data="store.objectEventChartData" />
                        <v-data-table-server v-model:items-per-page="store.objectEventsPerPage"
                            :headers="eventTableHeaders" :items="store.objectEvents"
                            :items-length="store.objectEventsTotalItems" :loading="store.isLoading" item-value="id"
                            :items-per-page-options="store.itemsPerPageOptions" @update:options="loadEvents">
                            <template v-slot:item.html="{ value }">
                                <span v-html="value"></span>
                            </template>
                        </v-data-table-server>
                    </div>
                </template>
                <template #expanded-content>
                    <BarChart v-if="store.objectEventTypeChartData != null"
                        :chart-data="store.objectEventTypeChartData" />
                </template>
            </ExpandableCard>
            <!-- <v-card title="Events" :subtitle="'A timeline of events for ' + store.world?.name" variant="text">
                <template v-slot:prepend>
                    <v-icon class="mr-2" icon="mdi-calendar-clock" size="32px"></v-icon>
                </template>
                <v-card-text class="ml-12">
                    <LineChart v-if="store.objectEventChartData != null" :chart-data="store.objectEventChartData" />
                    <v-data-table-server v-model:items-per-page="store.objectEventsPerPage" :headers="eventTableHeaders" :items="store.objectEvents"
                    :items-length="store.objectEventsTotalItems" :loading="store.isLoading" item-value="id" :items-per-page-options="store.itemsPerPageOptions"
                    @update:options="loadEvents">
                    <template v-slot:item.html="{ value }">
                        <span v-html="value"></span>
                    </template>
                </v-data-table-server>
                </v-card-text>
            </v-card> -->
        </v-col>
    </v-row>
    <v-row>
        <v-col>
            <v-card title="Chronicles" :subtitle="'A list of chronicles for ' + store.world?.name" variant="text">
                <template v-slot:prepend>
                    <v-icon class="mr-2" icon="mdi-calendar-clock" size="32px"></v-icon>
                </template>
                <v-card-text class="ml-12">
                    <v-data-table-server v-model:items-per-page="store.objectEventCollectionsPerPage"
                        :headers="eventCollectionTableHeaders" :items="store.objectEventCollections"
                        :items-length="store.objectEventCollectionsTotalItems" :loading="store.isLoading"
                        item-value="id" :items-per-page-options="store.itemsPerPageOptions"
                        @update:options="loadEventCollections">
                        <template v-slot:item.subtype="{ value }">
                            <span v-html="value"></span>
                        </template>
                        <template v-slot:item.html="{ value }">
                            <span v-html="value"></span>
                        </template>
                    </v-data-table-server>
                </v-card-text>
            </v-card>
        </v-col>
    </v-row>
    <v-row>
        <template v-for="(list, i) in lists" :key="i">
            <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                <LegendsCardList :list="list" />
            </v-col>
        </template>
    </v-row>
</template>

<style scoped></style>