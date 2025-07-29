using KaimiraGames;
using System.Text.RegularExpressions;

namespace WordleSolver.Source;


public enum GuessResultLetterState
{
    Grey,
    Yellow,
    Green
}

public record struct WordRemoved(string word, GuessResultLetterData wordGuessResultLetterData)
{

}

public record struct GuessResultLetterData(char letter, int letterIndex, GuessResultLetterState letterState)
{
    public bool ShouldKeep(string possibleAnswerWord, List<GuessResultLetterData> guessResultLetterDataList, List<WordRemoved> wordRemoveds)
    {
        switch (letterState)
        {
            case GuessResultLetterState.Grey:
                string possibleAnswerGreenGuessRemoved = possibleAnswerWord;

                foreach (GuessResultLetterData guessResultLetterData in guessResultLetterDataList)
                {
                    if (guessResultLetterData.letterState == GuessResultLetterState.Green && guessResultLetterData.letter == letter)
                    {
                        return true;
                    }
                }

                foreach (GuessResultLetterData guessResultLetterData in guessResultLetterDataList)
                {
                    if (guessResultLetterData.letterState == GuessResultLetterState.Yellow && guessResultLetterData.letter == letter)
                    {
                        return true;
                    }
                }

                if (!possibleAnswerWord.Contains(letter))
                {
                    return true;
                }


                break;

            case GuessResultLetterState.Yellow:
                possibleAnswerGreenGuessRemoved = possibleAnswerWord;

                foreach (GuessResultLetterData guessResultLetterData in guessResultLetterDataList)
                {
                    if (guessResultLetterData.letterState == GuessResultLetterState.Green && guessResultLetterData.letter == letter)
                    {
                        return true;
                    }
                }

                if (possibleAnswerWord.Contains(letter) && possibleAnswerWord[letterIndex] != letter)
                {
                    return true;
                }
                break;

            case GuessResultLetterState.Green:
                if (possibleAnswerWord[letterIndex] == letter)
                {
                    return true;
                }
                break;
        }

        wordRemoveds.Add(new WordRemoved(possibleAnswerWord, this));
        return false;
    }

}


internal class Program
{

