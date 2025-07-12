import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Send, Database, SmartToy, Copy, Download, Eye } from 'lucide-react';
import { useGenerateQuery, useAvailableModels } from '@/hooks/useQueries';
import { useConnectionStore } from '@/stores/connectionStore';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { columns as resultsColumns } from './ResultsTableColumns';
import { format } from 'date-fns';

const querySchema = z.object({
  prompt: z.string().min(1, 'Prompt is required'),
  aiModel: z.string().min(1, 'AI Model is required'),
  aiService: z.string().min(1, 'AI Service is required'),
});

type QueryFormData = z.infer<typeof querySchema>;

export function QueryGenerator() {
  const [selectedConnection, setSelectedConnection] = useState<any>(null);
  const [queryResult, setQueryResult] = useState<any>(null);
  const { connections, selectedConnection: storeSelectedConnection } = useConnectionStore();
  const { data: availableModels = {} } = useAvailableModels();
  const generateQueryMutation = useGenerateQuery();

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<QueryFormData>({
    resolver: zodResolver(querySchema),
  });

  const watchedAiService = watch('aiService');

  const onSubmit = async (data: QueryFormData) => {
    if (!selectedConnection) {
      alert('Please select a database connection first');
      return;
    }

    try {
      const result = await generateQueryMutation.mutateAsync({
        ...data,
        databaseType: selectedConnection.databaseType,
        connectionString: selectedConnection.connectionString,
      });
      
      setQueryResult({
        ...result,
        timestamp: new Date(),
        databaseName: selectedConnection.name,
        prompt: data.prompt,
      });
    } catch (error) {
      // Error handling is done in the mutation
    }
  };

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  const downloadQuery = (query: string, prompt: string) => {
    const blob = new Blob([`-- Generated SQL Query\n-- Prompt: ${prompt}\n-- Generated at: ${new Date().toISOString()}\n\n${query}`], {
      type: 'text/plain',
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `query_${Date.now()}.sql`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">AI Query Generator</h1>
        <p className="text-muted-foreground">
          Generate SQL queries from natural language using AI
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Left Panel - Query Form */}
        <div className="space-y-6">
          {/* Connection Selection */}
          <div className="bg-card border rounded-lg p-6">
            <h2 className="text-lg font-semibold mb-4">Database Connection</h2>
            {connections.length === 0 ? (
              <div className="text-center py-8">
                <Database className="mx-auto h-12 w-12 text-muted-foreground" />
                <h3 className="mt-2 text-sm font-medium text-foreground">No connections</h3>
                <p className="mt-1 text-sm text-muted-foreground">
                  Please add a database connection first.
                </p>
              </div>
            ) : (
              <div className="space-y-2">
                {connections.map((connection) => (
                  <div
                    key={connection.name}
                    className={`p-3 border rounded-lg cursor-pointer transition-colors ${
                      selectedConnection?.name === connection.name
                        ? 'border-primary bg-primary/5'
                        : 'border-border hover:border-primary/50'
                    }`}
                    onClick={() => setSelectedConnection(connection)}
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <h3 className="font-medium">{connection.name}</h3>
                        <p className="text-sm text-muted-foreground">{connection.databaseType}</p>
                      </div>
                      {connection.isConnected && (
                        <div className="w-2 h-2 bg-green-500 rounded-full"></div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Query Form */}
          <div className="bg-card border rounded-lg p-6">
            <h2 className="text-lg font-semibold mb-4">Generate Query</h2>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">
                  Natural Language Prompt
                </label>
                <textarea
                  {...register('prompt')}
                  rows={4}
                  className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                  placeholder="e.g., Show me the top 10 customers by order value"
                />
                {errors.prompt && (
                  <p className="text-sm text-destructive mt-1">{errors.prompt.message}</p>
                )}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    AI Service
                  </label>
                  <select
                    {...register('aiService')}
                    className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                  >
                    <option value="">Select AI service</option>
                    {Object.keys(availableModels).map((service) => (
                      <option key={service} value={service}>
                        {service}
                      </option>
                    ))}
                  </select>
                  {errors.aiService && (
                    <p className="text-sm text-destructive mt-1">{errors.aiService.message}</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    AI Model
                  </label>
                  <select
                    {...register('aiModel')}
                    className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                  >
                    <option value="">Select model</option>
                    {watchedAiService &&
                      availableModels[watchedAiService]?.map((model) => (
                        <option key={model} value={model}>
                          {model}
                        </option>
                      ))}
                  </select>
                  {errors.aiModel && (
                    <p className="text-sm text-destructive mt-1">{errors.aiModel.message}</p>
                  )}
                </div>
              </div>

              <Button
                type="submit"
                disabled={generateQueryMutation.isPending || !selectedConnection}
                className="w-full flex items-center justify-center"
              >
                {generateQueryMutation.isPending ? (
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
                ) : (
                  <SmartToy className="mr-2 h-4 w-4" />
                )}
                {generateQueryMutation.isPending ? 'Generating...' : 'Generate Query'}
              </Button>
            </form>
          </div>
        </div>

        {/* Right Panel - Results */}
        <div className="space-y-6">
          {queryResult && (
            <>
              {/* Query Summary */}
              <div className="bg-card border rounded-lg p-6">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-semibold">Generated Query</h2>
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => copyToClipboard(queryResult.query)}
                    >
                      <Copy className="h-4 w-4 mr-1" />
                      Copy
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => downloadQuery(queryResult.query, queryResult.prompt)}
                    >
                      <Download className="h-4 w-4 mr-1" />
                      Download
                    </Button>
                  </div>
                </div>
                
                <div className="space-y-4">
                  <div>
                    <h3 className="text-sm font-medium text-muted-foreground mb-2">Summary</h3>
                    <p className="text-sm text-foreground">{queryResult.summary}</p>
                  </div>
                  
                  <div>
                    <h3 className="text-sm font-medium text-muted-foreground mb-2">SQL Query</h3>
                    <pre className="bg-muted p-3 rounded-md text-sm overflow-x-auto">
                      <code>{queryResult.query}</code>
                    </pre>
                  </div>
                  
                  <div className="text-xs text-muted-foreground">
                    Generated at: {format(queryResult.timestamp, 'PPpp')}
                  </div>
                </div>
              </div>

              {/* Query Results */}
              {queryResult.results && queryResult.results.length > 0 && (
                <div className="bg-card border rounded-lg p-6">
                  <div className="flex items-center justify-between mb-4">
                    <h2 className="text-lg font-semibold">Query Results</h2>
                    <span className="text-sm text-muted-foreground">
                      {queryResult.results.length} rows
                    </span>
                  </div>
                  <DataTable columns={resultsColumns} data={queryResult.results} />
                </div>
              )}
            </>
          )}

          {!queryResult && (
            <div className="bg-card border rounded-lg p-6">
              <div className="text-center py-8">
                <SmartToy className="mx-auto h-12 w-12 text-muted-foreground" />
                <h3 className="mt-2 text-sm font-medium text-foreground">No query generated</h3>
                <p className="mt-1 text-sm text-muted-foreground">
                  Generate a query to see results here.
                </p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}