using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPModel
{
    public class Saving : IComparable<Saving>
    {
        Option a;
        Option b;
        double saving;

        public Saving(Option a, Option b, double saving)
        {
            this.a = a;
            this.b = b;
            this.saving = saving;
        }

        public Option A
        {
            get
            {
                return a;
            }

            set
            {
                a = value;
            }
        }

        public Option B
        {
            get
            {
                return b;
            }

            set
            {
                b = value;
            }
        }

        public double S
        {
            get
            {
                return saving;
            }

            set
            {
                saving = value;
            }
        }

        public int CompareTo(Saving other)
        {
            return this.saving.CompareTo(other.saving)*-1;
        }
    }
}
