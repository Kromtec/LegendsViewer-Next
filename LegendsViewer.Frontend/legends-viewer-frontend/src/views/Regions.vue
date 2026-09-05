<script setup lang="ts">
import { useRegionStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';

import RegionFilter from '../components/filter/RegionFilter.vue';

const store = useRegionStore();
const icon = "mdi-map-legend";
const title = "Regions";
const subtitle = "Uncover the distinct regions of the world, where every area tells a story through its terrain and inhabitants";
const overviewSubtitle = "Browse and search all world regions";
const overviewTitle = "Overview";
const wikiKeyWord = "Region";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Biome', key: 'type', align: 'start' },
    { title: 'Evilness', key: 'subtype', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :wikiKeyWord="wikiKeyWord" :tableHeaders="tableHeaders"
        :showFilters="true">
        <template v-slot:type-specific-filter="{ filters }">
            <RegionFilter :title="title" :filters="filters"
                @update:filters="val => filters.splice(0, filters.length, ...val)" />
        </template>
    </WorldObjectsPage>
</template>