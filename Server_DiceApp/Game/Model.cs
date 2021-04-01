using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Server_DiceApp.Classes;

namespace Server_DiceApp.Game
{
    public class Model : IModel
    {
        private List<Player> _players;

        public Model(List<Player> players) => _players = players;
    }
}
