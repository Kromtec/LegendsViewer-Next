import { defineStore } from 'pinia'
import client from '../apiClient'

export interface RecordEntry {
    rank: number;
    id: number;
    name: string;
    objectType: string;
    linkHtml: string;
    value: string;
    numericValue: number;
    subtitle?: string;
    detailText?: string;
    race?: string;
    isSupernatural?: boolean;
}

export interface RecordCard {
    id: string;
    title: string;
    icon: string;
    metricName: string;
    entries: RecordEntry[];
}

export interface RecordCategory {
    id: string;
    title: string;
    icon: string;
    cards: RecordCard[];
}

export interface RecordHighlight {
    title: string;
    value: string;
    icon: string;
    linkHtml?: string;
}

export interface WorldRecordsData {
    highlights: RecordHighlight[];
    categories: RecordCategory[];
}

export const useWorldRecordsStore = defineStore('worldRecords', {
    state: () => ({
        records: null as WorldRecordsData | null,
        isLoading: false as boolean
    }),
    actions: {
        async loadRecords(forceRefresh = false) {
            if (this.records && !forceRefresh) return;
            this.isLoading = true;
            try {
                // @ts-ignore
                const { data, error } = await client.GET('/api/World/records');
                if (error) {
                    console.error('Failed to load world records:', error);
                } else if (data) {
                    this.records = data as unknown as WorldRecordsData;
                }
            } catch (err) {
                console.error(err);
            } finally {
                this.isLoading = false;
            }
        },
        clearRecords() {
            this.records = null;
        }
    }
})
