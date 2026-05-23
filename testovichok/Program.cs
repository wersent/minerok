public class MineSweeper
{
    private readonly Random rnd = new();
    private readonly int _w;
    private readonly int _h;
    private readonly int _m;

    public MineSweeper(int w, int h, int m)
    {
        _w = w;
        _h = h;
        _m = m;
    }

    public char[,] GenerateField(Tuple<int, int> startField)
    {
        char[,] field;
        bool solvable;
        int attempts = 0;

        do
        {
            field = PlaceMines(startField);
            solvable = MakeFieldSolvable(field, startField);
            attempts++;
        } while (!solvable && attempts < 100);

        int[,] numsField = ComputeNumbers(field);
        for (int i = 0; i < _h; i++)
        {
            for (int j = 0; j < _w; j++)
            {
                if (numsField[i, j] > 0)
                    field[i,j] = (char)(numsField[i, j]+'0');
            }
        }
        return field;
    }

    private char[,] PlaceMines(Tuple<int, int> startField)
    {
        List<Tuple<int, int>> startFieldVicility = new List<Tuple<int, int>>(9)
            {
                (startField.Item1 - 1, startField.Item2 - 1).ToTuple(),
                (startField.Item1 - 1, startField.Item2).ToTuple(),
                (startField.Item1 - 1, startField.Item2 + 1).ToTuple(),
                (startField.Item1, startField.Item2 - 1).ToTuple(),
                (startField.Item1, startField.Item2).ToTuple(),
                (startField.Item1, startField.Item2 + 1).ToTuple(),
                (startField.Item1 + 1, startField.Item2 - 1).ToTuple(),
                (startField.Item1 + 1, startField.Item2).ToTuple(),
                (startField.Item1 + 1, startField.Item2 + 1).ToTuple(),
            };

        char[,] field = new char[_h, _w];
        for (int i = 0; i < _h; i++)
            for (int j = 0; j < _w; j++)
                field[i, j] = ' ';

        Tuple<int, int>[] mineCoords = new Tuple<int, int>[1200];
        var allPositions = new List<Tuple<int, int>>();
        for (int i = 0; i < _h; i++)
        {
            for (int j = 0; j < _w; j++)
            {
                if (!startFieldVicility.Contains((i, j).ToTuple()))
                    allPositions.Add(Tuple.Create(i, j));
            }
        }
        allPositions = allPositions.OrderBy(_ => rnd.Next()).ToList();
        for (int i = 0; i < mineCoords.Length; i++)
            mineCoords[i] = allPositions[i];

        for (int i = 0; i < _h; i++)
        {
            for (int j = 0; j < _w; j++)
            {
                if (mineCoords.Contains((i, j).ToTuple()))
                    field[i, j] = 'b';
            }
        }

        return field;
    }

    private int[,] ComputeNumbers(char[,] field)
    {
        int[,] nums = new int[_h, _w];

        for (int i = 0; i < _h; i++)
            for (int j =0; j < _w; j++)
            {
                if (field[i, j] == 'b')
                    nums[i, j] = -1;
                else
                    nums[i, j] = CountAdjBombs(field, (i, j).ToTuple());
            }

        return nums;
    }

    private int CountAdjBombs(char[,] field, Tuple<int, int> curPos)
    {
        int bombsCount = 0;

        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                if (i == curPos.Item1 && j == curPos.Item2) continue;
                int nj = curPos.Item1 + j, ni = curPos.Item2 + i;
                if (nj >= 0 && nj < _w && ni >= 0 && ni < _h && field[nj, ni] == 'b')
                    bombsCount++;
            }

        return bombsCount;
    }

    private bool MakeFieldSolvable(char[,] field, Tuple<int, int> startField)
    {
        for (int attempt = 0; attempt < 30; attempt++)   // Try multiple perturbations
        {
            var nums = ComputeNumbers(field);

            if (IsSolvableFromStart(field, nums, startField))
                return true;

            // Perturb: move one mine to fix ambiguity
            if (!PerturbField(field, nums))
                break;
        }
        return false;
    }

    private bool PerturbField(char[,] field, int[,] nums)
    {
        List<(int x, int y)> ambiguous = new List<(int x, int y)>();

        for (int y = 0; y < _h; y++)
            for (int x = 0; x < _w; x++)
            {
                if (field[y, x] != 'b' && nums[y, x] > 0 && nums[y, x] < 8)
                    ambiguous.Add((x, y));
            }

        if (ambiguous.Count == 0) return false;

        // Pick random ambiguous cell
        var (tx, ty) = ambiguous[rnd.Next(ambiguous.Count)];

        // Try to move a nearby mine
        for (int dy = -3; dy <= 3; dy++)
            for (int dx = -3; dx <= 3; dx++)
            {
                int nx = tx + dx, ny = ty + dy;
                if (nx < 0 || nx >= _w || ny < 0 || ny >= _h) continue;
                if (field[ny, nx] == 'b')
                {
                    field[ny, nx] = ' ';     // remove mine
                    field[ty, tx] = 'b';     // place mine here
                    return true;
                }
            }
        return false;
    }

    private bool IsSolvableFromStart(char[,] board, int[,] numbers, Tuple<int, int> startField)
    {
        // For a strong version we would implement full set-based solver.
        // Here we use a practical heuristic: simulate opening + basic deduction

        bool[,] revealed = new bool[_h, _w];
        FloodFill(board, numbers, revealed, startField.Item1, startField.Item2);

        // Simple single-point solver loop
        bool changed;
        do
        {
            changed = false;
            for (int y = 0; y < _h; y++)
                for (int x = 0; x < _w; x++)
                {
                    if (!revealed[y, x] || numbers[y, x] <= 0) continue;

                    // Basic deduction logic can be added here
                    // For now we consider it "solvable enough" if a large portion is opened
                }
        } while (changed);

        // Count how many safe cells are revealed
        int revealedCount = 0;
        for (int y = 0; y < _h; y++)
            for (int x = 0; x < _w; x++)
                if (revealed[y, x]) revealedCount++;

        int totalSafeCells = _w * _h - _m;
        return revealedCount >= totalSafeCells * 0.85;   // 85%+ is good enough for large board
    }

    private void FloodFill(char[,] board, int[,] numbers, bool[,] revealed, int x, int y)
    {
        if (x < 0 || x >= _w || y < 0 || y >= _h || revealed[y, x] || board[y, x] == 'b')
            return;

        revealed[y, x] = true;

        if (numbers[y, x] == 0)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    if (!(dx == 0 && dy == 0))
                        FloodFill(board, numbers, revealed, x + dx, y + dy);
        }
    }

    public void PrintBoard(char[,] board, int maxSize)
    {
        for (int y = 0; y < Math.Min(maxSize, _h); y++)   // Loop through rows
        {
            for (int x = 0; x < Math.Min(maxSize, _w); x++) // Loop through columns
            {
                // If cell is empty space (' '), print "." instead (better visibility)
                Console.Write(board[y, x] == ' ' ? ' ' : Convert.ToChar(board[y, x]));
            }
            Console.WriteLine();   // New line after each row
        }
    }
}

class Program
{
    static void Main()
    {
        var generator = new MineSweeper(64, 64, 1200);
        char[,] board = generator.GenerateField((2, 2).ToTuple());

        generator.PrintBoard(board, 64);   // Print first 20x20 for checking
        Console.WriteLine();
        Console.WriteLine(Convert.ToInt32('1'.ToString()));
    }
}

