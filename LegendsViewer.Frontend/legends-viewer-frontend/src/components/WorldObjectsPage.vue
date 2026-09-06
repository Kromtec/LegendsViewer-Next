<template>
    <v-row>
        <v-col cols="12">
            <v-card variant="text">
                <v-row align="center" no-gutters>
                    <v-col class="large-icon" cols="auto">
                        <v-icon :icon="icon"></v-icon>
                    </v-col>
                    <v-col>
                        <v-card-title>{{ title }}</v-card-title>
                        <v-card-subtitle class="multiline-subtitle">
                            {{ subtitle }}
                        </v-card-subtitle>
                    </v-col>
                    <v-col v-if="wikiKeyWord" cols="auto">
                        <v-btn append-icon="mdi-search-web"
                            :href="`https://dwarffortresswiki.org/index.php/${encodeURIComponent(wikiKeyWord)}`"
                            target="_blank">
                            Search DF Wiki

                            <template v-slot:append>
                                <v-icon color="primary"></v-icon>
                            </template>
                        </v-btn>
                    </v-col>
                </v-row>
            </v-card>
        </v-col>
    </v-row>
    <v-row>
        <slot name="type-specific-before-table"></slot>
    </v-row>
    <v-row>
        <v-col>
            <v-card :title="overviewTitle" :subtitle="overviewSubtitle" variant="text">
                <template v-slot:prepend>
                    <v-icon class="mr-2" icon="mdi-card-search-outline" size="32px"></v-icon>
                </template>
                <template v-slot:text>
                    <v-row>
                        <v-col>
                            <v-text-field v-model="searchString" label="Search" prepend-inner-icon="mdi-magnify"
                                variant="outlined" hide-details single-line></v-text-field>
                        </v-col>
                        <v-col v-if="showFilters">
                            <v-menu v-model="showFilterPopup" :close-on-content-click="false" location="left top">
                                <template v-slot:activator="{ props }">
                                    <v-btn class="filter-button" icon="mdi-filter-menu-outline" v-bind="props">
                                    </v-btn>
                                </template>

                                <v-card min-width="480" width="480">
                                    <v-list>
                                        <v-list-item :subtitle="title" title="Filter Settings">
                                            <template v-slot:append>
                                                <v-btn :class="'text-red'" icon="mdi-filter-off-outline" variant="text"
                                                    @click="clearFilters"></v-btn>
                                            </template>
                                        </v-list-item>
                                    </v-list>
                                    <slot name="type-specific-filter" :filters="draftFilters"
                                        @update:filters="draftFilters = $event">
                                    </slot>
                                    <v-card-actions>
                                        <v-spacer></v-spacer>
                                        <v-btn variant="text" @click="showFilterPopup = false">Cancel</v-btn>
                                        <v-btn color="primary" variant="text" @click="applyFilters">Apply</v-btn>
                                    </v-card-actions>
                                </v-card>
                            </v-menu>
                            <div class="filter-chip-container">
                                <v-chip-group column>
                                    <v-chip v-for="(filter, key) in filters" :key="key"
                                        :text="getChipTextByFilter(filter)" :value="filter" variant="outlined"></v-chip>
                                </v-chip-group>
                            </div>
                        </v-col>
                    </v-row>
                </template>
                <v-card-text>
                    <v-data-table-server v-model:items-per-page="store.objectsPerPage" :headers="tableHeaders"
                        :items="store.objects" :items-length="store.objectsTotalFilteredItems" :search="searchString"
                        :loading="store.isLoading" item-value="id" :items-per-page-options="store.itemsPerPageOptions"
                        @update:options="loadWorldObjects">
                        <template v-slot:item.html="{ value }">
                            <span v-html="value"></span>
                        </template>
                        <template v-slot:item.subtype="{ value }">
                            <span v-html="value"></span>
                        </template>
                    </v-data-table-server>
                </v-card-text>
                <template v-slot:append>
                    <v-chip class="ma-2" color="cyan" label>
                        <v-icon :icon="icon" start></v-icon>
                        {{ title }}: {{ store.objectsTotalItems }}
                    </v-chip>
                </template>
            </v-card>
        </v-col>
    </v-row>
    <v-row>
        <slot name="type-specific-after-table"></slot>
    </v-row>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { LoadItemsOptionsWithSearch, TableHeader } from '../types/legends';
