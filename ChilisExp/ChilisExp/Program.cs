using System;
using ChilisExp.GeneticsAlgorithms;

namespace ChilisExp
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                ChilisExpGenetics.ChilisExpGenetics chilisExp = new ChilisExpGenetics.ChilisExpGenetics(CrossoverMethod.SinglePoint,SelectionMethod.Truncation);
                chilisExp.init_population();
                chilisExp.run_algorithm();
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
