namespace BlogHybrid.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedByIp { get; set; } = string.Empty;
        public DateTime? RevokedDate { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsRevoked => RevokedDate != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation property
        public ApplicationUser User { get; set; } = null!;
    }
}