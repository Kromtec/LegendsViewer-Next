<script setup lang="ts">
import { useWrittenContentStore } from '../stores/worldObjectStores';
import WorldObjectsPage from '../components/WorldObjectsPage.vue';
import { TableHeader } from '../types/legends';
import WrittenContentFilter from '../components/filter/WrittenContentFilter.vue';

const store = useWrittenContentStore();
const icon = "mdi-bookshelf";
const title = "Written Content";
const subtitle = "The preserved knowledge, stories, and records that document the past and shape the future";
const overviewSubtitle = "Browse and search all written content";
const overviewTitle = "Overview";
const wikiKeyWord = "Book";
const tableHeaders: TableHeader[] = [
    { title: 'Id', key: 'id', align: 'end' },
    { title: 'Name', key: 'html', align: 'start' },
    { title: 'Type', key: 'type', align: 'start' },
    { title: 'Author', key: 'subtype', align: 'start' },
    { title: 'Chronicles', key: 'eventCollectionCount', align: 'end' },
    { title: 'Events', key: 'eventCount', align: 'end' },
];
</script>

<template>
    <WorldObjectsPage :store="store" :icon="icon" :title="title" :subtitle="subtitle" :overviewTitle="overviewTitle"
        :overviewSubtitle="overviewSubtitle" :wikiKeyWord="wikiKeyWord" :tableHeaders="tableHeaders"
        :showFilters="true">
        <template v-slot:type-specific-filter="{ filters }">
            <WrittenContentFilter :title="title" :filters="filters"
                @update:filters="val => filters.splice(0, filters.length, ...val)" />
        </template>
    </WorldObjectsPage>
</template>