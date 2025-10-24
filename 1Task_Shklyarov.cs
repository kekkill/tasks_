using System;
using System.Timers;

class Game
{
    Timer timer;
    bool timeE = false;                                                                                            //Окончание таймера
    int timeLimit = 15000;
    
    string startWord;
    string[] usedWords = new string[100];
    int wordsCount = 0;
    int currentPlayer = 1;
    bool gameEnd = false;
    string language = "ru"; 

    void StartGame()
    {
        ChooseLanguage();
        InitializeTimer();
        InitializeGame();
        DisplayGameRules();
        while (!gameEnd)
        {
            PlayTurn();
        }
        timer.Dispose();
    }

    void ChooseLanguage()
    {
        Console.WriteLine("Choose language / Выберите язык:");
        Console.WriteLine("1 - English");
        Console.WriteLine("2 - Русский");
        Console.Write("Ваш выбор / Your choice: ");
        
        string choice = Console.ReadLine();
        if (choice == "1")
        {
            language = "en";
        }
        else
        {
            language = "ru";
        }
        Console.WriteLine();
    }
    void DisplayGameRules()
    {
        Console.Clear();
        
        if (language == "ru")
        {
            Console.WriteLine("ИГРА В СЛОВА");
            Console.WriteLine("=================================");
            Console.WriteLine("Правила:");
            Console.WriteLine("1. Разрешено использовать только бувы из начального слова");
            Console.WriteLine("2. Нельзя повторять ранее произнесенные слова");
            Console.WriteLine("3. На 1 ход выделено 15 секунд");
            Console.WriteLine("4. Проигрывает тот, кто не называет указанное слово за промежуток времени");
            Console.WriteLine("=================================\n");
        }
        else
        {
            Console.WriteLine("WORD GAME");
            Console.WriteLine("=================================");
            Console.WriteLine("Rules:");
            Console.WriteLine("1. You can only use letters from the start word");
            Console.WriteLine("2. You cannot repeat previously spoken words");
            Console.WriteLine("3. You have 15 seconds per turn");
            Console.WriteLine("4. The player who doesn't name a word in time loses");
            Console.WriteLine("=================================\n");
        }
    }

    void InitializeGame()
    {
        if (language == "ru")
        {
            Console.Write("Введите начальное слово: ");
        }
        else
        {
            Console.Write("Enter start word: ");
        }
        
        startWord = Console.ReadLine().ToLower().Trim();
        
        if (language == "ru")
        {
            while (startWord.Length < 8 || startWord.Length > 33)
            {
                Console.Write("Ошибка, длина слова должна быть от 8 до 33 символов: ");
                startWord = Console.ReadLine().ToLower().Trim();
            }
        }
        else
        {
            while (startWord.Length < 8 || startWord.Length > 33)
            {
                Console.Write("Error, word length must be from 8 to 33 characters: ");
                startWord = Console.ReadLine().ToLower().Trim();
            }
        }
        
        usedWords[0] = startWord;
        wordsCount = 1;
        
        if (language == "ru")
        {
            Console.WriteLine($"\nначальное слово установлено: '{startWord}'");
            Console.WriteLine("\nНажмите любую клавишу чтобы начать игру");
        }
        else
        {
            Console.WriteLine($"\nStart word set: '{startWord}'");
            Console.WriteLine("\nPress any key to start the game");
        }
        Console.ReadKey();
    }

    void InitializeTimer()
    {
        timer = new Timer(timeLimit);
        timer.Elapsed += TimerEnd;
        timer.AutoReset = false;
    }

    void TimerEnd(object sender, ElapsedEventArgs e)
    {
        timeE = true;
        if (language == "ru")
        {
            Console.WriteLine("\nВРЕМЯ ВЫШЛО!");
        }
        else
        {
            Console.WriteLine("\nTIME IS UP!");
        }
    }

    void PlayTurn()
    {
        Console.Clear();
        GameHead();
        
        timeE = false;
        
        if (language == "ru")
        {
            Console.WriteLine($"\nХОД ИГРОКА {currentPlayer}");
            Console.WriteLine($"У вас 15 секунд!");
            Console.WriteLine($"Использовано слов: {wordsCount - 1}");
        }
        else
        {
            Console.WriteLine($"\nPLAYER {currentPlayer} TURN");
            Console.WriteLine($"You have 15 seconds!");
            Console.WriteLine($"Words used: {wordsCount - 1}");
        }
        
        DisplayUsedWords();
        
        if (language == "ru")
        {
            Console.Write($"\nИгрок {currentPlayer}, введите слово: ");
        }
        else
        {
            Console.Write($"\nPlayer {currentPlayer}, enter word: ");
        }
        
        timer.Start();
        string word = Console.ReadLine()?.ToLower().Trim();
        timer.Stop();
        
        if (!timeE)
        {
            ProcessPlayerWord(word);
        }
        else
        {
            if (language == "ru")
            {
                EndGame($"Игрок {currentPlayer} не успел ввести слово!");
            }
            else
            {
                EndGame($"Player {currentPlayer} didn't enter word in time!");
            }
        }
    }

