using BlogHybrid.Application.DTOs.Member;
using MediatR;

namespace BlogHybrid.Application.Queries.Member
{
    public class GetMembersQuery : IRequest<MemberListDto>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? RoleFilter { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "desc";
    }
}