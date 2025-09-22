using AutoMapper;
using BlogHybrid.Application.DTOs.Category;
using BlogHybrid.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Mappings
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Entity to DTO mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.PostCount, opt => opt.Ignore()); // Will be set manually in handlers

            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Category, UpdateCategoryDto>().ReverseMap();

            // Command to Entity mappings
            CreateMap<BlogHybrid.Application.Commands.Category.CreateCategoryCommand, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore());

            CreateMap<BlogHybrid.Application.Commands.Category.UpdateCategoryCommand, Category>()
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore());
        }
    }
}
