using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChilisExp.GeneticsAlgorithms;
using Comperator = System.Tuple<int, int>;

namespace ChilisExp.ChilisExpGenetics
{
    class ChilisExpGenetics : GeneticsAlgorithms<SortingNetGen>
    {
        public List<TestCaseGen> TcPopulation;
        public List<TestCaseGen> TcBuffer;
        public int GaTcPopSize;
        public int VectorSize;
        public int UppderBound;
        //public int TargetNetworkSize;

        public ChilisExpGenetics(CrossoverMethod crossMethod, SelectionMethod selectionMethod,int k,int upperBound) : base(crossMethod, selectionMethod)
        {            
            Population = new List<SortingNetGen>();
            Buffer = new List<SortingNetGen>();
            TcPopulation = new List<TestCaseGen>();
            TcBuffer = new List<TestCaseGen>();
            VectorSize = k;
            GaTcPopSize = 500;
            UppderBound = upperBound;
            LocalOptSearchEnabled = false;
        }

        public override void init_population()
        {
            for (int i = 0; i < GaPopSize; i++)
            {
                Population.Add(new SortingNetGen(VectorSize, UppderBound, Rand));
                Buffer.Add(new SortingNetGen(VectorSize, UppderBound, Rand));
            }
            for (int i = 0; i < GaTcPopSize; i++)
            {
                TcPopulation.Add(new TestCaseGen(VectorSize, Rand));
                TcBuffer.Add(new TestCaseGen(VectorSize, Rand));
            }
        }

        protected override void calc_fitness()
        {
            //reset fitness of the vectors
            for (int i = 0; i < GaTcPopSize; i++)
            {
                TcPopulation[i].Fitness = 0;
            }

            for (int i = 0; i < GaPopSize; i++)
            {
                Population[i].Fitness = 0;
                //for each sorting new gen try to sort all the vectors
                for (int j = 0; j < GaTcPopSize; j++)
                {                   
                    uint result = Population[i].SortVectorByNetwrok(TcPopulation[j].Vector);
                    Population[i].Fitness += result;
                    //if sortion faild, then vector rewarded
                    if (result != 0) TcPopulation[j].Fitness++; 
                }
            }
        }

        protected override void Mutate(SortingNetGen member)
        {
            //todo add mutation methods
            int pos1 = Rand.Next() % VectorSize;
            int pos2 = Rand.Next() % VectorSize;
            member.Swap(member.Network,pos1,pos2);
        }

        protected override void mate_by_method(SortingNetGen bufGen, SortingNetGen gen1, SortingNetGen gen2)
        {
            int tempSize = gen1.Network.Count;
            int spos = Rand.Next() % tempSize;
            int spos2 = Rand.Next() % (tempSize - spos) + spos;
            switch (CrosMethod)
            {
                case CrossoverMethod.SinglePoint:
                    List<Comperator> start1 = new List<Comperator>(gen1.Network.GetRange(0, spos));
                    List<Comperator> end1 = new List<Comperator>(gen2.Network.GetRange(spos, tempSize - spos));
                    bufGen.Network = new List<Comperator>(start1.Concat(end1));
                    break;
                case CrossoverMethod.TwoPoint:
                    var start2 = gen1.Network.GetRange(0, spos);
                    var mid2 = gen1.Network.GetRange(spos, spos2 - spos);
                    var end2 = gen1.Network.GetRange(spos2, tempSize- spos2);
                    var temp = (List<Comperator>)start2.Concat(mid2);
                    bufGen.Network = new List<Comperator>(temp.Concat(end2));
                    break;
                case CrossoverMethod.Uniform:
                    for (int j = 0; j < tempSize; j++)
                    {
                        // randomlly choose char from either gens    
                        int genToChoose = Rand.Next() % 2;
                        bufGen.Network[j] = (genToChoose == 0) ? gen1.Network[j] : gen2.Network[j];
                    }
                    break;
            }
        }

        protected override Tuple<string, uint> get_best_gen_details(SortingNetGen gen)
        {
            string str = "";
            foreach (var comperator in gen.Network)
            {
                str += comperator.Item1 + "," + comperator.Item2 + " ";
            }
            return new Tuple<string, uint>(str,gen.Fitness);
        }

        protected override SortingNetGen get_new_gen()
        {
            throw new NotImplementedException();
        }

        protected override int calc_distance(SortingNetGen gen1, SortingNetGen gen2)
        {
            throw new NotImplementedException();
        }

        protected override void sort_by_fitness()
        {
            Population.Sort((s1, s2) => s1.Fitness.CompareTo(s2.Fitness));
            TcPopulation = TcPopulation.OrderByDescending(x => x.Fitness).ToList();
            //TcPopulation.Sort((s1, s2) => s1.Fitness.CompareTo(s2.Fitness));
        }

        protected override void swap_population_with_buffer()
        {
            List<SortingNetGen> tempS = Population;
            Population = Buffer;
            Buffer = tempS;

            List<TestCaseGen> tempTc = TcPopulation;
            TcPopulation = TcBuffer;
            TcBuffer = tempTc;
        }

        private void elitism_parasite(int esize)
        {
            for (int i = 0; i < esize; i++)
            {
                TcBuffer[i] = TcPopulation[i];
            }
        }

