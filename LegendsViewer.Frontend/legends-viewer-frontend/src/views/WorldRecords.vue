<template>
  <div>
    <!-- Page Header -->
    <v-row>
      <v-col cols="12">
        <v-card variant="text">
          <v-row align="center" no-gutters>
            <v-col class="large-icon" cols="auto">
              <v-icon icon="mdi-trophy-outline" />
            </v-col>
            <v-col>
              <v-card-title>World Records</v-card-title>
              <v-card-subtitle class="multiline-subtitle">
                The greatest legends, darkest tragedies, and extraordinary achievements of {{ worldStore.world?.name || 'the world' }}
              </v-card-subtitle>
            </v-col>
          </v-row>
        </v-card>
      </v-col>
    </v-row>

    <!-- Top Hall of Fame Highlights Row -->
    <v-row v-if="recordsStore.records?.highlights && recordsStore.records.highlights.length > 0" class="mb-2">
      <v-col
        v-for="(highlight, i) in recordsStore.records.highlights"
        :key="i"
        cols="12"
        sm="6"
        md="3"
      >
        <v-card
          variant="tonal"
          class="h-100"
        >
          <v-card-text class="d-flex align-start py-3 px-4">
            <v-icon :icon="highlight.icon" size="32px" color="amber" class="mr-3 flex-shrink-0 mt-1"></v-icon>
            
            <div class="flex-grow-1 min-width-0">
              <div class="text-caption text-uppercase font-weight-bold tracking-wider text-medium-emphasis mb-1">
                {{ highlight.title }}
              </div>
              <div class="text-subtitle-2 font-weight-bold multiline-subtitle">
                <span v-if="highlight.linkHtml" v-html="highlight.linkHtml"></span>
                <span v-else>{{ highlight.value }}</span>
              </div>
            </div>

            <v-chip v-if="highlight.value" color="primary" label size="small" class="ml-3 flex-shrink-0 mt-1">
              {{ highlight.value }}
            </v-chip>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Category Tabs Header -->
    <v-row align="center" class="mt-2 mb-4">
      <v-col cols="12">
        <v-tabs v-model="activeTab" color="primary">
          <v-tab
            v-for="cat in recordsStore.records?.categories"
            :key="cat.id"
            :value="cat.id"
          >
            <v-icon :icon="cat.icon" class="mr-2"></v-icon>
            {{ cat.title }}
          </v-tab>
        </v-tabs>
      </v-col>
    </v-row>

    <!-- Loading State -->
    <v-row v-if="recordsStore.isLoading && !recordsStore.records">
      <v-col cols="12" class="text-center py-12">
        <v-progress-circular indeterminate color="primary" size="64" width="6"></v-progress-circular>
        <div class="mt-4 text-subtitle-1 text-medium-emphasis">Calculating World Records...</div>
      </v-col>
    </v-row>

    <!-- Category Content -->
    <template v-else-if="recordsStore.records">
      <v-window v-model="activeTab">
        <v-window-item
          v-for="cat in recordsStore.records.categories"
          :key="cat.id"
          :value="cat.id"
        >
          <v-row>
            <v-col
              v-for="card in cat.cards"
              :key="card.id"
              cols="12"
              xl="6"
              lg="12"
            >
              <RecordCard :card="card" />
            </v-col>
          </v-row>
        </v-window-item>
      </v-window>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useWorldRecordsStore } from '../stores/worldRecordsStore';
import { useWorldStore } from '../stores/worldStore';
import RecordCard from '../components/RecordCard.vue';

const recordsStore = useWorldRecordsStore();
const worldStore = useWorldStore();

const activeTab = ref('warfare');

onMounted(async () => {
  if (!worldStore.world) {
    await worldStore.loadWorld();
  }
  await recordsStore.loadRecords();
});
</script>

<style scoped>
.multiline-subtitle {
  white-space: normal;
}
</style>
