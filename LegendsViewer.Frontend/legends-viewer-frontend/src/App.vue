<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useBookmarkStore } from './stores/bookmarkStore';
import { useVersionStore } from './stores/versionStore';
import { useFavoriteStore } from './stores/favoriteStore';

const router = useRouter();

const handleGlobalClick = (event: MouseEvent) => {
  const target = (event.target as HTMLElement)?.closest('a');
  if (!target) return;
  const href = target.getAttribute('href');
  if (href && href.startsWith('/') && !target.hasAttribute('target') && !event.ctrlKey && !event.metaKey && !event.shiftKey) {
    event.preventDefault();
    router.push(href);
  }
};

const versionStore = useVersionStore()
versionStore.loadVersion()
const bookmarkStore = useBookmarkStore()
bookmarkStore.getAll()
const favoriteStore = useFavoriteStore();

const societyItems = [
  { title: 'Factions and Groups', to: '/entity' },
  { title: 'Historical Figures', to: '/hf' },
];
const geographyItems = [
  { title: 'Regions', to: '/region' },
  { title: 'Underground', to: '/uregion' },
  { title: 'Landmasses', to: '/landmass' },
  { title: 'Rivers', to: '/river' },
  { title: 'Mountain Peaks', to: '/mountainpeak' },
];
const infrastructureItems = [
  { title: 'Sites', to: '/site' },
  { title: 'Structures', to: '/structure' },
  { title: 'Constructions', to: '/construction' },
];
const artItems = [
  { title: 'Artifacts', to: '/artifact' },
  { title: 'Dance Forms', to: '/danceform' },
  { title: 'Musical Forms', to: '/musicalform' },
  { title: 'Poetic Forms', to: '/poeticform' },
  { title: 'Written Content', to: '/writtencontent' },
];
const warfareItems = [
  { title: 'Wars', to: '/war' },
  { title: 'Battles', to: '/battle' },
  { title: 'Duels', to: '/duel' },
  { title: 'Raids', to: '/raid' },
  { title: 'Site Conquerings', to: '/siteconquered' },
];
const conflictsItems = [
  { title: 'Insurrections', to: '/insurrection' },
  { title: 'Persecutions', to: '/persecution' },
  { title: 'Purges', to: '/purge' },
  { title: 'Coups', to: '/coup' },
];
const calamitiesItems = [
  { title: 'Rampages', to: '/beastattack' },
  { title: 'Abductions', to: '/abduction' },
  { title: 'Thefts', to: '/theft' },
];
const ritualItems = [
  { title: 'Processions', to: '/procession' },
  { title: 'Performances', to: '/performance' },
  { title: 'Journeys', to: '/journey' },
  { title: 'Competitions', to: '/competition' },
  { title: 'Ceremonies', to: '/ceremony' },
  { title: 'Occasions', to: '/occasion' },
];
</script>

<template>
  <v-responsive class="border rounded" @click="handleGlobalClick">
    <v-app>
      <v-app-bar>
        <div class="logo">
          <v-img src="/ceretelina.png"></v-img>
        </div>
        <h1>
          Legends Viewer
        </h1>
        <div style="position: absolute; right: 30px;">
          <v-btn href="https://github.com/Kromtec/LegendsViewer-Next/releases" target="_blank" :aria-label="versionStore.version">
            <p class="mr-2 mt-1">{{ versionStore.version }}</p>
            <v-icon icon="mdi-github" size="24px"></v-icon>
          </v-btn>
        </div>
      </v-app-bar>

      <v-navigation-drawer>
        <v-list nav class="nav-list">
          <v-list-item prepend-icon="mdi-file-tree-outline" title="Explore Worlds" to="/"
            :active-class="'v-list-item--active'" />

          <v-list-group value="Favorites" v-if="favoriteStore.favorites.length > 0">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-star" title="Favorites"></v-list-item>
            </template>
            <template v-for="fav in favoriteStore.favorites" :key="fav.type + fav.id">
              <v-list-item 
                :title="fav.name"
                :to="`/${fav.type}/${fav.id}`" 
                :active-class="'v-list-item--active'"
                style="padding-inline-start: 12px !important"
              >
                <template v-slot:prepend>
                  <div v-if="fav.icon" v-html="fav.icon" style="display: flex; justify-content: center; align-items: center; margin-right: 8px;"></div>
                  <v-icon v-else icon="mdi-circle-small" class="mr-2"></v-icon>
                </template>
              </v-list-item>
            </template>
          </v-list-group>

          <v-list-item prepend-icon="mdi-earth-box" title="World" to="/world" :active-class="'v-list-item--active'"
            :disabled="bookmarkStore?.isLoaded == false" />
          <v-list-item prepend-icon="mdi-map-search-outline" title="Map" to="/map" :active-class="'v-list-item--active'"
            :disabled="bookmarkStore?.isLoaded == false" />
          <v-list-item prepend-icon="mdi-timelapse" title="Eras" to="/era" :active-class="'v-list-item--active'"
            :disabled="bookmarkStore?.isLoaded == false" />
          <v-list-group value="Society">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-account-group-outline" title="Society"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in societyItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
          <v-list-group value="Geography">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-island-variant" title="Geography"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in geographyItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
          <v-list-group value="Infrastructure">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-home-modern" title="Infrastructure"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in infrastructureItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
          <v-list-group value="Art and Craft">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-diamond-stone" title="Art and Craft"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in artItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
          <v-list-group value="Warfare">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-sword-cross" title="Warfare"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in warfareItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
          <v-list-group value="Conflicts">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-shield-alert-outline" title="Conflicts"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in conflictsItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
          <v-list-group value="Calamities">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-alert-circle-outline" title="Calamities"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in calamitiesItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
          <v-list-group value="Rituals">
            <template v-slot:activator="{ props }">
              <v-list-item v-bind="props" prepend-icon="mdi-candle" title="Rituals"
                :disabled="bookmarkStore?.isLoaded == false"></v-list-item>
            </template>
            <template v-for="(item, i) in ritualItems" :key="i">
              <v-list-item :value="item.title + i" :title="item.title" :to="item.to"
                :disabled="bookmarkStore?.isLoaded == false" />
            </template>
          </v-list-group>
        </v-list>
      </v-navigation-drawer>

      <v-main>
        <v-container>
          <RouterView />
        </v-container>
      </v-main>
    </v-app>
  </v-responsive>
</template>

<style scoped>
.logo {
  margin: 12px;
  width: 36px;
}
</style>
