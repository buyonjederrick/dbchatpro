# DBChatPro Advanced React Client

An advanced React client for DBChatPro API built with modern technologies and best practices.

## 🚀 Features

### **Advanced UI/UX**
- **shadcn/ui** - Modern, accessible UI components
- **Tailwind CSS** - Utility-first CSS framework
- **Responsive Design** - Works on all devices
- **Dark/Light Mode** - Built-in theme support
- **Loading States** - Smooth user experience
- **Error Handling** - Comprehensive error management

### **Data Management**
- **TanStack Query** - Powerful data fetching and caching
- **Zustand** - Lightweight state management
- **React Hook Form** - Performant form handling
- **Zod** - Type-safe schema validation

### **Advanced Tables**
- **TanStack Table** - Feature-rich table component
- **Sorting** - Multi-column sorting
- **Filtering** - Global and column-specific filters
- **Pagination** - Built-in pagination
- **Virtual Scrolling** - Performance for large datasets
- **Export** - CSV/Excel export capabilities

### **Database Management**
- **Connection Management** - Add, edit, remove database connections
- **Schema Visualization** - View database schemas
- **Connection Status** - Real-time connection monitoring
- **Persistent Storage** - Connections saved locally

### **AI Query Generation**
- **Natural Language to SQL** - AI-powered query generation
- **Multiple AI Services** - OpenAI, Azure OpenAI, Ollama, GitHub Models, AWS Bedrock
- **Query History** - Track and manage generated queries
- **Query Export** - Download queries as SQL files
- **Results Display** - Advanced table view for query results

### **Advanced Features**
- **Query History** - Comprehensive history management
- **Search & Filtering** - Advanced search capabilities
- **Statistics Dashboard** - Usage analytics
- **Settings Management** - Comprehensive configuration
- **Toast Notifications** - User feedback system

## 🛠️ Tech Stack

### **Core**
- **React 18** - Latest React features
- **TypeScript** - Type safety
- **Vite** - Fast build tool
- **React Router** - Client-side routing

### **UI & Styling**
- **shadcn/ui** - Modern UI components
- **Tailwind CSS** - Utility-first CSS
- **Lucide React** - Beautiful icons
- **class-variance-authority** - Component variants

### **Data & State**
- **TanStack Query** - Data fetching & caching
- **Zustand** - State management
- **React Hook Form** - Form handling
- **Zod** - Schema validation

### **Tables & Data**
- **TanStack Table** - Advanced table features
- **TanStack Virtual** - Virtual scrolling
- **date-fns** - Date utilities

### **Development**
- **ESLint** - Code linting
- **TypeScript** - Type checking
- **Vite** - Development server

## 📦 Installation

```bash
# Navigate to the advanced client directory
cd react-client-advanced

# Install dependencies
npm install

# Start development server
npm run dev
```

## 🏗️ Project Structure

```
react-client-advanced/
├── src/
│   ├── components/
│   │   ├── ui/                 # shadcn/ui components
│   │   │   ├── button.tsx
│   │   │   ├── input.tsx
│   │   │   └── data-table.tsx
│   │   ├── Layout.tsx          # Main layout component
│   │   ├── DatabaseConnections.tsx
│   │   ├── QueryGenerator.tsx
│   │   ├── QueryHistory.tsx
│   │   ├── Settings.tsx
│   │   ├── ConnectionTableColumns.tsx
│   │   └── ResultsTableColumns.tsx
│   ├── hooks/
│   │   └── useQueries.ts       # TanStack Query hooks
│   ├── services/
│   │   └── api.ts              # API service layer
│   ├── stores/
│   │   └── connectionStore.ts  # Zustand store
│   ├── types/
│   │   └── api.ts              # TypeScript types
│   ├── lib/
│   │   └── utils.ts            # Utility functions
│   ├── App.tsx                 # Main app component
│   ├── main.tsx                # Entry point
│   └── index.css               # Global styles
├── public/                     # Static assets
├── package.json
├── tsconfig.json
├── tailwind.config.js
├── vite.config.ts
└── README.md
```

## 🎯 Key Components

### **DatabaseConnections**
- Connection management with form validation
- Real-time connection status
- Schema visualization
- Advanced table with sorting/filtering
- Connection cards with quick actions

### **QueryGenerator**
- Natural language to SQL conversion
- Multiple AI service support
- Real-time query generation
- Advanced results display with TanStack Table
- Query export and copy functionality

