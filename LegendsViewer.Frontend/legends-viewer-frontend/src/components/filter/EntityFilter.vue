<template>
    <v-list style="background-color: rgb(var(--v-theme-background));">
        <ThreeStateBoolFilter label="Is Civ" v-model="localRules.IsCiv" />
        <ThreeStateBoolFilter label="Has Sites" v-model="localRules.HasCurrentSites" />
        <ThreeStateBoolFilter label="Worships Deity" v-model="localRules.WorshipsDeity" />
        
        <v-list-item class="mt-1 mb-1">
            <div style="float: left; margin-top: 8px;">Type</div>
            <div style="float: right;">
                <v-select
                    density="compact"
                    hide-details
                    label=""
                    :items="entityTypeOptions"
                    item-title="title"
                    item-value="value"
                    v-model="localRules.EntityType"
                    width="220"
                ></v-select>
            </div>
        </v-list-item>

        <v-divider class="mt-3 mb-3"/>

        <NumberFilter
            label="Current Sites"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.CurrentSitesCountActive"
            v-model:operator="localRules.CurrentSitesCountOperator"
            v-model:value="localRules.CurrentSitesCountValue"
        />
        <NumberFilter
            label="Wars"
            :operators="['Equals', 'NotEquals', 'GreaterThan', 'LessThan']"
            v-model:active="localRules.WarCountActive"
            v-model:operator="localRules.WarCountOperator"
            v-model:value="localRules.WarCountValue"
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

const entityTypeOptions = [
  { title: 'All Types', value: '' },
  { title: 'Civilization', value: 'Civilization' },
  { title: 'Site Government', value: 'SiteGovernment' },
  { title: 'Religion', value: 'Religion' },
  { title: 'Guild', value: 'Guild' },
  { title: 'Military Unit', value: 'MilitaryUnit' },
  { title: 'Mercenary Company', value: 'MercenaryCompany' },
  { title: 'Performance Troupe', value: 'PerformanceTroupe' },
  { title: 'Merchant Company', value: 'MerchantCompany' },
  { title: 'Outcast', value: 'Outcast' },
  { title: 'Nomadic Group', value: 'NomadicGroup' },
  { title: 'Migrating Group', value: 'MigratingGroup' },
];

const localRules = ref({
    IsCiv: '',               // "true", "false", or ""
    HasCurrentSites: '',     // "true", "false", or ""
    WorshipsDeity: '',       // "true", "false", or ""
    EntityType: '',          // selected value or ""
    CurrentSitesCountActive: false,
    CurrentSitesCountOperator: null as FilterOperator | null,
    CurrentSitesCountValue: null as number | null,
    WarCountActive: false,
    WarCountOperator: null as FilterOperator | null,
    WarCountValue: null as number | null,
});

let isUpdatingFromProps = false;

watch(() => props.filters, (filters) => {
    if (!filters) return;
    isUpdatingFromProps = true;

    localRules.value.IsCiv = findRuleValue(filters, "IsCiv");
    localRules.value.HasCurrentSites = findRuleValue(filters, "HasCurrentSites");
    localRules.value.WorshipsDeity = findRuleValue(filters, "WorshipsDeity");
    localRules.value.EntityType = findRuleValue(filters, "EntityType");

    localRules.value.CurrentSitesCountActive = existsRule(filters, "CurrentSitesCount");
    localRules.value.CurrentSitesCountOperator = findRuleOperator(filters, "CurrentSitesCount");
    localRules.value.CurrentSitesCountValue = findRuleNumberValue(filters, "CurrentSitesCount");

    localRules.value.WarCountActive = existsRule(filters, "WarCount");
    localRules.value.WarCountOperator = findRuleOperator(filters, "WarCount");
    localRules.value.WarCountValue = findRuleNumberValue(filters, "WarCount");

    isUpdatingFromProps = false;
}, { immediate: true, deep: true });

watch(localRules, () => {
    if (isUpdatingFromProps || !props.filters) return;
    const filters = props.filters;

    // Boolean rules
    updateBoolRule(filters, localRules.value.IsCiv, "IsCiv");
    updateBoolRule(filters, localRules.value.HasCurrentSites, "HasCurrentSites");
    updateBoolRule(filters, localRules.value.WorshipsDeity, "WorshipsDeity");

    // EntityType rule
    if (!localRules.value.EntityType) {
        removeRule(filters, "EntityType");
    } else {
        setRule(filters, "EntityType", "Equals", localRules.value.EntityType);
    }

    // Number rules
    updateNumberRule(filters, localRules.value.CurrentSitesCountActive, localRules.value.CurrentSitesCountOperator, localRules.value.CurrentSitesCountValue, "CurrentSitesCount");
    updateNumberRule(filters, localRules.value.WarCountActive, localRules.value.WarCountOperator, localRules.value.WarCountValue, "WarCount");
}, { deep: true });
</script>
