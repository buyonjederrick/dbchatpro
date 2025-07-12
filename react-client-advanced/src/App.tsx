import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { Toaster } from 'react-hot-toast';
import { Database, SmartToy, History, Settings } from 'lucide-react';

import { Layout } from '@/components/Layout';
import { DatabaseConnections } from '@/components/DatabaseConnections';
import { QueryGenerator } from '@/components/QueryGenerator';
import { QueryHistory } from '@/components/QueryHistory';
import { Settings as SettingsPage } from '@/components/Settings';

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
    },
  },
});

const navigation = [
  { name: 'Connections', href: '/', icon: Database },
  { name: 'Query Generator', href: '/query', icon: SmartToy },
  { name: 'History', href: '/history', icon: History },
  { name: 'Settings', href: '/settings', icon: Settings },
];

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <div className="min-h-screen bg-background">
          <Layout navigation={navigation}>
            <Routes>
              <Route path="/" element={<DatabaseConnections />} />
              <Route path="/query" element={<QueryGenerator />} />
              <Route path="/history" element={<QueryHistory />} />
              <Route path="/settings" element={<SettingsPage />} />
            </Routes>
          </Layout>
          <Toaster 
            position="bottom-right"
            toastOptions={{
              duration: 4000,
              style: {
                background: 'hsl(var(--background))',
                color: 'hsl(var(--foreground))',
                border: '1px solid hsl(var(--border))',
              },
            }}
          />
          <ReactQueryDevtools initialIsOpen={false} />
        </div>
      </Router>
    </QueryClientProvider>
  );
}

export default App;