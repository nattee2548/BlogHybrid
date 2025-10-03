// BlogHybrid.Application/Mappings/TagMappingProfile.cs
using AutoMapper;
using BlogHybrid.Application.DTOs.Tag;
using BlogHybrid.Domain.Entities;

namespace BlogHybrid.Application.Mappings
{
    public class TagMappingProfile : Profile
    {
        public TagMappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.PostCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatorName, opt => opt.Ignore());

            CreateMap<Tag, CreateTagDto>().ReverseMap();
            CreateMap<Tag, UpdateTagDto>().ReverseMap();

            // Command to Entity mappings
            CreateMap<BlogHybrid.Application.Commands.Tag.CreateTagCommand, Tag>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore());

            CreateMap<BlogHybrid.Application.Commands.Tag.UpdateTagCommand, Tag>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Creator, opt => opt.Ignore())
                .ForMember(dest => dest.PostTags, opt => opt.Ignore());
        }
    }
}