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
        private int _roomId;
        private int _maxPlayers;
        private List<Player> _players = new();
        private IModel _model;

        public int RoomId { get => _roomId; }
        public int MaxPlayers { get => _maxPlayers; }
        public List<Player> Players { get => _players; }
        public IModel Model { get => _model; }

        public Room(int roomId, int maxPlayers, int numFaces, int maxScore) => (_roomId, _maxPlayers, _model) = (roomId, maxPlayers, new Model(Players, numFaces, maxScore));

        public void SendAll(string message) { foreach (var player in Players) { player.Manager.SendMessage(message); player.Manager.GetMessage(); } }

        public void SendAllWithoutOne(int playerId, string message) { for (int i = 0; i < Players.Count; i++) { if (i != playerId) { Players[i].Manager.SendMessage(message); Players[i].Manager.GetMessage(); } } }

        public void SendPlayer(int playerId, string message) { for (int i = 0; i < Players.Count; i++) { if (i == playerId) { Players[i].Manager.SendMessage(message); Players[i].Manager.GetMessage(); } } }

        public bool IsFullRoom() => Players.Count >= MaxPlayers;
    }
}
