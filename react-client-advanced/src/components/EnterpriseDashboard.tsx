import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { 
  Activity, 
  Database, 
  Shield, 
  Settings, 
  Clock, 
  Users, 
  TrendingUp,
  AlertCircle,
  CheckCircle,
  XCircle
} from 'lucide-react';
import { useEnterpriseDashboard, useRealTimeMonitoring, useAuditLogs } from '@/hooks/useEnterprise';
import { AuditLogFilters } from '@/types/api';
import { format } from 'date-fns';

export const EnterpriseDashboard: React.FC = () => {
  const [auditFilters, setAuditFilters] = useState<AuditLogFilters>({
    page: 1,
    pageSize: 20,
  });

  const { metrics, mcpStatus, isLoading, error } = useEnterpriseDashboard();
  const { recentAuditLogs } = useRealTimeMonitoring();
  const { data: auditLogs } = useAuditLogs(auditFilters);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="text-lg font-semibold">Error loading dashboard</h3>
          <p className="text-muted-foreground">Please try again later</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Enterprise Dashboard</h1>
          <p className="text-muted-foreground">
            Real-time monitoring and system overview
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Badge variant={mcpStatus?.isConnected ? 'default' : 'destructive'}>
            {mcpStatus?.isConnected ? (
              <>
                <CheckCircle className="h-3 w-3 mr-1" />
                MCP Connected
              </>
            ) : (
              <>
                <XCircle className="h-3 w-3 mr-1" />
                MCP Disconnected
              </>
            )}
          </Badge>
        </div>
      </div>

      {/* Metrics Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Queries</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {metrics?.mcpMetrics.totalQueries || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              +{metrics?.mcpMetrics.successfulQueries || 0} successful
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Connections</CardTitle>
            <Database className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {metrics?.connections.active || 0}
            </div>
            <p className="text-xs text-muted-foreground">
              of {metrics?.connections.total || 0} total
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Avg Response Time</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {metrics?.mcpMetrics.averageResponseTime?.toFixed(2) || 0}ms
            </div>
            <p className="text-xs text-muted-foreground">
              Last query: {metrics?.mcpMetrics.lastQueryAt ? 
                format(new Date(metrics.mcpMetrics.lastQueryAt), 'HH:mm') : 'N/A'}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">System Status</CardTitle>
            <Shield className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-green-600">
              Operational
            </div>
            <p className="text-xs text-muted-foreground">
              All systems normal
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Detailed Views */}
      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="audit">Audit Logs</TabsTrigger>
          <TabsTrigger value="mcp">MCP Status</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            {/* Weekly Stats Chart */}
            <Card>
              <CardHeader>
                <CardTitle>Weekly Activity</CardTitle>
                <CardDescription>Query activity over the past week</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {metrics?.weeklyStats?.map((stat, index) => (
                    <div key={index} className="flex items-center justify-between">
                      <span className="text-sm">{stat.date}</span>
                      <div className="flex items-center space-x-2">
                        <span className="text-sm">{stat.count} queries</span>
                        <Badge variant="outline" className="text-xs">
                          {stat.successful} successful
                        </Badge>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Recent Activity */}
            <Card>
              <CardHeader>
                <CardTitle>Recent Activity</CardTitle>
                <CardDescription>Latest system events</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {recentAuditLogs.slice(0, 5).map((log) => (
                    <div key={log.id} className="flex items-center space-x-2">
                      <div className="w-2 h-2 bg-primary rounded-full"></div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium truncate">
                          {log.action}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {log.userName} • {format(new Date(log.timestamp), 'MMM dd, HH:mm')}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="audit" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Audit Logs</CardTitle>
              <CardDescription>System activity and user actions</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {/* Filters */}
                <div className="flex flex-wrap gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setAuditFilters({ ...auditFilters, action: 'QUERY_EXECUTED' })}
                  >
                    Queries
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setAuditFilters({ ...auditFilters, action: 'CONNECTION_CREATED' })}
                  >
                    Connections
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setAuditFilters({ ...auditFilters, action: 'SYSTEM_CONFIG_UPDATED' })}
                  >
                    Config Changes
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setAuditFilters({ page: 1, pageSize: 20 })}
                  >
                    Clear Filters
                  </Button>
                </div>

                {/* Audit Logs Table */}
                <div className="space-y-2">
                  {auditLogs?.data?.map((log) => (
                    <div key={log.id} className="flex items-center justify-between p-3 border rounded-lg">
                      <div className="flex items-center space-x-3">
                        <Badge variant="outline">{log.action}</Badge>
                        <div>
                          <p className="text-sm font-medium">{log.userName || 'System'}</p>
                          <p className="text-xs text-muted-foreground">
                            {log.resourceType} • {format(new Date(log.timestamp), 'MMM dd, yyyy HH:mm:ss')}
                          </p>
                        </div>
                      </div>
                      {log.details && (
                        <p className="text-xs text-muted-foreground max-w-xs truncate">
                          {log.details}
                        </p>
                      )}
                    </div>
                  ))}
                </div>

                {/* Pagination */}
                {auditLogs && auditLogs.totalPages > 1 && (
                  <div className="flex items-center justify-between">
                    <p className="text-sm text-muted-foreground">
                      Showing {auditLogs.data.length} of {auditLogs.totalCount} logs
                    </p>
                    <div className="flex space-x-2">
                      <Button
                        variant="outline"
                        size="sm"
                        disabled={auditFilters.page === 1}
                        onClick={() => setAuditFilters({ ...auditFilters, page: (auditFilters.page || 1) - 1 })}
                      >
                        Previous
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        disabled={auditFilters.page === auditLogs.totalPages}
                        onClick={() => setAuditFilters({ ...auditFilters, page: (auditFilters.page || 1) + 1 })}
                      >
                        Next
                      </Button>
                    </div>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="mcp" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>MCP Connection Status</CardTitle>
              <CardDescription>Model Context Protocol server status and information</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">Connection Status</span>
                  <Badge variant={mcpStatus?.isConnected ? 'default' : 'destructive'}>
                    {mcpStatus?.isConnected ? 'Connected' : 'Disconnected'}
                  </Badge>
                </div>

                {mcpStatus?.serverInfo && (
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium">Server Name</span>
                      <span className="text-sm">{mcpStatus.serverInfo.name}</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium">Version</span>
                      <span className="text-sm">{mcpStatus.serverInfo.version}</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium">Capabilities</span>
                      <span className="text-sm">{mcpStatus.serverInfo.capabilities.length}</span>
                    </div>
                  </div>
                )}

                {mcpStatus?.lastConnectedAt && (
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium">Last Connected</span>
                    <span className="text-sm">
                      {format(new Date(mcpStatus.lastConnectedAt), 'MMM dd, yyyy HH:mm:ss')}
                    </span>
                  </div>
                )}

                {mcpStatus?.errorMessage && (
                  <div className="p-3 bg-destructive/10 border border-destructive/20 rounded-lg">
                    <p className="text-sm text-destructive">{mcpStatus.errorMessage}</p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};