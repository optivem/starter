namespace Dsl.Core.Shared;

public interface IUseCase<T>
{
    T Execute();
}
