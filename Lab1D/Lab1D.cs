using System.Text;

namespace SimplexMJV
{
    class Lab1D
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            int n, m;
            while (true)
            {
                Console.Write("Введіть кількість змінних: ");
                if (int.TryParse(Console.ReadLine(), out n) && n > 0) break;
                Console.WriteLine("Помилка! Введіть ціле число більше 0.");
            }

            while (true)
            {
                Console.Write("Введіть кількість обмежень: ");
                if (int.TryParse(Console.ReadLine(), out m) && m > 0) break;
                Console.WriteLine("Помилка! Введіть ціле число більше 0.");
            }

            double[,] table = new double[m + 1, n + 1];
            string[] rowHeaders = new string[m + 1];
            string[] colHeaders = new string[n + 1];

            for (int j = 0; j < n; j++) colHeaders[j] = $"-x{j + 1}";
            colHeaders[n] = "1";
            for (int i = 0; i < m; i++) rowHeaders[i] = $"y{i + 1}";
            rowHeaders[m] = "Z";

            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nНалаштування обмеження №{i + 1}");
                for (int j = 0; j < n; j++)
                    table[i, j] = ReadDouble($"  Коефіцієнт при x{j + 1}: ");

                table[i, n] = ReadDouble("  Вільний член b: ");
                int sign;
                while (true)
                {
                    Console.Write(" Оберіть знак (1 для '<=', 2 для '>='): ");
                    string input = Console.ReadLine()?.Trim() ?? "";

                    if (int.TryParse(input, out sign) && (sign == 1 || sign == 2))
                    {
                        break; 
                    }

                    Console.WriteLine(" Помилка! Введіть тільки 1 або 2.");
                }
            }

            Console.WriteLine("\nКоефіцієнти цільової функції Z (для мінімізації)");
            for (int j = 0; j < n; j++)
                table[m, j] = ReadDouble($"  Коефіцієнт Z при x{j + 1}: ");

            SimplexSolver solver = new SimplexSolver(table, rowHeaders, colHeaders);
            solver.Solve();

            Console.WriteLine("\nНатисніть Enter, щоб завершити...");
            Console.ReadLine();
        }

        static double ReadDouble(string message)
        {
            while (true)
            {
                Console.Write(message);
                if (double.TryParse(Console.ReadLine(), out double res)) return res;
                Console.WriteLine("Помилка: введіть число.");
            }
        }
    }

    class SimplexSolver
    {
        double[,] a;
        string[] rows, cols;
        int m, n, originalN;
        int sCounter = 0;
        const double EPS = 0.000001;

        public SimplexSolver(double[,] initialTable, string[] r, string[] c)
        {
            a = initialTable;
            rows = r;
            cols = c;
            m = a.GetLength(0) - 1;
            n = a.GetLength(1) - 1;
            originalN = n;
        }

        public void Solve()
        {
            Console.WriteLine("\nПРОТОКОЛ ОБЧИСЛЕНЬ (МЕТОД ГОМОРІ)");
            PrintTable("ПОЧАТКОВА ТАБЛИЦЯ");

            while (true)
            {
                // 1. Пошук опорного розв'язку
                if (!SimplexStep(true)) { Console.WriteLine("\nПомилка: Система несумісна."); return; }
                PrintCurrentX("Знайдено опорний розв’язок");

                // 2. Оптимізація
                if (!SimplexStep(false)) { Console.WriteLine("\nПомилка: Функція не обмежена."); return; }
                PrintCurrentX("Знайдено оптимальний розв’язок");

                // 3. Перевірка на цілочисельність
                int targetRow = -1;
                double maxFrac = 0;
                for (int i = 0; i < m; i++)
                {
                    if (rows[i].Contains("x"))
                    {
                        double frac = a[i, n] - Math.Floor(a[i, n] + EPS);
                        if (frac > EPS && frac < (1 - EPS))
                        {
                            if (frac > maxFrac) { maxFrac = frac; targetRow = i; }
                        }
                    }
                }

                if (targetRow == -1)
                {
                    FinalResult();
                    break;
                }

                Console.WriteLine($"\nДробова частина у {rows[targetRow]}, додаємо відсікання s{sCounter + 1}");
                AddGomoryCut(targetRow);
            }
        }

        private bool SimplexStep(bool isPhase1)
        {
            while (true)
            {
                int r = -1, s = -1;
                if (isPhase1)
                {
                    for (int i = 0; i < m; i++) if (a[i, n] < -EPS) { r = i; break; }
                    if (r == -1) break;
                    for (int j = 0; j < n; j++) if (a[r, j] < -EPS) { s = j; break; }
                    if (s == -1) return false;
                }
                else
                {
                    for (int j = 0; j < n; j++) if (a[m, j] < -EPS) { s = j; break; }
                    if (s == -1) break;
                    double minRatio = double.MaxValue;
                    for (int i = 0; i < m; i++)
                    {
                        if (a[i, s] > EPS)
                        {
                            double ratio = a[i, n] / a[i, s];
                            if (ratio < minRatio) { minRatio = ratio; r = i; }
                        }
                    }
                    if (r == -1) return false;
                }

                Console.WriteLine($"\nКРОК МЖВ: РЕ {rows[r]}/{cols[s]} = {Math.Round(a[r, s], 2)}");
                ExecuteMJV(r, s);
                PrintTable("ПОТОЧНА ТАБЛИЦЯ");
            }
            return true;
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

            string rName = rows[r].Replace("-", "");
            string cName = cols[s].Replace("-", "");
            rows[r] = cName;
            cols[s] = "-" + rName;
            a = nextA;
        }

        private void AddGomoryCut(int r)
        {
            sCounter++;
            int newM = m + 1;
            double[,] next = new double[newM + 1, n + 1];
            string[] newRows = new string[newM + 1];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j <= n; j++) next[i, j] = a[i, j];
                newRows[i] = rows[i];
            }

            for (int j = 0; j <= n; j++)
                next[m, j] = -(a[r, j] - Math.Floor(a[r, j] + EPS));

            newRows[m] = $"s{sCounter}";
            for (int j = 0; j <= n; j++) next[newM, j] = a[m, j];
            newRows[newM] = "Z";

            a = next; rows = newRows; m = newM;
            PrintTable($"ТАБЛИЦЯ ПІСЛЯ ДОДАВАННЯ s{sCounter}");
        }

        private void PrintTable(string title)
        {
            Console.WriteLine($"\n{title}:");
            Console.Write("\t");
            for (int j = 0; j <= n; j++) Console.Write($"{cols[j]}\t");
            Console.WriteLine("\n\t" + new string('-', (n + 1) * 8));
            for (int i = 0; i <= m; i++)
            {
                Console.Write($"{rows[i]}\t");
                for (int j = 0; j <= n; j++) Console.Write($"{Math.Round(a[i, j], 2):F2}\t");
                Console.WriteLine();
            }
        }

        private void PrintCurrentX(string label)
        {
            Console.Write("X = (");
            List<string> vals = new List<string>();
            for (int i = 1; i <= originalN; i++)
            {
                double val = 0;
                for (int k = 0; k < m; k++) if (rows[k] == $"x{i}") val = a[k, n];
                vals.Add(Math.Round(val, 2).ToString("F2"));
            }
            Console.WriteLine($"{string.Join("; ", vals)})");
        }

        private void FinalResult()
        {
            Console.WriteLine("ОПТИМАЛЬНИЙ ЦІЛОЧИСЛОВИЙ РОЗВ'ЯЗОК:");
            PrintCurrentX("X");
            Console.WriteLine($"Z_min = {Math.Round(-a[m, n], 2):F2}");
        }
    }
}