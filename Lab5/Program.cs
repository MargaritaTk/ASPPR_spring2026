using System.Text;

namespace TransportTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            int rows = ReadInt("Введіть кількість пунктів відправлення (рядків): ");
            int cols = ReadInt("Введіть кількість пунктів призначення (стовпців): ");

            int[,] sp = new int[rows, cols];
            int[] po = new int[rows];
            int[] pn = new int[cols];

            Console.WriteLine("\nВведіть матрицю вартостей перевезень SP:");
            for (int i = 0; i < rows; i++)
            {
                Console.WriteLine($"Рядок {i + 1} ({cols} значення):");
                int[] vals = ReadIntArray(cols);
                for (int j = 0; j < cols; j++) sp[i, j] = vals[j];
            }

            Console.WriteLine("\nВведіть вектор запасів PO:");
            po = ReadIntArray(rows);

            Console.WriteLine("\nВведіть вектор заявок PN:");
            pn = ReadIntArray(cols);

            Console.WriteLine("\nЗгенерований протокол обчислення:\n");

            Console.WriteLine("Матриця вартостей:");
            PrintMatrix(sp, rows, cols);

            Console.WriteLine("Вектор запасів:");
            Console.WriteLine("  " + string.Join("  ", po));

            Console.WriteLine("Вектор заявок:");
            Console.WriteLine("  " + string.Join("  ", pn));

            int sumPO = po.Sum();
            int sumPN = pn.Sum();

            Console.WriteLine($"\nЗагальні запаси: PO = {sumPO}");
            Console.WriteLine($"Загальні заявки: PN = {sumPN}");

            if (sumPO == sumPN)
            {
                Console.WriteLine("Задача є ЗАКРИТОЮ (PO = PN). Фіктивний пункт не потрібен.");
            }
            else
            {
                Console.WriteLine("Задача є ВІДКРИТОЮ (PO ≠ PN).");
                if (sumPO > sumPN)
                {
                    int diff = sumPO - sumPN;
                    Console.WriteLine($"Σ PO > Σ PN, тому додається фіктивний пункт призначення (стовпець) з заявкою PN = {diff} та нульовими вартостями.");
                    int[,] spNew = new int[rows, cols + 1];
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            spNew[i, j] = sp[i, j];
                    sp = spNew;

                    int[] pnNew = new int[cols + 1];
                    for (int j = 0; j < cols; j++) pnNew[j] = pn[j];
                    pnNew[cols] = diff;
                    pn = pnNew;
                    cols++;
                }
                else
                {
                    int diff = sumPN - sumPO;
                    Console.WriteLine($"PN > PO, тому додається фіктивний пункт відправлення (рядок) з запасом PO = {diff} та нульовими вартостями.");
                    int[,] spNew = new int[rows + 1, cols];
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            spNew[i, j] = sp[i, j];
                    sp = spNew;

                    int[] poNew = new int[rows + 1];
                    for (int i = 0; i < rows; i++) poNew[i] = po[i];
                    poNew[rows] = diff;
                    po = poNew;
                    rows++;
                }

                Console.WriteLine("\nМатриця вартостей після додавання фіктивного пункту:");
                PrintMatrix(sp, rows, cols);
                Console.WriteLine("Вектор запасів після коригування:");
                Console.WriteLine("  " + string.Join("  ", po));
                Console.WriteLine("Вектор заявок після коригування:");
                Console.WriteLine("  " + string.Join("  ", pn));
            }

            Console.WriteLine("\nПошук опорного плану перевезень методом північно-західного кута:");
            var nw = NorthWestCorner(sp, (int[])po.Clone(), (int[])pn.Clone(), rows, cols);

            Console.WriteLine("\nПослідовність заповнення таблиці:");
            Console.WriteLine(string.Join("->", nw.Steps));

            Console.WriteLine("\nОпорний план перевезень (Пн-Зх кут):");
            PrintPlanWithX(nw.Plan, rows, cols);

            int costNW = GetCost(sp, nw.Plan, rows, cols);
            Console.WriteLine("\nВартість перевезень за опорним планом:");
            PrintCostFormula(sp, nw.Plan, rows, cols, costNW);

            PotentialsMethod(sp, nw.Plan, rows, cols);

            Console.WriteLine("\nПошук опорного плану перевезень методом мінімального елемента:");
            int[,] planME = MinElementMethod(sp, (int[])po.Clone(), (int[])pn.Clone(), rows, cols);
            PrintPlanWithX(planME, rows, cols);

            int costME = GetCost(sp, planME, rows, cols);
            Console.WriteLine("\nВартість перевезень за опорним планом (Min):");
            PrintCostFormula(sp, planME, rows, cols, costME);

            PotentialsMethod(sp, planME, rows, cols);

            Console.WriteLine("\nНатисніть Enter для виходу...");
            Console.ReadLine();
        }

        static (int[,] Plan, List<string> Steps) NorthWestCorner(int[,] sp, int[] supply, int[] demand, int rows, int cols)
        {
            int[,] plan = new int[rows, cols];
            List<string> steps = new List<string>();
            int i = 0, j = 0;
            while (i < rows && j < cols)
            {
                int val = Math.Min(supply[i], demand[j]);
                plan[i, j] = val;
                steps.Add($"(x{i + 1}{j + 1} = {val})");
                supply[i] -= val;
                demand[j] -= val;
                if (supply[i] == 0 && i < rows - 1) i++;
                else j++;
            }
            return (plan, steps);
        }

        static int[,] MinElementMethod(int[,] sp, int[] supply, int[] demand, int rows, int cols)
        {
            int[,] plan = new int[rows, cols];
            bool[] rowD = new bool[rows];
            bool[] colD = new bool[cols];

            while (rowD.Contains(false) && colD.Contains(false))
            {
                int minV = int.MaxValue, mi = -1, mj = -1;
                for (int i = 0; i < rows; i++)
                {
                    if (rowD[i]) continue;
                    for (int j = 0; j < cols; j++)
                    {
                        if (colD[j]) continue;
                        if (sp[i, j] < minV) { minV = sp[i, j]; mi = i; mj = j; }
                    }
                }
                if (mi == -1) break;

                int amount = Math.Min(supply[mi], demand[mj]);
                plan[mi, mj] = amount;
                supply[mi] -= amount;
                demand[mj] -= amount;

                if (supply[mi] == 0) rowD[mi] = true;
                if (demand[mj] == 0) colD[mj] = true;
            }
            return plan;
        }

        static void PotentialsMethod(int[,] sp, int[,] plan, int rows, int cols)
        {
            int iter = 0;
            while (true)
            {
                iter++;
                Console.WriteLine("\nПошук оптимального плану перевезень методом потенціалів:");

                double[] u = Enumerable.Repeat(double.NaN, rows).ToArray();
                double[] v = Enumerable.Repeat(double.NaN, cols).ToArray();
                u[0] = 0;

                bool[,] basis = new bool[rows, cols];
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++) if (plan[i, j] > 0) basis[i, j] = true;

                bool changed = true;
                while (changed)
                {
                    changed = false;
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            if (basis[i, j])
                            {
                                if (!double.IsNaN(u[i]) && double.IsNaN(v[j])) { v[j] = sp[i, j] - u[i]; changed = true; }
                                else if (double.IsNaN(u[i]) && !double.IsNaN(v[j])) { u[i] = sp[i, j] - v[j]; changed = true; }
                            }
                }

                Console.WriteLine("\nПотенціали постачальників:");
                Console.WriteLine("  " + string.Join("  ", u.Select(x => x.ToString())));
                Console.WriteLine("Потенціали споживачів:");
                Console.WriteLine("  " + string.Join("  ", v.Select(x => x.ToString())));

                Console.WriteLine("Непрямі вартості:");
                var problematic = new List<(int i, int j, double d)>();
                for (int i = 0; i < rows; i++)
                {
                    Console.Write("  ");
                    for (int j = 0; j < cols; j++)
                    {
                        if (basis[i, j]) Console.Write(" x ");
                        else
                        {
                            double ind = u[i] + v[j];
                            double delta = ind - sp[i, j];
                            Console.Write($"{ind,2} ");
                            if (delta > 0) problematic.Add((i, j, delta));
                        }
                    }
                    Console.WriteLine();
                }

                if (problematic.Count == 0)
                {
                    Console.WriteLine("\nУмова оптимальності виконується.");
                    Console.WriteLine("\nЗнайдено оптимальний план перевезень:");
                    PrintPlanWithX(plan, rows, cols);
                    PrintCostFormula(sp, plan, rows, cols, GetCost(sp, plan, rows, cols));
                    break;
                }

                Console.WriteLine("\nУмова оптимальності не виконується.");
                Console.Write("Знайдено «проблемні» клітини: ");
                Console.WriteLine(string.Join("; ", problematic.Select(p => $"[{p.i + 1}, {p.j + 1}]")));

                var best = problematic.OrderByDescending(p => p.d).First();
                var cycle = BuildCycle(basis, best.i, best.j, rows, cols);

                Console.WriteLine("\nПобудовано цикл:");
                PrintCycleGrid(cycle, rows, cols);

                int lambda = int.MaxValue;
                for (int k = 1; k < cycle.Count; k += 2)
                    lambda = Math.Min(lambda, plan[cycle[k].i, cycle[k].j]);

                Console.WriteLine($"\nЗнайдено значення λ: {lambda}, економія: {lambda * best.d}");

                plan[best.i, best.j] = lambda;
                for (int k = 1; k < cycle.Count; k++)
                {
                    if (k % 2 == 1) plan[cycle[k].i, cycle[k].j] -= lambda;
                    else plan[cycle[k].i, cycle[k].j] += lambda;
                }

                Console.WriteLine("\nНовий план перевезень:");
                PrintPlanWithX(plan, rows, cols);
                PrintCostFormula(sp, plan, rows, cols, GetCost(sp, plan, rows, cols));
            }
        }

        static int ReadInt(string p)
        {
            while (true)
            {
                Console.Write(p);
                string input = Console.ReadLine();
                if (int.TryParse(input, out int result) && result > 0)
                    return result;
                Console.WriteLine("  Помилка: введіть ціле додатне число.");
            }
        }

        static int[] ReadIntArray(int c)
        {
            while (true)
            {
                string line = Console.ReadLine();
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != c)
                {
                    Console.WriteLine($"  Помилка: потрібно рівно {c} значень, введено {parts.Length}. Спробуйте ще раз:");
                    continue;
                }
                int[] result = new int[c];
                bool ok = true;
                for (int i = 0; i < c; i++)
                {
                    if (!int.TryParse(parts[i], out result[i]) || result[i] < 0)
                    {
                        Console.WriteLine($"  Помилка: «{parts[i]}» не є цілим невід'ємним числом. Введіть рядок ще раз:");
                        ok = false;
                        break;
                    }
                }
                if (ok) return result;
            }
        }

        static void PrintMatrix(int[,] m, int r, int c)
        {
            for (int i = 0; i < r; i++) { Console.Write("  "); for (int j = 0; j < c; j++) Console.Write($"{m[i, j],3} "); Console.WriteLine(); }
        }

        static void PrintPlanWithX(int[,] plan, int r, int c)
        {
            for (int i = 0; i < r; i++) { Console.Write("  "); for (int j = 0; j < c; j++) Console.Write(plan[i, j] == 0 ? "  x " : $"{plan[i, j],3} "); Console.WriteLine(); }
        }

        static int GetCost(int[,] sp, int[,] plan, int r, int c)
        {
            int s = 0;
            for (int i = 0; i < r; i++) for (int j = 0; j < c; j++) s += sp[i, j] * plan[i, j];
            return s;
        }

        static void PrintCostFormula(int[,] sp, int[,] plan, int r, int c, int total)
        {
            List<string> parts = new List<string>();
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    if (plan[i, j] > 0) parts.Add($"{plan[i, j]} * {sp[i, j]}");
            Console.WriteLine($"  S = {string.Join(" + ", parts)} = {total}");
        }

        static void PrintCycleGrid(List<(int i, int j)> cycle, int r, int c)
        {
            string[,] g = new string[r, c];
            for (int i = 0; i < r; i++) for (int j = 0; j < c; j++) g[i, j] = " x ";
            for (int k = 0; k < cycle.Count; k++) g[cycle[k].i, cycle[k].j] = (k == 0) ? " λ " : (k % 2 == 1 ? " - " : " + ");
            for (int i = 0; i < r; i++) { Console.Write("  "); for (int j = 0; j < c; j++) Console.Write(g[i, j]); Console.WriteLine(); }
        }

        static List<(int i, int j)> BuildCycle(bool[,] basis, int si, int sj, int r, int c)
        {
            var p = new List<(int i, int j)> { (si, sj) };
            FindCycle(basis, p, si, sj, r, c, true);
            return p;
        }

        static bool FindCycle(bool[,] basis, List<(int i, int j)> path, int si, int sj, int r, int c, bool hor)
        {
            var curr = path.Last();
            if (hor)
            {
                for (int j = 0; j < c; j++)
                {
                    if (j == curr.j) continue;
                    if (j == sj && curr.i == si && path.Count > 2) return true;
                    if (basis[curr.i, j]) { path.Add((curr.i, j)); if (FindCycle(basis, path, si, sj, r, c, !hor)) return true; path.RemoveAt(path.Count - 1); }
                }
            }
            else
            {
                for (int i = 0; i < r; i++)
                {
                    if (i == curr.i) continue;
                    if (i == si && curr.j == sj && path.Count > 2) return true;
                    if (basis[i, curr.j]) { path.Add((i, curr.j)); if (FindCycle(basis, path, si, sj, r, c, !hor)) return true; path.RemoveAt(path.Count - 1); }
                }
            }
            return false;
        }
    }
}