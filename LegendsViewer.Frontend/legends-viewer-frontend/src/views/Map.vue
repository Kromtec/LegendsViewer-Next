<template>
  <div class="map-container">
    <div id="map" style="height: 100%;"></div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, watch, ref, onBeforeUnmount } from 'vue';
import { useRouter, useRoute, type Router } from 'vue-router';
import { useWorldStore } from '../stores/worldStore';
import { useWorldMapStore } from '../stores/mapStore';
import L, { Map as LeafletMap, ImageOverlay, Layer, LayerGroup } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { components } from '../generated/api-schema'; // Import from the OpenAPI schema

export type SiteType = components['schemas']['SiteType'];
type SiteMarker = components['schemas']['SiteMarkerDto'];

interface SiteAtCoordinate {
  marker: SiteMarker;
  x: number;
  y: number;
}

interface MarkerConfig {
  shape: 'circle' | 'triangle' | 'square' | 'pentagon' | 'hexagon' | 'star';
  size?: number;
  color?: string;
}

const siteTypeMarkers: Record<SiteType, MarkerConfig> = {
  Unknown: { shape: 'circle' },

  // Dwarves
  Hillocks: { shape: 'square', size: 2 },
  Fortress: { shape: 'pentagon' },
  MountainHalls: { shape: 'hexagon', size: 4 },

  // Elves
  ForestRetreat: { shape: 'pentagon' },

  // Human
  Hamlet: { shape: 'square', size: 2 },
  Town: { shape: 'pentagon' },
  Castle: { shape: 'hexagon', size: 4 },

  // Goblins
  DarkPits: { shape: 'pentagon' },
  DarkFortress: { shape: 'hexagon' },

  // Main Civilizations
  Monastery: { shape: 'triangle' },
  Fort: { shape: 'triangle' },
  Tomb: { shape: 'triangle' },

  // Mysterious
  MysteriousLair: { shape: 'square', size: 2, color: '#AAAAFF' },
  MysteriousDungeon: { shape: 'pentagon', color: '#AAAAFF' },
  MysteriousPalace: { shape: 'hexagon', size: 4, color: '#AAAAFF' },

  // Nature (Kobolds often start there)
  Cave: { shape: 'circle' },

  // Monsters
  Lair: { shape: 'circle', size: 2 },

  // Demons
  Vault: { shape: 'star' },

  // Minotaur
  Labyrinth: { shape: 'star' },

  // Titan and Colossus
  Shrine: { shape: 'star' },

  // Necromancer
  Tower: { shape: 'star', size: 4 },

  // Others
  Camp: { shape: 'circle' },
  ImportantLocation: { shape: 'star' },
};

function createMarker(siteType: SiteType, siteColor: string | null | undefined, latlng: L.LatLngExpression): L.Layer {
  const config = siteTypeMarkers[siteType];
  const color = config?.color ?? siteColor ?? "#666"
  const size = config?.size ?? 3;
  const options = { pane: 'siteMarkerPane', color: color };
  switch (config?.shape) {
    case 'circle':
      return L.circle(latlng, { ...options, radius: size });
    case 'triangle':
      return createPolygon(latlng, 3, size, color);
    case 'square':
      return createPolygon(latlng, 4, size, color);
    case 'pentagon':
      return createPolygon(latlng, 5, size, color);
    case 'hexagon':
      return createPolygon(latlng, 6, size, color);
    case 'star':
      return createStar(latlng, 5, size, size / 2, color);
    default:
      return L.circle(latlng, { ...options, radius: size / 2 });
  }
}

function createPolygon(center: L.LatLngExpression, sides: number, size: number, color: string): L.Polygon {
  const vertices: L.LatLngExpression[] = [];
  for (let i = 0; i < sides; i++) {
    const angle = (i / sides) * 2 * Math.PI + Math.PI / 2; // Add 90 degrees (π/2 radians)
    const vertex: L.LatLngExpression = [
      (center as number[])[0] + size * Math.sin(angle),
      (center as number[])[1] + size * Math.cos(angle)
    ]
    vertices.push(vertex);
  }
  return L.polygon(vertices, { pane: 'siteMarkerPane', color });
}

function createStar(center: L.LatLngExpression, points: number, outer: number, inner: number, color: string): L.Polygon {
  const vertices: L.LatLngExpression[] = [];
  for (let i = 0; i < points * 2; i++) {
    const angle = (i / (points * 2)) * 2 * Math.PI + Math.PI / 2; // Add 90 degrees (π/2 radians)
    const radius = i % 2 === 0 ? outer : inner;
    const vertex: L.LatLngExpression = [
      (center as number[])[0] + radius * Math.sin(angle),
      (center as number[])[1] + radius * Math.cos(angle)
    ]
    vertices.push(vertex);
  }
  return L.polygon(vertices, { pane: 'siteMarkerPane', color });
}

