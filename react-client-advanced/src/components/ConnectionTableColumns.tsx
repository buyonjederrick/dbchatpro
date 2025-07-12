import { ColumnDef } from '@tanstack/react-table';
import { DatabaseConnectionResponse } from '@/types/api';
import { CheckCircle, XCircle, Eye, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';

export const columns: ColumnDef<DatabaseConnectionResponse>[] = [
  {
    accessorKey: 'name',
    header: 'Connection Name',
    cell: ({ row }) => (
      <div className="font-medium">{row.getValue('name')}</div>
    ),
  },
  {
    accessorKey: 'databaseType',
    header: 'Database Type',
    cell: ({ row }) => (
      <div className="text-sm text-muted-foreground">{row.getValue('databaseType')}</div>
    ),
  },
  {
    accessorKey: 'isConnected',
    header: 'Status',
    cell: ({ row }) => {
      const isConnected = row.getValue('isConnected') as boolean;
      return (
        <div className="flex items-center">
          {isConnected ? (
            <CheckCircle className="h-4 w-4 text-green-500 mr-2" />
          ) : (
            <XCircle className="h-4 w-4 text-red-500 mr-2" />
          )}
          <span className={isConnected ? 'text-green-600' : 'text-red-600'}>
            {isConnected ? 'Connected' : 'Disconnected'}
          </span>
        </div>
      );
    },
  },
  {
    accessorKey: 'schema',
    header: 'Tables',
    cell: ({ row }) => {
      const schema = row.getValue('schema') as any;
      return (
        <div className="text-sm text-muted-foreground">
          {schema ? `${schema.tables.length} tables` : 'No schema'}
        </div>
      );
    },
  },
  {
    id: 'actions',
    header: 'Actions',
    cell: ({ row }) => {
      const connection = row.original;
      
      return (
        <div className="flex items-center gap-2">
          <Button size="sm" variant="ghost">
            <Eye className="h-4 w-4" />
          </Button>
          <Button size="sm" variant="ghost">
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      );
    },
  },
];