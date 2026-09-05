<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <SelectFilter label="Structure Type" :items="structureTypeOptions" v-model="localRules.StructureType" />
        <SelectFilter label="Subtype" :items="structureSubTypeOptions" v-model="localRules.StructureSubType" />

        <v-divider class="mt-3 mb-3"/>

        <ThreeStateBoolFilter label="Has Inhabitants" v-model="localRules.HasInhabitants" />
        <ThreeStateBoolFilter label="Has Deity" v-model="localRules.HasDeity" />
        <ThreeStateBoolFilter label="Has Religion" v-model="localRules.HasReligion" />
        <ThreeStateBoolFilter label="Has Entity" v-model="localRules.HasEntity" />
        <ThreeStateBoolFilter label="Has Copied Artifacts" v-model="localRules.HasCopiedArtifacts" />

        <v-divider class="mt-3 mb-3"/>

        <NumberFilter
            label="Inhabitant Count"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.InhabitantCountActive"
            v-model:operator="localRules.InhabitantCountOperator"
            v-model:value="localRules.InhabitantCountValue"
        />
        <NumberFilter
            label="Copied Artifacts"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.CopiedArtifactCountActive"
            v-model:operator="localRules.CopiedArtifactCountOperator"
            v-model:value="localRules.CopiedArtifactCountValue"
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

const structureTypeOptions = [
  { title: 'All Types', value: '' },
  { title: 'Mead Hall', value: 'MeadHall' },
  { title: 'Market', value: 'Market' },
  { title: 'Keep', value: 'Keep' },
  { title: 'Temple', value: 'Temple' },
  { title: 'Dungeon', value: 'Dungeon' },
  { title: 'Tomb', value: 'Tomb' },
  { title: 'Inn / Tavern', value: 'InnTavern' },
  { title: 'Underworld Spire', value: 'UnderworldSpire' },
  { title: 'Library', value: 'Library' },
  { title: 'Tower', value: 'Tower' },
  { title: 'Counting House', value: 'CountingHouse' },
  { title: 'Guildhall', value: 'Guildhall' },
  { title: 'Hospital', value: 'Hospital' },
];

const structureSubTypeOptions = [
  { title: 'All Subtypes', value: '' },
  { title: 'Catacombs', value: 'Catacombs' },
  { title: 'Sewers', value: 'Sewers' },
  { title: 'Dungeon', value: 'Dungeon' },
];

const localRules = ref({
    StructureType: '',              // selected value or ""
    StructureSubType: '',           // selected value or ""
    HasInhabitants: '',             // "true", "false", or ""
    HasDeity: '',                   // "true", "false", or ""
    HasReligion: '',                // "true", "false", or ""
    HasEntity: '',                  // "true", "false", or ""
    HasCopiedArtifacts: '',         // "true", "false", or ""
    InhabitantCountActive: false,
    InhabitantCountOperator: null as FilterOperator | null,
    InhabitantCountValue: null as number | null,
    CopiedArtifactCountActive: false,
    CopiedArtifactCountOperator: null as FilterOperator | null,
    CopiedArtifactCountValue: null as number | null,
});

useFilterRules(
  props,
  localRules,
  (filters) => {
    localRules.value.StructureType = findRuleValue(filters, "StructureType") || findRuleValue(filters, "Type");
    localRules.value.StructureSubType = findRuleValue(filters, "StructureSubType") || findRuleValue(filters, "Subtype");
    localRules.value.HasInhabitants = findRuleValue(filters, "HasInhabitants");
    localRules.value.HasDeity = findRuleValue(filters, "HasDeity");
    localRules.value.HasReligion = findRuleValue(filters, "HasReligion");
    localRules.value.HasEntity = findRuleValue(filters, "HasEntity");
    localRules.value.HasCopiedArtifacts = findRuleValue(filters, "HasCopiedArtifacts");

    localRules.value.InhabitantCountActive = existsRule(filters, "InhabitantCount") || existsRule(filters, "InhabitantsCount");
    localRules.value.InhabitantCountOperator = findRuleOperator(filters, "InhabitantCount") || findRuleOperator(filters, "InhabitantsCount");
    localRules.value.InhabitantCountValue = findRuleNumberValue(filters, "InhabitantCount") ?? findRuleNumberValue(filters, "InhabitantsCount");

    localRules.value.CopiedArtifactCountActive = existsRule(filters, "CopiedArtifactCount") || existsRule(filters, "CopiedArtifactsCount");
    localRules.value.CopiedArtifactCountOperator = findRuleOperator(filters, "CopiedArtifactCount") || findRuleOperator(filters, "CopiedArtifactsCount");
    localRules.value.CopiedArtifactCountValue = findRuleNumberValue(filters, "CopiedArtifactCount") ?? findRuleNumberValue(filters, "CopiedArtifactsCount");
  },
  (filters) => {
    if (!localRules.value.StructureType) {
        removeRule(filters, "StructureType");
        removeRule(filters, "Type");
    } else {
        removeRule(filters, "Type");
        setRule(filters, "StructureType", "Equals", localRules.value.StructureType);
    }

    if (!localRules.value.StructureSubType) {
        removeRule(filters, "StructureSubType");
        removeRule(filters, "Subtype");
    } else {
        removeRule(filters, "Subtype");
        setRule(filters, "StructureSubType", "Equals", localRules.value.StructureSubType);
    }

    updateBoolRule(filters, localRules.value.HasInhabitants, "HasInhabitants");
    updateBoolRule(filters, localRules.value.HasDeity, "HasDeity");
    updateBoolRule(filters, localRules.value.HasReligion, "HasReligion");
    updateBoolRule(filters, localRules.value.HasEntity, "HasEntity");
    updateBoolRule(filters, localRules.value.HasCopiedArtifacts, "HasCopiedArtifacts");

    updateNumberRule(filters, localRules.value.InhabitantCountActive, localRules.value.InhabitantCountOperator, localRules.value.InhabitantCountValue, "InhabitantCount");
    updateNumberRule(filters, localRules.value.CopiedArtifactCountActive, localRules.value.CopiedArtifactCountOperator, localRules.value.CopiedArtifactCountValue, "CopiedArtifactCount");
  }
);
</script>
