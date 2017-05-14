using System;
using KnapsackProblem.GeneticsAlgorithms;

namespace ChilisExp.ChilisExpGenetics
{
    class ChilisExpGenetics : GeneticsAlgorithms<ChilisExpGen>
    {
        public ChilisExpGenetics(CrossoverMethod crossMethod, SelectionMethod selectionMethod) : base(crossMethod, selectionMethod)
        {
        }

        public override void init_population()
        {
            throw new NotImplementedException();
        }

        protected override void calc_fitness()
        {
            throw new NotImplementedException();
        }

        protected override void Mutate(ChilisExpGen member)
        {
            throw new NotImplementedException();
        }

        protected override void mate_by_method(ChilisExpGen bufGen, ChilisExpGen gen1, ChilisExpGen gen2)
        {
            throw new NotImplementedException();
        }

        protected override Tuple<string, uint> get_best_gen_details(ChilisExpGen gen)
        {
            throw new NotImplementedException();
        }

        protected override ChilisExpGen get_new_gen()
        {
            throw new NotImplementedException();
        }

        protected override int calc_distance(ChilisExpGen gen1, ChilisExpGen gen2)
        {
            throw new NotImplementedException();
        }
    }
}
