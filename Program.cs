using System;
using System.Text;
using System.Collections.Generic;

class JordanMatrixLab
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Пошук оберненої матриці");
            Console.WriteLine("2. Розв'язання СЛАР способом 1");
            Console.WriteLine("3. Розв'язання системи способом 2");
            Console.WriteLine("4. Метод Гаусса (Спосіб 3)");
            Console.WriteLine("5. Пошук рангу матриці");
            Console.WriteLine("0. Вихід");
            Console.Write("\nОберіть (0-5): ");

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 5) continue;
            if (choice == 0) break;
            int n;
            Console.Write("Введіть кількість рядків n: ");
            while (!int.TryParse(Console.ReadLine(), out n)) { Console.Write("Помилка! Введіть ціле число n: "); }
            int m_dim;
            Console.Write("Введіть кількість стовпців m: ");
            while (!int.TryParse(Console.ReadLine(), out m_dim)) { Console.Write("Помилка! Введіть ціле число m: "); }

            double[,] A = new double[n, m_dim];
            double[] B = new double[n];

            Console.WriteLine("\nВведіть матрицю A:");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m_dim; j++)
                {
                    double val;
                    Console.Write($"A[{i + 1},{j + 1}] = ");
                    while (!double.TryParse(Console.ReadLine()?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                    {
                        Console.Write($"Невірно! Введіть число для A[{i + 1},{j + 1}] = ");
                    }
                    A[i, j] = val;
                }
            }

            if (choice >= 2 && choice <= 4)
            {
                Console.WriteLine("\nВведіть вектор B:");
                for (int i = 0; i < n; i++)
                {
                    double val;
                    Console.Write($"B[{i + 1}] = ");
                    while (!double.TryParse(Console.ReadLine()?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                    {
                        Console.Write($"Невірно! Введіть число для B[{i + 1}] = ");
                    }
                    B[i] = val;
                }
            }

            Console.Clear();
            Console.WriteLine("1. РІШЕННЯ");
            StringBuilder sbHeader = new StringBuilder();
            sbHeader.AppendLine("\nПОЧАТКОВІ ДАНІ:");
            sbHeader.AppendLine("Матриця A:");
            sbHeader.AppendLine(MatrixToString(A));
            if (choice >= 2 && choice <= 4)
            {
                sbHeader.Append("Вектор B: [ ");
                for (int i = 0; i < n; i++) sbHeader.Append($"{B[i]:0.###} ");
                sbHeader.AppendLine("]");
            }

            string protocol = "";
            try
            {
                switch (choice)
                {
                    case 1: protocol = sbHeader.ToString() + InverseProcess(A, n, false, null); break;
                    case 2: protocol = sbHeader.ToString() + InverseProcess(A, n, true, B); break;
                    case 3: protocol = sbHeader.ToString() + Method2(A, B, n); break;
                    case 4: protocol = sbHeader.ToString() + MethodGauss(A, B, n); break;
                    case 5: protocol = sbHeader.ToString() + CalculateRank(A, n, m_dim); break;
                }
                Console.WriteLine("\n2. ПРОТОКОЛ ОБЧИСЛЕНЬ");
                Console.WriteLine(protocol);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nУВАГА: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть Enter для повернення в меню...");
            Console.ReadLine();
        }
    }

    static string InverseProcess(double[,] A, int n, bool solveSystem, double[] B)
    {
        StringBuilder sb = new StringBuilder();
        double[,] C = (double[,])A.Clone();
        double[] bClone = null;
        if (solveSystem) bClone = (double[])B.Clone();

        for (int i = 0; i < n; i++)
        {
            if (!HandleZeroPivot(C, i, n, sb, bClone)) throw new Exception("Нуль на діагоналі.");
            sb.AppendLine($"Крок #{i + 1}. Розв'язувальний елемент a_rs: [{i + 1},{i + 1}] = {C[i, i]:0.###}");
            C = ProceduraZJV(C, i, i);
            sb.AppendLine(MatrixToString(C));
        }

        if (solveSystem)
        {
            sb.AppendLine("\nОбчислення розв’язків:");
            for (int i = 0; i < n; i++)
            {
                double Xi = 0;
                string line = $"X[{i + 1}] = ";
                for (int j = 0; j < n; j++)
                {
                    Xi += C[i, j] * bClone[j];
                    line += $"({C[i, j]:0.###}) * {bClone[j]:0.###}" + (j == n - 1 ? "" : " + ");
                }
                sb.AppendLine($"{line} = {Xi:0.###}");
                Console.WriteLine($"X[{i + 1}] = {Xi:0.###}");
            }
        }
        else { Console.WriteLine("Обернена матриця C:"); PrintMatrix(C); }
        return sb.ToString();
    }

    static string Method2(double[,] A, double[] B, int n)
    {
        StringBuilder sb = new StringBuilder();
        double[,] mat = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++) mat[i, j] = A[i, j];
            mat[i, n] = -B[i];
        }
        for (int i = 0; i < n; i++)
        {
            HandleZeroPivot(mat, i, n, sb, null);
            mat = ProceduraZJV(mat, i, i);
            sb.AppendLine($"Крок #{i + 1}:\n{MatrixToString(mat)}");
        }
        sb.AppendLine("\nОбчислення розв’язків:");
        for (int i = 0; i < n; i++)
        {
            sb.AppendLine($"X[{i + 1}] = {-mat[i, n]:0.###}");
            Console.WriteLine($"X[{i + 1}] = {-mat[i, n]:0.###}");
        }
        return sb.ToString();
    }

    static string MethodGauss(double[,] A, double[] B, int n)
    {
        StringBuilder sb = new StringBuilder();
        double[,] m = new double[n, n + 1];
        for (int i = 0; i < n; i++) { for (int j = 0; j < n; j++) m[i, j] = A[i, j]; m[i, n] = B[i]; }

        for (int i = 0; i < n; i++)
        {
            int max = i;
            for (int k = i + 1; k < n; k++) if (Math.Abs(m[k, i]) > Math.Abs(m[max, i])) max = k;
            for (int j = 0; j <= n; j++) { double t = m[i, j]; m[i, j] = m[max, j]; m[max, j] = t; }
            if (Math.Abs(m[i, i]) < 1e-10) continue;
            for (int k = i + 1; k < n; k++)
            {
                double f = m[k, i] / m[i, i];
                for (int j = i; j <= n; j++) m[k, j] -= f * m[i, j];
            }
            sb.AppendLine($"Прямий хід, крок {i + 1}:\n{MatrixToString(m)}");
        }

        double[] x = new double[n];
        sb.AppendLine("\nОбчислення розв’язків (зворотний хід):");
        for (int i = n - 1; i >= 0; i--)
        {
            double s = 0;
            string line = $"X[{i + 1}] = ( {m[i, n]:0.###} ";
            for (int j = i + 1; j < n; j++)
            {
                s += m[i, j] * x[j];
                line += $"- ({m[i, j]:0.###} * {x[j]:0.###}) ";
            }
            x[i] = (m[i, n] - s) / m[i, i];
            sb.AppendLine($"{line} ) / {m[i, i]:0.###} = {x[i]:0.###}");
            Console.WriteLine($"X[{i + 1}] = {x[i]:0.###}");
        }
        return sb.ToString();
    }

    static string CalculateRank(double[,] A, int n, int m_dim)
    {
        double[,] temp = (double[,])A.Clone();
        int r = 0;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < Math.Min(n, m_dim); i++)
        {
            if (Math.Abs(temp[i, i]) < 1e-10)
            {
                HandleZeroPivot(temp, i, n, sb, null);
            }
            if (Math.Abs(temp[i, i]) > 1e-10)
            {
                temp = ProceduraZJV(temp, i, i);
                r = r + 1; 
            }
        }

        Console.WriteLine($"Ранг матриці r: {r}");
        return $"Обчислення завершено. Виявлено ранг: {r}";
    }

    static bool HandleZeroPivot(double[,] m, int k, int n, StringBuilder sb, double[] bVector)
    {
        if (Math.Abs(m[k, k]) > 1e-10) return true;
        for (int i = k + 1; i < n; i++)
        {
            if (Math.Abs(m[i, k]) > 1e-10)
            {
                for (int j = 0; j < m.GetLength(1); j++) { double t = m[k, j]; m[k, j] = m[i, j]; m[i, j] = t; }

                if (bVector != null) { double tempB = bVector[k]; bVector[k] = bVector[i]; bVector[i] = tempB; }

                sb.AppendLine($"(!) Рядок {k + 1} замінено на {i + 1}");
                return true;
            }
        }
        return false;
    }

    static double[,] ProceduraZJV(double[,] m, int r, int s)
    {
        int rows = m.GetLength(0), cols = m.GetLength(1);
        double[,] res = new double[rows, cols];
        double a_rs = m[r, s];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                if (i == r && j == s) res[i, j] = 1 / a_rs;
                else if (i == r) res[i, j] = m[r, j] / a_rs;
                else if (j == s) res[i, j] = -m[i, s] / a_rs;
                else res[i, j] = m[i, j] - (m[i, s] * m[r, j]) / a_rs;
            }
        return res;
    }

    static string MatrixToString(double[,] m)
    {
        StringBuilder s = new StringBuilder();
        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m.GetLength(1); j++) s.Append($"{m[i, j],10:0.###} ");
            s.AppendLine();
        }
        return s.ToString();
    }

    static void PrintMatrix(double[,] m)
    {
        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m.GetLength(1); j++) Console.Write($"{m[i, j],10:0.###} ");
            Console.WriteLine();
        }
    }
}