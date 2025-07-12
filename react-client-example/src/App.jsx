import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  TextField,
  Button,
  Typography,
  Box,
  Grid,
  Card,
  CardContent,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow
} from '@mui/material';
import { Database, SmartToy, Send } from '@mui/icons-material';
import axios from 'axios';

const API_BASE_URL = '/api';

function App() {
  const [connections, setConnections] = useState([]);
  const [selectedConnection, setSelectedConnection] = useState(null);
  const [prompt, setPrompt] = useState('');
  const [aiModel, setAiModel] = useState('gpt-4');
  const [aiService, setAiService] = useState('OpenAI');
  const [queryResult, setQueryResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [availableModels, setAvailableModels] = useState({});

  // Connection form state
  const [connectionForm, setConnectionForm] = useState({
    name: '',
    databaseType: 'MSSQL',
    connectionString: ''
  });

  useEffect(() => {
    fetchAvailableModels();
  }, []);

  const fetchAvailableModels = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/ai/models`);
      setAvailableModels(response.data);
    } catch (error) {
      console.error('Failed to fetch available models:', error);
    }
  };

  const handleConnectionSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await axios.post(`${API_BASE_URL}/database/connect`, connectionForm);
      
      if (response.data.isConnected) {
        setConnections(prev => [...prev, response.data]);
        setConnectionForm({ name: '', databaseType: 'MSSQL', connectionString: '' });
        setError('');
      } else {
        setError(response.data.errorMessage || 'Failed to connect to database');
      }
    } catch (error) {
      setError(error.response?.data?.errorMessage || 'Failed to connect to database');
    } finally {
      setLoading(false);
    }
  };

  const handleQuerySubmit = async (e) => {
    e.preventDefault();
    if (!selectedConnection || !prompt.trim()) return;

    setLoading(true);
    setError('');
    setQueryResult(null);

    try {
      const response = await axios.post(`${API_BASE_URL}/ai/query`, {
        prompt: prompt,
        aiModel: aiModel,
        aiService: aiService,
        databaseType: selectedConnection.databaseType,
        connectionString: selectedConnection.connectionString
      });

      setQueryResult(response.data);
      setError('');
    } catch (error) {
      setError(error.response?.data?.errorMessage || 'Failed to generate query');
    } finally {
      setLoading(false);
    }
  };

  const handleConnectionSelect = (connection) => {
    setSelectedConnection(connection);
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h3" component="h1" gutterBottom align="center">
        <Database sx={{ mr: 2, verticalAlign: 'middle' }} />
        DBChatPro API Client
      </Typography>

      <Grid container spacing={4}>
        {/* Database Connections */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              Database Connections
            </Typography>
            
            <Box component="form" onSubmit={handleConnectionSubmit} sx={{ mb: 3 }}>
              <TextField
                fullWidth
                label="Connection Name"
                value={connectionForm.name}
                onChange={(e) => setConnectionForm(prev => ({ ...prev, name: e.target.value }))}
                margin="normal"
                required
              />
              
              <FormControl fullWidth margin="normal">
                <InputLabel>Database Type</InputLabel>
                <Select
                  value={connectionForm.databaseType}
                  onChange={(e) => setConnectionForm(prev => ({ ...prev, databaseType: e.target.value }))}
                  label="Database Type"
                >
                  <MenuItem value="MSSQL">SQL Server</MenuItem>
                  <MenuItem value="MYSQL">MySQL</MenuItem>
                  <MenuItem value="POSTGRESQL">PostgreSQL</MenuItem>
                  <MenuItem value="ORACLE">Oracle</MenuItem>
                </Select>
              </FormControl>
              
              <TextField
                fullWidth
                label="Connection String"
                value={connectionForm.connectionString}
                onChange={(e) => setConnectionForm(prev => ({ ...prev, connectionString: e.target.value }))}
                margin="normal"
                required
                multiline
                rows={3}
              />
              
              <Button
                type="submit"
                variant="contained"
                fullWidth
                disabled={loading}
                sx={{ mt: 2 }}
              >
                {loading ? <CircularProgress size={24} /> : 'Connect to Database'}
              </Button>
            </Box>

            {connections.length > 0 && (
              <Box>
                <Typography variant="h6" gutterBottom>
                  Connected Databases
                </Typography>
                {connections.map((connection, index) => (
                  <Card
                    key={index}
                    sx={{
                      mb: 2,
                      cursor: 'pointer',
                      border: selectedConnection === connection ? 2 : 1,
                      borderColor: selectedConnection === connection ? 'primary.main' : 'divider'
                    }}
                    onClick={() => handleConnectionSelect(connection)}
                  >
                    <CardContent>
                      <Typography variant="h6">{connection.name}</Typography>
                      <Chip label={connection.databaseType} size="small" sx={{ mt: 1 }} />
                    </CardContent>
                  </Card>
                ))}
              </Box>
            )}
          </Paper>
        </Grid>

        {/* AI Query Interface */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              <SmartToy sx={{ mr: 1, verticalAlign: 'middle' }} />
              AI Query Generator
            </Typography>

            {selectedConnection && (
              <Alert severity="info" sx={{ mb: 2 }}>
                Connected to: {selectedConnection.name} ({selectedConnection.databaseType})
              </Alert>
            )}

            <Box component="form" onSubmit={handleQuerySubmit}>
              <TextField
                fullWidth
                label="Natural Language Prompt"
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
                margin="normal"
                multiline
                rows={3}
                placeholder="e.g., Show me the top 10 customers by order value"
                disabled={!selectedConnection}
              />

              <Grid container spacing={2} sx={{ mt: 2 }}>
                <Grid item xs={6}>
                  <FormControl fullWidth>
                    <InputLabel>AI Service</InputLabel>
                    <Select
                      value={aiService}
                      onChange={(e) => setAiService(e.target.value)}
                      label="AI Service"
                    >
                      {Object.keys(availableModels).map(service => (
                        <MenuItem key={service} value={service}>{service}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={6}>
                  <FormControl fullWidth>
                    <InputLabel>AI Model</InputLabel>
                    <Select
                      value={aiModel}
                      onChange={(e) => setAiModel(e.target.value)}
                      label="AI Model"
                    >
                      {availableModels[aiService]?.map(model => (
                        <MenuItem key={model} value={model}>{model}</MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
              </Grid>

              <Button
                type="submit"
                variant="contained"
                fullWidth
                disabled={!selectedConnection || loading || !prompt.trim()}
                sx={{ mt: 2 }}
                endIcon={<Send />}
              >
                {loading ? <CircularProgress size={24} /> : 'Generate Query'}
              </Button>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mt: 2 }}>
                {error}
              </Alert>
            )}

            {queryResult && (
              <Box sx={{ mt: 3 }}>
                <Typography variant="h6" gutterBottom>
                  Query Results
                </Typography>
                
                <Card sx={{ mb: 2 }}>
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      Summary
                    </Typography>
                    <Typography variant="body2" paragraph>
                      {queryResult.summary}
                    </Typography>
                    
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      Generated SQL
                    </Typography>
                    <Paper sx={{ p: 2, bgcolor: 'grey.100' }}>
                      <Typography variant="body2" component="pre" sx={{ m: 0, fontFamily: 'monospace' }}>
                        {queryResult.query}
                      </Typography>
                    </Paper>
                  </CardContent>
                </Card>

                {queryResult.results && queryResult.results.length > 0 && (
                  <Card>
                    <CardContent>
                      <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                        Query Results
                      </Typography>
                      <TableContainer>
                        <Table size="small">
                          <TableHead>
                            <TableRow>
                              {Object.keys(queryResult.results[0]).map(header => (
                                <TableCell key={header}>{header}</TableCell>
                              ))}
                            </TableRow>
                          </TableHead>
                          <TableBody>
                            {queryResult.results.map((row, index) => (
                              <TableRow key={index}>
                                {Object.values(row).map((cell, cellIndex) => (
                                  <TableCell key={cellIndex}>{String(cell)}</TableCell>
                                ))}
                              </TableRow>
                            ))}
                          </TableBody>
                        </Table>
                      </TableContainer>
                    </CardContent>
                  </Card>
                )}
              </Box>
            )}
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
}

export default App;