// BlogHybrid.Application/Queries/Tag/GetTagsQuery.cs
using BlogHybrid.Application.DTOs.Tag;
using MediatR;

namespace BlogHybrid.Application.Queries.Tag
{
    public class GetTagsQuery : IRequest<TagListDto>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortDirection { get; set; } = "asc";
    }
}