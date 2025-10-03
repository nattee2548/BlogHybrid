using AutoMapper;
using BlogHybrid.Application.DTOs.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Community;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Community
{
    #region Get Communities Handler

    public class GetCommunitiesHandler : IRequestHandler<GetCommunitiesQuery, CommunityListDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCommunitiesHandler> _logger;

        public GetCommunitiesHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCommunitiesHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommunityListDto> Handle(GetCommunitiesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (communities, totalCount) = await _unitOfWork.Communities.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.CategoryId,
                    request.SearchTerm,
                    request.IsPrivate,
                    request.IsActive,
                    request.SortBy,
                    request.SortDirection,
                    cancellationToken);

                var communityDtos = new List<CommunityDto>();

                foreach (var community in communities)
                {
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

                    communityDtos.Add(dto);
                }

                return new CommunityListDto
                {
                    Communities = communityDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting communities");
                return new CommunityListDto
                {
                    Communities = new List<CommunityDto>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }
    }

    #endregion

    #region Get Communities By Category Handler

    public class GetCommunitiesByCategoryHandler : IRequestHandler<GetCommunitiesByCategoryQuery, List<CommunityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCommunitiesByCategoryHandler> _logger;

        public GetCommunitiesByCategoryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCommunitiesByCategoryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<CommunityDto>> Handle(GetCommunitiesByCategoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var communities = await _unitOfWork.Communities.GetByCategoryIdAsync(request.CategoryId, cancellationToken);

                // Filter based on privacy and active status
                if (request.OnlyActive)
                {
                    communities = communities.Where(c => c.IsActive).ToList();
                }

                if (!request.IncludePrivate && !string.IsNullOrEmpty(request.CurrentUserId))
                {
                    // Include public communities + private communities where user is member
                    var filteredCommunities = new List<BlogHybrid.Domain.Entities.Community>();

                    foreach (var community in communities)
                    {
                        if (!community.IsPrivate)
                        {
                            filteredCommunities.Add(community);
                        }
                        else if (await _unitOfWork.Communities.IsMemberAsync(community.Id, request.CurrentUserId, cancellationToken))
                        {
                            filteredCommunities.Add(community);
                        }
                    }

                    communities = filteredCommunities;
                }
                else if (!request.IncludePrivate)
                {
                    communities = communities.Where(c => !c.IsPrivate).ToList();
                }

                var communityDtos = new List<CommunityDto>();

                foreach (var community in communities)
                {
                    var dto = _mapper.Map<CommunityDto>(community);

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

                    communityDtos.Add(dto);
                }

                return communityDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting communities by category: {CategoryId}", request.CategoryId);
                return new List<CommunityDto>();
            }
        }
    }

    #endregion

    #region Get User Communities Handler

    public class GetUserCommunitiesHandler : IRequestHandler<GetUserCommunitiesQuery, List<CommunityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserCommunitiesHandler> _logger;

        public GetUserCommunitiesHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetUserCommunitiesHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<CommunityDto>> Handle(GetUserCommunitiesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var communities = await _unitOfWork.Communities.GetUserCommunitiesAsync(request.UserId, cancellationToken);

                var communityDtos = communities.Select(c => _mapper.Map<CommunityDto>(c)).ToList();

                return communityDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user communities: {UserId}", request.UserId);
                return new List<CommunityDto>();
            }
        }
    }

    #endregion

    #region Get Popular Communities Handler

    public class GetPopularCommunitiesHandler : IRequestHandler<GetPopularCommunitiesQuery, List<CommunityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPopularCommunitiesHandler> _logger;

        public GetPopularCommunitiesHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetPopularCommunitiesHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<CommunityDto>> Handle(GetPopularCommunitiesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (communities, _) = await _unitOfWork.Communities.GetPagedAsync(
                    pageNumber: 1,
                    pageSize: request.Top,
                    categoryId: request.CategoryId,
                    searchTerm: null,
                    isPrivate: request.OnlyPublic ? false : null,
                    isActive: true,
                    sortBy: "MemberCount",
                    sortDirection: "desc",
                    cancellationToken);

                var communityDtos = communities.Select(c => _mapper.Map<CommunityDto>(c)).ToList();

                return communityDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular communities");
                return new List<CommunityDto>();
            }
        }
    }

    #endregion
}