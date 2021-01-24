using System;
using System.Threading;

namespace ConsoleMenuUI
{
    public enum StyleType
    {
        Header,
        Item,
        ActiveItem,
        InfoText
    }

    public class ConsoleMenuUi
    {
        public static void StyleText(string text, StyleType type, int padding)
        {
            switch (type)
            {
                case StyleType.Header:
                    StyleMenuHeader(text, padding);
                    break;
                case StyleType.Item:
                    StyleMenuItems(text, padding);
                    break;
                case StyleType.ActiveItem:
                    StyleActiveItem(text, padding);
                    break;
                case StyleType.InfoText:
                    StyleInfoText(text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void StyleInfoText(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void StyleMenuItems(string text, int padding)
        {
            switch (text.Substring(0, 1))
            {
                case "X":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "R":
                case "M":
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }

            Console.WriteLine(text.PadLeft(text.Length + padding));
            Console.ResetColor();
        }

        private static void StyleActiveItem(string text, int padding)
        {
            
            Console.Write(" ".PadLeft(padding));
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = text.Substring(0, 1) == "X" ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void StyleMenuHeader(string text, int padding)
        {
            const string emptyLine = "";
            var pad = text.Length;

            Console.BackgroundColor = ConsoleColor.White;

            Console.WriteLine(emptyLine.PadLeft(pad + padding).PadRight(pad + (2 * padding)));
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text.PadLeft(pad + padding).PadRight(pad + (2 * padding)));

            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(emptyLine.PadLeft(pad + padding).PadRight(pad + (2 * padding)));

            Console.ResetColor();
            //Console.WriteLine();
        }
    }
}