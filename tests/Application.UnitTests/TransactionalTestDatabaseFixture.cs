using SkyLabIdP.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class TransactionalTestDatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }

    public TransactionalTestDatabaseFixture()
    {
        // 使用 InMemory 資料庫進行測試，避免外部資料庫依賴
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // 每次測試使用不同的資料庫名稱
            .Options;

        Context = new ApplicationDbContext(options);
        
        // InMemory 資料庫會自動創建，不需要遷移
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        // InMemory 資料庫會在 Context 被釋放時自動清理
        Context.Dispose();
    }
}