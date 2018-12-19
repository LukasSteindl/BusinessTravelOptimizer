using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class Result
    {
        List<int> sequence;
    
        object model;
        double percent;
        int modelEvaluations;

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

     

        public object Model
        {
            get
            {
                return model;
            }

            set
            {
                model = value;
            }
        }

        public double Percent
        {
            get
            {
                return percent;
            }

            set
            {
                percent = value;
            }
        }

        public int ModelEvaluations
        {
            get
            {
                return modelEvaluations;
            }

            set
            {
                modelEvaluations = value;
            }
        }

        public Result(List<int> sequence, object model,double percent,int modelEvaluations)
        {
            this.Sequence = sequence;
            this.ModelEvaluations = modelEvaluations;
            this.Model = model;
            this.Percent = percent;
        }


    }
}
