import React, { useState } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { 
  Settings, 
  Shield, 
  Database, 
  Network, 
  Security,
  Eye,
  EyeOff,
  Save,
  Plus,
  Trash2
} from 'lucide-react';
import { useSystemConfigurations, useSetSystemConfiguration } from '@/hooks/useEnterprise';
import type { SystemConfiguration, SystemConfigurationRequest } from '@/types/api';
import { toast } from 'react-hot-toast';

const CONFIG_CATEGORIES = [
  'database',
  'security',
  'network',
  'ai',
  'logging',
  'performance',
  'general'
];

export const SystemConfiguration: React.FC = () => {
  const [selectedCategory, setSelectedCategory] = useState<string>('general');
  const [isAddingNew, setIsAddingNew] = useState(false);
  const [newConfig, setNewConfig] = useState<Partial<SystemConfigurationRequest>>({
    key: '',
    value: '',
    category: 'general',
    description: '',
    isEncrypted: false,
  });
  const [showPasswords, setShowPasswords] = useState(false);

  const { data: configurations, isLoading } = useSystemConfigurations(selectedCategory);
  const setConfiguration = useSetSystemConfiguration();

  const handleAddConfiguration = async () => {
    if (!newConfig.key || !newConfig.value) {
      toast.error('Key and value are required');
      return;
    }

    try {
      await setConfiguration.mutateAsync({
        key: newConfig.key,
        value: newConfig.value,
        category: newConfig.category || 'general',
        description: newConfig.description,
        isEncrypted: newConfig.isEncrypted || false,
        userId: 'current-user', // This would come from auth context
        userName: 'Current User', // This would come from auth context
      });

      setNewConfig({
        key: '',
        value: '',
        category: 'general',
        description: '',
        isEncrypted: false,
      });
      setIsAddingNew(false);
      toast.success('Configuration added successfully');
    } catch (error) {
      toast.error('Failed to add configuration');
    }
  };

  const handleUpdateConfiguration = async (config: SystemConfiguration, newValue: string) => {
    try {
      await setConfiguration.mutateAsync({
        key: config.key,
        value: newValue,
        category: config.category || 'general',
        description: config.description,
        isEncrypted: config.isEncrypted,
        userId: 'current-user',
        userName: 'Current User',
      });
      toast.success('Configuration updated successfully');
    } catch (error) {
      toast.error('Failed to update configuration');
    }
  };

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'database':
        return <Database className="h-4 w-4" />;
      case 'security':
        return <Shield className="h-4 w-4" />;
      case 'network':
        return <Network className="h-4 w-4" />;
      case 'ai':
        return <Settings className="h-4 w-4" />;
      default:
        return <Settings className="h-4 w-4" />;
    }
  };

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'database':
        return 'bg-blue-100 text-blue-800';
      case 'security':
        return 'bg-red-100 text-red-800';
      case 'network':
        return 'bg-green-100 text-green-800';
      case 'ai':
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">System Configuration</h1>
          <p className="text-muted-foreground">
            Manage enterprise system settings and configurations
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => setShowPasswords(!showPasswords)}
          >
            {showPasswords ? <EyeOff className="h-4 w-4 mr-2" /> : <Eye className="h-4 w-4 mr-2" />}
            {showPasswords ? 'Hide' : 'Show'} Passwords
          </Button>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-4">
        {/* Category Sidebar */}
        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Categories</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {CONFIG_CATEGORIES.map((category) => (
                <Button
                  key={category}
                  variant={selectedCategory === category ? 'default' : 'ghost'}
                  className="w-full justify-start"
                  onClick={() => setSelectedCategory(category)}
                >
                  {getCategoryIcon(category)}
                  <span className="ml-2 capitalize">{category}</span>
                </Button>
              ))}
            </CardContent>
          </Card>

          {/* Add New Configuration */}
          <Card>
            <CardHeader>
              <CardTitle>Add Configuration</CardTitle>
            </CardHeader>
            <CardContent>
              <Button
                variant="outline"
                className="w-full"
                onClick={() => setIsAddingNew(!isAddingNew)}
              >
                <Plus className="h-4 w-4 mr-2" />
                Add New
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Configuration List */}
        <div className="md:col-span-3 space-y-4">
          {/* Add New Configuration Form */}
          {isAddingNew && (
            <Card>
              <CardHeader>
                <CardTitle>Add New Configuration</CardTitle>
                <CardDescription>Create a new system configuration</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="configKey">Configuration Key</Label>
                    <Input
                      id="configKey"
                      value={newConfig.key}
                      onChange={(e) => setNewConfig({ ...newConfig, key: e.target.value })}
                      placeholder="e.g., database.connection.timeout"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="configCategory">Category</Label>
                    <Select
                      value={newConfig.category}
                      onValueChange={(value) => setNewConfig({ ...newConfig, category: value })}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {CONFIG_CATEGORIES.map((category) => (
                          <SelectItem key={category} value={category}>
                            {category.charAt(0).toUpperCase() + category.slice(1)}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="configValue">Value</Label>
                  <Textarea
                    id="configValue"
                    value={newConfig.value}
                    onChange={(e) => setNewConfig({ ...newConfig, value: e.target.value })}
                    placeholder="Enter configuration value"
                    rows={3}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="configDescription">Description</Label>
                  <Input
                    id="configDescription"
                    value={newConfig.description}
                    onChange={(e) => setNewConfig({ ...newConfig, description: e.target.value })}
                    placeholder="Optional description"
                  />
                </div>

                <div className="flex items-center space-x-2">
                  <Switch
                    id="encrypted"
                    checked={newConfig.isEncrypted}
                    onCheckedChange={(checked) => setNewConfig({ ...newConfig, isEncrypted: checked })}
                  />
                  <Label htmlFor="encrypted">Encrypt this value</Label>
                </div>

                <div className="flex space-x-2">
                  <Button onClick={handleAddConfiguration} disabled={setConfiguration.isPending}>
                    <Save className="h-4 w-4 mr-2" />
                    Save Configuration
                  </Button>
                  <Button
                    variant="outline"
                    onClick={() => setIsAddingNew(false)}
                  >
                    Cancel
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Configuration List */}
          <Card>
            <CardHeader>
              <CardTitle>
                {selectedCategory.charAt(0).toUpperCase() + selectedCategory.slice(1)} Configurations
              </CardTitle>
              <CardDescription>
                Manage system configurations for {selectedCategory}
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {configurations?.length === 0 ? (
                  <div className="text-center py-8 text-muted-foreground">
                    No configurations found for this category
                  </div>
                ) : (
                  configurations?.map((config) => (
                    <div key={config.id} className="border rounded-lg p-4 space-y-3">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-2">
                          <Badge variant="outline" className={getCategoryColor(config.category || 'general')}>
                            {config.category}
                          </Badge>
                          <span className="font-medium">{config.key}</span>
                          {config.isEncrypted && (
                            <Shield className="h-4 w-4 text-muted-foreground" />
                          )}
                        </div>
                        <div className="flex items-center space-x-2">
                          <span className="text-xs text-muted-foreground">
                            Updated: {new Date(config.updatedAt).toLocaleDateString()}
                          </span>
                        </div>
                      </div>

                      {config.description && (
                        <p className="text-sm text-muted-foreground">{config.description}</p>
                      )}

                      <div className="space-y-2">
                        <Label className="text-sm font-medium">Value</Label>
                        <div className="flex items-center space-x-2">
                          <Input
                            type={config.isEncrypted && !showPasswords ? 'password' : 'text'}
                            value={config.value}
                            onChange={(e) => handleUpdateConfiguration(config, e.target.value)}
                            className="font-mono text-sm"
                          />
                          {config.isEncrypted && (
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => setShowPasswords(!showPasswords)}
                            >
                              {showPasswords ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                            </Button>
                          )}
                        </div>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};