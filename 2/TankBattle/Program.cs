using System;
using System.Threading;
using System.Collections.Concurrent;

class TankGame
{
    private const int Width = 60;
    private const int Height = 20;
    private static int tankPos = Width / 2;
    private static int score = 0;
    private static bool running = true;
    private static Random random = new Random();
    private static int targetPos = -1;
    private static int bulletPos = -1;
    private static int bulletY = -1;
    private static readonly BlockingCollection<ConsoleKey> inputQueue = new BlockingCollection<ConsoleKey>();

    static void Main()
    {
      
        Thread? inputThread = null;

        try
        {
            if (!Console.IsInputRedirected && !Console.IsOutputRedirected)
            {
                Console.Title = "Танковая Битва";
                Console.CursorVisible = false;
                Console.OutputEncoding = System.Text.Encoding.UTF8;
            }

            if (!Console.IsInputRedirected)
            {
                inputThread = new Thread(ReadInput);
                inputThread.IsBackground = true;
                inputThread.Start();
            }

            GameLoop();
        }
        finally
        {
            CleanUp(inputThread);
        }
    }

    private static void GameLoop()
    {
        while (running)
        {
            ProcessInput();
            UpdateGame();
            RenderGame();
            Thread.Sleep(100);
        }
    }

    private static void ReadInput()
    {
        try
        {
            while (running)
            {
                var key = Console.ReadKey(true).Key;
                inputQueue.Add(key);
            }
        }
        catch (InvalidOperationException)
        {
        
        }
        finally
        {
            inputQueue.CompleteAdding();
        }
    }

    private static void ProcessInput()
    {
        while (inputQueue.TryTake(out var key))
        {
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    tankPos = Math.Max(1, tankPos - 1);
                    break;
                case ConsoleKey.RightArrow:
                    tankPos = Math.Min(Width - 2, tankPos + 1);
                    break;
                case ConsoleKey.Spacebar when bulletY == -1:
                    bulletPos = tankPos;
                    bulletY = Height - 2;
                    break;
                case ConsoleKey.Escape:
                    running = false;
                    break;
            }
        }
    }

    private static void UpdateGame()
    {
        if (targetPos == -1 && random.Next(0, 20) == 0)
        {
            targetPos = random.Next(1, Width - 1);
        }

        if (bulletY != -1)
        {
            bulletY--;
            
            if (bulletY == 1 && bulletPos == targetPos)
            {
                score++;
                targetPos = -1;
                bulletY = -1;
            }
            else if (bulletY < 1)
            {
                bulletY = -1;
            }
        }
    }

    private static void RenderGame()
    {
        if (Console.IsOutputRedirected) return;

        try
        {
            Console.SetCursorPosition(0, 0);
            Console.Write($"Счёт: {score}".PadRight(Width - 1) + "\n");
            Console.Write(new string('═', Width) + "\n");
            
            for (int y = 1; y < Height - 1; y++)
            {
                Console.Write("║");
                for (int x = 1; x < Width - 1; x++)
                {
                    char c = ' ';
                    if (y == 1 && x == targetPos) c = '☼';
                    else if (y == bulletY && x == bulletPos) c = '•';
                    Console.Write(c);
                }
                Console.Write("║\n");
            }
            
            Console.Write(new string('═', Width) + "\n");
            Console.Write(new string(' ', tankPos) + "▲\n");
        }
        catch (IOException)
        {
        
        }
    }

    private static void CleanUp(Thread? inputThread)
    {
        running = false;
        inputQueue.CompleteAdding();

        if (!Console.IsOutputRedirected)
        {
            try
            {
                Console.Clear();
                Console.WriteLine($"Игра окончена. Счёт: {score}");
                Console.CursorVisible = true;
            }
            catch (IOException)
            {
          
            }
        }

        inputThread?.Join();
    }
}