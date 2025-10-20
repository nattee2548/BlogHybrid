namespace BlogHybrid.Application.Configuration
{
    public class CommunitySettings
    {
        public const string SectionName = "CommunitySettings";

        public int MaxCommunitiesPerUser { get; set; } = 2;
        public int MaxCategoriesPerCommunity { get; set; } = 3;
        public int SoftDeleteRetentionDays { get; set; } = 30;
        public int InviteExpiryDays { get; set; } = 7;
        public int MaxMembersPerCommunity { get; set; } = 10000;
        public int MinNameLength { get; set; } = 3;
        public int MaxNameLength { get; set; } = 100;
        public int MaxDescriptionLength { get; set; } = 1000;
        public int MaxRulesLength { get; set; } = 5000;
    }
}