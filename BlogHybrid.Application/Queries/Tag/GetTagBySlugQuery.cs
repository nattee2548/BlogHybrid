// BlogHybrid.Application/Queries/Tag/GetTagBySlugQuery.cs
using BlogHybrid.Application.DTOs.Tag;
using MediatR;

namespace BlogHybrid.Application.Queries.Tag
{
    public class GetTagBySlugQuery : IRequest<TagDto?>
    {
        public string Slug { get; set; } = string.Empty;
    }
}