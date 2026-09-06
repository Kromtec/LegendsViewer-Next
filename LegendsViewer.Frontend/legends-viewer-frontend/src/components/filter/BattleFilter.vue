<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <SelectFilter label="Outcome" :items="outcomeOptions" v-model="localRules.Outcome" />

        <v-divider class="mt-3 mb-3"/>

        <ThreeStateBoolFilter label="Has Mercenaries" v-model="localRules.HasMercenaries" />
        <ThreeStateBoolFilter label="Has Undead Squads" v-model="localRules.HasUndeadSquads" />
        <ThreeStateBoolFilter label="Has Notable Defenders" v-model="localRules.HasNotableDefenders" />

        <v-divider class="mt-3 mb-3"/>

        <NumberFilter
            label="Total Deaths"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.TotalDeathsActive"
            v-model:operator="localRules.TotalDeathsOperator"
            v-model:value="localRules.TotalDeathsValue"
        />
        <NumberFilter
            label="Attacker Count"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.AttackerCountActive"
            v-model:operator="localRules.AttackerCountOperator"
            v-model:value="localRules.AttackerCountValue"
        />
        <NumberFilter
            label="Defender Count"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.DefenderCountActive"
            v-model:operator="localRules.DefenderCountOperator"
            v-model:value="localRules.DefenderCountValue"
        />
        <NumberFilter
            label="Attacker Deaths"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.AttackerDeathsActive"
            v-model:operator="localRules.AttackerDeathsOperator"
            v-model:value="localRules.AttackerDeathsValue"
        />
        <NumberFilter
            label="Defender Deaths"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.DefenderDeathsActive"
            v-model:operator="localRules.DefenderDeathsOperator"
            v-model:value="localRules.DefenderDeathsValue"
        />
    </v-list>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { FilterOperator, FilterRuleDto } from '../../stores/worldObjectStores';
import ThreeStateBoolFilter from './controls/ThreeStateBoolFilter.vue';
import NumberFilter from './controls/NumberFilter.vue';
import SelectFilter from './controls/SelectFilter.vue';
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
import { useFilterRules } from '../../composables/useFilterRules';

const props = defineProps<{
    title: string;
    filters: FilterRuleDto[] | null;
}>();

const outcomeOptions = [
  { title: 'All Outcomes', value: '' },
  { title: 'Attacker Won', value: 'AttackerWon' },
  { title: 'Defender Won', value: 'DefenderWon' },
];

const localRules = ref({
    Outcome: '',                      // selected value or ""
    HasMercenaries: '',               // "true", "false", or ""
    HasUndeadSquads: '',              // "true", "false", or ""
    HasNotableDefenders: '',          // "true", "false", or ""
    TotalDeathsActive: false,
    TotalDeathsOperator: null as FilterOperator | null,
    TotalDeathsValue: null as number | null,
    AttackerCountActive: false,
    AttackerCountOperator: null as FilterOperator | null,
    AttackerCountValue: null as number | null,
    DefenderCountActive: false,
    DefenderCountOperator: null as FilterOperator | null,
    DefenderCountValue: null as number | null,
    AttackerDeathsActive: false,
    AttackerDeathsOperator: null as FilterOperator | null,
    AttackerDeathsValue: null as number | null,
    DefenderDeathsActive: false,
    DefenderDeathsOperator: null as FilterOperator | null,
    DefenderDeathsValue: null as number | null,
});

useFilterRules(
  props,
  localRules,
  (filters) => {
    localRules.value.Outcome = findRuleValue(filters, "Outcome") || findRuleValue(filters, "BattleOutcome");
    localRules.value.HasMercenaries = findRuleValue(filters, "HasMercenaries");
    localRules.value.HasUndeadSquads = findRuleValue(filters, "HasUndeadSquads");
    localRules.value.HasNotableDefenders = findRuleValue(filters, "HasNotableDefenders");

    localRules.value.TotalDeathsActive = existsRule(filters, "TotalDeaths") || existsRule(filters, "DeathCount");
    localRules.value.TotalDeathsOperator = findRuleOperator(filters, "TotalDeaths") || findRuleOperator(filters, "DeathCount");
    localRules.value.TotalDeathsValue = findRuleNumberValue(filters, "TotalDeaths") ?? findRuleNumberValue(filters, "DeathCount");

    localRules.value.AttackerCountActive = existsRule(filters, "AttackerCount");
    localRules.value.AttackerCountOperator = findRuleOperator(filters, "AttackerCount");
    localRules.value.AttackerCountValue = findRuleNumberValue(filters, "AttackerCount");

    localRules.value.DefenderCountActive = existsRule(filters, "DefenderCount");
    localRules.value.DefenderCountOperator = findRuleOperator(filters, "DefenderCount");
    localRules.value.DefenderCountValue = findRuleNumberValue(filters, "DefenderCount");

    localRules.value.AttackerDeathsActive = existsRule(filters, "AttackerDeaths") || existsRule(filters, "AttackerDeathCount");
    localRules.value.AttackerDeathsOperator = findRuleOperator(filters, "AttackerDeaths") || findRuleOperator(filters, "AttackerDeathCount");
    localRules.value.AttackerDeathsValue = findRuleNumberValue(filters, "AttackerDeaths") ?? findRuleNumberValue(filters, "AttackerDeathCount");

    localRules.value.DefenderDeathsActive = existsRule(filters, "DefenderDeaths") || existsRule(filters, "DefenderDeathCount");
    localRules.value.DefenderDeathsOperator = findRuleOperator(filters, "DefenderDeaths") || findRuleOperator(filters, "DefenderDeathCount");
    localRules.value.DefenderDeathsValue = findRuleNumberValue(filters, "DefenderDeaths") ?? findRuleNumberValue(filters, "DefenderDeathCount");
  },
  (filters) => {
    if (!localRules.value.Outcome) {
        removeRule(filters, "Outcome");
        removeRule(filters, "BattleOutcome");
    } else {
        removeRule(filters, "BattleOutcome");
        setRule(filters, "Outcome", "Equals", localRules.value.Outcome);
    }

    updateBoolRule(filters, localRules.value.HasMercenaries, "HasMercenaries");
    updateBoolRule(filters, localRules.value.HasUndeadSquads, "HasUndeadSquads");
    updateBoolRule(filters, localRules.value.HasNotableDefenders, "HasNotableDefenders");

    updateNumberRule(filters, localRules.value.TotalDeathsActive, localRules.value.TotalDeathsOperator, localRules.value.TotalDeathsValue, "TotalDeaths");
    updateNumberRule(filters, localRules.value.AttackerCountActive, localRules.value.AttackerCountOperator, localRules.value.AttackerCountValue, "AttackerCount");
    updateNumberRule(filters, localRules.value.DefenderCountActive, localRules.value.DefenderCountOperator, localRules.value.DefenderCountValue, "DefenderCount");
    updateNumberRule(filters, localRules.value.AttackerDeathsActive, localRules.value.AttackerDeathsOperator, localRules.value.AttackerDeathsValue, "AttackerDeaths");
    updateNumberRule(filters, localRules.value.DefenderDeathsActive, localRules.value.DefenderDeathsOperator, localRules.value.DefenderDeathsValue, "DefenderDeaths");
  }
);
</script>

<style scoped></style>
