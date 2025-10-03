using AutoMapper;
using BlogHybrid.Application.DTOs.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Community;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Community
{
    public class GetCommunityByIdHandler : IRequestHandler<GetCommunityByIdQuery, CommunityDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCommunityByIdHandler> _logger;

        public GetCommunityByIdHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCommunityByIdHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommunityDto?> Handle(GetCommunityByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var community = await _unitOfWork.Communities.GetByIdWithDetailsAsync(request.Id, cancellationToken);

                if (community == null)
                    return null;

                // Check if user can access this community
                if (community.IsPrivate && !string.IsNullOrEmpty(request.CurrentUserId))
                {
                    var isMember = await _unitOfWork.Communities
                        .IsMemberAsync(community.Id, request.CurrentUserId, cancellationToken);

                    if (!isMember)
                    {
                        _logger.LogWarning("User {UserId} attempted to access private community {CommunityId}",
                            request.CurrentUserId, community.Id);
                        return null; // Don't reveal that private community exists
                    }
                }
                else if (community.IsPrivate)
                {
                    // Guest user trying to access private community
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
                _logger.LogError(ex, "Error getting community by id: {CommunityId}", request.Id);
                return null;
            }
        }
    }
}