<script setup lang="ts">
import { useStructureStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';

const store = useStructureStore();
const icon = "mdi-home-silo";
const title = "Structures";
const subtitle = "From grand guildhalls to mysterious catacombs, uncover the stories behind each structure";
const overviewSubtitle = "Browse and search all site structures";
const overviewTitle = "Overview";
const wikiKeyWord = "Structure";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :wikiKeyWord="wikiKeyWord" :tableHeaders="tableHeaders">
    </WorldObjectsPage>
</template>