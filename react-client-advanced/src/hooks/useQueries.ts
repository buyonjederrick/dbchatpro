import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api, queryKeys } from '@/services/api';
import { DatabaseConnectionRequest, AIQueryRequest, ChatRequest } from '@/types/api';
import toast from 'react-hot-toast';

// Database queries
export const useSupportedDatabaseTypes = () => {
  return useQuery({
    queryKey: queryKeys.database.supportedTypes(),
    queryFn: api.getSupportedDatabaseTypes,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

export const useConnectDatabase = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: api.connectDatabase,
    onSuccess: (data) => {
      if (data.isConnected) {
        toast.success(`Successfully connected to ${data.name}`);
        queryClient.invalidateQueries({ queryKey: queryKeys.database.connections() });
      } else {
        toast.error(data.errorMessage || 'Failed to connect to database');
      }
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.errorMessage || 'Failed to connect to database');
    },
  });
};

// AI queries
export const useAvailableModels = () => {
  return useQuery({
    queryKey: queryKeys.ai.models(),
    queryFn: api.getAvailableModels,
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
};

export const useGenerateQuery = () => {
  return useMutation({
    mutationFn: api.generateQuery,
    onSuccess: (data) => {
      if (data.errorMessage) {
        toast.error(data.errorMessage);
      } else {
        toast.success('Query generated successfully');
      }
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.errorMessage || 'Failed to generate query');
    },
  });
};

export const useChat = () => {
  return useMutation({
    mutationFn: api.chat,
    onError: (error: any) => {
      toast.error(error.response?.data?.errorMessage || 'Failed to send chat message');
    },
  });
};