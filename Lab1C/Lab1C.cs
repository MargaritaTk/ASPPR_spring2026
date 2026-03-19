using System.Text;

namespace SimplexMJV
{
    class Lab1C
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            int n = ReadInt("Введіть кількість змінних: ");
            int m = ReadInt("Введіть кількість обмежень: ");

            double[,] table = new double[m + 1, n + 1];
            string[] rowHeaders = new string[m + 1];
            string[] colHeaders = new string[n + 1];

            for (int j = 0; j < n; j++) colHeaders[j] = $"-x{j + 1}";
            colHeaders[n] = "1";

            for (int i = 0; i < m; i++) rowHeaders[i] = $"y{i + 1}";
            rowHeaders[m] = "Z";

            Console.WriteLine($"\nУвага! Знак однієї із нерівностей системи обмежень необхідно змінити на протилежний (номер нерівності вказує викладач)!");
            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nНалаштування обмеження №{i + 1}:");
                for (int j = 0; j < n; j++)
                    table[i, j] = ReadDouble($"  Коефіцієнт при x{j + 1}: ");

                table[i, n] = ReadDouble("  Вільний член b: ");

                Console.WriteLine("  Тип: 1: '<=', 2: '>=', 3: '='");
                int type = ReadInt("  Ваш вибір: ", 1, 3);

