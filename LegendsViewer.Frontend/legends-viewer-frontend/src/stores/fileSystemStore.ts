import { defineStore } from 'pinia';
import client from "../apiClient"; // Import the global client
import { components } from '../generated/api-schema'; // Import from the OpenAPI schema

export type FilesAndSubdirectories = components['schemas']['FilesAndSubdirectoriesDto'];

export const dfDirectoryStorageKey = 'df-directory'

export const useFileSystemStore = defineStore('fileSystem', {
  state: () => ({
    filesAndSubdirectories: {} as FilesAndSubdirectories,
    loading: false as boolean,
  }),

  actions: {
    async loadDirectory(path: string = "/") {
      // Set loading state to true
      this.loading = true;

      try {
        // Fetch directory info from the backend
        // @ts-ignore
        const { data, error } = await client.GET('/api/FileSystem/{path}', {
          params: {
            path: { path: path },
          },
        });

        if (error) {
          console.error(error);
        } else if (data) {
          // Set the received data to the store state
          this.filesAndSubdirectories = data as FilesAndSubdirectories;
        }
      } catch (err) {
        console.error('Error loading directory:', err);
      } finally {
        // Set loading state to false after the operation
        this.loading = false;
      }
    },
    async loadSubDirectory(currentPath: string = "/", subFolder: string = "/") {
      // Set loading state to true
      this.loading = true;

      try {
        // Fetch directory info from the backend
        // @ts-ignore
        const { data, error } = await client.GET('/api/FileSystem/{currentPath}/{subFolder}', {
          params: {
            path:
            {
              currentPath: encodeURIComponent(currentPath),
              subFolder: encodeURIComponent(subFolder)
            },
          },
        });

        if (error) {
          console.error(error);
        } else if (data) {
          // Set the received data to the store state
          this.filesAndSubdirectories = data as FilesAndSubdirectories;
        }
      } catch (err) {
        console.error('Error loading directory:', err);
      } finally {
        // Set loading state to false after the operation
        this.loading = false;
      }
    },

    async initialize() {
      // Set loading state to true
      this.loading = true;

      try {
        // Fetch from the stored path if there is one, else filesystem root
        const storedPath = localStorage.getItem(dfDirectoryStorageKey)
        // Fetch directory info from the backend
        // @ts-ignore
        const { data, error } = await client.GET(`/api/FileSystem/{path}`, {
          params: {
            path : {
              path: storedPath ? encodeURIComponent(storedPath) : ""
            }
          }
        });

        if (error) {
          console.error(error);
        } else if (data) {
          // Set the received data to the store state
          this.filesAndSubdirectories = data as FilesAndSubdirectories;
        }
      } catch (err) {
        console.error('Error loading directory:', err);
      } finally {
        // Set loading state to false after the operation
        this.loading = false;
      }
    },
  },
});
