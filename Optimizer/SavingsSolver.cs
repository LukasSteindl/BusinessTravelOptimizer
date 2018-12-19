using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class SavingsSolver : Solver
    {
        public SavingsSolver(ISolvable model) : base(model)
        {

        }
        public override Result Optimize()
        {
            model.InitModel();
            List<int> sequence = model.get_savings_heuristic_sequence();
            //model.SetSequence(sequence);
            return new Result(sequence, model.getModelClone(), 0,1);
        }
    }
}
