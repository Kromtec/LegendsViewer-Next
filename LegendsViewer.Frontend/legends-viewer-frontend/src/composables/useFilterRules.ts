import { watch, type Ref } from 'vue';
import type { FilterRuleDto } from '../stores/worldObjectStores';

/**
 * Composable to synchronize props.filters with a local reactive rules object,
 * preventing reactive watcher feedback loops.
 */
export function useFilterRules<T extends object>(
  props: { filters: FilterRuleDto[] | null },
  localRules: Ref<T>,
  toLocal: (filters: FilterRuleDto[]) => void,
  fromLocal: (filters: FilterRuleDto[]) => void
) {
  let isUpdatingFromProps = false;

  watch(
    () => props.filters,
    (filters) => {
      if (!filters) return;
      isUpdatingFromProps = true;
      toLocal(filters);
      isUpdatingFromProps = false;
    },
    { immediate: true, deep: true }
  );

  watch(
    localRules,
    () => {
      if (isUpdatingFromProps || !props.filters) return;
      fromLocal(props.filters);
    },
    { deep: true }
  );
}
