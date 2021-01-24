using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConsoleMenuUI;

namespace MenuSystem
{
    public enum MenuLevel
    {
        Level0,
        Level1,
        Level2Plus,
        LevelBlank
    }

    public class Menu
    {
        private List<MenuItem> MenuItems = new List<MenuItem>();

        private readonly MenuLevel _menuLevel;

        private string? _menuHeader;
        private readonly string[] _reservedActions = new string[3];

        private bool _hasDefaultMenuItems;

        private readonly bool _initDefaultReturnToMAin;
        private readonly bool _initDefaultReturnToPrevious;
        public Menu(MenuLevel level, bool initDefaultReturnToMain = true, bool initDefaultReturnToPrevious = true)
        {
            _menuLevel = level;
            _initDefaultReturnToMAin = initDefaultReturnToMain;
            _initDefaultReturnToPrevious = initDefaultReturnToPrevious;
        }

        public string RunMenu()
        {
           
            if (!_hasDefaultMenuItems && _menuLevel != MenuLevel.LevelBlank)
            {
                AddAllDefaultMenuItems();
            }

            var currentItemIndex = 0;
            string userChoice = "";
            string keyEntry = "";
            
            do
            {
                Console.Clear();

                if (_menuHeader != null)
                {
                    ConsoleMenuUi.StyleText(_menuHeader, StyleType.Header, 10);
                }
                Console.WriteLine("");
                
                for (var i = 0; i < MenuItems.Count; i++)
                {
                    if (currentItemIndex == i)
                    {
                        ConsoleMenuUi.StyleText(MenuItems[i].ToString(), StyleType.ActiveItem, 7);

                        if (keyEntry.Length == 0)
                        {
                            userChoice = MenuItems[currentItemIndex].UserChoice.ToLower();
                        }
                    }
                    else
                    {
                        ConsoleMenuUi.StyleText(MenuItems[i].ToString(), StyleType.Item, 7);
                    }
                }
                
                Console.Write("Current choice: " + userChoice);

                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.Tab:
                    {
                        keyEntry = "";
                        currentItemIndex++;
                        if (currentItemIndex > MenuItems.Count - 1)
                        {
                            currentItemIndex = 0;
                        }

                        break;
                    }
                    case ConsoleKey.UpArrow:
                    {
                        keyEntry = "";
                        currentItemIndex--;
                        if (currentItemIndex < 0)
                        {
                            currentItemIndex = MenuItems.Count - 1;
                        }

                        break;
                    }
                    case ConsoleKey.Backspace:
                    {
                        keyEntry = userChoice.Remove(userChoice.Length - 1);
                        userChoice = keyEntry;
                        break;
                    }
                    case ConsoleKey.Enter:
                    {
                        keyEntry = "";
                        break;
                    }
                    default:
                    {
                        keyEntry += key.KeyChar.ToString().ToLower().Trim();
                        userChoice = keyEntry;
                        break;
                    }
                }
                
                if (key.Key != ConsoleKey.Enter) continue;

                if (!_reservedActions.Contains(userChoice))
                {
                    var userMenuItem = MenuItems.FirstOrDefault(t =>
                        string.Equals(t.UserChoice, userChoice, StringComparison.CurrentCultureIgnoreCase));

                    if (userMenuItem != null)
                    {
                        userChoice = userMenuItem.MethodToExecute();
                    }
                    else
                    {
                        Console.WriteLine();
                        ConsoleMenuUi.StyleText("we don't have this option :(", StyleType.InfoText, 0);
                        Thread.Sleep(2000);
                        userChoice = "";
                        keyEntry = "";
                    }
                }

                if (userChoice == "x")
                {
                    if (_menuLevel == MenuLevel.Level0)
                    {
                        Console.WriteLine("");
                        ConsoleMenuUi.StyleText("Closing down...", StyleType.InfoText, 0);
                    }

                    break;
                }

                if (_menuLevel != MenuLevel.Level0 && userChoice == "m")
                {
                    break;
                }

                if (_menuLevel == MenuLevel.Level2Plus && userChoice == "r")
                {
                    break;
                }
            } while (true);

            return userChoice;
        }
        

        private void AddAllDefaultMenuItems()
        {
            if (_menuLevel == MenuLevel.Level2Plus && _initDefaultReturnToPrevious)
            {
                MenuItems.Add(new MenuItem("Return to previous menu", "R", () => "r"));
                _reservedActions[2] = "r";
            }

            if (_menuLevel != MenuLevel.Level0 &&  _initDefaultReturnToMAin)
            {
                MenuItems.Add(new MenuItem("Return to Main menu", "M", () => "m"));
                _reservedActions[1] = "m";
            }

            MenuItems.Add(new MenuItem("Exit", "X", () => "x"));
            _reservedActions[0] = "x";
            _hasDefaultMenuItems = true;
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            if (menuItem.UserChoice.Trim().Length == 0 || menuItem.UserChoice.Trim().Length > 5)
            {
                throw new ArgumentException(
                    $"User choice can be 1 to 4 symbols, You wrote {menuItem.UserChoice.Length} symbol('s).");
            }

            string systemChoice = $"Choice {menuItem.UserChoice} is used by the system";
            const string chooseOther = "please choose something else.";

            if (menuItem.UserChoice.ToLower() == "x")
            {
                throw new ArgumentException($"{systemChoice}, {chooseOther}");
            }

            if ((_menuLevel == MenuLevel.Level1 || _menuLevel == MenuLevel.Level2Plus) &&
                menuItem.UserChoice.ToLower() == "m")
            {
                throw new ArgumentException($"{systemChoice} in this level for navigating to main menu, {chooseOther}");
            }

            if (_menuLevel == MenuLevel.Level2Plus && menuItem.UserChoice.ToLower() == "r")
            {
                throw new ArgumentException(
                    $"{systemChoice} in this level for returning to previous menu, {chooseOther}");
            }

            if (menuItem.Label == "")
            {
                throw new ArgumentException("Label cannot be empty!");
            }

            if (menuItem.Label.Length > 90)
            {
                throw new ArgumentException("Label is too long, please write shorter!");
            }

            foreach (var item in MenuItems)
            {
                if (item.UserChoice == menuItem.UserChoice)
                {
                    throw new ArgumentException("This use choice is already in menu!");
                }
            }

            MenuItems.Add(menuItem);
        }

        public void AddMenuHeader(string header)
        {
            _menuHeader = header;
        }
    }
}