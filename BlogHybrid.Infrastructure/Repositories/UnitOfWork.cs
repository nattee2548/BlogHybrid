//using BlogHybrid.Application.Interfaces.Repositories;
//using BlogHybrid.Domain.Entities;
//using BlogHybrid.Infrastructure.Data;
//using Microsoft.EntityFrameworkCore.Storage;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BlogHybrid.Infrastructure.Repositories
//{
//    public class UnitOfWork : IUnitOfWork
//    {
//        private readonly ApplicationDbContext _context;
//        private IDbContextTransaction? _transaction;

//        private ICategoryRepository? _categories;

//        public UnitOfWork(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public ICategoryRepository Categories =>
//            _categories ??= new CategoryRepository(_context);


//        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//        {
//            return await _context.SaveChangesAsync(cancellationToken);
//        }

//        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
//        {
//            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
//        }

//        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
//        {
//            if (_transaction != null)
//            {
//                await _transaction.CommitAsync(cancellationToken);
//                await _transaction.DisposeAsync();
//                _transaction = null;
//            }
//        }

//        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
//        {
//            if (_transaction != null)
//            {
//                await _transaction.RollbackAsync(cancellationToken);
//                await _transaction.DisposeAsync();
//                _transaction = null;
//            }
//        }

//        public void Dispose()
//        {
//            _transaction?.Dispose();
//            _context.Dispose();
//        }
//    }
//}


using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Infrastructure.Data;
using BlogHybrid.Infrastructure.Repositories;
using BlogHybrid.Domain.Entities;
using Microsoft.AspNetCore.Identity;
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

        public UnitOfWork(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

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