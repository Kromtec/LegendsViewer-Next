<template>
  <v-card :title="card.title" :subtitle="card.metricName ? 'Ranked by ' + card.metricName : ''" variant="text">
    <template v-slot:prepend>
      <v-icon class="mr-2" :icon="card.icon || 'mdi-trophy-outline'" size="32px"></v-icon>
    </template>
    <v-card-text>
      <v-list v-if="card?.entries && card.entries.length > 0" class="ml-12" lines="two">
        <template v-for="entry in card.entries" :key="entry.id + '-' + entry.rank">
          <v-list-item class="px-2">
            <template v-slot:prepend>
              <v-chip
                :color="getRankColor(entry.rank)"
                label
                size="small"
                class="mr-3 font-weight-bold flex-shrink-0"
                style="min-width: 36px; justify-content: center;"
              >
                #{{ entry.rank }}
              </v-chip>
            </template>

            <v-list-item-title class="text-truncate">
              <div v-if="entry.linkHtml" v-html="entry.linkHtml" class="text-truncate"></div>
              <div v-else class="text-truncate">{{ entry.name }}</div>
            </v-list-item-title>

            <v-list-item-subtitle v-if="entry.subtitle || entry.detailText">
              <div class="d-flex flex-column text-truncate">
                <span v-if="entry.subtitle" class="text-truncate">{{ entry.subtitle }}</span>
                <span v-if="entry.detailText" class="text-caption text-medium-emphasis text-truncate">{{ entry.detailText }}</span>
              </div>
            </v-list-item-subtitle>

            <template v-slot:append>
              <v-chip color="primary" label size="small" class="ml-3 flex-shrink-0">
                {{ entry.value }}
              </v-chip>
            </template>
          </v-list-item>
        </template>
      </v-list>
      <div v-else class="ml-12 my-4 text-caption text-medium-emphasis">
        No records found.
      </div>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { RecordCard } from '../stores/worldRecordsStore';

defineProps<{
  card: RecordCard;
}>();

const getRankColor = (rank: number) => {
  switch (rank) {
    case 1: return 'amber';
    case 2: return 'blue-grey-lighten-2';
    case 3: return 'deep-orange-lighten-1';
    default: return 'cyan';
  }
};
</script>

<style scoped>
:deep(.v-list-item-title),
:deep(.v-list-item-subtitle) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
:deep(.v-list-item-title *) {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  display: inline;
}
</style>
