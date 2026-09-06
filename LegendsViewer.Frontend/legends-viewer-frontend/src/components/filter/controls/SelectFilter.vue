<template>
  <div class="d-flex align-center justify-space-between px-4 py-2" style="width: 100%;">
    <div class="text-body-2 pr-2 text-truncate">{{ label }}</div>
    <v-select
      density="compact"
      hide-details
      label=""
      :items="items"
      item-title="title"
      item-value="value"
      v-model="proxyValue"
      :width="width || 220"
      class="flex-shrink-0"
    ></v-select>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

export interface SelectOption {
  title: string;
  value: string;
}

const props = defineProps<{
  label: string;
  items: SelectOption[] | string[];
  modelValue: string;
  width?: string | number;
}>();

const emit = defineEmits(['update:modelValue']);

const proxyValue = computed({
  get: () => props.modelValue,
  set: (val: string) => emit('update:modelValue', val)
});
</script>
