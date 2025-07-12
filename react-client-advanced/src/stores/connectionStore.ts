import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { DatabaseConnectionResponse, ConnectionStore } from '@/types/api';

export const useConnectionStore = create<ConnectionStore>()(
  persist(
    (set, get) => ({
      connections: [],
      selectedConnection: null,

      addConnection: (connection: DatabaseConnectionResponse) => {
        set((state) => ({
          connections: [...state.connections.filter(c => c.name !== connection.name), connection],
        }));
      },

      removeConnection: (name: string) => {
        set((state) => ({
          connections: state.connections.filter(c => c.name !== name),
          selectedConnection: state.selectedConnection?.name === name ? null : state.selectedConnection,
        }));
      },

      selectConnection: (connection: DatabaseConnectionResponse) => {
        set({ selectedConnection: connection });
      },

      clearConnections: () => {
        set({ connections: [], selectedConnection: null });
      },
    }),
    {
      name: 'dbchatpro-connections',
    }
  )
);