using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using JetBrains.Annotations;
using eBuddy.Assignment.Networking.Parsers;

namespace eBuddy.Assignment.Networking
{
    public sealed class WebService : IWebService
    {
        private const string BALANCER_HOST = "s-connector.ebuddy.com";
        private const string MESSAGE_END = "\n";
        private const int BALANCER_PORT = 110;
        private const int BUFFER_SIZE = 2048;


        private readonly TimeSpan _timeoutTimeSpan = TimeSpan.FromSeconds(40);
        [NotNull] private readonly Parser _parser = new Parser();
        [NotNull] private readonly MessageParser _messageParser = new MessageParser();

        private string _accumulator;

        public IObservable<BalancerResponce> GetBalancerResponce(string balancerMessage)
        {
            return Observable.Create<BalancerResponce>(
               observer =>
               Scheduler.Default.Schedule(() =>
               {
                   var balancerSocket = GetNewSocket();
                   var listener = GetListener(balancerSocket, BALANCER_HOST, BALANCER_PORT);

                   Observable.FromEventPattern<SocketAsyncEventArgs>(listener, "Completed")
                       .Timeout(_timeoutTimeSpan)
                       .Subscribe(
                           sListener =>
                           {
                               var eventArgs = sListener.EventArgs;
                               ProcessConnectionToBalancer(eventArgs, observer, balancerSocket, balancerMessage);
                           },
                           ex =>
                           {
                               CloseConnetion(balancerSocket);
                               observer.OnError(ex);
                               observer.OnCompleted();
                           },
                           () =>
                           {
                               CloseConnetion(balancerSocket);
                               observer.OnCompleted();
                           });
                   balancerSocket.ConnectAsync(listener);
               }));
        }

        public IObservable<EmfMessage> GetMessage(string serverMessage, string server, int port)
        {
            return Observable.Create<EmfMessage>(observer =>
                Scheduler.Default.Schedule(
                    () =>
                    {
                        Debug.WriteLine("Connecting to server");

                        var serverSocket = GetNewSocket();
                        var listener = GetListener(serverSocket, server, port);

                        Observable.FromEventPattern<SocketAsyncEventArgs>(listener, "Completed")
                                  .Timeout(_timeoutTimeSpan)
                                  .Subscribe(
                                      sListener =>
                                      {
                                          var eventArgs = sListener.EventArgs;
                                          switch (eventArgs.LastOperation)
                                          {
                                              case SocketAsyncOperation.Connect:
                                                  EnsureSuccsess(eventArgs, observer, serverSocket);
                                                  Debug.WriteLine("connected to server");
                                                  SendMessage(eventArgs, serverSocket, serverMessage);
                                                  break;
                                              case SocketAsyncOperation.Send:
                                                  EnsureSuccsess(eventArgs, observer, serverSocket);
                                                  Debug.WriteLine("message sent to server");
                                                  var responseBuffer = new byte[BUFFER_SIZE];
                                                  eventArgs.SetBuffer(responseBuffer, 0, BUFFER_SIZE);
                                                  _accumulator = string.Empty;
                                                  serverSocket.ReceiveAsync(eventArgs);
                                                  break;
                                              case SocketAsyncOperation.Receive:

                                                  EnsureSuccsess(eventArgs, observer, serverSocket);
                                                  Debug.WriteLine("got responce from server");
                                                  var rawMessage = ReadServerMessage(eventArgs);
                                                  _accumulator += rawMessage;

                                                  if (_accumulator.EndsWith(MESSAGE_END))
                                                  {
                                                      if (_accumulator.StartsWith("#"))
                                                      {
                                                          var parsed = _parser.ParceBannerInfo(_accumulator);
                                                          Debug.WriteLine("Parsed Responce: " + parsed);
                                                          _accumulator = string.Empty;
                                                          serverSocket.ReceiveAsync(eventArgs);
                                                      }
                                                      else
                                                      {
                                                          try
                                                          {
                                                              var message =
                                                                  _messageParser.ParseMessage(_accumulator);
                                                              observer.OnNext(message);
                                                              observer.OnCompleted();
                                                          }
                                                          catch (ParseException exception)
                                                          {
                                                              observer.OnError(exception);
                                                              observer.OnCompleted();
                                                          }
                                                      }
                                                  }
                                                  else
                                                  {
                                                      serverSocket.ReceiveAsync(eventArgs);
                                                  }

                                                  
                                                  break;
                                          }
                                      },
                                      ex =>
                                      {
                                          CloseConnetion(serverSocket);
                                          observer.OnError(ex);
                                          observer.OnCompleted();
                                      },
                                      () =>
                                      {
                                          CloseConnetion(serverSocket);
                                          observer.OnCompleted();
                                      });
                        serverSocket.ConnectAsync(listener);
                    })
                    );
        }

