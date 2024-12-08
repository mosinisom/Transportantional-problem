using System;
using System.Collections.Generic;
using System.Linq;

class TransportProblem
{
    public static void Solve(int[] supply, int[] demand, int[,] costs)
    {
        int n = supply.Length;
        int m = demand.Length;

        if (supply.Sum() != demand.Sum())
        {
            Console.WriteLine("Задача несбалансирована!");
            return;
        }

        int[,] allocation = GetInitialSolution(supply, demand, costs);
        bool isOptimal = false;
        int iterations = 0;
        const int MAX_ITERATIONS = 100;

        while (!isOptimal && iterations < MAX_ITERATIONS)
        {
            iterations++;

            (double[] u, double[] v) = FindPotentials(allocation, costs);
            if (u == null || v == null)
            {
                Console.WriteLine("Не удалось найти потенциалы!");
                return;
            }

            (int maxI, int maxJ, double maxDelta) = FindMaxDelta(allocation, costs, u, v);

            if (maxDelta <= 0)
            {
                isOptimal = true;
                continue;
            }

            if (!Redistribute(allocation, maxI, maxJ))
            {
                Console.WriteLine("Не удалось найти цикл пересчета!");
                return;
            }
        }

        if (!isOptimal)
        {
            Console.WriteLine("Превышено максимальное число итераций!");
        }

        PrintSolution(allocation, costs);
    }

    private static int[,] GetInitialSolution(int[] supply, int[] demand, int[,] costs)
    {
        int n = supply.Length;
        int m = demand.Length;
        int[,] allocation = new int[n, m];
        int[] remainingSupply = (int[])supply.Clone();
        int[] remainingDemand = (int[])demand.Clone();

        while (remainingSupply.Sum() > 0)
        {
            int minCost = int.MaxValue;
            int minI = -1, minJ = -1;

            for (int i = 0; i < n; i++)
            {
                if (remainingSupply[i] <= 0) continue;
                for (int j = 0; j < m; j++)
                {
                    if (remainingDemand[j] <= 0) continue;
                    if (costs[i, j] < minCost)
                    {
                        minCost = costs[i, j];
                        minI = i;
                        minJ = j;
                    }
                }
            }

            if (minI == -1 || minJ == -1) break;

            int quantity = Math.Min(remainingSupply[minI], remainingDemand[minJ]);
            allocation[minI, minJ] = quantity;
            remainingSupply[minI] -= quantity;
            remainingDemand[minJ] -= quantity;
        }

        return allocation;
    }

    private static (double[] u, double[] v) FindPotentials(int[,] allocation, int[,] costs)
    {
        int n = allocation.GetLength(0);
        int m = allocation.GetLength(1);
        double[] u = new double[n];
        double[] v = new double[m];
        bool[] uDefined = new bool[n];
        bool[] vDefined = new bool[m];

        u[0] = 0;
        uDefined[0] = true;

        bool changed;
        do
        {
            changed = false;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (allocation[i, j] > 0)
                    {
                        if (uDefined[i] && !vDefined[j])
                        {
                            v[j] = costs[i, j] - u[i];
                            vDefined[j] = true;
                            changed = true;
                        }
                        else if (!uDefined[i] && vDefined[j])
                        {
                            u[i] = costs[i, j] - v[j];
                            uDefined[i] = true;
                            changed = true;
                        }
                    }
                }
            }
        } while (changed);

        return uDefined.All(x => x) && vDefined.All(x => x) ? (u, v) : (null, null);
    }

    private static (int maxI, int maxJ, double maxDelta) FindMaxDelta(int[,] allocation, int[,] costs, double[] u, double[] v)
    {
        int n = allocation.GetLength(0);
        int m = allocation.GetLength(1);
        double maxDelta = -1;
        int maxI = -1, maxJ = -1;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (allocation[i, j] == 0)
                {
                    double delta = u[i] + v[j] - costs[i, j];
                    if (delta > maxDelta)
                    {
                        maxDelta = delta;
                        maxI = i;
                        maxJ = j;
                    }
                }
            }
        }

        return (maxI, maxJ, maxDelta);
    }

    private static bool Redistribute(int[,] allocation, int startI, int startJ)
    {
        var cycle = FindCycle(allocation, startI, startJ);
        if (cycle == null || cycle.Count == 0) return false;

        int minValue = int.MaxValue;
        for (int i = 1; i < cycle.Count; i += 2)
        {
            var (row, col) = cycle[i];
            minValue = Math.Min(minValue, allocation[row, col]);
        }

        for (int i = 0; i < cycle.Count; i++)
        {
            var (row, col) = cycle[i];
            allocation[row, col] += (i % 2 == 0) ? minValue : -minValue;
        }

        return true;
    }

    private static List<(int, int)> FindCycle(int[,] allocation, int startI, int startJ)
    {
        int n = allocation.GetLength(0);
        int m = allocation.GetLength(1);
        var visited = new bool[n, m];
        var cycle = new List<(int, int)>();

        cycle.Add((startI, startJ));
        if (FindCycleDFS(allocation, startI, startJ, startI, startJ, true, cycle, visited))
        {
            return cycle;
        }

        return null;
    }

    private static bool FindCycleDFS(int[,] allocation, int currentI, int currentJ, int startI, int startJ, bool lookingForVertical, List<(int, int)> cycle, bool[,] visited)
    {
        if (lookingForVertical)
        {
            for (int i = 0; i < allocation.GetLength(0); i++)
            {
                if (i != currentI && (allocation[i, currentJ] > 0 || (i == startI && currentJ == startJ)))
                {
                    if (i == startI && currentJ == startJ && cycle.Count > 3)
                    {
                        return true;
                    }

                    if (!visited[i, currentJ])
                    {
                        visited[i, currentJ] = true;
                        cycle.Add((i, currentJ));

                        if (FindCycleDFS(allocation, i, currentJ, startI, startJ, false, cycle, visited))
                        {
                            return true;
                        }

                        cycle.RemoveAt(cycle.Count - 1);
                        visited[i, currentJ] = false;
                    }
                }
            }
        }
        else
        {
            for (int j = 0; j < allocation.GetLength(1); j++)
            {
                if (j != currentJ && (allocation[currentI, j] > 0 || (currentI == startI && j == startJ)))
                {
                    if (currentI == startI && j == startJ && cycle.Count > 3)
                    {
                        return true;
                    }

                    if (!visited[currentI, j])
                    {
                        visited[currentI, j] = true;
                        cycle.Add((currentI, j));

                        if (FindCycleDFS(allocation, currentI, j, startI, startJ, true, cycle, visited))
                        {
                            return true;
                        }

                        cycle.RemoveAt(cycle.Count - 1);
                        visited[currentI, j] = false;
                    }
                }
            }
        }

        return false;
    }

    private static void PrintSolution(int[,] allocation, int[,] costs)
    {
        Console.WriteLine("\nИтоговый план перевозок:");
        int n = allocation.GetLength(0);
        int m = allocation.GetLength(1);
        double totalCost = 0;

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                Console.Write($"{allocation[i, j]}\t");
                totalCost += allocation[i, j] * costs[i, j];
            }
            Console.WriteLine();
        }

        Console.WriteLine($"\nОбщая стоимость перевозок: {totalCost}");
    }
}