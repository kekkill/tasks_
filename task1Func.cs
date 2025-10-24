using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Timers;
using Timer = System.Timers.Timer;

public static class WordValidator
{
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
}

public class GameState
{
    public string StartWord;
    public HashSet<string> UsedWords;
    public int CurrentPlayer;
    public bool IsGameFinished;
    public CultureInfo CurrentCulture;

    public GameState()
    {
        StartWord = "";
        UsedWords = new HashSet<string>();
        CurrentPlayer = 1;
        IsGameFinished = false;
    }

    public int GetWordsUsedCount()
    {
        return UsedWords.Count - 1;
    }
}

public class Game
{
    private const int MIN_WORD_LENGTH = 8;
    private const int MAX_WORD_LENGTH = 33;
    private const int TIME_LIMIT_MS = 15000;
    private const int DISPLAY_HISTORY_LIMIT = 10;

    private Timer _timer;
    private bool _timeExpired = false;
    private GameState _state;
    private ResourceManager _resourceManager;

    public Game()
    {
        _resourceManager = new ResourceManager("Tasks.Resources", Assembly.GetExecutingAssembly());
        _timer = new Timer(TIME_LIMIT_MS);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = false;
        _state = new GameState();
    }

    public void StartGame()
    {
        ChooseLanguage();
        DisplayGameRules();
        InitializeGame();

        while (!_state.IsGameFinished)
        {
            PlayTurn();
        }

        _timer.Stop();
    }

    private void ChooseLanguage()
    {
        Console.WriteLine(_resourceManager.GetString("ChooseLanguage", CultureInfo.InvariantCulture));
        Console.WriteLine(_resourceManager.GetString("LanguageOption1", CultureInfo.InvariantCulture));
        Console.WriteLine(_resourceManager.GetString("LanguageOption2", CultureInfo.InvariantCulture));
        Console.Write(_resourceManager.GetString("LanguageChoice", CultureInfo.InvariantCulture));

        string choice = Console.ReadLine();
        if (choice == "1")
        {
            _state.CurrentCulture = new CultureInfo("en-US");
        }
        else
        {
            _state.CurrentCulture = new CultureInfo("ru-RU");
        }
        Console.WriteLine();
    }

    private void DisplayGameRules()
    {
        Console.Clear();

        Console.WriteLine(_resourceManager.GetString("GameTitle", _state.CurrentCulture));
        Console.WriteLine("=================================");
        Console.WriteLine(_resourceManager.GetString("RulesTitle", _state.CurrentCulture));
        Console.WriteLine(_resourceManager.GetString("Rule1", _state.CurrentCulture));
        Console.WriteLine(_resourceManager.GetString("Rule2", _state.CurrentCulture));
        Console.WriteLine(_resourceManager.GetString("Rule3", _state.CurrentCulture));
        Console.WriteLine(_resourceManager.GetString("Rule4", _state.CurrentCulture));
        Console.WriteLine("=================================\n");
    }

    private void InitializeGame()
    {
        Console.Write(_resourceManager.GetString("EnterStartWord", _state.CurrentCulture));

        _state.StartWord = Console.ReadLine().ToLower().Trim();

        string errorMsg = _resourceManager.GetString("ErrorWordLength", _state.CurrentCulture);

        while (_state.StartWord.Length < MIN_WORD_LENGTH || _state.StartWord.Length > MAX_WORD_LENGTH)
        {
            Console.Write(errorMsg);
            _state.StartWord = Console.ReadLine().ToLower().Trim();
        }

        _state.UsedWords = new HashSet<string> { _state.StartWord };

        string confirmation = string.Format(_resourceManager.GetString("StartWordSet", _state.CurrentCulture), _state.StartWord);
        Console.WriteLine(confirmation);
        Console.WriteLine(_resourceManager.GetString("PressAnyKeyToStart", _state.CurrentCulture));
        Console.ReadKey();
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        _timeExpired = true;
        Console.WriteLine();
        Console.WriteLine(_resourceManager.GetString("TimeExpired", _state.CurrentCulture));
    }

    private void PlayTurn()
    {
        Console.Clear();
        DisplayGameHeader();

        _timeExpired = false;
        DisplayTurnInfo();
        DisplayUsedWords();

        string inputPrompt = string.Format(_resourceManager.GetString("EnterWordPrompt", _state.CurrentCulture), _state.CurrentPlayer);
        Console.Write(inputPrompt);

        _timer.Start();
        string word = Console.ReadLine().ToLower().Trim();
        _timer.Stop();

        if (_timeExpired)
        {
            string message = string.Format(_resourceManager.GetString("TimeExpired", _state.CurrentCulture), _state.CurrentPlayer);
            EndGame(message);
        }
        else if (!string.IsNullOrEmpty(word))
        {
            ProcessPlayerWord(word);
        }
        else
        {
            string message = string.Format(_resourceManager.GetString("PlayerLost", _state.CurrentCulture),
                                         _state.CurrentPlayer,
                                         _resourceManager.GetString("ErrorEmptyWord", _state.CurrentCulture));
            EndGame(message);
        }
    }

