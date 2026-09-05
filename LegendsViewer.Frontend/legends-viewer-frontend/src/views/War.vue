<script setup lang="ts">
import { useWarStore } from '../stores/worldObjectStores';
import { useWarMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import ExpandableCard from '../components/ExpandableCard.vue';
import DoughnutChart from '../components/DoughnutChart.vue';
import WarfareGraph from '../components/WarfareGraph.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useWarStore()
const mapStore = useWarMapStore()


const lists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'War Overview', items: store.object?.miscList ?? [], icon: "mdi-swords-crossed", subtitle: "A summary of significant details of this war" },
    { title: 'Battles', items: store.object?.battleList ?? [], icon: "mdi-chess-bishop", subtitle: "Battles shaping the realm’s history" },
    { title: 'Notable Deaths', items: store.object?.notableDeathLinks ?? [], icon: "mdi-grave-stone", subtitle: "Key figures who died in this war" },
]);

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'war'">
        <template v-slot:type-specific-before-table>
            <v-col v-if="store.object?.battleGraphData != null" cols="12" xl="4" lg="6" md="12">
                <ExpandableCard title="Battle Graph" subtitle="Scaled representation of faction roles in battle"
                    icon="mdi-chess-bishop">
                    <template #compact-content>
                        <WarfareGraph :key="'battlegraph' + store.object.id" :data="store.object.battleGraphData ?? []" />
                    </template>
                    <template #expanded-content>
                        <WarfareGraph :key="'battlegraph-full' + store.object.id" :data="store.object?.battleGraphData ?? []" :fullscreen="true" />
                    </template>
                </ExpandableCard>
            </v-col>
            <template v-for="(list, i) in lists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
           <v-col v-if="store.object?.deathsByRace?.labels != null && store.object?.deathsByRace?.labels?.length > 0"
                cols="12" xl="4" lg="6" md="12">
                <v-card title="Deaths by Race" :subtitle="'How many deaths were in this war, grouped by the race'"
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
    </WorldObjectPage>
</template>

<style scoped></style>