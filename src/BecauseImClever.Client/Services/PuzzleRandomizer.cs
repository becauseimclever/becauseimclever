namespace BecauseImClever.Client.Services;

/// <summary>
/// Generates deterministic puzzle parameters from a seed.
/// The same seed always produces identical puzzles, enabling testability and session consistency.
/// </summary>
public class PuzzleRandomizer
{
    private static readonly string[] Messages =
    [
        "THE TRUTH IS OUT THERE",
        "SEEK AND YOU SHALL FIND",
        "KNOWLEDGE IS POWER",
        "ESCAPE THE LABYRINTH",
        "FOLLOW THE WHITE RABBIT",
    ];

    private static readonly string[] AllIngredients =
    [
        "Dragon Pepper", "Moon Sugar", "Starlight Flour",
        "Phantom Salt", "Echo Honey", "Twilight Butter",
        "Shadow Vanilla", "Thunder Milk",
    ];

    private static readonly DateOnly[] DatePool =
    [
        new(1876, 3, 10), new(1903, 12, 17), new(1927, 5, 21),
        new(1945, 8, 6), new(1969, 7, 20), new(1981, 8, 12),
        new(1995, 1, 1), new(2001, 10, 23), new(2012, 7, 4),
        new(2020, 2, 29),
    ];

    private PuzzleRandomizer(int seed)
    {
        this.Seed = seed;
    }

    public int Seed { get; }

    public static PuzzleRandomizer NewGame()
        => new(Environment.TickCount);

    public static PuzzleRandomizer FromSeed(int seed)
        => new(seed);

    public FoyerPuzzleParams GenerateFoyerPuzzle()
    {
        var rng = new Random(this.Seed);
        var selected = Shuffle(rng, DatePool).Take(5).ToList();
        var correctOrder = selected.OrderBy(d => d).ToList();
        return new FoyerPuzzleParams(selected, correctOrder);
    }

    public LibraryPuzzleParams GenerateLibraryPuzzle()
    {
        var rng = new Random(this.Seed ^ 0x4C494252); // "LIBR" xor to offset from foyer seed
        var cipherKey = GenerateCipherKey(rng);
        var message = Messages[rng.Next(Messages.Length)];
        var encoded = Encode(message, cipherKey);
        return new LibraryPuzzleParams(cipherKey, encoded, message);
    }

    public KitchenPuzzleParams GenerateKitchenPuzzle()
    {
        var rng = new Random(this.Seed ^ 0x4B495443); // "KITC"
        var selected = Shuffle(rng, AllIngredients).Take(5).ToList();
        var correctSequence = Shuffle(rng, selected.ToArray()).ToList();
        return new KitchenPuzzleParams(selected, correctSequence);
    }

    public StudyPuzzleParams GenerateStudyPuzzle()
    {
        var rng = new Random(this.Seed ^ 0x53545544); // "STUD"
        var digits = string.Concat(Enumerable.Range(0, 3).Select(_ => rng.Next(0, 10)));
        return new StudyPuzzleParams(digits);
    }

    public GardenPuzzleParams GenerateGardenPuzzle()
    {
        var rng = new Random(this.Seed ^ 0x47415244); // "GARD"
        var digits = string.Concat(Enumerable.Range(0, 3).Select(_ => rng.Next(0, 10)));
        var mazeSize = 7;
        var walls = GenerateMazeWalls(rng, mazeSize);
        return new GardenPuzzleParams(digits, walls, mazeSize);
    }

    public ExitPuzzleParams GenerateExitPuzzle()
    {
        var study = this.GenerateStudyPuzzle();
        var garden = this.GenerateGardenPuzzle();
        return new ExitPuzzleParams(study.CodeDigits + garden.CodeDigits);
    }

    private static string GenerateCipherKey(Random rng)
    {
        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        return new string(Shuffle(rng, alphabet).ToArray());
    }

    private static string Encode(string plainText, string cipherKey)
    {
        return new string(plainText.Select(c =>
        {
            if (char.IsLetter(c))
            {
                var index = char.ToUpperInvariant(c) - 'A';
                return cipherKey[index];
            }

            return c;
        }).ToArray());
    }

    private static T[] Shuffle<T>(Random rng, T[] source)
    {
        var array = source.ToArray();
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return array;
    }

    private static bool[,] GenerateMazeWalls(Random rng, int size)
    {
        var walls = new bool[size, size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                // Start (0,0) and end (size-1,size-1) are always open
                if ((x == 0 && y == 0) || (x == size - 1 && y == size - 1))
                {
                    walls[y, x] = false;
                }
                else
                {
                    walls[y, x] = rng.Next(100) < 30; // ~30% walls
                }
            }
        }

        return walls;
    }
}