import type { FilterOperator, FilterRuleDto } from '../stores/worldObjectStores'; // Adjust the path as needed

const props = defineProps({
    store: {
        type: Object,
        required: true,
    },
    icon: {
        type: String,
        required: true,
    },
    title: {
        type: String,
        required: true,
    },
    subtitle: {
        type: String,
        required: true,
    },
    overviewTitle: {
        type: String,
        required: true,
    },
    overviewSubtitle: {
        type: String,
        required: true,
    },
    wikiKeyWord: {
        type: String,
        required: false,
    },
    tableHeaders: {
        type: Array as () => TableHeader[],
        required: true,
    },
    showFilters: {
        type: Boolean,
        required: false,
        default: false
    }
});

const searchString = ref("")
const filters = ref<FilterRuleDto[]>([]);
const draftFilters = ref<FilterRuleDto[]>([]);
const showFilterPopup = ref(false)

const applyFilters = () => {
    filters.value = [...draftFilters.value];
    showFilterPopup.value = false;
};

const clearFilters = () => {
    filters.value = [];
    draftFilters.value = [];
    showFilterPopup.value = false;
};

// const removeFilter = (filterToRemove: FilterRuleDto) => {
//     const index = filters.value.findIndex(f =>
//         f.propertyName === filterToRemove.propertyName &&
//         f.operator === filterToRemove.operator &&
//         f.value === filterToRemove.value
//     );
//     if (index !== -1) {
//         filters.value.splice(index, 1);
//         draftFilters.value = filters.value.map(f => ({ ...f })); // also keep draft in sync
//     }
// };

const getChipTextByFilter = (filter: FilterRuleDto) => {
    if (filter.propertyName?.startsWith("Is")) {
        if (filter.operator == 'Equals') {
            return filter.value === "true" ? filter.propertyName : "NOT " + filter.propertyName
        }
        if (filter.operator == 'NotEquals') {
            return filter.value === "false" ? filter.propertyName : "NOT " + filter.propertyName
        }
    }
    else {
        return filter.propertyName + " " + getOperatorString(filter.operator) + " " + filter.value;
    }
};

const getOperatorString = (operator: FilterOperator | undefined) => {
    switch (operator) {
        case 'Equals':
            return "==";
        case 'NotEquals':
            return "!=";
        case 'GreaterThan':
            return ">";
        case 'LessThan':
            return "<";
        case 'Contains':
            return "~";
        default:
            return "|";
    }
}

const loadWorldObjects = async ({ page, itemsPerPage, sortBy, search }: LoadItemsOptionsWithSearch) => {
    await props.store.loadOverview(page, itemsPerPage, sortBy, search, filters.value)
}

// Load initial data when component mounts
loadWorldObjects({ page: 1, itemsPerPage: props.store.objectEventsPerPage, sortBy: [], search: searchString.value })

watch(searchString, () => {
    loadWorldObjects({ page: 1, itemsPerPage: props.store.objectEventsPerPage, sortBy: [], search: searchString.value })
});
watch(filters, () => {
    loadWorldObjects({ page: 1, itemsPerPage: props.store.objectEventsPerPage, sortBy: [], search: searchString.value });
}, { deep: true });
</script>

<style scoped>
.multiline-subtitle {
    white-space: normal;
}

.filter-button {
    position: absolute;
    margin: 4px;
}

.filter-chip-container {
    display: inline-block;
    border: 1px solid gray;
    border-radius: 4px;
    height: 56px;
    width: 100%;
    padding: 4px 64px;
    vertical-align: middle;
}
</style>
