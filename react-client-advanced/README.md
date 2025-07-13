# DBChatPro Advanced React Client

An enterprise-grade React client for DBChatPro with advanced features, MCP (Model Context Protocol) integration, and comprehensive monitoring capabilities.

## 🚀 Features

### Core Features
- **Database Connections**: Manage multiple database connections with real-time status
- **AI-Powered Query Generation**: Generate SQL queries using natural language
- **Query History**: Track and manage query execution history
- **Real-time Results**: View query results with advanced data visualization

### Enterprise Features
- **MCP Integration**: Full Model Context Protocol support with real-time status monitoring
- **Enterprise Dashboard**: Comprehensive system monitoring and metrics
- **Audit Logging**: Complete audit trail with filtering and search capabilities
- **System Configuration**: Enterprise-grade configuration management with encryption support
- **Session Management**: Advanced user session handling and validation
- **Real-time Monitoring**: Live system metrics and performance tracking

### Advanced UI/UX
- **Modern Design**: Built with shadcn/ui components and Tailwind CSS
- **Responsive Layout**: Optimized for desktop and mobile devices
- **Dark/Light Mode**: Automatic theme switching
- **Real-time Updates**: Live data updates with WebSocket-like polling
- **Advanced Tables**: TanStack Table integration with sorting, filtering, and pagination

## 🛠 Technology Stack

- **React 18** with TypeScript
- **TanStack Query** for server state management
- **TanStack Table** for advanced data tables
- **shadcn/ui** for modern UI components
- **Tailwind CSS** for styling
- **React Router** for navigation
- **Zustand** for client state management
- **Axios** for HTTP requests
- **React Hook Form** with Zod validation
- **Lucide React** for icons
- **date-fns** for date manipulation

## 📦 Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## 🏗 Project Structure

```
src/
├── components/
│   ├── ui/                 # shadcn/ui components
│   ├── DatabaseConnections.tsx
│   ├── QueryGenerator.tsx
│   ├── QueryHistory.tsx
│   ├── Settings.tsx
│   ├── EnterpriseDashboard.tsx    # Enterprise dashboard
│   ├── MCPQueryInterface.tsx      # MCP query interface
│   └── SystemConfiguration.tsx    # System configuration
├── hooks/
│   └── useEnterprise.ts           # Enterprise hooks
├── services/
│   └── api.ts                     # API service layer
├── stores/
│   └── connectionStore.ts         # Zustand stores
├── types/
│   └── api.ts                     # TypeScript definitions
└── lib/
    └── utils.ts                   # Utility functions
```

## 🔧 Configuration

### Environment Variables

Create a `.env.local` file:

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_MCP_SERVER_URL=ws://localhost:3001
VITE_ENABLE_ENTERPRISE_FEATURES=true
```

### API Endpoints

The client integrates with the following enterprise endpoints:

- `GET /api/enterprise/audit-logs` - Retrieve audit logs
- `GET /api/enterprise/metrics` - Get system metrics
- `GET /api/enterprise/system-config` - Get system configurations
- `POST /api/enterprise/system-config` - Update system configurations
- `POST /api/enterprise/session` - Create user sessions
- `GET /api/enterprise/mcp/status` - Get MCP connection status
- `POST /api/enterprise/mcp/query` - Execute MCP queries

## 🎯 Enterprise Features

### MCP Integration

The client provides full Model Context Protocol integration:

- **Real-time Status**: Monitor MCP server connection status
- **Query Interface**: Execute natural language queries through MCP
- **Query History**: Track MCP query executions
- **Metadata Tracking**: Monitor tokens used, processing time, and model information

### Enterprise Dashboard

Comprehensive monitoring dashboard with:

- **System Metrics**: Real-time performance metrics
- **Connection Status**: Database and MCP connection monitoring
- **Audit Logs**: Filterable audit trail with pagination
- **Weekly Statistics**: Query activity trends
- **MCP Status**: Detailed MCP server information

### System Configuration

Enterprise-grade configuration management:

- **Category-based Organization**: Organize configs by category
- **Encryption Support**: Secure sensitive configuration values
- **Audit Trail**: Track configuration changes
- **Real-time Updates**: Live configuration updates

### Audit Logging

Complete audit trail system:

- **Comprehensive Logging**: Track all user actions and system events
- **Advanced Filtering**: Filter by date, user, action type
- **Pagination**: Handle large audit log datasets
- **Real-time Updates**: Live audit log updates

## 🔒 Security Features

- **Session Management**: Secure user session handling
- **Authentication**: Bearer token authentication
- **Request Tracking**: Unique request IDs for tracing
- **Error Handling**: Comprehensive error handling and user feedback
- **Input Validation**: Client-side validation with Zod schemas

## 📊 Performance Features

- **Query Caching**: TanStack Query for intelligent caching
- **Optimistic Updates**: Immediate UI feedback
- **Background Refetching**: Automatic data updates
- **Virtual Scrolling**: Efficient rendering of large datasets
- **Debounced Input**: Optimized search and filtering

## 🎨 UI/UX Features

- **Modern Design**: Clean, professional interface
- **Responsive Layout**: Works on all device sizes
- **Accessibility**: WCAG compliant components
- **Loading States**: Comprehensive loading indicators
- **Error States**: User-friendly error handling
- **Toast Notifications**: Real-time user feedback

## 🚀 Development

### Code Quality

```bash
# Lint code
npm run lint

# Fix linting issues
npm run lint:fix

# Type checking
npm run type-check
```

### Adding New Features

1. **API Integration**: Add new endpoints to `src/services/api.ts`
2. **Type Definitions**: Update `src/types/api.ts` with new types
3. **Hooks**: Create custom hooks in `src/hooks/`
4. **Components**: Build reusable components in `src/components/`
5. **State Management**: Use Zustand for global state

### Enterprise Feature Development

When adding enterprise features:

1. **Audit Logging**: Ensure all actions are logged
2. **Error Handling**: Implement comprehensive error handling
3. **Performance**: Consider impact on system performance
4. **Security**: Validate all inputs and handle sensitive data
5. **Testing**: Add comprehensive tests for new features

## � Customization

### Theming

The client uses CSS custom properties for theming. Customize colors in `src/index.css`:

```css
:root {
  --background: 0 0% 100%;
  --foreground: 222.2 84% 4.9%;
  --primary: 222.2 47.4% 11.2%;
  /* ... more variables */
}
```

### Component Styling

All components use Tailwind CSS classes and can be customized through the `tailwind.config.js` file.

## 📈 Monitoring

### Performance Monitoring

- **Query Performance**: Track query execution times
- **API Response Times**: Monitor API endpoint performance
- **User Interactions**: Track user engagement metrics
- **Error Rates**: Monitor application error rates

### Health Checks

- **MCP Connection**: Real-time MCP server status
- **Database Connections**: Monitor database connectivity
- **API Endpoints**: Health checks for all API endpoints
- **System Resources**: Monitor system resource usage

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new features
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For enterprise support and questions:

- **Documentation**: Check the API documentation
- **Issues**: Report bugs and feature requests
- **Enterprise Support**: Contact the development team

---

Built with ❤️ for enterprise-grade database management and AI-powered query generation.