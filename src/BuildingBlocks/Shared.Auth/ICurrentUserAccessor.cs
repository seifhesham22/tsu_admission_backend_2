namespace Shared.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser Get();

    bool TryGet(out CurrentUser? user);
}
