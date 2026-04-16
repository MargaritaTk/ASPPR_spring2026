using System.Globalization;
using System.Text;

namespace MatrixGame
{
    class Lab3
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            int m, n;
            while (true)
            {
                m = ReadInt("Введіть m: ");
                n = ReadInt("Введіть n: ");
                if (m == 3 && n == 3) break;
                Console.WriteLine("Дана робота приймає тільки матрицю 3x3.");
            }

            static int ReadInt(string message)
            {
                int result;
                while (true)
                {
                    Console.Write(message);
                    if (int.TryParse(Console.ReadLine(), out result) && result > 0)
                        return result;
                    Console.WriteLine("Помилка! Введіть ціле число більше 0.");
                }
            }

            double[,] A = new double[m, n];
            Console.WriteLine($"Введіть матрицю А {m}x{n} (через пробіл):");

            for (int i = 0; i < m; i++)
            {
                bool validRow = false;
                while (!validRow)
                {
                    Console.Write($"Рядок {i + 1}: ");
                    var input = Console.ReadLine().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (input.Length < n)
                    {
                        Console.WriteLine($"Помилка! Потрібно ввести {n} число/числа/чисел.");
                        continue;
                    }

                    try
                    {
                        for (int j = 0; j < n; j++)
                        {
                            if (!double.TryParse(input[j].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out A[i, j]))
                            {
                                throw new Exception($"'{input[j]}' не є числом.");
                            }
                        }
                        validRow = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Помилка у рядку: {ex.Message} Спробуйте ще раз.");
                    }
                }
            }

            Console.WriteLine("\nЗгенерований протокол обчислення:");
            Console.WriteLine("\nМатриця А:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) Console.Write($"{A[i, j],5} ");
                Console.WriteLine();
            }

            double[] alpha_i = new double[m];
            for (int i = 0; i < m; i++)
            {
                double min = A[i, 0];
                for (int j = 1; j < n; j++) if (A[i, j] < min) min = A[i, j];
                alpha_i[i] = min;
            }
            double alpha = alpha_i.Max();

            double[] beta_j = new double[n];
            for (int j = 0; j < n; j++)
            {
                double max = A[0, j];
                for (int i = 1; i < m; i++) if (A[i, j] > max) max = A[i, j];
                beta_j[j] = max;
            }
            double beta = beta_j.Min();

            Console.WriteLine("\nПошук сідлової точки:");
            Console.WriteLine($"Знайдено нижню ціну гри: {alpha}");
            Console.WriteLine($"Знайдено верхню ціну гри: {beta}");
            if (alpha == beta) Console.WriteLine("Сідлову точку знайдено.");
            else Console.WriteLine("Сідлову точку не знайдено...");

            Console.WriteLine("\nРозв’язання матричної гри симплекс-методом...");

            double L = (A.Cast<double>().Min() <= 0) ? Math.Abs(A.Cast<double>().Min()) + 1 : 0;

            Console.WriteLine("\nПостановка прямої задачі:");
            Console.WriteLine("Z = " + string.Join(" + ", Enumerable.Range(1, n).Select(i => $"q{i}")) + " -> max");
            Console.WriteLine("при обмеженнях:");
            for (int i = 0; i < m; i++)
            {
                string str = "";
                for (int j = 0; j < n; j++) str += $"{(A[i, j] + L):F2} * q{j + 1} " + (j == n - 1 ? "" : "+ ");
                Console.WriteLine($"{str} <= 1");
            }
            Console.WriteLine(string.Join(", ", Enumerable.Range(1, n).Select(i => $"q{i}")) + " >= 0");

            Console.WriteLine("\nПостановка двоїстої задачі:");
            Console.WriteLine("W = " + string.Join(" + ", Enumerable.Range(1, m).Select(i => $"p{i}")) + " -> min");
            Console.WriteLine("при обмеженнях:");
            for (int j = 0; j < n; j++)
            {
                string str = "";
                for (int i = 0; i < m; i++) str += $"{(A[i, j] + L):F2} * p{i + 1} " + (i == m - 1 ? "" : "+ ");
                Console.WriteLine($"{str} >= 1");
            }
            Console.WriteLine(string.Join(", ", Enumerable.Range(1, m).Select(i => $"p{i}")) + " >= 0");

