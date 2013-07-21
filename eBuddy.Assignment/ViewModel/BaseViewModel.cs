using System.ComponentModel;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace eBuddy.Assignment.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                SmartDispatcher.BeginInvoke(
                    () =>
                        {
                            handler(this, new PropertyChangedEventArgs(propertyName));
                        }
                    );
                
            }
        }
    }
}