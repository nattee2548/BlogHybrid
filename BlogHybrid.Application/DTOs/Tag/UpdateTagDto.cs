// BlogHybrid.Application/DTOs/Tag/UpdateTagDto.cs
namespace BlogHybrid.Application.DTOs.Tag
{
    public class UpdateTagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}