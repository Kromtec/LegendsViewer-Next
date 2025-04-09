<template>
  <div class="map-container">
    <div id="map" style="height: 100%;"></div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, watch, ref, onBeforeUnmount } from 'vue';
import { useWorldStore } from '../stores/worldStore';
import { useWorldMapStore } from '../stores/mapStore';
import L, { Map, ImageOverlay, Layer, LayerGroup } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { components } from '../generated/api-schema'; // Import from the OpenAPI schema

export type SiteType = components['schemas']['SiteType'];

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
  switch (config?.shape) {
    case 'circle':
      return L.circle(latlng, { color: color, radius: size });
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
      return L.circle(latlng, { color: color, radius: size / 2 });
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
  return L.polygon(vertices, { color });
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
  return L.polygon(vertices, { color });
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
    const leafletMap = ref<Map>();
    const currentOverlay = ref<ImageOverlay | null>(null);

    // This will hold the LayerGroups for different owners
    const ownerLayers: Record<string, LayerGroup> = {};
    const controlLayers = ref<L.Control.Layers>();

    const initMap = async () => {
      if (!leafletMap.value) {
        leafletMap.value = L.map('map', {
          crs: L.CRS.Simple,
          zoom: 0,
          minZoom: -2,
          maxZoom: 2
        });
      }
      await worldStore.loadWorld();
      await mapStore.loadWorldMap('Large');
      if (mapStore.worldMapMax != null) {
        loadImageToMap(mapStore.worldMapMax);
      }
      // Add custom control to activate/deactivate all layers
      addCustomControl(leafletMap.value, ownerLayers);
    };

    const loadImageToMap = (base64Image: string) => {
      if (!leafletMap.value) return;

      if (currentOverlay.value) {
        leafletMap.value.removeLayer(currentOverlay.value as unknown as Layer);
      }

      const scale = 8;
      const width = (worldStore.world.width ?? 0);
      const height = (worldStore.world.height ?? 0);

      const bounds: L.LatLngBoundsExpression = [[0, 0], [scale * height, scale * width]];
      const imageOverlay = L.imageOverlay(base64Image, bounds);

      imageOverlay.addTo(leafletMap.value);
      currentOverlay.value = imageOverlay;

      if (worldStore.world.siteMarkers != null) {
        const layersControl: Record<string, LayerGroup> = {};

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

                marker.bindPopup(`${siteMarker.name}<br>${siteMarker.typeAsString}<br><br>${siteMarker.owner}`);
                ownerLayers[ownerText].addLayer(marker); // Add the marker to the owner's layer
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
      }

      leafletMap.value.fitBounds(bounds);
    };

    watch(() => mapStore.worldMapMax, (newBase64Map) => {
      if (newBase64Map) {
        loadImageToMap(newBase64Map);
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
</style>