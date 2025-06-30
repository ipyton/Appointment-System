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
            var adminApiKey = configuration["AzureSearch:AdminApiKey"];
            _indexName = configuration["AzureSearch:IndexName"] ?? "appointment-system-index";

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
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(SearchDocument));

            var definition = new SearchIndex(_indexName, searchFields);

            // Add suggesters for autocomplete functionality
            var suggester = new SearchSuggester("sg", new[] { "Name", "Description", "Email", "BusinessName" });
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
        private async Task IndexDocumentAsync(SearchDocument document)
        {
            var batch = IndexDocumentsBatch.Upload(new[] { document });
            await _searchClient.IndexDocumentsAsync(batch);
        }

        /// <summary>
        /// Indexes a batch of documents
        /// </summary>
        private async Task IndexDocumentsAsync(IEnumerable<SearchDocument> documents)
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
            var batch = IndexDocumentsBatch.Delete("Id", documentId);
            await _searchClient.IndexDocumentsAsync(batch);
        }

        /// <summary>
        /// Searches for documents in the index
        /// </summary>
        public async Task<SearchResults<SearchDocument>> SearchAsync(
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
            options.Facets.Add("Tags");
            options.Facets.Add("IsServiceProvider");
            options.Facets.Add("IsActive");

            return await _searchClient.SearchAsync<SearchDocument>(searchText, options);
        }

        /// <summary>
        /// Gets autocomplete suggestions
        /// </summary>
        public async Task<SuggestResults<SearchDocument>> SuggestAsync(
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

            return await _searchClient.SuggestAsync<SearchDocument>(searchText, suggesterName, options);
        }
    }
} 