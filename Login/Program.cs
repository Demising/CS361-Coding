using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;


namespace login {
    class Program {
        public static void printPage(string pageName) {
            Console.WriteLine("====================================================================");
            Console.WriteLine($"                            {pageName}                            ");
            Console.WriteLine("--------------------------------------------------------------------");
            Console.WriteLine("         Library         Add a Game        Help         Logout         ");
            Console.WriteLine("--------------------------------------------------------------------");
        }

        public static void UserLibraryPrint(string[][] librarySeparate) {
            const int FieldWidth = 50;
            for (int i = 0; i < librarySeparate.Length; i++) {
                Console.Write($"| {librarySeparate[i][0],-FieldWidth}");

                Console.WriteLine(" |             |");

                Console.Write($"| Developer: {librarySeparate[i][4],-39} |");
                Console.WriteLine("    Remove   |");

                Console.Write($"| Publisher: {librarySeparate[i][5],-39} |");
                Console.WriteLine("     Game    |");

                string releaseDate = $"{librarySeparate[i][1]} {librarySeparate[i][2]}, {librarySeparate[i][3]}";
                Console.WriteLine($"| Release Date: {releaseDate,-36} |             |");

                if (i != librarySeparate.Length - 1) {
                    Console.WriteLine("--------------------------------------------------------------------");
                }
            }
            Console.WriteLine("====================================================================");
        }

        public static void UserLibrary(string libraryPath) {
            printPage("Library");
            

            string[] library = File.ReadAllLines(libraryPath);
            string[][] librarySeparate = new string[library.Length][];
            int game = 0;

            foreach (string section in library) {
                string[] rowData = section.Split(",");

                librarySeparate[game] = rowData;

                game++;
            }

            UserLibraryPrint(librarySeparate);
        }

        public static void RemoveGameFinal (string gameChoice, bool gameFound, List<string> newLibrary, string libraryPath) {
            string playerChoice = "";
            if (gameFound) {
                Console.Write($"{gameChoice} will be deleted. Are you sure you want to continue?: ");
                playerChoice = Console.ReadLine();
                if ((playerChoice == "Yes") || (playerChoice == "yes") || (playerChoice == "Y") || (playerChoice == "y")) {
                    File.WriteAllLines(libraryPath, newLibrary);
                    Console.WriteLine($"Successfully removed '{gameChoice}'.");
                } else {
                    Console.WriteLine("Game wron't be deleted.");
                }
            } else {
                Console.WriteLine($"Error: Game '{gameChoice}' was not found in the library.");
            }
        }

        public static void RemoveGame (string libraryPath) {
            string gameChoice = "";
            Console.Write("Which game do you want to remove? (Enter the exact name. This also removes any data.): ");
            gameChoice = Console.ReadLine();

            string[] library = File.ReadAllLines(libraryPath);

            List<string> newLibrary = new List<string>();
            bool gameFound = false;

            foreach (string gameLine in library) {
                string gameName = gameLine.Split(',')[0];

                if (gameName.Equals(gameChoice, StringComparison.OrdinalIgnoreCase)) {
                    gameFound = true;
                } else {
                    newLibrary.Add(gameLine);
                }
            }

            RemoveGameFinal(gameChoice, gameFound, newLibrary, libraryPath);

            UserLibrary(libraryPath);
        }

        public static void HelpPage(string helpPath) {
            printPage("Help Page");
            string[] help = File.ReadAllLines(helpPath);
            foreach (string line in help) {
                Console.WriteLine($"{line}");
            }

            Console.WriteLine("====================================================================");
        }

        public static void AddAGameListHelper(string libraryPath, string newGame) {
            List<string> games = new List<string>(File.ReadAllLines(libraryPath));

            games.Add(newGame);

            games.Sort();

            File.WriteAllLines(libraryPath, games);
        }

        public static string AddAGameStringHelper() {
            string newGame = "";
            Console.Write("What is the game name?: ");
            newGame += Console.ReadLine() + ",";
            Console.Write("What is the game's release date?\nMonth: ");
            newGame += Console.ReadLine() + ",";
            Console.Write("Day: ");
            newGame += Console.ReadLine() + ",";
            Console.Write("Year: ");
            newGame += Console.ReadLine() + ",";
            Console.Write("Who's the developer?: ");
            newGame += Console.ReadLine() + ",";
            Console.Write("Who's the publisher?: ");
            newGame += Console.ReadLine();
            Console.WriteLine("====================================================================");

            return newGame;
        }

