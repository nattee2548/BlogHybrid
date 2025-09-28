using BlogHybrid.Application.DTOs.Member;
using MediatR;

namespace BlogHybrid.Application.Queries.Member
{
    public class GetMemberByIdQuery : IRequest<MemberDto?>
    {
        public string Id { get; set; } = string.Empty;
    }
}