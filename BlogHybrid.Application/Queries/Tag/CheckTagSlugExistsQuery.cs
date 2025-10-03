// BlogHybrid.Application/Queries/Tag/CheckTagSlugExistsQuery.cs
using MediatR;

namespace BlogHybrid.Application.Queries.Tag
{
    public class CheckTagSlugExistsQuery : IRequest<bool>
    {
        public string Slug { get; set; } = string.Empty;
        public int? ExcludeId { get; set; }
    }
}