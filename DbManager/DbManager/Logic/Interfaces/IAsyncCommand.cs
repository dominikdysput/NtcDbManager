using System.Threading.Tasks;

namespace DbManager.Infrastructure
{
    public interface IAsyncCommand
    {
        bool CanExecute();
        Task Execute();
    }
}
