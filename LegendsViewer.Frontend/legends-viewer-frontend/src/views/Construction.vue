<script setup lang="ts">
import { useConstructionStore } from '../stores/worldObjectStores';
import { useConstructionMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useConstructionStore();
const mapStore = useConstructionMapStore();

const isRoad = computed(() => {
    const type = store.object?.type?.toLowerCase() ?? '';
    const masterType = store.object?.masterConstructionType?.toLowerCase() ?? '';
    return type.includes('road') || masterType.includes('road');
});

const isTunnel = computed(() => {
    const type = store.object?.type?.toLowerCase() ?? '';
    const masterType = store.object?.masterConstructionType?.toLowerCase() ?? '';
    return type.includes('tunnel') || masterType.includes('tunnel');
});

const isBridge = computed(() => {
    const type = store.object?.type?.toLowerCase() ?? '';
    return type.includes('bridge');
});

const showSurfaceRegions = computed(() => {
    if (isTunnel.value) return false;
    return (store.object?.regionLinks?.length ?? 0) > 0;
});

const showUndergroundRegions = computed(() => {
    if (isRoad.value) return false;
    return (store.object?.undergroundRegionLinks?.length ?? 0) > 0;
});

const cardLists: ComputedRef<LegendLinkListData[]> = computed(() => {
    const lists: LegendLinkListData[] = [];
    
    // For bridges, single region and river links are displayed directly inside the summary info card
    if (!isBridge.value) {
        if (showSurfaceRegions.value) {
            lists.push({ title: 'Surface Regions', items: store.object?.regionLinks ?? [], icon: "mdi-map-legend", subtitle: "Surface regions traversed by this construction" });
        }
        if (showUndergroundRegions.value) {
            lists.push({ title: 'Underground Regions', items: store.object?.undergroundRegionLinks ?? [], icon: "mdi-tunnel", subtitle: "Caverns and subterranean layers traversed by this construction" });
        }
        if ((store.object?.riverLinks?.length ?? 0) > 0) {
            lists.push({ title: 'Crossed Rivers', items: store.object?.riverLinks ?? [], icon: "mdi-waves", subtitle: "Rivers crossed by or flowing beneath this construction" });
        }
    }

    if ((store.object?.sectionLinks?.length ?? 0) > 0) {
        lists.push({ title: 'Sub-sections', items: store.object?.sectionLinks ?? [], icon: "mdi-road-variant", subtitle: "Individual construction sections belonging to this route" });
    }

    return lists;
});

const getConstructionIcon = (type?: string | null) => {
    const t = type?.toLowerCase() ?? '';
    if (t.includes('bridge')) return 'mdi-bridge';
    if (t.includes('tunnel')) return 'mdi-tunnel-outline';
    return 'mdi-road';
};

const getConstructionColor = (type?: string | null) => {
    const t = type?.toLowerCase() ?? '';
    if (t.includes('bridge')) return 'info';
    if (t.includes('tunnel')) return 'purple';
    return 'warning';
};
</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'construction'">
        <template v-slot:type-specific-before-table>
            <!-- Summary Info Card -->
            <v-col cols="12" xl="4" lg="6" md="12">
                <v-card variant="text">
                    <v-card-text>
                        <v-list lines="two" class="mt-10" style="background-color: rgb(var(--v-theme-background));" density="compact">
                            <v-list-item v-if="store.object?.type" title="Construction Type">
                                <v-list-item-subtitle>
                                    <v-chip
                                        :color="getConstructionColor(store.object?.type)"
                                        variant="tonal"
                                        size="small"
                                        class="font-weight-bold mt-1"
                                    >
                                        <v-icon :icon="getConstructionIcon(store.object?.type)" start></v-icon>
                                        {{ store.object.type }}
                                    </v-chip>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="!isBridge && store.object?.squareTiles != null && store.object.squareTiles > 0" title="Length / Distance">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-ruler" class="mr-1"></v-icon>
                                    {{ store.object?.squareTiles }} Tiles
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.site1ToLink" title="Connected Site (Start)">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-home-city-outline" class="mr-1"></v-icon>
                                    <span v-html="store.object?.site1ToLink"></span>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.site2ToLink" title="Connected Site (End)">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-home-city-outline" class="mr-1"></v-icon>
                                    <span v-html="store.object?.site2ToLink"></span>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.masterConstructionToLink" title="Master Route">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-routes" class="mr-1"></v-icon>
                                    <span v-html="store.object?.masterConstructionToLink"></span>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <!-- Non-Bridge Summary Counts -->
                            <v-list-item v-if="!isBridge && showSurfaceRegions" title="Surface Regions">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-map-legend" class="mr-1"></v-icon>
                                    {{ store.object?.regionLinks?.length }}
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="!isBridge && showUndergroundRegions" title="Underground Regions">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-tunnel" class="mr-1"></v-icon>
                                    {{ store.object?.undergroundRegionLinks?.length }}
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="!isBridge && store.object?.riverLinks != null && store.object.riverLinks.length > 0" title="Crossed Rivers">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-waves" class="mr-1"></v-icon>
                                    {{ store.object.riverLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>

                            <!-- Bridge Direct Region and River Links -->
                            <v-list-item v-if="isBridge && showSurfaceRegions && store.object?.regionLinks?.length" title="Surface Region">
                                <v-list-item-subtitle class="mt-1">
                                    <span v-html="store.object.regionLinks.join(', ')"></span>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="isBridge && showUndergroundRegions && store.object?.undergroundRegionLinks?.length" title="Underground Region">
                                <v-list-item-subtitle class="mt-1">
                                    <span v-html="store.object.undergroundRegionLinks.join(', ')"></span>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="isBridge && store.object?.riverLinks != null && store.object.riverLinks.length > 0" title="Crossed River">
                                <v-list-item-subtitle class="mt-1">
                                    <span v-html="store.object.riverLinks.join(', ')"></span>
                                </v-list-item-subtitle>
                            </v-list-item>

                            <v-list-item v-if="store.object?.sectionLinks != null && store.object.sectionLinks.length > 0" title="Sub-sections">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-road-variant" class="mr-1"></v-icon>
                                    {{ store.object.sectionLinks.length }}
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