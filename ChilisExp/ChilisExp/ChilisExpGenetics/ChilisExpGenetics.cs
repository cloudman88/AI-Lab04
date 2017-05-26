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
        public List<TestCaseGen> TcPopulation; //parasite population
        public List<TestCaseGen> TcBuffer; //parasite buffer
        public List<List<int>> BinaryVectors; //used for the zero-one principal
        public MutationOperator MutationOpt;
        public int GaTcPopSize;
        public int MaxFitnessParasite;
        public int VectorSize;
        public int UppderBound;

        public ChilisExpGenetics(CrossoverMethod crossMethod, SelectionMethod selectionMethod,MutationOperator mutationOperator,int k,int upperBound) : base(crossMethod, selectionMethod)
        {            
            Population = new List<SortingNetGen>();
            Buffer = new List<SortingNetGen>();
            TcPopulation = new List<TestCaseGen>();
            TcBuffer = new List<TestCaseGen>();
            VectorSize = k;
            GaTcPopSize = 50;
            UppderBound = upperBound;
            MutationOpt = mutationOperator;
            MaxFitnessParasite = Population.Count;
            LocalOptSearchEnabled = true;
            InitBindaryVectors();
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
        private void InitBindaryVectors()
        {
            BinaryVectors = Tools.ZeroOnePrincipal.CreateBinaryPermutations(VectorSize);            
        }

        protected override void calc_fitness()
        {
            //reset fitness of the parasites (vectors)      
            for (int i = 0; i < GaTcPopSize; i++)
            {
                TcPopulation[i].Fitness = 0;
            }
            for (int i = 0; i < GaPopSize; i++)
            {
                Population[i].Fitness = 0;
                //for each sorting net gen try to sort all the vectors
                for (int j = 0; j < GaTcPopSize; j++)
                {                   
                    uint result = Population[i].SortVectorByNetwrok(TcPopulation[j].Vector);
                    Population[i].Fitness += result;
                    //if sortion faild, then vector rewarded
                    if (result != 0) TcPopulation[j].Fitness++; 
                }
                if (Population[i].Fitness == 0)
                {
                    // brtue force check all binary vectors with the
                    uint count = (uint)BinaryVectors.Count;
                    uint countSuccess = count;
                    foreach (var binVec in BinaryVectors)
                    {
                        uint res = Population[i].SortVectorByNetwrok(binVec);
                        if (res != 0) countSuccess--;
                    }
                    // update fitness accordingly
                    Population[i].Fitness = count - countSuccess;
                }
            }
        }

        protected override void Mutate(SortingNetGen member)
        {
            switch (MutationOpt)
            {
                    case MutationOperator.IndirectReplacement:
                        IndirectReplacement(Rand.Next()%UppderBound,member);
                    break;
                    case MutationOperator.Exchange:
                        int pos1 = Rand.Next() % VectorSize;
                        int pos2 = Rand.Next() % VectorSize;
                        member.Swap(member.Network, pos1, pos2);
                    break;
                    case MutationOperator.PointMutation3Times:
                        int firstPos = Rand.Next()%UppderBound;
                        for (int i = firstPos; i < firstPos+3 && i < UppderBound; i++)
                        {
                            IndirectReplacement(i,member);
                        }
                    break;
                default: throw new Exception("the selected mutation opertator is not suported in chilis exp");
            }
        }

        private void IndirectReplacement(int index, SortingNetGen member)
        {
            member.Network.RemoveAt(index);
            int num = Rand.Next() % VectorSize;
            Comperator comperator = new Comperator(num, Rand.Next() % (VectorSize-num)+num);
            member.Network.Insert(Rand.Next()% member.Network.Count,comperator);
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
            return new SortingNetGen(VectorSize,UppderBound,Rand);
        }

        protected override int calc_distance(SortingNetGen gen1, SortingNetGen gen2)
        {
            int dif = 0;
            for (int i = 0; i < gen1.Network.Count; i++)
            {
                if (gen1.Network[i].Item1 != gen2.Network[i].Item1) dif++;
                if (gen1.Network[i].Item2 != gen2.Network[i].Item2) dif++;
            }
            return dif;
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

        #region ParasiteMethods
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
            //Mate the rest           
            switch (SelectMethod)
            {
                case SelectionMethod.Truncation:
                    SelectionByTruncationParasite(esize);
                    break;
                case SelectionMethod.Tournament:
                    SelectionByTournamentParasite(esize);
                    break;
            }            
        }
    
        private void MutateParsite(TestCaseGen testCaseGen)
        {
            switch (MutationOpt)
            {
                case MutationOperator.IndirectReplacement:
                    IndirectReplacementParasite(Rand.Next() % VectorSize, testCaseGen);
                    break;
                case MutationOperator.Exchange:
                    int pos1 = Rand.Next() % VectorSize;
                    int pos2 = Rand.Next() % VectorSize;
                    var temp = testCaseGen.Vector[pos1];
                    testCaseGen.Vector[pos1] = testCaseGen.Vector[pos2];
                    testCaseGen.Vector[pos2] = temp;
                    break;
                case MutationOperator.PointMutation3Times:
                    int firstPos = Rand.Next() % VectorSize;
                    for (int i = firstPos; i < firstPos + 3 && i < VectorSize; i++)
                    {
                        IndirectReplacementParasite(i, testCaseGen);
                    }
                    break;
                default: throw new Exception("the selected mutation opertator is not suported in chilis exp");
            }
        }
        private void IndirectReplacementParasite(int index, TestCaseGen testCaseGen)
        {
            testCaseGen.Vector.RemoveAt(index);
            int newNum = Rand.Next()%VectorSize;
            testCaseGen.Vector.Insert(Rand.Next() % testCaseGen.Vector.Count, newNum);
        }
        protected List<int> BuildRouletteWheelParasite(uint totalFit)
        {
            List<int> rWheel = new List<int>();
            for (int j = 0; j < GaTcPopSize; j++)
            {
                float genSlicePercent = (float)(MaxFitnessParasite - TcPopulation[j].Fitness) / totalFit;
                long genSliceSize = (long)Math.Round(genSlicePercent * totalFit);
                for (int i = 0; i < genSliceSize; i++)
                {
                    rWheel.Add(j);
                }
            }
            return rWheel;
        }            
        private void SelectionByTournamentParasite(int esize)
        {
            for (int i = esize; i < GaTcPopSize; i++)
            {
                //select 2 pairs of gens
                //pair one
                int i1 = Rand.Next() % GaTcPopSize;
                int i2 = Rand.Next() % GaTcPopSize;
                //pair two
                int i3 = Rand.Next() % GaTcPopSize;
                int i4 = Rand.Next() % GaTcPopSize;
                //get the best gen out of each pair, by fitness value
                var gen1 = (TcPopulation[i1].Fitness < TcPopulation[i2].Fitness) ? TcPopulation[i1] : TcPopulation[i2];
                var gen2 = (TcPopulation[i3].Fitness < TcPopulation[i4].Fitness) ? TcPopulation[i3] : TcPopulation[i4];
                //mate the 2 best gens out of each pair
                mate_by_method_parasite(TcBuffer[i], gen1, gen2);

                if (Rand.Next() < GaMutation * GaMutationFactor) MutateParsite(TcBuffer[i]);
            }
        }
        private void SelectionByTruncationParasite(int esize)
        {
            for (int i = esize; i < GaTcPopSize; i++)
            {
                var i1 = Rand.Next() % (GaTcPopSize / 2);
                var i2 = Rand.Next() % (GaTcPopSize / 2);
                mate_by_method_parasite(TcBuffer[i], TcPopulation[i1], TcPopulation[i2]);
                if (Rand.Next() < GaMutation * GaMutationFactor) MutateParsite(TcBuffer[i]);
            }
        }
        private  void mate_by_method_parasite(TestCaseGen bufGen, TestCaseGen gen1, TestCaseGen gen2)
        {
            int spos = Rand.Next() % VectorSize;
            int spos2 = Rand.Next() % (VectorSize - spos) + spos;
            switch (CrosMethod)
            {
                case CrossoverMethod.SinglePoint:
                    var start1 = gen1.Vector.GetRange(0, spos);
                    var end1 = gen2.Vector.GetRange(spos, VectorSize - spos);
                    bufGen.Vector = new List<int>(start1.Concat(end1));
                    break;
                case CrossoverMethod.TwoPoint:
                    var start2 = gen1.Vector.GetRange(0, spos);
                    var mid2 = gen1.Vector.GetRange(spos, spos2 - spos);
                    var end2 = gen1.Vector.GetRange(spos2, VectorSize - spos2);
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
        }
        #endregion
        public override void run_algorithm()
        {               
            long totalTicks = 0;
            int totalIteration = -1;
            Stopwatch stopWatch = new Stopwatch();
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

                // print the best one, average and std dev by iteration number                
                print_result_details(Population[0], avg, stdDev, i);
                if (LocalOptSearchEnabled == true) search_local_optima(avg, stdDev, i);
                stopWatch.Restart(); // restart timers for next iteration
                if ((Population)[0].Fitness == 0)
                {
                    //save number of iteration                                                           
                    totalIteration = i + 1;
                    break;
                }
                Mate(); // mate the host population together
                MateParasite(); // mate the parasite population together
                swap_population_with_buffer(); // swap buffers
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
            Console.WriteLine("\nTimig in milliseconds:");
            Console.WriteLine("Total Ticks " + totalTicks);            
        }
       
    }
}