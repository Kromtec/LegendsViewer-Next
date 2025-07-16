<script setup lang="ts">
import { useHistoricalFigureStore } from '../stores/worldObjectStores';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import FamilyTree from '../components/FamilyTree.vue';
import LegendsCardList from '../components/LegendsCardList.vue';
import ExpandableCard from '../components/ExpandableCard.vue';
import { computed, ComputedRef } from 'vue';
import { LegendLinkListData } from '../types/legends';

const store = useHistoricalFigureStore()

const getByRank = (subrank: string) => {
    switch (subrank) {
        case 'dabbl':
            return 'mdi-square-medium-outline'
        case 'novic':
            return 'mdi-square-medium'
        case 'adequ':
        case 'compe':
            return 'mdi-square-outline'
        case 'skill':
        case 'profi':
            return 'mdi-square'
        case 'talen':
        case 'adept':
            return 'mdi-star-outline'
        case 'exper':
        case 'profe':
            return 'mdi-star'
        case 'accom':
        case 'great':
            return 'mdi-diamond-outline'
        case 'maste':
            return 'mdi-diamond-outline'
        case 'highm':
            return 'mdi-crown-outline'
        case 'grand':
            return 'mdi-crown'
        case 'legen':
            return 'mdi-trophy '

        default:
            return 'mdi-help-circle-outline'
    }
}

const preBeforeLists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Profile Overview', items: store.object?.miscList ?? [], icon: "mdi-account-details-outline", subtitle: "A summary of significant details and personal traits" },
]);

const beforeLists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Related Factions and Groups', items: store.object?.relatedEntityList ?? [], icon: "mdi-account-group", subtitle: "The organizations and groups connected to this figure" },
    { title: 'Related Sites', items: store.object?.relatedSiteList ?? [], icon: "mdi-home-switch-outline", subtitle: "The locations tied to this figure, from settlements to strongholds of power" },
    { title: 'Close Relationships', items: store.object?.relatedHistoricalFigureList ?? [], icon: "mdi-heart-outline", subtitle: "The personal and familial bonds that define the life of the historical figure" },
    { title: 'Vague Relationships', items: store.object?.vagueRelationshipList ?? [], icon: "mdi-account-question-outline", subtitle: "The uncertain and loosely documented associations in the figure’s life, from distant friends to comrades-in-arms" },
    { title: 'Worshipped Deities', items: store.object?.worshippedDeities ?? [], icon: "mdi-hand-heart-outline", subtitle: "The divine beings revered by the historical figure, shaping their beliefs and actions" },
    { title: 'Journey Pets', items: store.object?.journeyPets ?? [], icon: "mdi-paw", subtitle: "Trusted animal companions that accompanied this figure on their travels" },
    { title: 'Noble Positions', items: store.object?.positionList ?? [], icon: "mdi-seal", subtitle: "The ruling elite, guiding the fate of realms and people" },
    { title: 'Worshipping Figures', items: store.object?.worshippingFiguresList ?? [], icon: "mdi-meditation", subtitle: "Historical figures who revere this creature, deity or force of nature" },
    { title: 'Worshipping Entities', items: store.object?.worshippingEntitiesList ?? [], icon: "mdi-crowd", subtitle: "Groups or organizations that hold this deity in reverence" },
]);

const afterLists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Notable Kills', items: store.object?.notableKillList ?? [], icon: "mdi-skull-crossbones-outline", subtitle: "A record of the most significant and renowned kills achieved by this figure" },
    { title: 'Artifacts', items: store.object?.holdingArtifactLinks ?? [], icon: "mdi-diamond", subtitle: "Currently held artifacts" },
    { title: 'Dedicated Structures', items: store.object?.dedicatedStructuresLinks ?? [], icon: "mdi-home-silo", subtitle: "Structures dedicated to " + store.object?.name },
    { title: 'Snatcher Of', items: store.object?.snatchedHfLinks ?? [], icon: "mdi-chess-bishop", subtitle: "Victims snatched by " + store.object?.name },
    { title: 'Battles', items: store.object?.battleLinks ?? [], icon: "mdi-chess-bishop", subtitle: "Battles fought by " + store.object?.name },
    { title: 'Beast Attacks', items: store.object?.beastAttackLinks ?? [], icon: "mdi-chess-knight", subtitle: "Beast attacks recorded" }
]);

</script>

<template>
    <WorldObjectPage :store="store" :object-type="'hf'">
        <template v-slot:type-specific-before-table>
            <template v-for="(list, i) in preBeforeLists" :key="i">
                <v-col v-if="list?.items.length" cols="12" xl="4" lg="6" md="12">
                    <LegendsCardList :list="list" />
                </v-col>
            </template>
            <template v-if="store.object?.familyTreeData">
                <v-col cols="12" xl="4" lg="6" md="12">
                    <ExpandableCard title="Family Tree" subtitle="Ancestry and Descendants" icon="mdi-family-tree">
                        <template #compact-content>
                            <FamilyTree :key="store.object.id" :data="store.object.familyTreeData" />
                        </template>
                        <template #expanded-content>
                            <FamilyTree :key="store.object.id" :data="store.object.familyTreeData" :fullscreen="true" />
                        </template>
                    </ExpandableCard>
                </v-col>
            </template>
            <v-col v-if="store.object?.skillDescriptions != null && store.object.skillDescriptions.length > 0" cols="12"
                xl="4" lg="6" md="12">
                <v-card title="Skills" subtitle="Expertise and Abilities" height="400" variant="text">
                    <template v-slot:prepend>
                        <v-icon class="mr-2" icon="mdi-account-star-outline" size="32px"></v-icon>
                    </template>
                    <v-card-text height="360">
                        <v-list class="ml-8 pb-10" height="360" scrollable
                            style="background-color: rgb(var(--v-theme-background));">
                            <v-list-item v-for="(skillDescription, i) in store.object?.skillDescriptions" :key="i">
                                <template v-slot:prepend>
                                    <v-chip :class="'mr-4 ' + skillDescription.category" label>
                                        <v-icon :icon="getByRank(skillDescription.subrank ?? '')"></v-icon>
                                    </v-chip>
                                </template>
                                <v-list-item-title>{{ skillDescription.rank }}
                                    {{ skillDescription.name }}</v-list-item-title>
                                <v-list-item-subtitle>{{ skillDescription.token }} | {{ skillDescription.rank
                                    }}</v-list-item-subtitle>
                                <template v-slot:append>
                                    <v-chip :class="skillDescription.category" label>
                                        {{ skillDescription.points }}
                                    </v-chip>
                                </template>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
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