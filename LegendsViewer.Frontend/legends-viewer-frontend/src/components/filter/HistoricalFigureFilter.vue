<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <ThreeStateBoolFilter label="Alive" v-model="localRules.Alive" />
        <ThreeStateBoolFilter label="Deity" v-model="localRules.Deity" />
        <div class="d-flex align-center justify-space-between px-4 py-2" style="width: 100%;">
            <div class="text-body-2 pr-2">Special</div>
            <v-btn-toggle density="compact" v-model="localRules.Special" divided class="flex-shrink-0">
                <v-btn value="vampire" size="small">
                    <img :src="vampireImageData" width="24" height="24" />
                </v-btn>
                <v-btn value="werebeast" size="small">
                    <img :src="werebeastImageData" width="24" height="24" />
                </v-btn>
                <v-btn value="necromancer" size="small">
                    <img :src="necromancerImageData" width="24" height="24" />
                </v-btn>
            </v-btn-toggle>
        </div>
        <v-divider class="mt-3 mb-3"/>
        <NumberFilter
            label="Age"
            :operators="['Equals','NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.AgeRuleActive"
            v-model:operator="localRules.AgeOperator"
            v-model:value="localRules.AgeValue"
            />
        <NumberFilter
            label="Born"
            :operators="['Equals','NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.BornRuleActive"
            v-model:operator="localRules.BornOperator"
            v-model:value="localRules.BornValue"
            />
        <NumberFilter
            label="Died"
            :operators="['Equals','NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.DiedRuleActive"
            v-model:operator="localRules.DiedOperator"
            v-model:value="localRules.DiedValue"
            />
    </v-list>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { vampireImageData, werebeastImageData, necromancerImageData } from '../FamilyTree.vue';
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
import { useFilterRules } from '../../composables/useFilterRules';

// Accept an array of filter rules as a prop
const props = defineProps<{
    title: string
    filters: FilterRuleDto[] | null;
}>();

// Create local copies for binding. You might use a more structured model instead.
const localRules = ref({
    Alive: '',      // e.g., "true" or "false"
    Deity: '',      // e.g., "true" or "false"
    Special: '',    // e.g., "vampire", "werebeast", or "necromancer"
    AgeRuleActive: false,
    AgeOperator: null as FilterOperator | null,
    AgeValue: null as number | null,
    BornRuleActive: false,
    BornOperator: null as FilterOperator | null,
    BornValue: null as number | null,
    DiedRuleActive: false,
    DiedOperator: null as FilterOperator | null,
    DiedValue: null as number | null
});

useFilterRules(
  props,
  localRules,
  (filters) => {
    localRules.value.Alive = findRuleValue(filters, "IsAlive");
    localRules.value.Deity = findRuleValue(filters, "IsDeity");

    if (findRuleValue(filters, "IsVampire") === "true") {
        localRules.value.Special = "vampire";
    } else if (findRuleValue(filters, "IsWerebeast") === "true") {
        localRules.value.Special = "werebeast";
    } else if (findRuleValue(filters, "IsNecromancer") === "true") {
        localRules.value.Special = "necromancer";
    } else {
        localRules.value.Special = '';
    }
    localRules.value.AgeRuleActive = existsRule(filters, "Age");
    localRules.value.AgeOperator = findRuleOperator(filters, "Age");
    localRules.value.AgeValue = findRuleNumberValue(filters, "Age");
    localRules.value.BornRuleActive = existsRule(filters, "BirthYear");
    localRules.value.BornOperator = findRuleOperator(filters, "BirthYear");
    localRules.value.BornValue = findRuleNumberValue(filters, "BirthYear");
    localRules.value.DiedRuleActive = existsRule(filters, "DeathYear");
    localRules.value.DiedOperator = findRuleOperator(filters, "DeathYear");
    localRules.value.DiedValue = findRuleNumberValue(filters, "DeathYear");
  },
  (filters) => {
    // ALIVE and DEITY filter logic
    updateBoolRule(filters, localRules.value.Alive, "IsAlive");
    updateBoolRule(filters, localRules.value.Deity, "IsDeity");

    // AGE, BORN, DIED filter logic
    updateNumberRule(filters, localRules.value.AgeRuleActive, localRules.value.AgeOperator, localRules.value.AgeValue, "Age");
    updateNumberRule(filters, localRules.value.BornRuleActive, localRules.value.BornOperator, localRules.value.BornValue, "BirthYear");
    updateNumberRule(filters, localRules.value.DiedRuleActive, localRules.value.DiedOperator, localRules.value.DiedValue, "DeathYear");

    // SPECIAL filter logic
    const specials = ["IsVampire", "IsWerebeast", "IsNecromancer"];
    specials.forEach((s) => removeRule(filters, s));

    switch (localRules.value.Special) {
        case 'vampire':
            setRule(filters, "IsVampire", "Equals", "true");
            break;
        case 'werebeast':
            setRule(filters, "IsWerebeast", "Equals", "true");
            break;
        case 'necromancer':
            setRule(filters, "IsNecromancer", "Equals", "true");
            break;
    }
  }
);
</script>