        public static void AddAGame (string libraryPath) {
            printPage("Add a Game");

            string newGame = AddAGameStringHelper();

            AddAGameListHelper(libraryPath, newGame);
        }

        public static List<Game> SortGamesHelper(string option, List<Game> original) {
            List<Game> sorted;
            while (true) {
                if ((option == "Game Name") || (option == "game name")) {
                    sorted = original.OrderBy(game => game.Name).ToList();
                    Console.WriteLine("Sorted by Game Name.");
                    break;
                } else if ((option == "Release Date") || (option == "release date")) {
                    sorted = original.OrderBy(game => game.Year).ThenBy(game => game.GetMonthAsInt()).ThenBy(game => game.Day).ToList();
                    Console.WriteLine("Sorted by Release Date.");
                    break;
                } else if ((option == "Developer") || (option == "developer")) {
                    sorted = original.OrderBy(game => game.Developer).ToList();
                    Console.WriteLine("Sorted by Developer.");
                    break;
                } else if ((option == "Publisher") || (option == "publisher")) {
                    sorted = original.OrderBy(game => game.Publisher).ToList();
                    Console.WriteLine("Sorted by Publisher.");
                    break;
                } else {
                    Console.Write("Invalid option. No sorting performed. Please try again: ");
                    option = Console.ReadLine();
                }
            }
            return sorted;
        }

        public static void SortGames(string libraryPath) {
            List<Game> gameList = File.ReadAllLines(libraryPath)
                                      .Select(line => new Game(line))
                                      .ToList();

            Console.Write("How do you want to sort the games? (Game Name, Release Date, Developer, Publisher): ");
            string option = Console.ReadLine();

            List<Game> sortedList = SortGamesHelper(option, gameList);

            List<string> newLines = sortedList.Select(game => game.ToCsvString()).ToList();

            File.WriteAllLines(libraryPath, newLines);
            UserLibrary(libraryPath);
        }

        public static void FindUserCredentialsHelper(int countLeft, string type) {
            Console.Write($"Incorrect {type}.");
            Console.WriteLine("You have {0} tries left.", countLeft);
            if (countLeft == 0) {
                Console.WriteLine("Maximum number of tries met. Closing Program");
                Environment.Exit(0);
            }
        }
        public static string[] FindUserCredentialsFinal(string loginPath, string username) {
            string[] lines = File.ReadAllLines(loginPath);

            foreach (string line in lines) {
                string[] parts = line.Split(',');
                if (parts.Length >= 2) {
                    string fileUsername = parts[0].Trim();
                    string filePassword = parts[1].Trim();
                    if (fileUsername.Equals(username)) {
                        return new string[] { fileUsername, filePassword };
                    }
                }
            }

            return new string[] { "NotFound" };
        }

        public static string[] FindUserCredentials(string loginPath, string username) {
            if (!File.Exists(loginPath)) {
                Console.WriteLine($"Error: Login file not found at {loginPath}");
                return new string[] { "NotFound" };
            }

            return FindUserCredentialsFinal(loginPath, username);
        }
        public static bool LoginCheckUser(string loginPath) {
            string newAccountChoice = "";
            bool newAccount = false;

            string[] lines = File.ReadAllLines(loginPath);
            if (lines.Length == 0) {
                Console.Write("There is no account detected. Would you want to make one? [Y/N]: ");
                newAccountChoice = Console.ReadLine();
                if ((newAccountChoice == "Y") || (newAccountChoice == "y") || (newAccountChoice == "Yes") || (newAccountChoice == "yes")) {
                    newAccount = true;
                    Console.Write("What is your username?: ");
                    string account = Console.ReadLine();
                    Console.Write("What is your password?: ");
                    account += "," + Console.ReadLine();
                    File.WriteAllText(loginPath, account);
                } else {
                    Console.WriteLine("Closing the application.");
                    Environment.Exit(0);
                }
            }
            return newAccount;
        }

        public static void LoginPassword (string loginPath, string username) {
            int tries = 3;
            while (tries != 0) {
                Console.Write("Password: ");
                string password = Console.ReadLine();
                string[] credentials = FindUserCredentials(loginPath, username);
                if (credentials[1] != password) {
                    tries--;
                    FindUserCredentialsHelper(tries, "password");
                } else {
                    break;
                }
            }
        }

