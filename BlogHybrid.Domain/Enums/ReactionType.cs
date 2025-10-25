namespace BlogHybrid.Domain.Enums
{
    /// <summary>
    /// ประเภทของ Reaction บน Comment (เหมือน Facebook Reactions)
    /// </summary>
    public enum ReactionType
    {
        /// <summary>
        /// 😊 Like - ชอบ
        /// </summary>
        Like = 1,

        /// <summary>
        /// ❤️ Love - รัก
        /// </summary>
        Love = 2,

        /// <summary>
        /// 😂 Haha - ตลก
        /// </summary>
        Haha = 3,

        /// <summary>
        /// 😮 Wow - ประหลาดใจ
        /// </summary>
        Wow = 4,

        /// <summary>
        /// 😢 Sad - เศร้า
        /// </summary>
        Sad = 5,

        /// <summary>
        /// 😡 Angry - โกรธ
        /// </summary>
        Angry = 6
    }
}