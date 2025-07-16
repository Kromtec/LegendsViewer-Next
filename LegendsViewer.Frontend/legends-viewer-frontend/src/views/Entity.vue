<script setup lang="ts">
import { useEntityStore } from '../stores/worldObjectStores';
import { useEntityMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import ExpandableCard from '../components/ExpandableCard.vue';
import WarfareGraph from '../components/WarfareGraph.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useEntityStore()
const mapStore = useEntityMapStore()

const beforeLists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Wars', items: store.object?.warList ?? [], icon: "mdi-sword-cross", subtitle: "Wars shaping the realm’s history" },
    { title: 'Noble Positions', items: store.object?.entityPositionAssignmentsList ?? [], icon: "mdi-seal", subtitle: "The ruling elite, guiding the fate of realms and people" },
    { title: 'Related Factions and Groups', items: store.object?.entityEntityLinkList ?? [], icon: "mdi-account-group", subtitle: "The organizations and groups connected to this entity" },
    { title: 'Current Sites', items: store.object?.currentSiteList ?? [], icon: "mdi-home-outline", subtitle: "The sites held by this entity, from settlements to strongholds of power" },
    { title: 'Lost Sites', items: store.object?.lostSiteList ?? [], icon: "mdi-home-off-outline", subtitle: "Former strongholds and settlements once under the control of this entity" },
    { title: 'Worshipped Deities', items: store.object?.worshippedLinks ?? [], icon: "mdi-weather-sunset", subtitle: "The divine beings revered across the lands" },
]);

const afterLists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Related Sites', items: store.object?.entitySiteLinkList ?? [], icon: "mdi-home-switch-outline", subtitle: "The locations tied to this entity, from settlements to strongholds of power" },
]);

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'entity'">
        <template v-slot:type-specific-before-table>
            <v-col v-if="store.object?.warGraphData != null" cols="12" xl="4" lg="6" md="12">
                <ExpandableCard title="War Graph" subtitle="Scaled representation of faction roles in war"
                    icon="mdi-sword-cross">
                    <template #compact-content>
                        <WarfareGraph :key="'wargraph' + store.object.id" :data="store.object.warGraphData ?? []" />
                    </template>
                    <template #expanded-content>
                        <WarfareGraph :key="'wargraph-full' + store.object.id" :data="store.object?.warGraphData ?? []" :fullscreen="true" />
                    </template>
                </ExpandableCard>
            </v-col>
            <template v-for="(list, i) in beforeLists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
        </template>
        <template v-slot:type-specific-after-table>
            <template v-for="(list, i) in afterLists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
        </template>
    </WorldObjectPage>
</template>

<style scoped></style>