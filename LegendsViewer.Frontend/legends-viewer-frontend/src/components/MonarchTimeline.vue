<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import ExpandableCard from './ExpandableCard.vue';
import { LeaderTimelineDto, LeaderTimelineItemDto } from '../types/legends';

const props = withDefaults(defineProps<{
  entityId: number;
  isCiv?: boolean;
  mode?: 'primaryOnly' | 'otherPositions';
}>(), {
  isCiv: false,
  mode: 'primaryOnly'
});

const timelines = ref<LeaderTimelineDto[]>([]);
const selectedPosition = ref<string>('');
const isLoading = ref<boolean>(false);
const fetchError = ref<string | null>(null);

// Determine which timelines to expose for this card mode
const filteredTimelines = computed<LeaderTimelineDto[]>(() => {
  if (props.mode === 'primaryOnly') {
    return props.isCiv && timelines.value.length > 0 ? [timelines.value[0]] : [];
  } else {
    // 'otherPositions' mode:
    // If it's a Civ, show secondary positions (index 1 onwards).
    // If non-Civ entity (Religion, Guild, etc.), show all available positions.
    return props.isCiv ? timelines.value.slice(1) : timelines.value;
  }
});

const availablePositions = computed(() => filteredTimelines.value.map(t => t.leaderType));

const activeTimeline = computed<LeaderTimelineItemDto[]>(() => {
  if (props.mode === 'primaryOnly') {
    return filteredTimelines.value[0]?.leaders ?? [];
  }
  if (!selectedPosition.value && availablePositions.value.length > 0) {
    return filteredTimelines.value[0]?.leaders ?? [];
  }
  const found = filteredTimelines.value.find(t => t.leaderType === selectedPosition.value);
  return found ? found.leaders : [];
});

const cardTitle = computed(() => {
  if (props.mode === 'primaryOnly') {
    return 'Monarch Line';
  }
  return 'Position Timelines';
});

const cardSubtitle = computed(() => {
  if (props.mode === 'primaryOnly') {
    return 'Chronological lineage of primary rulers';
  }
  return 'Chronological lineage of noble position holders';
});

const cardIcon = computed(() => {
  return props.mode === 'primaryOnly' ? 'mdi-crown' : 'mdi-seal-variant';
});

const loadMonarchTimelines = async (id: number) => {
  if (!id) return;
  isLoading.value = true;
  fetchError.value = null;
  try {
    const response = await fetch(`http://localhost:15421/api/Entity/${id}/monarchs`);
    if (!response.ok) {
      fetchError.value = `Failed to load monarch timeline (HTTP ${response.status})`;
      timelines.value = [];
      return;
    }
    const data: LeaderTimelineDto[] = await response.json();
    timelines.value = data || [];
    if (filteredTimelines.value.length > 0) {
      selectedPosition.value = filteredTimelines.value[0].leaderType;
    } else {
      selectedPosition.value = '';
    }
  } catch (err) {
    console.error('Error fetching monarch timeline:', err);
    fetchError.value = 'Network error fetching monarch timeline';
  } finally {
    isLoading.value = false;
  }
};

watch([() => props.entityId, availablePositions], () => {
  if (availablePositions.value.length > 0 && !availablePositions.value.includes(selectedPosition.value)) {
    selectedPosition.value = availablePositions.value[0];
  }
});

watch(() => props.entityId, (newId) => {
  if (newId) {
    void loadMonarchTimelines(newId);
  }
}, { immediate: true });

onMounted(() => {
  if (props.entityId) {
    void loadMonarchTimelines(props.entityId);
  }
});
</script>

