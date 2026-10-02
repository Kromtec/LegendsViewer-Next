<template>
  <v-card :title="title" variant="text">
    <template #prepend>
      <v-icon class="mr-2" :icon="icon" size="32px" />
    </template>
    <v-card-text class="ml-12">
      <v-data-table
        :headers="headers"
        :items="items"
        density="compact"
        :items-per-page="itemsPerPage"
        class="card-table"
      />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { TableHeader } from '../types/legends';

withDefaults(
  defineProps<{
    title: string;
    items: any[];
    icon?: string;
    itemsPerPage?: number;
    headers?: TableHeader[];
  }>(),
  {
    icon: 'mdi-account-group',
    itemsPerPage: 5,
    headers: () => [
      { title: 'Race', key: 'race.namePlural' },
      { title: 'Count', key: 'count', align: 'end' },
    ],
  }
);
</script>

<style scoped>
:deep(.card-table th),
:deep(.card-table .v-data-table-header__content) {
  font-weight: 700 !important;
}
</style>