        private static void CloseConnetion(Socket socket)
        {
            if (!socket.Connected) return;
            socket.Shutdown(SocketShutdown.Send);
            socket.Close();
        }

        private void ProcessConnectionToBalancer(SocketAsyncEventArgs eventArgs, IObserver<BalancerResponce> observer,
                                                                 Socket socket, string balancerMessage)
        {
            switch (eventArgs.LastOperation)
            {
                case SocketAsyncOperation.Connect:

                    EnsureSuccsess(eventArgs, observer, socket);                    
                    SendMessage(eventArgs, socket, balancerMessage);
                    break;
                case SocketAsyncOperation.Send:

                    EnsureSuccsess(eventArgs, observer, socket);
                    var responseBuffer = new byte[BUFFER_SIZE];
                    eventArgs.SetBuffer(responseBuffer, 0, BUFFER_SIZE);
                    _accumulator = string.Empty;
                    socket.ReceiveAsync(eventArgs);
                    break;
                case SocketAsyncOperation.Receive:

                    EnsureSuccsess(eventArgs, observer, socket);
                    var responce = ReadLoaderResponce(eventArgs);
                    _accumulator += responce;
                    if (_accumulator.EndsWith(MESSAGE_END))
                    {
                        var parced = _parser.ParceLoaderResponce(_accumulator);
                        _accumulator = string.Empty;
                        Debug.WriteLine(responce);
                        observer.OnNext(parced);
                        observer.OnCompleted();    
                    }
                    else
                    {
                        socket.ReceiveAsync(eventArgs);
                    }
                    break;
            }
        }


        


        private string ReadServerMessage(SocketAsyncEventArgs eventArgs)
        {
            var responce = Encoding.UTF8.GetString(eventArgs.Buffer, 0, eventArgs.BytesTransferred);
            Debug.WriteLine("message received from server: " + responce);
            return responce;
        }

        private string ReadLoaderResponce(SocketAsyncEventArgs eventArgs)
        {
            var responce = Encoding.UTF8.GetString(eventArgs.Buffer, 0, eventArgs.BytesTransferred);
            Debug.WriteLine("message received from balancer: " + responce);
            return responce;
        }

        private static void EnsureSuccsess<T>(SocketAsyncEventArgs eventArgs, IObserver<T> observer, Socket socket)
        {
            if (eventArgs.SocketError == SocketError.Success) return;

            observer.OnError(new ConnectionException("Oops, something went wrong."));
            observer.OnCompleted();
        }

        private static void SendMessage(SocketAsyncEventArgs eventArgs, Socket socket, string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            eventArgs.SetBuffer(buffer, 0, buffer.Length);
            socket.SendAsync(eventArgs);
        }

        private static SocketAsyncEventArgs GetListener(Socket socket, string server, int port)
        {
            var eventArgs = new SocketAsyncEventArgs();
            var loaderEndPoint = new DnsEndPoint(server, port);
            eventArgs.RemoteEndPoint = loaderEndPoint;
            eventArgs.UserToken = socket;
            return eventArgs;
        }

        private static Socket GetNewSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

    }
}
