// BlogHybrid.Application/Queries/Tag/SearchTagsQuery.cs
using BlogHybrid.Application.DTOs.Tag;
using MediatR;

namespace BlogHybrid.Application.Queries.Tag
{
    public class SearchTagsQuery : IRequest<List<TagDto>>
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int Limit { get; set; } = 10;
    }
}