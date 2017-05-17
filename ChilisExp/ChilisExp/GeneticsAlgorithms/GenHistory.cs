namespace ChilisExp.GeneticsAlgorithms
{
    class GenHistory<T> where T : Gen
    {
        public T BestGen { get; set; }
        public double Avg { get; set; }
        public double StdDev { get; set; }
        public GenHistory(T bestGen, double avg, double stdDev)
        {
            BestGen = bestGen;
            Avg = avg;
            StdDev = stdDev;
        }
    }
}
