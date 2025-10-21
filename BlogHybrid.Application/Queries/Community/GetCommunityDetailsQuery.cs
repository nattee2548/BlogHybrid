using BlogHybrid.Application.DTOs.Community;
using MediatR;

namespace BlogHybrid.Application.Queries.Community
{
    /// <summary>
    /// Query สำหรับดึงข้อมูล Community Details พร้อมสิทธิ์ของ User และจำนวน Pending Members
    /// </summary>
    public class GetCommunityDetailsQuery : IRequest<CommunityDetailsDto?>
    {
        public string Slug { get; set; } = string.Empty;
        public string? CurrentUserId { get; set; }
    }
}