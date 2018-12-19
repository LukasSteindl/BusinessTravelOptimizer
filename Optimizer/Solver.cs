using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public abstract class Solver
    {
        protected ISolvable model;
        protected Solver(ISolvable model)
        {
            this.model = model;
            ProgressHandler = new Progress<Result>();
        }

        protected Progress<Result> progressHandler;
        public abstract Result Optimize();
        public Progress<Result> ProgressHandler
        {
            get
            {
                return progressHandler;
            }

            set
            {
                progressHandler = value;
            }
        }

    }
}