            double[,] table = new double[m + 1, n + 1];
            string[] rLabels = new string[m + 1]; string[] cLabels = new string[n + 1];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++) table[i, j] = A[i, j] + L;
                table[i, n] = 1; rLabels[i] = $"p{i + 1}";
            }
            for (int j = 0; j < n; j++) { table[m, j] = -1; cLabels[j] = $"-q{j + 1}"; }
            cLabels[n] = "1"; rLabels[m] = "Z";

            SimplexSolver solver = new SimplexSolver(table, rLabels, cLabels, m, n);
            var res = solver.Solve();

            Console.WriteLine("\nЗнайдено оптимальні рішення двоїстих задач!");
            Console.WriteLine($"Перший гравець: p: {string.Join("; ", res.P_vals.Select(v => v.ToString("F2")))}");
            Console.WriteLine($"Другий гравець: q: {string.Join("; ", res.Q_vals.Select(v => v.ToString("F2")))}");

            double V = (1.0 / res.Z_val) - L;
            Console.WriteLine($"\nЦіна гри: {V:F2}");

            Console.WriteLine("\nРозрахунок змішаних стратегій...");
            Console.WriteLine("\nСтратегії 1-го гравця:");
            Console.WriteLine(string.Join("; ", res.P_mixed.Select(v => v.ToString("F2"))));

            Console.WriteLine("\nСтратегії 2-го гравця:");
            Console.WriteLine(string.Join("; ", res.Q_mixed.Select(v => v.ToString("F2"))));

            Console.WriteLine($"\nОстаточна ціна гри: {V:F2}");

            Console.WriteLine("\nРезультати моделювання матричної гри:");
            Console.WriteLine("----------------------------------------------------------------------------------");
            Console.WriteLine(" № | Rnd A | Str A | Rnd B | Str B | Виграш | Накопич. вигр. | Сер. виграш ");
            Console.WriteLine("----------------------------------------------------------------------------------");

            Random rnd = new Random();
            double accWin = 0; int[] freqA = new int[m], freqB = new int[n];
            for (int k = 1; k <= 50; k++)
            {
                double rA = rnd.NextDouble(), rB = rnd.NextDouble();
                int sA = Pick(rA, res.P_mixed), sB = Pick(rB, res.Q_mixed);
                double win = A[sA, sB];
                accWin += win; freqA[sA]++; freqB[sB]++;
                Console.WriteLine($"{k,2} | {rA:F3} | {sA + 1,5} | {rB:F3} | {sB + 1,5} | {win,6:F1} | {accWin,14:F1} | {accWin / k,11:F4}");
            }

            Console.WriteLine();
            Console.WriteLine("ПІДСУМКОВЕ ПОРІВНЯННЯ СТРАТЕГІЙ");
            Console.WriteLine(new string('-', 60));
            Console.WriteLine("{0,-15} | {1,-15} | {2,-15}", "Стратегія", "Теоретична", "Експериментальна");
            Console.WriteLine(new string('-', 60));

            for (int i = 0; i < m; i++)
            {
                double p_teor = res.P_mixed[i];
                double p_exp = (double)freqA[i] / 50.0;

                Console.WriteLine("A{0,-14} | {1,-15:F3} | {2,-15:F3}", i + 1, p_teor, p_exp);
            }

            Console.WriteLine(new string('-', 60));
            Console.WriteLine($"Ціна гри: V_теор = {V:F3} | V_експ = {accWin / 50.0:F3}");

        }

        static int Pick(double r, double[] p)
        {
            double s = 0;
            for (int i = 0; i < p.Length; i++) { s += p[i]; if (r <= s) return i; }
            return p.Length - 1;
        }
    }

    class SimplexSolver
    {
        double[,] a; string[] rL, cL; int M, N;
        public SimplexSolver(double[,] t, string[] r, string[] c, int m, int n) { a = t; rL = r; cL = c; M = m; N = n; }

        public (double[] P_vals, double[] Q_vals, double[] P_mixed, double[] Q_mixed, double Z_val) Solve()
        {
            PrintTable("Складено таку симплекс-таблицю");
            while (true)
            {
                int r = -1, s = -1;
                int rows = a.GetLength(0) - 1, cols = a.GetLength(1) - 1;
                for (int j = 0; j < cols; j++) if (a[rows, j] < -1e-9) { s = j; break; }
                if (s == -1) break;
                double minRatio = 1e18;
                for (int i = 0; i < rows; i++) if (a[i, s] > 1e-9 && a[i, cols] / a[i, s] < minRatio) { minRatio = a[i, cols] / a[i, s]; r = i; }
                if (r == -1) break;

                double ars = a[r, s];
                double[,] nxt = new double[rows + 1, cols + 1];
                for (int i = 0; i <= rows; i++)
                    for (int j = 0; j <= cols; j++)
                    {
                        if (i == r && j == s) nxt[i, j] = 1 / ars;
                        else if (i == r) nxt[i, j] = a[r, j] / ars;
                        else if (j == s) nxt[i, j] = -a[i, s] / ars;
                        else nxt[i, j] = (a[i, j] * ars - a[i, s] * a[r, j]) / ars;
                    }
                string tmp = rL[r]; rL[r] = cL[s].Replace("-", ""); cL[s] = "-" + tmp;
                a = nxt;
            }
            PrintTable("Остаточна симплекс-таблиця");

            double Z = a[M, N];
            double[] p_v = new double[M], q_v = new double[N];
            for (int i = 0; i < M; i++)
            {
                for (int k = 0; k < N; k++) if (cL[k] == $"-p{i + 1}") p_v[i] = a[M, k];
                for (int k = 0; k < M; k++) if (rL[k] == $"q{i + 1}") q_v[i] = a[k, N];
            }
            return (p_v, q_v, p_v.Select(x => x / Z).ToArray(), q_v.Select(x => x / Z).ToArray(), Z);
        }

        void PrintTable(string title)
        {
            Console.WriteLine($"\n{title}:");
            Console.Write("            ");
            for (int j = 0; j < a.GetLength(1); j++) Console.Write($"{cL[j],10} ");
            Console.WriteLine("\n-----------" + new string('-', a.GetLength(1) * 11));
            for (int i = 0; i < a.GetLength(0); i++)
            {
                Console.Write($"{rL[i],5} {(i == M ? "Z" : "r" + (i + 1)),2} = ");
                for (int j = 0; j < a.GetLength(1); j++) Console.Write($"{a[i, j],10:F2} ");
                Console.WriteLine();
            }
        }
    }
}