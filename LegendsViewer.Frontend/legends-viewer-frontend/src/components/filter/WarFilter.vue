<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <ThreeStateBoolFilter label="Is Ongoing" v-model="localRules.IsOngoing" />

        <v-divider class="mt-3 mb-3"/>

        <NumberFilter
            label="Total Deaths"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.DeathCountActive"
            v-model:operator="localRules.DeathCountOperator"
            v-model:value="localRules.DeathCountValue"
        />
        <NumberFilter
            label="Attacker Deaths"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.AttackerDeathCountActive"
            v-model:operator="localRules.AttackerDeathCountOperator"
            v-model:value="localRules.AttackerDeathCountValue"
        />
        <NumberFilter
            label="Defender Deaths"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.DefenderDeathCountActive"
            v-model:operator="localRules.DefenderDeathCountOperator"
            v-model:value="localRules.DefenderDeathCountValue"
        />
        <NumberFilter
            label="Battles"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.BattleCountActive"
            v-model:operator="localRules.BattleCountOperator"
            v-model:value="localRules.BattleCountValue"
        />
        <NumberFilter
            label="Sites Lost"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.SitesLostCountActive"
            v-model:operator="localRules.SitesLostCountOperator"
            v-model:value="localRules.SitesLostCountValue"
        />
        <NumberFilter
            label="Length (Years)"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.LengthActive"
            v-model:operator="localRules.LengthOperator"
            v-model:value="localRules.LengthValue"
        />
    </v-list>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { FilterOperator, FilterRuleDto } from '../../stores/worldObjectStores';
import ThreeStateBoolFilter from './controls/ThreeStateBoolFilter.vue';
import NumberFilter from './controls/NumberFilter.vue';
import {
  existsRule,
  findRuleValue,
  findRuleNumberValue,
  findRuleOperator,
  updateBoolRule,
  updateNumberRule
} from '../../utils/filterRuleHelpers';

import { useFilterRules } from '../../composables/useFilterRules';

const props = defineProps<{
    title: string;
    filters: FilterRuleDto[] | null;
}>();

const localRules = ref({
    IsOngoing: '',                 // "true", "false", or ""
    DeathCountActive: false,
    DeathCountOperator: null as FilterOperator | null,
    DeathCountValue: null as number | null,
    AttackerDeathCountActive: false,
    AttackerDeathCountOperator: null as FilterOperator | null,
    AttackerDeathCountValue: null as number | null,
    DefenderDeathCountActive: false,
    DefenderDeathCountOperator: null as FilterOperator | null,
    DefenderDeathCountValue: null as number | null,
    BattleCountActive: false,
    BattleCountOperator: null as FilterOperator | null,
    BattleCountValue: null as number | null,
    SitesLostCountActive: false,
    SitesLostCountOperator: null as FilterOperator | null,
    SitesLostCountValue: null as number | null,
    LengthActive: false,
    LengthOperator: null as FilterOperator | null,
    LengthValue: null as number | null,
});

useFilterRules(
  props,
  localRules,
  (filters) => {
    localRules.value.IsOngoing = findRuleValue(filters, "IsOngoing");

    localRules.value.DeathCountActive = existsRule(filters, "DeathCount");
    localRules.value.DeathCountOperator = findRuleOperator(filters, "DeathCount");
    localRules.value.DeathCountValue = findRuleNumberValue(filters, "DeathCount");

    localRules.value.AttackerDeathCountActive = existsRule(filters, "AttackerDeathCount");
    localRules.value.AttackerDeathCountOperator = findRuleOperator(filters, "AttackerDeathCount");
    localRules.value.AttackerDeathCountValue = findRuleNumberValue(filters, "AttackerDeathCount");

    localRules.value.DefenderDeathCountActive = existsRule(filters, "DefenderDeathCount");
    localRules.value.DefenderDeathCountOperator = findRuleOperator(filters, "DefenderDeathCount");
    localRules.value.DefenderDeathCountValue = findRuleNumberValue(filters, "DefenderDeathCount");

    localRules.value.BattleCountActive = existsRule(filters, "BattleCount") || existsRule(filters, "Battles");
    localRules.value.BattleCountOperator = findRuleOperator(filters, "BattleCount") || findRuleOperator(filters, "Battles");
    localRules.value.BattleCountValue = findRuleNumberValue(filters, "BattleCount") ?? findRuleNumberValue(filters, "Battles");

    localRules.value.SitesLostCountActive = existsRule(filters, "SitesLostCount") || existsRule(filters, "SitesLost");
    localRules.value.SitesLostCountOperator = findRuleOperator(filters, "SitesLostCount") || findRuleOperator(filters, "SitesLost");
    localRules.value.SitesLostCountValue = findRuleNumberValue(filters, "SitesLostCount") ?? findRuleNumberValue(filters, "SitesLost");

    localRules.value.LengthActive = existsRule(filters, "Length");
    localRules.value.LengthOperator = findRuleOperator(filters, "Length");
    localRules.value.LengthValue = findRuleNumberValue(filters, "Length");
  },
  (filters) => {
    updateBoolRule(filters, localRules.value.IsOngoing, "IsOngoing");

    updateNumberRule(filters, localRules.value.DeathCountActive, localRules.value.DeathCountOperator, localRules.value.DeathCountValue, "DeathCount");
    updateNumberRule(filters, localRules.value.AttackerDeathCountActive, localRules.value.AttackerDeathCountOperator, localRules.value.AttackerDeathCountValue, "AttackerDeathCount");
    updateNumberRule(filters, localRules.value.DefenderDeathCountActive, localRules.value.DefenderDeathCountOperator, localRules.value.DefenderDeathCountValue, "DefenderDeathCount");
    updateNumberRule(filters, localRules.value.BattleCountActive, localRules.value.BattleCountOperator, localRules.value.BattleCountValue, "BattleCount");
    updateNumberRule(filters, localRules.value.SitesLostCountActive, localRules.value.SitesLostCountOperator, localRules.value.SitesLostCountValue, "SitesLostCount");
    updateNumberRule(filters, localRules.value.LengthActive, localRules.value.LengthOperator, localRules.value.LengthValue, "Length");
  }
);
</script>