    void GameHead()
    {
        if (language == "ru")
        {
            Console.WriteLine("ИГРА В СЛОВА");
            Console.WriteLine($"Начальное слово: '{startWord}'");
            Console.WriteLine("=================================");
        }
        else
        {
            Console.WriteLine("WORD GAME");
            Console.WriteLine($"Start word: '{startWord}'");
            Console.WriteLine("=================================");
        }
    }

    void DisplayUsedWords()
    {
        if (wordsCount > 1)
        {
            if (language == "ru")
            {
                Console.WriteLine($"\nИстория слов:");
            }
            else
            {
                Console.WriteLine($"\nWords history:");
            }
            for (int i = 1; i < wordsCount; i++)
            {
                Console.WriteLine($"   {i}. {usedWords[i]}");
            }
        }
    }

    void ProcessPlayerWord(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            if (language == "ru")
            {
                EndGame($"Игрок {currentPlayer} ввел пустое слово");
            }
            else
            {
                EndGame($"Player {currentPlayer} entered empty word");
            }
            return;
        }
        
        string validationResult = ValidateWord(word);
        
        if (validationResult == "VALID")
        {
            usedWords[wordsCount] = word;
            wordsCount++;
            
            if (language == "ru")
            {
                Console.WriteLine($"\nСлово '{word}' правильно");
                Console.WriteLine("\nНажмите любую клавишу для перехода хода");
            }
            else
            {
                Console.WriteLine($"\nWord '{word}' accepted!");
                Console.WriteLine("\nPress any key for next turn");
            }
            
            Console.ReadKey();
            
            if (currentPlayer == 1)
            {
                currentPlayer = 2;
            }
            else
            {
                currentPlayer = 1;
            }
        }
        else
        {
            if (language == "ru")
            {
                EndGame($"Игрок {currentPlayer} проиграл! {validationResult}");
            }
            else
            {
                EndGame($"Player {currentPlayer} lost! {validationResult}");
            }
        }
    }

    string ValidateWord(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            if (language == "ru")
            {
                return "Слово не может быть пустым!";
            }
            else
            {
                return "Word cannot be empty!";
            }
        }
        if (!ValidateLetters(word, startWord))
        {
            if (language == "ru")
            {
                return $"Слово должно состоять из букв '{startWord}'!";
            }
            else
            {
                return $"Word must consist of letters from '{startWord}'!";
            }
        }
        if (IsWordUsed(word))
        {
            if (language == "ru")
            {
                return "Это слово уже использовалось";
            }
            else
            {
                return "This word was already used";
            }
        }
        
        return "VALID";
    }

    bool ValidateLetters(string playerWord, string startWord)
    {
        bool[] usedLetters = new bool[startWord.Length];
        
        for (int i = 0; i < playerWord.Length; i++)
        {
            char currentChar = playerWord[i];
            bool letterFound = false;
            
            for (int j = 0; j < startWord.Length; j++)
            {
                if (startWord[j] == currentChar && !usedLetters[j])
                {
                    usedLetters[j] = true;
                    letterFound = true;
                    break;
                }
            }
            if (!letterFound)
            {
                return false;
            }
        }
        
        return true;
    }

    bool IsWordUsed(string word)
    {
        for (int i = 0; i < wordsCount; i++)
        {
            if (usedWords[i] == word)
            {
                return true;
            }
        }
        return false;
    }

    void EndGame(string message)
    {
        Console.Clear();
        
        if (language == "ru")
        {
            Console.WriteLine("ИГРА ОКОНЧЕНА");
            Console.WriteLine("=================================");
            Console.WriteLine(message);
            
            Console.WriteLine($"\nСтатистика игры:");
            Console.WriteLine($"Начальное слово: '{startWord}'");
            Console.WriteLine($"Всего слов названо: {wordsCount - 1}");
            
            if (wordsCount > 1)
            {
                Console.WriteLine($"\nВсе названные слова:");
                for (int i = 1; i < wordsCount; i++)
                {
                    Console.WriteLine($"   {i}. {usedWords[i]}");
                }
            }
            int winner;
            if (currentPlayer == 1)
            {
                winner = 2;
            }
            else
            {
                winner = 1;
            }
            Console.WriteLine($"\nПобеда за: Игроком {winner}");
        }
        else
        {
            Console.WriteLine("GAME OVER");
            Console.WriteLine("=================================");
            Console.WriteLine(message);
            Console.WriteLine($"\nGame statistics:");
            Console.WriteLine($"Start word: '{startWord}'");
            Console.WriteLine($"Total words named: {wordsCount - 1}");
            
            if (wordsCount > 1)
            {
                Console.WriteLine($"\nAll named words:");
                for (int i = 1; i < wordsCount; i++)
                {
                    Console.WriteLine($"   {i}. {usedWords[i]}");
                }
            }
            
            int winner;
            if (currentPlayer == 1)
            {
                winner = 2;
            }
            else
            {
                winner = 1;
            }
            Console.WriteLine($"\nWinner: Player {winner}");
        }
        
        gameEnd = true;
    }
    static void Main()
    {
        Game game = new Game();
        game.StartGame();
        
        Console.WriteLine("\n\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}