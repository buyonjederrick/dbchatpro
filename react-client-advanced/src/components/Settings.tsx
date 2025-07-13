import React, { useState } from 'react';
import { Settings as SettingsIcon, Save, RefreshCw, Key, Database, SmartToy, Shield, BarChart3 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { SystemConfiguration } from './SystemConfiguration';

export function Settings() {
  const [settings, setSettings] = useState({
    maxRows: 1000,
    defaultAiService: 'OpenAI',
    defaultAiModel: 'gpt-4',
    autoSaveQueries: true,
    showQueryResults: true,
    enableNotifications: true,
  });

  const [aiConfig, setAiConfig] = useState({
    openaiKey: '',
    azureEndpoint: '',
    azureKey: '',
    ollamaEndpoint: 'http://localhost:11434',
  });

  const handleSaveSettings = () => {
    // Save settings logic
    console.log('Saving settings:', settings);
  };

  const handleSaveAIConfig = () => {
    // Save AI configuration logic
    console.log('Saving AI config:', aiConfig);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Settings</h1>
        <p className="text-muted-foreground">
          Configure your DBChatPro application preferences
        </p>
      </div>

      <Tabs defaultValue="general" className="space-y-4">
        <TabsList>
          <TabsTrigger value="general">General</TabsTrigger>
          <TabsTrigger value="ai">AI Configuration</TabsTrigger>
          <TabsTrigger value="enterprise">Enterprise</TabsTrigger>
        </TabsList>

        <TabsContent value="general">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* General Settings */}
            <div className="space-y-6">
              <div className="bg-card border rounded-lg p-6">
                <div className="flex items-center mb-4">
                  <SettingsIcon className="h-5 w-5 mr-2" />
                  <h2 className="text-lg font-semibold">General Settings</h2>
                </div>
                
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">
                      Maximum Rows per Query
                    </label>
                    <input
                      type="number"
                      value={settings.maxRows}
                      onChange={(e) => setSettings(prev => ({ ...prev, maxRows: parseInt(e.target.value) }))}
                      className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">
                      Default AI Service
                    </label>
                    <select
                      value={settings.defaultAiService}
                      onChange={(e) => setSettings(prev => ({ ...prev, defaultAiService: e.target.value }))}
                      className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    >
                      <option value="OpenAI">OpenAI</option>
                      <option value="AzureOpenAI">Azure OpenAI</option>
                      <option value="Ollama">Ollama</option>
                      <option value="GitHubModels">GitHub Models</option>
                      <option value="AWSBedrock">AWS Bedrock</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">
                      Default AI Model
                    </label>
                    <select
                      value={settings.defaultAiModel}
                      onChange={(e) => setSettings(prev => ({ ...prev, defaultAiModel: e.target.value }))}
                      className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    >
                      <option value="gpt-4">GPT-4</option>
                      <option value="gpt-4o">GPT-4o</option>
                      <option value="gpt-3.5-turbo">GPT-3.5 Turbo</option>
                    </select>
                  </div>

                  <div className="space-y-2">
                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.autoSaveQueries}
                        onChange={(e) => setSettings(prev => ({ ...prev, autoSaveQueries: e.target.checked }))}
                        className="mr-2"
                      />
                      <span className="text-sm">Auto-save generated queries</span>
                    </label>

                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.showQueryResults}
                        onChange={(e) => setSettings(prev => ({ ...prev, showQueryResults: e.target.checked }))}
                        className="mr-2"
                      />
                      <span className="text-sm">Show query results by default</span>
                    </label>

                    <label className="flex items-center">
                      <input
                        type="checkbox"
                        checked={settings.enableNotifications}
                        onChange={(e) => setSettings(prev => ({ ...prev, enableNotifications: e.target.checked }))}
                        className="mr-2"
                      />
                      <span className="text-sm">Enable notifications</span>
                    </label>
                  </div>

                  <Button onClick={handleSaveSettings} className="w-full">
                    <Save className="h-4 w-4 mr-2" />
                    Save Settings
                  </Button>
                </div>
              </div>
            </div>

            {/* Database Settings */}
            <div className="space-y-6">
              <div className="bg-card border rounded-lg p-6">
                <div className="flex items-center mb-4">
                  <Database className="h-5 w-5 mr-2" />
                  <h2 className="text-lg font-semibold">Database Settings</h2>
                </div>
                
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">
                      Connection Timeout (seconds)
                    </label>
                    <input
                      type="number"
                      defaultValue={30}
                      className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">
                      Query Timeout (seconds)
                    </label>
                    <input
                      type="number"
                      defaultValue={60}
                      className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    />
                  </div>

                  <Button variant="outline" className="w-full">
                    <RefreshCw className="h-4 w-4 mr-2" />
                    Test All Connections
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="ai">
          <div className="space-y-6">
            <div className="bg-card border rounded-lg p-6">
              <div className="flex items-center mb-4">
                <SmartToy className="h-5 w-5 mr-2" />
                <h2 className="text-lg font-semibold">AI Configuration</h2>
              </div>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    OpenAI API Key
                  </label>
                  <div className="relative">
                    <input
                      type="password"
                      value={aiConfig.openaiKey}
                      onChange={(e) => setAiConfig(prev => ({ ...prev, openaiKey: e.target.value }))}
                      className="w-full px-3 py-2 pr-10 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                      placeholder="sk-..."
                    />
                    <Key className="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    Azure OpenAI Endpoint
                  </label>
                  <input
                    type="url"
                    value={aiConfig.azureEndpoint}
                    onChange={(e) => setAiConfig(prev => ({ ...prev, azureEndpoint: e.target.value }))}
                    className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    placeholder="https://your-resource.openai.azure.com/"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    Azure OpenAI Key
                  </label>
                  <div className="relative">
                    <input
                      type="password"
                      value={aiConfig.azureKey}
                      onChange={(e) => setAiConfig(prev => ({ ...prev, azureKey: e.target.value }))}
                      className="w-full px-3 py-2 pr-10 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                      placeholder="..."
                    />
                    <Key className="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    Ollama Endpoint
                  </label>
                  <input
                    type="url"
                    value={aiConfig.ollamaEndpoint}
                    onChange={(e) => setAiConfig(prev => ({ ...prev, ollamaEndpoint: e.target.value }))}
                    className="w-full px-3 py-2 border border-input rounded-md bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                    placeholder="http://localhost:11434"
                  />
                </div>

                <Button onClick={handleSaveAIConfig} className="w-full">
                  <Save className="h-4 w-4 mr-2" />
                  Save AI Configuration
                </Button>
              </div>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="enterprise">
          <SystemConfiguration />
        </TabsContent>
      </Tabs>
    </div>
  );
}