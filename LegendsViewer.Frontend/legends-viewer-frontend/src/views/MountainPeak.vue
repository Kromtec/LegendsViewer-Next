<script setup lang="ts">
import { useMountainPeakStore } from '../stores/worldObjectStores';
import { useMountainPeakMapStore } from '../stores/mapStore';
import WorldObjectPage from '../components/WorldObjectPage.vue';

const store = useMountainPeakStore();
const mapStore = useMountainPeakMapStore();

</script>

<template>
    <WorldObjectPage :store="store" :mapStore="mapStore" :object-type="'mountainpeak'">
        <template v-slot:type-specific-before-table>
            <v-col cols="12" xl="4" lg="6" md="12">
                <!-- Additional Infos -->
                <v-card max-height="400" variant="text">
                    <v-card-text>
                        <v-list lines="two" class="mt-10" style="background-color: rgb(var(--v-theme-background));" density="compact">
                            <v-list-item v-if="store.object?.isVolcano != null" title="Peak Type">
                                <v-list-item-subtitle>
                                    <v-icon :icon="store.object?.isVolcano ? 'mdi-volcano-outline' : 'mdi-summit'" class="mr-1"></v-icon>
                                    {{ store.object?.isVolcano ? 'Volcano' : 'Mountain Peak' }}
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.heightMeter != null" title="Elevation">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-elevation-rise" class="mr-1"></v-icon>
                                    {{ store.object?.heightMeter }} ({{ store.object?.heightFeet }}) &bull; {{ store.object?.height }} DF Units
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.worldRank != null" title="World Height Rank">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-trophy-outline" class="mr-1"></v-icon>
                                    #{{ store.object?.worldRank }} of {{ store.object?.totalPeaksInWorld }} peaks worldwide
                                    <span v-if="store.object?.isHighestInWorld" class="ml-1 text-amber-darken-2 font-weight-bold">&bull; Highest Worldwide</span>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.regionalRank != null && store.object?.totalPeaksInRegion != null && store.object.totalPeaksInRegion > 1 && store.object?.regionLink" title="Regional Height Rank">
                                <v-list-item-subtitle>
                                    <v-icon icon="mdi-podium" class="mr-1"></v-icon>
                                    #{{ store.object?.regionalRank }} of {{ store.object?.totalPeaksInRegion }} peaks in region
                                    <span v-if="store.object?.isHighestInRegion" class="ml-1 text-amber-darken-2 font-weight-bold">&bull; Highest in Region</span>
                                </v-list-item-subtitle>
                            </v-list-item>
                            <v-list-item v-if="store.object?.regionLink" title="Region">
                                <v-list-item-subtitle>
                                    <div v-html="store.object?.regionLink"></div>
                                </v-list-item-subtitle>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
            </v-col>
        </template>
    </WorldObjectPage>
</template>

<style scoped></style>