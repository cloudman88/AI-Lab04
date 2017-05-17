using System;
using ChilisExp.GeneticsAlgorithms;

namespace ChilisExp.ChilisExpGenetics
{
    class ChilisExpGenetics : GeneticsAlgorithms<SortingNetGen>
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

        protected override void Mutate(SortingNetGen member)
        {
            throw new NotImplementedException();
        }

        protected override void mate_by_method(SortingNetGen bufGen, SortingNetGen gen1, SortingNetGen gen2)
        {
            throw new NotImplementedException();
        }

        protected override Tuple<string, uint> get_best_gen_details(SortingNetGen gen)
        {
            throw new NotImplementedException();
        }

        protected override SortingNetGen get_new_gen()
        {
            throw new NotImplementedException();
        }

        protected override int calc_distance(SortingNetGen gen1, SortingNetGen gen2)
        {
            throw new NotImplementedException();
        }
    }
}
