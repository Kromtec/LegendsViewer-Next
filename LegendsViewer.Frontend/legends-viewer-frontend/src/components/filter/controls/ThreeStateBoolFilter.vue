<template>
  <div class="d-flex align-center justify-space-between px-4 py-2" style="width: 100%;">
    <div class="text-body-2 pr-2 text-truncate">{{ label }}</div>
    <v-btn-toggle
      density="compact"
      v-model="proxyValue"
      divided
      class="flex-shrink-0"
    >
      <v-btn value="true" size="small"><v-icon>mdi-check</v-icon></v-btn>
      <v-btn value="false" size="small"><v-icon>mdi-close</v-icon></v-btn>
    </v-btn-toggle>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{ label: string; modelValue: string }>();
const emit = defineEmits(['update:modelValue']);

const proxyValue = computed({
  get: () => props.modelValue,
  set: (val: string) => emit('update:modelValue', val)
});
</script>