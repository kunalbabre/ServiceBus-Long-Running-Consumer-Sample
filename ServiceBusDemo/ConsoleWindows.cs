using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceBusDemo
{
    class ConsoleWindows
    {
        static List<string> area1 = new List<string>();
        static List<string> area2 = new List<string>();
        static int areaHeights = 0;

        public static void Draw()
        {
            // Number of rows for each area
            areaHeights = (Console.WindowHeight - 2) / 2;

            drawScreen();
        }

        public static void WriteLine(string msg, int area)
        {
            if (area == 1)
            {
                AddLineToBuffer(ref area1, msg);
            }
            else
            {
                AddLineToBuffer(ref area2, msg);
            }
            drawScreen();

        }

        private static void AddLineToBuffer(ref List<string> areaBuffer, string line)
        {
            areaBuffer.Insert(0, line);

            if (areaBuffer.Count == areaHeights)
            {
                areaBuffer.RemoveAt(areaHeights - 1);
            }
        }

        public static void Clear(int area)
        {
            if(area == 1)
            {
                area1.Clear();
            }
            else
            {
                area2.Clear();
            }
        }


        private static void drawScreen()
        {
            Console.Clear();

            // Draw the area divider
            for (int i = 0; i < Console.BufferWidth; i++)
            {
                Console.SetCursorPosition(i, areaHeights);
                Console.Write('-');
            }

            int currentLine = areaHeights - 1;

            for (int i = 0; i < area1.Count; i++)
            {
                Console.SetCursorPosition(0, currentLine - (i + 1));
                Console.WriteLine(area1[i]);

            }

            currentLine = (areaHeights * 2);
            for (int i = 0; i < area2.Count; i++)
            {
                Console.SetCursorPosition(0, currentLine - (i + 1));
                Console.WriteLine(area2[i]);
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Write("> ");

        }

  
    }
}
