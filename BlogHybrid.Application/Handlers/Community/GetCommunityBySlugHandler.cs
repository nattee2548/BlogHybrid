using AutoMapper;
using BlogHybrid.Application.DTOs.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Community;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlogHybrid.Application.Handlers.Community
{
    #region Get Community By Slug Handler

    public class GetCommunityBySlugHandler : IRequestHandler<GetCommunityBySlugQuery, CommunityDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCommunityBySlugHandler> _logger;

        public GetCommunityBySlugHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCommunityBySlugHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommunityDto?> Handle(GetCommunityBySlugQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var community = await _unitOfWork.Communities.GetBySlugWithDetailsAsync(request.Slug, cancellationToken);

                if (community == null)
                    return null;

                // Check if user can access this community
                if (community.IsPrivate && !string.IsNullOrEmpty(request.CurrentUserId))
                {
                    var isMember = await _unitOfWork.Communities
                        .IsMemberAsync(community.Id, request.CurrentUserId, cancellationToken);

                    if (!isMember)
                    {
                        _logger.LogWarning("User {UserId} attempted to access private community {Slug}",
                            request.CurrentUserId, request.Slug);
                        return null;
                    }
                }
                else if (community.IsPrivate)
                {
                    return null;
                }

                var dto = _mapper.Map<CommunityDto>(community);

                // Set current user membership status
                if (!string.IsNullOrEmpty(request.CurrentUserId))
                {
                    dto.IsCurrentUserMember = await _unitOfWork.Communities
                        .IsMemberAsync(community.Id, request.CurrentUserId, cancellationToken);

                    if (dto.IsCurrentUserMember == true)
                    {
                        var member = await _unitOfWork.Communities
                            .GetMemberAsync(community.Id, request.CurrentUserId, cancellationToken);
                        dto.CurrentUserRole = member?.Role;
                    }
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting community by slug: {Slug}", request.Slug);
                return null;
            }
        }
    }

    #endregion

    #region Check User Community Limit Handler

    public class CheckUserCommunityLimitHandler : IRequestHandler<CheckUserCommunityLimitQuery, CheckUserCommunityLimitResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<BlogHybrid.Application.Configuration.CommunitySettings> _communitySettings;

        public CheckUserCommunityLimitHandler(
            IUnitOfWork unitOfWork,
            IOptions<BlogHybrid.Application.Configuration.CommunitySettings> communitySettings)
        {
            _unitOfWork = unitOfWork;
            _communitySettings = communitySettings;
        }

        public async Task<CheckUserCommunityLimitResult> Handle(CheckUserCommunityLimitQuery request, CancellationToken cancellationToken)
        {
            var currentCount = await _unitOfWork.Communities
                .GetUserCommunityCountAsync(request.UserId, includeDeleted: false, cancellationToken);

            var maxAllowed = _communitySettings.Value.MaxCommunitiesPerUser;

            return new CheckUserCommunityLimitResult
            {
                CanCreateMore = currentCount < maxAllowed,
                CurrentCount = currentCount,
                MaxAllowed = maxAllowed
            };
        }
    }

    #endregion

    #region Check Community Slug Exists Handler

    public class CheckCommunitySlugExistsHandler : IRequestHandler<CheckCommunitySlugExistsQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckCommunitySlugExistsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CheckCommunitySlugExistsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Communities.SlugExistsAsync(request.Slug, request.ExcludeId, cancellationToken);
        }
    }

    #endregion

    #region Get Community Stats Handler

    public class GetCommunityStatsHandler : IRequestHandler<GetCommunityStatsQuery, CommunityStatsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCommunityStatsHandler> _logger;

        public GetCommunityStatsHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCommunityStatsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CommunityStatsDto> Handle(GetCommunityStatsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (communities, totalCount) = await _unitOfWork.Communities.GetPagedAsync(
                    pageNumber: 1,
                    pageSize: int.MaxValue,
                    categoryId: request.CategoryId,
                    cancellationToken: cancellationToken);

                var stats = new CommunityStatsDto
                {
                    TotalCommunities = totalCount,
                    ActiveCommunities = communities.Count(c => c.IsActive),
                    PrivateCommunities = communities.Count(c => c.IsPrivate),
                    PublicCommunities = communities.Count(c => !c.IsPrivate),
                    TotalMembers = communities.Sum(c => c.MemberCount),
                    TotalPosts = communities.Sum(c => c.PostCount)
                };

                // Group by category
                var categoryGroups = communities.GroupBy(c => new { c.CategoryId, c.Category.Name });
                stats.CommunitiesByCategory = categoryGroups.Select(g => new CategoryCommunityStat
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    CommunityCount = g.Count()
                }).ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting community stats");
                return new CommunityStatsDto();
            }
        }
    }

    #endregion
}