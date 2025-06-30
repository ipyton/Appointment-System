# Azure Search Integration for Appointment System

This document explains how the Appointment System integrates with Azure Cognitive Search for indexing and searching users and services.

## Setup

1. Create an Azure Cognitive Search service in the Azure Portal
2. Update the `appsettings.json` file with your Azure Search service details:

```json
"AzureSearch": {
  "Endpoint": "https://your-search-service.search.windows.net",
  "AdminApiKey": "your-admin-api-key",
  "QueryApiKey": "your-query-api-key",
  "IndexName": "appointment-system-index"
}
```

## Architecture

The search integration consists of the following components:

### Models

- **SearchDocument**: The document model for Azure Search that represents both users and services
- **SearchDocumentAdapter**: Adapter class for converting ApplicationUser and Service entities to SearchDocument objects

### Services

- **AzureSearchService**: Core service for interacting with Azure Search
- **SearchIndexingEventHandler**: Event handler for indexing entities when they are created, updated, or deleted
- **SearchIndexingService**: Background service for periodic reindexing of all entities

### Controllers

- **SearchController**: API endpoints for searching and suggesting users and services
- **AccountController**: Updated to index users when they are created
- **ServicesController**: Manages services and indexes them when they are created, updated, or deleted

## API Endpoints

### Search

- `GET /api/search`: Search for users and services
  - Query parameters:
    - `query`: Search text
    - `type`: Filter by type ("User" or "Service")
    - `isServiceProvider`: Filter users by service provider status
    - `isActive`: Filter by active status
    - `skip`: Number of results to skip (for pagination)
    - `top`: Number of results to return (for pagination)

### Suggestions

- `GET /api/search/suggest`: Get autocomplete suggestions
  - Query parameters:
    - `query`: Text to get suggestions for
    - `fuzzy`: Whether to use fuzzy matching (default: true)
    - `top`: Number of suggestions to return (default: 5)

### Indexing

- `POST /api/search/index`: Manually trigger indexing of all users and services (Admin only)

## Automatic Indexing

The system automatically indexes:

1. Users when they are created through the registration process
2. Services when they are created, updated, or deleted through the ServicesController
3. All entities periodically through the background SearchIndexingService

## Search Document Structure

The SearchDocument model includes the following fields:

- **Id**: Unique identifier (key)
- **Type**: "User" or "Service"
- **Name**: User's full name or service name
- **Description**: User's business description or service description
- **IsActive**: Whether the entity is active
- **CreatedAt**: When the entity was created
- **Email**: User's email (for users only)
- **Address**: User's address (for users only)
- **IsServiceProvider**: Whether the user is a service provider (for users only)
- **BusinessName**: User's business name (for users only)
- **Price**: Service price (for services only)
- **DurationMinutes**: Service duration in minutes (for services only)
- **ProviderId**: ID of the service provider (for services only)
- **Tags**: List of tags for faceting and filtering

## Example Usage

### Search for services

```http
GET /api/search?query=massage&type=Service&isActive=true&skip=0&top=10
```

### Get suggestions

```http
GET /api/search/suggest?query=mas&fuzzy=true&top=5
```

### Manually trigger indexing (Admin only)

```http
POST /api/search/index
``` 