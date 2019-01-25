using AutoMapper;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Models.Articles;
using CommunityWiki.Models.ArticleTypes;
using CommunityWiki.Models.Votes;

namespace CommunityWiki.Models
{
    public class ArticleTypeMapProfile : Profile
    {
        public ArticleTypeMapProfile()
        {
            CreateMap<ArticleType, ArticleTypeModel>();
            CreateMap<ArticleType, EditArticleTypeViewModel>();
        }
    }

    public class ArticleMapProfile : Profile
    {
        public ArticleMapProfile()
        {
            CreateMap<Article, ArticleModel>();
            CreateMap<Article, EditArticleViewModel>();
            CreateMap<Article, ArticleViewModel>();

            CreateMap<ArticleRevision, ArticleRevisionModel>();

            CreateMap<Vote, VoteModel>();
        }
    }
}
