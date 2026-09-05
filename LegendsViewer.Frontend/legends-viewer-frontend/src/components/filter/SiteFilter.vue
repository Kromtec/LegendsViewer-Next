<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <ThreeStateBoolFilter label="Occupied" v-model="localRules.IsOccupied" />
        <ThreeStateBoolFilter label="Has Structures" v-model="localRules.HasStructures" />
        
        <v-list-item class="mt-1 mb-1">
            <div style="float: left; margin-top: 8px;">Site Type</div>
            <div style="float: right;">
                <v-select
                    density="compact"
                    hide-details
                    label=""
                    :items="siteTypeOptions"
                    item-title="title"
                    item-value="value"
                    v-model="localRules.SiteType"
                    width="220"
                ></v-select>
            </div>
        </v-list-item>

        <v-divider class="mt-3 mb-3"/>

        <NumberFilter
            label="Structures"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.StructureCountActive"
            v-model:operator="localRules.StructureCountOperator"
            v-model:value="localRules.StructureCountValue"
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
import { ref, watch, watchEffect } from 'vue';
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

const siteTypeOptions = [
  { title: 'All Types', value: '' },
  { title: 'Town', value: 'Town' },
  { title: 'Fortress', value: 'Fortress' },
  { title: 'Dark Fortress', value: 'DarkFortress' },
  { title: 'Forest Retreat', value: 'ForestRetreat' },
  { title: 'Hamlet', value: 'Hamlet' },
  { title: 'Tower', value: 'Tower' },
  { title: 'Cave', value: 'Cave' },
  { title: 'Dark Pits', value: 'DarkPits' },
  { title: 'Hillocks', value: 'Hillocks' },
  { title: 'Tomb', value: 'Tomb' },
  { title: 'Vault', value: 'Vault' },
  { title: 'Mountain Halls', value: 'MountainHalls' },
  { title: 'Camp', value: 'Camp' },
  { title: 'Lair', value: 'Lair' },
  { title: 'Labyrinth', value: 'Labyrinth' },
  { title: 'Shrine', value: 'Shrine' },
  { title: 'Fort', value: 'Fort' },
  { title: 'Monastery', value: 'Monastery' },
  { title: 'Castle', value: 'Castle' }
];

const localRules = ref({
    IsOccupied: '',          // "true", "false", or ""
    HasStructures: '',       // "true", "false", or ""
    SiteType: '',            // selected value or ""
    StructureCountActive: false,
    StructureCountOperator: null as FilterOperator | null,
    StructureCountValue: null as number | null,
    BattleCountActive: false,
    BattleCountOperator: null as FilterOperator | null,
    BattleCountValue: null as number | null,
});

watchEffect(() => {
    const filters = props.filters ?? [];

    localRules.value.IsOccupied = findRuleValue(filters, "IsOccupied");
    localRules.value.HasStructures = findRuleValue(filters, "HasStructures");
    localRules.value.SiteType = findRuleValue(filters, "SiteType");

    localRules.value.StructureCountActive = existsRule(filters, "StructureCount");
    localRules.value.StructureCountOperator = findRuleOperator(filters, "StructureCount");
    localRules.value.StructureCountValue = findRuleNumberValue(filters, "StructureCount");

    localRules.value.BattleCountActive = existsRule(filters, "BattleCount");
    localRules.value.BattleCountOperator = findRuleOperator(filters, "BattleCount");
    localRules.value.BattleCountValue = findRuleNumberValue(filters, "BattleCount");
});

watch(localRules, () => {
    if (!props.filters) return;
    const filters = props.filters;

    // Boolean rules
    updateBoolRule(filters, localRules.value.IsOccupied, "IsOccupied");
    updateBoolRule(filters, localRules.value.HasStructures, "HasStructures");

    // SiteType rule
    if (!localRules.value.SiteType) {
        removeRule(filters, "SiteType");
    } else {
        setRule(filters, "SiteType", "Equals", localRules.value.SiteType);
    }

    // Number rules
    updateNumberRule(filters, localRules.value.StructureCountActive, localRules.value.StructureCountOperator, localRules.value.StructureCountValue, "StructureCount");
    updateNumberRule(filters, localRules.value.BattleCountActive, localRules.value.BattleCountOperator, localRules.value.BattleCountValue, "BattleCount");
}, { deep: true });
</script>
