using MediatR;

namespace BlogHybrid.Application.Queries.Auth
{
    public class CheckUserExistsQuery : IRequest<CheckUserExistsResult>
    {
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
    }

    public class CheckUserExistsResult
    {
        public bool EmailExists { get; set; }
        public bool DisplayNameExists { get; set; }
    }
}