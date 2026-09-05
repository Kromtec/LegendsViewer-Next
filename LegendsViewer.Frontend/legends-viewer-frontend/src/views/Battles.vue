<script setup lang="ts">
import { useBattleStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';
import BattleFilter from '../components/filter/BattleFilter.vue';

const store = useBattleStore();
const icon = "mdi-chess-bishop";
const title = "Battles";
const subtitle = "Pivotal clashes between armies that decided the fate of nations";
const overviewSubtitle = "Browse and search all battles";
const overviewTitle = "Overview";
const tableHeaders: TableHeader[] = [
    { title: 'Start', key: 'startDate', align: 'center' },
    { title: 'End', key: 'endDate', align: 'center' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Attacker vs. Defender', key: 'subtype', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :tableHeaders="tableHeaders" :showFilters="true">
        <template v-slot:type-specific-filter="{ filters }">
            <BattleFilter :title="title" :filters="filters"
                @update:filters="val => filters.splice(0, filters.length, ...val)" />
        </template>
    </WorldObjectsPage>
</template>