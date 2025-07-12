import axios from 'axios';
import {
  DatabaseConnectionRequest,
  DatabaseConnectionResponse,
  AIQueryRequest,
  AIQueryResponse,
  ChatRequest,
  ChatResponse,
  AvailableModels,
} from '@/types/api';

const API_BASE_URL = '/api';

// Create axios instance with default config
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// API functions
export const api = {
  // Database endpoints
  connectDatabase: async (request: DatabaseConnectionRequest): Promise<DatabaseConnectionResponse> => {
    const response = await apiClient.post<DatabaseConnectionResponse>('/database/connect', request);
    return response.data;
  },

  getSupportedDatabaseTypes: async (): Promise<string[]> => {
    const response = await apiClient.get<string[]>('/database/supported-types');
    return response.data;
  },

  // AI endpoints
  generateQuery: async (request: AIQueryRequest): Promise<AIQueryResponse> => {
    const response = await apiClient.post<AIQueryResponse>('/ai/query', request);
    return response.data;
  },

  chat: async (request: ChatRequest): Promise<ChatResponse> => {
    const response = await apiClient.post<ChatResponse>('/ai/chat', request);
    return response.data;
  },

  getAvailableModels: async (): Promise<AvailableModels> => {
    const response = await apiClient.get<AvailableModels>('/ai/models');
    return response.data;
  },
};

// Query keys for TanStack Query
export const queryKeys = {
  database: {
    all: ['database'] as const,
    connections: () => [...queryKeys.database.all, 'connections'] as const,
    supportedTypes: () => [...queryKeys.database.all, 'supported-types'] as const,
  },
  ai: {
    all: ['ai'] as const,
    models: () => [...queryKeys.ai.all, 'models'] as const,
    query: (request: AIQueryRequest) => [...queryKeys.ai.all, 'query', request] as const,
    chat: (request: ChatRequest) => [...queryKeys.ai.all, 'chat', request] as const,
  },
} as const;