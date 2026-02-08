using DesktopMemo.Data.Persistence;

namespace DesktopMemo.Data.Infrastructure;

public interface IDbContextFactory
{
    DesktopMemoDbContext Create();
}
