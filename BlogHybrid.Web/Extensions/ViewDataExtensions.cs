using BlogHybrid.Web.Helpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BlogHybrid.Web.Extensions
{
    /// <summary>
    /// Extension Methods สำหรับ ViewData เพื่อจัดการ SEO Metadata
    /// </summary>
    public static class ViewDataExtensions
    {
        private const string SeoMetadataKey = "SeoMetadata";

        /// <summary>
        /// เพิ่ม SEO Metadata ลงใน ViewData
        /// </summary>
        public static void SetSeoMetadata(this ViewDataDictionary viewData, SeoMetadata metadata)
        {
            viewData[SeoMetadataKey] = metadata;

            // เก็บ Title แยกสำหรับใช้งานง่าย
            viewData["Title"] = metadata.Title;
        }

        /// <summary>
        /// ดึง SEO Metadata จาก ViewData
        /// </summary>
        public static SeoMetadata? GetSeoMetadata(this ViewDataDictionary viewData)
        {
            return viewData[SeoMetadataKey] as SeoMetadata;
        }

        /// <summary>
        /// สร้างและเพิ่ม Basic SEO (แบบสั้น)
        /// </summary>
        public static void SetBasicSeo(
            this ViewDataDictionary viewData,
            string title,
            string description,
            string? keywords = null,
            string? ogImage = null)
        {
            var metadata = SeoHelper.CreateBasicSeo(title, description, keywords, ogImage);
            viewData.SetSeoMetadata(metadata);
        }

        /// <summary>
        /// สร้างและเพิ่ม Article SEO (สำหรับบทความ)
        /// </summary>
        public static void SetArticleSeo(
            this ViewDataDictionary viewData,
            string title,
            string description,
            string authorName,
            DateTime publishedDate,
            DateTime? modifiedDate = null,
            string? featuredImage = null,
            List<string>? tags = null,
            string? category = null)
        {
            var metadata = SeoHelper.CreateArticleSeo(
                title,
                description,
                authorName,
                publishedDate,
                modifiedDate,
                featuredImage,
                tags,
                category);

            viewData.SetSeoMetadata(metadata);
        }

        /// <summary>
        /// สร้างและเพิ่ม Profile SEO (สำหรับโปรไฟล์)
        /// </summary>
        public static void SetProfileSeo(
            this ViewDataDictionary viewData,
            string userName,
            string bio,
            string? profileImage = null)
        {
            var metadata = SeoHelper.CreateProfileSeo(userName, bio, profileImage);
            viewData.SetSeoMetadata(metadata);
        }

        /// <summary>
        /// เพิ่ม Canonical URL
        /// </summary>
        public static void SetCanonicalUrl(this ViewDataDictionary viewData, string canonicalUrl)
        {
            var metadata = viewData.GetSeoMetadata();
            if (metadata != null)
            {
                metadata.CanonicalUrl = canonicalUrl;
            }
        }

        /// <summary>
        /// เพิ่ม Structured Data
        /// </summary>
        public static void SetStructuredData(this ViewDataDictionary viewData, string structuredData)
        {
            var metadata = viewData.GetSeoMetadata();
            if (metadata != null)
            {
                metadata.StructuredData = structuredData;
            }
        }

        /// <summary>
        /// เพิ่ม Robots Meta Tag
        /// </summary>
        public static void SetRobots(this ViewDataDictionary viewData, string robots)
        {
            var metadata = viewData.GetSeoMetadata();
            if (metadata != null)
            {
                metadata.Robots = robots;
            }
        }

        /// <summary>
        /// ตรวจสอบว่ามี SEO Metadata หรือไม่
        /// </summary>
        public static bool HasSeoMetadata(this ViewDataDictionary viewData)
        {
            return viewData.GetSeoMetadata() != null;
        }
    }
}


