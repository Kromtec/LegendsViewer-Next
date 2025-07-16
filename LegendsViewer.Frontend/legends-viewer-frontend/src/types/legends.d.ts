import { components } from '../generated/api-schema'; // Import from the OpenAPI schema

type ListItemDto = components['schemas']['ListItemDto'];

export type LegendLinkListData = {
    title: string,
    subtitle: string,
    icon: string,
    items: string[] | ListItemDto[]
}

export interface LoadItemsSortOption {
    key: string,
    order: "asc" | "desc"
}

export interface LoadItemsOptions {
    page: number;
    itemsPerPage: number;
    sortBy: LoadItemsSortOption[];
}

export interface LoadItemsOptionsWithSearch {
    page: number;
    itemsPerPage: number;
    sortBy: LoadItemsSortOption[];
    search: string;
}

export interface TableHeader {
    title: string;
    key: string;
    align?: 'start' | 'end' | 'center';
    sortable?: boolean;
}
