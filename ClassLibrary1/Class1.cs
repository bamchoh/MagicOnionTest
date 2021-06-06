using System;
using System.Threading.Tasks;
using MessagePack;
using MagicOnion;

namespace ClassLibrary1
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<int> SumAsync(int x, int y);
    }

    public interface IGamingHubReceiver
    {
        void OnJoin(Player player, DateTime dateTime);
        void OnLeave(Player player, DateTime dateTime);
        void OnPost(Player player, DateTime dateTime, string message);
    }

    public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
    {
        Task<Player[]> JoinAsync(string roomName, string userName);
        Task LeaveAsync();
        Task PostAsync(string message);
    }

    [MessagePackObject]
    public class Player
    {
        [Key(0)]
        public string Name { get; set; }
    }
}
