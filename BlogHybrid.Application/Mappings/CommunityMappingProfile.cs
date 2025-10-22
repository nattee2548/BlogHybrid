using AutoMapper;
using BlogHybrid.Application.Commands.Community;
using BlogHybrid.Application.DTOs.Community;
using BlogHybrid.Domain.Entities;

namespace BlogHybrid.Application.Mappings
{
    public class CommunityMappingProfile : Profile
    {
        public CommunityMappingProfile()
        {
            // ========== Entity to DTO mappings ==========
            CreateMap<Community, CommunityDto>()
                // Primary category (first one for backward compatibility)
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src =>
                    src.CommunityCategories.OrderBy(cc => cc.AssignedAt).FirstOrDefault() != null
                        ? src.CommunityCategories.OrderBy(cc => cc.AssignedAt).FirstOrDefault()!.CategoryId
                        : 0))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    src.CommunityCategories.OrderBy(cc => cc.AssignedAt).FirstOrDefault() != null
                        ? src.CommunityCategories.OrderBy(cc => cc.AssignedAt).FirstOrDefault()!.Category.Name
                        : string.Empty))
                .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src =>
                    src.CommunityCategories.OrderBy(cc => cc.AssignedAt).FirstOrDefault() != null
                        ? src.CommunityCategories.OrderBy(cc => cc.AssignedAt).FirstOrDefault()!.Category.Slug
                        : string.Empty))
                // All categories
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                    src.CommunityCategories
                        .OrderBy(cc => cc.AssignedAt)
                        .Select(cc => new CommunityCategoryInfo
                        {
                            CategoryId = cc.CategoryId,
                            CategoryName = cc.Category.Name,
                            CategorySlug = cc.Category.Slug,
                            CategoryColor = cc.Category.Color
                        })
                        .ToList()))
                .ForMember(dest => dest.CreatorDisplayName, opt => opt.MapFrom(src => src.Creator.DisplayName))
                .ForMember(dest => dest.IsCurrentUserMember, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserRole, opt => opt.Ignore());

            // ========== ⭐ เพิ่ม Command to Entity mappings ⭐ ==========

            // CreateCommunityCommand -> Community
            CreateMap<CreateCommunityCommand, Community>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore()) // Generate in handler
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore()) // Set in handler
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.PostCount, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore()) // Old single category FK
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityMembers, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityInvites, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityCategories, opt => opt.Ignore()); // Many-to-many

            // UpdateCommunityCommand -> Community
            CreateMap<UpdateCommunityCommand, Community>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.PostCount, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore()) // Old single category FK
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityMembers, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityInvites, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityCategories, opt => opt.Ignore()); // Many-to-many

            // ========== DTO to Entity mappings ==========
            CreateMap<CreateCommunityDto, Community>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.PostCount, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityMembers, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityInvites, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityCategories, opt => opt.Ignore());

            CreateMap<UpdateCommunityDto, Community>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorId, opt => opt.Ignore())
                .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                .ForMember(dest => dest.PostCount, opt => opt.Ignore())
                .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityMembers, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityInvites, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.CommunityCategories, opt => opt.Ignore());

            // ========== CommunityMember mappings ==========
            CreateMap<CommunityMember, CommunityMemberDto>()
                .ForMember(dest => dest.UserDisplayName, opt => opt.MapFrom(src => src.User.DisplayName))
                .ForMember(dest => dest.UserProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // ========== Community -> CommunityDetailsDto ==========
            CreateMap<Community, CommunityDetailsDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    src.CommunityCategories.FirstOrDefault() != null
                        ? src.CommunityCategories.FirstOrDefault()!.Category.Name
                        : string.Empty))
                .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src =>
                    src.CommunityCategories.FirstOrDefault() != null
                        ? src.CommunityCategories.FirstOrDefault()!.Category.Slug
                        : string.Empty))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src =>
                    src.CommunityCategories.FirstOrDefault() != null
                        ? src.CommunityCategories.FirstOrDefault()!.CategoryId
                        : 0))
                .ForMember(dest => dest.CreatorDisplayName, opt => opt.MapFrom(src => src.Creator.DisplayName ?? src.Creator.UserName))
                .ForMember(dest => dest.IsCurrentUserMember, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentUserRole, opt => opt.Ignore())
                .ForMember(dest => dest.IsCreator, opt => opt.Ignore())
                .ForMember(dest => dest.IsAdmin, opt => opt.Ignore())
                .ForMember(dest => dest.IsModerator, opt => opt.Ignore())
                .ForMember(dest => dest.PendingMembersCount, opt => opt.Ignore());
        }
    }
}