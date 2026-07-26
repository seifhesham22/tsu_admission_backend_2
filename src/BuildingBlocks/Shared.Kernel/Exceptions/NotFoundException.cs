namespace Shared.Kernel.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException For<T>(object key) =>
        new($"{typeof(T).Name} with identifier '{key}' was not found.");
}
