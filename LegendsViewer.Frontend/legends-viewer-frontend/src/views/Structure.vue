<script setup lang="ts">
import { useStructureStore } from '../stores/worldObjectStores';
import { computed, ComputedRef } from 'vue';
import { useStructureMapStore } from '../stores/mapStore';
import { LegendLinkListData } from '../types/legends';
import WorldObjectPage from '../components/WorldObjectPage.vue';
import LegendsCardList from '../components/LegendsCardList.vue';

const store = useStructureStore();
const mapStore = useStructureMapStore();

const lists: ComputedRef<LegendLinkListData[]> = computed(() => [
    { title: 'Inhabitants', items: store.object?.inhabitantLinks ?? [], icon: "mdi-account-group", subtitle: "Figures residing or present in this structure" },
    { title: 'Copied Artifacts', items: store.object?.copiedArtifactLinks ?? [], icon: "mdi-book-multiple", subtitle: "Written content and copied works stored here" }
]);

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'structure'">
        <template v-slot:type-specific-before-table>
            <v-col cols="12" xl="4" lg="6" md="12">
                <!-- Additional Infos -->
                <v-card max-height="500" variant="text">
                    <v-card-text>
                        <v-list lines="two" class="mt-10" style="background-color: rgb(var(--v-theme-background));" density="compact">
                            <v-list-item v-if="store.object?.siteToLink" title="Site">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.siteToLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.type || store.object?.subtype" title="Type">
                                <v-list-item-subtitle>
                                    {{ store.object?.type }} <span v-if="store.object?.subtype">({{ store.object?.subtype }})</span>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.altName" title="Alternative Name">
                                <v-list-item-subtitle>
                                    {{ store.object?.altName }}
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.entityToLink" title="Entity / Organisation">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.entityToLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.ownerToLink" title="Owner / Leader">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.ownerToLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.deityToLink" title="Deity Worshipped">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.deityToLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.religionToLink" title="Religion">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.religionToLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.inhabitantLinks && store.object?.inhabitantLinks.length > 0" title="Inhabitants">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-account-group" class="mr-1"></v-icon>
                                    {{ store.object?.inhabitantLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.copiedArtifactLinks && store.object?.copiedArtifactLinks.length > 0" title="Copied Artifacts">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-book-multiple" class="mr-1"></v-icon>
                                    {{ store.object?.copiedArtifactLinks.length }}
                                </v-list-item-subtitle>
                            </v-list-item>
                        </v-list>
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