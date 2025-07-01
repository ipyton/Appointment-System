using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using Appointment_System.Models;
using System.Text.Json;

namespace Appointment_System.Services
{
    public class AzureSearchService
    {
        private readonly SearchIndexClient _indexClient;
        private readonly SearchClient _searchClient;
        private readonly string _indexName;

        public AzureSearchService(IConfiguration configuration)
        {
            var searchServiceEndpoint = configuration["AzureSearch:Endpoint"];
            
            // Prioritize environment variables over configuration values
            var adminApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_ADMIN_API_KEY") ?? 
                              configuration["AzureSearch:AdminApiKey"];
            
            var queryApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_QUERY_API_KEY") ?? 
                              configuration["AzureSearch:QueryApiKey"];
                              
            _indexName = configuration["AzureSearch:IndexName"] ?? "appointment-system-index";

            if (string.IsNullOrEmpty(adminApiKey))
            {
                throw new InvalidOperationException("Azure Search Admin API Key is not configured. Set the AZURE_SEARCH_ADMIN_API_KEY environment variable.");
            }

            // Initialize the clients
            _indexClient = new SearchIndexClient(
                new Uri(searchServiceEndpoint),
                new AzureKeyCredential(adminApiKey));

            _searchClient = new SearchClient(
                new Uri(searchServiceEndpoint),
                _indexName,
                new AzureKeyCredential(adminApiKey));
        }

        /// <summary>
        /// Creates or updates the search index
        /// </summary>
        public async Task CreateOrUpdateIndexAsync()
        {

                
            // Define fields for the index
    var fields = new List<SearchField>
    {
        new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
        new SearchableField("type") { IsFilterable = true, IsSortable = true },
        new SearchableField("name") { IsFilterable = true, IsSortable = true },
        new SearchableField("description"),
        new SimpleField("isActive", SearchFieldDataType.Boolean) { IsFilterable = true },
        new SimpleField("createdAt", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
        new SearchableField("email") { IsFilterable = true },
        new SearchableField("address"),
        new SimpleField("isServiceProvider", SearchFieldDataType.Boolean) { IsFilterable = true },
        new SearchableField("businessName"),
        new SimpleField("price", SearchFieldDataType.Double) { IsFilterable = true, IsSortable = true },
        new SimpleField("durationMinutes", SearchFieldDataType.Int32) { IsFilterable = true },
        new SimpleField("providerId", SearchFieldDataType.String) { IsFilterable = true },
        new SearchField("tags", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsSearchable = true, IsFilterable = true, IsFacetable = true }
    };

            var definition = new SearchIndex(_indexName, fields);

            // Add suggesters for autocomplete functionality
            var suggester = new SearchSuggester("sg", new[] { "name", "description", "email", "businessName" });
            definition.Suggesters.Add(suggester);

            await _indexClient.CreateOrUpdateIndexAsync(definition);
        }

        /// <summary>
        /// Indexes a single ApplicationUser
        /// </summary>
        public async Task IndexUserAsync(ApplicationUser user)
        {
            var document = SearchDocumentAdapter.FromApplicationUser(user);
            await IndexDocumentAsync(document);
        }

        /// <summary>
        /// Indexes a single Service
        /// </summary>
        public async Task IndexServiceAsync(Service service)
        {
            var document = SearchDocumentAdapter.FromService(service);
            await IndexDocumentAsync(document);
        }

        /// <summary>
        /// Indexes a batch of ApplicationUsers
        /// </summary>
        public async Task IndexUsersAsync(IEnumerable<ApplicationUser> users)
        {
            var documents = SearchDocumentAdapter.FromApplicationUsers(users);
            await IndexDocumentsAsync(documents);
        }

        /// <summary>
        /// Indexes a batch of Services
        /// </summary>
        public async Task IndexServicesAsync(IEnumerable<Service> services)
        {
            var documents = SearchDocumentAdapter.FromServices(services);
            await IndexDocumentsAsync(documents);
        }

        /// <summary>
        /// Indexes a single document
        /// </summary>
        private async Task IndexDocumentAsync(Dictionary<string, object> document)
        {
            var batch = IndexDocumentsBatch.Upload(new[] { document });
            await _searchClient.IndexDocumentsAsync(batch);
        }

        /// <summary>
        /// Indexes a batch of documents
        /// </summary>
        private async Task IndexDocumentsAsync(IEnumerable<Dictionary<string, object>> documents)
        {
            if (documents == null || !documents.Any())
                return;

            var batch = IndexDocumentsBatch.Upload(documents);
            await _searchClient.IndexDocumentsAsync(batch);
        }

        /// <summary>
        /// Removes a user from the index
        /// </summary>
        public async Task DeleteUserAsync(string userId)
        {
            await DeleteDocumentAsync($"user-{userId}");
        }

        /// <summary>
        /// Removes a service from the index
        /// </summary>
        public async Task DeleteServiceAsync(int serviceId)
        {
            await DeleteDocumentAsync($"service-{serviceId}");
        }

        /// <summary>
        /// Removes a document from the index
        /// </summary>
        private async Task DeleteDocumentAsync(string documentId)
        {
            var batch = IndexDocumentsBatch.Delete("id", new[] { documentId });
            await _searchClient.IndexDocumentsAsync(batch);
        }

        /// <summary>
        /// Searches for documents in the index
        /// </summary>
        public async Task<SearchResults<Dictionary<string, object>>> SearchAsync(
            string searchText,
            string filter = null,
            int skip = 0,
            int top = 50,
            params string[] selectFields)
        {
            var options = new SearchOptions
            {
                IncludeTotalCount = true,
                Filter = filter,
                Skip = skip,
                Size = top
            };

            if (selectFields != null && selectFields.Length > 0)
            {
                foreach (var field in selectFields)
                {
                    options.Select.Add(field);
                }
            }

            // Add facets for filtering
            options.Facets.Add("tags");
            options.Facets.Add("isServiceProvider");
            options.Facets.Add("isActive");

            return await _searchClient.SearchAsync<Dictionary<string, object>>(searchText, options);
        }

        /// <summary>
        /// Gets autocomplete suggestions
        /// </summary>
        public async Task<SuggestResults<Dictionary<string, object>>> SuggestAsync(
            string searchText,
            string suggesterName = "sg",
            bool fuzzy = true,
            int top = 5)
        {
            var options = new SuggestOptions
            {
                UseFuzzyMatching = fuzzy,
                Size = top
            };

            return await _searchClient.SuggestAsync<Dictionary<string, object>>(searchText, suggesterName, options);
        }
    }
} 