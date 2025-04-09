import { defineStore } from 'pinia';
import client from "../apiClient"; // Import the global client
import { paths } from '../generated/api-schema';
import { components } from '../generated/api-schema'; // Import from the OpenAPI schema
import { LoadItemsSortOption } from '../types/legends';
import { UnwrapRef } from 'vue';

type PathsWithMethod<TPaths, TMethod extends string> = {
    [K in keyof TPaths]: TPaths[K] extends { [method in TMethod]: any } ? K : never;
  }[keyof TPaths];

// Common types
type WorldObjectDto = components['schemas']['WorldObjectDto'];
type WorldEventDto = components['schemas']['WorldEventDto'];
type ChartDataDto = components['schemas']['ChartDataDto'];

const worldObjectApiPaths: Record<string, {
    overview: PathsWithMethod<paths, "get">;
    object: PathsWithMethod<paths, "get">;
    objectEvents: PathsWithMethod<paths, "get">;
    objectEventCollections: PathsWithMethod<paths, "get">;
    eventChart: PathsWithMethod<paths, "get">;
    eventTypeChart: PathsWithMethod<paths, "get">;
}> = {
    Site: {
        overview: "/api/Site"  as PathsWithMethod<paths, "get">,
        object: "/api/Site/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Site/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Site/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Site/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Site/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Region: {
        overview: "/api/Region" as PathsWithMethod<paths, "get">,
        object: "/api/Region/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Region/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Region/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Region/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Region/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    UndergroundRegion: {
        overview: "/api/UndergroundRegion" as PathsWithMethod<paths, "get">,
        object: "/api/UndergroundRegion/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/UndergroundRegion/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/UndergroundRegion/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/UndergroundRegion/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/UndergroundRegion/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Landmass: {
        overview: "/api/Landmass" as PathsWithMethod<paths, "get">,
        object: "/api/Landmass/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Landmass/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Landmass/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Landmass/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Landmass/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    River: {
        overview: "/api/River" as PathsWithMethod<paths, "get">,
        object: "/api/River/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/River/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/River/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/River/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/River/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Structure: {
        overview: "/api/Structure" as PathsWithMethod<paths, "get">,
        object: "/api/Structure/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Structure/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Structure/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Structure/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Structure/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Construction: {
        overview: "/api/Construction" as PathsWithMethod<paths, "get">,
        object: "/api/Construction/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Construction/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Construction/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Construction/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Construction/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    MountainPeak: {
        overview: "/api/MountainPeak" as PathsWithMethod<paths, "get">,
        object: "/api/MountainPeak/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/MountainPeak/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/MountainPeak/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/MountainPeak/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/MountainPeak/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    HistoricalFigure: {
        overview: "/api/HistoricalFigure" as PathsWithMethod<paths, "get">,
        object: "/api/HistoricalFigure/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/HistoricalFigure/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/HistoricalFigure/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/HistoricalFigure/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/HistoricalFigure/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Entity: {
        overview: "/api/Entity" as PathsWithMethod<paths, "get">,
        object: "/api/Entity/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Entity/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Entity/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Entity/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Entity/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    DanceForm: {
        overview: "/api/DanceForm" as PathsWithMethod<paths, "get">,
        object: "/api/DanceForm/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/DanceForm/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/DanceForm/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/DanceForm/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/DanceForm/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    MusicalForm: {
        overview: "/api/MusicalForm" as PathsWithMethod<paths, "get">,
        object: "/api/MusicalForm/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/MusicalForm/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/MusicalForm/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/MusicalForm/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/MusicalForm/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    PoeticForm: {
        overview: "/api/PoeticForm" as PathsWithMethod<paths, "get">,
        object: "/api/PoeticForm/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/PoeticForm/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/PoeticForm/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/PoeticForm/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/PoeticForm/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    WrittenContent: {
        overview: "/api/WrittenContent" as PathsWithMethod<paths, "get">,
        object: "/api/WrittenContent/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/WrittenContent/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/WrittenContent/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/WrittenContent/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/WrittenContent/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Artifact: {
        overview: "/api/Artifact" as PathsWithMethod<paths, "get">,
        object: "/api/Artifact/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Artifact/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Artifact/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Artifact/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Artifact/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Era: {
        overview: "/api/Era" as PathsWithMethod<paths, "get">,
        object: "/api/Era/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Era/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Era/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Era/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Era/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
};

const eventCollectionApiPaths: Record<string, {
    overview: PathsWithMethod<paths, "get">;
    object: PathsWithMethod<paths, "get">;
    objectEvents: PathsWithMethod<paths, "get">;
    objectEventCollections: PathsWithMethod<paths, "get">;
    eventChart: PathsWithMethod<paths, "get">;
    eventTypeChart: PathsWithMethod<paths, "get">;
}> = {
    // EventCollections
    War: {
        overview: "/api/War" as PathsWithMethod<paths, "get">,
        object: "/api/War/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/War/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/War/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/War/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/War/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Battle: {
        overview: "/api/Battle" as PathsWithMethod<paths, "get">,
        object: "/api/Battle/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Battle/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Battle/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Battle/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Battle/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Duel: {
        overview: "/api/Duel" as PathsWithMethod<paths, "get">,
        object: "/api/Duel/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Duel/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Duel/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Duel/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Duel/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Raid: {
        overview: "/api/Raid" as PathsWithMethod<paths, "get">,
        object: "/api/Raid/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Raid/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Raid/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Raid/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Raid/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    SiteConquered: {
        overview: "/api/SiteConquered" as PathsWithMethod<paths, "get">,
        object: "/api/SiteConquered/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/SiteConquered/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/SiteConquered/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/SiteConquered/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/SiteConquered/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },

    Insurrection: {
        overview: "/api/Insurrection" as PathsWithMethod<paths, "get">,
        object: "/api/Insurrection/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Insurrection/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Insurrection/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Insurrection/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Insurrection/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Persecution: {
        overview: "/api/Persecution" as PathsWithMethod<paths, "get">,
        object: "/api/Persecution/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Persecution/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Persecution/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Persecution/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Persecution/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Purge: {
        overview: "/api/Purge" as PathsWithMethod<paths, "get">,
        object: "/api/Purge/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Purge/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Purge/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Purge/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Purge/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Coup: {
        overview: "/api/Coup" as PathsWithMethod<paths, "get">,
        object: "/api/Coup/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Coup/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Coup/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Coup/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Coup/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },

    BeastAttack: {
        overview: "/api/BeastAttack" as PathsWithMethod<paths, "get">,
        object: "/api/BeastAttack/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/BeastAttack/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/BeastAttack/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/BeastAttack/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/BeastAttack/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Abduction: {
        overview: "/api/Abduction" as PathsWithMethod<paths, "get">,
        object: "/api/Abduction/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Abduction/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Abduction/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Abduction/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Abduction/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Theft: {
        overview: "/api/Theft" as PathsWithMethod<paths, "get">,
        object: "/api/Theft/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Theft/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Theft/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Theft/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Theft/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },

    Procession: {
        overview: "/api/Procession" as PathsWithMethod<paths, "get">,
        object: "/api/Procession/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Procession/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Procession/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Procession/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Procession/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Performance: {
        overview: "/api/Performance" as PathsWithMethod<paths, "get">,
        object: "/api/Performance/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Performance/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Performance/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Performance/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Performance/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Journey: {
        overview: "/api/Journey" as PathsWithMethod<paths, "get">,
        object: "/api/Journey/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Journey/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Journey/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Journey/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Journey/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Competition: {
        overview: "/api/Competition" as PathsWithMethod<paths, "get">,
        object: "/api/Competition/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Competition/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Competition/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Competition/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Competition/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Ceremony: {
        overview: "/api/Ceremony" as PathsWithMethod<paths, "get">,
        object: "/api/Ceremony/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Ceremony/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Ceremony/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Ceremony/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Ceremony/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
    Occasion: {
        overview: "/api/Occasion" as PathsWithMethod<paths, "get">,
        object: "/api/Occasion/{id}" as PathsWithMethod<paths, "get">,
        objectEvents: "/api/Occasion/{id}/events" as PathsWithMethod<paths, "get">,
        objectEventCollections: "/api/Occasion/{id}/eventcollections" as PathsWithMethod<paths, "get">,
        eventChart: "/api/Occasion/{id}/eventchart" as PathsWithMethod<paths, "get">,
        eventTypeChart: "/api/Occasion/{id}/eventtypechart" as PathsWithMethod<paths, "get">
    },
};

export const useSiteStore = createWorldObjectStore<components['schemas']['Site']>('Site', 'object');
export const useRegionStore = createWorldObjectStore<components['schemas']['WorldRegion']>('Region', 'object');
export const useUndergroundRegionStore = createWorldObjectStore<components['schemas']['UndergroundRegion']>('UndergroundRegion', 'object');
export const useLandmassStore = createWorldObjectStore<components['schemas']['Landmass']>('Landmass', 'object');
export const useRiverStore = createWorldObjectStore<components['schemas']['River']>('River', 'object');
export const useStructureStore = createWorldObjectStore<components['schemas']['Structure']>('Structure', 'object');
export const useConstructionStore = createWorldObjectStore<components['schemas']['WorldConstruction']>('Construction', 'object');
export const useMountainPeakStore = createWorldObjectStore<components['schemas']['MountainPeak']>('MountainPeak', 'object');
export const useHistoricalFigureStore = createWorldObjectStore<components['schemas']['HistoricalFigure']>('HistoricalFigure', 'object');
export const useEntityStore = createWorldObjectStore<components['schemas']['Entity']>('Entity', 'object');
export const useDanceFormStore = createWorldObjectStore<components['schemas']['DanceForm']>('DanceForm', 'object');
export const useMusicalFormStore = createWorldObjectStore<components['schemas']['MusicalForm']>('MusicalForm', 'object');
export const usePoeticFormStore = createWorldObjectStore<components['schemas']['PoeticForm']>('PoeticForm', 'object');
export const useWrittenContentStore = createWorldObjectStore<components['schemas']['WrittenContent']>('WrittenContent', 'object');
export const useArtifactStore = createWorldObjectStore<components['schemas']['Artifact']>('Artifact', 'object');
export const useEraStore = createWorldObjectStore<components['schemas']['Era']>('Era', 'object');

export const useWarStore = createWorldObjectStore<components['schemas']['War']>('War', 'collection');
export const useBattleStore = createWorldObjectStore<components['schemas']['Battle']>('Battle', 'collection');
export const useDuelStore = createWorldObjectStore<components['schemas']['Duel']>('Duel', 'collection');
export const useRaidStore = createWorldObjectStore<components['schemas']['Raid']>('Raid', 'collection');
export const useSiteConqueredStore = createWorldObjectStore<components['schemas']['SiteConquered']>('SiteConquered', 'collection');

export const useInsurrectionStore = createWorldObjectStore<components['schemas']['Insurrection']>('Insurrection', 'collection');
export const usePersecutionStore = createWorldObjectStore<components['schemas']['Persecution']>('Persecution', 'collection');
export const usePurgeStore = createWorldObjectStore<components['schemas']['Purge']>('Purge', 'collection');
export const useCoupStore = createWorldObjectStore<components['schemas']['EntityOverthrownCollection']>('Coup', 'collection');

export const useBeastAttackStore = createWorldObjectStore<components['schemas']['BeastAttack']>('BeastAttack', 'collection');
export const useAbductionStore = createWorldObjectStore<components['schemas']['Abduction']>('Abduction', 'collection');
export const useTheftStore = createWorldObjectStore<components['schemas']['Theft']>('Theft', 'collection');

export const useProcessionStore = createWorldObjectStore<components['schemas']['ProcessionCollection']>('Procession', 'collection');
export const usePerformanceStore = createWorldObjectStore<components['schemas']['PerformanceCollection']>('Performance', 'collection');
export const useJourneyStore = createWorldObjectStore<components['schemas']['Journey']>('Journey', 'collection');
export const useCompetitionStore = createWorldObjectStore<components['schemas']['CompetitionCollection']>('Competition', 'collection');
export const useCeremonyStore = createWorldObjectStore<components['schemas']['CeremonyCollection']>('Ceremony', 'collection');
export const useOccasionStore = createWorldObjectStore<components['schemas']['Occasion']>('Occasion', 'collection');

export function createWorldObjectStore<T>(resourceName: string, type: string) {
    let pathsForResource;
    if (type == 'collection') {
        pathsForResource = eventCollectionApiPaths[resourceName];
    } else {
        pathsForResource = worldObjectApiPaths[resourceName];
    }
    if (!pathsForResource) {
        throw new Error(`Invalid resource name: ${resourceName}`);
    }
    return defineStore(resourceName, {
        state: () => ({
            objects: [] as WorldObjectDto[],
            objectsTotalItems: 0 as number,
            objectsTotalFilteredItems: 0 as number,
            objectsPerPage: 10 as number,

            // object is typed specifically for each store
            object: null as T | null,
            objectEvents: [] as WorldEventDto[],
            objectEventsTotalItems: 0 as number,
            objectEventsPerPage: 10 as number,
            objectEventCollections: [] as WorldObjectDto[],
            objectEventCollectionsTotalItems: 0 as number,
            objectEventCollectionsPerPage: 10 as number,

            objectEventChartData: null as ChartDataDto | null,
            objectEventTypeChartData: null as ChartDataDto | null,

            itemsPerPageOptions: [
                {value: 10, title: '10'},
                {value: 25, title: '25'},
                {value: 50, title: '50'},
                {value: 100, title: '100'}
              ],

            isLoading: false as boolean
        }),
        actions: {
            async loadOverview(pageNumber: number, pageSize: number, sortBy: LoadItemsSortOption[], search: string) {
                this.isLoading = true;
                // @ts-ignore
                const { data, error } = await client.GET(pathsForResource.overview, {
                    params: {
                        query: {
                            pageNumber: pageNumber,
                            pageSize: pageSize,
                            sortKey: sortBy[0]?.key,
                            sortOrder: sortBy[0]?.order,
                            search: search
                        },
                    },
                });

                if (error !== undefined) {
                    this.isLoading = false;
                    console.error(error);
                } else if (data) {
                    this.objects = (data as any).items ?? [];
                    this.objectsTotalItems = (data as any).totalCount ?? 0;
                    this.objectsTotalFilteredItems = (data as any).totalFilteredCount ?? 0;
                    this.isLoading = false;
                }
            },
            async load(id: number) {
                this.isLoading = true;
                const { data, error } = await client.GET(pathsForResource.object, {
                    params: {
                        path: { id: id },
                    },
                });

                if (error !== undefined) {
                    this.isLoading = false;
                    console.error(error);
                } else if (data) {
                    this.object = data as UnwrapRef<T> | null;
                    this.isLoading = false;
                }
            },
            async loadEvents(id: number, pageNumber: number, pageSize: number, sortBy: LoadItemsSortOption[]) {
                this.isLoading = true;
                // @ts-ignore
                const { data, error } = await client.GET(pathsForResource.objectEvents, {
                    params: {
                        path: { id: id },
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
            async loadEventCollections(id: number, pageNumber: number, pageSize: number, sortBy: LoadItemsSortOption[]) {
                this.isLoading = true;
                const { data, error } = await client.GET(pathsForResource.objectEventCollections, {
                    params: {
                        path: { id: id },
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
            async loadEventChartData(id: number) {
                this.isLoading = true;
                const { data, error } = await client.GET(pathsForResource.eventChart, {
                    params: { path: { id: id } },
                });

                if (error !== undefined) {
                    this.isLoading = false;
                    console.error(error);
                } else if (data) {
                    this.objectEventChartData = data as ChartDataDto;
                    this.isLoading = false;
                }
            },
            async loadEventTypeChartData(id: number) {
                this.isLoading = true;
                const { data, error } = await client.GET(pathsForResource.eventTypeChart, {
                    params: { path: { id: id } },
                });

                if (error !== undefined) {
                    this.isLoading = false;
                    console.error(error);
                } else if (data) {
                    this.objectEventTypeChartData = data as ChartDataDto;
                    this.isLoading = false;
                }
            },
        },
    });
}
