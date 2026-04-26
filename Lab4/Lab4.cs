namespace GameWithNature
{
    class Program
    {
        static double ReadDouble(string message)
        {
            double result;
            Console.Write(message);
            while (!double.TryParse(Console.ReadLine().Replace('.', ','), out result))
            {
                Console.WriteLine("Помилка! Введіть коректне число.");
            }
            return result;
        }

        static int ReadInt(string message)
        {
            int result;
            Console.Write(message);
            while (!int.TryParse(Console.ReadLine(), out result) || result <= 0)
            {
                Console.WriteLine("Помилка! Введіть ціле позитивне число.");
            }
            return result;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                int rows = ReadInt("Введіть кількість рядків: ");
                int cols = ReadInt("Введіть кількість стовпців: ");

                double[,] U = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    bool rowCorrect = false;
                    while (!rowCorrect)
                    {
                        Console.Write($"Введіть ряд {i + 1} ({cols} числа через пробіл): ");
                        string inputStr = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(inputStr)) continue;

                        string[] rowInput = inputStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (rowInput.Length != cols)
                        {
                            Console.WriteLine($"Помилка! Потрібно ввести рівно {cols} чисел.");
                            continue;
                        }

                        try
                        {
                            for (int j = 0; j < cols; j++)
                                U[i, j] = double.Parse(rowInput[j].Replace('.', ','));
                            rowCorrect = true;
                        }
                        catch
                        {
                            Console.WriteLine("Помилка! У рядку знайдено літери або невірний формат.");
                        }
                    }
                }

                double y_coef = ReadDouble("\nВведіть коефіцієнт Y: ");

                Console.Write($"Введіть {cols} ймовірностей (через пробіл): ");
                double[] P;
                while (true)
                {
                    try
                    {
                        P = Console.ReadLine().Replace('.', ',')
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(double.Parse).ToArray();
                        if (P.Length == cols) break;
                        Console.Write($"Помилка! Введіть рівно {cols} чисел: ");
                    }
                    catch { Console.Write("Помилка формату! Спробуйте ще раз: "); }
                }
                Console.WriteLine("Згенерований протокол обчислення:");
                Console.WriteLine("\nМатриця корисності результатів U:");
                PrintMatrix(U);

                Console.WriteLine("\nКритерій Вальда:");
                double[] mins = new double[rows];
                for (int i = 0; i < rows; i++)
                {
                    mins[i] = GetRow(U, i).Min();
                    Console.WriteLine($"min в рядку {i + 1}: {mins[i]}");
                }
                double maxOfMins = mins.Max();
                Console.WriteLine($"\nМаксимальний елемент: {maxOfMins}");
                OptimalStrat(mins, maxOfMins);

                Console.WriteLine("\nКритерій максимаксу:");
                double[] maxs = new double[rows];
                for (int i = 0; i < rows; i++)
                {
                    maxs[i] = GetRow(U, i).Max();
                    Console.WriteLine($"max в рядку {i + 1}: {maxs[i]}");
                }
                double maxOfMaxs = maxs.Max();
                Console.WriteLine($"\nМаксимальний елемент: {maxOfMaxs}");
                OptimalStrat(maxs, maxOfMaxs);

                Console.WriteLine("\nКритерій Гурвіца:");
                Console.WriteLine($"Коефіцієнт y = {y_coef}\n");
                double[] hurwitz = new double[rows];
                for (int i = 0; i < rows; i++)
                {
                    hurwitz[i] = y_coef * mins[i] + (1 - y_coef) * maxs[i];
                    Console.WriteLine($"s{i + 1} = {y_coef} * {mins[i]} + (1 - {y_coef}) * {maxs[i]} = {Math.Round(hurwitz[i], 2)}");
                }
                double maxHur = hurwitz.Max();
                Console.WriteLine($"\nМаксимальний елемент: {Math.Round(maxHur, 2)}");
                OptimalStrat(hurwitz, maxHur);

                Console.WriteLine("\nКритерій Севіджа:");
                Console.WriteLine("\nМатриця ризиків:");
                double[,] R = new double[rows, cols];
                for (int j = 0; j < cols; j++)
                {
                    double colMax = GetColumn(U, j).Max();
                    for (int i = 0; i < rows; i++) R[i, j] = colMax - U[i, j];
                }
                PrintMatrix(R);
                double[] maxRisks = new double[rows];
                for (int i = 0; i < rows; i++)
                {
                    maxRisks[i] = GetRow(R, i).Max();
                    Console.WriteLine($"max в рядку {i + 1}: {maxRisks[i]}");
                }
                double minRisk = maxRisks.Min();
                Console.WriteLine($"\nМінімальний елемент: {minRisk}");
                OptimalStrat(maxRisks, minRisk, isRisk: true);

                Console.WriteLine("\nКритерій Байєса:");
                Console.WriteLine("Ймовірності: " + string.Join("; ", P.Select((p, idx) => $"p{idx + 1}={p}")));
                double[] bayes = new double[rows];
                for (int i = 0; i < rows; i++)
                {
                    string f = $"s{i + 1} = ";
                    for (int j = 0; j < cols; j++)
                    {
                        bayes[i] += U[i, j] * P[j];
                        f += $"{U[i, j]} * {P[j]}" + (j < cols - 1 ? " + " : "");
                    }
                    Console.WriteLine($"{f} = {Math.Round(bayes[i], 2)}");
                }
                double maxBay = bayes.Max();
                Console.WriteLine($"\nМаксимальний елемент: {Math.Round(maxBay, 2)}");
                OptimalStrat(bayes, maxBay);

                Console.WriteLine("\nКритерій Лапласа:");
                double pL = 1.0 / cols;
                double[] laplace = new double[rows];
                for (int i = 0; i < rows; i++)
                {
                    string f = $"s{i + 1} = ";
                    for (int j = 0; j < cols; j++)
                    {
                        laplace[i] += U[i, j] * pL;
                        f += $"{U[i, j]} * {Math.Round(pL, 2)}" + (j < cols - 1 ? " + " : "");
                    }
                    Console.WriteLine($"{f} = {Math.Round(laplace[i], 2)}");
                }
                double maxLap = laplace.Max();
                Console.WriteLine($"\nМаксимальний елемент: {Math.Round(maxLap, 2)}");
                OptimalStrat(laplace, maxLap);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Критична помилка: " + ex.Message);
            }

            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }

        static double[] GetRow(double[,] m, int r) => Enumerable.Range(0, m.GetLength(1)).Select(j => m[r, j]).ToArray();
        static double[] GetColumn(double[,] m, int c) => Enumerable.Range(0, m.GetLength(0)).Select(i => m[i, c]).ToArray();

        static void PrintMatrix(double[,] m)
        {
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                    Console.Write($"{m[i, j],6} ");
                Console.WriteLine();
            }
        }

        static void OptimalStrat(double[] arr, double target, bool isRisk = false)
        {
            var indices = Enumerable.Range(0, arr.Length)
                .Where(i => Math.Abs(arr[i] - target) < 0.001)
                .Select(i => $"A{i + 1}");
            Console.WriteLine($"Оптимальні стратегії: {string.Join(" або ", indices)}");
        }
    }
}