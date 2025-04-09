import { defineStore } from 'pinia'
import client from "../apiClient"; // Import the global client
import { components } from '../generated/api-schema'; // Import from the OpenAPI schema
import { LoadItemsSortOption } from '../types/legends';

export type WorldDto = components['schemas']['WorldDto'];
// Common types
type WorldObjectDto = components['schemas']['WorldObjectDto'];
type WorldEventDto = components['schemas']['WorldEventDto'];
type ChartDataDto = components['schemas']['ChartDataDto'];

export const useWorldStore = defineStore('world', {
    state: () => ({
        world: '' as WorldDto,
        objectEvents: [] as WorldEventDto[],
        objectEventsTotalItems: 0 as number,
        objectEventsPerPage: 10 as number,
        objectEventCollections: [] as WorldObjectDto[],
        objectEventCollectionsTotalItems: 0 as number,
        objectEventCollectionsPerPage: 10 as number,

        itemsPerPageOptions: [
            { value: 10, title: '10' },
            { value: 25, title: '25' },
            { value: 50, title: '50' },
            { value: 100, title: '100' }
        ],

        objectEventChartData: null as ChartDataDto | null,
        objectEventTypeChartData: null as ChartDataDto | null,
        isLoading: false as boolean
    }),
    actions: {
        async loadWorld() {
            this.isLoading = true;
            const { data, error } = await client.GET("/api/World");

            if (error !== undefined) {
                this.isLoading = false;
                console.error(error);
            } else if (data) {
                this.world = data;
                this.isLoading = false;
            }
        },
        async loadEvents(pageNumber: number, pageSize: number, sortBy: LoadItemsSortOption[]) {
            this.isLoading = true;
            const { data, error } = await client.GET("/api/World/events", {
                params: {
                    query: {
                        pageNumber: pageNumber,
                        pageSize: pageSize,
                        sortKey: sortBy[0]?.key,
                        sortOrder: sortBy[0]?.order
                    },
                },
            });

            if (error !== undefined) {
                this.isLoading = false;
                console.error(error);
            } else if (data) {
                this.objectEvents = (data as any).items as WorldEventDto[] ?? [];
                this.objectEventsTotalItems = (data as any).totalCount as number ?? 0;
                this.isLoading = false;
            }
        },
        async loadEventCollections(pageNumber: number, pageSize: number, sortBy: LoadItemsSortOption[]) {
            this.isLoading = true;
            const { data, error } = await client.GET("/api/World/eventcollections", {
                params: {
                    query: {
                        pageNumber: pageNumber,
                        pageSize: pageSize,
                        sortKey: sortBy[0]?.key,
                        sortOrder: sortBy[0]?.order
                    },
                },
            });

            if (error !== undefined) {
                this.isLoading = false;
                console.error(error);
            } else if (data) {
                this.objectEventCollections = (data as any).items as WorldObjectDto[] ?? [];
                this.objectEventCollectionsTotalItems = (data as any).totalCount as number ?? 0;
                this.isLoading = false;
            }
        },
        async loadEventChartData() {
            this.isLoading = true;
            const { data, error } = await client.GET("/api/World/eventchart");

            if (error !== undefined) {
                this.isLoading = false;
                console.error(error);
            } else if (data) {
                this.objectEventChartData = data as ChartDataDto;
                this.isLoading = false;
            }
        },
        async loadEventTypeChartData() {
            this.isLoading = true;
            const { data, error } = await client.GET("/api/World/eventtypechart");

            if (error !== undefined) {
                this.isLoading = false;
                console.error(error);
            } else if (data) {
                this.objectEventTypeChartData = data as ChartDataDto;
                this.isLoading = false;
            }
        },
    },
})