using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MEPModel
{
    public class Engineer
    {
        String name;
        //List<Request> assignedRequests = new List<Request>();
        SortedList<int, Request> assignedRequests = new SortedList<int, Request>();
        List<String> skills = new List<String>();
        Location homeOffice;
        MEP model;


        public Engineer (String name,Location homeOffice,MEP model)
        {
            Name = name;
            HomeOffice = homeOffice;
            this.Model = model;
           
        }

        public double cost_of_requests()
        {
            double cost = 0;
            //kosten zum ersten und vom letzten request
            //es wird angenommen, dass die Anreise immer so erfolgt, dass kein Hotel nötig ist!
            if (AssignedRequests.Count > 0)
            {
               cost += model.cost_between_request_and_home(AssignedRequests.Values[0],this); 
               cost += model.cost_between_request_and_home(AssignedRequests.Values[AssignedRequests.Count - 1],this);
            }
            //kosten für die optimalen routen zwischen den reisestationen:
            for (int i = 0; i < AssignedRequests.Count - 1; i++)
            {
                foreach (Route r in optimalRoute_between_Options(AssignedRequests.Values[i].SelectedOption, AssignedRequests.Values[i + 1].SelectedOption))
                {
                    cost += r.Cost;
                }
            }
            return cost;
        }

       

        //gibt zurück wie sich die Kosten verändern wenn man die Option zusätzlich aufnimmt:
        public double probeOption(Option o)
        {
            //debug:
            //foreach (Request r in assignedRequests.Values)
            //    Console.WriteLine("Assigned:" + r.Description);
            //Console.WriteLine("New:" + o.R.Description);

            //gibt zurück wieviel mehr es kosten würde diese Option zu wählen
            this.AssignedRequests.Add(o.Startday,o.R);
            int index = this.AssignedRequests.IndexOfKey(o.Startday);
            Request left = null;
            Request right = null;
            if (index > 0)
                left = this.AssignedRequests.Values[index - 1];
            if (index < this.AssignedRequests.Count-1)
                right = this.AssignedRequests.Values[index + 1];

            //entferne den hypothetischen Request wieder: 
            this.AssignedRequests.Remove(o.Startday);

            //ermittle wie hoch die ab und anreisekosten zuvor waren!
            //zwischen linkem und rechtem request            
            double original_cost = 0;
            if (left != null && right != null)
                foreach (Route r in optimalRoute_between_Options(left.SelectedOption, right.SelectedOption))
                {
                    original_cost += r.Cost;
                }

            //bzw zuvor zum ersten request
            if (left == null && right != null)
            {
                original_cost += model.cost_between_request_and_home(right,this);
            }
            //bzw zuvor vom letzten request
            if (left != null && right == null)
            {
                original_cost += model.cost_between_request_and_home(left,this);
            }

            //ermittle neue kosten:
            double cost = 0;
            //wenn ganz links eingefügt wurde, reise von zuhause an
            if (left == null)
            {
                cost += model.cost_between_request_and_home(o.R,this);
            }
            else //falls es einen linken nachbar gibt ermittle kosten von diesem zur aktuellen option
            {
                foreach (Route r in optimalRoute_between_Options(left.SelectedOption, o))
                {
                    cost += r.Cost;
                }
            }


            //wenn ganz am ende eingefügt wurde, reise danach nach hause
            if (right == null)
            {
                cost += model.cost_between_request_and_home(o.R,this);
            }
            else //falls es einen rechten nachbar gibt ermittle kosten zu diesem
            {
                foreach (Route r in optimalRoute_between_Options(o, right.SelectedOption))
                {
                    cost += r.Cost;
                }
            }

            
           
            //gib die Deltakosten zurück!
            return cost-original_cost;
        }

        //Prüft ob es günstiger ist direkt weiterzufahren oder zwischendurch nach Hause zu fahren zwischen 
        //zwei Einsätzen:
        public List<Route> optimalRoute_between_Options(Option A, Option B)
        {
            List<Route> routes = new List<Route>();
          
            double distance_between_Requests = model.get_distance(A.R.RequestLocation, B.R.RequestLocation);
            double travelduration_between_requests = distance_between_Requests / model.AvgTravelSpeed;
            int days_between_A_and_B = B.Startday - A.Endday;


            double cost_of_going_home_from_A = model.cost_between_request_and_home(A.R, this);
            double cost_of_going_from_home_to_B = model.cost_between_request_and_home(B.R,this);

            //wertmäßige kosten + pagatorische kosten:
            double cost_of_direkt_travel = travelduration_between_requests * model.HourlyWage + distance_between_Requests * model.MilageAllowance + days_between_A_and_B*model.HotelCostPerNight;
            double cost_of_going_home_in_between = cost_of_going_home_from_A + cost_of_going_from_home_to_B; //travelduration_A_Home_B * model.HourlyWage + totaldistance_when_going_home_in_between * model.MilageAllowance;


            if (cost_of_direkt_travel > cost_of_going_home_in_between)
                routes.Add(new Route(A.R.RequestLocation, B.R.RequestLocation, days_between_A_and_B, cost_of_direkt_travel));
            else
            {
                routes.Add(new Route(A.R.RequestLocation, HomeOffice, 0, cost_of_going_home_from_A));
                routes.Add(new Route(HomeOffice, B.R.RequestLocation, 0 , cost_of_going_from_home_to_B));
            }

            return routes;
        }

     

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public SortedList<int,Request> AssignedRequests
        {
            get
            {
                return assignedRequests;
            }

            set
            {
                assignedRequests = value;
            }
        }

        public List<String> Skills
        {
            get
            {
                return skills;
            }

            set
            {
                skills = value;
            }
        }

        public Location HomeOffice
        {
            get
            {
                return homeOffice;
            }

            set
            {
                homeOffice = value;
            }
        }

        



        [JsonIgnore]
        public MEP Model
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

     
        

        
    }
}
