using System.Windows.Input;
using JetBrains.Annotations;
using eBuddy.Assignment.Navigation;
using eBuddy.Assignment.Utils;

namespace eBuddy.Assignment.ViewModel
{
    public class MainViewModel: BaseViewModel
    {
        [NotNull] private readonly INavigationService _navigationService;
        private bool _navigationStarted;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            navigationService.ThrowIfNull("navigationService");
        }

        private ICommand _showBannerCommand;
        [NotNull, UsedImplicitly]   public  ICommand ShowBannerCommand
        {
            get
            {
                return _showBannerCommand ?? 
                    (_showBannerCommand = new SimpleCommand(Navigate));
            }
        }

        private void Navigate()
        {
            _navigationService.Navigate(Pages.BannerPage);
        }
    }
}
