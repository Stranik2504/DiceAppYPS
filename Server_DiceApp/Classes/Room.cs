using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetiLab;
using Server_DiceApp.Game;

namespace Server_DiceApp.Classes
{
    public class Room
    {
        public int RoomId { get; set; }
        public int MaxPlayers { get; set; }
        public List<Player> Players { get; set; } = new();
        public IModel Model { get; set; } = new Model();

        public Room(int roomId, int maxPlayers) => (RoomId, MaxPlayers) = (roomId, maxPlayers);

        public bool IsFullRoom() => Players.Count < MaxPlayers;
    }
}
