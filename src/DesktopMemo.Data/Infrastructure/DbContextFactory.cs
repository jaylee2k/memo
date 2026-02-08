using DesktopMemo.Data.Persistence;

namespace DesktopMemo.Data.Infrastructure;

public class DbContextFactory : IDbContextFactory
{
    public DesktopMemoDbContext Create()
    {
        return new DesktopMemoDbContext();
    }
}
