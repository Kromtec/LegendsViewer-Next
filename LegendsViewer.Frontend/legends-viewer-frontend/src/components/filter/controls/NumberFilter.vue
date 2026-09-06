<template>
  <div class="d-flex align-center justify-space-between px-4 py-1" style="width: 100%;">
    <div class="d-flex align-center flex-grow-1 pr-2 overflow-hidden">
      <v-checkbox
        hide-details
        density="compact"
        :label="label"
        v-model="proxyActive"
        class="ma-0 pa-0 text-truncate"
      ></v-checkbox>
    </div>
    <div class="d-flex align-center ga-2 flex-shrink-0">
      <v-select
        density="compact"
        hide-details
        label=""
        :items="operators"
        v-model="proxyOperator"
        width="110"
      ></v-select>
      <v-number-input
        density="compact"
        hide-details
        :reverse="false"
        controlVariant="stacked"
        label=""
        :hideInput="false"
        :inset="true"
        width="100"
        v-model="proxyValue"
      ></v-number-input>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  label: string;
  operators: string[];
  active: boolean;
  operator: string | null;
  value: number | null;
}>();
const emit = defineEmits(['update:active', 'update:operator', 'update:value']);

const proxyActive = computed({
  get: () => props.active,
  set: (val: boolean) => emit('update:active', val)
});
const proxyOperator = computed({
  get: () => props.operator,
  set: (val: string | null) => emit('update:operator', val)
});
const proxyValue = computed({
  get: () => props.value,
  set: (val: number | null) => emit('update:value', val)
});
</script>