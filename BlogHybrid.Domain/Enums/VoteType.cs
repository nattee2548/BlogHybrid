namespace BlogHybrid.Domain.Enums
{
    /// <summary>
    /// ประเภทของการโหวต Comment
    /// </summary>
    public enum VoteType
    {
        /// <summary>
        /// Upvote - เพิ่มคะแนน (+1)
        /// </summary>
        Upvote = 1,

        /// <summary>
        /// Downvote - ลดคะแนน (-1)
        /// </summary>
        Downvote = -1
    }
}