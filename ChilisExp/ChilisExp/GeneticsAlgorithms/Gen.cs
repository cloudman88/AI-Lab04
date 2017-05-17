namespace ChilisExp.GeneticsAlgorithms
{
    abstract class Gen //what used to be ga_struct in the given file
    {
        public uint Fitness { get; set; }
        public uint Age { get; set; }
        protected Gen()
        {
            Age = 0;
            Fitness = 0;
        }
    }
} 