        private void MateParasite()
        {
            int esize = (int)(GaTcPopSize * GaElitRate);
            elitism_parasite(esize);

            for (int i = esize; i < GaTcPopSize; i++)
            {
                var i1 = Rand.Next() % (GaTcPopSize / 2);
                var i2 = Rand.Next() % (GaTcPopSize / 2);

                int spos = Rand.Next() % VectorSize;
                int spos2 = Rand.Next() % (VectorSize - spos) + spos;

                var bufGen = TcBuffer[i];
                var gen1 = TcPopulation[i1];
                var gen2 = TcPopulation[i2];
                switch (CrosMethod)
                {
                    case CrossoverMethod.SinglePoint:
                        var start1 = gen1.Vector.GetRange(0, spos);
                        var end1 = gen2.Vector.GetRange(spos, VectorSize- spos);
                        bufGen.Vector = new List<int>(start1.Concat(end1));
                        break;
                    case CrossoverMethod.TwoPoint:
                        var start2 = gen1.Vector.GetRange(0, spos);
                        var mid2 = gen1.Vector.GetRange(spos, spos2- spos);
                        var end2 = gen1.Vector.GetRange(spos2, VectorSize- spos2);
                        var temp = (List<int>)start2.Concat(mid2);
                        bufGen.Vector = new List<int>(temp.Concat(end2));
                        break;
                    case CrossoverMethod.Uniform:
                        for (int j = 0; j < VectorSize; j++)
                        {
                            // randomlly choose char from either gens    
                            int genToChoose = Rand.Next() % 2;
                            bufGen.Vector[j] = (genToChoose == 0) ? gen1.Vector[j] : gen2.Vector[j];
                        }
                        break;
                }
                //if (Rand.Next() < GaMutation * GaMutationFactor) Mutate(TcBuffer[i]);
            }
        }
        public override void run_algorithm()
        {               
            long totalTicks = 0;
            int totalIteration = -1;
            Stopwatch stopWatch = new Stopwatch();
                //stopwatch is used for both clock ticks and elasped time measuring
            stopWatch.Start();
            for (int i = 0; i < GaMaxiter; i++)
            {
                calc_fitness(); // calculate fitness
                sort_by_fitness(); // sort them
                var avg = calc_avg(); // calc avg
                var stdDev = calc_std_dev(avg); //calc std dev

                //calculate time differences                
                stopWatch.Stop();
                double ticks = (stopWatch.ElapsedTicks/(double) Stopwatch.Frequency)*1000;
                totalTicks += (long) ticks;

                print_result_details(Population[0], avg, stdDev, i);
                    // print the best one, average and std dev by iteration number                
                if (LocalOptSearchEnabled == true) search_local_optima(avg, stdDev, i);
                stopWatch.Restart(); // restart timers for next iteration
                if ((Population)[0].Fitness == 0)
                {
                    Console.WriteLine("Network Depth: " + Population[0].Network.Count);
                    Verify(Population[0]);
                    //Console.WriteLine("Sorted vectors: " + Population[0].Network.Count);
                    //foreach (var testCaseGen in TcPopulation)
                    //{
                    //    var vector = testCaseGen.Vector;
                    //    var network = Population[0].Network;
                    //    List<int> tempVec = new List<int>(vector);
                    //    for (int k = 0; k < network.Count; k++)
                    //    {
                    //        var index1 = network[k].Item1;
                    //        var index2 = network[k].Item2;
                    //        if (tempVec[index1] > tempVec[index2])
                    //        {
                    //            Population[0].Swap(tempVec, index1, index2);
                    //        }
                    //    }
                    //    string res = "";
                    //    foreach (var item in tempVec)
                    //    {
                    //        res += item + " ";
                    //    }
                    //    Console.WriteLine(res);
                    //}
                    //save number of iteration                                                           
                    totalIteration = i + 1;
                    break;
                }
                Mate(); // mate the population together
                MateParasite();
                Check();
                swap_population_with_buffer(); // swap buffers
                Check();
            }
            if (totalIteration == GaMaxiter)
            {
                Console.WriteLine("Failed to find solution in " + totalIteration + " iterations.");
            }
            else
            {
                Console.WriteLine("Iterations: " + totalIteration);
            }
            Console.WriteLine("Network Depth: " + Population[0].Network.Count);
            Console.WriteLine("UpperBound: " + UppderBound);
            Console.WriteLine("\nTimig in milliseconds:");
            Console.WriteLine("Total Ticks " + totalTicks);            
        }

        private void Verify(SortingNetGen sngen)
        {
            IEnumerable<IEnumerable<int>> result = GetPermutations(Enumerable.Range(1, VectorSize), VectorSize);
                    Console.WriteLine("#permutation: "+ result.Count());

            foreach (var permutation in result)
            {
                var dif = sngen.SortVectorByNetwrok(permutation.ToList(),true);
                if (dif != 0)
                {
                    string res = "";
                    foreach (var num in permutation.ToList())
                    {
                        res += num + " ";
                    }
                    Console.WriteLine("failed to sort:");
                    Console.WriteLine(res);
                }
            }
        }
        static IEnumerable<IEnumerable<T>>GetPermutations<T>(IEnumerable < T > list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        private void Check()
        {
            var vec = TcPopulation.First().Vector;
            bool x = true;
            foreach (var testGen in TcPopulation)
            {
                for (int i = 0; i < vec.Count; i++)
                {
                    if (vec[i] != testGen.Vector[i])
                    {
                        x = false;
                        break;
                    };
                }
            }
            if (x == true)
            {
                Console.WriteLine("same parsites");                
            }
            var net = Population.First().Network;
            foreach (var sgen in Population)
            {
                for (int i = 0; i < vec.Count; i++)
                {
                    if (net[i].Item1 != sgen.Network[i].Item1) return;
                    if (net[i].Item2 != sgen.Network[i].Item2) return;
                }
            }
            Console.WriteLine("same hosts");
        }
    }
}