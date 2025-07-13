using DBChatPro.Models;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Diagnostics;

namespace DBChatPro.Services
{
    public class AdvancedDatabaseService
    {
        private readonly ILogger<AdvancedDatabaseService> _logger;
        private readonly Dictionary<string, object> _connectionPool = new Dictionary<string, object>();
        private readonly object _poolLock = new object();

        public AdvancedDatabaseService(ILogger<AdvancedDatabaseService> logger)
        {
            _logger = logger;
        }

        // Enhanced data retrieval with timeout support
        public async Task<List<List<string>>> GetDataTableWithTimeout(AIConnection connection, string query, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new List<List<string>>();

            try
            {
                _logger.LogInformation("Executing query with timeout: {Query}", query);

                using var dbConnection = CreateConnection(connection.ConnectionString, connection.DatabaseType);
                await dbConnection.OpenAsync(cancellationToken);

                using var command = CreateCommand(dbConnection, query, connection.DatabaseType);
                command.CommandTimeout = 300; // 5 minutes default timeout

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                
                // Get column headers
                var headers = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    headers.Add(reader.GetName(i));
                }
                result.Add(headers);

                // Get data rows
                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "NULL";
                        row.Add(value);
                    }
                    result.Add(row);
                }

                stopwatch.Stop();
                _logger.LogInformation("Query executed successfully in {Time}ms, returned {Rows} rows", 
                    stopwatch.ElapsedMilliseconds, result.Count - 1);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Query execution was cancelled due to timeout");
                throw new TimeoutException("Query execution timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute query with timeout");
                throw;
            }

            return result;
        }

        // Batch query execution with transaction support
        public async Task<BatchExecutionResult> ExecuteBatchQueries(
            AIConnection connection, 
            List<string> queries, 
            bool useTransaction = true,
            CancellationToken cancellationToken = default)
        {
            var result = new BatchExecutionResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Executing batch of {Count} queries with transaction: {UseTransaction}", 
                    queries.Count, useTransaction);

                using var dbConnection = CreateConnection(connection.ConnectionString, connection.DatabaseType);
                await dbConnection.OpenAsync(cancellationToken);

                IDbTransaction? transaction = null;
                if (useTransaction)
                {
                    transaction = await dbConnection.BeginTransactionAsync(cancellationToken);
                }

                try
                {
                    foreach (var query in queries)
                    {
                        var item = new BatchQueryItem { Query = query };

                        try
                        {
                            using var command = CreateCommand(dbConnection, query, connection.DatabaseType);
                            if (transaction != null)
                            {
                                command.Transaction = transaction;
                            }

                            var rowData = new List<List<string>>();
                            using var reader = await command.ExecuteReaderAsync(cancellationToken);
                            
                            // Get headers
                            var headers = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                headers.Add(reader.GetName(i));
                            }
                            rowData.Add(headers);

                            // Get data
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var row = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "NULL";
                                    row.Add(value);
                                }
                                rowData.Add(row);
                            }

                            item.Results = rowData;
                            item.RowsReturned = rowData.Count - 1;
                            item.IsSuccessful = true;
                        }
                        catch (Exception ex)
                        {
                            item.IsSuccessful = false;
                            item.ErrorMessage = ex.Message;
                            _logger.LogError(ex, "Failed to execute query in batch: {Query}", query);
                        }

                        result.Queries.Add(item);
                    }

                    if (transaction != null)
                    {
                        await transaction.CommitAsync(cancellationToken);
                    }

                    result.IsSuccessful = result.Queries.All(q => q.IsSuccessful);
                    result.SuccessfulQueries = result.Queries.Count(q => q.IsSuccessful);
                    result.FailedQueries = result.Queries.Count(q => !q.IsSuccessful);
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                    throw;
                }
                finally
                {
                    transaction?.Dispose();
                }

                stopwatch.Stop();
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("Batch execution completed. Successful: {Success}, Failed: {Failed}, Time: {Time}ms",
                    result.SuccessfulQueries, result.FailedQueries, result.ExecutionTimeMs);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                _logger.LogError(ex, "Failed to execute batch queries");
            }

            return result;
        }

        // Connection pooling with health checks
        public async Task<object> GetPooledConnection(string connectionString, string databaseType)
        {
            var poolKey = $"{connectionString}_{databaseType}";

            lock (_poolLock)
            {
                if (_connectionPool.ContainsKey(poolKey))
                {
                    var connection = _connectionPool[poolKey];
                    if (IsConnectionHealthy(connection, databaseType))
                    {
                        return connection;
                    }
                    else
                    {
                        _connectionPool.Remove(poolKey);
                    }
                }
            }

            // Create new connection
            var newConnection = CreateConnection(connectionString, databaseType);
            
            lock (_poolLock)
            {
                _connectionPool[poolKey] = newConnection;
            }

            return newConnection;
        }

        // Query performance monitoring
        public async Task<QueryPerformanceMetrics> MonitorQueryPerformance(
            AIConnection connection, 
            string query, 
            CancellationToken cancellationToken = default)
        {
            var metrics = new QueryPerformanceMetrics();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var dbConnection = CreateConnection(connection.ConnectionString, connection.DatabaseType);
                await dbConnection.OpenAsync(cancellationToken);

                using var command = CreateCommand(dbConnection, query, connection.DatabaseType);
                
                // Record start time and memory usage
                var startTime = DateTime.UtcNow;
                var startMemory = GC.GetTotalMemory(false);

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                
                var rowCount = 0;
                var columnCount = 0;
                var headers = new List<string>();

                // Read headers
                if (await reader.ReadAsync(cancellationToken))
                {
                    columnCount = reader.FieldCount;
                    for (int i = 0; i < columnCount; i++)
                    {
                        headers.Add(reader.GetName(i));
                    }
                    rowCount++;
                }

                // Read data
                while (await reader.ReadAsync(cancellationToken))
                {
                    rowCount++;
                }

                stopwatch.Stop();
                var endMemory = GC.GetTotalMemory(false);

                metrics.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
                metrics.RowsReturned = rowCount;
                metrics.ColumnsReturned = columnCount;
                metrics.MemoryUsageBytes = endMemory - startMemory;
                metrics.StartTime = startTime;
                metrics.EndTime = startTime.AddMilliseconds(metrics.ExecutionTimeMs);
                metrics.IsSuccessful = true;

                _logger.LogInformation("Query performance monitored. Time: {Time}ms, Rows: {Rows}, Memory: {Memory}bytes",
                    metrics.ExecutionTimeMs, metrics.RowsReturned, metrics.MemoryUsageBytes);
            }
            catch (Exception ex)
            {
                metrics.IsSuccessful = false;
                metrics.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to monitor query performance");
            }

            return metrics;
        }

        // Database health check
        public async Task<DatabaseHealthStatus> CheckDatabaseHealth(AIConnection connection)
        {
            var status = new DatabaseHealthStatus();

            try
            {
                using var dbConnection = CreateConnection(connection.ConnectionString, connection.DatabaseType);
                await dbConnection.OpenAsync();

                // Test basic connectivity
                status.IsConnected = dbConnection.State == ConnectionState.Open;

                if (status.IsConnected)
                {
                    // Test simple query execution
                    using var command = CreateCommand(dbConnection, "SELECT 1", connection.DatabaseType);
                    using var reader = await command.ExecuteReaderAsync();
                    
                    if (await reader.ReadAsync())
                    {
                        status.IsQueryable = true;
                    }

                    // Get connection info
                    status.DatabaseType = connection.DatabaseType;
                    status.ConnectionString = connection.ConnectionString;
                    status.LastChecked = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                status.IsConnected = false;
                status.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Database health check failed");
            }

            return status;
        }

        // Schema analysis with detailed information
        public async Task<DetailedSchemaInfo> GetDetailedSchema(AIConnection connection)
        {
            var schema = new DetailedSchemaInfo();

            try
            {
                using var dbConnection = CreateConnection(connection.ConnectionString, connection.DatabaseType);
                await dbConnection.OpenAsync();

                // Get table information
                var tableQuery = GetTableQuery(connection.DatabaseType);
                using var tableCommand = CreateCommand(dbConnection, tableQuery, connection.DatabaseType);
                using var tableReader = await tableCommand.ExecuteReaderAsync();

                while (await tableReader.ReadAsync())
                {
                    var table = new TableInfo
                    {
                        Name = tableReader["TABLE_NAME"]?.ToString() ?? "",
                        Schema = tableReader["TABLE_SCHEMA"]?.ToString() ?? "",
                        Type = tableReader["TABLE_TYPE"]?.ToString() ?? ""
                    };

                    // Get column information for this table
                    var columnQuery = GetColumnQuery(connection.DatabaseType, table.Name);
                    using var columnCommand = CreateCommand(dbConnection, columnQuery, connection.DatabaseType);
                    using var columnReader = await columnCommand.ExecuteReaderAsync();

                    while (await columnReader.ReadAsync())
                    {
                        var column = new ColumnInfo
                        {
                            Name = columnReader["COLUMN_NAME"]?.ToString() ?? "",
                            DataType = columnReader["DATA_TYPE"]?.ToString() ?? "",
                            IsNullable = columnReader["IS_NULLABLE"]?.ToString() == "YES",
                            DefaultValue = columnReader["COLUMN_DEFAULT"]?.ToString() ?? ""
                        };
                        table.Columns.Add(column);
                    }

                    schema.Tables.Add(table);
                }

                schema.IsSuccessful = true;
                schema.GeneratedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                schema.IsSuccessful = false;
                schema.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to get detailed schema");
            }

            return schema;
        }

        private IDbConnection CreateConnection(string connectionString, string databaseType)
        {
            return databaseType.ToUpper() switch
            {
                "MSSQL" => new SqlConnection(connectionString),
                "MYSQL" => new MySqlConnection(connectionString),
                "POSTGRESQL" => new NpgsqlConnection(connectionString),
                "ORACLE" => new OracleConnection(connectionString),
                _ => throw new ArgumentException($"Unsupported database type: {databaseType}")
            };
        }

        private IDbCommand CreateCommand(IDbConnection connection, string query, string databaseType)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.Text;
            return command;
        }

        private bool IsConnectionHealthy(object connection, string databaseType)
        {
            try
            {
                if (connection is IDbConnection dbConnection)
                {
                    return dbConnection.State == ConnectionState.Open;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GetTableQuery(string databaseType)
        {
            return databaseType.ToUpper() switch
            {
                "MSSQL" => "SELECT TABLE_NAME, TABLE_SCHEMA, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                "MYSQL" => "SELECT TABLE_NAME, TABLE_SCHEMA, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                "POSTGRESQL" => "SELECT table_name, table_schema, 'BASE TABLE' as table_type FROM information_schema.tables WHERE table_type = 'BASE TABLE'",
                "ORACLE" => "SELECT table_name, owner as table_schema, 'BASE TABLE' as table_type FROM all_tables",
                _ => throw new ArgumentException($"Unsupported database type: {databaseType}")
            };
        }

        private string GetColumnQuery(string databaseType, string tableName)
        {
            return databaseType.ToUpper() switch
            {
                "MSSQL" => $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'",
                "MYSQL" => $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'",
                "POSTGRESQL" => $"SELECT column_name, data_type, is_nullable, column_default FROM information_schema.columns WHERE table_name = '{tableName}'",
                "ORACLE" => $"SELECT column_name, data_type, nullable as is_nullable, data_default as column_default FROM all_tab_columns WHERE table_name = '{tableName}'",
                _ => throw new ArgumentException($"Unsupported database type: {databaseType}")
            };
        }
    }

    // Result classes for advanced database operations
    public class BatchExecutionResult
    {
        public bool IsSuccessful { get; set; }
        public List<BatchQueryItem> Queries { get; set; } = new List<BatchQueryItem>();
        public int SuccessfulQueries { get; set; }
        public int FailedQueries { get; set; }
        public int ExecutionTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class BatchQueryItem
    {
        public string Query { get; set; } = string.Empty;
        public List<List<string>> Results { get; set; } = new List<List<string>>();
        public int RowsReturned { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class QueryPerformanceMetrics
    {
        public bool IsSuccessful { get; set; }
        public int ExecutionTimeMs { get; set; }
        public int RowsReturned { get; set; }
        public int ColumnsReturned { get; set; }
        public long MemoryUsageBytes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DatabaseHealthStatus
    {
        public bool IsConnected { get; set; }
        public bool IsQueryable { get; set; }
        public string DatabaseType { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DetailedSchemaInfo
    {
        public bool IsSuccessful { get; set; }
        public List<TableInfo> Tables { get; set; } = new List<TableInfo>();
        public DateTime GeneratedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TableInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
    }

    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; } = string.Empty;
    }
}