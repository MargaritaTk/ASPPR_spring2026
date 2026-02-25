using System;
using System.Text;

namespace SimplexMJV
{
    class Lab1B
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Увага! У ДРУГІЙ нерівності системи обмежень слід змінити знак нерівності на протилежний!");
            int n, m;
            while (true)
            {
                Console.Write("\nВведіть кількість змінних: ");
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

            for (int j = 0; j < n; j++) colHeaders[j] = $"x{j + 1}";
            colHeaders[n] = "b";
            for (int i = 0; i < m; i++) rowHeaders[i] = $"s{i + 1}";
            rowHeaders[m] = "Z";

            for (int i = 0; i < m; i++)
            {
                Console.WriteLine($"\nНалаштування обмеження №{i + 1}");
                for (int j = 0; j < n; j++)
                {
                    bool ok = false;
                    do
                    {
                        Console.Write($"  Коефіцієнт при x{j + 1}: ");
                        string input = Console.ReadLine();
                        if (double.TryParse(input, out table[i, j])) ok = true;
                        else Console.WriteLine("Помилка: введіть число.");
                    } while (!ok);
                }

                bool bOk = false;
                do
                {
                    Console.Write("Вільний член b (число після знаку): ");
                    string input = Console.ReadLine();
                    if (double.TryParse(input, out table[i, n])) bOk = true;
                    else Console.WriteLine("Помилка: введіть число.");
                } while (!bOk);

                int sign = 0;
                do
                {
                    Console.Write("  Оберіть знак (1 для '<=', 2 для '>='): ");
                    string input = Console.ReadLine();
                    if (int.TryParse(input, out sign) && (sign == 1 || sign == 2)) break;
                    Console.WriteLine("Помилка: введіть 1 або 2.");
                } while (true);
            }

            Console.WriteLine("\nКоефіцієнти цільової функції Z");
            for (int j = 0; j < n; j++)
            {
                bool ok = false;
                do
                {
                    Console.Write($"  Коефіцієнт Z при x{j + 1}: ");
                    string input = Console.ReadLine();
                    if (double.TryParse(input, out table[m, j])) ok = true;
                    else Console.WriteLine("Помилка: введіть число.");
                } while (!ok);
            }

            SimplexSolver solver = new SimplexSolver(table, rowHeaders, colHeaders);
            solver.Solve();

            Console.WriteLine("\nНатисніть Enter, щоб завершити...");
            Console.ReadLine();
        }
    }

    class SimplexSolver
    {
        double[,] a;
        string[] rows, cols;
        int m, n;

        public SimplexSolver(double[,] initialTable, string[] r, string[] c)
        {
            a = initialTable;
            rows = r;
            cols = c;
            m = a.GetLength(0) - 1;
            n = a.GetLength(1) - 1;
        }

        public void Solve()
        {
            Console.WriteLine("\nПРОТОКОЛ МЖВ:");
            PrintTable("ПОЧАТКОВА ТАБЛИЦЯ");

            while (true)
            {
                int r = -1;
                for (int i = 0; i < m; i++) if (a[i, n] < -0.000001) { r = i; break; }
                if (r == -1) break;

                int s = -1;
                for (int j = 0; j < n; j++) if (a[r, j] < -0.000001) { s = j; break; }

                if (s == -1) { Console.WriteLine("\nПомилка: Система обмежень несумісна."); return; }

                Console.WriteLine($"\nЕТАП 1: Пошук опорного. Розв'язувальний елемент: {rows[r]}/{cols[s]} = {Math.Round(a[r, s], 2)}");
                ExecuteMJV(r, s);
                PrintTable("ОНОВЛЕНА ТАБЛИЦЯ");
            }
            Console.WriteLine("\nОпорний розв'язок знайдено.");

            while (true)
            {
                int s = -1;
                for (int j = 0; j < n; j++) if (a[m, j] > 0.000001) { s = j; break; }
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

                if (r == -1) { Console.WriteLine("\nПомилка: Функція не обмежена."); return; }

                Console.WriteLine($"\nЕТАП 2: Оптимізація. Розв'язувальний елемент: {rows[r]}/{cols[s]} = {Math.Round(a[r, s], 2)}");
                ExecuteMJV(r, s);
                PrintTable("ОНОВЛЕНА ТАБЛИЦЯ");
            }

            FinalResult();
        }

        private void ExecuteMJV(int r, int s)
        {
            double ars = a[r, s];
            double[,] nextA = new double[m + 1, n + 1];
            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == r && j == s) nextA[i, j] = 1 / ars;
                    else if (i == r) nextA[i, j] = a[r, j] / ars;
                    else if (j == s) nextA[i, j] = -a[i, s] / ars;
                    else nextA[i, j] = (a[i, j] * ars - a[i, s] * a[r, j]) / ars;
                }
            }
            string temp = rows[r]; rows[r] = cols[s]; cols[s] = temp;
            a = nextA;
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
                for (int j = 0; j <= n; j++) Console.Write($"{Math.Round(a[i, j], 2)}\t");
                Console.WriteLine();
            }
        }

        private void FinalResult()
        {
            Console.WriteLine("РЕЗУЛЬТАТ:");
            double[] resX = new double[n + 1];
            for (int i = 0; i < m; i++)
                if (rows[i].StartsWith("x"))
                {
                    int idx = int.Parse(rows[i].Substring(1));
                    if (idx <= n) resX[idx] = a[i, n];
                }

            for (int i = 1; i <= n; i++) Console.WriteLine($" x{i} = {Math.Round(resX[i], 2)}");
            Console.WriteLine($" Z_min = {Math.Round(-a[m, n], 2)}");
        }
    }
}