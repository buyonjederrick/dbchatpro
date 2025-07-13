import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-hot-toast';
import { api, queryKeys, handleAPIError } from '@/services/api';
import type {
  AuditLogFilters,
  SystemConfigurationRequest,
  CreateSessionRequest,
  UpdateSessionRequest,
  MCPQueryRequest,
} from '@/types/api';

// Audit Logs Hook
export const useAuditLogs = (filters: AuditLogFilters = {}) => {
  return useQuery({
    queryKey: queryKeys.enterprise.auditLogs(filters),
    queryFn: () => api.getAuditLogs(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
    refetchInterval: 30 * 1000, // Refetch every 30 seconds for real-time updates
  });
};

// Metrics Hook
export const useMetrics = () => {
  return useQuery({
    queryKey: queryKeys.enterprise.metrics(),
    queryFn: () => api.getMetrics(),
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // Refetch every minute
  });
};

// System Configuration Hooks
export const useSystemConfigurations = (category?: string) => {
  return useQuery({
    queryKey: queryKeys.enterprise.systemConfig(category),
    queryFn: () => api.getSystemConfigurations(category),
    staleTime: 10 * 60 * 1000, // 10 minutes
  });
};

export const useSetSystemConfiguration = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (request: SystemConfigurationRequest) => api.setSystemConfiguration(request),
    onSuccess: (data: any, variables: SystemConfigurationRequest) => {
      toast.success('System configuration updated successfully');
      // Invalidate and refetch system configurations
      queryClient.invalidateQueries({ queryKey: queryKeys.enterprise.systemConfig(variables.category) });
    },
    onError: (error: any) => {
      const apiError = handleAPIError(error);
      toast.error(`Failed to update system configuration: ${apiError.message}`);
    },
  });
};

// Session Management Hooks
export const useCreateSession = () => {
  return useMutation({
    mutationFn: (request: CreateSessionRequest) => api.createSession(request),
    onSuccess: (data: any) => {
      // Store session token
      localStorage.setItem('sessionToken', data.sessionId);
      toast.success('Session created successfully');
    },
    onError: (error: any) => {
      const apiError = handleAPIError(error);
      toast.error(`Failed to create session: ${apiError.message}`);
    },
  });
};

export const useUpdateSession = () => {
  return useMutation({
    mutationFn: ({ sessionId, request }: { sessionId: string; request: UpdateSessionRequest }) =>
      api.updateSession(sessionId, request),
    onSuccess: () => {
      toast.success('Session updated successfully');
    },
    onError: (error: any) => {
      const apiError = handleAPIError(error);
      toast.error(`Failed to update session: ${apiError.message}`);
    },
  });
};

export const useValidateSession = (sessionId: string) => {
  return useQuery({
    queryKey: queryKeys.session.validate(sessionId),
    queryFn: () => api.validateSession(sessionId),
    enabled: !!sessionId,
    refetchInterval: 5 * 60 * 1000, // Check every 5 minutes
    retry: false, // Don't retry on failure
  });
};

// MCP Hooks
export const useMCPStatus = () => {
  return useQuery({
    queryKey: queryKeys.enterprise.mcpStatus(),
    queryFn: () => api.getMCPStatus(),
    staleTime: 10 * 1000, // 10 seconds
    refetchInterval: 30 * 1000, // Refetch every 30 seconds
  });
};

export const useExecuteMCPQuery = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (request: MCPQueryRequest) => api.executeMCPQuery(request),
    onSuccess: (data: any) => {
      toast.success('MCP query executed successfully');
      // Invalidate metrics to update stats
      queryClient.invalidateQueries({ queryKey: queryKeys.enterprise.metrics() });
    },
    onError: (error: any) => {
      const apiError = handleAPIError(error);
      toast.error(`Failed to execute MCP query: ${apiError.message}`);
    },
  });
};

// Enterprise Dashboard Hook
export const useEnterpriseDashboard = () => {
  const metrics = useMetrics();
  const mcpStatus = useMCPStatus();
  
  return {
    metrics: metrics.data,
    mcpStatus: mcpStatus.data,
    isLoading: metrics.isLoading || mcpStatus.isLoading,
    error: metrics.error || mcpStatus.error,
  };
};

// Real-time Monitoring Hook
export const useRealTimeMonitoring = () => {
  const metrics = useMetrics();
  const auditLogs = useAuditLogs({ pageSize: 10 }); // Recent logs
  
  return {
    metrics: metrics.data,
    recentAuditLogs: auditLogs.data?.data || [],
    isLoading: metrics.isLoading || auditLogs.isLoading,
    error: metrics.error || auditLogs.error,
  };
};