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
        private static event Action<string, ConsoleColor> Print = (text, color) => { Console.ForegroundColor = color; Console.WriteLine(text); Console.ResetColor(); };
        private static event Action<Action> RoomsMove = (move) => { _mutex.WaitOne(); move?.Invoke(); _mutex.ReleaseMutex(); };

        static void Main(string[] args)
        {
            TcpListener listener = new(IPAddress.Parse(Ip), Port);
            listener.Start();

            Print("[Server started]", ConsoleColor.Blue);

            try
            {
                while (true)
                {
                    new Thread(AddConnection).Start(new NetworkManager(listener.AcceptTcpClient().GetStream()));
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
                manager.SendMessage("get-value|Для создания комнаты введите: \"/create {количество игроков}\". Для присоединения введите: \"/connect {id комнаты}\"");
                input = manager.GetMessage();
            }

            if (input.StartsWith("/create"))
            {
                try
                {
                    int countPlayers = int.Parse(input.Split(' ')[1]);
                    RoomsMove(() => { _rooms.Add(new Room(startNumber, countPlayers)); _rooms.GetRoom(startNumber).Players.Add(new Player(manager)); });

                    manager.SendMessage("|Ваша комната создана. Код подключения: " + startNumber.ToString() + ".\nОжидайте присоединения других игроков...");
                    manager.GetMessage();

                    new Thread(Game).Start(startNumber.ToString());
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

        private static void Game(object obj) => Game(obj as string);

        private static void Game(string roomId)
        {
            var room = _rooms.GetRoom(int.Parse(roomId));

            while (room.IsFullRoom()) { }

            int num = 1;
            foreach (var item in room.Players) { item.Manager.SendMessage("|Все подключились."); item.Manager.GetMessage(); item.Manager.SendMessage(string.Format("green|Начало игры. Вы игрок номер {0}", num)); item.Manager.GetMessage(); num++; }
            Print($"[Start game for room: {room.RoomId}]", ConsoleColor.Green);
        }
    }

    public static class Addition
    {
        public static bool Contains(this List<Room> rooms, int roomId)
        {
            foreach (var item in rooms) { if (item.RoomId == roomId) return true; }
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
