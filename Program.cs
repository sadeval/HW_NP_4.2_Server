using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RockPaperScissorsLizardSpockServer
{
    class Program
    {
        private static TcpListener server;

        static async Task Main()
        {
            server = new TcpListener(IPAddress.Any, 5500);
            server.Start();
            Console.WriteLine("Сервер запущен и ожидает подключения клиентов...");

            while (true)
            {
                TcpClient client1 = await server.AcceptTcpClientAsync();
                Console.WriteLine("Клиент 1 подключен.");
                TcpClient client2 = await server.AcceptTcpClientAsync();
                Console.WriteLine("Клиент 2 подключен.");

                _ = HandleGameSession(client1, client2);
            }
        }

        private static async Task HandleGameSession(TcpClient client1, TcpClient client2)
        {
            NetworkStream stream1 = client1.GetStream();
            NetworkStream stream2 = client2.GetStream();

            byte[] buffer = new byte[1024];
            int rounds = 5;
            int score1 = 0, score2 = 0;

            for (int round = 1; round <= rounds; round++)
            {
                string move1 = await ReceiveMove(stream1);
                string move2 = await ReceiveMove(stream2);

                string result = DetermineWinner(move1, move2);
                Console.WriteLine($"Раунд {round}: Клиент 1: {move1} | Клиент 2: {move2} -> {result}");

                if (result.Contains("Клиент 1 выиграл")) score1++;
                else if (result.Contains("Клиент 2 выиграл")) score2++;

                await SendResult(stream1, result);
                await SendResult(stream2, result);
            }

            string finalResult = $"Итоговый счет: Клиент 1: {score1}, Клиент 2: {score2}";
            await SendResult(stream1, finalResult);
            await SendResult(stream2, finalResult);

            client1.Close();
            client2.Close();
            Console.WriteLine("Игра завершена и клиенты отключены.");
        }

        private static async Task<string> ReceiveMove(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        }

        private static async Task SendResult(NetworkStream stream, string result)
        {
            byte[] data = Encoding.UTF8.GetBytes(result);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private static string DetermineWinner(string move1, string move2)
        {
            if (move1 == move2) return "Ничья!";
            return (move1, move2) switch
            {
                ("Камень", "Ножницы") => "Клиент 1 выиграл!",
                ("Камень", "Ящерица") => "Клиент 1 выиграл!",
                ("Ножницы", "Бумага") => "Клиент 1 выиграл!",
                ("Ножницы", "Ящерица") => "Клиент 1 выиграл!",
                ("Бумага", "Камень") => "Клиент 1 выиграл!",
                ("Бумага", "Спок") => "Клиент 1 выиграл!",
                ("Спок", "Камень") => "Клиент 1 выиграл!",
                ("Спок", "Ножницы") => "Клиент 1 выиграл!",
                ("Ящерица", "Спок") => "Клиент 1 выиграл!",
                ("Ящерица", "Бумага") => "Клиент 1 выиграл!",
                _ => "Клиент 2 выиграл!"
            };
        }
    }
}
