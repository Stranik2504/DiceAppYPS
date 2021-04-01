using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetiLab;

namespace Server_DiceApp.Classes
{
    public class Player
    {
        public NetworkManager Manager { get; set; }
        public int Score { get; set; } = 0;

        public Player(NetworkManager manager) => Manager = manager;
    }
}
