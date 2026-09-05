<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <SelectFilter label="Content Type" :items="contentTypeOptions" v-model="localRules.WrittenContentType" />

        <v-divider class="mt-3 mb-3"/>

        <ThreeStateBoolFilter label="Has Styles" v-model="localRules.HasStyles" />
        <ThreeStateBoolFilter label="Has References" v-model="localRules.HasReferences" />
        <ThreeStateBoolFilter label="Has Physical Artifact" v-model="localRules.HasArtifact" />

        <v-divider class="mt-3 mb-3"/>

        <NumberFilter
            label="Page Count"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.PageCountActive"
            v-model:operator="localRules.PageCountOperator"
            v-model:value="localRules.PageCountValue"
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

const contentTypeOptions = [
  { title: 'All Types', value: '' },
  { title: 'Autobiography', value: 'Autobiography' },
  { title: 'Biography', value: 'Biography' },
  { title: 'Chronicle', value: 'Chronicle' },
  { title: 'Dialog', value: 'Dialog' },
  { title: 'Essay', value: 'Essay' },
  { title: 'Guide', value: 'Guide' },
  { title: 'Letter', value: 'Letter' },
  { title: 'Manual', value: 'Manual' },
  { title: 'Novel', value: 'Novel' },
  { title: 'Play', value: 'Play' },
  { title: 'Poem', value: 'Poem' },
  { title: 'Short Story', value: 'ShortStory' },
  { title: 'Choreography', value: 'Choreography' },
  { title: 'Musical Composition', value: 'MusicalComposition' },
  { title: 'Star Chart', value: 'StarChart' },
  { title: 'Cultural History', value: 'CulturalHistory' },
  { title: 'Comparative Biography', value: 'ComparativeBiography' },
  { title: 'Cultural Comparison', value: 'CulturalComparison' },
  { title: 'Atlas', value: 'Atlas' },
  { title: 'Treatise On Technological Evolution', value: 'TreatiseOnTechnologicalEvolution' },
  { title: 'Alternate History', value: 'AlternateHistory' },
  { title: 'Star Catalogue', value: 'StarCatalogue' },
  { title: 'Dictionary', value: 'Dictionary' },
  { title: 'Genealogy', value: 'Genealogy' },
  { title: 'Encyclopedia', value: 'Encyclopedia' },
  { title: 'Biographical Dictionary', value: 'BiographicalDictionary' },
];

const localRules = ref({
    WrittenContentType: '',           // selected value or ""
    HasStyles: '',                    // "true", "false", or ""
    HasReferences: '',                // "true", "false", or ""
    HasArtifact: '',                  // "true", "false", or ""
    PageCountActive: false,
    PageCountOperator: null as FilterOperator | null,
    PageCountValue: null as number | null,
});

useFilterRules(
  props,
  localRules,
  (filters) => {
    localRules.value.WrittenContentType = findRuleValue(filters, "WrittenContentType") || findRuleValue(filters, "Type") || findRuleValue(filters, "Form");
    localRules.value.HasStyles = findRuleValue(filters, "HasStyles");
    localRules.value.HasReferences = findRuleValue(filters, "HasReferences");
    localRules.value.HasArtifact = findRuleValue(filters, "HasArtifact");

    localRules.value.PageCountActive = existsRule(filters, "PageCount") || existsRule(filters, "Pages");
    localRules.value.PageCountOperator = findRuleOperator(filters, "PageCount") || findRuleOperator(filters, "Pages");
    localRules.value.PageCountValue = findRuleNumberValue(filters, "PageCount") ?? findRuleNumberValue(filters, "Pages");
  },
  (filters) => {
    if (!localRules.value.WrittenContentType) {
        removeRule(filters, "WrittenContentType");
        removeRule(filters, "Type");
        removeRule(filters, "Form");
    } else {
        removeRule(filters, "Type");
        removeRule(filters, "Form");
        setRule(filters, "WrittenContentType", "Equals", localRules.value.WrittenContentType);
    }

    updateBoolRule(filters, localRules.value.HasStyles, "HasStyles");
    updateBoolRule(filters, localRules.value.HasReferences, "HasReferences");
    updateBoolRule(filters, localRules.value.HasArtifact, "HasArtifact");

    updateNumberRule(filters, localRules.value.PageCountActive, localRules.value.PageCountOperator, localRules.value.PageCountValue, "PageCount");
  }
);
</script>

<style scoped></style>
