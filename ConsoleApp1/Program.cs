using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion.Client;
using ClassLibrary1;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5001");

            /*
            var client = MagicOnionClient.Create<IMyFirstService>(channel);

            var result = await client.SumAsync(123, 456);
            Console.WriteLine($"Result: {result}");
            */

            var client2 = new GamingHubClient();

            var playername = "";

            Console.Write("お名前を入力してください: ");
            playername = Console.ReadLine();

            var player = await client2.ConnectAsync(channel, "room1", playername);

            await client2.PostAsync("こんにちわ ^^");

            await client2.PostAsync("誰かいるかな?? (o_o;)");

            await Task.Run(() =>
            {
                while (true)
                {
                    var ki = Console.ReadKey();
                    if(ki.Key == ConsoleKey.Escape)
                    {
                        break;
                    }

                    Task.Delay(100).Wait();
                }
            });

            await client2.LeaveAsync();

            await client2.DisposeAsync();
        }
    }

    public class GamingHubClient : IGamingHubReceiver
    {
        Dictionary<string, Player> players = new Dictionary<string, Player>();

        IGamingHub client;

        public async Task<Player> ConnectAsync(GrpcChannel grpcChannel, string roomName, string playerName)
        {
            this.client = await StreamingHubClient.ConnectAsync<IGamingHub, IGamingHubReceiver>(grpcChannel, this);

            var roomPlayers = await client.JoinAsync(roomName, playerName);

            foreach(var player in roomPlayers)
            {
                (this as IGamingHubReceiver).OnJoin(player, DateTime.Now);
            }

            return players[playerName];
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
            Console.WriteLine("{0} さんが入室しました(^ ^)v: ", player.Name);

            players[player.Name] = player;
        }

        public void OnLeave(Player player, DateTime dateTime)
        {
            Console.WriteLine("{0} さんが退室しました(;_; ): ", player.Name);

            if (players.TryGetValue(player.Name, out var p))
            {
                players.Remove(player.Name);
            }
        }

        public void OnPost(Player player, DateTime dateTime, string message)
        {
            Console.WriteLine("{0} さんが投稿しました: {1}", player.Name, message);
        }
    }
}
