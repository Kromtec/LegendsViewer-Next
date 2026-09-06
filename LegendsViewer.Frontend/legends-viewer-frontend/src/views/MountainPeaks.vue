<script setup lang="ts">
import { useMountainPeakStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';

const store = useMountainPeakStore();
const icon = "mdi-summit";
const title = "Mountain Peaks and Volcanos";
const subtitle = "From majestic summits to explosive craters, explore nature's most formidable formations";
const overviewSubtitle = "Browse and search all mountain peaks and volcanos";
const overviewTitle = "Overview";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Height', key: 'heightMeter', align: 'end' },
    { title: 'World Rank', key: 'worldRank', align: 'end' },
    { title: 'Region', key: 'regionLink', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :tableHeaders="tableHeaders">
    </WorldObjectsPage>
</template>