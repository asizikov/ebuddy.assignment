using Microsoft.Phone.Controls;
using eBuddy.Assignment.Utils;
using eBuddy.Assignment.ViewModel;

namespace eBuddy.Assignment.View
{
    public partial class BannerPage : PhoneApplicationPage
    {
        public BannerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = IoC.Current.Resolve<BannerViewModel>();
        }
    }
}