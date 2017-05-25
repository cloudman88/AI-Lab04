using System;
using System.Linq;
using ChilisExp.GeneticsAlgorithms;

namespace ChilisExp
{
    class Manager
    {
        private CrossoverMethod _crossoverMethod;
        private MutationOperator _mutationOperator;
        private SelectionMethod _selectionMethod;
        private int _k;
        private int _targetNetworkSize;
        public Manager()
        {
        }

        public void Run()
        {
            choose_crossover_method();
            choose_selection_method();
            choose_mutations_operator();
            choose_k();
            choose_target_network_size();
            ChilisExpGenetics.ChilisExpGenetics ceg = new ChilisExpGenetics.ChilisExpGenetics(_crossoverMethod,_selectionMethod,_mutationOperator,_k,_targetNetworkSize);
            do
            {
                ceg.init_population();
                ceg.run_algorithm();
                Console.WriteLine("press any key to run again or escapse to exit");
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        private void choose_crossover_method()
        {
            Console.WriteLine("Please Choose CrossOver Method :");
            var methodsList = Enum.GetValues(typeof(CrossoverMethod)).Cast<CrossoverMethod>().ToList();           
            for (int i = 0; i < 3; i++)
            {
                methodsList.RemoveAt(0);
            }          
            for (int i = 0; i < methodsList.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + methodsList[i]);
            }
            int input = 0;
            do
            {
                input = get_input();

            } while (input <= 0 || input > methodsList.Count);
            _crossoverMethod = methodsList[input - 1];
        }
        private void choose_mutations_operator()
        {
            Console.WriteLine("Please Choose Mutation Operator :");
            var mutationList = Enum.GetValues(typeof(MutationOperator)).Cast<MutationOperator>().ToList();
            for (int i = 0; i < mutationList.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + mutationList[i]);
            }
            int input = 0;
            do
            {
                input = get_input();

            } while (input <= 0 || input > mutationList.Count);
            _mutationOperator = mutationList[input];
        }
        private void choose_selection_method()
        {
            Console.WriteLine("Please set if Selection method: ");
            var selectionList = Enum.GetValues(typeof(SelectionMethod)).Cast<SelectionMethod>().ToList();
            for (int i = 0; i < selectionList.Count; i++)
            {
                var index = i + 1;
                Console.WriteLine(index + ". " + selectionList[i]);
            }
            int input = 0;
            do
            {
                input = get_input();

            } while (input <= 0 || input > selectionList.Count);
            _selectionMethod = selectionList[input];
        }

        private void choose_k()
        {
            Console.WriteLine("Please choose k size (should be between 2 to 16) ");
            int input = 0;
            do
            {
                input = get_input();
            } while (input < 2 || input >16);
            _k = input;
        }

        private void choose_target_network_size()
        {
            Console.WriteLine("Please choose target network size: (should be between k to k log k) ");
            int input = 0;
            do
            {
                input = get_input();
            } while (input < _k || input > _k*Math.Log(_k,2));
            _targetNetworkSize = input;
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
                    Console.WriteLine("please enter a number in the correct range");
                }

            } while (!validInput);
            return input;
        }
    }
}
