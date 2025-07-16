<!--suppress ALL -->
<script setup lang="ts">
import {computed, onUnmounted, ref} from 'vue';
import { useBookmarkStore } from '../stores/bookmarkStore';
import {dfDirectoryStorageKey, useFileSystemStore} from '../stores/fileSystemStore';

const bookmarkStore = useBookmarkStore()
const fileSystemStore = useFileSystemStore()
bookmarkStore.getAll()
fileSystemStore.initialize();

const unsubscribe = fileSystemStore.$subscribe((_, state) => {
  const newStateCurrentDirectory = state.filesAndSubdirectories.currentDirectory
  if (newStateCurrentDirectory) {
    localStorage.setItem(dfDirectoryStorageKey, newStateCurrentDirectory);
  }
})

onUnmounted(() => {
  unsubscribe()
})

const fileName = ref<string>('')

// Function to prepare a proper base64 string for png images
const getImageData = (bookmark: any) => {
  if (!bookmark.worldMapImage) {
    return ''; // Return an empty string if there's no image data
  }
  return `data:image/png;base64,${bookmark.worldMapImage}`;
}

const readFromClipboard = async () => {
  try {
    // Read the text from the clipboard
    let clipboardText = await navigator.clipboard.readText();

    console.log('Clipboard Text:', clipboardText);

    if (clipboardText.includes("\"")) {
      clipboardText = clipboardText.replace("\"", "").replace("\"", "")
      console.log('Clipboard Text:', clipboardText);
    }

    // Regular expression to check if the path ends with .xml
    const xmlFileRegex = /^(.*[\\/])?([^\\/]+\.xml)$/i;

    // Check if the text from the clipboard is a valid path that ends with .xml
    const match = clipboardText.match(xmlFileRegex);

    if (match) {
      const fullPath = match[0]; // Full path
      const filename = match[2]; // Filename (the second capture group from the regex)

      console.log('Full path:', fullPath);
      console.log('Filename:', filename);

      fileSystemStore.loadDirectory(fullPath)
      fileName.value = filename
    } else {
      fileSystemStore.loadDirectory(clipboardText)
    }
  } catch (err) {
    console.error('Failed to read from clipboard:', err);
  }
}

const isDialogVisible = computed({
  get() {
    return bookmarkStore.bookmarkWarning != null && bookmarkStore.bookmarkWarning.length > 0;
  },
  set(value: boolean) {
    if (!value) {
      bookmarkStore.bookmarkWarning = ''; // Clear bookmarkWarning on close
    }
  },
});

const closeDialog = () => {
  isDialogVisible.value = false; // Close the dialog and clear the warning
};

const isSnackbarVisible = computed({
  get() {
    return bookmarkStore.bookmarkError != null && bookmarkStore.bookmarkError.length > 0;
  },
  set(value: boolean) {
    if (!value) {
      bookmarkStore.bookmarkError = ''; // Clear bookmarkError on close
    }
  },
});

const closeSnackbar = () => {
  isSnackbarVisible.value = false; // Close the snackbar and clear the error
};

</script>

