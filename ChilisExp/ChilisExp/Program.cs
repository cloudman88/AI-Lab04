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
                int k = 6;
                int uppderBound = (int)Math.Ceiling((k * Math.Log(k, 2)));
                for (; uppderBound > 10; uppderBound--)
                {
                    ChilisExpGenetics.ChilisExpGenetics chilisExp = 
                        new ChilisExpGenetics.ChilisExpGenetics(CrossoverMethod.SinglePoint,SelectionMethod.Truncation,
                        k,uppderBound);
                    chilisExp.init_population();
                    chilisExp.run_algorithm();                    
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
