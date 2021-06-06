using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

using Prism.Mvvm;
using Prism.Commands;
using Grpc.Net.Client;
using ClassLibrary1;
using MagicOnion.Client;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowVM();
        }
    }

    public class MainWindowVM : BindableBase, IGamingHubReceiver
    {
        private string _url;
        public string URL
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        private string _username;
        public string UserName
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set { SetProperty(ref _messages, value); }
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand PostCommand { get; set; }

        public MainWindowVM()
        {
            _url = "https://localhost:5001";

            _messages = new ObservableCollection<Message>();

            BindingOperations.EnableCollectionSynchronization(this.Messages, new object());

            ConnectCommand = new DelegateCommand(Connect);
            PostCommand = new DelegateCommand(() => PostAsync(Message));
        }

        public async void Connect()
        {
            var channel = GrpcChannel.ForAddress(_url);

            await ConnectAsync(channel, "General", _username);
        }

        IGamingHub client;

        public async Task ConnectAsync(GrpcChannel grpcChannel, string roomName, string playerName)
        {
            this.client = await StreamingHubClient.ConnectAsync<IGamingHub, IGamingHubReceiver>(grpcChannel, this);

            var roomPlayers = await client.JoinAsync(roomName, playerName);

            foreach (var player in roomPlayers)
            {
                if(player.Name != playerName)
                    (this as IGamingHubReceiver).OnJoin(player, DateTime.UtcNow);
            }
        }

        public Task LeaveAsync()
        {
            return client.LeaveAsync();
        }

        public Task PostAsync(string message)
        {
            return client.PostAsync(message);
        }

        public Task DisposeAsync()
        {
            return client.DisposeAsync();
        }

        public Task WaitForDisconnect()
        {
            return client.WaitForDisconnect();
        }

        public void OnJoin(Player player, DateTime dateTime)
        {
            Messages.Add(new WpfApp1.Message()
            {
                PostedDate = dateTime.ToLocalTime(),
                Name = "管理者",
                Text = string.Format("{0} さんが入室しました(^ ^)v: ", player.Name),
            });
        }

        public void OnLeave(Player player, DateTime dateTime)
        {
            Messages.Add(new WpfApp1.Message()
            {
                PostedDate = dateTime.ToLocalTime(),
                Name = "管理者",
                Text = string.Format("{0} さんが退室しました(;_; ): ", player.Name),
            });
        }

        public void OnPost(Player player, DateTime dateTime, string message)
        {
            Messages.Add(new WpfApp1.Message()
            {
                PostedDate = dateTime.ToLocalTime(),
                Name = player.Name,
                Text = message,
            });
        }
    }

    public class Message
    {
        public DateTime PostedDate { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
    }
}
