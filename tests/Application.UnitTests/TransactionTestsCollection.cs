namespace Application.UnitTests;

using Xunit;

[CollectionDefinition("TransactionalTests")]
public class TransactionTestsCollection : ICollectionFixture<TransactionalTestDatabaseFixture>
{
}
