using BlogHybrid.Application.DTOs.Community;
using MediatR;

namespace BlogHybrid.Application.Queries.Community
{
    public class GetCommunityByIdQuery : IRequest<CommunityDto?>
    {
        public int Id { get; set; }

        // Current user (for checking membership & role)
        public string? CurrentUserId { get; set; }

        // Include deleted communities (for admin)
        public bool IncludeDeleted { get; set; } = false;
    }
}