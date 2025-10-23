using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Domain.Entities;
using BlogHybrid.Infrastructure.Data;
using BlogHybrid.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlogHybrid.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private ICategoryRepository? _categories;
        private IUserRepository? _users;
        private ITagRepository? _tags;
        private ICommunityRepository? _communities;
        private IPostRepository? _posts;
        public UnitOfWork(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public DbContext DbContext => _context;
        // Existing repository property
        public ICategoryRepository Categories
        {
            get
            {
                _categories ??= new CategoryRepository(_context);
                return _categories;
            }
        }

        // New User repository property
        public IUserRepository Users
        {
            get
            {
                _users ??= new UserRepository(_userManager, _roleManager, _context);
                return _users;
            }
        }
        public ICommunityRepository Communities
        {
            get
            {
                _communities ??= new CommunityRepository(_context);
                return _communities;
            }
        }
        public ITagRepository Tags
        {
            get
            {
                _tags ??= new TagRepository(_context);
                return _tags;
            }
        }
        public IPostRepository Posts
        {
            get
            {
                _posts ??= new PostRepository(_context);
                return _posts;
            }
        }
        // Transaction methods
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Save changes
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Dispose
        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}