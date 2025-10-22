// ไฟล์ใหม่: BlogHybrid.Domain/Enums/CommunityMemberStatus.cs
namespace BlogHybrid.Domain.Enums
{
    public enum CommunityMemberStatus
    {
        Creator = 0,      // เจ้าของชุมชน
        Approved = 1,     // สมาชิกที่อนุมัติแล้ว
        Pending = 2,      // รอการอนุมัติ
        Banned = 3        // ถูกแบน
    }
}