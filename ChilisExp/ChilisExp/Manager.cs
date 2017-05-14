using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KnapsackProblem.GeneticsAlgorithms;

namespace ChilisExp
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
           
            switch (inputEngine)
            {
                case 1:
                    break;
                default:
                    Console.WriteLine("please enter a number between 1 to 3");
                    break;
            }

        }
      
        private void run_genetics_sol()
        {          
            ChilisExpGenetics.ChilisExpGenetics ceg = new ChilisExpGenetics.ChilisExpGenetics(CrossoverMethod.Uniform,SelectionMethod.Truncation);
            do
            {
                ceg.init_population();
                ceg.run_algorithm();
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
