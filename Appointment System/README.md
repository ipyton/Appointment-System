# Appointment System

## Environment Variables Setup

This application uses environment variables to securely store sensitive information like API keys. Follow these steps to set up your environment variables:

### Required Environment Variables

- `AZURE_SEARCH_ADMIN_API_KEY`: Your Azure Search admin API key
- `AZURE_SEARCH_QUERY_API_KEY`: Your Azure Search query API key

### Setting Environment Variables

#### Windows

1. Open Command Prompt as Administrator
2. Set environment variables using:
   ```
   setx AZURE_SEARCH_ADMIN_API_KEY "your-admin-api-key" /M
   setx AZURE_SEARCH_QUERY_API_KEY "your-query-api-key" /M
   ```
3. Restart your applications or computer for the changes to take effect

#### macOS/Linux

1. Add these lines to your ~/.bash_profile, ~/.zshrc, or appropriate shell configuration file:
   ```
   export AZURE_SEARCH_ADMIN_API_KEY="your-admin-api-key"
   export AZURE_SEARCH_QUERY_API_KEY="your-query-api-key"
   ```
2. Run `source ~/.bash_profile` (or the appropriate file) to apply changes to the current session

### Development Environment

In development mode, the application will automatically load these values from the appsettings.json file if they are provided there. However, for production deployments, you should set the environment variables at the system or hosting platform level.

### Docker Environment

If using Docker, you can pass environment variables using:

```
docker run -e AZURE_SEARCH_ADMIN_API_KEY="your-admin-api-key" -e AZURE_SEARCH_QUERY_API_KEY="your-query-api-key" your-image-name
```

## Additional Configuration

For other configuration options, refer to the appsettings.json file. 