                if (type == 2)
                {
                    for (int j = 0; j <= n; j++) table[i, j] *= -1;
                }
                else if (type == 3)
                {
                    rowHeaders[i] = $"0_{i + 1}";
                    if (table[i, n] < 0)
                    {
                        for (int j = 0; j <= n; j++) table[i, j] *= -1;
                    }
                }
            }

            Console.WriteLine("\nКоефіцієнти цільової функції Z (мінімізація):");
            for (int j = 0; j < n; j++)
                table[m, j] = ReadDouble($"  Коефіцієнт Z при x{j + 1}: ");

            SimplexSolver solver = new SimplexSolver(table, rowHeaders, colHeaders);
            solver.Solve();

            Console.WriteLine("\nРоботу завершено. Натисніть Enter...");
            Console.ReadLine();
        }

        static double ReadDouble(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                if (double.TryParse(Console.ReadLine(), out double res)) return res;
                Console.WriteLine("Помилка: введіть число.");
            }
        }

        static int ReadInt(string msg, int min = 1, int max = 100)
        {
            while (true)
            {
                Console.Write(msg);
                if (int.TryParse(Console.ReadLine(), out int res) && res >= min && res <= max) return res;
                Console.WriteLine($"Введіть ціле число від {min} до {max}.");
            }
        }
    }

    class SimplexSolver
    {
        double[,] a;
        List<string> rows, cols;
        int m, n;

        public SimplexSolver(double[,] table, string[] r, string[] c)
        {
            a = table;
            rows = r.ToList();
            cols = c.ToList();
            m = a.GetLength(0) - 1;
            n = a.GetLength(1) - 1;
        }

        public void Solve()
        {
            PrintTable("ПОЧАТКОВА ТАБЛИЦЯ");
            ProcessZeroRows();

            if (FindBasicSolution())
            {
                Console.WriteLine("\nЗнайдено опорний розв'язок:");
                PrintVectors();

                Optimize();

                Console.WriteLine("\nЗнайдено оптимальний розв'язок:");
                PrintVectors();
                FinalResult();
            }
        }

        private void PrintVectors()
        {
            int maxX = 0;
            int maxY = 0;
            var all = rows.Concat(cols).Select(s => s.Replace("-", ""));
            foreach (var s in all)
            {
                if (s.StartsWith("x")) maxX = Math.Max(maxX, int.Parse(s.Substring(1)));
                if (s.StartsWith("y")) maxY = Math.Max(maxY, int.Parse(s.Substring(1)));
            }

            double[] xValues = new double[maxX];
            double[] yValues = new double[maxY];

            for (int i = 0; i < m; i++)
            {
                string name = rows[i].Replace("-", "");
                if (name.StartsWith("x")) xValues[int.Parse(name.Substring(1)) - 1] = a[i, n];
                if (name.StartsWith("y")) yValues[int.Parse(name.Substring(1)) - 1] = a[i, n];
            }

            Console.WriteLine($"X = ({string.Join("; ", xValues.Select(v => Math.Round(v, 2)))})");
            Console.WriteLine($"Y = ({string.Join("; ", yValues.Select(v => Math.Round(v, 2)))})");
        }

        private void ProcessZeroRows()
        {
            for (int i = 0; i < m; i++)
            {
                if (rows[i].StartsWith("0_"))
                {
                    int s = -1;
                    for (int j = 0; j < n; j++)
                    {
                        if (Math.Abs(a[i, j]) > 1e-9) { s = j; break; }
                    }

                    if (s != -1)
                    {
                        Console.WriteLine($"Розв'язувальний рядок: {rows[i]}");
                        Console.WriteLine($"Розв'язувальний стовпець: {cols[s]}");
                        ExecuteMJV(i, s);
                        DeleteColumn(s);
                        PrintTable($"Після видалення 0-стовпця (виключено {rows[i]})");
                    }
                }
            }
        }

        private bool FindBasicSolution()
        {
            Console.WriteLine("\nЕТАП 1: Пошук опорного розв'язку");
            while (true)
            {
                int r = -1;
                for (int i = 0; i < m; i++)
                    if (a[i, n] < -1e-9) { r = i; break; }

                if (r == -1) return true;

                int s = -1;
                for (int j = 0; j < n; j++)
                    if (a[r, j] < -1e-9) { s = j; break; }

                if (s == -1)
                {
                    Console.WriteLine("Система обмежень несумісна.");
                    return false;
                }

                Console.WriteLine($"\nРозв'язувальний рядок: {rows[r]}");
                Console.WriteLine($"Розв'язувальний стовпець: {cols[s]}");
                ExecuteMJV(r, s);
                PrintTable("Крок МЖВ (Етап 1)");
            }
        }

        private void Optimize()
        {
            Console.WriteLine("\nЕТАП 2: Пошук оптимального розв'язку");
            while (true)
            {
                int s = -1;
                for (int j = 0; j < n; j++)
                    if (a[m, j] < -1e-9) { s = j; break; }

                if (s == -1) break;

                int r = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    if (a[i, s] > 1e-9)
                    {
                        double ratio = a[i, n] / a[i, s];
                        if (ratio < minRatio) { minRatio = ratio; r = i; }
                    }
                }

                if (r == -1)
                {
                    Console.WriteLine("Функція не обмежена.");
                    return;
                }

                Console.WriteLine($"\nРозв'язувальний рядок: {rows[r]}");
                Console.WriteLine($"Розв'язувальний стовпець: {cols[s]}");
                ExecuteMJV(r, s);
                PrintTable("Крок МЖВ (Етап 2)");
            }
        }

        private void ExecuteMJV(int r, int s)
        {
            double ars = a[r, s];
            double[,] next = new double[m + 1, n + 1];

            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == r && j == s) next[i, j] = 1 / ars;
                    else if (i == r) next[i, j] = a[r, j] / ars;
                    else if (j == s) next[i, j] = -a[i, s] / ars;
                    else next[i, j] = (a[i, j] * ars - a[i, s] * a[r, j]) / ars;
                }
            }

            string rowTag = rows[r].Replace("-", "");
            string colTag = cols[s].Replace("-", "");
            rows[r] = colTag;
            cols[s] = "-" + rowTag;
            a = next;
        }

        private void DeleteColumn(int colIdx)
        {
            double[,] next = new double[m + 1, n];
            for (int i = 0; i <= m; i++)
            {
                int newJ = 0;
                for (int j = 0; j <= n; j++)
                {
                    if (j == colIdx) continue;
                    next[i, newJ++] = a[i, j];
                }
            }
            cols.RemoveAt(colIdx);
            a = next;
            n--;
        }

        private void PrintTable(string title)
        {
            Console.WriteLine($"\n{title}:");
            Console.Write("\t");
            for (int j = 0; j <= n; j++) Console.Write($"{cols[j]}\t");
            Console.WriteLine("\n" + new string('-', (n + 2) * 8));

            for (int i = 0; i <= m; i++)
            {
                Console.Write($"{rows[i]}\t");
                for (int j = 0; j <= n; j++) Console.Write($"{Math.Round(a[i, j], 2)}\t");
                Console.WriteLine();
            }
        }

        private void FinalResult()
        {
            Console.WriteLine($"\nZ_min = {Math.Round(-a[m, n], 4)}");
        }
    }
}