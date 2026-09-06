<script setup lang="ts">
import { useBattleStore } from '../stores/worldObjectStores';
import { useBattleMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import DoughnutChart from '../components/DoughnutChart.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useBattleStore()
const mapStore = useBattleMapStore()


const lists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Battle Overview', items: store.object?.miscList ?? [], icon: "mdi-swords-crossed", subtitle: "A summary of significant details of this battle" },
    { title: 'Notable Deaths', items: store.object?.notableDeathLinks ?? [], icon: "mdi-grave-stone", subtitle: "Key figures who died in this battle" },
]);

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'battle'">
        <template v-slot:type-specific-before-table>
            <template v-for="(list, i) in lists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
           <v-col v-if="store.object?.deathsByRace?.labels != null && store.object?.deathsByRace?.labels?.length > 0"
                cols="12" xl="4" lg="6" md="12">
                <v-card title="Deaths by Race" :subtitle="'How many deaths were in this battle, grouped by the race'"
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