<template>
  <v-col v-if="filteredTimelines.length > 0" cols="12" xl="4" lg="6" md="12">
    <ExpandableCard
      :title="cardTitle"
      :subtitle="cardSubtitle"
      :icon="cardIcon"
    >
      <template #compact-content>
        <div class="monarch-timeline-container">
          <div v-if="mode === 'otherPositions' && availablePositions.length > 1" class="position-select-wrapper mb-3">
            <v-select
              v-model="selectedPosition"
              :items="availablePositions"
              label="Noble Position"
              density="compact"
              variant="outlined"
              hide-details
              style="max-width: 260px;"
              prepend-inner-icon="mdi-seal"
            />
          </div>

          <div class="timeline-scroll-container">
            <v-timeline align="start" density="compact" class="monarch-timeline">
              <v-timeline-item
                v-for="item in activeTimeline"
                :key="item.id + '-' + item.startYear"
                :dot-color="item.endYear === null ? 'amber-darken-2' : 'blue-grey-darken-2'"
                :icon="item.endYear === null ? 'mdi-crown' : 'mdi-crown-outline'"
                size="x-small"
              >
                <v-card variant="outlined" class="pa-2 px-3 rounded-lg border-opacity-25 elevation-1 bubble-card">
                  <!-- Row 1: Character Name (left, truncates) & Current Status Badge (right) -->
                  <div class="d-flex align-center justify-space-between ga-2">
                    <div class="d-flex align-center min-w-0 flex-grow-1 text-truncate me-2">
                      <span class="text-subtitle-2 font-weight-bold text-truncate d-inline-flex align-center mw-100" v-html="item.link"></span>
                    </div>
                    <div v-if="item.endYear === null" class="d-flex align-center flex-shrink-0">
                      <v-chip
                        size="x-small"
                        color="success"
                        variant="flat"
                        class="font-weight-bold px-2 text-caption my-0"
                      >
                        Current
                      </v-chip>
                    </div>
                  </div>

                  <!-- Row 2: Position Title Chip + Timeline Period (left) & Reign Duration (right) -->
                  <div class="d-flex align-center justify-space-between ga-2 my-1 text-caption">
                    <div class="d-flex align-center ga-2 min-w-0 text-truncate me-2">
                      <v-chip
                        size="x-small"
                        color="amber-darken-4"
                        variant="flat"
                        class="font-weight-medium text-caption flex-shrink-0"
                      >
                        {{ item.positionTitle }}
                      </v-chip>
                      <div class="font-weight-medium text-medium-emphasis text-truncate">
                        <v-icon size="x-small" class="mr-1 text-medium-emphasis">mdi-calendar-clock</v-icon>
                        {{ item.startYearDisplay }} – {{ item.endYearDisplay }}
                      </div>
                    </div>
                    <div class="text-medium-emphasis font-weight-medium text-no-wrap flex-shrink-0 ms-auto">
                      {{ item.reignDuration }}
                    </div>
                  </div>

                  <!-- Row 3: Predecessor Lineage (left) & Lifespan / Death Cause (right) -->
                  <div class="d-flex align-center justify-space-between ga-2 text-caption text-medium-emphasis">
                    <div class="text-truncate me-2">
                      <span class="font-weight-medium">{{ item.predecessorRelation }}</span>
                    </div>

                    <div class="d-flex align-center ga-2 text-no-wrap flex-shrink-0 ms-auto">
                      <span v-if="item.birthYear > -1 || item.deathYear > -1">
                        b. {{ item.birthYear > -1 ? item.birthYear : '?' }}
                        <template v-if="!item.isAlive"> · d. {{ item.deathYear > -1 ? item.deathYear : '?' }}</template>
                      </span>
                      <span v-if="item.deathCause" class="text-error font-weight-medium">
                        ({{ item.deathCause }})
                      </span>
                    </div>
                  </div>
                </v-card>
              </v-timeline-item>
            </v-timeline>
          </div>
        </div>
      </template>

      <template #expanded-content>
        <div class="monarch-timeline-container pa-2">
          <div v-if="mode === 'otherPositions' && availablePositions.length > 1" class="position-select-wrapper mb-4">
            <v-select
              v-model="selectedPosition"
              :items="availablePositions"
              label="Noble Position"
              density="compact"
              variant="outlined"
              hide-details
              style="max-width: 280px;"
              prepend-inner-icon="mdi-seal"
            />
          </div>

          <div class="timeline-scroll-container-expanded">
            <v-timeline align="start" density="comfortable" class="monarch-timeline-full">
              <v-timeline-item
                v-for="item in activeTimeline"
                :key="'full-' + item.id + '-' + item.startYear"
                :dot-color="item.endYear === null ? 'amber-darken-2' : 'blue-grey-darken-2'"
                :icon="item.endYear === null ? 'mdi-crown' : 'mdi-crown-outline'"
                size="small"
              >
                <v-card variant="outlined" class="pa-3 rounded-lg elevation-2 bubble-card">
                  <!-- Row 1: Character Name (left, truncates) & Current Status Badge (right) -->
                  <div class="d-flex align-center justify-space-between ga-2 mb-1">
                    <div class="d-flex align-center min-w-0 flex-grow-1 text-truncate me-2">
                      <span class="text-subtitle-1 font-weight-bold text-truncate d-inline-flex align-center mw-100" v-html="item.link"></span>
                    </div>
                    <div v-if="item.endYear === null" class="d-flex align-center flex-shrink-0">
                      <v-chip
                        size="small"
                        color="success"
                        variant="flat"
                        class="font-weight-bold my-0"
                      >
                        Current
                      </v-chip>
                    </div>
                  </div>

                  <!-- Row 2: Position Title Chip + Timeline Period (left) & Reign Duration (right) -->
                  <div class="d-flex align-center justify-space-between ga-2 my-2 text-body-2">
                    <div class="d-flex align-center ga-2 min-w-0 text-truncate me-2">
                      <v-chip
                        size="small"
                        color="amber-darken-4"
                        variant="flat"
                        class="font-weight-medium flex-shrink-0"
                      >
                        {{ item.positionTitle }}
                      </v-chip>
                      <div class="font-weight-medium text-medium-emphasis text-truncate">
                        <v-icon size="small" class="mr-1 text-medium-emphasis">mdi-calendar-clock</v-icon>
                        {{ item.startYearDisplay }} – {{ item.endYearDisplay }}
                      </div>
                    </div>
                    <div class="text-medium-emphasis font-weight-medium text-no-wrap flex-shrink-0 ms-auto">
                      Reign: {{ item.reignDuration }}
                    </div>
                  </div>

                  <!-- Row 3: Predecessor Lineage (left) & Lifespan / Death Cause (right) -->
                  <div class="d-flex align-center justify-space-between ga-3 text-body-2 text-medium-emphasis">
                    <div class="text-truncate me-2">
                      <span class="font-weight-medium">{{ item.predecessorRelation }}</span>
                    </div>

                    <div class="d-flex align-center ga-3 text-no-wrap flex-shrink-0 ms-auto">
                      <span v-if="item.birthYear > -1 || item.deathYear > -1">
                        b. {{ item.birthYear > -1 ? item.birthYear : '?' }}
                        <template v-if="!item.isAlive"> · d. {{ item.deathYear > -1 ? item.deathYear : '?' }}</template>
                      </span>
                      <span v-if="item.deathCause" class="text-error font-weight-medium">
                        ({{ item.deathCause }})
                      </span>
                    </div>
                  </div>
                </v-card>
              </v-timeline-item>
            </v-timeline>
          </div>
        </div>
      </template>
    </ExpandableCard>
  </v-col>
</template>

<style scoped>
.monarch-timeline-container {
  width: 100%;
  overflow-x: hidden;
}
.position-select-wrapper {
  padding-left: 46px;
  margin-top: 10px;
}
.timeline-scroll-container {
  max-height: 480px;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 8px;
}
.timeline-scroll-container-expanded {
  max-height: 700px;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 12px;
}
.monarch-timeline :deep(.v-timeline-item__body) {
  padding-block-start: 2px;
  padding-block-end: 8px;
  max-width: 100%;
  overflow-x: hidden;
}
.bubble-card {
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  transition: background-color 0.15s ease-in-out, border-color 0.15s ease-in-out;
}
.bubble-card:hover {
  background-color: rgba(255, 255, 255, 0.03);
}
.mw-100 {
  max-width: 100%;
}
.bubble-card :deep(a) {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  vertical-align: middle;
  line-height: 1.2;
}
</style>
