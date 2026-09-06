<script setup lang="ts">
import { useConstructionStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';

const store = useConstructionStore();
const icon = "mdi-sign-caution";
const title = "Constructions";
const subtitle = "Explore the vital infrastructure that supports trade, travel, and exploration across the world";
const overviewSubtitle = "Browse and search all world constructions";
const overviewTitle = "Overview";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Length', key: 'squareTiles', align: 'end' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :tableHeaders="tableHeaders">
    </WorldObjectsPage>
</template>