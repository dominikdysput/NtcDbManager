namespace DbManager.Infrastructure
{
    public interface IIoc
    {
        T GetInstance<T>() where T : class;
    }
}
