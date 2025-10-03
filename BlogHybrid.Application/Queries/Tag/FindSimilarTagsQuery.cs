// BlogHybrid.Application/Queries/Tag/FindSimilarTagsQuery.cs
using BlogHybrid.Application.Interfaces.Services;
using MediatR;

namespace BlogHybrid.Application.Queries.Tag
{
    public class FindSimilarTagsQuery : IRequest<List<SimilarTagResult>>
    {
        public string TagName { get; set; } = string.Empty;
        public int Limit { get; set; } = 5;
    }
}