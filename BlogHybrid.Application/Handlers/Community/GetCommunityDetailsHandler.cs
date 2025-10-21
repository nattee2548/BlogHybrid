using AutoMapper;
using BlogHybrid.Application.DTOs.Community;
using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Community;
using BlogHybrid.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Community
{
    public class GetCommunityDetailsHandler : IRequestHandler<GetCommunityDetailsQuery, CommunityDetailsDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCommunityDetailsHandler> _logger;

        public GetCommunityDetailsHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCommunityDetailsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommunityDetailsDto?> Handle(GetCommunityDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. ดึงข้อมูล Community
                var community = await _unitOfWork.Communities.GetBySlugWithDetailsAsync(request.Slug, cancellationToken);

                if (community == null)
                {
                    _logger.LogWarning("Community with slug {Slug} not found", request.Slug);
                    return null;
                }

                // 2. Map ข้อมูลพื้นฐาน
                var dto = _mapper.Map<CommunityDetailsDto>(community);

                // 3. ตรวจสอบสิทธิ์และข้อมูลเพิ่มเติมของ Current User
                if (!string.IsNullOrEmpty(request.CurrentUserId))
                {
                    // ตรวจสอบว่า user เป็นสมาชิกหรือไม่
                    var membership = await _unitOfWork.Communities
                        .GetMemberAsync(community.Id, request.CurrentUserId, cancellationToken);

                    if (membership != null && membership.IsApproved && !membership.IsBanned)
                    {
                        dto.IsCurrentUserMember = true;
                        dto.CurrentUserRole = membership.Role;
                    }
                    else
                    {
                        dto.IsCurrentUserMember = false;
                    }

                    // ตรวจสอบว่าเป็น Creator, Admin, หรือ Moderator
                    dto.IsCreator = community.CreatorId == request.CurrentUserId;
                    dto.IsAdmin = membership?.Role == CommunityRole.Admin;
                    dto.IsModerator = membership?.Role == CommunityRole.Moderator;

                    // 4. นับจำนวน Pending Members (เฉพาะผู้ที่มีสิทธิ์จัดการ)
                    if (dto.CanManageMembers)
                    {
                        dto.PendingMembersCount = await _unitOfWork.Communities
                            .GetPendingMembersCountAsync(community.Id, cancellationToken);

                        _logger.LogInformation("User {UserId} can manage members. Pending count: {Count}",
                            request.CurrentUserId, dto.PendingMembersCount);
                    }
                }
                else
                {
                    // User ไม่ได้ล็อกอิน
                    dto.IsCurrentUserMember = false;
                    dto.IsCreator = false;
                    dto.IsAdmin = false;
                    dto.IsModerator = false;
                    dto.PendingMembersCount = 0;
                }

                // 5. ตรวจสอบการเข้าถึง Private Community
                if (community.IsPrivate && dto.IsCurrentUserMember != true && !dto.IsCreator)
                {
                    _logger.LogWarning("User {UserId} attempted to access private community {Slug}",
                        request.CurrentUserId ?? "Anonymous", request.Slug);
                    return null;
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting community details for slug: {Slug}", request.Slug);
                throw;
            }
        }
    }
}