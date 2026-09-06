<script setup lang="ts">
import { useRiverStore } from '../stores/worldObjectStores';
import { useRiverMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useRiverStore();
const mapStore = useRiverMapStore();

const lists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Crossed Regions', items: store.object?.regionLinks ?? [], icon: "mdi-map-legend", subtitle: "Regions through which this river flows" },
    { title: 'Sites along River', items: store.object?.siteLinks ?? [], icon: "mdi-home-switch-outline", subtitle: "Settlements and fortresses located on or adjacent to this river" },
    { title: 'Constructions & Bridges', items: store.object?.constructionLinks ?? [], icon: "mdi-bridge", subtitle: "Bridges, roads, and structures built over or along this river" },
]);

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'river'">
        <template v-slot:type-specific-before-table>
            <v-col cols="12" xl="4" lg="6" md="12">
                <!-- Additional Infos -->
                <v-card max-height="400" variant="text">
                    <v-card-text>
                        <v-list lines="two" class="mt-10" style="background-color: rgb(var(--v-theme-background));" density="compact">
                            <v-list-item v-if="store.object?.length != null" title="River Length">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-map-marker-path" class="mr-1"></v-icon>
                                    {{ store.object?.length }} Tiles
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.regionLinks != null && store.object?.regionLinks.length > 0" title="Crossed Regions">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-map-legend" class="mr-1"></v-icon>
                                    {{ store.object?.regionLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.siteLinks != null && store.object?.siteLinks.length > 0" title="Sites along River">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-home-group" class="mr-1"></v-icon>
                                    {{ store.object?.siteLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.constructionLinks != null && store.object?.constructionLinks.length > 0" title="Constructions & Bridges">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-bridge" class="mr-1"></v-icon>
                                    {{ store.object?.constructionLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
            </v-col>
            <template v-for="(list, i) in lists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
        </template>
    </WorldObjectPage>
</template>

<style scoped></style>