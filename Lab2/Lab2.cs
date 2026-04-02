using System.Text;

namespace SimplexMJV
{
    class Lab2
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

            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nНалаштування обмеження №{i + 1}:");
                for (int j = 0; j < n; j++)
                    table[i, j] = ReadDouble($"  Коефіцієнт при x{j + 1}: ");

                table[i, n] = ReadDouble("  Вільний член b: ");

                Console.WriteLine("  Тип: 1: '<=', 2: '>=', 3: '='");
                int type = ReadInt("  Ваш вибір: ", 1, 3);

                if (type == 2 || type == 3)
                    for (int j = 0; j <= n; j++) table[i, j] *= -1;
            }

            Console.WriteLine("\nКоефіцієнти цільової функції Z:");
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
        string[] rows, cols;
        int m, n;
        int initialN;

        public SimplexSolver(double[,] initialTable, string[] r, string[] c)
        {
            a = initialTable;
            rows = r;
            cols = c;
            m = a.GetLength(0) - 1;
            n = a.GetLength(1) - 1;
            initialN = n;
        }

        public void Solve()
        {
            Console.WriteLine("\nПОЧАТОК ПРОТОКОЛУ ОБЧИСЛЕНЬ МЖВ");
            PrintTable("ПОЧАТКОВА ТАБЛИЦЯ");

            int step = 1;
            while (true)
            {
                int r = -1;
                for (int i = 0; i < m; i++)
                    if (a[i, n] < -0.000001) { r = i; break; }

                if (r == -1) break;

                int s = -1;
                for (int j = 0; j < n; j++)
                    if (a[r, j] < -0.000001) { s = j; break; }

                if (s == -1) { Console.WriteLine("\nСистема обмежень несумісна."); return; }

                Console.WriteLine($"\nКРОК {step++} (Етап 1): Опорний розв'язок. РЕ: [{rows[r]}, {cols[s]}] = {Math.Round(a[r, s], 2)}");
                ExecuteMJV(r, s);
                PrintTable("ПОТОЧНА ТАБЛИЦЯ");
            }

            Console.WriteLine("\nОПОРНИЙ РОЗВ'ЯЗОК ЗНАЙДЕНО");
            PrintCurrentSolution("Пряма задача (опорний розв'язок):");
            PrintDualSolution("Двоїста задача (опорний розв'язок):");

            while (true)
            {
                int s = -1;
                for (int j = 0; j < n; j++)
                    if (a[m, j] < -0.000001) { s = j; break; }

                if (s == -1) break;

                int r = -1;
                double minRatio = double.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    if (a[i, s] > 0.000001)
                    {
                        double ratio = a[i, n] / a[i, s];
                        if (ratio < minRatio) { minRatio = ratio; r = i; }
                    }
                }

                if (r == -1) { Console.WriteLine("\nФункція не обмежена."); return; }

                Console.WriteLine($"\nКРОК {step++} (Етап 2): Оптимізація. РЕ: [{rows[r]}, {cols[s]}] = {Math.Round(a[r, s], 2)}");
                ExecuteMJV(r, s);
                PrintTable("ПОТОЧНА ТАБЛИЦЯ");
            }

            Console.WriteLine("\nОПТИМАЛЬНИЙ РОЗВ'ЯЗОК ЗНАЙДЕНО");
            PrintCurrentSolution("Пряма задача (оптимальний розв'язок):");
            PrintDualSolution("Двоїста задача (оптимальний розв'язок):");
        }

        private void PrintCurrentSolution(string title)
        {
            Console.WriteLine($"\n{title}");
            double[] resX = new double[initialN + 1];
            for (int i = 0; i < m; i++)
            {
                if (rows[i].Contains("x"))
                {
                    string idPart = rows[i].Replace("x", "").Replace("-", "");
                    if (int.TryParse(idPart, out int idx) && idx <= initialN) resX[idx] = a[i, n];
                }
            }
            for (int i = 1; i <= initialN; i++)
                Console.WriteLine($" x{i} = {Math.Round(resX[i], 2)}");
            Console.WriteLine($" Z = {Math.Round(-a[m, n], 2)}");
        }

        private void PrintDualSolution(string title)
        {
            Console.WriteLine($"\n{title}");
            double[] resY = new double[m + 1];
            for (int j = 0; j < n; j++)
            {
                string colTag = cols[j].Replace("-", "");
                if (colTag.StartsWith("y"))
                {
                    string idPart = colTag.Substring(1);
                    if (int.TryParse(idPart, out int idx) && idx <= m) resY[idx] = a[m, j];
                }
            }
            for (int i = 1; i <= m; i++)
                Console.WriteLine($" y{i} = {Math.Round(resY[i], 2)}");
            Console.WriteLine($" W = {Math.Round(-a[m, n], 2)}");
        }

        private void ExecuteMJV(int r, int s)
        {
            double ars = a[r, s];
            double[,] nextA = new double[m + 1, n + 1];
            for (int i = 0; i <= m; i++)
                for (int j = 0; j <= n; j++)
                {
                    if (i == r && j == s) nextA[i, j] = 1 / ars;
                    else if (i == r) nextA[i, j] = a[r, j] / ars;
                    else if (j == s) nextA[i, j] = -a[i, s] / ars;
                    else nextA[i, j] = (a[i, j] * ars - a[i, s] * a[r, j]) / ars;
                }
            string rName = rows[r];
            string cName = cols[s];
            rows[r] = cName.Replace("-", "");
            cols[s] = "-" + rName.Replace("-", "");
            a = nextA;
        }

        private void PrintTable(string title)
        {
            Console.WriteLine($"\n{title}");
            Console.Write("\t");
            for (int j = 0; j <= n; j++) Console.Write($"{cols[j]}\t");
            Console.WriteLine("\n\t" + new string('-', (n + 1) * 10));
            for (int i = 0; i <= m; i++)
            {
                Console.Write($"{rows[i]}\t");
                for (int j = 0; j <= n; j++) Console.Write($"{Math.Round(a[i, j], 2)}\t");
                Console.WriteLine();
            }
        }
    }
}