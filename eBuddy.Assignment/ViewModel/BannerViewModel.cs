using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using JetBrains.Annotations;
using eBuddy.Assignment.Networking;
using eBuddy.Assignment.Networking.Parsers;
using eBuddy.Assignment.Utils;

namespace eBuddy.Assignment.ViewModel
{
    public class BannerViewModel : BaseViewModel
    {

        private readonly string balancerMessage =
    "e_action=check;e_version=J2ME2;e_device=TestDevice;e_build=1.5;e_operator=TestOperator\n";

        private readonly string serverMessage =
            "e_action=get_banner;e_device=nokia_6300;e_version=J2ME2;e_format=png;e_ip=62.69.184.55;e_operator=dev;e_width=216;e_height=160\n";

        [NotNull]
        private readonly IWebService _webService;
        [CanBeNull]
        private EmfMessage _message;

        public BannerViewModel(IWebService webService)
        {
            webService.ThrowIfNull("webService");

            _webService = webService;
            GetMessageAsync();
        }

        private void GetMessageAsync()
        {
            IsLoading = true;
            IsFailed = false;
            IsBannerLoaded = false;

            Banner = null;
            TextBanner = null;

            _webService.GetBalancerResponce(balancerMessage)
                .Subscribe(responce =>
                    {
                        if (responce.Success)
                        {
                            LoadBannerAsync(responce.Server, responce.Port);
                        }
                        else
                        {
                            IsFailed = true;
                        }
                    },
                     ex =>
                     {
                         IsFailed = true;
                         IsBannerLoaded = false;
                     },
                     () =>
                     {
                         IsLoading = false;
                     });


        }

        private void LoadBannerAsync(string server, int port)
        {
            _webService.GetMessage(serverMessage, server, port)
                                    .Subscribe(result =>
                                    {
                                        _message = result;
                                        SmartDispatcher.BeginInvoke(() =>
                                        {
                                            IsFailed = !HandleMessage(_message);
                                        });
                                    },
                                    exception =>
                                    {
                                        IsLoading = false;
                                        IsFailed = true;
                                    },
                                    () =>
                                    {
                                        IsLoading = false;
                                    });
        }


        private bool _succsess;

        [UsedImplicitly]
        public bool Succsess
        {
            get { return _succsess; }
            set
            {
                if (value.Equals(_succsess)) return;
                _succsess = value;
                RaisePropertyChanged("Succsess");
            }
        }

        private bool _isLoading;

        [UsedImplicitly]
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (value.Equals(_isLoading)) return;
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

        private bool _isFailed;

        [UsedImplicitly]
        public bool IsFailed
        {
            get { return _isFailed; }
            set
            {
                if (value.Equals(_isFailed)) return;
                _isFailed = value;
                RaisePropertyChanged("IsFailed");
            }
        }

        private bool _isBannerLoaded;

        [UsedImplicitly]
        public bool IsBannerLoaded
        {
            get { return _isBannerLoaded; }
            set
            {
                if (value.Equals(_isBannerLoaded)) return;
                _isBannerLoaded = value;
                RaisePropertyChanged("IsBannerLoaded");
            }
        }

        private BitmapImage _banner;

        [CanBeNull, UsedImplicitly]
        public BitmapImage Banner
        {
            get
            {
                return _banner;
            }
            set
            {
                if (Equals(value, _banner)) return;
                _banner = value;
                RaisePropertyChanged("Banner");
            }
        }

        private ICommand _retry;
        

        [NotNull, UsedImplicitly]
        public ICommand RetryCommand
        {
            get { return _retry ?? (_retry = new SimpleCommand(GetMessageAsync)); }
        }


        private string _textBanner;
        [CanBeNull, UsedImplicitly]
        public string TextBanner
        {
            get { return _textBanner; }
            set
            {
                if (value == _textBanner) return;
                _textBanner = value;
                RaisePropertyChanged("TextBanner");
            }
        }


        private bool HandleMessage(EmfMessage message)
        {
            if (message.Type != MessageType.GetBanner) return false;
            if (!message.Parameters.ContainsKey(BannerParameterKeys.Status)) return false;
            if (message.Parameters[BannerParameterKeys.Status] != "success") return false;
            if (!message.Parameters.ContainsKey(BannerParameterKeys.Type)) return false;

            var type = message.Parameters[BannerParameterKeys.Type];
            switch (type)
            {
                case "0":

                    TextBanner = "No banner found";
                    break;
                case "1":

                    if (!message.Parameters.ContainsKey(BannerParameterKeys.Text)) return false;
                    TextBanner = message.Parameters[BannerParameterKeys.Text];
                    break;
                case "2":

                    var baseString = message.Parameters[BannerParameterKeys.ContentBase64];
                    var contentType = message.Parameters[BannerParameterKeys.ContentType];
                    if (string.IsNullOrEmpty(contentType)) return false;

                    var decodedImage = GetImage(Convert.FromBase64String(baseString));
                    if (decodedImage == null) return false;

                    Banner = decodedImage;

                    break;
                default:
                    return false;
            }

            IsBannerLoaded = true;
            return true;
        }

        [CanBeNull]
        private BitmapImage GetImage(byte[] rawImageBytes)
        {
            BitmapImage imageSource;

            try
            {
                using (var stream = new MemoryStream(rawImageBytes))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var b = new BitmapImage();
                    b.SetSource(stream);
                    imageSource = b;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return imageSource;
        }
    }
}
