using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    class Individual
    {
        List<int> sequence = new List<int>();
        double fitness;

        public Individual (List<int> sequence, double fitness)
        {
            Sequence = sequence;
            Fitness = fitness;
        }

        public List<int> Sequence
        {
            get
            {
                return sequence;
            }

            set
            {
                sequence = value;
            }
        }

        public double Fitness
        {
            get
            {
                return fitness;
            }

            set
            {
                fitness = value;
            }
        }
    }
}
