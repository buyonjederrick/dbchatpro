import React, { useState, useRef, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { 
  Play, 
  History, 
  Settings, 
  Copy, 
  Download,
  Loader2,
  CheckCircle,
  XCircle,
  Clock,
  Zap
} from 'lucide-react';
import { useExecuteMCPQuery, useMCPStatus } from '@/hooks/useEnterprise';
import { MCPQueryRequest, MCPQueryResult } from '@/types/api';
import { toast } from 'react-hot-toast';

interface QueryHistoryItem {
  id: string;
  timestamp: Date;
  request: MCPQueryRequest;
  result: MCPQueryResult;
}

export const MCPQueryInterface: React.FC = () => {
  const [query, setQuery] = useState('');
  const [aiModel, setAiModel] = useState('gpt-4');
  const [aiPlatform, setAiPlatform] = useState('openai');
  const [sessionId, setSessionId] = useState('');
  const [userId, setUserId] = useState('');
  const [queryHistory, setQueryHistory] = useState<QueryHistoryItem[]>([]);
  const [isExecuting, setIsExecuting] = useState(false);
  const [currentResult, setCurrentResult] = useState<MCPQueryResult | null>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const executeQuery = useExecuteMCPQuery();
  const { data: mcpStatus } = useMCPStatus();

  // Auto-resize textarea
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = `${textareaRef.current.scrollHeight}px`;
    }
  }, [query]);

  const handleExecuteQuery = async () => {
    if (!query.trim()) {
      toast.error('Please enter a query');
      return;
    }

    if (!mcpStatus?.isConnected) {
      toast.error('MCP server is not connected');
      return;
    }

    setIsExecuting(true);
    setCurrentResult(null);

    const request: MCPQueryRequest = {
      prompt: query,
      aiModel,
      aiPlatform,
      sessionId: sessionId || undefined,
      userId: userId || undefined,
    };

    try {
      const result = await executeQuery.mutateAsync(request);
      setCurrentResult(result);
      
      // Add to history
      const historyItem: QueryHistoryItem = {
        id: crypto.randomUUID(),
        timestamp: new Date(),
        request,
        result,
      };
      setQueryHistory((prev: QueryHistoryItem[]) => [historyItem, ...prev.slice(0, 9)]); // Keep last 10
      
      toast.success('Query executed successfully');
    } catch (error) {
      toast.error('Failed to execute query');
    } finally {
      setIsExecuting(false);
    }
  };

  const handleCopyResult = () => {
    if (currentResult?.response) {
      navigator.clipboard.writeText(currentResult.response);
      toast.success('Result copied to clipboard');
    }
  };

  const handleDownloadResult = () => {
    if (currentResult) {
      const data = {
        query: query,
        result: currentResult,
        timestamp: new Date().toISOString(),
      };
      
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `mcp-query-${Date.now()}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      
      toast.success('Result downloaded');
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleExecuteQuery();
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">MCP Query Interface</h1>
          <p className="text-muted-foreground">
            Advanced Model Context Protocol query execution
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

      <Tabs defaultValue="query" className="space-y-4">
        <TabsList>
          <TabsTrigger value="query">Query Interface</TabsTrigger>
          <TabsTrigger value="history">Query History</TabsTrigger>
          <TabsTrigger value="settings">Settings</TabsTrigger>
        </TabsList>

        <TabsContent value="query" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-3">
            {/* Query Input */}
            <div className="md:col-span-2 space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle>Query Input</CardTitle>
                  <CardDescription>
                    Enter your natural language query. Use Ctrl+Enter to execute.
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="query">Query</Label>
                    <Textarea
                      id="query"
                      ref={textareaRef}
                      placeholder="Describe what you want to query from the database..."
                      value={query}
                      onChange={(e) => setQuery(e.target.value)}
                      onKeyPress={handleKeyPress}
                      className="min-h-[120px] resize-none"
                    />
                  </div>
                  
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2">
                      <Button
                        onClick={handleExecuteQuery}
                        disabled={isExecuting || !mcpStatus?.isConnected}
                        className="flex items-center space-x-2"
                      >
                        {isExecuting ? (
                          <>
                            <Loader2 className="h-4 w-4 animate-spin" />
                            Executing...
                          </>
                        ) : (
                          <>
                            <Play className="h-4 w-4" />
                            Execute Query
                          </>
                        )}
                      </Button>
                    </div>
                    
                    <div className="flex items-center space-x-2 text-sm text-muted-foreground">
                      <Clock className="h-4 w-4" />
                      {currentResult?.metadata?.processingTime ? 
                        `${currentResult.metadata.processingTime}ms` : 'Ready'}
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Results */}
              {currentResult && (
                <Card>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle>Query Results</CardTitle>
                      <div className="flex items-center space-x-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={handleCopyResult}
                        >
                          <Copy className="h-4 w-4 mr-2" />
                          Copy
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={handleDownloadResult}
                        >
                          <Download className="h-4 w-4 mr-2" />
                          Download
                        </Button>
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent className="space-y-4">
                    {currentResult.errorMessage ? (
                      <div className="p-4 bg-destructive/10 border border-destructive/20 rounded-lg">
                        <p className="text-destructive">{currentResult.errorMessage}</p>
                      </div>
                    ) : (
                      <>
                        {currentResult.query && (
                          <div className="space-y-2">
                            <Label className="text-sm font-medium">Generated SQL</Label>
                            <div className="p-3 bg-muted rounded-lg font-mono text-sm">
                              {currentResult.query}
                            </div>
                          </div>
                        )}
                        
                        <div className="space-y-2">
                          <Label className="text-sm font-medium">Response</Label>
                          <div className="p-3 bg-muted rounded-lg">
                            {currentResult.response}
                          </div>
                        </div>

                        {currentResult.metadata && (
                          <div className="flex items-center space-x-4 text-sm text-muted-foreground">
                            <div className="flex items-center space-x-1">
                              <Zap className="h-4 w-4" />
                              <span>{currentResult.metadata.tokensUsed} tokens</span>
                            </div>
                            <div className="flex items-center space-x-1">
                              <Clock className="h-4 w-4" />
                              <span>{currentResult.metadata.processingTime}ms</span>
                            </div>
                            <div className="flex items-center space-x-1">
                              <Settings className="h-4 w-4" />
                              <span>{currentResult.metadata.modelUsed}</span>
                            </div>
                          </div>
                        )}
                      </>
                    )}
                  </CardContent>
                </Card>
              )}
            </div>

            {/* Configuration Panel */}
            <div className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle>Configuration</CardTitle>
                  <CardDescription>Query execution settings</CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="aiModel">AI Model</Label>
                    <Select value={aiModel} onValueChange={setAiModel}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="gpt-4">GPT-4</SelectItem>
                        <SelectItem value="gpt-3.5-turbo">GPT-3.5 Turbo</SelectItem>
                        <SelectItem value="claude-3">Claude-3</SelectItem>
                        <SelectItem value="gemini-pro">Gemini Pro</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="aiPlatform">AI Platform</Label>
                    <Select value={aiPlatform} onValueChange={setAiPlatform}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="openai">OpenAI</SelectItem>
                        <SelectItem value="anthropic">Anthropic</SelectItem>
                        <SelectItem value="google">Google</SelectItem>
                        <SelectItem value="azure">Azure OpenAI</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="sessionId">Session ID (Optional)</Label>
                    <Input
                      id="sessionId"
                      value={sessionId}
                      onChange={(e) => setSessionId(e.target.value)}
                      placeholder="Enter session ID"
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="userId">User ID (Optional)</Label>
                    <Input
                      id="userId"
                      value={userId}
                      onChange={(e) => setUserId(e.target.value)}
                      placeholder="Enter user ID"
                    />
                  </div>
                </CardContent>
              </Card>

              {/* MCP Status */}
              <Card>
                <CardHeader>
                  <CardTitle>MCP Status</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-sm">Connection</span>
                    <Badge variant={mcpStatus?.isConnected ? 'default' : 'destructive'}>
                      {mcpStatus?.isConnected ? 'Connected' : 'Disconnected'}
                    </Badge>
                  </div>
                  
                  {mcpStatus?.serverInfo && (
                    <>
                      <div className="flex items-center justify-between">
                        <span className="text-sm">Server</span>
                        <span className="text-sm">{mcpStatus.serverInfo.name}</span>
                      </div>
                      <div className="flex items-center justify-between">
                        <span className="text-sm">Version</span>
                        <span className="text-sm">{mcpStatus.serverInfo.version}</span>
                      </div>
                    </>
                  )}
                </CardContent>
              </Card>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="history" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Query History</CardTitle>
              <CardDescription>Recent MCP query executions</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {queryHistory.length === 0 ? (
                  <div className="text-center py-8 text-muted-foreground">
                    No query history yet
                  </div>
                ) : (
                  queryHistory.map((item) => (
                    <div key={item.id} className="border rounded-lg p-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-2">
                          <Badge variant="outline">
                            {item.request.aiModel}
                          </Badge>
                          <span className="text-sm text-muted-foreground">
                            {item.timestamp.toLocaleString()}
                          </span>
                        </div>
                        <Badge variant={item.result.errorMessage ? 'destructive' : 'default'}>
                          {item.result.errorMessage ? 'Failed' : 'Success'}
                        </Badge>
                      </div>
                      
                      <div>
                        <p className="text-sm font-medium">Query:</p>
                        <p className="text-sm text-muted-foreground">{item.request.prompt}</p>
                      </div>
                      
                      {item.result.query && (
                        <div>
                          <p className="text-sm font-medium">Generated SQL:</p>
                          <div className="p-2 bg-muted rounded text-sm font-mono">
                            {item.result.query}
                          </div>
                        </div>
                      )}
                      
                      <div>
                        <p className="text-sm font-medium">Response:</p>
                        <p className="text-sm text-muted-foreground line-clamp-3">
                          {item.result.response}
                        </p>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="settings" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>MCP Settings</CardTitle>
              <CardDescription>Configure MCP connection and behavior</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>Connection Settings</Label>
                <p className="text-sm text-muted-foreground">
                  MCP connection settings are managed server-side. Contact your administrator for configuration changes.
                </p>
              </div>
              
              <div className="space-y-2">
                <Label>Query Settings</Label>
                <p className="text-sm text-muted-foreground">
                  Default AI model and platform settings can be configured in your user preferences.
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};