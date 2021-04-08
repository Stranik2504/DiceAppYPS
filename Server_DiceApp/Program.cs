using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Server_DiceApp.Classes;
using Server_DiceApp.Game;
using SetiLab;

namespace Server_DiceApp
{
    class Program
    {
        private const string Ip = "127.0.0.1";
        private const int Port = 13000;

        private static List<Room> _rooms = new();
        private static Mutex _mutex = new();
        private static int startNumber = 0;

        static void Main(string[] args)
        {
            TcpListener listener = new(IPAddress.Parse(Ip), Port);
            listener.Start();

            Print("[Server started]", ConsoleColor.Blue);

            try
            {
                while (true)
                {
                    new Thread(AddConnection).Start(new NetworkManagerServer(listener.AcceptTcpClient()));
                }
            }
            catch (Exception ex) { Print("[Error]: " + ex.Message, ConsoleColor.Red); }
        }

        private static void AddConnection(object obj) => AddConnection(obj as NetworkManager);

        private static void AddConnection(NetworkManager manager)
        {
            string input = "";

            while (!input.StartsWith("/create") && !input.StartsWith("/connect"))
            {
                manager.SendMessage("|Для создания комнаты введите: \"/create {количество игроков} {количество граней у кубика} {до скольки очков идет игра}\".");
                manager.GetMessage();
                manager.SendMessage("get-value|Для присоединения введите: \"/connect {id комнаты}\"");
                input = manager.GetMessage();
            }

            if (input.StartsWith("/create"))
            {
                try
                {
                    int countPlayers = int.Parse(input.Split(' ')[1]);
                    int numFaces = int.Parse(input.Split(' ')[2]);
                    int maxScore = int.Parse(input.Split(' ')[3]);

                    countPlayers = countPlayers > 0 ? countPlayers : 1;

                    RoomsMove(() => { _rooms.Add(new Room(startNumber, countPlayers, numFaces, maxScore)); _rooms.GetRoom(startNumber).Players.Add(new Player(manager)); });

                    manager.SendMessage("|Ваша комната создана. Код подключения: " + startNumber.ToString() + ".\nОжидайте присоединения других игроков...");
                    manager.GetMessage();

                    new Thread(Game).Start(startNumber);
                    Print($"[Creating room with id: {startNumber}]", ConsoleColor.Green);

                    startNumber++;
                }
                catch { manager.SendMessage("exit|Ошибка"); manager.GetMessage(); }
            }
            else if (input.StartsWith("/connect"))
            {
                try
                {
                    int roomId = int.Parse(input.Split(' ')[1]);

                    RoomsMove(() => {
                        if (_rooms.Contains(roomId))
                        {
                            if (!_rooms.IsFullRoom(roomId))
                            {
                                _rooms.GetRoom(roomId).Players.Add(new Player(manager));
                                manager.SendMessage("|Вы присоединились.\nОжидайте присоединения других игроков...");
                                manager.GetMessage();

                                Print($"[Player connected to: {startNumber}]", ConsoleColor.Green);
                            }
                            else { manager.SendMessage("exit|Комната уже заполнена"); manager.GetMessage(); }
                        }
                        else { manager.SendMessage("exit|Комнаты не существует"); manager.GetMessage(); }
                    });
                }
                catch { manager.SendMessage("exit|Ошибка"); manager.GetMessage(); }
            }
            else { manager.SendMessage("exit|Ошибка"); manager.GetMessage(); }
        }

        private static void Game(object obj) => Game(obj as int?);

        private static void Game(int? roomId)
        {
            var room = _rooms.GetRoom(roomId.Value);

            while (!room.IsFullRoom()) { }

            int num = 1;
            room.SendAll("|Все подключились.");
            foreach (var item in room.Players) { item.Manager.SendMessage(string.Format("green|Начало игры. Вы игрок номер {0}", num)); item.Manager.GetMessage(); num++; }
            Print($"[Start game for room: {room.RoomId}]", ConsoleColor.Green);

            while (!room.Model.IsWin())
            {
                for (int i = 0; i < room.Players.Count; i++)
                {
                    room.SendAllWithoutOne(i, string.Format("green|Игрок номер {0} бросает кости", i + 1));
                    room.SendPlayer(i, "get-value|Вы бросаете. Для броска нажмите enter");

                    int got = room.Model.Play();
                    room.SendPlayer(i, "|Вы выбросили число: " + got);
                    room.SendAllWithoutOne(i, string.Format("green|Игрок номер {0} выбросил число: {1}", i + 1, got));
                    room.SendAll("green|---------------------------------------");
                }

                for (int i = 0; i < room.Players.Count; i++) { room.SendAll(string.Format("green|В сумме очков у игрока номер {0}: {1} (До победы {2})", i + 1, room.Players[i].Score, room.Model.MaxScore - room.Players[i].Score)); }
                room.SendAll("green|---------------------------------------");
            }

            Print($"[Room game end: {room.RoomId}]", ConsoleColor.Green);
            var winners = room.Model.WhoIsWin();

            if (winners.Length > 1)
            {
                for (int i = 0; i < room.Players.Count; i++)
                {
                    if (winners.Contains(i))
                    {
                        string output = "blue|Ты выиграл вместе с игроком(-ами): ";
                        for (int j = 0; j < winners.Length; j++) { if (winners[j] != i) { output += (winners[j] + 1).ToString(); if (j < winners.Length - 1) { output += ", "; } else { output += "."; } } }
                        room.SendPlayer(i, output);
                    }
                    else
                    {
                        string output = "blue|Выигрывшие игроки с номерами: ";
                        for (int j = 0; j < winners.Length; j++) { output += (winners[j] + 1).ToString(); if (j < winners.Length - 1) { output += ", "; } else { output += "."; } }
                        room.SendPlayer(i, output);
                    }
                }
            }
            else 
            {
                room.SendAllWithoutOne(winners[0], string.Format("blue|Игрок номер {0} выиграл", winners[0] + 1));
                room.SendPlayer(winners[0], "blue|Ты выиграл");
            }

            room.SendAll("exit;green|Закрытие команты");
            Print($"[Room closed: {room.RoomId}]", ConsoleColor.Green);
        }

        private static void Print(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void RoomsMove(Action move) 
        { 
            _mutex.WaitOne(); 
            move?.Invoke(); 
            _mutex.ReleaseMutex(); 
        }
    }

    public static class Addition
    {
        public static bool Contains(this List<Room> rooms, int roomId)
        {
            foreach (var item in rooms) { if (item.RoomId == roomId) return true; }
            return false;
        }

        public static bool Contains(this int[] winners, int num)
        {
            foreach (var item in winners) { if (item == num) return true; }
            return false;
        }

        public static bool IsFullRoom(this List<Room> rooms, int roomId)
        {
            foreach (var item in rooms) { if (item.RoomId == roomId) { if (item.IsFullRoom()) { return true; } else { return false; } } }
            return true;
        }

        public static Room GetRoom(this List<Room> rooms, int roomId)
        {
            foreach (var item in rooms) { if (item.RoomId == roomId) return item; }
            return default;
        }
    }
}
