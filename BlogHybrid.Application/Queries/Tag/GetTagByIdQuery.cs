// BlogHybrid.Application/Queries/Tag/GetTagByIdQuery.cs
using BlogHybrid.Application.DTOs.Tag;
using MediatR;

namespace BlogHybrid.Application.Queries.Tag
{
    public class GetTagByIdQuery : IRequest<TagDto?>
    {
        public int Id { get; set; }
    }
}