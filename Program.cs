using static TransportProblem;

class Program
{
    static void Main(string[] args)
    {
        // Запасы поставщиков
        int[] supply = { 20, 20, 20, 20 };

        // Потребности потребителей
        int[] demand = { 19, 19, 19, 19, 4 }; 

        // Тарифы
        int[,] cost = {
            { 15, 1, 22, 19, 1 },
            { 21, 18, 11, 4, 3 },
            { 26, 29, 23, 26, 24 },
            { 21, 10, 3, 19 , 27}
        };

        Solve(supply, demand, cost);
    }
}