function coordinateKey(x: number, y: number): string {
  return `${x},${y}`;
}

function appendRichContent(container: HTMLElement, html: string | null | undefined): void {
  if (!html) return;

  const parsedDocument = new DOMParser().parseFromString(html, 'text/html');
  for (const child of [...parsedDocument.body.childNodes]) {
    container.appendChild(document.importNode(child, true));
  }
}

function wireInternalLinks(container: HTMLElement, router: Router): void {
  for (const link of container.querySelectorAll<HTMLAnchorElement>('a[href]')) {
    const href = link.getAttribute('href');
    if (!href?.startsWith('/')) continue;

    link.addEventListener('click', event => {
      event.preventDefault();
      void router.push(href);
    });
  }
}

function groupSitesByCoordinate(siteMarkers: SiteMarker[]): Map<string, SiteAtCoordinate[]> {
  const sitesByCoordinate = new Map<string, SiteAtCoordinate[]>();

  for (const marker of siteMarkers) {
    for (const coordinate of marker.coordinates ?? []) {
      if (coordinate.x == null || coordinate.y == null) continue;

      const key = coordinateKey(coordinate.x, coordinate.y);
      const sites = sitesByCoordinate.get(key) ?? [];
      if (!sites.some(site => site.marker === marker)) {
        sites.push({ marker, x: coordinate.x, y: coordinate.y });
      }
      sitesByCoordinate.set(key, sites);
    }
  }

  return sitesByCoordinate;
}

function createSitePopup(sites: SiteAtCoordinate[], router: Router): HTMLElement {
  const container = document.createElement('div');
  container.className = 'site-picker';

  if (sites.length > 1) {
    container.classList.add('site-picker--multiple');
    const heading = document.createElement('strong');
    heading.className = 'site-picker__heading';
    heading.textContent = `${sites.length} sites at this location`;
    container.appendChild(heading);
  }

  for (const { marker } of sites) {
    const item = document.createElement('div');
    item.className = 'site-picker__item';

    if (marker.name) appendRichContent(item, marker.name);
    else item.append('Unknown site');
    item.append(document.createElement('br'));
    item.append(marker.typeAsString ?? 'Unknown');
    item.append(document.createElement('br'), document.createElement('br'));
    appendRichContent(item, marker.owner ?? 'Others');
    wireInternalLinks(item, router);
    container.appendChild(item);
  }

  return container;
}

function addCustomControl(map: L.Map, ownerLayers: Record<string, L.LayerGroup>) {
  const customControl = L.Control.extend({
    options: {
      position: 'topright', // Position of the control on the map
    },
    onAdd: function () {
      const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control leaflet-control-custom');

      // Activate All button
      const activateButton = L.DomUtil.create('a', '', container);
      activateButton.innerHTML = 'All';
      activateButton.style.width = '40px';
      activateButton.style.cursor = 'pointer';
      activateButton.style.padding = '0px';
      activateButton.style.background = '#333';
      activateButton.style.color = '#fff';
      activateButton.style.border = '2px solid #ccc';

      // Deactivate All button
      const deactivateButton = L.DomUtil.create('a', '', container);
      deactivateButton.innerHTML = 'None';
      deactivateButton.style.width = '40px';
      deactivateButton.style.cursor = 'pointer';
      deactivateButton.style.padding = '0px';
      deactivateButton.style.background = '#333';
      deactivateButton.style.color = '#fff';
      deactivateButton.style.border = '2px solid #ccc';
      deactivateButton.style.marginTop = '5px';
      deactivateButton.style.marginRight = '0px';

      // Activate all layers
      L.DomEvent.on(activateButton, 'click', () => {
        for (const owner in ownerLayers) {
          map.addLayer(ownerLayers[owner]); // Add each owner's layer to the map
        }
      });

      // Deactivate all layers
      L.DomEvent.on(deactivateButton, 'click', () => {
        for (const owner in ownerLayers) {
          map.removeLayer(ownerLayers[owner]); // Remove each owner's layer from the map
        }
      });

      return container;
    },
  });

  // Add the custom control to the map
  map.addControl(new customControl());
}

