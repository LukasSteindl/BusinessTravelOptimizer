using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPModel
{
    public class Route
    {
        Location from;
        Location to;
        int hotelnights;
        //double distance;
        double cost;

        public Route(Location from, Location to, int hotelnights, //double distance,
            double cost)
        {
            this.from = from;
            this.to = to;
            this.hotelnights = hotelnights;
            //this.distance = distance;
            this.cost = cost;
        }

        public Location From
        {
            get
            {
                return from;
            }

            set
            {
                from = value;
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

        //public double Distance
        //{
        //    get
        //    {
        //        return distance;
        //    }

        //    set
        //    {
        //        distance = value;
        //    }
        //}

        public int Hotelnights
        {
            get
            {
                return hotelnights;
            }

            set
            {
                hotelnights = value;
            }
        }

        public Location To
        {
            get
            {
                return to;
            }

            set
            {
                to = value;
            }
        }
    }
}
