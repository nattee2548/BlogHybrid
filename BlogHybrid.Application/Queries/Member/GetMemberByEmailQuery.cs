using BlogHybrid.Application.DTOs.Member;
using MediatR;

namespace BlogHybrid.Application.Queries.Member
{
    public class GetMemberByEmailQuery : IRequest<MemberDto?>
    {
        public string Email { get; set; } = string.Empty;
    }
}