export default defineComponent({
  setup() {
    const worldStore = useWorldStore();
    const mapStore = useWorldMapStore();
    const router = useRouter();
    const route = useRoute();
    const leafletMap = ref<LeafletMap>();
    const currentOverlay = ref<ImageOverlay | null>(null);
    const siteCountLayer = ref<LayerGroup | null>(null);
    const highlightLayer = ref<LayerGroup | null>(null);
    const siteMarkersMap = new Map<number, { marker: L.Layer; popup: HTMLElement }>();

    // This will hold the LayerGroups for different owners
    const ownerLayers: Record<string, LayerGroup> = {};
    const controlLayers = ref<L.Control.Layers>();

    const syncSiteCountLayer = () => {
      if (!leafletMap.value || !siteCountLayer.value) return;

      const countLayer = siteCountLayer.value as unknown as Layer;
      const shouldShow = leafletMap.value.getZoom() === leafletMap.value.getMaxZoom();
      const isVisible = leafletMap.value.hasLayer(countLayer);

      if (shouldShow && !isVisible) {
        leafletMap.value.addLayer(countLayer);
      } else if (!shouldShow && isVisible) {
        leafletMap.value.removeLayer(countLayer);
      }
    };

    const initMap = async () => {
      if (!leafletMap.value) {
        leafletMap.value = L.map('map', {
          crs: L.CRS.Simple,
          zoom: 0,
          minZoom: -2,
          maxZoom: 2
        });
        leafletMap.value.on('zoomend', syncSiteCountLayer);

        // Custom pane for pulse highlight circles placed above map overlay (400) and below markers (550)
        const highlightPane = leafletMap.value.createPane('highlightPane');
        highlightPane.style.zIndex = '450';
        highlightPane.style.pointerEvents = 'none';

        // Custom pane for interactive site markers (zIndex 550)
        const siteMarkerPane = leafletMap.value.createPane('siteMarkerPane');
        siteMarkerPane.style.zIndex = '550';
      }
      await worldStore.loadWorld();
      await mapStore.loadWorldMap('Large');
      if (mapStore.worldMapMax != null) {
        loadImageToMap(mapStore.worldMapMax);
      }
      // Add custom control to activate/deactivate all layers
      addCustomControl(leafletMap.value, ownerLayers);
    };

    const toLatLng = (x: number, y: number): L.LatLngTuple => {
      const scale = 8;
      const height = worldStore.world.height ?? 0;
      return [(height - y) * scale - 0.5 * scale, x * scale + 0.5 * scale];
    };

    const handleObjectFocus = async (type: string, id: number) => {
      if (!leafletMap.value) return;

      if (highlightLayer.value) {
        leafletMap.value.removeLayer(highlightLayer.value as unknown as Layer);
      }
      highlightLayer.value = new L.LayerGroup().addTo(leafletMap.value);

      const siteMarkers = worldStore.world.siteMarkers ?? [];

      const addPulseCircle = (x: number, y: number, radius = 18) => {
        const latlng = toLatLng(x, y);
        const circle = L.circle(latlng, {
          pane: 'highlightPane',
          color: '#ffcc00',
          fillColor: '#ff3300',
          fillOpacity: 0.4,
          radius: radius,
          weight: 4,
          interactive: false,
          className: 'target-highlight-pulse',
        });
        highlightLayer.value?.addLayer(circle);
      };

      const lowerType = type.toLowerCase();

      // 1. SITE
      if (lowerType === 'site') {
        const site = siteMarkers.find(s => s.id === id);
        if (site && site.coordinates?.length) {
          const { x, y } = site.coordinates[0];
          if (x != null && y != null) {
            const latlng = toLatLng(x, y);
            leafletMap.value.setView(latlng, 1);
            addPulseCircle(x, y, 16);

            const siteEntry = siteMarkersMap.get(id);
            if (siteEntry) {
              siteEntry.marker.openPopup();
            }
            return;
          }
        }
        await fetchAndFocusCoordinates(type, id);
      }
      // 2. ENTITY (Civilization / Faction / Group owning sites)
      else if (lowerType === 'entity') {
        // Find sites owned by entity ID (using ownerId or currentOwnerId)
        // @ts-ignore
        const matchingSites = siteMarkers.filter(s => s.ownerId === id || s.currentOwnerId === id);

        if (matchingSites.length > 0) {
          const points: L.LatLngTuple[] = [];

          // Make sure matching owner layers are active on the map
          for (const site of matchingSites) {
            const ownerText = site.ownerText ?? 'Unknown';
            if (ownerLayers[ownerText] && !leafletMap.value.hasLayer(ownerLayers[ownerText])) {
              leafletMap.value.addLayer(ownerLayers[ownerText]);
            }

            site.coordinates?.forEach(coord => {
              if (coord.x != null && coord.y != null) {
                points.push(toLatLng(coord.x, coord.y));
                addPulseCircle(coord.x, coord.y, 16);
              }
            });
          }

          if (points.length === 1) {
            leafletMap.value.setView(points[0], 1);
            const firstSite = matchingSites[0];
            const siteEntry = firstSite.id ? siteMarkersMap.get(firstSite.id) : undefined;
            if (siteEntry) {
              siteEntry.marker.openPopup();
            }
          } else if (points.length > 1) {
            const bounds = L.latLngBounds(points);
            leafletMap.value.fitBounds(bounds, { padding: [80, 80] });
          }
        } else {
          // Entity has no current sites in siteMarkers list -> fetch center/coords from API
          await fetchAndFocusCoordinates(type, id);
        }
      }
      // 3. OTHER OBJECT TYPES (Region, Landmass, River, Construction, Structure, MountainPeak, Artifact, etc.)
      else {
        await fetchAndFocusCoordinates(type, id);
      }
    };

    const fetchAndFocusCoordinates = async (type: string, id: number) => {
      if (!leafletMap.value) return;

      const scale = 8;
      const addPulseCircle = (x: number, y: number, radius = 18) => {
        const latlng = toLatLng(x, y);
        const circle = L.circle(latlng, {
          pane: 'highlightPane',
          color: '#ffcc00',
          fillColor: '#ff3300',
          fillOpacity: 0.4,
          radius: radius,
          weight: 4,
          interactive: false,
          className: 'target-highlight-pulse',
        });
        highlightLayer.value?.addLayer(circle);
      };

      try {
        const response = await fetch(`http://localhost:15421/api/WorldMap/coordinates/${type}/${id}`);
        if (!response.ok) return;
        const data = await response.json();

        if (data) {
          const { minX, maxX, minY, maxY, centerX, centerY } = data;
          if (minX != null && maxX != null && minY != null && maxY != null) {
            if (minX === maxX && minY === maxY) {
              const latlng = toLatLng(centerX, centerY);
              leafletMap.value.setView(latlng, 1);
            } else {
              const southWest = toLatLng(minX, maxY);
              const northEast = toLatLng(maxX, minY);
              const bounds = L.latLngBounds(southWest, northEast);
              leafletMap.value.fitBounds(bounds, { padding: [80, 80] });
            }
          } else if (centerX != null && centerY != null) {
            const latlng = toLatLng(centerX, centerY);
            leafletMap.value.setView(latlng, 1);
          }

          if (centerX != null && centerY != null) {
            const radius = (maxX != null && minX != null) ? Math.max(16, (maxX - minX + 1) * scale * 0.6) : 18;
            addPulseCircle(centerX, centerY, isNaN(radius) ? 18 : radius);
          }
        }
      } catch (err) {
        console.error('Failed to fetch object coordinates for map focus:', err);
      }
    };

    const loadImageToMap = (base64Image: string) => {
      if (!leafletMap.value) return;

      if (currentOverlay.value) {
        leafletMap.value.removeLayer(currentOverlay.value as unknown as Layer);
      }
      if (siteCountLayer.value) {
        leafletMap.value.removeLayer(siteCountLayer.value as unknown as Layer);
        siteCountLayer.value = null;
      }
      siteMarkersMap.clear();

      const scale = 8;
      const width = (worldStore.world.width ?? 0);
      const height = (worldStore.world.height ?? 0);

      const bounds: L.LatLngBoundsExpression = [[0, 0], [scale * height, scale * width]];
      const imageOverlay = L.imageOverlay(base64Image, bounds);

      imageOverlay.addTo(leafletMap.value);
      currentOverlay.value = imageOverlay;

      if (worldStore.world.siteMarkers != null) {
        const layersControl: Record<string, LayerGroup> = {};
        const sitesByCoordinate = groupSitesByCoordinate(worldStore.world.siteMarkers);

        // Iterate over the site markers
        for (const siteMarker of worldStore.world.siteMarkers) {
          if (siteMarker.coordinates != null && siteMarker.owner != null) {
            const ownerText = siteMarker.ownerText ?? 'Unknown'
            // Create a layer group for each owner if it doesn't exist
            if (!ownerLayers[ownerText]) {
              ownerLayers[ownerText] = new L.LayerGroup();
              layersControl[siteMarker.owner] = ownerLayers[ownerText];
            }

            for (const coordinate of siteMarker.coordinates) {
              if (coordinate.x != null && coordinate.y != null) {
                const marker = createMarker(
                  siteMarker.type ?? 'Unknown',
                  siteMarker.color,
                  [(height - coordinate.y) * scale - 0.5 * scale, coordinate.x * scale + 0.5 * scale]
                );

                const sitesAtCoordinate = sitesByCoordinate.get(coordinateKey(coordinate.x, coordinate.y)) ?? [];
                const popupContent = createSitePopup(sitesAtCoordinate, router);
                marker.bindPopup(
                  popupContent,
                  sitesAtCoordinate.length > 1 ? { minWidth: 220 } : undefined
                );
                ownerLayers[ownerText].addLayer(marker); // Add the marker to the owner's layer

                if (siteMarker.id != null) {
                  siteMarkersMap.set(siteMarker.id, { marker, popup: popupContent });
                }
              }
            }
          }
        }

        // Add the layers to the map
        for (const key in ownerLayers) {
          ownerLayers[key].addTo(leafletMap.value);
        }
        if (!controlLayers.value) {
          controlLayers.value = L.control.layers(undefined, layersControl).addTo(leafletMap.value);
        }

        const countLayer = new L.LayerGroup();
        for (const sites of sitesByCoordinate.values()) {
          if (sites.length < 2) continue;

          const { x, y } = sites[0];
          const countBadge = L.marker(
            [(height - y) * scale - 0.5 * scale, x * scale + 0.5 * scale],
            {
              icon: L.divIcon({
                className: 'site-count-marker',
                html: `<span>${sites.length}</span>`,
                iconSize: [12, 12],
                iconAnchor: [6, 6],
              }),
              interactive: false,
              keyboard: false,
              zIndexOffset: 1000,
            }
          );
          countLayer.addLayer(countBadge);
        }
        siteCountLayer.value = countLayer;
        syncSiteCountLayer();
      }

      // Check if target object is specified in route query params
      const queryType = route.query.type as string | undefined;
      const queryId = route.query.id ? parseInt(route.query.id as string, 10) : undefined;

      if (queryType && queryId && !isNaN(queryId)) {
        void handleObjectFocus(queryType, queryId);
      } else {
        leafletMap.value.fitBounds(bounds);
      }
    };

    watch(() => mapStore.worldMapMax, (newBase64Map) => {
      if (newBase64Map) {
        loadImageToMap(newBase64Map);
      }
    });

    watch(() => [route.query.type, route.query.id], ([newType, newId]) => {
      if (newType && newId) {
        const typeStr = newType as string;
        const idNum = parseInt(newId as string, 10);
        if (!isNaN(idNum)) {
          void handleObjectFocus(typeStr, idNum);
        }
      }
    });

    onMounted(() => {
      initMap();
    });

    onBeforeUnmount(() => {
      if (leafletMap.value) {
        leafletMap.value.remove();
      }
    });

    return {
      mapStore,
    };
  },
});
</script>

