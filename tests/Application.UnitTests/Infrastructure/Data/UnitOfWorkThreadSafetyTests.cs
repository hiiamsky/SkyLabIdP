using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Moq;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Data;
using Xunit;

namespace Application.UnitTests.Infrastructure.Data;

/// <summary>
/// UnitOfWork 線程安全測試（不需要真實資料庫連線）
/// </summary>
public class UnitOfWorkThreadSafetyTests
{
    private static IConfiguration CreateConfig(string connString = "Server=.;Database=Test;Trusted_Connection=True;")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = connString
            }!)
            .Build();
    }

    [Fact]
    public async Task CommitAsync_WithoutTransaction_ThrowsException()
    {
        await using var unitOfWork = new UnitOfWork(CreateConfig());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await unitOfWork.CommitAsync());

        Assert.Contains("No active transaction", exception.Message);
    }

    [Fact]
    public async Task RollbackAsync_WithoutTransaction_DoesNotThrow()
    {
        await using var unitOfWork = new UnitOfWork(CreateConfig());

        await unitOfWork.RollbackAsync();
    }

    [Fact]
    public async Task DisposeAsync_AfterDispose_PreventsFurtherOperations()
    {
        var unitOfWork = new UnitOfWork(CreateConfig());
        await unitOfWork.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await unitOfWork.BeginTransactionAsync());
    }

    [Fact]
    public async Task DisposeAsync_AfterDispose_CommitThrows()
    {
        var unitOfWork = new UnitOfWork(CreateConfig());
        await unitOfWork.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await unitOfWork.CommitAsync());
    }

    [Fact]
    public async Task DisposeAsync_AfterDispose_RollbackThrows()
    {
        var unitOfWork = new UnitOfWork(CreateConfig());
        await unitOfWork.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await unitOfWork.RollbackAsync());
    }

    [Fact]
    public async Task MultipleDisposeAsync_DoesNotThrow()
    {
        var unitOfWork = new UnitOfWork(CreateConfig());

        await unitOfWork.DisposeAsync();
        await unitOfWork.DisposeAsync();
        await unitOfWork.DisposeAsync();
    }

    [Fact]
    public void Constructor_WithMissingConnectionString_ThrowsException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()!)
            .Build();

        Assert.Throws<InvalidOperationException>(() => new UnitOfWork(config));
    }

    [Fact]
    public void Constructor_WithValidConfig_CreatesInstance()
    {
        var unitOfWork = new UnitOfWork(CreateConfig());

        Assert.NotNull(unitOfWork);
        Assert.Null(unitOfWork.CurrentTransaction);
    }
}
