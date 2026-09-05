<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <v-list-item class="mt-1 mb-1">
            <div style="float: left; margin-top: 8px;">Biome</div>
            <div style="float: right;">
                <v-select
                    density="compact"
                    hide-details
                    label=""
                    :items="biomeOptions"
                    item-title="title"
                    item-value="value"
                    v-model="localRules.Biome"
                    width="220"
                ></v-select>
            </div>
        </v-list-item>

        <v-list-item class="mt-1 mb-1">
            <div style="float: left; margin-top: 8px;">Evilness</div>
            <div style="float: right;">
                <v-select
                    density="compact"
                    hide-details
                    label=""
                    :items="evilnessOptions"
                    item-title="title"
                    item-value="value"
                    v-model="localRules.Evilness"
                    width="220"
                ></v-select>
            </div>
        </v-list-item>

        <v-divider class="mt-3 mb-3"/>

        <ThreeStateBoolFilter label="Has Sites" v-model="localRules.HasSites" />
        <ThreeStateBoolFilter label="Has Force" v-model="localRules.HasForce" />

        <v-divider class="mt-3 mb-3"/>

        <NumberFilter
            label="Size (Tiles)"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.SquareTilesActive"
            v-model:operator="localRules.SquareTilesOperator"
            v-model:value="localRules.SquareTilesValue"
        />
        <NumberFilter
            label="Sites"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.SiteCountActive"
            v-model:operator="localRules.SiteCountOperator"
            v-model:value="localRules.SiteCountValue"
        />
        <NumberFilter
            label="Battles"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.BattleCountActive"
            v-model:operator="localRules.BattleCountOperator"
            v-model:value="localRules.BattleCountValue"
        />
    </v-list>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { FilterOperator, FilterRuleDto } from '../../stores/worldObjectStores';
import ThreeStateBoolFilter from './controls/ThreeStateBoolFilter.vue';
import NumberFilter from './controls/NumberFilter.vue';
import {
  existsRule,
  findRuleValue,
  findRuleNumberValue,
  findRuleOperator,
  removeRule,
  setRule,
  updateBoolRule,
  updateNumberRule
} from '../../utils/filterRuleHelpers';

const props = defineProps<{
    title: string;
    filters: FilterRuleDto[] | null;
}>();

const biomeOptions = [
  { title: 'All Biomes', value: '' },
  { title: 'Mountains', value: 'Mountains' },
  { title: 'Forest', value: 'Forest' },
  { title: 'Ocean', value: 'Ocean' },
  { title: 'Desert', value: 'Desert' },
  { title: 'Glacier', value: 'Glacier' },
  { title: 'Tundra', value: 'Tundra' },
  { title: 'Wetland', value: 'Wetland' },
  { title: 'Grassland', value: 'Grassland' },
  { title: 'Hills', value: 'Hills' },
  { title: 'Lake', value: 'Lake' },
];

const evilnessOptions = [
  { title: 'All Alignments', value: '' },
  { title: 'Good', value: 'Good' },
  { title: 'Neutral', value: 'Neutral' },
  { title: 'Evil', value: 'Evil' },
];

const localRules = ref({
    Biome: '',              // selected value or ""
    Evilness: '',           // selected value or ""
    HasSites: '',           // "true", "false", or ""
    HasForce: '',           // "true", "false", or ""
    SquareTilesActive: false,
    SquareTilesOperator: null as FilterOperator | null,
    SquareTilesValue: null as number | null,
    SiteCountActive: false,
    SiteCountOperator: null as FilterOperator | null,
    SiteCountValue: null as number | null,
    BattleCountActive: false,
    BattleCountOperator: null as FilterOperator | null,
    BattleCountValue: null as number | null,
});

let isUpdatingFromProps = false;

watch(() => props.filters, (filters) => {
    if (!filters) return;
    isUpdatingFromProps = true;

    localRules.value.Biome = findRuleValue(filters, "Biome") || findRuleValue(filters, "RegionType");
    localRules.value.Evilness = findRuleValue(filters, "Evilness");
    localRules.value.HasSites = findRuleValue(filters, "HasSites");
    localRules.value.HasForce = findRuleValue(filters, "HasForce");

    localRules.value.SquareTilesActive = existsRule(filters, "SquareTiles");
    localRules.value.SquareTilesOperator = findRuleOperator(filters, "SquareTiles");
    localRules.value.SquareTilesValue = findRuleNumberValue(filters, "SquareTiles");

    localRules.value.SiteCountActive = existsRule(filters, "SiteCount");
    localRules.value.SiteCountOperator = findRuleOperator(filters, "SiteCount");
    localRules.value.SiteCountValue = findRuleNumberValue(filters, "SiteCount");

    localRules.value.BattleCountActive = existsRule(filters, "BattleCount");
    localRules.value.BattleCountOperator = findRuleOperator(filters, "BattleCount");
    localRules.value.BattleCountValue = findRuleNumberValue(filters, "BattleCount");

    isUpdatingFromProps = false;
}, { immediate: true, deep: true });

watch(localRules, () => {
    if (isUpdatingFromProps || !props.filters) return;
    const filters = props.filters;

    // Biome rule
    if (!localRules.value.Biome) {
        removeRule(filters, "Biome");
        removeRule(filters, "RegionType");
    } else {
        removeRule(filters, "RegionType");
        setRule(filters, "Biome", "Equals", localRules.value.Biome);
    }

    // Evilness rule
    if (!localRules.value.Evilness) {
        removeRule(filters, "Evilness");
    } else {
        setRule(filters, "Evilness", "Equals", localRules.value.Evilness);
    }

    // Boolean rules
    updateBoolRule(filters, localRules.value.HasSites, "HasSites");
    updateBoolRule(filters, localRules.value.HasForce, "HasForce");

    // Number rules
    updateNumberRule(filters, localRules.value.SquareTilesActive, localRules.value.SquareTilesOperator, localRules.value.SquareTilesValue, "SquareTiles");
    updateNumberRule(filters, localRules.value.SiteCountActive, localRules.value.SiteCountOperator, localRules.value.SiteCountValue, "SiteCount");
    updateNumberRule(filters, localRules.value.BattleCountActive, localRules.value.BattleCountOperator, localRules.value.BattleCountValue, "BattleCount");
}, { deep: true });
</script>
