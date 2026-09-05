<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <ThreeStateBoolFilter label="Is Held" v-model="localRules.IsHeld" />
        <ThreeStateBoolFilter label="Is Written Content" v-model="localRules.IsWrittenContent" />
        <ThreeStateBoolFilter label="Located in Site" v-model="localRules.IsLocatedInSite" />

        <SelectFilter label="Artifact Type" :items="artifactTypeOptions" v-model="localRules.Type" />

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

const artifactTypeOptions = [
  { title: 'All Types', value: '' },
  { title: 'Book', value: 'Book' },
  { title: 'Weapon', value: 'Weapon' },
  { title: 'Armor', value: 'Armor' },
  { title: 'Shield', value: 'Shield' },
  { title: 'Helm', value: 'Helm' },
  { title: 'Gloves', value: 'Gloves' },
  { title: 'Shoes', value: 'Shoes' },
  { title: 'Pants', value: 'Pants' },
  { title: 'Slab', value: 'Slab' },
  { title: 'Toy', value: 'Toy' },
  { title: 'Tool', value: 'Tool' },
  { title: 'Instrument', value: 'Instrument' },
  { title: 'Figurine', value: 'Figurine' },
  { title: 'Ring', value: 'Ring' },
  { title: 'Amulet', value: 'Amulet' },
  { title: 'Scepter', value: 'Scepter' },
  { title: 'Crown', value: 'Crown' },
  { title: 'Goblet', value: 'Goblet' },
  { title: 'Box', value: 'Box' },
];

const localRules = ref({
    IsHeld: '',               // "true", "false", or ""
    IsWrittenContent: '',     // "true", "false", or ""
    IsLocatedInSite: '',      // "true", "false", or ""
    Type: '',                 // selected value or ""
    PageCountActive: false,
    PageCountOperator: null as FilterOperator | null,
    PageCountValue: null as number | null,
});

useFilterRules(
  props,
  localRules,
  (filters) => {
    localRules.value.IsHeld = findRuleValue(filters, "IsHeld");
    localRules.value.IsWrittenContent = findRuleValue(filters, "IsWrittenContent");
    localRules.value.IsLocatedInSite = findRuleValue(filters, "IsLocatedInSite");
    localRules.value.Type = findRuleValue(filters, "Type") || findRuleValue(filters, "ArtifactType");

    localRules.value.PageCountActive = existsRule(filters, "PageCount");
    localRules.value.PageCountOperator = findRuleOperator(filters, "PageCount");
    localRules.value.PageCountValue = findRuleNumberValue(filters, "PageCount");
  },
  (filters) => {
    updateBoolRule(filters, localRules.value.IsHeld, "IsHeld");
    updateBoolRule(filters, localRules.value.IsWrittenContent, "IsWrittenContent");
    updateBoolRule(filters, localRules.value.IsLocatedInSite, "IsLocatedInSite");

    if (!localRules.value.Type) {
        removeRule(filters, "Type");
        removeRule(filters, "ArtifactType");
    } else {
        removeRule(filters, "ArtifactType");
        setRule(filters, "Type", "Equals", localRules.value.Type);
    }

    updateNumberRule(filters, localRules.value.PageCountActive, localRules.value.PageCountOperator, localRules.value.PageCountValue, "PageCount");
  }
);
</script>