        public static void LoginUsername (string loginPath) {
            int tries = 3;
            while (tries != 0) {
                Console.Write("Username: ");
                string username = Console.ReadLine();
                string[] credentials = FindUserCredentials(loginPath, username);
                if (credentials[0] != username) {
                    tries--;
                    FindUserCredentialsHelper(tries, "username");
                } else {
                    LoginPassword(loginPath, username);
                    break;
                }
            }
        }

        public static void Login(string loginPath) {
            if (!LoginCheckUser(loginPath)) {
                LoginUsername(loginPath);
            }
        }

        public static string ApplicationMoverHelper(string libraryPath, string helpPath, string pageSelect, string playerOption) {
            if ((playerOption == "Help") || (playerOption == "help")) {
                pageSelect = "Help";
                HelpPage(helpPath);
            } else if ((playerOption == "Library") || (playerOption == "library")) {
                pageSelect = "Library";
                UserLibrary(libraryPath);
            } else if ((playerOption == "Add a Game") || (playerOption == "add a game") || (playerOption == "Add A Game")) {
                pageSelect = "Add a Game";
                AddAGame(libraryPath);
            } else if (((playerOption == "Remove Game") && (pageSelect == "Library")) || ((playerOption == "remove game") && (pageSelect == "Library"))) {
                RemoveGame(libraryPath);
            } else if (((playerOption == "Sort Games") && (pageSelect == "Library")) || ((playerOption == "sort games") && (pageSelect == "Library"))) {
                SortGames(libraryPath);
            } else if ((playerOption == "Remove Game") && (pageSelect != "Library")) {
                Console.WriteLine("Not on the Library Page to remove a game. Please go to Library to do so.");
            } else if ((playerOption == "Logout") || (playerOption == "logout")) {
                Console.WriteLine("Program is logging out. Have a good day.");
            } else {
                Console.WriteLine("Invalid input. Please try again.");
            }
            return pageSelect;
        }

        public static void ApplicationMover(string libraryPath, string helpPath) {
            string pageSelect = "Library";
            string playerOption = "";

            while ((playerOption != "Logout") && (playerOption != "logout")) {
                Console.Write("What do you want to do?: Library, Help, Add a Game, ");
                if (pageSelect == "Library") {
                    Console.WriteLine("Sort Games, Remove Game, Logout");
                } else {
                    Console.WriteLine("Logout");
                }
                playerOption = Console.ReadLine();
                Console.Write("\n");

                pageSelect = ApplicationMoverHelper(libraryPath, helpPath, pageSelect, playerOption);
            }
        }

        public static void Main() {
            Console.WriteLine("Welcome to the Video Game Library.\nHere, you can see which games are in your library, and remove ones you don't want.\n" +
                "Now with the ability to add games directly in the application.\n\nPlease Login");

            string loginPath = @"H:\ComputerScience\OSU\CS361\Course Assignment\sprint 1\Login\login-info.txt";
            string libraryPath = @"H:\ComputerScience\OSU\CS361\Course Assignment\sprint 1\Login\Library.txt";
            string helpPath = @"H:\ComputerScience\OSU\CS361\Course Assignment\sprint 1\Login\help-info.txt";

            Login(loginPath);
            
            Console.WriteLine("Welcome to the video game library application. Here, you can choose to either see your library of owned games, or go to the help page.");

            UserLibrary(libraryPath);

            ApplicationMover(libraryPath, helpPath);

            Environment.Exit(0);
        }

    }
}

public class Game {
    public string Name { get; set; }
    public string Month { get; set; }
    public int Day { get; set; }
    public int Year { get; set; }
    public string Developer { get; set; }
    public string Publisher { get; set; }

    public Game(string csvLine) {
        string[] parts = csvLine.Split(',');
        if (parts.Length == 6) {
            Name = parts[0];
            Month = parts[1];
            Developer = parts[4];
            Publisher = parts[5];
            int.TryParse(parts[2], out int day);
            int.TryParse(parts[3], out int year);
            Day = day;
            Year = year;
        }
    }

    public string ToCsvString() {
        return $"{Name},{Month},{Day},{Year},{Developer},{Publisher}";
    }

    public int GetMonthAsInt() {
        try {
            return DateTime.ParseExact(this.Month, "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;
        } catch {
            if (DateTime.TryParse($"1 {this.Month} 2000", out DateTime dt)) {
                return dt.Month;
            }
            return 0;
        }
    }
}