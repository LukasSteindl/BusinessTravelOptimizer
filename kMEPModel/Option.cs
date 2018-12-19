using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPModel
{
    public class Option : IComparable<Option>
    {
        int id;
        Request r;
        Engineer e;
        int startday;
        double cost;
        OptionState state;
        List<Option> collidingOptions = new List<Option>();

      
        public enum OptionState { Available, Selected, Locked}

        public Option (int id, Engineer e, Request r, int startday) 
        {
            Id = id;
            E = e;
            R = r;
            Startday = startday;
            Cost = 0; 
            state = OptionState.Available;
        }

        [JsonIgnore]
        public Engineer E
        {
            get
            {
                return e;
            }

            set
            {
                e = value;
            }
        }

        public int Startday
        {
            get
            {
                return startday;
            }

            set
            {
                startday = value;
            }
        }

        [JsonIgnore]
        public int Endday
        {
            get
            {
                return startday + R.Duration-1;
            }
        }

        public double Cost
        {
            get
            {
                return cost;
            }

            set
            {
                cost = value;
            }
        }

        [JsonIgnore]
        public Request R
        {
            get
            {
                return r;
            }

            set
            {
                r = value;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public OptionState State
        {
            get
            {
                return state;
            }

            set
            {
                state = value;
            }
        }


        [JsonIgnore]
        public List<Option> CollidingOptions
        {
            get
            {
                return collidingOptions;
            }

            set
            {
                collidingOptions = value;
            }
        }

        [JsonIgnore]
        public string Description
        {
            get { return this.E.Name + this.startday + Environment.NewLine + "Cost:" + this.Cost; }
        }

        public int CompareTo(Option other)
        {
            if (this == other)
                return 1;
            else
                return this.Cost.CompareTo(other.Cost)*-1;
         }
    }
}
