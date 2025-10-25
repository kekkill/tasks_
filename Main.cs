using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

class Program
{
    private const int MIN_WORD_LENGTH = 8;
    private const int MAX_WORD_LENGTH = 33;
    private const int TIME_LIMIT_MS = 15000;
    private const int DISPLAY_HISTORY_LIMIT = 10;

    private static string _startWord = "";
    private static HashSet<string> _usedWords = new HashSet<string>();
    private static int _currentPlayer = 1;
    private static bool _isGameFinished = false;
    private static CultureInfo _currentCulture;
    private static CultureInfo _selectedCulture;

    private static bool _timeExpired = false;
    private static ResourceManager _resourceManager = new ResourceManager("Tasks.Resources", Assembly.GetExecutingAssembly());

    private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(StdHandle std);
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hdl);
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();
    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    private static string LocalWord(string LocalWord_)
    {
        return _resourceManager.GetString(LocalWord_, _currentCulture);
    }

    public static bool CanFormWordFromLetters(string wordToCheck, string sourceWord)
    {
        char[] sourceLetters = sourceWord.ToCharArray();
        bool[] usedLetters = new bool[sourceLetters.Length];

        for (int i = 0; i < wordToCheck.Length; i++)
        {
            char currentChar = wordToCheck[i];
            bool foundLetter = false;

            for (int j = 0; j < sourceLetters.Length; j++)
            {
                if (sourceLetters[j] == currentChar && !usedLetters[j])
                {
                    usedLetters[j] = true;
                    foundLetter = true;
                    break;
                }
            }

            if (!foundLetter)
            {
                return false;
            }
        }

        return true;
    }

    private static void StartTimer()
    {
        _timeExpired = false;
        ThreadPool.QueueUserWorkItem((o) => {
            Thread.Sleep(TIME_LIMIT_MS);
            if (!_timeExpired && !_isGameFinished)
            {
                _timeExpired = true;
                Console.WriteLine();
                Console.WriteLine(LocalWord("TimeExpired"));

                IntPtr stdin = GetStdHandle(StdHandle.Stdin);
                CloseHandle(stdin);
            }
        });
    }

    private static void ChooseLanguage()
    {
        Console.WriteLine(_resourceManager.GetString("ChooseLanguage", CultureInfo.InvariantCulture));
        Console.WriteLine(_resourceManager.GetString("LanguageOption1", CultureInfo.InvariantCulture));
        Console.WriteLine(_resourceManager.GetString("LanguageOption2", CultureInfo.InvariantCulture));
        Console.Write(_resourceManager.GetString("LanguageChoice", CultureInfo.InvariantCulture));

        string choice = Console.ReadLine();
        if (choice == "1")
        {
            _selectedCulture = new CultureInfo("en-US");
            _currentCulture = _selectedCulture;
        }
        else
        {
            _selectedCulture = new CultureInfo("ru-RU");
            _currentCulture = _selectedCulture;
        }
        Console.WriteLine();
    }

    private static void DisplayGameRules()
    {
        Console.Clear();

        Console.WriteLine(LocalWord("GameTitle"));
        Console.WriteLine(LocalWord("Separator"));
        Console.WriteLine(LocalWord("RulesTitle"));
        Console.WriteLine(LocalWord("Rule1"));
        Console.WriteLine(LocalWord("Rule2"));
        Console.WriteLine(LocalWord("Rule3"));
        Console.WriteLine(LocalWord("Rule4"));
        Console.WriteLine(LocalWord("Separator") + "\n");
    }

    private static void InitializeGame()
    {
        Console.Write(LocalWord("EnterStartWord"));

        _startWord = Console.ReadLine().ToLower().Trim();

        string errorMsg = LocalWord("ErrorWordLength");

        while (_startWord.Length < MIN_WORD_LENGTH || _startWord.Length > MAX_WORD_LENGTH)
        {
            Console.Write(errorMsg);
            _startWord = Console.ReadLine().ToLower().Trim();
        }

        _usedWords = new HashSet<string> { _startWord };

        string confirmation = string.Format(LocalWord("StartWordSet"), _startWord);
        Console.WriteLine(confirmation);
        Console.WriteLine(LocalWord("PressAnyKeyToStart"));
        Console.ReadKey();
    }

    private static void DisplayGameHeader()
    {
        Console.WriteLine(LocalWord("GameTitle"));
        string startWordDisplay = string.Format(LocalWord("CurrentStartWord"), _startWord);
        Console.WriteLine(startWordDisplay);
        Console.WriteLine(LocalWord("Separator"));
    }

    private static void DisplayTurnInfo()
    {
        string playerTurn = string.Format(LocalWord("PlayerTurn"), _currentPlayer);
        Console.WriteLine(playerTurn);
        Console.WriteLine(LocalWord("TimeLimit"));

        int wordsUsedCount = _usedWords.Count - 1;
        string wordsUsed = string.Format(LocalWord("WordsUsedCount"), wordsUsedCount);
        Console.WriteLine(wordsUsed);
    }

    private static void DisplayUsedWords()
    {
        if (_usedWords.Count <= 1)
            return;

        Console.WriteLine(LocalWord("WordsHistory"));

        string[] allWords = new string[_usedWords.Count];
        _usedWords.CopyTo(allWords);

        int startIndex = 0;
        if (allWords.Length > DISPLAY_HISTORY_LIMIT)
        {
            startIndex = allWords.Length - DISPLAY_HISTORY_LIMIT;
        }

        int counter = 1;
        int index = startIndex;
        while (index < allWords.Length)
        {
            if (allWords[index] != _startWord)
            {
                Console.WriteLine(LocalWord("WordListFormat"), counter, allWords[index]);
                counter++;
            }
            index++;
        }
    }

    private static string ValidateWord(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return LocalWord("ErrorEmptyWord");
        }

        if (!CanFormWordFromLetters(word, _startWord))
        {
            return string.Format(LocalWord("ErrorInvalidLetters"), _startWord);
        }

        if (_usedWords.Contains(word))
        {
            return LocalWord("ErrorWordUsed");
        }

        return "VALID";
    }

    private static void ProcessPlayerWord(string word)
    {
        string validationResult = ValidateWord(word);

        if (validationResult == "VALID")
        {
            _usedWords.Add(word);

            string successMessage = string.Format(LocalWord("WordAccepted"), word);
            Console.WriteLine(successMessage);

            Console.WriteLine(LocalWord("PressAnyKeyToContinue"));

      
            if (_timeExpired)
            {
                AllocConsole();
            }
            Console.ReadKey();
        }
        else
        {
            string message = string.Format(LocalWord("PlayerLost"), _currentPlayer, validationResult);
            EndGame(message);
        }
    }

    private static void PlayTurn()
    {
        Console.Clear();
        DisplayGameHeader();

        _timeExpired = false;
        DisplayTurnInfo();
        DisplayUsedWords();

        string inputPrompt = string.Format(LocalWord("EnterWordPrompt"), _currentPlayer);
        Console.Write(inputPrompt);

        StartTimer(); 

        string word = "";
        try
        {
            word = Console.ReadLine()?.ToLower().Trim() ?? "";
        }
        catch (Exception)
        {

            word = "";
         
            AllocConsole();
        }

        if (_timeExpired)
        {
            string message = string.Format(LocalWord("TimeExpired"), _currentPlayer);
            EndGame(message);
        }
        else if (string.IsNullOrEmpty(word))
        {
            string message = string.Format(LocalWord("PlayerLost"),
                                         _currentPlayer,
                                         LocalWord("ErrorEmptyWord"));
            EndGame(message);
        }
        else
        {
            ProcessPlayerWord(word);
        }
    }

    private static void DisplayGameOverStatistics(string message)
    {
        Console.WriteLine(LocalWord("GameOver"));
        Console.WriteLine(LocalWord("Separator"));
        Console.WriteLine(message);

        Console.WriteLine(LocalWord("GameStatistics"));

        string startWordInfo = string.Format(LocalWord("CurrentStartWord"), _startWord);
        Console.WriteLine(startWordInfo);

        int wordsUsedCount = _usedWords.Count - 1;
        string totalWords = string.Format(LocalWord("TotalWordsNamed"), wordsUsedCount);
        Console.WriteLine(totalWords);

        if (_usedWords.Count > 1)
        {
            Console.WriteLine(LocalWord("AllNamedWords"));

            string[] allWords = new string[_usedWords.Count];
            _usedWords.CopyTo(allWords);

            int counter = 1;
            int index = 0;
            while (index < allWords.Length)
            {
                if (allWords[index] != _startWord)
                {
                    Console.WriteLine(LocalWord("WordListFormat"), counter, allWords[index]);
                    counter++;
                }
                index++;
            }
        }

        int winner;
        if (_currentPlayer == 1)
        {
            winner = 2;
        }
        else
        {
            winner = 1;
        }
        string winnerMessage = string.Format(LocalWord("Winner"), winner);
        Console.WriteLine(winnerMessage);
    }

    private static void EndGame(string message)
    {
        Console.Clear();
        DisplayGameOverStatistics(message);
        _isGameFinished = true;
    }

    private static void StartGame()
    {
        ChooseLanguage();
        DisplayGameRules();
        InitializeGame();

        while (!_isGameFinished)
        {
            PlayTurn();
        }
    }

    static void Main()
    {
        StartGame();
        if (_timeExpired)
        {
            AllocConsole();
        }

        Console.WriteLine(_resourceManager.GetString("OutInfo", _selectedCulture));
        Console.WriteLine(LocalWord("PressAnyKeyToExit"));
        Thread.Sleep(2000);
    }
}