<template>
  <v-row dense>
    <v-col cols="12" md="3">
      <v-card class="mx-auto" max-width="320">
        <v-container>
          <v-icon icon="mdi-earth-box-plus" size="300"></v-icon>
        </v-container>

        <v-card-title>
          Explore a new world
        </v-card-title>

        <v-card-subtitle>
          Select an exported legends XML file
        </v-card-subtitle>

        <v-card-actions>
          <v-dialog width="auto" min-width="480">
            <template v-slot:activator="{ props: activatorProps }">
              <v-btn color="orange-lighten-2" prepend-icon="mdi-earth" text="Select" variant="tonal" class="ml-1"
                v-bind="activatorProps" :disabled="bookmarkStore.isLoading" :loading="bookmarkStore.isLoading"></v-btn>
            </template>

            <template v-slot:default="{ isActive }">
              <v-card prepend-icon="mdi-earth" title="Select World Export">
                <v-card-text class="px-4" style="max-width: 720px;">
                  <v-alert type="info" variant="tonal" style="margin-bottom: 16px;">
                    This file list shows only the main export file (e.g., &lt;savename&gt;-&lt;timestamp&gt;-legends.xml). 
                    If other files related to the same export are present,
                    they will be automatically detected and included when you select the main file.
                  </v-alert> 

                  <v-form>
                    <v-text-field v-model="fileSystemStore.filesAndSubdirectories.currentDirectory" readonly
                      label="Current Folder">
                      <template v-slot:append>
                        <v-btn aria-label="Copy path from clipboard" icon="mdi-clipboard-outline"
                          @click="readFromClipboard()">
                        </v-btn>
                      </template> </v-text-field>
                    <v-text-field v-model="fileName" readonly label="File Name"></v-text-field>

                  </v-form>

                  <v-list density="compact" height="220" scrollable>
                    <v-list-subheader>Directories</v-list-subheader>
                    <v-list-item v-if="fileSystemStore.filesAndSubdirectories.currentDirectory != '/'"
                      @click="fileSystemStore.loadDirectory(fileSystemStore.filesAndSubdirectories.parentDirectory ?? '/')"
                      color="primary" variant="plain">
                      <template v-slot:prepend>
                        <v-icon icon="mdi-folder-outline"></v-icon>
                      </template>

                      <v-list-item-title v-text="'..'"></v-list-item-title>
                    </v-list-item>

                    <v-list-item v-for="(item, i) in fileSystemStore.filesAndSubdirectories.subdirectories" :key="i"
                      :value="item"
                      @click="fileSystemStore.loadSubDirectory(fileSystemStore.filesAndSubdirectories.currentDirectory ?? '/', item); fileName = ''"
                      color="primary" variant="plain">
                      <template v-slot:prepend>
                        <v-icon icon="mdi-folder-outline"></v-icon>
                      </template>

                      <v-list-item-title v-text="item"></v-list-item-title>
                    </v-list-item>
                  </v-list>

                  <v-list density="compact" min-height="220">
                    <v-list-subheader>World Exports</v-list-subheader>
                    <v-list-item v-for="(item, i) in fileSystemStore.filesAndSubdirectories.files" :key="i"
                      :value="item" color="primary" variant="plain">
                      <template v-slot:prepend>
                        <v-icon icon="mdi-file-xml-box"></v-icon>
                      </template>

                      <v-list-item-title v-text="item" @click="fileName = item">
                      </v-list-item-title>
                    </v-list-item>
                  </v-list>

                </v-card-text>

                <v-divider></v-divider>

                <v-card-actions>
                  <v-btn text="Close" @click="isActive.value = false"></v-btn>

                  <v-spacer></v-spacer>

                  <v-btn color="surface-variant" text="Load World" variant="flat"
                    :disabled="fileName == null || fileName == ''"
                    @click="bookmarkStore.loadByFolderAndFile(fileSystemStore.filesAndSubdirectories.currentDirectory ?? '/', fileName); isActive.value = false;"></v-btn>
                </v-card-actions>
              </v-card>
            </template>
          </v-dialog>
          <v-spacer></v-spacer>

        </v-card-actions>
      </v-card>
    </v-col>
    <template v-for="(bookmark, i) in bookmarkStore.bookmarks.slice().reverse()" :key="i">
      <v-col v-if="bookmark != null && bookmark.filePath" :for="i" cols="12" md="3">
        <v-card class="mx-auto" max-width="320">
          <v-container>
            <v-img height="300px" width="300px" class="pixelated-image" :src="getImageData(bookmark)"></v-img>
          </v-container>

          <v-card-title>
            {{ (bookmark.worldName != null && bookmark.worldName.length > 0 ?
              bookmark.worldName :
              bookmark.worldRegionName)
            }}
            <v-chip class="float-right">
              {{ bookmark.worldWidth + " x " + bookmark.worldHeight }}
            </v-chip>
          </v-card-title>

          <v-card-subtitle>
            {{ (bookmark.worldAlternativeName != null && bookmark.worldAlternativeName.length > 0 ?
              bookmark.worldAlternativeName :
              '-')
            }}
          </v-card-subtitle>

          <v-card-actions>
            <v-btn
              v-if="bookmark.filePath && bookmark.state !== 'Loaded' || bookmark.latestTimestamp !== bookmark.loadedTimestamp"
              :loading="bookmark.state === 'Loading'" color="blue" text="Load" :disabled="bookmarkStore.isLoading"
              variant="tonal" class="ml-1"
              @click="bookmarkStore.loadByFullPath(bookmark.filePath ?? '', bookmark.latestTimestamp ?? '')">
            </v-btn>
            <v-btn
              v-if="bookmark.filePath && bookmark.state === 'Loaded' && bookmark.latestTimestamp === bookmark.loadedTimestamp"
              color="green-lighten-2" text="Explore" variant="tonal" class="ml-1" :disabled="bookmarkStore.isLoading"
              to="/world">
            </v-btn>
            <v-menu
              v-if="bookmark.filePath && bookmark.state !== 'Loaded' || bookmark.latestTimestamp !== bookmark.loadedTimestamp"
              :disabled="bookmarkStore.isLoading" transition="slide-x-transition">
              <template v-slot:activator="{ props }">
                <v-btn v-bind="props" icon="mdi-dots-horizontal" variant="plain" density="compact"></v-btn>
              </template>

              <v-list>
                <v-list-item :disabled="bookmarkStore.isLoading"
                  @click="bookmarkStore.deleteByFullPath(bookmark.filePath ?? '', bookmark.latestTimestamp ?? '')">
                  <v-list-item-title>
                    <v-icon class="mt-n1" color="error" icon="mdi-delete-outline"></v-icon>
                    Delete Bookmark
                  </v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
            <v-spacer></v-spacer>
            <v-menu transition="slide-x-transition">
              <template v-slot:activator="{ props }">
                <v-btn v-bind="props" variant="text"
                  :disabled="bookmark.worldTimestamps == null || bookmark.worldTimestamps.length <= 1">
                  {{ bookmark.latestTimestamp }}
                  <template v-if="bookmark.worldTimestamps != null && bookmark.worldTimestamps.length > 1"
                    v-slot:append>
                    <v-icon icon="mdi-menu-down"></v-icon>
                  </template>
                </v-btn>
              </template>

              <v-list>
                <v-list-item v-for="(item, i) in bookmark.worldTimestamps ?? []" :key="i"
                  @click="bookmark.latestTimestamp = item">
                  <v-list-item-title>{{ item }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
            <!-- <v-combobox v-model="bookmark.latestTimestamp" :items="bookmark.worldTimestamps ?? []" density="compact"
            label="Timestamps" width="160" :disabled="bookmarkStore.isLoading"></v-combobox> -->
          </v-card-actions>
        </v-card>
      </v-col>
    </template>
  </v-row>
  <v-dialog v-model="isDialogVisible" transition="dialog-top-transition" width="500px">
    <v-card max-width="400" prepend-icon="mdi-alert-outline" title="Warning" :text="bookmarkStore.bookmarkWarning">
      <template v-slot:actions>
        <v-btn class="ms-auto" text="Ok" @click="closeDialog"></v-btn>
      </template>
    </v-card>
  </v-dialog>
  <v-snackbar v-model="isSnackbarVisible" multi-line top color="error">
    {{ bookmarkStore.bookmarkError }}

    <template v-slot:actions>
      <v-btn color="black" variant="tonal" @click="closeSnackbar">
        Close
      </v-btn>
    </template>
  </v-snackbar>
</template>