### **QueryHistory**
- Comprehensive query history
- Advanced search and filtering
- Statistics dashboard
- Export capabilities
- Bulk operations

### **Settings**
- Application configuration
- AI service configuration
- Database settings
- Advanced options
- Import/export settings

## 🔧 Configuration

### **Environment Variables**
Create a `.env` file in the root directory:

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_NAME=DBChatPro Advanced
```

### **API Configuration**
The client is configured to proxy API requests to the .NET API:

```typescript
// vite.config.ts
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
})
```

## 🚀 Advanced Features

### **TanStack Query Integration**
```typescript
// Custom hooks for data fetching
const { data: connections, isLoading } = useConnections();
const { mutate: connectDatabase, isPending } = useConnectDatabase();
```

### **Zustand State Management**
```typescript
// Persistent connection store
const { connections, addConnection, removeConnection } = useConnectionStore();
```

### **React Hook Form with Zod**
```typescript
// Type-safe form validation
const form = useForm<ConnectionFormData>({
  resolver: zodResolver(connectionSchema)
});
```

### **TanStack Table Features**
```typescript
// Advanced table with sorting, filtering, pagination
const table = useReactTable({
  data,
  columns,
  getSortedRowModel: getSortedRowModel(),
  getFilteredRowModel: getFilteredRowModel(),
  getPaginationRowModel: getPaginationRowModel(),
});
```

## 🎨 UI Components

### **shadcn/ui Components**
- **Button** - Multiple variants and sizes
- **Input** - Form inputs with validation
- **DataTable** - Advanced table component
- **Toast** - Notification system

### **Custom Components**
- **Layout** - Responsive sidebar layout
- **Connection Cards** - Visual connection management
- **Query Results** - Advanced data display
- **Statistics Cards** - Usage analytics

## 🔄 Data Flow

1. **User Action** → React Hook Form validation
2. **Valid Data** → TanStack Query mutation
3. **API Call** → DBChatPro API
4. **Response** → Zustand store update
5. **UI Update** → Component re-render
6. **Toast Notification** → User feedback

## 🧪 Testing

```bash
# Run linting
npm run lint

# Fix linting issues
npm run lint:fix

# Build for production
npm run build

# Preview production build
npm run preview
```

## 🚀 Deployment

### **Build for Production**
```bash
npm run build
```

### **Deploy to Vercel**
```bash
# Install Vercel CLI
npm i -g vercel

# Deploy
vercel
```

### **Deploy to Netlify**
```bash
# Build the project
npm run build

# Deploy the dist folder
```

## 🔧 Development

### **Adding New Components**
1. Create component in `src/components/`
2. Add TypeScript types in `src/types/`
3. Create custom hooks in `src/hooks/`
4. Add to routing in `App.tsx`

### **Adding New API Endpoints**
1. Add to `src/services/api.ts`
2. Create query hooks in `src/hooks/useQueries.ts`
3. Add types in `src/types/api.ts`

### **Styling Guidelines**
- Use Tailwind CSS classes
- Follow shadcn/ui patterns
- Use CSS variables for theming
- Maintain responsive design

## 📚 API Integration

### **Database Connections**
```typescript
// Connect to database
const result = await api.connectDatabase({
  name: 'My Database',
  databaseType: 'MSSQL',
  connectionString: '...'
});
```

### **AI Query Generation**
```typescript
// Generate SQL query
const result = await api.generateQuery({
  prompt: 'Show me top 10 customers',
  aiModel: 'gpt-4',
  aiService: 'OpenAI',
  databaseType: 'MSSQL',
  connectionString: '...'
});
```

## 🎯 Performance Features

- **TanStack Query Caching** - Intelligent data caching
- **Virtual Scrolling** - Performance for large datasets
- **Code Splitting** - Lazy-loaded components
- **Optimized Builds** - Vite production optimizations
- **Persistent State** - Zustand with persistence

## 🔒 Security

- **Input Validation** - Zod schema validation
- **Type Safety** - TypeScript throughout
- **Secure API Calls** - Axios with proper config
- **Environment Variables** - Secure configuration

## 🤝 Contributing

1. Follow TypeScript best practices
2. Use TanStack Query for data fetching
3. Implement proper error handling
4. Add loading states
5. Write responsive components
6. Follow shadcn/ui patterns

## 📄 License

This project maintains the same license as the original DBChatPro project.