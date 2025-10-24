// BlogHybrid.Application/Queries/Post/GetPostDetailBySlugQuery.cs
using BlogHybrid.Application.DTOs.Post;
using MediatR;

namespace BlogHybrid.Application.Queries.Post
{
    /// <summary>
    /// Query สำหรับดึงรายละเอียดโพสต์ตาม slug พร้อม comments
    /// </summary>
    public class GetPostDetailBySlugQuery : IRequest<PostDetailDto?>
    {
        public string Slug { get; set; } = string.Empty;
        public string? CurrentUserId { get; set; } // สำหรับเช็คสิทธิ์และ likes
    }
}