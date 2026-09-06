import { defineStore } from 'pinia'
import client from "../apiClient"; // Import the global client
import { components } from '../generated/api-schema'; // Import from the OpenAPI schema
import { useWorldRecordsStore } from './worldRecordsStore';

export type Bookmark = components['schemas']['Bookmark'];

// Track in-flight requests to handle race conditions
let currentRequestId = 0;

export const useBookmarkStore = defineStore('bookmark', {
  state: () => ({
    bookmarks: [] as Bookmark[],
    bookmarkError: '' as string,
    bookmarkWarning: '' as string,
    isLoadingNewWorld: false as boolean
  }),
  getters: {
    isLoadingExistingWorld: (state) => {
      return state.bookmarks.some(bookmark => bookmark.state === 'Loading');
    },
    isLoading: (state) => {
      return state.isLoadingNewWorld || state.bookmarks.some(bookmark => bookmark.state === 'Loading');
    },
    isLoaded: (state) => {
      return state.bookmarks.some(bookmark => bookmark.state === 'Loaded');
    },
    currentWorld: (state) => {
      return state.bookmarks.find(bookmark => bookmark.state === 'Loaded');
    },
  },
  actions: {
    async loadByFullPath(filePath: string, latestTimestamp: string) {
      // Increment request ID for race condition handling
      const requestId = ++currentRequestId;

      // Set the state of the bookmark to 'Loading' if it exists
      let existingBookmark = this.bookmarks.find(bookmark => bookmark.filePath === filePath);
      if (existingBookmark) {
        existingBookmark.state = 'Loading';
      }
      else {
        this.isLoadingNewWorld = true;
      }

      const { data, error } = await client.POST("/api/Bookmark/loadByFullPath", {
        body: filePath.replace("{TIMESTAMP}", latestTimestamp), // Send the filePath as raw text
      });

      // Check if this response is still the latest request (race condition fix)
      if (requestId !== currentRequestId) {
        return; // Stale response, ignore it
      }

      if (error !== undefined) {
        console.error(error);
        let existingBookmark = this.bookmarks.find(bookmark => bookmark.filePath === filePath);
        if (existingBookmark) {
          existingBookmark.state = 'Default';
        }
        this.isLoadingNewWorld = false;
        this.bookmarkError = error.title ?? error.type ?? '';
      } else if (data) {
        const newBookmark = data as Bookmark;
        if (newBookmark.worldName == null || newBookmark.worldName.length == 0) {
          this.bookmarkWarning = 'The legends_plus.xml file was not found. Dwarf Fortress currently exports only a limited amount of legends data. To access more detailed information, including proper maps and other important features, please install DFHack, which will automatically export the additional data.'
        }
        // Check if the bookmark already exists (computed AFTER await)
        const index = this.bookmarks.findIndex(bookmark => bookmark.filePath === newBookmark.filePath);
        this.bookmarks.forEach(b => b.state = 'Default')
        if (index !== -1) {
          // Update the existing bookmark
          this.bookmarks[index] = newBookmark;
        } else {
          // Add the new bookmark to the array
          this.bookmarks.push(newBookmark);
        }

        useWorldRecordsStore().clearRecords();
        this.isLoadingNewWorld = false;
      } else {
        // Handle case where both error and data are undefined
        this.isLoadingNewWorld = false;
      }
    },
    async deleteByFullPath(filePath: string, latestTimestamp: string) {
      // Increment request ID for race condition handling
      const requestId = ++currentRequestId;

      // Set the state of the bookmark to 'Loading' if it exists
      let existingBookmark = this.bookmarks.find(bookmark => bookmark.filePath === filePath);
      if (existingBookmark) {
        existingBookmark.state = 'Loading';
      }
      else {
        this.isLoadingNewWorld = true;
      }

      const { data, error, response } = await client.DELETE("/api/Bookmark/{encodedFilePath}", {
        params: {
          path: {
            encodedFilePath: filePath.replace("{TIMESTAMP}", latestTimestamp)
          }
        },
      });

      // Check if this response is still the latest request (race condition fix)
      if (requestId !== currentRequestId) {
        return; // Stale response, ignore it
      }

      // Check for X-File-Missing header — the file was deleted or moved
      const fileMissing = response?.headers.get('X-File-Missing') === 'true';
      if (fileMissing) {
        this.bookmarkWarning = 'The file was not found. It may have been deleted or moved.';
        let existingBookmark = this.bookmarks.find(bookmark => bookmark.filePath === filePath);
        if (existingBookmark) {
          existingBookmark.state = 'Default';
        }
        this.isLoadingNewWorld = false;
        return;
      }

      if (error !== undefined) {
        console.error(error);
        let existingBookmark = this.bookmarks.find(bookmark => bookmark.filePath === filePath);
        if (existingBookmark) {
          existingBookmark.state = 'Default';
        }
        this.isLoadingNewWorld = false;
        this.bookmarkError = error.title ?? error.type ?? 'Failed to delete bookmark';
      } else if (data !== undefined) {
        const newBookmark = data as Bookmark | null | undefined;

        // Find index AFTER the await to avoid stale reference
        const index = this.bookmarks.findIndex(bookmark => bookmark.filePath === filePath);
        if (newBookmark != null && index !== -1) {
          // Update the existing bookmark
          this.bookmarks[index] = newBookmark;
        } else if (index !== -1) {
          // Remove the bookmark if newBookmark is null or undefined
          this.bookmarks.splice(index, 1);
        }
        // If index === -1, bookmark was already removed, nothing to do

        this.isLoadingNewWorld = false;
      } else {
        // Handle case where both error and data are undefined
        this.isLoadingNewWorld = false;
      }
    },
    async loadByFolderAndFile(folderPath: string, fileName: string) {
      // Increment request ID for race condition handling
      const requestId = ++currentRequestId;

      let fileNameWithoutTimestamp: string = '';
      let timestamp: string = '';
      const firstHyphenIndex: number = fileName.indexOf('-');

      if (firstHyphenIndex !== -1) {
        timestamp = fileName.substring(firstHyphenIndex + 1); // Extract timestamp part
        fileNameWithoutTimestamp = fileName.replace(timestamp, "{TIMESTAMP}")
      }

      // Set the state of the bookmark to 'Loading' if it exists
      let existingBookmark = this.bookmarks.find(bookmark => bookmark.filePath?.startsWith(folderPath) && bookmark.filePath?.endsWith(fileNameWithoutTimestamp));
      if (existingBookmark) {
        existingBookmark.state = 'Loading';
      }
      else {
        this.isLoadingNewWorld = true;
      }

      const { data, error } = await client.POST("/api/Bookmark/loadByFolderAndFile", {
        body: folderPath, // Send the filePath as raw text
        params: {
          query: {
            fileName: fileName
          }
        },
      });

      // Check if this response is still the latest request (race condition fix)
      if (requestId !== currentRequestId) {
        return; // Stale response, ignore it
      }

      if (error !== undefined) {
        console.error(error);
        // Reset bookmark state on error
        let existingBookmark = this.bookmarks.find(bookmark => bookmark.filePath?.startsWith(folderPath) && bookmark.filePath?.endsWith(fileNameWithoutTimestamp));
        if (existingBookmark) {
          existingBookmark.state = 'Default';
        }
        this.isLoadingNewWorld = false;
        this.bookmarkError = error.title ?? error.type ?? 'Failed to load world';
      } else if (data) {
        const newBookmark = data as Bookmark;

        // Check if the bookmark already exists (computed AFTER await)
        const index = this.bookmarks.findIndex(bookmark => bookmark.filePath === newBookmark.filePath);
        this.bookmarks.forEach(b => b.state = 'Default')

        if (index !== -1) {
          // Update the existing bookmark
          this.bookmarks[index] = newBookmark;
        } else {
          // Add the new bookmark to the array
          this.bookmarks.push(newBookmark);
        }

        useWorldRecordsStore().clearRecords();
        this.isLoadingNewWorld = false;
      } else {
        // Handle case where both error and data are undefined
        this.isLoadingNewWorld = false;
      }
    },
    async getAll() {
      const { data, error } = await client.GET("/api/Bookmark");
      if (error !== undefined) {
        console.error(error)
      } else {
        this.bookmarks = data as Bookmark[]
      }
    },
  },
})