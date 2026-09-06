<script setup lang="ts">
import { useRegionStore } from '../stores/worldObjectStores';
import { useRegionMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useRegionStore();
const mapStore = useRegionMapStore();

const cardLists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Related Sites', items: store.object?.siteLinks ?? [], icon: "mdi-home-switch-outline", subtitle: "Settlements, fortresses, and sites located in this region" },
    { title: 'Rivers', items: store.object?.riverLinks ?? [], icon: "mdi-waves", subtitle: "Rivers flowing through this region" },
    { title: 'Mountain Peaks & Volcanoes', items: store.object?.mountainPeakLinks ?? [], icon: "mdi-summit", subtitle: "Mountain summits and volcanoes in this region" },
    { title: 'Constructions & Bridges', items: store.object?.constructionLinks ?? [], icon: "mdi-bridge", subtitle: "Bridges, roads, and structures built in this region" },
    { title: 'Battles & Conflicts', items: store.object?.battleLinks ?? [], icon: "mdi-chess-bishop", subtitle: "Historic battles and sieges fought in this region" },
    { title: 'Notable Deaths', items: store.object?.notableDeathLinks ?? [], icon: "mdi-grave-stone", subtitle: "Historical figures who met their end in this region" },
]);

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'region'">
        <template v-slot:type-specific-before-table>
            <!-- Summary Info Card -->
            <v-col cols="12" xl="4" lg="6" md="12">
                <v-card max-height="400" variant="text">
                    <v-card-text>
                        <v-list lines="two" class="mt-10" style="background-color: rgb(var(--v-theme-background));" density="compact">
                            <v-list-item v-if="store.object?.squareTiles != null && store.object.squareTiles > 0" title="Region Size">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-vector-square" class="mr-1"></v-icon>
                                    {{ store.object?.squareTiles }} Tiles
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.evilness != null && store.object.evilness !== 'Neutral'" title="Alignment / Evilness">
                                <v-list-item-subtitle>
                                    <v-chip
                                        :color="store.object.evilness === 'Evil' ? 'error' : store.object.evilness === 'Good' ? 'success' : 'info'"
                                        variant="tonal"
                                        size="small"
                                        class="font-weight-bold mt-1"
                                    >
                                        <v-icon :icon="store.object.evilness === 'Evil' ? 'mdi-skull' : store.object.evilness === 'Good' ? 'mdi-creation' : 'mdi-shield-outline'" start></v-icon>
                                        {{ store.object.evilness }} Region
                                    </v-chip>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.forceLink" title="Worshipped Force">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.forceLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.siteLinks != null && store.object.siteLinks.length > 0" title="Sites">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-home-group" class="mr-1"></v-icon>
                                    {{ store.object.siteLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.constructionLinks != null && store.object.constructionLinks.length > 0" title="Constructions & Bridges">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-bridge" class="mr-1"></v-icon>
                                    {{ store.object.constructionLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.battleLinks != null && store.object.battleLinks.length > 0" title="Battles">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-chess-bishop" class="mr-1"></v-icon>
                                    {{ store.object.battleLinks.length }}
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