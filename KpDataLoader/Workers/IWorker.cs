namespace KpDataLoader.Workers;

public interface IWorker
{
    Task<bool> RunAsync();
}