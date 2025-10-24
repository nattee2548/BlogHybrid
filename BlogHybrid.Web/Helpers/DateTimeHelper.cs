// BlogHybrid.Web/Helpers/DateTimeHelper.cs
namespace BlogHybrid.Web.Helpers
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// แปลง DateTime เป็นข้อความ "Time Ago" ภาษาไทย
        /// </summary>
        public static string ToTimeAgo(this DateTime dateTime)
        {
            var timeDiff = DateTime.UtcNow - dateTime;

            if (timeDiff.TotalMinutes < 1)
            {
                return "เมื่อสักครู่";
            }
            else if (timeDiff.TotalMinutes < 60)
            {
                return $"{(int)timeDiff.TotalMinutes} นาทีที่แล้ว";
            }
            else if (timeDiff.TotalHours < 24)
            {
                return $"{(int)timeDiff.TotalHours} ชั่วโมงที่แล้ว";
            }
            else if (timeDiff.TotalDays < 7)
            {
                return $"{(int)timeDiff.TotalDays} วันที่แล้ว";
            }
            else if (timeDiff.TotalDays < 30)
            {
                return $"{(int)(timeDiff.TotalDays / 7)} สัปดาห์ที่แล้ว";
            }
            else if (timeDiff.TotalDays < 365)
            {
                return $"{(int)(timeDiff.TotalDays / 30)} เดือนที่แล้ว";
            }
            else
            {
                return dateTime.ToString("dd MMM yyyy",
                    new System.Globalization.CultureInfo("th-TH"));
            }
        }

        /// <summary>
        /// แปลง DateTime เป็นข้อความแบบสั้น (สำหรับ comment)
        /// </summary>
        public static string ToShortTimeAgo(this DateTime dateTime)
        {
            var timeDiff = DateTime.UtcNow - dateTime;

            if (timeDiff.TotalMinutes < 1)
                return "เมื่อสักครู่";
            else if (timeDiff.TotalMinutes < 60)
                return $"{(int)timeDiff.TotalMinutes}น";
            else if (timeDiff.TotalHours < 24)
                return $"{(int)timeDiff.TotalHours}ชม";
            else if (timeDiff.TotalDays < 7)
                return $"{(int)timeDiff.TotalDays}ว";
            else if (timeDiff.TotalDays < 30)
                return $"{(int)(timeDiff.TotalDays / 7)}สัปดาห์";
            else
                return dateTime.ToString("dd MMM",
                    new System.Globalization.CultureInfo("th-TH"));
        }
    }
}