<script setup lang="ts">
import { useArtifactStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';
import ArtifactFilter from '../components/filter/ArtifactFilter.vue';

const store = useArtifactStore();
const icon = "mdi-diamond-stone";
const title = "Artifacts";
const subtitle = "The treasured relics of history, each holding tales of power, craftsmanship, and legend";
const overviewSubtitle = "Browse and search all artifacts";
const overviewTitle = "Overview";
const wikiKeyWord = "artifact";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Subtype', key: 'subtype', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :wikiKeyWord="wikiKeyWord" :tableHeaders="tableHeaders"
        :showFilters="true">
        <template v-slot:type-specific-filter="{ filters }">
            <ArtifactFilter :title="title" :filters="filters"
                @update:filters="val => filters.splice(0, filters.length, ...val)" />
        </template>
    </WorldObjectsPage>
</template>