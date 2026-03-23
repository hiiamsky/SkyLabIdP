using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Data.Repositories;

namespace SkyLabIdP.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SqlConnection _connection;
    private IDbTransaction? _transaction;

    private readonly SemaphoreSlim _transactionLock = new(1, 1);
    private readonly object _repositoryLock = new();
    private bool _disposed;

    private IUserTenantRepository? _userTenants;
    private IPasswordHistoryRepository? _passwordHistories;
    private ISkyLabDocUserDetailRepository? _skyLabDocUserDetails;
    private ISkyLabDevelopUserDetailRepository? _skyLabDevelopUserDetails;
    private IAuditLogRepository? _auditLogs;
    private IBranchRepository? _branches;
    private IFunctionGroupRepository? _functionGroups;
    private IFunctionRepository? _functions;
    private IFileUploadRepository? _fileUploads;
    private ISysCodeRepository? _sysCodes;
    private IBranchAreaRepository? _branchAreas;

    public UnitOfWork(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _connection = new SqlConnection(connectionString);
    }

    public IDbTransaction? CurrentTransaction => _transaction;

    public IUserTenantRepository UserTenants
    {
        get
        {
            if (_userTenants == null)
            {
                lock (_repositoryLock)
                {
                    _userTenants ??= new UserTenantRepository(GetOpenConnection(), _transaction);
                }
            }
            return _userTenants;
        }
    }

    public IPasswordHistoryRepository PasswordHistories
    {
        get
        {
            if (_passwordHistories == null)
            {
                lock (_repositoryLock)
                {
                    _passwordHistories ??= new PasswordHistoryRepository(GetOpenConnection(), _transaction);
                }
            }
            return _passwordHistories;
        }
    }

    public ISkyLabDocUserDetailRepository SkyLabDocUserDetails
    {
        get
        {
            if (_skyLabDocUserDetails == null)
            {
                lock (_repositoryLock)
                {
                    _skyLabDocUserDetails ??= new SkyLabDocUserDetailRepository(GetOpenConnection(), _transaction);
                }
            }
            return _skyLabDocUserDetails;
        }
    }

    public ISkyLabDevelopUserDetailRepository SkyLabDevelopUserDetails
    {
        get
        {
            if (_skyLabDevelopUserDetails == null)
            {
                lock (_repositoryLock)
                {
                    _skyLabDevelopUserDetails ??= new SkyLabDevelopUserDetailRepository(GetOpenConnection(), _transaction);
                }
            }
            return _skyLabDevelopUserDetails;
        }
    }

    public IAuditLogRepository AuditLogs
    {
        get
        {
            if (_auditLogs == null)
            {
                lock (_repositoryLock)
                {
                    _auditLogs ??= new AuditLogRepository(GetOpenConnection(), _transaction);
                }
            }
            return _auditLogs;
        }
    }

    public IBranchRepository Branches
    {
        get
        {
            if (_branches == null)
            {
                lock (_repositoryLock)
                {
                    _branches ??= new BranchRepository(GetOpenConnection(), _transaction);
                }
            }
            return _branches;
        }
    }

    public IFunctionGroupRepository FunctionGroups
    {
        get
        {
            if (_functionGroups == null)
            {
                lock (_repositoryLock)
                {
                    _functionGroups ??= new FunctionGroupRepository(GetOpenConnection(), _transaction);
                }
            }
            return _functionGroups;
        }
    }

    public IFunctionRepository Functions
    {
        get
        {
            if (_functions == null)
            {
                lock (_repositoryLock)
                {
                    _functions ??= new FunctionRepository(GetOpenConnection(), _transaction);
                }
            }
            return _functions;
        }
    }

    public IFileUploadRepository FileUploads
    {
        get
        {
            if (_fileUploads == null)
            {
                lock (_repositoryLock)
                {
                    _fileUploads ??= new FileUploadRepository(GetOpenConnection(), _transaction);
                }
            }
            return _fileUploads;
        }
    }

    public ISysCodeRepository SysCodes
    {
        get
        {
            if (_sysCodes == null)
            {
                lock (_repositoryLock)
                {
                    _sysCodes ??= new SysCodeRepository(GetOpenConnection(), _transaction);
                }
            }
            return _sysCodes;
        }
    }

    public IBranchAreaRepository BranchAreas
    {
        get
        {
            if (_branchAreas == null)
            {
                lock (_repositoryLock)
                {
                    _branchAreas ??= new BranchAreaRepository(GetOpenConnection(), _transaction);
                }
            }
            return _branchAreas;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _transactionLock.WaitAsync(cancellationToken);
        try
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException(
                    "A transaction is already in progress. Commit or rollback the existing transaction before starting a new one.");
            }

            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync(cancellationToken);
            }
            else if (_connection.State == ConnectionState.Broken)
            {
                _connection.Close();
                await _connection.OpenAsync(cancellationToken);
            }

            _transaction = await _connection.BeginTransactionAsync(cancellationToken);
            ResetRepositories();
        }
        finally
        {
            _transactionLock.Release();
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _transactionLock.WaitAsync(cancellationToken);
        try
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            await ((DbTransaction)_transaction).CommitAsync(cancellationToken);
            await DisposeTransactionAsync();
        }
        finally
        {
            _transactionLock.Release();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_transaction == null) return;

        await _transactionLock.WaitAsync(cancellationToken);
        try
        {
            if (_transaction != null)
            {
                await ((DbTransaction)_transaction).RollbackAsync(cancellationToken);
                await DisposeTransactionAsync();
            }
        }
        finally
        {
            _transactionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await _transactionLock.WaitAsync();
        try
        {
            if (_transaction != null)
                await DisposeTransactionAsync();
        }
        finally
        {
            _transactionLock.Release();
        }

        await _connection.DisposeAsync();
        _transactionLock.Dispose();
        _disposed = true;
    }

    private SqlConnection GetOpenConnection()
    {
        ThrowIfDisposed();

        lock (_repositoryLock)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
            else if (_connection.State == ConnectionState.Broken)
            {
                _connection.Close();
                _connection.Open();
            }

            return _connection;
        }
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (_transaction is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else
            _transaction?.Dispose();

        _transaction = null;
        ResetRepositories();
    }

    private void ResetRepositories()
    {
        lock (_repositoryLock)
        {
            _userTenants = null;
            _passwordHistories = null;
            _skyLabDocUserDetails = null;
            _skyLabDevelopUserDetails = null;
            _auditLogs = null;
            _branches = null;
            _functionGroups = null;
            _functions = null;
            _fileUploads = null;
            _sysCodes = null;
            _branchAreas = null;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
