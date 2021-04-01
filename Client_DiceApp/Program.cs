using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using SetiLab;

namespace Client_DiceApp
{
    class Program
    {
        private const string Ip = "127.0.0.1";
        private const int Port = 13000;

        private static event Action<string, ConsoleColor> Print = (text, color) => { Console.ForegroundColor = color; Console.WriteLine(text); Console.ResetColor(); };

        static void Main(string[] args)
        {
            try
            {
                TcpClient client = new(Ip, Port);
                Print("Успешное подключение к серверу", ConsoleColor.Blue);

                NetworkManager manager = new(client.GetStream());
                string input = "";

                while (input != "exit")
                {
                    input = manager.GetMessage();
                    List<string> commands = input.Split('|')[0].Split(';').ToList();
                    input = input.Remove(0, input.Split('|')[0].Length + 1);

                    if (!commands.Contains("unvisible")) { if (commands.Contains("green")) { Print(input, ConsoleColor.Green); } else { Console.WriteLine(input); } }
                    if (commands.Contains("get-value")) { string text = Console.ReadLine(); manager.SendMessage(text == "" ? "ok" : text); }
                    else { manager.SendMessage("ok"); }
                    input = commands.Contains("exit") ? input = "exit" : input;
                }

                Print("Отключение от сервера", ConsoleColor.Blue);
                Thread.Sleep(500);
            }
            catch (Exception ex) { Print("[Error]: " + ex.Message, ConsoleColor.Red); }
        }
    }

    public static class Addition
    {
        public static List<string> ToList(this string[] mass)
        {
            List<string> results = new();

            foreach (var item in mass) { results.Add(item); }

            return results;
        }
    }
}
