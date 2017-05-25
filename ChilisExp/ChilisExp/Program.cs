using System;
using System.Collections.Generic;
using System.Linq;
using ChilisExp.ChilisExpGenetics;
using ChilisExp.GeneticsAlgorithms;

namespace ChilisExp
{
    class Program
    {
        static void Main(string[] args)
        {
            Manager man = new Manager();
            man.Run();

            do
            {
                int k = 6;
                int uppderBound = (int)Math.Ceiling((k * Math.Log(k, 2)));
                for (; uppderBound > 10; uppderBound--)
                {
                    ChilisExpGenetics.ChilisExpGenetics chilisExp = 
                        new ChilisExpGenetics.ChilisExpGenetics(CrossoverMethod.SinglePoint,
                        SelectionMethod.Truncation,MutationOperator.Exchange, 
                        k,uppderBound);
                    chilisExp.init_population();
                    chilisExp.run_algorithm();                    
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

        }
    }
}