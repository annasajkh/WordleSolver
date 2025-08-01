using MoreComplexDataStructures;
using System.Text.RegularExpressions;

namespace WordleSolver.Source;


public enum GuessResultLetterState
{
    Grey,
    Yellow,
    Green
}

public record struct GuessResultLetterData(char letter, int letterIndex, GuessResultLetterState letterState)
{
    public bool ShouldKeep(string possibleAnswerWord, List<GuessResultLetterData> guessResultLetterDataList)
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
        return false;
    }

}


internal class Program
{

    public static List<Tuple<string, long>> ReducePossibility(string wordleExpression, List<Tuple<string, long>> possibleAnswersWeights)
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
                            return possibleAnswersWeights;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid wordle expression");
                    return possibleAnswersWeights;
                }
            }
        }

        List<Tuple<string, long>> newPossibleAnswers = new();

        foreach (Tuple<string, long> possibleAnswer in possibleAnswersWeights)
        {
            bool shouldRemove = false;

            foreach (GuessResultLetterData guessResultLetterData in guessResultLetterDataList)
            {
                if (!guessResultLetterData.ShouldKeep(possibleAnswer.Item1, guessResultLetterDataList))
                {
                    shouldRemove = true;
                    break;
                }
            }

            if (!shouldRemove)
            {
                newPossibleAnswers.Add(new Tuple<string, long>(possibleAnswer.Item1, possibleAnswer.Item2));
            }
            else
            {
                Console.WriteLine($"Removing possible answer {possibleAnswer}");
            }
        }

        return newPossibleAnswers;
    }

    static WeightedRandomGenerator<string> possibleAnswersGenerator = new();
    static WeightedRandomGenerator<string> possibleAnswersWithNonDuplicateLettersGenerator = new();

    static List<Tuple<string, long>> possibleAnswersWeights = new();
    static List<Tuple<string, long>> possibleAnswersWithNonDuplicateLettersWeights = new();

    static string firstGuessWord = "";
    static string[] wordsRaw = File.ReadAllLines("Assets/Words/Words.txt");
    static string[] wordsWithNonDuplicateLettersRaw = File.ReadAllLines("Assets/Words/WordsWithNonDuplicateLetters.txt");

    static bool isFirstGuessWord = true;

    static void Init()
    {
        isFirstGuessWord = true;
        possibleAnswersWeights.Clear();
        possibleAnswersWithNonDuplicateLettersWeights.Clear();

        for (int i = 0; i < wordsRaw.Length; i++)
        {
            string[] wordWithFrequency = wordsRaw[i].Split(",");
            possibleAnswersWeights.Add(new Tuple<string, long>(wordWithFrequency[0], long.Parse(wordWithFrequency[1])));
        }

        for (int i = 0; i < wordsWithNonDuplicateLettersRaw.Length; i++)
        {
            string[] wordsWithNonDuplicateLettersWithFrequency = wordsWithNonDuplicateLettersRaw[i].Split(",");
            possibleAnswersWithNonDuplicateLettersWeights.Add(new Tuple<string, long>(wordsWithNonDuplicateLettersWithFrequency[0], int.Parse(wordsWithNonDuplicateLettersWithFrequency[1])));
        }

        possibleAnswersGenerator.SetWeightings(possibleAnswersWeights);
        possibleAnswersWithNonDuplicateLettersGenerator.SetWeightings(possibleAnswersWithNonDuplicateLettersWeights);

        firstGuessWord = possibleAnswersWithNonDuplicateLettersGenerator.Generate();

        Console.WriteLine();
        Console.WriteLine($"Your guess word is \"{firstGuessWord}\"");
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

    you can also type these commands in the guess result:

    Guess result: reset
    Reset the program
    
    Guess result: won
    If you won the game and type this it will congrats you and reset the program


    Guess result: lost
    If you lost the game and type this it will be sad for you and reset the program


    Guess result: alt
    If chosen guess word is not in the wordle word list then you can use this for finding alternative word
""".Trim());

        Console.WriteLine();

        Init();

        string guessWord = firstGuessWord;

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
                isFindAlternative = true;

                Console.WriteLine($"Finding alternative words");

                if (isFindAlternative && isFirstGuessWord)
                {
#if DEBUG
                    Console.WriteLine("Removing from possibleAnswersWithNonDuplicateLetters");
#endif

                    for (int i = possibleAnswersWithNonDuplicateLettersWeights.Count - 1; i >= 0; i--)
                    {
                        if (possibleAnswersWithNonDuplicateLettersWeights[i].Item1 == guessWord)
                        {
                            possibleAnswersWithNonDuplicateLettersWeights.Remove(new Tuple<string, long>(possibleAnswersWithNonDuplicateLettersWeights[i].Item1, possibleAnswersWithNonDuplicateLettersWeights[i].Item2));
                        }
                    }

                    possibleAnswersWithNonDuplicateLettersGenerator.SetWeightings(possibleAnswersWithNonDuplicateLettersWeights);
                }
                else
                {
#if DEBUG
                    Console.WriteLine("Removing from possibleAnswers");
#endif
                    for (int i = possibleAnswersWeights.Count - 1; i >= 0; i--)
                    {
                        if (possibleAnswersWeights[i].Item1 == guessWord)
                        {
                            possibleAnswersWeights.Remove(new Tuple<string, long>(possibleAnswersWeights[i].Item1, possibleAnswersWeights[i].Item2));
                        }
                    }

                    possibleAnswersGenerator.SetWeightings(possibleAnswersWeights);
                }

                if (isFindAlternative && isFirstGuessWord)
                {
                    if (possibleAnswersWithNonDuplicateLettersGenerator.WeightingCount == 1)
                    {
#if DEBUG
                        Console.WriteLine("Getting from possibleAnswersWithNonDuplicateLetters");
#endif
                        Console.WriteLine($"There is no more alternative that's the last possibility and it is \"{guessWord}\"");
                        Init();
                        continue;
                    }
                }
                else
                {
                    if (possibleAnswersGenerator.WeightingCount == 1)
                    {
#if DEBUG
                        Console.WriteLine("Getting from possibleAnswers");
#endif
                        Console.WriteLine($"There is no more alternative that's the last possibility and it is \"{guessWord}\"");
                        Init();
                        continue;
                    }
                }

            }
            else if (guessResult.Split(" ").Length != 10)
            {
                Console.WriteLine("Invalid command or expression");
                continue;
            }

            if (!isFindAlternative)
            {
                possibleAnswersWeights = ReducePossibility(guessResult, possibleAnswersWeights);
                possibleAnswersGenerator.SetWeightings(possibleAnswersWeights);
                isFirstGuessWord = false;
            }

            try
            {
                if (isFirstGuessWord && isFindAlternative)
                {
#if DEBUG
                    Console.WriteLine("Getting from possibleAnswersWithNonDuplicateLetters");
#endif
                    guessWord = possibleAnswersWithNonDuplicateLettersGenerator.Generate();
                    Console.WriteLine($"Your guess word is \"{guessWord}\"");
                }
                else
                {
#if DEBUG
                    Console.WriteLine("Getting from possibleAnswers");
#endif
                    guessWord = possibleAnswersGenerator.Generate();
                    Console.WriteLine($"Your guess word is \"{guessWord}\"");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error: {exception}");
            }

        }
    }
}
