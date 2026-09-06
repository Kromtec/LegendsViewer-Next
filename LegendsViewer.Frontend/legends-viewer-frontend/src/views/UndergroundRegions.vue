<script setup lang="ts">
import { useUndergroundRegionStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';

const store = useUndergroundRegionStore();
const icon = "mdi-tunnel";
const title = "Underground Regions";
const subtitle = "Discover the secrets of the underground, where darkness holds treasures and ancient civilizations thrive";
const overviewSubtitle = "Browse and search all underground regions";
const overviewTitle = "Overview";
const wikiKeyWord = "Region";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Depth', key: 'subtype', align: 'center' },
    { title: 'Size', key: 'squareTiles', align: 'end' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :wikiKeyWord="wikiKeyWord" :tableHeaders="tableHeaders">
    </WorldObjectsPage>
</template>