<script setup lang="ts">
import { useUndergroundRegionStore } from '../stores/worldObjectStores';
import { useUndergroundRegionMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useUndergroundRegionStore();
const mapStore = useUndergroundRegionMapStore();

const cardLists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Breaches & Discoveries', items: store.object?.breachLinks ?? [], icon: "mdi-pickaxe", subtitle: "Events where a civilization or fortress breached into this layer" },
    { title: 'Surface Regions Above', items: store.object?.regionLinks ?? [], icon: "mdi-map-legend", subtitle: "Surface regions located directly above this subterranean layer" },
    { title: 'Related Sites', items: store.object?.siteLinks ?? [], icon: "mdi-home-modern", subtitle: "Fortresses, caverns, and sites intersecting this underground region" },
    { title: 'Battles', items: store.object?.battleLinks ?? [], icon: "mdi-chess-bishop", subtitle: "Historic battles and conflicts fought in or near this underground region" },
]);

const getLayerIcon = (type?: string | null) => {
    const t = type?.toLowerCase() ?? '';
    if (t.includes('magma')) return 'mdi-fire';
    if (t.includes('underworld')) return 'mdi-skull-crossbones';
    return 'mdi-tunnel';
};

const getLayerColor = (type?: string | null) => {
    const t = type?.toLowerCase() ?? '';
    if (t.includes('magma')) return 'error';
    if (t.includes('underworld')) return 'deep-purple';
    return 'info';
};
</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'uregion'">
        <template v-slot:type-specific-before-table>
            <!-- Summary Info Card -->
            <v-col cols="12" xl="4" lg="6" md="12">
                <v-card max-height="400" variant="text">
                    <v-card-text>
                        <v-list lines="two" class="mt-10" style="background-color: rgb(var(--v-theme-background));" density="compact">
                            <v-list-item v-if="store.object?.type" title="Layer Type">
                                <v-list-item-subtitle>
                                    <v-chip
                                        :color="getLayerColor(store.object?.type)"
                                        variant="tonal"
                                        size="small"
                                        class="font-weight-bold mt-1"
                                    >
                                        <v-icon :icon="getLayerIcon(store.object?.type)" start></v-icon>
                                        {{ store.object.type }}
                                        <span v-if="store.object?.depth != null" class="ml-1">(Depth {{ store.object.depth }})</span>
                                    </v-chip>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.squareTiles != null && store.object.squareTiles > 0" title="Subterranean Area">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-vector-square" class="mr-1"></v-icon>
                                    {{ store.object?.squareTiles }} Tiles
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.breachLinks != null && store.object.breachLinks.length > 0" title="Breaches & Discoveries">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-pickaxe" class="mr-1"></v-icon>
                                    {{ store.object.breachLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.regionLinks != null && store.object.regionLinks.length > 0" title="Surface Regions Above">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-map-legend" class="mr-1"></v-icon>
                                    {{ store.object.regionLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.siteLinks != null && store.object.siteLinks.length > 0" title="Related Sites">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-home-modern" class="mr-1"></v-icon>
                                    {{ store.object.siteLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
            </v-col>

            <!-- Detail Lists -->
            <template v-for="(list, i) in cardLists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
        </template>
    </WorldObjectPage>
</template>

<style scoped></style>