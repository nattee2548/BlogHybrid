// ไฟล์: BlogHybrid.Web/wwwroot/js/slugService.js

class SlugService {
    static thaiToEnglishMap = {
        // คำศัพท์ที่ใช้บ่อย
        'เทคโนโลยี': 'technology',
        'ข่าวสาร': 'news',
        'บทความ': 'articles',
        'การศึกษา': 'education',
        'ธุรกิจ': 'business',
        'การเงิน': 'finance',
        'กีฬา': 'sports',
        'ท่องเที่ยว': 'travel',
        'สุขภาพ': 'health',
        'อาหาร': 'food',
        'รถยนต์': 'automotive',
        'บันเทิง': 'entertainment',
        'ภาพยนตร์': 'movies',
        'ดนตรี': 'music',
        'แฟชั่น': 'fashion',
        'บ้านและสวน': 'home-garden',

        // พยัญชนะไทย
        'ก': 'g', 'ข': 'k', 'ค': 'k', 'ง': 'ng',
        'จ': 'j', 'ฉ': 'ch', 'ช': 'ch', 'ซ': 's',
        'ญ': 'y', 'ด': 'd', 'ต': 't', 'ถ': 'th',
        'ท': 'th', 'ธ': 'th', 'น': 'n', 'บ': 'b',
        'ป': 'p', 'ผ': 'ph', 'ฝ': 'f', 'พ': 'ph',
        'ฟ': 'f', 'ภ': 'ph', 'ม': 'm', 'ย': 'y',
        'ร': 'r', 'ล': 'l', 'ว': 'w', 'ศ': 's',
        'ษ': 's', 'ส': 's', 'ห': 'h', 'ฬ': 'l',
        'อ': 'o', 'ฮ': 'h',

        // สระไทย
        'ะ': 'a', 'า': 'a', 'ิ': 'i', 'ี': 'i',
        'ึ': 'ue', 'ื': 'ue', 'ุ': 'u', 'ู': 'u',
        'เ': 'e', 'แ': 'ae', 'โ': 'o', 'ใ': 'ai',
        'ไ': 'ai', 'ำ': 'am', 'ั': 'a', 'ํ': 'am'
    };

    static generateSlug(input, maxLength = 50) {
        if (!input) return '';

        let slug = input.trim().toLowerCase();

        // แปลงคำศัพท์ยาวก่อน
        const longWords = Object.entries(this.thaiToEnglishMap)
            .filter(([key]) => key.length > 1)
            .sort(([a], [b]) => b.length - a.length);

        for (const [thai, eng] of longWords) {
            slug = slug.replace(new RegExp(thai, 'g'), eng);
        }

        // แปลงตัวอักษรทีละตัว
        for (const [thai, eng] of Object.entries(this.thaiToEnglishMap)) {
            if (thai.length === 1) {
                slug = slug.replace(new RegExp(thai, 'g'), eng);
            }
        }

        // ลบวรรณยุกต์
        slug = slug.replace(/[่้๊๋์]/g, '');

        // แทนที่อักขระพิเศษด้วย -
        slug = slug.replace(/[^\w\-]/g, '-');

        // ลบ - ซ้ำ
        slug = slug.replace(/-+/g, '-');
        slug = slug.replace(/^-|-$/g, '');

        // จำกัดความยาว
        if (slug.length > maxLength) {
            slug = slug.substring(0, maxLength).replace(/-$/, '');
        }

        return slug || Math.random().toString(36).substring(2, 10);
    }

    static async checkSlugExists(slug, checkUrl) {
        try {
            const response = await fetch(`${checkUrl}?slug=${encodeURIComponent(slug)}`);
            const result = await response.json();
            return result.exists || false;
        } catch (error) {
            console.error('Error checking slug:', error);
            return true;
        }
    }

    static async updateSlugStatus(slug, checkUrl, statusElement) {
        if (!slug || !statusElement) return;

        try {
            const exists = await this.checkSlugExists(slug, checkUrl);

            if (exists) {
                statusElement.innerHTML = '<span class="text-danger"><i class="fas fa-times me-1"></i>URL นี้ถูกใช้งานแล้ว</span>';
            } else {
                statusElement.innerHTML = '<span class="text-success"><i class="fas fa-check me-1"></i>URL พร้อมใช้งาน</span>';
            }
        } catch (error) {
            statusElement.innerHTML = '<span class="text-warning"><i class="fas fa-exclamation-triangle me-1"></i>ไม่สามารถตรวจสอบ URL ได้</span>';
        }
    }
}

window.SlugService = SlugService;