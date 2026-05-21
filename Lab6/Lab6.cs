namespace HungarianMethod
{
    class Lab6
    {
        static int N;
        static int[,] originalCost;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            int rows, cols;
            while (true)
            {
                while (true)
                {
                    Console.Write("Введіть кількість рядків: ");
                    if (int.TryParse(Console.ReadLine(), out rows) && rows > 0)
                        break;
                    Console.WriteLine("Помилка: має бути ціле додатне число.");
                }
                while (true)
                {
                    Console.Write("Введіть кількість стовпців: ");
                    if (int.TryParse(Console.ReadLine(), out cols) && cols > 0)
                        break;
                    Console.WriteLine("Помилка: має бути ціле додатне число.");
                }
                if (rows == cols) { N = rows; break; }
                Console.WriteLine($"Помилка: матриця має бути квадратною (рядки = {rows}, стовпці = {cols}). Спробуйте ще раз.");
            }

            originalCost = new int[N, N];
            int[,] cost = new int[N, N];

            Console.WriteLine($"Введіть матрицю вартостей робіт ({N}x{N}):");
            for (int i = 0; i < N; i++)
            {
                while (true)
                {
                    Console.Write($"Рядок {i + 1}: ");
                    string[] parts = Console.ReadLine()
                        .Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length != N)
                    {
                        Console.WriteLine($"Помилка: потрібно ввести рівно {N} чисел.");
                        continue;
                    }

                    bool ok = true;
                    int[] row = new int[N];
                    for (int j = 0; j < N; j++)
                    {
                        if (!int.TryParse(parts[j], out row[j]))
                        {
                            Console.WriteLine($"Помилка: '{parts[j]}' не є цілим числом.");
                            ok = false;
                            break;
                        }
                    }
                    if (!ok) continue;

                    for (int j = 0; j < N; j++)
                    {
                        originalCost[i, j] = row[j];
                        cost[i, j] = row[j];
                    }
                    break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Згенерований протокол обчислення:");
            Console.WriteLine();
            Console.WriteLine("Матриця вартостей:");
            PrintMatrix(cost);

            Console.WriteLine("Пошук мінімальних елементів у кожному рядку та віднімання його від кожного елемента в рядку:");
            for (int i = 0; i < N; i++)
            {
                int rowMin = int.MaxValue;
                for (int j = 0; j < N; j++) if (cost[i, j] < rowMin) rowMin = cost[i, j];
                Console.WriteLine($"В рядку {i + 1} знайдено 'min': {rowMin}");
                for (int j = 0; j < N; j++) cost[i, j] -= rowMin;
            }
            Console.WriteLine();
            Console.WriteLine("Матриця вартостей після віднімання мінімальних елементів у рядках:");
            PrintMatrix(cost);

            Console.WriteLine("Пошук мінімальних елементів у кожному стовпці та віднімання його від кожного елемента в стовпці:");
            for (int j = 0; j < N; j++)
            {
                int colMin = int.MaxValue;
                for (int i = 0; i < N; i++) if (cost[i, j] < colMin) colMin = cost[i, j];
                Console.WriteLine($"В стовпці {j + 1} знайдено 'min': {colMin}");
                for (int i = 0; i < N; i++) cost[i, j] -= colMin;
            }
            Console.WriteLine();
            Console.WriteLine("Матриця вартостей після віднімання мінімальних елементів у стовпцях:");
            PrintMatrix(cost);

            Console.WriteLine("Пошук матриці оптимальних призначень:");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Викреслення всіх нулів:");
                Console.WriteLine();

                FindMin(cost, out bool[] rowCov, out bool[] colCov);

                Console.WriteLine("Матриця вартостей після викреслення рядків і стовпців з нулями:");
                PrintMatrixWithCover(cost, rowCov, colCov);

                int lines = 0;
                for (int i = 0; i < N; i++) if (rowCov[i]) lines++;
                for (int j = 0; j < N; j++) if (colCov[j]) lines++;

                Console.WriteLine($"Кількість призначень на роботу: {lines}, всього робіт: {N}");
                Console.WriteLine();

                if (lines >= N)
                {
                    Console.WriteLine("Матрицю оптимальних призначень знайдено!");
                    Console.WriteLine();
                    break;
                }

                Console.WriteLine("Матрицю оптимальних призначень не знайдено...");
                Console.WriteLine();

                int minVal = int.MaxValue;
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                        if (!rowCov[i] && !colCov[j] && cost[i, j] < minVal)
                            minVal = cost[i, j];

                Console.WriteLine($"Серед невикреслених елементів знайдено 'min': {minVal}");
                Console.WriteLine();

                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        if (!rowCov[i] && !colCov[j]) cost[i, j] -= minVal;
                        else if (rowCov[i] && colCov[j]) cost[i, j] += minVal;
                    }

                Console.WriteLine("Матриця вартостей після додавання/віднімання 'min' до/від відповідних елементів:");
                PrintMatrix(cost);
            }

            Console.WriteLine("Побудова матриці призначень:");
            Console.WriteLine();

            int[] finalAssignment = Assignment(cost);

            Console.WriteLine("Матриця вартостей, в якій відмічено призначення на роботу:");
            PrintMatrixWithAssignment(cost, finalAssignment);
            Console.WriteLine();

            int[,] assignMatrix = new int[N, N];
            for (int i = 0; i < N; i++) assignMatrix[i, finalAssignment[i]] = 1;

            Console.WriteLine("Матриця призначень:");
            PrintMatrix(assignMatrix);

            Console.WriteLine("Загальна вартість робіт:");
            Console.WriteLine();
            var terms = new List<string>();
            int total = 0;
            for (int i = 0; i < N; i++)
            {
                int v = originalCost[i, finalAssignment[i]];
                terms.Add(v.ToString());
                total += v;
            }
            Console.WriteLine($"S = {string.Join(" + ", terms)} = {total}");

            Console.WriteLine();
            Console.Write("Натисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
        static void FindMin(int[,] m, out bool[] rowCov, out bool[] colCov)
        {
            int[] rowMatch = new int[N];
            int[] colMatch = new int[N];
            for (int i = 0; i < N; i++) rowMatch[i] = -1;
            for (int j = 0; j < N; j++) colMatch[j] = -1;

            for (int i = 0; i < N; i++)
            {
                bool[] vis = new bool[N];
                TryAugment(m, i, rowMatch, colMatch, vis);
            }

            bool[] markedRow = new bool[N];
            bool[] markedCol = new bool[N];
            for (int i = 0; i < N; i++) if (rowMatch[i] == -1) markedRow[i] = true;

            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < N; i++)
                {
                    if (!markedRow[i]) continue;
                    for (int j = 0; j < N; j++)
                        if (m[i, j] == 0 && !markedCol[j])
                        { markedCol[j] = true; changed = true; }
                }
                for (int j = 0; j < N; j++)
                {
                    if (!markedCol[j]) continue;
                    int r = colMatch[j];
                    if (r != -1 && !markedRow[r])
                    { markedRow[r] = true; changed = true; }
                }
            }

            rowCov = new bool[N];
            colCov = new bool[N];
            for (int i = 0; i < N; i++) rowCov[i] = !markedRow[i];
            for (int j = 0; j < N; j++) colCov[j] = markedCol[j];
        }

        static bool TryAugment(int[,] m, int row, int[] rowMatch, int[] colMatch, bool[] vis)
        {
            for (int j = 0; j < N; j++)
            {
                if (m[row, j] == 0 && !vis[j])
                {
                    vis[j] = true;
                    if (colMatch[j] == -1 || TryAugment(m, colMatch[j], rowMatch, colMatch, vis))
                    {
                        rowMatch[row] = j;
                        colMatch[j] = row;
                        return true;
                    }
                }
            }
            return false;
        }

        static int[] Assignment(int[,] m)
        {
            int[] rowMatch = new int[N];
            int[] colMatch = new int[N];
            for (int i = 0; i < N; i++) rowMatch[i] = -1;
            for (int j = 0; j < N; j++) colMatch[j] = -1;

            bool[] rowDone = new bool[N];
            bool[] colDone = new bool[N];

            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < N; i++)
                {
                    if (rowDone[i]) continue;
                    int cnt = 0, lastJ = -1;
                    for (int j = 0; j < N; j++)
                        if (!colDone[j] && m[i, j] == 0) { cnt++; lastJ = j; }
                    if (cnt == 1)
                    {
                        rowMatch[i] = lastJ; colMatch[lastJ] = i;
                        rowDone[i] = colDone[lastJ] = true;
                        changed = true;
                    }
                }
                for (int j = 0; j < N; j++)
                {
                    if (colDone[j]) continue;
                    int cnt = 0, lastI = -1;
                    for (int i = 0; i < N; i++)
                        if (!rowDone[i] && m[i, j] == 0) { cnt++; lastI = i; }
                    if (cnt == 1)
                    {
                        rowMatch[lastI] = j; colMatch[j] = lastI;
                        rowDone[lastI] = colDone[j] = true;
                        changed = true;
                    }
                }
            }

            for (int i = 0; i < N; i++)
                if (rowMatch[i] == -1)
                {
                    bool[] vis = new bool[N];
                    TryAugment(m, i, rowMatch, colMatch, vis);
                }

            return rowMatch;
        }

        static int Width(int[,] m)
        {
            int w = 2;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (m[i, j].ToString().Length > w) w = m[i, j].ToString().Length;
            return w;
        }

        static void PrintMatrix(int[,] m)
        {
            int w = Width(m);
            for (int i = 0; i < N; i++)
            {
                Console.Write("  ");
                for (int j = 0; j < N; j++)
                    Console.Write(m[i, j].ToString().PadLeft(w + 1));
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void PrintMatrixWithCover(int[,] m, bool[] rowCov, bool[] colCov)
        {
            int w = 3;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (m[i, j].ToString().Length > w) w = m[i, j].ToString().Length;

            for (int i = 0; i < N; i++)
            {
                Console.Write("  ");
                for (int j = 0; j < N; j++)
                {
                    string cell;
                    if (rowCov[i] && colCov[j]) cell = "+";
                    else if (!rowCov[i] && colCov[j]) cell = "|";
                    else if (rowCov[i] && !colCov[j]) cell = m[i, j].ToString();
                    else cell = m[i, j] == 0 ? "[0]" : m[i, j].ToString();

                    Console.Write(cell.PadLeft(w + 1));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void PrintMatrixWithAssignment(int[,] m, int[] assignment)
        {
            int w = 3;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    string s = assignment[i] == j ? $"[{m[i, j]}]" : m[i, j].ToString();
                    if (s.Length > w) w = s.Length;
                }

            for (int i = 0; i < N; i++)
            {
                Console.Write("  ");
                for (int j = 0; j < N; j++)
                {
                    string cell = assignment[i] == j ? $"[{m[i, j]}]" : m[i, j].ToString();
                    Console.Write(cell.PadLeft(w + 1));
                }
                Console.WriteLine();
            }
        }
    }
}