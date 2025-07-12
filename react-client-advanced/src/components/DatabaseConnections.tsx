import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Trash2, Database, CheckCircle, XCircle, Eye } from 'lucide-react';
import { useConnectDatabase, useSupportedDatabaseTypes } from '@/hooks/useQueries';
import { useConnectionStore } from '@/stores/connectionStore';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { columns } from './ConnectionTableColumns';

const connectionSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  databaseType: z.string().min(1, 'Database type is required'),
  connectionString: z.string().min(1, 'Connection string is required'),
});

type ConnectionFormData = z.infer<typeof connectionSchema>;

export function DatabaseConnections() {
  const [showForm, setShowForm] = useState(false);
  const { connections, addConnection, removeConnection, selectConnection } = useConnectionStore();
  const { data: supportedTypes = [] } = useSupportedDatabaseTypes();
  const connectMutation = useConnectDatabase();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ConnectionFormData>({
    resolver: zodResolver(connectionSchema),
  });

  const onSubmit = async (data: ConnectionFormData) => {
    try {
      const result = await connectMutation.mutateAsync(data);
      if (result.isConnected) {
        addConnection(result);
        reset();
        setShowForm(false);
      }
    } catch (error) {
      // Error handling is done in the mutation
    }
  };

  const handleRemoveConnection = (name: string) => {
    removeConnection(name);
  };

  const handleSelectConnection = (connection: any) => {
    selectConnection(connection);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Database Connections</h1>
          <p className="text-muted-foreground">
            Manage your database connections and view schemas
          </p>
        </div>
        <Button onClick={() => setShowForm(!showForm)}>
          <Plus className="mr-2 h-4 w-4" />
          Add Connection
        </Button>
      </div>

      {/* Connection Form */}
      {showForm && (
        <div className="bg-card border rounded-lg p-6">
          <h2 className="text-lg font-semibold mb-4">New Connection</h2>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">
                  Connection Name
                </label>
                <input
                  {...register('name')}
                  className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                  placeholder="My Database"
                />
                {errors.name && (
                  <p className="text-sm text-destructive mt-1">{errors.name.message}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-foreground mb-1">
                  Database Type
                </label>
                <select
                  {...register('databaseType')}
                  className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                >
                  <option value="">Select database type</option>
                  {supportedTypes.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
                {errors.databaseType && (
                  <p className="text-sm text-destructive mt-1">{errors.databaseType.message}</p>
                )}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">
                Connection String
              </label>
              <textarea
                {...register('connectionString')}
                rows={3}
                className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                placeholder="Data Source=localhost;Initial Catalog=mydb;Trusted_Connection=True;"
              />
              {errors.connectionString && (
                <p className="text-sm text-destructive mt-1">{errors.connectionString.message}</p>
              )}
            </div>

            <div className="flex gap-2">
              <Button
                type="submit"
                disabled={connectMutation.isPending}
                className="flex items-center"
              >
                {connectMutation.isPending ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
                ) : (
                  <Database className="mr-2 h-4 w-4" />
                )}
                {connectMutation.isPending ? 'Connecting...' : 'Connect'}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => {
                  setShowForm(false);
                  reset();
                }}
              >
                Cancel
              </Button>
            </div>
          </form>
        </div>
      )}

      {/* Connections Table */}
      <div className="bg-card border rounded-lg">
        <div className="p-6">
          <h2 className="text-lg font-semibold mb-4">Active Connections</h2>
          {connections.length === 0 ? (
            <div className="text-center py-8">
              <Database className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-2 text-sm font-medium text-foreground">No connections</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Get started by adding a database connection.
              </p>
            </div>
          ) : (
            <DataTable columns={columns} data={connections} />
          )}
        </div>
      </div>

      {/* Connection Status Cards */}
      {connections.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {connections.map((connection) => (
            <div
              key={connection.name}
              className="bg-card border rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center">
                  {connection.isConnected ? (
                    <CheckCircle className="h-5 w-5 text-green-500 mr-2" />
                  ) : (
                    <XCircle className="h-5 w-5 text-red-500 mr-2" />
                  )}
                  <h3 className="font-medium text-foreground">{connection.name}</h3>
                </div>
                <div className="flex gap-1">
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => handleSelectConnection(connection)}
                  >
                    <Eye className="h-4 w-4" />
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => handleRemoveConnection(connection.name)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
              <p className="text-sm text-muted-foreground mb-2">
                {connection.databaseType}
              </p>
              {connection.schema && (
                <p className="text-xs text-muted-foreground">
                  {connection.schema.tables.length} tables
                </p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}