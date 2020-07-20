using SimpleInjector;

namespace DbManager.Infrastructure
{
    class SimpleInjectorIoc : IIoc
    {
        private Container _container;

        public SimpleInjectorIoc(Container simpleInjector)
        {
            _container = simpleInjector;
        }

        public T GetInstance<T>() where T : class
        {
            return _container.GetInstance<T>();
        }
    }
}
