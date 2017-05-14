using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KnapsackProblem.DynamicSol;
using KnapsackProblem.GeneticsAlgorithms;
using KnapsackProblem.GeneticsSol;
using KnapsackProblem.HeuristicSol;
using KnapsackProblem.Tools;

namespace KnapsackProblem
{
    class Manager
    {
        public Manager()
        {
        }

        public void Run()
        {
            print_options();
            int inputEngine = get_input();
            print_problems();
            int inputProblem = get_input();
            var ksProbelms = Enum.GetValues(typeof(KsProbelmFiles)).Cast<KsProbelmFiles>()
                                        .Select(x => x.ToString()).ToArray();
            string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\ksProblems\";
            switch (inputEngine)
            {
                case 1:
                    run_genetics_sol(path+ksProbelms[inputProblem-1]);
                    break;
                case 2:
                    run_heuristics_sol(path+ksProbelms[inputProblem - 1]);
                    break;
                case 3:
                    run_dynamic_sol(path+ksProbelms[inputProblem - 1]);
                    break;
                default:
                    Console.WriteLine("please enter a number between 1 to 3");
                    break;
            }

        }
        private void run_dynamic_sol(string ksProbelm)
        {
            KsProblemDynamic ksd = new KsProblemDynamic();
            ksd.run_algorithm(ksProbelm+".dat");
        }
        private void run_heuristics_sol(string ksProbelm)
        {
            Console.WriteLine("please choose search method: ");
            Console.WriteLine("1. Depth first search ");
            Console.WriteLine("2. Best first search ");

            int searchMethod = get_input();
            SearchAlgorithm sa = SearchAlgorithm.DepthFirstSearch;
            switch (searchMethod)
            {
                case 1:
                    sa = SearchAlgorithm.DepthFirstSearch;
                    break;
                case 2:
                    sa = SearchAlgorithm.BestFirstSearch;
                    break;
                default:
                    Console.WriteLine("please enter a number between 1 to 2");
                    break;
            }

            Console.WriteLine("please choose relaxation method: ");
            Console.WriteLine("1. Capacity ");
            Console.WriteLine("2. Integrality ");

            int relaxMethod = get_input();
            NeglectedConstrain nc = NeglectedConstrain.Capacity;
            switch (relaxMethod)
            {
                case 1:
                    nc = NeglectedConstrain.Capacity;
                    break;
                case 2:
                    nc = NeglectedConstrain.Integrality;
                    break;
                default:
                    Console.WriteLine("please enter a number between 1 to 2");
                    break;
            }

            KsProblemHeuristic ksph = new KsProblemHeuristic(sa,nc);
            ksph.run_algorithm(ksProbelm+".dat");            

        }
        private void run_genetics_sol(string ksProbelm)
        {          

            KsProblemGenetics kspg = new KsProblemGenetics(CrossoverMethod.Uniform,SelectionMethod.Truncation);
            do
            {
                kspg.run_algorithm(ksProbelm+".dat");
                Console.WriteLine("press any key to run again or escapse to exit");
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }


        private void print_options()
        {
            Console.WriteLine("Please choose solution engine to the knapsack probelm by number: ");
            Console.WriteLine("1.Genectics");
            Console.WriteLine("2.Heuristic -Branch and Bound");
            Console.WriteLine("3.Dynamic Programming");
        }
        private void print_problems()
        {
            Console.WriteLine("please choose a problem from the list below");
            var ksProbelms = Enum.GetValues(typeof(KsProbelmFiles)).Cast<KsProbelmFiles>()
                                        .Select(x => x.ToString()).ToArray();
            int i = 1;
            foreach (var probelm in ksProbelms)
            {
                Console.WriteLine(i+". "+probelm.ToString());
                i++;
            }
        }
        private int get_input()
        {
            bool validInput = true;
            int input = 0;
            do
            {
                try
                {
                    input = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("");
                }
                catch (Exception)
                {
                    validInput = false;
                    Console.WriteLine("please enter a number");
                }

            } while (!validInput);
            return input;
        }
    }
}
