using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_DiceApp.Game
{
    public interface IModel
    {
        int Turn { get; }
        int MaxScore { get; }

        int Play();
        bool IsWin();
        int[] WhoIsWin();
    }
}
