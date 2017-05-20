using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChilisExp.GeneticsAlgorithms
{
    public enum SelectionMethod  
    { 
        Truncation,
        Tournament,
        RwsSus,
        Aging,
        ThresholdSpeciation
    }
    public enum CrossoverMethod
    {
        // these are for String Search only (non-ordered chromosomes)
        SinglePoint,
        TwoPoint,
        Uniform,
        // these are for Nqueens,bin packing problem only (ordered chromosomes)
        Pmx, //Partially Matched
        Ox,  //Ordered
        Cx,  //Cycle 
        Er   //Edge recombination
    }
    enum MutationOperator
    {
        Displacement,
        Exchange,
        Insertion,
        SimpleInversion,
        Inversion,
        Scramble
    }
    abstract class GeneticsAlgorithms<T> where T : Gen
    {
        protected readonly Random Rand;
        protected const int GaMaxiter = 1000;		// maximum iterations 16384
        protected const int GaPopSize = 20;		// ga population size 2048
        protected const double GaElitRate = 0.1;	    // elitism rate
        protected const double GaMutationRate = 0.25;    // mutation rate
        protected const int MaxRand = Int32.MaxValue;    //Max value of random function in C#
        protected const double GaMutation = MaxRand * GaMutationRate;
        protected const uint AgeThreshold = 20;
        protected int BestGensHistoryTargetSize = 20;
        protected const int Beta = 12;
        protected const int AvgMaxDif = 30;
        protected const int StdDevMaxDif = 30;

        protected double GaMutationFactor;
        protected int Alpha;
        protected uint MaxFitness;
        protected List<T> Population;
        protected List<T> Buffer;
        protected CrossoverMethod CrosMethod;
        protected SelectionMethod SelectMethod;
        protected List<GenHistory<T>> BestGensHistory;
        protected bool HyperMutWasCalled;
        protected bool LocalOptSearchEnabled;

        protected GeneticsAlgorithms(CrossoverMethod crossMethod, SelectionMethod selectionMethod)
        {
            Rand = new Random();
            Population = new List<T>();
            Buffer = new List<T>();
            CrosMethod = crossMethod;
            SelectMethod = selectionMethod;
            BestGensHistory = new List<GenHistory<T>>(BestGensHistoryTargetSize);
            GaMutationFactor = 1;
            LocalOptSearchEnabled = true;
        }
        public abstract void init_population();
        public virtual void run_algorithm()
        {
            long totalTicks = 0;
            long totalElasped = 0;
            int totalIteration = -1;
            Stopwatch stopWatch = new Stopwatch(); //stopwatch is used for both clock ticks and elasped time measuring
            stopWatch.Start();
            for (int i = 0; i < GaMaxiter; i++)
            {
                calc_fitness();      // calculate fitness
                sort_by_fitness();   // sort them
                var avg = calc_avg(); // calc avg
                var stdDev = calc_std_dev(avg); //calc std dev

                //calculate time differences                
                stopWatch.Stop();
                double ticks = (stopWatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000;
                double elasped = (stopWatch.Elapsed.Ticks / (double)Stopwatch.Frequency) * 1000;
                totalElasped += (long)elasped;
                totalTicks += (long)ticks;

                print_result_details(Population[0], avg, stdDev, i);  // print the best one, average and std dev by iteration number                
                if (LocalOptSearchEnabled == true) search_local_optima(avg, stdDev, i);

                stopWatch.Restart(); // restart timers for next iteration
                if ((Population)[0].Fitness == 0)
                {
                    totalIteration = i + 1; // save number of iteration                                                           
                    break;
                }                
                Mate();     // mate the population together
                swap_population_with_buffer();       // swap buffers
            }
            if (totalIteration == GaMaxiter)
            {
                Console.WriteLine("Failed to find solution in " + totalIteration + " iterations.");
            }
            else
            {
                Console.WriteLine("Iterations: " + totalIteration);
            }
            Console.WriteLine("\nTimig in milliseconds:");
            Console.WriteLine("Total Ticks " + totalTicks);
            Console.WriteLine("Total Elasped " + totalElasped);
            Console.WriteLine("Elaspeds - Ticks = " + (totalElasped - totalTicks) + "\n");
        }

        protected void HyperMutations(bool isReset = false)
        {
            if (isReset == false)
            {
                if (GaMutationRate * GaMutationFactor * 3 < 1)
                {
                    GaMutationFactor *= 3;
                }
                else if (GaMutationRate * GaMutationFactor * 2 < 1)
                {
                    GaMutationFactor *= 2;
                }
                else if (GaMutationRate * GaMutationFactor * 1.5 < 1)
                {
                    GaMutationFactor *= 1.5;
                }
            }
            else
                GaMutationFactor = 1;
        }

        protected void search_local_optima(double avg, double stdDev, int index)
        {
            //check if there is an old bestGen for comparison
            if (BestGensHistory.Count > 0 && (BestGensHistory.Count != BestGensHistoryTargetSize))
            {
                var lastInBestHist = BestGensHistory.Last();
                var firstInBestHist = BestGensHistory.First();
                // if similar add to bestGensHistory, using calc_distance and avg,stdDev diffrence
                if (calc_distance(Population[0], lastInBestHist.BestGen) == 0 &&
                   (Math.Abs(avg - firstInBestHist.Avg) <= AvgMaxDif || (avg - firstInBestHist.Avg) > AvgMaxDif) &&
                   (Math.Abs(stdDev - firstInBestHist.StdDev) <= StdDevMaxDif || (stdDev - firstInBestHist.StdDev) > StdDevMaxDif))
                {
                    GenHistory<T> gHist = new GenHistory<T>(Population[0], avg, stdDev);
                    BestGensHistory.Add(gHist);
                }
                else
                {
                    if (HyperMutWasCalled == true)
                    {
                        HyperMutations(true); //reset mutation rate factor
                        Console.WriteLine("Local optima seems to be over at iteration " + (index + 1));
                        HyperMutWasCalled = false;
                    }
                    BestGensHistory.Clear();
                }
            }
            //if bestGensHistory reached the target size => this is local optima
            else if (BestGensHistory.Count == BestGensHistoryTargetSize)
            {
                if (HyperMutWasCalled != true)
                {
                    Console.WriteLine("Local optima was discoverd after " + (index + 1) + " iterations ");
                    HyperMutations(); // local optima rescue     
                }
                BestGensHistory.RemoveAt(BestGensHistory.Count - 1);
                HyperMutWasCalled = true;
            }
            //if bestGensHistory is empty add the first gen
            else
            {
                GenHistory<T> gHist = new GenHistory<T>(Population[0], avg, stdDev);
                BestGensHistory.Add(gHist);
            }
        }

        protected abstract void calc_fitness();
        protected virtual void sort_by_fitness()
        {
            Population.Sort((s1, s2) => s1.Fitness.CompareTo(s2.Fitness));
        }
        protected void Elitism(int esize)
        {
            for (int i = 0; i < esize; i++)
            {
                Buffer[i] = Population[i];
            }
        }

        protected int elitism_with_aging(int esize)
        {
            int bufCounter = 0;
            int j = -1; //index used to iterate over the rest of population in case 1 gen of the esiz gens is over the threshold
            for (int i = 0; i < esize; i++)
            {
                bool stop = false;
                do
                {
                    j++;
                    Population[j].Age++;
                    if (Population[j].Age > AgeThreshold)
                        Population[j] = get_new_gen();
                    else stop = true;
                } while (stop == false && j < GaPopSize);
                Buffer[i] = Population[j];
                bufCounter++;
            }
            return bufCounter;
        }
        protected abstract void Mutate(T member);
        protected virtual void Mate()
        {
            int esize = (int)(GaPopSize * GaElitRate);
            if (SelectMethod == SelectionMethod.Aging)
            {
                esize = elitism_with_aging(esize);
                //Mate the rest
                SelectionByAging(esize);
            }
            else
            {
                Elitism(esize);
                //Mate the rest           
                switch (SelectMethod)
                {
                    case SelectionMethod.Truncation:
                        SelectionByTruncation(esize);
                        break;
                    case SelectionMethod.Tournament:
                        SelectionByTournament(esize);
                        break;
                    case SelectionMethod.RwsSus:
                        SelectionByRwsSus(esize);
                        break;
                    case SelectionMethod.ThresholdSpeciation:
                        SelectionByThresholdSpeciation(esize);
                        break;
                        //default: throw new Exception("Selection method: "+ selectMethod + " isnt supported for this problem");
                }
            }
        }

        protected void SelectionByRwsSus(int esize)
        {
            uint totalFit = 0;
            for (int i = 0; i < GaPopSize; i++)
            {
                totalFit += (MaxFitness - Population[i].Fitness);
            }
            List<int> rWheel = BuildRouletteWheel(totalFit);
            int n = (GaPopSize) * 2;
            int p = (int)totalFit / n; // p is used to advance the index over the wheel
            int index = Rand.Next() % p;  //starting index is randomly selected by the size of p            
            for (int i = esize; i < GaPopSize; i++)  // loop over population and advance index by 2*p in every iteration
            {
                int i1 = rWheel[index];
                int i2 = rWheel[index + p];
                mate_by_method(Buffer[i], Population[i1], Population[i2]);
                if (Rand.Next() < GaMutation * GaMutationFactor) Mutate(Buffer[i]);
                index += 2 * p;
            }
        }
        protected List<int> BuildRouletteWheel(uint totalFit)
        {
            List<int> rWheel = new List<int>();
            for (int j = 0; j < GaPopSize; j++)
            {
                float genSlicePercent = (float)(MaxFitness - Population[j].Fitness) / totalFit;
                long genSliceSize = (long)Math.Round(genSlicePercent * totalFit);
                for (int i = 0; i < genSliceSize; i++)
                {
                    rWheel.Add(j);
                }
            }
            return rWheel;
        }
        protected void SelectionByTruncation(int eSize)
        {
            for (int i = eSize; i < GaPopSize; i++)
            {
                var i1 = Rand.Next() % (GaPopSize / 2);
                var i2 = Rand.Next() % (GaPopSize / 2);
                mate_by_method(Buffer[i], Population[i1], Population[i2]);
                if (Rand.Next() < GaMutation * GaMutationFactor) Mutate(Buffer[i]);
            }
        }
        protected void SelectionByTournament(int eSize)
        {
            for (int i = eSize; i < GaPopSize; i++)
            {
                //select 2 pairs of gens
                //pair one
                int i1 = Rand.Next() % GaPopSize;
                int i2 = Rand.Next() % GaPopSize;
                //pair two
                int i3 = Rand.Next() % GaPopSize;
                int i4 = Rand.Next() % GaPopSize;
                //get the best gen out of each pair, by fitness value
                var gen1 = (Population[i1].Fitness < Population[i2].Fitness) ? Population[i1] : Population[i2];
                var gen2 = (Population[i3].Fitness < Population[i4].Fitness) ? Population[i3] : Population[i4];
                //mate the 2 best gens out of each pair
                mate_by_method(Buffer[i], gen1, gen2);

                if (Rand.Next() < GaMutation * GaMutationFactor) Mutate(Buffer[i]);
            }
        }
        protected void SelectionByAging(int eSize)
        {
            for (int i = eSize; i < GaPopSize; i++)
            {
                var i1 = (Rand.Next() % GaPopSize);
                var i2 = (Rand.Next() % GaPopSize);
                mate_by_method(Buffer[i], Population[i1], Population[i2]);
                Buffer[i].Age = 0;
                if (Rand.Next() < GaMutation * GaMutationFactor) Mutate(Buffer[i]);
            }
        }

        protected abstract void mate_by_method(T bufGen, T gen1, T gen2);
        protected void print_result_details(T bestGen, double avg, double stdDev, int iteration)
        {
            Tuple<string, uint> bestGenDetails = get_best_gen_details(bestGen);
            Console.WriteLine("#" + (iteration + 1) + " Best: " + bestGenDetails.Item1 + " (" + bestGenDetails.Item2 + ")" +
                ", Average: " + string.Format("{0:N2}", avg) +
                ", Standard Deviation: " + string.Format("{0:N2}", stdDev));
        }
        protected abstract Tuple<string, uint> get_best_gen_details(T gen);
        protected virtual void swap_population_with_buffer()
        {
            List<T> temp = Population;
            Population = Buffer;
            Buffer = temp;
        }
        protected double calc_avg(bool isMax = false)
        {
            double avg = 0;
            for (int i = 0; i < GaPopSize; i++)
            {
                uint fit = (isMax == false) ? Population[i].Fitness : (MaxFitness - Population[i].Fitness);
                avg += fit;
            }
            avg /= GaPopSize;
            return avg;
        }
        protected double calc_std_dev(double avg, bool isMax = false)
        {
            double standardDeviation = 0.0;
            int i;
            for (i = 0; i < GaPopSize; ++i)
            {
                uint fit = (isMax == false) ? Population[i].Fitness : (MaxFitness - Population[i].Fitness);
                standardDeviation += Math.Pow(fit - avg, 2);
            }
            standardDeviation = Math.Sqrt(standardDeviation / GaPopSize);
            return standardDeviation;
        }
        protected abstract T get_new_gen();

        protected void SelectionByThresholdSpeciation(int esize)
        {
            SearchAlpha();
            for (int i = esize; i < GaPopSize; i++)
            {
                bool stop = false;
                while (stop == false)
                {
                    int i2 = Rand.Next() % GaPopSize;
                    int i1 = Rand.Next() % GaPopSize;
                    var gen1 = Population[i1];
                    var gen2 = Population[i2];
                    if (calc_distance(gen1, gen2) < Alpha)
                    {
                        mate_by_method(Buffer[i], gen1, gen2);
                        stop = true;
                    }
                }
                if (Rand.Next() < GaMutation * GaMutationFactor) Mutate(Buffer[i]);
            }
        }

        protected void SearchAlpha()
        {
            HashSet<int> uniqueSpecies = new HashSet<int>();
            bool stop = false;
            do
            {
                uniqueSpecies.Clear();
                var speciationThreshold = Alpha;
                for (int i = 0; i < GaPopSize; i++)
                {
                    for (int j = i + 1; j < GaPopSize; j++)
                    {
                        int result = calc_distance(Population[i], Population[j]);
                        if (result > speciationThreshold)
                        {
                            uniqueSpecies.Add(result);
                            if (uniqueSpecies.Count > Beta)
                            {
                                stop = true;
                                break;
                            }
                        }
                    }
                    if (stop == true)
                    {
                        break;
                    }
                }
                if (uniqueSpecies.Count < Beta)
                    Alpha--;
                else if (uniqueSpecies.Count > Beta)
                    Alpha++;
            } while (uniqueSpecies.Count != Beta);
        }

        protected abstract int calc_distance(T gen1, T gen2);
    }
}