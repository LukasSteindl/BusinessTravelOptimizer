using Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class GreedySolver:Solver
    {
        public GreedySolver(ISolvable model) : base(model)
        {

        }
        public override Result Optimize()
        {
            model.InitModel();
            List<int> sequence = new List<int>();
            for (int j = 0; j < model.GetNumberOfElements(); j++)
                sequence.Add(j);

            //model.SetSequence(sequence);
            return new Result(sequence, model.getModelClone(),0,1);
        }
    }
}
