// สร้างไฟล์ใหม่: BlogHybrid.Application/Services/SlugService.cs

using System.Text.RegularExpressions;

namespace BlogHybrid.Application.Services
{
    public interface ISlugService
    {
        string GenerateSlug(string input, int maxLength = 50);
        Task<string> GenerateUniqueSlugAsync(string input, Func<string, int?, Task<bool>> checkExists, int? excludeId = null, int maxLength = 50);
    }

    public class SlugService : ISlugService
    {
        private static readonly Dictionary<string, string> ThaiToEnglishMap = new()
        {
            // คำศัพท์ที่ใช้บ่อย (ตรวจสอบก่อนแปลงตัวอักษร)
            {"เทคโนโลยี", "technology"},
            {"ข่าวสาร", "news"},
            {"บทความ", "articles"},
            {"การศึกษา", "education"},
            {"ธุรกิจ", "business"},
            {"การเงิน", "finance"},
            {"กีฬา", "sports"},
            {"ท่องเที่ยว", "travel"},
            {"สุขภาพ", "health"},
            {"อาหาร", "food"},
            {"รถยนต์", "automotive"},
            {"บันเทิง", "entertainment"},
            {"ภาพยนตร์", "movies"},
            {"ดนตรี", "music"},
            {"แฟชั่น", "fashion"},
            {"บ้านและสวน", "home-garden"},
            {"การเมือง", "politics"},
            {"สิ่งแวดล้อม", "environment"},
            {"วิทยาศาสตร์", "science"},
            {"ประวัติศาสตร์", "history"},
            {"ศิลปะ", "art"},
            {"วรรณกรรม", "literature"},
            {"เกมส์", "games"},
            {"มือถือ", "mobile"},
            {"คอมพิวเตอร์", "computer"},
            {"อินเทอร์เน็ต", "internet"},

            // พยัญชนะไทย
            {"ก", "g"}, {"ข", "k"}, {"ค", "k"}, {"ง", "ng"},
            {"จ", "j"}, {"ฉ", "ch"}, {"ช", "ch"}, {"ซ", "s"},
            {"ญ", "y"}, {"ด", "d"}, {"ต", "t"}, {"ถ", "th"},
            {"ท", "th"}, {"ธ", "th"}, {"น", "n"}, {"บ", "b"},
            {"ป", "p"}, {"ผ", "ph"}, {"ฝ", "f"}, {"พ", "ph"},
            {"ฟ", "f"}, {"ภ", "ph"}, {"ม", "m"}, {"ย", "y"},
            {"ร", "r"}, {"ล", "l"}, {"ว", "w"}, {"ศ", "s"},
            {"ษ", "s"}, {"ส", "s"}, {"ห", "h"}, {"ฬ", "l"},
            {"อ", "o"}, {"ฮ", "h"},

            // สระไทย
            {"ะ", "a"}, {"า", "a"}, {"ิ", "i"}, {"ี", "i"},
            {"ึ", "ue"}, {"ื", "ue"}, {"ุ", "u"}, {"ู", "u"},
            {"เ", "e"}, {"แ", "ae"}, {"โ", "o"}, {"ใ", "ai"},
            {"ไ", "ai"}, {"ำ", "am"}, {"ั", "a"}, {"ํ", "am"},
            
            // เลขไทย
            {"๐", "0"}, {"๑", "1"}, {"๒", "2"}, {"๓", "3"}, {"๔", "4"},
            {"๕", "5"}, {"๖", "6"}, {"๗", "7"}, {"๘", "8"}, {"๙", "9"}
        };

        public string GenerateSlug(string input, int maxLength = 50)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var slug = input.Trim().ToLowerInvariant();

            // แปลงคำศัพท์ที่ใช้บ่อยก่อน (คำยาว)
            foreach (var pair in ThaiToEnglishMap.Where(x => x.Key.Length > 1).OrderByDescending(x => x.Key.Length))
            {
                slug = slug.Replace(pair.Key, pair.Value);
            }

            // แปลงตัวอักษรทีละตัว
            foreach (var pair in ThaiToEnglishMap.Where(x => x.Key.Length == 1))
            {
                slug = slug.Replace(pair.Key, pair.Value);
            }

            // ลบวรรณยุกต์และการันต์
            slug = Regex.Replace(slug, @"[่้๊๋์]", "");

            // แทนที่ช่องว่างและอักขระพิเศษด้วย -
            slug = Regex.Replace(slug, @"[^\w\-]", "-");

            // ลบ - ที่ติดกันและที่ต้นท้าย
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            // จำกัดความยาว
            if (slug.Length > maxLength)
            {
                slug = slug.Substring(0, maxLength).TrimEnd('-');
            }

            // ถ้าเป็นช่องว่างหรือว่างเปล่า ให้ส่งกลับ random string
            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = Guid.NewGuid().ToString("N")[..8];
            }

            return slug;
        }

        public async Task<string> GenerateUniqueSlugAsync(string input, Func<string, int?, Task<bool>> checkExists, int? excludeId = null, int maxLength = 50)
        {
            var baseSlug = GenerateSlug(input, maxLength - 10); // เผื่อพื้นที่สำหรับตัวเลข
            var slug = baseSlug;
            var counter = 1;

            while (await checkExists(slug, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;

                // ป้องกันการวนซ้ำไม่สิ้นสุด
                if (counter > 999)
                {
                    slug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..8]}";
                    break;
                }
            }

            return slug;
        }
    }
}