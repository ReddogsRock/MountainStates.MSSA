// IndexedDB and LocalStorage wrapper for MSSA PWA
// Provides offline storage capabilities for trial entries

window.mssaStorage = {
    dbName: 'MSSA_PWA_DB',
    dbVersion: 1,
    db: null,

    // Initialize IndexedDB
    async initDB() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, this.dbVersion);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                this.db = request.result;
                resolve();
            };

            request.onupgradeneeded = (event) => {
                const db = event.target.result;

                // Create object stores if they don't exist
                if (!db.objectStoreNames.contains('entries')) {
                    const entryStore = db.createObjectStore('entries', { keyPath: 'tempId' });
                    entryStore.createIndex('trialId', 'trialId', { unique: false });
                    entryStore.createIndex('synced', 'synced', { unique: false });
                }

                if (!db.objectStoreNames.contains('trials')) {
                    db.createObjectStore('trials', { keyPath: 'trialId' });
                }

                if (!db.objectStoreNames.contains('handlers')) {
                    db.createObjectStore('handlers', { keyPath: 'handlerId' });
                }

                if (!db.objectStoreNames.contains('dogs')) {
                    db.createObjectStore('dogs', { keyPath: 'dogId' });
                }

                if (!db.objectStoreNames.contains('classes')) {
                    db.createObjectStore('classes', { keyPath: 'classId' });
                }
            };
        });
    },

    // Add entry to queue (offline entries waiting to sync)
    async addEntry(entry) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['entries'], 'readwrite');
            const store = transaction.objectStore('entries');

            // Add timestamp and synced flag
            entry.createdDate = new Date().toISOString();
            entry.synced = false;

            const request = store.add(entry);

            request.onsuccess = () => resolve(entry.tempId);
            request.onerror = () => reject(request.error);
        });
    },

    // Get all unsynced entries
    async getUnsyncedEntries() {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['entries'], 'readonly');
            const store = transaction.objectStore('entries');
            const results = [];

            const request = store.openCursor();

            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    // Only include entries where synced is false
                    if (cursor.value.synced === false) {
                        results.push(cursor.value);
                    }
                    cursor.continue();
                } else {
                    // Done iterating
                    resolve(results);
                }
            };

            request.onerror = () => reject(request.error);
        });
    },

    // Get all entries for a specific trial
    async getEntriesByTrial(trialId) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['entries'], 'readonly');
            const store = transaction.objectStore('entries');
            const results = [];

            const request = store.openCursor();

            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    // Only include entries for this trial
                    if (cursor.value.trialId === trialId) {
                        results.push(cursor.value);
                    }
                    cursor.continue();
                } else {
                    // Done iterating
                    resolve(results);
                }
            };

            request.onerror = () => reject(request.error);
        });
    },

    // Mark entry as synced
    async markEntrySynced(tempId) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['entries'], 'readwrite');
            const store = transaction.objectStore('entries');
            const getRequest = store.get(tempId);

            getRequest.onsuccess = () => {
                const entry = getRequest.result;
                if (entry) {
                    entry.synced = true;
                    entry.syncedDate = new Date().toISOString();
                    const updateRequest = store.put(entry);
                    updateRequest.onsuccess = () => resolve();
                    updateRequest.onerror = () => reject(updateRequest.error);
                } else {
                    resolve(); // Entry not found, that's okay
                }
            };
            getRequest.onerror = () => reject(getRequest.error);
        });
    },

    // Delete entry (after successful sync)
    async deleteEntry(tempId) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['entries'], 'readwrite');
            const store = transaction.objectStore('entries');
            const request = store.delete(tempId);

            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    },

    // Cache trial data for offline use
    async cacheTrial(trial) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['trials'], 'readwrite');
            const store = transaction.objectStore('trials');
            const request = store.put(trial);

            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    },

    // Get cached trial
    async getCachedTrial(trialId) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['trials'], 'readonly');
            const store = transaction.objectStore('trials');
            const request = store.get(trialId);

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    // Cache handlers for offline search
    async cacheHandlers(handlers) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['handlers'], 'readwrite');
            const store = transaction.objectStore('handlers');

            // Clear existing handlers first
            const clearRequest = store.clear();
            clearRequest.onsuccess = () => {
                // Add all handlers
                handlers.forEach(handler => store.put(handler));
                resolve();
            };
            clearRequest.onerror = () => reject(clearRequest.error);
        });
    },

    // Get all cached handlers
    async getCachedHandlers() {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['handlers'], 'readonly');
            const store = transaction.objectStore('handlers');
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    // Cache dogs for offline selection
    async cacheDogs(dogs) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['dogs'], 'readwrite');
            const store = transaction.objectStore('dogs');

            const clearRequest = store.clear();
            clearRequest.onsuccess = () => {
                dogs.forEach(dog => store.put(dog));
                resolve();
            };
            clearRequest.onerror = () => reject(clearRequest.error);
        });
    },

    // Get all cached dogs
    async getCachedDogs() {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['dogs'], 'readonly');
            const store = transaction.objectStore('dogs');
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    // Cache classes for offline selection
    async cacheClasses(classes) {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['classes'], 'readwrite');
            const store = transaction.objectStore('classes');

            const clearRequest = store.clear();
            clearRequest.onsuccess = () => {
                classes.forEach(cls => store.put(cls));
                resolve();
            };
            clearRequest.onerror = () => reject(clearRequest.error);
        });
    },

    // Get all cached classes
    async getCachedClasses() {
        if (!this.db) await this.initDB();

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(['classes'], 'readonly');
            const store = transaction.objectStore('classes');
            const request = store.getAll();

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    // LocalStorage helpers (for simple key-value storage)
    setLocal(key, value) {
        localStorage.setItem(key, JSON.stringify(value));
    },

    getLocal(key) {
        const value = localStorage.getItem(key);
        return value ? JSON.parse(value) : null;
    },

    removeLocal(key) {
        localStorage.removeItem(key);
    },

    // Get queue count (for status display)
    async getQueueCount() {
        const entries = await this.getUnsyncedEntries();
        return entries.length;
    }
};

// Initialize on load
window.mssaStorage.initDB();