using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using ClassLibrary1;

namespace GrpcService2.Services
{
    public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
    {
        public async UnaryResult<int> SumAsync(int x, int y)
        {
            Console.WriteLine($"Received:{x}, {y}");
            return x + y;
        }
    }

    public class GamingHub : StreamingHubBase<IGamingHub, IGamingHubReceiver>, IGamingHub
    {
        IGroup room;
        Player self;
        IInMemoryStorage<Player> storage;

        public async Task<Player[]> JoinAsync(string roomName, string userName)
        {
            self = new Player() { Name = userName };

            (room, storage) = await Group.AddAsync(roomName, self);

            Broadcast(room).OnJoin(self, DateTime.UtcNow);

            return storage.AllValues.ToArray();
        }

        public async Task LeaveAsync()
        {
            await room.RemoveAsync(this.Context);
            Broadcast(room).OnLeave(self, DateTime.UtcNow);
        }

        public async Task PostAsync(string message)
        {
            Broadcast(room).OnPost(self, DateTime.UtcNow, message);
        }

        protected override ValueTask OnDisconnected()
        {
            return CompletedTask;
        }
    }
}
