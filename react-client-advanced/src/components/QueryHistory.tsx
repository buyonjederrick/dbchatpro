import React, { useState } from 'react';
import { History, Search, Filter, Download, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DataTable } from '@/components/ui/data-table';
import { format } from 'date-fns';

// Mock data for demonstration
const mockHistory = [
  {
    id: '1',
    timestamp: new Date('2024-01-15T10:30:00'),
    prompt: 'Show me the top 10 customers by order value',
    query: 'SELECT TOP 10 CustomerName, SUM(OrderValue) as TotalValue FROM Customers c JOIN Orders o ON c.CustomerId = o.CustomerId GROUP BY CustomerName ORDER BY TotalValue DESC',
    summary: 'This query retrieves the top 10 customers based on their total order value...',
    databaseName: 'SalesDB',
    aiModel: 'gpt-4',
    aiService: 'OpenAI',
    results: [{ CustomerName: 'John Doe', TotalValue: 15000 }],
  },
  {
    id: '2',
    timestamp: new Date('2024-01-15T09:15:00'),
    prompt: 'Find all orders from last month',
    query: 'SELECT * FROM Orders WHERE OrderDate >= DATEADD(month, -1, GETDATE())',
    summary: 'This query finds all orders from the last month...',
    databaseName: 'SalesDB',
    aiModel: 'gpt-4',
    aiService: 'OpenAI',
    results: [{ OrderId: 1, OrderDate: '2024-01-15' }],
  },
];

const historyColumns = [
  {
    accessorKey: 'timestamp',
    header: 'Date',
    cell: ({ row }: any) => (
      <div className="text-sm">
        {format(row.getValue('timestamp'), 'MMM dd, yyyy HH:mm')}
      </div>
    ),
  },
  {
    accessorKey: 'prompt',
    header: 'Prompt',
    cell: ({ row }: any) => (
      <div className="max-w-xs truncate" title={row.getValue('prompt')}>
        {row.getValue('prompt')}
      </div>
    ),
  },
  {
    accessorKey: 'databaseName',
    header: 'Database',
    cell: ({ row }: any) => (
      <div className="text-sm text-muted-foreground">{row.getValue('databaseName')}</div>
    ),
  },
  {
    accessorKey: 'aiModel',
    header: 'AI Model',
    cell: ({ row }: any) => (
      <div className="text-sm text-muted-foreground">{row.getValue('aiModel')}</div>
    ),
  },
  {
    accessorKey: 'results',
    header: 'Results',
    cell: ({ row }: any) => {
      const results = row.getValue('results');
      return (
        <div className="text-sm text-muted-foreground">
          {results ? `${results.length} rows` : 'No results'}
        </div>
      );
    },
  },
  {
    id: 'actions',
    header: 'Actions',
    cell: ({ row }: any) => (
      <div className="flex items-center gap-2">
        <Button size="sm" variant="ghost">
          <Search className="h-4 w-4" />
        </Button>
        <Button size="sm" variant="ghost">
          <Download className="h-4 w-4" />
        </Button>
        <Button size="sm" variant="ghost">
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>
    ),
  },
];

export function QueryHistory() {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedDatabase, setSelectedDatabase] = useState('all');

  const filteredHistory = mockHistory.filter((item) => {
    const matchesSearch = item.prompt.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         item.databaseName.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesDatabase = selectedDatabase === 'all' || item.databaseName === selectedDatabase;
    return matchesSearch && matchesDatabase;
  });

  const databases = Array.from(new Set(mockHistory.map(item => item.databaseName)));

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Query History</h1>
        <p className="text-muted-foreground">
          View and manage your previous query generations
        </p>
      </div>

      {/* Filters */}
      <div className="bg-card border rounded-lg p-6">
        <div className="flex items-center gap-4">
          <div className="flex-1">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <input
                type="text"
                placeholder="Search queries..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
              />
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Filter className="h-4 w-4 text-muted-foreground" />
            <select
              value={selectedDatabase}
              onChange={(e) => setSelectedDatabase(e.target.value)}
              className="px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
            >
              <option value="all">All Databases</option>
              {databases.map((db) => (
                <option key={db} value={db}>{db}</option>
              ))}
            </select>
          </div>
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
        </div>
      </div>

      {/* History Table */}
      <div className="bg-card border rounded-lg">
        <div className="p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold">Recent Queries</h2>
            <span className="text-sm text-muted-foreground">
              {filteredHistory.length} queries
            </span>
          </div>
          
          {filteredHistory.length === 0 ? (
            <div className="text-center py-8">
              <History className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-2 text-sm font-medium text-foreground">No queries found</h3>
              <p className="mt-1 text-sm text-muted-foreground">
                {searchTerm || selectedDatabase !== 'all' 
                  ? 'Try adjusting your filters'
                  : 'Generate some queries to see them here'
                }
              </p>
            </div>
          ) : (
            <DataTable columns={historyColumns} data={filteredHistory} />
          )}
        </div>
      </div>

      {/* Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-card border rounded-lg p-4">
          <div className="flex items-center">
            <div className="p-2 bg-blue-100 rounded-lg">
              <History className="h-6 w-6 text-blue-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-muted-foreground">Total Queries</p>
              <p className="text-2xl font-bold">{mockHistory.length}</p>
            </div>
          </div>
        </div>
        
        <div className="bg-card border rounded-lg p-4">
          <div className="flex items-center">
            <div className="p-2 bg-green-100 rounded-lg">
              <Search className="h-6 w-6 text-green-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-muted-foreground">This Month</p>
              <p className="text-2xl font-bold">
                {mockHistory.filter(item => 
                  item.timestamp.getMonth() === new Date().getMonth()
                ).length}
              </p>
            </div>
          </div>
        </div>
        
        <div className="bg-card border rounded-lg p-4">
          <div className="flex items-center">
            <div className="p-2 bg-purple-100 rounded-lg">
              <Filter className="h-6 w-6 text-purple-600" />
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-muted-foreground">Databases</p>
              <p className="text-2xl font-bold">{databases.length}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}