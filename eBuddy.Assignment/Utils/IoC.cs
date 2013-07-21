using System.Windows;
using Funq;
using JetBrains.Annotations;
using eBuddy.Assignment.Navigation;
using eBuddy.Assignment.Networking;
using eBuddy.Assignment.ViewModel;

namespace eBuddy.Assignment.Utils
{
    public sealed class IoC
    {
        private readonly Container _container;
        [CanBeNull] private static IoC _instance;

        private IoC()
        {
            _container = new Container();
            _container.Register<IWebService>(container => new WebService());
            _container.Register<INavigationService>(container => new NavigationService(((App)Application.Current).RootFrame));
            _container.Register(c => new BannerViewModel(c.Resolve<IWebService>()));
            _container.Register(c => new MainViewModel(c.Resolve<INavigationService>()));
        }

        [NotNull]
        public static IoC Current
        {
            get
            {
                return _instance ?? (
                    _instance = new IoC());
            }
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
