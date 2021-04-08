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
        private Random _random = new Random();
        private int _turn = 0;
        private int _numFaces = 7;
        private int _maxScore = 40;

        public int Turn { get => _turn; }
        public int MaxScore { get => _maxScore; }

        public Model(List<Player> players, int numFaces, int maxScore) => (_players, _numFaces, _maxScore) = (players, numFaces > 0 ? numFaces : 1, maxScore > 0 ? maxScore : 1);

        public int Play()
        {
            int num = Throw();

            _players[_turn].Score += num;

            _turn++;
            if (_turn >= _players.Count)
                _turn = 0;

            return num;
        }

        public bool IsWin()
        {
            foreach (var player in _players) { if (player.Score >= _maxScore) { return true; } }
            return false;
        }

        public int[] WhoIsWin()
        {
            if (IsWin())
            {
                int max = _players[0].Score;
                List<int> num = new();

                for (int i = 0; i < _players.Count; i++) { 
                    if (_players[i].Score > max) 
                    { 
                        max = _players[i].Score; 
                        num.Clear(); 
                        num.Add(i); 
                    } 
                    else if (_players[i].Score == max) 
                    { 
                        num.Add(i); 
                    } 
                }

                return num.ToArray();
            } 
            else { return new int[] { -1 }; }
        }

        private int Throw() => _random.Next(1, _numFaces);
    }
}