    public static WeightedList<string> ReducePossibility(string wordleExpression, WeightedList<string> possibleAnswers)
    {
        string[] wordleExpressionArr = wordleExpression.Split(" ");
        List<GuessResultLetterData> guessResultLetterDataList = new();


        for (int i = 0; i < wordleExpressionArr.Length; i++)
        {
            if (wordleExpressionArr[i] == "ye" || wordleExpressionArr[i] == "ge" || wordleExpressionArr[i] == "gr")
            {
                try
                {
                    string wordleGuessLetter = wordleExpressionArr[i - 1];
                    string wordleType = wordleExpressionArr[i];

                    switch (wordleType)
                    {
                        case "gr":
                            guessResultLetterDataList.Add(new GuessResultLetterData(wordleGuessLetter[0], i / 2, GuessResultLetterState.Grey));
                            break;

                        case "ye":
                            guessResultLetterDataList.Add(new GuessResultLetterData(wordleGuessLetter[0], i / 2, GuessResultLetterState.Yellow));
                            break;

                        case "ge":
                            guessResultLetterDataList.Add(new GuessResultLetterData(wordleGuessLetter[0], i / 2, GuessResultLetterState.Green));
                            break;

                        default:
                            Console.WriteLine("Invalid wordle expression");
                            return possibleAnswers;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid wordle expression");
                    return possibleAnswers;
                }
            }
        }

        WeightedList<string> newPossibleAnswers = new();

        foreach (string possibleAnswer in possibleAnswers)
        {
            bool shouldRemove = false;

            foreach (GuessResultLetterData guessResultLetterData in guessResultLetterDataList)
            {
                if (!guessResultLetterData.ShouldKeep(possibleAnswer, guessResultLetterDataList, wordRemoveds))
                {
                    shouldRemove = true;
                    break;
                }
            }

            if (!shouldRemove)
            {
                newPossibleAnswers.Add(possibleAnswer, possibleAnswers.GetWeightOf(possibleAnswer));
            }
            else
            {
#if DEBUG
                Console.WriteLine($"Removing possible answer \"{possibleAnswer}\"");
#endif
            }
        }

        return newPossibleAnswers;
    }

    static WeightedList<string> possibleAnwsers = new();
    static WeightedList<string> possibleAnwsersWithNonDuplicateLetters = new();
    static string firstGuesssWord = "";
    static string[] wordsRaw = File.ReadAllLines("Assets/Words.txt");
    static string[] wordsWithNonDuplicateLettersRaw = File.ReadAllLines("Assets/WordsWithNonDuplicateLetters.txt");
    static bool isInit;
    static List<WordRemoved> wordRemoveds = new();


    static void Init()
    {
        possibleAnwsers.Clear();

        for (int i = 0; i < wordsRaw.Length; i++)
        {
            string[] wordWithFrequency = wordsRaw[i].Split(",");
            possibleAnwsers.Add(wordWithFrequency[0], int.Parse(wordWithFrequency[1]));
        }

        if (!isInit)
        {
            for (int i = 0; i < wordsWithNonDuplicateLettersRaw.Length; i++)
            {
                string[] wordsWithNonDuplicateLettersWithFrequency = wordsWithNonDuplicateLettersRaw[i].Split(",");
                possibleAnwsersWithNonDuplicateLetters.Add(wordsWithNonDuplicateLettersWithFrequency[0], int.Parse(wordsWithNonDuplicateLettersWithFrequency[1]));
            }
        }

        firstGuesssWord = possibleAnwsersWithNonDuplicateLetters.Next();

        Console.WriteLine();
        Console.WriteLine($"Your choosen word is \"{firstGuesssWord}\"");

        isInit = true;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("""
How to use:
    The program will give you a possible answer. You need to type that on wordle,
    then type the wordle evaluation result in the guess result prompt with oscillating pair of letter and letter state.
    Where letter state is

    gr = grey 
    ye = yellow
    ge = green

    for example:
    Your guess word next is "crane"
    Guess result: c ye r ge a gr n gr e gr

    this means

    c = yellow
    r = green
    a = grey
    n = grey
    e = grey

    you can also type these word in the guess result:

    Guess result: reset
    Reset the program
    
    Guess result: won
    If you won the game and type this it will congrats you and reset the program


    Guess result: lost
    If you lost the game and type this it will be sad for you and reset the program


    Guess result: alt
    If choosen guess word is not in the wordle word list then you can use this for finding alternative word
""".Trim());

        Console.WriteLine();

        Init();

        string guessWord = "";

        while (true)
        {
            Console.Write("Guess result: ");

            string? guessResult = Console.ReadLine();

            if (guessResult is null || guessResult.Trim() == "")
            {
                continue;
            }

            guessResult = Regex.Replace(guessResult, @"\s+", " ");

            bool isFindAlternative = false;

            if (guessResult == "reset")
            {
                Init();
                continue;
            }
            else if (guessResult == "won")
            {
                Console.WriteLine($"Congratulation for wining lol");
                Init();
                continue;
            }
            else if (guessResult == "lost")
            {
                Console.WriteLine($"I'm sorry for your lost");
                Init();
                continue;
            }
            else if (guessResult == "alt")
            {
                Console.WriteLine($"Finding alternative words");

                possibleAnwsers.Remove(guessWord);

                if (possibleAnwsers.Count == 1)
                {
                    Console.WriteLine($"There is no more alternative that's the last possibility and it is \"{guessWord}\"");
                    Init();
                    continue;
                }

                isFindAlternative = true;
            }
            else if (guessResult.Split(" ").Length != 10)
            {
                Console.WriteLine("Invalid command or expression");
                possibleAnwsers.Remove(guessWord);
            }

            if (!isFindAlternative)
            {
                possibleAnwsers = ReducePossibility(guessResult, possibleAnwsers);
            }

            try
            {
                guessWord = possibleAnwsers.Next();
                Console.WriteLine($"Your guess word is \"{guessWord}\"");
            }
            catch (Exception)
            {
                Console.WriteLine("Word answser: ");
                string wordAnswer = Console.ReadLine()!;

                foreach (WordRemoved wordRemoved in wordRemoveds)
                {
                    if (wordRemoved.word == wordAnswer)
                    {
                        WordRemoved wordCausingError = wordRemoved;
                    }
                }
            }

        }
    }
}