    private void DisplayGameHeader()
    {
        Console.WriteLine(_resourceManager.GetString("GameTitle", _state.CurrentCulture));
        string startWordDisplay = string.Format(_resourceManager.GetString("CurrentStartWord", _state.CurrentCulture), _state.StartWord);
        Console.WriteLine(startWordDisplay);
        Console.WriteLine("=================================");
    }

    private void DisplayTurnInfo()
    {
        string playerTurn = string.Format(_resourceManager.GetString("PlayerTurn", _state.CurrentCulture), _state.CurrentPlayer);
        Console.WriteLine(playerTurn);
        Console.WriteLine(_resourceManager.GetString("TimeLimit", _state.CurrentCulture));

        string wordsUsed = string.Format(_resourceManager.GetString("WordsUsedCount", _state.CurrentCulture), _state.GetWordsUsedCount());
        Console.WriteLine(wordsUsed);
    }

    private void DisplayUsedWords()
    {
        if (_state.UsedWords.Count <= 1)
            return;

        Console.WriteLine(_resourceManager.GetString("WordsHistory", _state.CurrentCulture));

        string[] allWords = new string[_state.UsedWords.Count];
        _state.UsedWords.CopyTo(allWords);

        int startIndex = 0;
        if (allWords.Length > DISPLAY_HISTORY_LIMIT)
        {
            startIndex = allWords.Length - DISPLAY_HISTORY_LIMIT;
        }

        int counter = 1;
        int index = startIndex;
        while (index < allWords.Length)
        {
            if (allWords[index] != _state.StartWord)
            {
                Console.WriteLine("   {0}. {1}", counter, allWords[index]);
                counter++;
            }
            index++;
        }
    }

    private void ProcessPlayerWord(string word)
    {
        string validationResult = ValidateWord(word);

        if (validationResult == "VALID")
        {
            _state.UsedWords.Add(word);

            string successMessage = string.Format(_resourceManager.GetString("WordAccepted", _state.CurrentCulture), word);
            Console.WriteLine(successMessage);

            Console.WriteLine(_resourceManager.GetString("PressAnyKeyToContinue", _state.CurrentCulture));
            Console.ReadKey();

            if (_state.CurrentPlayer == 1)
            {
                _state.CurrentPlayer = 2;
            }
            else
            {
                _state.CurrentPlayer = 1;
            }
        }
        else
        {
            string message = string.Format(_resourceManager.GetString("PlayerLost", _state.CurrentCulture), _state.CurrentPlayer, validationResult);
            EndGame(message);
        }
    }

    private string ValidateWord(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return _resourceManager.GetString("ErrorEmptyWord", _state.CurrentCulture);
        }

        if (!WordValidator.CanFormWordFromLetters(word, _state.StartWord))
        {
            return string.Format(_resourceManager.GetString("ErrorInvalidLetters", _state.CurrentCulture), _state.StartWord);
        }

        if (_state.UsedWords.Contains(word))
        {
            return _resourceManager.GetString("ErrorWordUsed", _state.CurrentCulture);
        }

        return "VALID";
    }

    private void EndGame(string message)
    {
        Console.Clear();
        DisplayGameOverStatistics(message);
        _state.IsGameFinished = true;
    }

    private void DisplayGameOverStatistics(string message)
    {
        Console.WriteLine(_resourceManager.GetString("GameOver", _state.CurrentCulture));
        Console.WriteLine("=================================");
        Console.WriteLine(message);

        Console.WriteLine(_resourceManager.GetString("GameStatistics", _state.CurrentCulture));

        string startWordInfo = string.Format(_resourceManager.GetString("CurrentStartWord", _state.CurrentCulture), _state.StartWord);
        Console.WriteLine(startWordInfo);

        string totalWords = string.Format(_resourceManager.GetString("TotalWordsNamed", _state.CurrentCulture), _state.GetWordsUsedCount());
        Console.WriteLine(totalWords);

        if (_state.UsedWords.Count > 1)
        {
            Console.WriteLine(_resourceManager.GetString("AllNamedWords", _state.CurrentCulture));

            string[] allWords = new string[_state.UsedWords.Count];
            _state.UsedWords.CopyTo(allWords);

            int counter = 1;
            int index = 0;
            while (index < allWords.Length)
            {
                if (allWords[index] != _state.StartWord)
                {
                    Console.WriteLine("   {0}. {1}", counter, allWords[index]);
                    counter++;
                }
                index++;
            }
        }
        int winner;
        if(_state.CurrentPlayer == 1)
        {
            winner = 2;
        }
        else
        {
            winner = 1;
        }
            string winnerMessage = string.Format(_resourceManager.GetString("Winner", _state.CurrentCulture), winner);
        Console.WriteLine(winnerMessage);
    }
}

class Program
{
    static void Main()
    {
        Game game = new Game();
        game.StartGame();

        ResourceManager resources = new ResourceManager("Tasks.Resources", Assembly.GetExecutingAssembly());
        Console.WriteLine(resources.GetString("OutInfo", CultureInfo.CurrentCulture));
        Console.ReadKey();
    }
}