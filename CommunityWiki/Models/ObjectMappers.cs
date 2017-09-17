using AutoMapper;
using CommunityWiki.Entities.Articles;
using CommunityWiki.Models.Articles;
using CommunityWiki.Models.ArticleTypes;
using CommunityWiki.Models.Votes;

namespace CommunityWiki.Models
{
    public static class ObjectMappers
    {
        internal static IMapper Mapper { get; }

        static ObjectMappers()
        {
            Mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ArticleTypeMapProfile>();
                cfg.AddProfile<ArticleMapProfile>();
            }).CreateMapper();
        }
    }

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

    public static class ModelExtensions
    {
        public static ArticleTypeModel ToModel(this ArticleType type) => Mapper.Map<ArticleTypeModel>(type);
        public static EditArticleTypeViewModel ToEditModel(this ArticleType type) => Mapper.Map<EditArticleTypeViewModel>(type);

        public static ArticleModel ToModel(this Article article) => Mapper.Map<ArticleModel>(article);
        public static ArticleViewModel ToViewModel(this Article article) => Mapper.Map<ArticleViewModel>(article);
        public static EditArticleViewModel ToEditModel(this Article article) => Mapper.Map<EditArticleViewModel>(article);
        public static ArticleRevisionModel ToModel(this ArticleRevision revision) => Mapper.Map<ArticleRevisionModel>(revision);
    }
}
