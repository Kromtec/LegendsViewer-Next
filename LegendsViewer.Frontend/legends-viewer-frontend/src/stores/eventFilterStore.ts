import { defineStore } from 'pinia';
import { components } from '../generated/api-schema';

export type EventFilterDto = components['schemas']['EventFilterDto'];

const STORAGE_KEY = 'legends_viewer_excluded_event_types';

function loadStoredFilters(): string[] {
    try {
        const stored = localStorage.getItem(STORAGE_KEY);
        return stored ? JSON.parse(stored) : [];
    } catch {
        return [];
    }
}

function saveStoredFilters(types: string[]) {
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(types));
    } catch (e) {
        console.error('Failed to save event filters to localStorage', e);
    }
}

export const useEventFilterStore = defineStore('eventFilter', {
    state: () => ({
        excludedEventTypes: loadStoredFilters(),
    }),
    getters: {
        isFiltered: (state): boolean => state.excludedEventTypes.length > 0,
        filterDto: (state): EventFilterDto => ({
            excludedEventTypes: [...state.excludedEventTypes]
        })
    },
    actions: {
        toggleEventType(eventType: string) {
            const index = this.excludedEventTypes.indexOf(eventType);
            if (index > -1) {
                this.excludedEventTypes.splice(index, 1);
            } else {
                this.excludedEventTypes.push(eventType);
            }
            saveStoredFilters(this.excludedEventTypes);
        },
        excludeEventType(eventType: string) {
            if (!this.excludedEventTypes.includes(eventType)) {
                this.excludedEventTypes.push(eventType);
                saveStoredFilters(this.excludedEventTypes);
            }
        },
        includeEventType(eventType: string) {
            const index = this.excludedEventTypes.indexOf(eventType);
            if (index > -1) {
                this.excludedEventTypes.splice(index, 1);
                saveStoredFilters(this.excludedEventTypes);
            }
        },
        excludeEventTypes(types: string[]) {
            let changed = false;
            types.forEach(t => {
                if (!this.excludedEventTypes.includes(t)) {
                    this.excludedEventTypes.push(t);
                    changed = true;
                }
            });
            if (changed) {
                saveStoredFilters(this.excludedEventTypes);
            }
        },
        includeEventTypes(types: string[]) {
            const initialLen = this.excludedEventTypes.length;
            this.excludedEventTypes = this.excludedEventTypes.filter(t => !types.includes(t));
            if (this.excludedEventTypes.length !== initialLen) {
                saveStoredFilters(this.excludedEventTypes);
            }
        },
        clearFilters() {
            this.excludedEventTypes = [];
            saveStoredFilters(this.excludedEventTypes);
        }
    }
});
