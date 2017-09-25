using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityWiki.Config;
using CommunityWiki.Entities.Articles;
using Microsoft.Extensions.Options;
using Nest;

namespace CommunityWiki.Services
{
    public interface ISearchService
    {
        ElasticClient Client { get; }

        Task IndexArticle(Article article);
    }

    public class SearchService : ISearchService
    {
        private readonly SearchConfig _config;
        private ElasticClient _client;

        public ElasticClient Client
        {
            get
            {
                return _client ?? (_client = GetClient());
            }
        }

        public SearchService(IOptions<SearchConfig> config)
        {
            _config = config.Value;

        }

        public async Task IndexArticle(Article article)
        {
            await VerifyIndexExists();
            await Client.IndexAsync(article, idx => idx.Index(_config.IndexName));
        }
        
        private ElasticClient GetClient()
        {
            var nodeUri = new Uri(_config.ElasticNodeUri);
            var settings = new ConnectionSettings(nodeUri);
            settings.DefaultIndex(_config.IndexName);
            var client = new ElasticClient(settings);

            return client;
        }

        private async Task VerifyIndexExists()
        {
            var exists = await Client.IndexExistsAsync(_config.IndexName);
            if (!exists.Exists)
            {
                var settings = new IndexSettings();
                settings.NumberOfReplicas = 0;
                settings.NumberOfShards = 5;

                var createRequest = new CreateIndexRequest(new IndexName { Name = _config.IndexName, Cluster = "fFGNSMh" });
                createRequest.Settings = settings;

                await Client.CreateIndexAsync(createRequest);
            }
        }
    }

    public class ArticleSearchResultModel
    {
        public int Total { get; set; }

        public IEnumerable<Article> Articles { get; set; }
    }
}
