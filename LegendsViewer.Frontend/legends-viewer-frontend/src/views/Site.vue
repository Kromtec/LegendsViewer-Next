<script setup lang="ts">
import { useSiteStore } from '../stores/worldObjectStores';
import { computed, ComputedRef } from 'vue'
import { useSiteMapStore } from '../stores/mapStore';
import { LegendLinkListData } from '../types/legends';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import DoughnutChart from '../components/DoughnutChart.vue';

const store = useSiteStore()
const mapStore = useSiteMapStore()

const lists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Structures', items: store.object?.structuresLinks ?? [], icon: "mdi-home-silo", subtitle: "Notable buildings at this site" },
    { title: 'Related Historical Figures', items: store.object?.relatedHistoricalFigureLinks ?? [], icon: "mdi-account-star", subtitle: "Famous figures tied to this site" },
    { title: 'Notable Deaths', items: store.object?.notableDeathLinks ?? [], icon: "mdi-grave-stone", subtitle: "Key figures who died here" },
    { title: 'Battles', items: store.object?.battleLinks ?? [], icon: "mdi-chess-bishop", subtitle: "Battles fought at this site" },
    { title: 'Conquerings', items: store.object?.conqueringLinks ?? [], icon: "mdi-chess-pawn", subtitle: "Conquests that took place here" },
    { title: 'Raids', items: store.object?.raidLinks ?? [], icon: "mdi-lightning-bolt", subtitle: "Raids carried out at this site" },
    { title: 'Duels', items: store.object?.duelLinks ?? [], icon: "mdi-sword-cross", subtitle: "Duels fought at this site" },
    { title: 'Persecutions', items: store.object?.persecutionLinks ?? [], icon: "mdi-map-marker-alert", subtitle: "Acts of persecution at this site" },
    { title: 'Insurrections', items: store.object?.insurrectionLinks ?? [], icon: "mdi-map-marker-alert", subtitle: "Revolts and uprisings here" },
    { title: 'Abductions', items: store.object?.abductionLinks ?? [], icon: "mdi-map-marker-alert", subtitle: "Kidnappings that happened here" },
    { title: 'Beast Attacks', items: store.object?.beastAttackLinks ?? [], icon: "mdi-chess-knight", subtitle: "Beast attacks recorded here" }
]);

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'site'">
        <template v-slot:type-specific-before-table>
            <v-col cols="12" xl="4" lg="6" md="12">
                <!-- Additional Infos -->
                <v-card max-height="400" variant="text">
                    <v-card-text>
                        <v-list lines="two" class="mt-10" style="background-color: rgb(var(--v-theme-background));">
                            <v-list-item v-if="store.object?.regionToLink" title="Region">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.regionToLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.currentOwnerToLink" title="Current Owner">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.currentOwnerToLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item
                                v-if="store.object?.battleLinks != null && store.object?.battleLinks.length > 0"
                                title="Battles">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-chess-bishop"></v-icon>
                                    {{ store.object?.battleLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item
                                v-if="store.object?.beastAttackLinks != null && store.object?.beastAttackLinks.length > 0"
                                title="Beast Attacks">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-chess-knight"></v-icon>
                                    {{ store.object?.beastAttackLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item
                                v-if="store.object?.notableDeathLinks != null && store.object?.notableDeathLinks.length > 0"
                                title="Notable Deaths">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-grave-stone"></v-icon>
                                    {{ store.object?.notableDeathLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col v-if="store.object?.populationsByRace?.labels != null && store.object?.populationsByRace?.labels?.length > 0"
                cols="12" xl="4" lg="6" md="12">
                <v-card title="Population by Race" subtitle="A demographic breakdown of the population at this site"
                    height="400" variant="text">
                    <template v-slot:prepend>
                        <v-icon class="mr-2" icon="mdi-chart-donut" size="32px"></v-icon>
                    </template>
                    <v-card-text>
                        <DoughnutChart :chart-data="store.object?.populationsByRace" />
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col v-if="store.object?.deathsByRace?.labels != null && store.object?.deathsByRace?.labels?.length > 0"
                cols="12" xl="4" lg="6" md="12">
                <v-card title="Deaths by Race" :subtitle="'How many deaths were at this site, grouped by the race'"
                    height="400" variant="text">
                    <template v-slot:prepend>
                        <v-icon class="mr-2" icon="mdi-grave-stone" size="32px"></v-icon>
                    </template>
                    <v-card-text>
                        <DoughnutChart :chart-data="store.object?.deathsByRace" />
                    </v-card-text>
                </v-card>
            </v-col>
        </template>
        <template v-slot:type-specific-after-table>
            <template v-for="(list, i) in lists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
        </template>
    </WorldObjectPage>

</template>

<style scoped></style>