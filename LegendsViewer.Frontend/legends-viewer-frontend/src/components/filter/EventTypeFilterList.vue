<template>
  <v-card variant="outlined" class="pa-4 mb-4">
    <v-row align="center" no-gutters class="mb-2">
      <v-col>
        <div class="text-subtitle-1 font-weight-bold">Filter Event Types</div>
        <div class="text-caption text-medium-emphasis">
          Uncheck event types to exclude them globally across all pages.
        </div>
      </v-col>
      <v-col cols="auto" class="d-flex gap-2">
        <v-btn
          size="small"
          variant="tonal"
          color="primary"
          @click="selectAll"
        >
          Select All
        </v-btn>
        <v-btn
          size="small"
          variant="tonal"
          color="warning"
          class="ml-2"
          @click="deselectAll"
        >
          Deselect All
        </v-btn>
      </v-col>
    </v-row>

    <v-text-field
      v-model="searchQuery"
      density="compact"
      placeholder="Search event types..."
      prepend-inner-icon="mdi-magnify"
      hide-details
      clearable
      class="mb-3"
    ></v-text-field>

    <v-sheet max-height="250" class="overflow-y-auto pr-2">
      <v-list density="compact" select-strategy="independent">
        <v-list-item
          v-for="item in filteredEventTypes"
          :key="item.typeKey"
          class="px-1 py-0"
        >
          <template #prepend>
            <v-checkbox-btn
              :model-value="!filterStore.excludedEventTypes.includes(item.typeKey)"
              color="primary"
              density="compact"
              @update:model-value="toggleType(item.typeKey)"
            />
          </template>

          <v-list-item-title class="text-body-2">
            {{ item.displayName }}
            <span class="text-caption text-disabled ml-1">({{ item.typeKey }})</span>
          </v-list-item-title>

          <template #append>
            <v-chip size="x-small" variant="outlined">
              {{ item.count }}
            </v-chip>
          </template>
        </v-list-item>
      </v-list>
    </v-sheet>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useEventFilterStore } from '../../stores/eventFilterStore';

const props = defineProps<{
  chartData: any | null;
}>();

const emit = defineEmits(['change']);
const filterStore = useEventFilterStore();
const searchQuery = ref('');

interface EventTypeItem {
  displayName: string;
  typeKey: string;
  count: number;
}

const parsedEventTypes = computed<EventTypeItem[]>(() => {
  if (!props.chartData || !props.chartData.labels || !props.chartData.datasets?.[0]?.data) {
    return [];
  }

  const labels: string[] = props.chartData.labels;
  const counts: number[] = props.chartData.datasets[0].data;

  return labels.map((label, index) => {
    // Label format from backend: "Human readable (raw_type)      123"
    const match = label.match(/^(.*?)\s+\((.*?)\)\s+(\d+)$/);
    if (match) {
      return {
        displayName: match[1].trim(),
        typeKey: match[2].trim(),
        count: parseInt(match[3], 10),
      };
    }
    // Fallback parsing if regex doesn't match exactly
    return {
      displayName: label.trim(),
      typeKey: label.trim(),
      count: counts[index] ?? 0,
    };
  });
});

const filteredEventTypes = computed(() => {
  if (!searchQuery.value) {
    return parsedEventTypes.value;
  }
  const q = searchQuery.value.toLowerCase();
  return parsedEventTypes.value.filter(
    (item) =>
      item.displayName.toLowerCase().includes(q) ||
      item.typeKey.toLowerCase().includes(q)
  );
});

const toggleType = (typeKey: string) => {
  filterStore.toggleEventType(typeKey);
  emit('change');
};

const selectAll = () => {
  const typesInView = filteredEventTypes.value.map((i) => i.typeKey);
  filterStore.includeEventTypes(typesInView);
  emit('change');
};

const deselectAll = () => {
  const typesInView = filteredEventTypes.value.map((i) => i.typeKey);
  filterStore.excludeEventTypes(typesInView);
  emit('change');
};
</script>

<style scoped>
</style>
