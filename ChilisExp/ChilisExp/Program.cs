using System;
using System.Collections.Generic;
using ChilisExp.ChilisExpGenetics;
using ChilisExp.GeneticsAlgorithms;

namespace ChilisExp
{
    class Program
    {
        static void Main(string[] args)
        {
            //SortingNetGen sng = new SortingNetGen(6,12,new Random());
            List<int> l1 = new List<int> {1,2,3,4,5,6};
            List<int> l2 = new List<int> {2,3,4,5,6,1};
            List<int> l3 = new List<int> {1,2,3,4,6,5};
            List<int> l4 = new List<int> {6,4,5,1,3,2};
            //var res1 = sng.calc_lev_distance(l1,l2);
            //var res2 = sng.calc_lev_distance(l1,l3);
            //var res3 = sng.calc_lev_distance(l1,l4);
            //Console.WriteLine("lev res1: " + res1+ " res2: "+ res2 + " res3: "+ res3);
            var res4 = kt(l1, l2);
            var res5 = kt(l1, l3);
            var res6 = kt(l1, l4);
            Console.WriteLine("kt res4: " + res4+ " res5: "+ res5 + " res6: "+ res6);


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

        public static int kt(List<int> l1, List<int> l2)
        {
            //Kendall tau distance
            int distance = 0;
            var list1 = l1;
            var list2 = l2;
            for (int i = 0; i<l1.Count; i++)
            {
                for (int j = i + 1; j<l1.Count; j++)
                {
                    if ((list1[i] < list1[j] && list2[i] > list2[j]) ||
                        (list1[i] > list1[j] && list2[i] < list2[j])) distance++;
                }
            }
            return distance;            
        }
    }
}