<style>
.map-container {
  height: 880px;
  width: 100%;
  position: relative;
  /* Ensure the map and controls are positioned properly */
}

.leaflet-control-layers,
.leaflet-container {
  background: rgb(var(--v-theme-background));
  color: rgb(var(--v-theme-foreground));
}

.leaflet-layer,
.leaflet-control-zoom-in,
.leaflet-control-zoom-out,
.leaflet-control-attribution,
.leaflet-popup {
  filter: invert(100%) hue-rotate(180deg) brightness(95%) contrast(90%);
}

.leaflet-control-layers-overlays label {
  margin-top: 4px;
}

.site-picker__heading {
  display: block;
  margin-bottom: 10px;
}

.site-picker--multiple .site-picker__item {
  padding: 8px 0;
}

.site-picker--multiple .site-picker__item + .site-picker__item {
  border-top: 1px solid rgba(127, 127, 127, 0.35);
}

.site-count-marker {
  pointer-events: none;
}

.site-count-marker span {
  display: flex;
  width: 12px;
  height: 12px;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(255, 255, 255, 0.8);
  border-radius: 50%;
  background: rgba(25, 25, 25, 0.72);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.45);
  color: rgba(255, 255, 255, 0.9);
  font-size: 8px;
  font-weight: 600;
  line-height: 1;
}

@keyframes map-target-pulse {
  0% {
    stroke-width: 3px;
    stroke-opacity: 1;
    fill-opacity: 0.5;
  }
  50% {
    stroke-width: 7px;
    stroke-opacity: 0.7;
    fill-opacity: 0.2;
  }
  100% {
    stroke-width: 3px;
    stroke-opacity: 1;
    fill-opacity: 0.5;
  }
}

.target-highlight-pulse {
  animation: map-target-pulse 1.5s infinite ease-in-out;
}
</style>

