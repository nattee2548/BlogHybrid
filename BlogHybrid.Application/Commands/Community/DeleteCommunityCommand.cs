using MediatR;

namespace BlogHybrid.Application.Commands.Community
{
    public class DeleteCommunityCommand : IRequest<DeleteCommunityResult>
    {
        public int Id { get; set; }

        // จะถูก set จาก Controller (Current User)
        public string? CurrentUserId { get; set; }

        // สำหรับ admin - force permanent delete
        public bool PermanentDelete { get; set; } = false;
    }

    public class DeleteCommunityResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsSoftDeleted { get; set; } // true = soft delete, false = permanent
        public DateTime? CanRestoreUntil { get; set; } // วันที่จะถูกลบถาวร
    }

    public class RestoreCommunityCommand : IRequest<RestoreCommunityResult>
    {
        public int Id { get; set; }

        // จะถูก set จาก Controller (Current User)
        public string? CurrentUserId { get; set; }
    }

    public class RestoreCommunityResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ToggleCommunityStatusCommand : IRequest<ToggleCommunityStatusResult>
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }

        // จะถูก set จาก Controller (Current User)
        public string? CurrentUserId { get; set; }
    }

    public class ToggleCommunityStatusResult
    {
        public bool Success { get; set; }
        public bool NewStatus { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}