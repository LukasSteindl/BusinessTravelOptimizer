using Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Threading;

namespace MEPModel
{
    public class MEP : ISolvable
    {
        private double milageAllowance;
        private int avgTravelSpeed; 
        private int hourlyWage; 
        private int hotelCostPerNight; 
        private int revenuePerDayOnsite;
        double totalCost;


        private CancellationToken cancelToken;

        public MEP()
        {
            milageAllowance = -2;
            avgTravelSpeed = 60; //60km/h
            hourlyWage = -60; //to value the travel time
            hotelCostPerNight = -150; //-120 Hotel -30 Dieten
            RevenuePerDayOnsite = 1640; //for profit subtract hourlywage and travelcost (milage + valued traveltime + hotel if applicable)
            
        }

      
       


        List<String> skills = new List<String>();
        List<Engineer> engineers = new List<Engineer>();

        List<int> sequence = new List<int>();
        List<Location> locations = new List<Location>();

        double[,] distances;
        bool allowTripContinuation = false;
        bool allowUnprofitableRequests = true;


        [JsonIgnore]
        List<Request> requests = new List<Request>();
        [JsonIgnore]
        Dictionary<int, Option> options = new Dictionary<int, Option>();

        //bool[,] kollisions;

        int modelEvaluations;
        public int ModelEvaluations
        {
            get
            {
                return modelEvaluations;
            }
            set
            { //benötigt setter für JSON Serialisierung
                this.modelEvaluations = value;
            }
        }



        [JsonIgnore]
        public CancellationToken CancelToken
        {
            get
            {
                return cancelToken;
            }

            set
            {
                this.cancelToken = value;
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

        public List<Engineer> Engineers
        {
            get
            {
                return engineers;
            }

            set
            {
                engineers = value;
            }
        }

        public List<Request> Requests
        {
            get
            {
                return requests;
            }

            set
            {
                requests = value;
            }
        }

        [JsonIgnore]
        public Dictionary<int,Option> Options
        {
            get
            {
                return options;
            }

            set
            {
                options = value;
            }
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

        public List<Location> Locations
        {
            get
            {
                return locations;
            }

            set
            {
                locations = value;
            }
        }

        public double MilageAllowance
        {
            get
            {
                return milageAllowance;
            }

            set
            {
                milageAllowance = value;
            }
        }

        public bool AllowTripContinuation
        {
            get
            {
                return allowTripContinuation;
            }

            set
            {
                allowTripContinuation = value;
            }
        }


        private bool permutateOptions;


        public double[,] Distances
        {
            get
            {
                return distances;
            }

            set
            {
                distances = value;
            }
        }

        public int AvgTravelSpeed
        {
            get
            {
                return avgTravelSpeed;
            }

            set
            {
                avgTravelSpeed = value;
            }
        }

        public int HourlyWage
        {
            get
            {
                return hourlyWage;
            }

            set
            {
                hourlyWage = value;
            }
        }

        public int HotelCostPerNight
        {
            get
            {
                return hotelCostPerNight;
            }

            set
            {
                hotelCostPerNight = value;
            }
        }

        public int RevenuePerDayOnsite
        {
            get
            {
                return revenuePerDayOnsite;
            }

            set
            {
                revenuePerDayOnsite = value;
            }
        }

        public bool AllowUnprofitableRequests
        {
            get
            {
                return allowUnprofitableRequests;
            }

            set
            {
                allowUnprofitableRequests = value;
            }
        }

        public bool PermutateOptions
        {
            get
            {
                return permutateOptions;
            }

            set
            {
                permutateOptions = value;
            }
        }

        private void Compute_Options()
        {
            Options.Clear();

            //compute options:
            int optionid = 0;
            foreach (Request r in Requests)
            {
                r.Options.Clear();
                foreach (Engineer e in Engineers)
                    if (IsQualified(r.SkillsRequested, e.Skills))
                    {
                        foreach (int possibleStartday in r.PossibleStartDays)
                        {
                            Option o = new Option(optionid, e, r, possibleStartday);
                            if (!AllowTripContinuation)
                            {
                                o.Cost = cost_between_request_and_home(r, e) * 2; //*2 mal hin und Rückreise
                            }
                            else
                            {
                                //falls es sich auszahlt, fahre jeden tag nach hause sonst bleibe im Hotel.
                                //Anzahl der Anfahrten und Rückfahrten falls man nicht im Hotel bleibt:
                                //ignoriert die erste Anfahrt und die letzte Rückfahrt! (dies wird aufgrund der benachbarten
                                //Requests berechnet!
                                int number_of_roundtrips = r.Duration - 1; //entspricht Anzahl der Hotelnächte falls man nicht jeden Tag nachhause fährt!
                                double hotel_cost = number_of_roundtrips * HotelCostPerNight;
                                double goingHomeCost = cost_between_request_and_home(r, e) * number_of_roundtrips * 2;
                                if (hotel_cost > goingHomeCost)
                                    o.Cost = hotel_cost;
                                else
                                    o.Cost = goingHomeCost; //kleiner fehler noch! aktuell werden dieten nicht richtig berücksichtigt
                            }

                            //Ertrag addieren! 
                            o.Cost += r.Duration * RevenuePerDayOnsite;
                            options.Add(optionid,o);
                            r.Options.Add(o);
                            optionid += 1;
                        }
                    }

                //no_delivery_option?
                //ist nicht nötig, da bei der SetSequence auch entschieden werden kann,
                //dass falls ein request nicht gewinnbringend (zu einer gegebenen Situation) 
                //hinzugenommen werden kann, keine Option ausgewählt wird
                //var no_delivery_option = new MEPModel.Option(optionid, null, r, -1);
                //no_delivery_option.Cost = 0; //kein Ertrag!
                //r.Options.Add(no_delivery_option);
                //optionid += 1;
                //sort Options by Cost
                r.Options.Sort();
            }

            //  kollisions = new bool[Options.Count, Options.Count];
            //compute collisions

            Parallel.ForEach(Options.Values, o1 =>
            {
                foreach (Option o2 in Options.Values)
                    if (o1 != o2)
                        if (ComputeKollision(o1, o2))
                            o1.CollidingOptions.Add(o2);
            });
            // foreach (Option o1 in Options)
               

        }

        public double cost_between_request_and_home(Request r, Engineer e)
        {

            var distance = get_distance(e.HomeOffice, r.RequestLocation);
           
            var traveltime = distance / AvgTravelSpeed;
            return traveltime * HourlyWage + distance * MilageAllowance;
        }

        public double get_distance(Location from, Location to)
        {
            return Distances[Locations.IndexOf(from), Locations.IndexOf(to)];
        }

        //muss nur noch erweitert werden um auch optionen gegeneinander auszuschließen
        //die sich aufgrund der reisedistanz zwischen ihnen nicht ausgehen können
        //aufgrund des dreieckssatzes reicht es aus die distanz zwischen den optionen
        //zu prüfen und nicht die distanz zwischen den optionen und dem Homeoffice einzubeziehen
        //es sollen nicht mehr als 4 Stunden reisezeit nach dem letzten Termin erlaubt sein.
        //sonst sind die optionen nicht kompatibel! im weiteren kann der optimizer dann
        //aus zwei optionen wählen die kompatibel sind und wo ein tag dazwischen liegt
        //dabei wird abgewogen ob es günstiger ist nachhause zu fahren und neu anzureisen
        //oder einen tag in einem Hotel am abfahrtsort oder am Zielort zu verbringen.
        private bool ComputeKollision(Option o1, Option o2)
        {
            //optionen des selben requests kollidieren immer! 
            if (o1.R == o2.R)
                return true;

            if (o1.E == o2.E && ((o1.Startday >= o2.Startday && o1.Startday <= o2.Endday)
                || o2.Startday >= o1.Startday && o2.Startday <= o1.Endday))
                return true;
            else
                return false;
        }

        private bool IsQualified(List<String> skillsrequested, List<String> skilloffered)
        {
            foreach (String requestedSkill in skillsrequested)
                if (!skilloffered.Contains(requestedSkill))
                    return false;
            return true;

        }


        public double GetCost()
        {
                return totalCost;
        }

        public int GetNumberOfElements()
        {
            //bei Weiterfahrt ist es nötig alle optionen durchzuprobieren!
            if (permutateOptions)
                return options.Count();
            return requests.Count;
        }
        public void SetSequence (List<int> sequence)
        {
            modelEvaluations += 1;
            totalCost = 0;
            this.sequence = sequence;

            //zu Beginn sind alle Optionen verfügbar:
            SetAllOptionsAvailable();

            if (AllowTripContinuation)
            {
                //mit weiterfahrt müssten alle Optionssequenzen getestet werden
                if (permutateOptions) 
                    SetOptionSequence(sequence);
                else
                    SetRequestSequence(sequence);
            }
            else
            {
                //ohne weiterfahrt reicht es alle permutationen aus requests zu prüfen
                if (permutateOptions)
                    SetOptionSequence(sequence);
                else
                    SetSequenceSimple(sequence);
            }

        }

        private void SetRequestSequence(List<int> Sequence)
        {
            foreach (int rid in sequence)
            {
                Option cheapest = null;
                double minadditional_cost = double.MinValue;
                double added_cost = 0;
                //double added_cost_m1 = 0;
                Request r = Requests[rid];
                r.SelectedOption = null;
                //ermittle günstigste, verfügbare Option
                foreach (Option o in r.Options)
                    if (o.State == Option.OptionState.Available)
                    {
                        //bei jedem in frage kommenden Engineer die Optionen prüfen:
                        //delta in distance: 
                        //Schnell aber riskant:
                        added_cost = o.E.probeOption(o) + o.Cost;

                        //Langsam aber sicher: verbleibt im Code zum Vergleich!
                        //double distance_without_option = o.E.cost_of_requests();
                        //o.E.AssignedRequests.Add(o.Startday, r);
                        //r.SelectedOption = o;
                        //double distance_with_option = o.E.cost_of_requests();
                        //o.E.AssignedRequests.Remove(o.Startday);
                        //r.SelectedOption = null;
                        //added_cost = (distance_with_option - distance_without_option) + o.Cost;

                        //if (Math.Abs(added_cost-added_cost_m1) > 1)
                        //{
                        //    Console.WriteLine("Unterschied! " + (added_cost - added_cost_m1));
                        //    foreach (Request rx in o.E.AssignedRequests.Values)
                        //        Console.WriteLine(rx.Description);
                        //    Console.WriteLine(o.R.Description);
                        //}
                        if (added_cost > minadditional_cost)
                        {
                            minadditional_cost = added_cost;
                            cheapest = o;
                        }
                    }

                //setze günstigste Option
                if (cheapest != null)
                {
                    if (added_cost > 0 || AllowUnprofitableRequests)
                    {
                        //Console.WriteLine(cheapest.E.Name + " " + cheapest.R.RequestLocation.City + " " + minadditional_cost);
                        cheapest.State = Option.OptionState.Selected;
                        cheapest.E.AssignedRequests.Add(cheapest.Startday, r);
                        r.SelectedOption = cheapest;
                        totalCost += minadditional_cost;
                        //lock conflicting options
                        foreach (Option co in cheapest.CollidingOptions)
                            co.State = Option.OptionState.Locked;
                    }
                }
            }
        }


        private void SetAllOptionsAvailable()
        {
            //eine Art leichtgewichtiges Model init() da hier nur die gesetzten Optionen freigegeben werden
            //nicht aber die Optionen neu berechnet werden
            foreach (Option o in Options.Values)
                o.State = Option.OptionState.Available;
            //und kein Request zugeordnet
            foreach (Engineer e in Engineers)
            {
                e.AssignedRequests.Clear();
            }
        }

        //berechnet eine vollständige sequenz
        private void SetSequenceSimple(List<int> sequence)
        {
            //die Sequenz gibt an in welcher Reihenfolge für jeden Workshop die jeweils beste 
            //verbleibende Option zugeordnet werden soll. 
            foreach (int rid in sequence)
            {
                Request r = Requests[rid];
                r.SelectedOption = null;
              
                foreach (Option o in r.Options)
                {
                    if (o.State == Option.OptionState.Available && (o.Cost > 0 || AllowUnprofitableRequests))
                    {
                        o.State = Option.OptionState.Selected;
                        totalCost += o.Cost;
                        o.E.AssignedRequests.Add(o.Startday,r);
                        r.SelectedOption = o;
                        //lock conflicting options
                        foreach (Option co in o.CollidingOptions)
                            co.State = Option.OptionState.Locked;
                        break;
                    }
                }
            }
        }

        private void SetOptionSequence(List<int> Sequence)
        {
            //für das MEP mit Weiterfahrt genügt es nicht nur alle Permutationen aus Requests zu betrachten.
            //stattdessen müssen alle Permutationen aller Optionen betrachtet werden.
            double cost = 0;
            foreach (int oid in sequence)
            {
                var o = this.options[oid];


                if (o.State == Option.OptionState.Available)
                {
                    //evaluieren ist nun schneller als vorher da die option einfach hinzugenommen wird! 
                    cost += select_option(o);
                }

            }
            totalCost = cost;
        }

        private double select_option(Option o)
        {
            var cost = o.E.probeOption(o) + o.Cost;
            o.E.AssignedRequests.Add(o.Startday, o.R);
            o.R.SelectedOption = o;
            o.State = Option.OptionState.Selected;
            foreach (Option oc in o.CollidingOptions)
            {
                oc.State = Option.OptionState.Locked;
            }
            return cost;
        }

        private double unselect_option(Option o) 
        {
            if (o != null)
            { 
            o.State = Option.OptionState.Available;
            o.R.SelectedOption = null;
            o.E.AssignedRequests.Remove(o.Startday);

            //setzt die gewählte option auf verfügbar und ggf. auch die Optionen die mit der Option o in Konflikt standen
            //sofern diese nicht mit anderen Optionen in Konflikt stehen
            foreach (Option co in o.CollidingOptions)
            {
                bool has_other_collisions = false;
                foreach (Option co_other in co.CollidingOptions)
                {
                    if (co_other.State == Option.OptionState.Selected)
                    {
                        has_other_collisions = true;
                        break;
                    }
                }
                if (!has_other_collisions)
                    co.State = Option.OptionState.Available; //gib option frei wenn sie sonst keine Kollisionen hat! 
            }
         
            return (o.E.probeOption(o) + o.Cost) * -1; //gibt zurück wie sich der gewinn durch abwahl der option verändert!
            }
            return 0;
        }

        private List<Option> get_free_options_of_request(Request r, Option originalSelected)
        {
            var free_options = new List<Option>();
            foreach (Option o in r.Options)
                if (o.State == Option.OptionState.Available && o != originalSelected)
                    free_options.Add(o);

            return free_options;
        }

        public void undo_last_random_option_change()
        {
            double costdelta = 0;
            costdelta += unselect_option(new_selected_option);
            costdelta += select_option(original_selected_option);
            totalCost += costdelta;

            SwapOptions(original_selected_option, new_selected_option);
        }
    
        Option original_selected_option;
        Option new_selected_option;
        public List<int> random_option_change()
        {
            //wählt einen zufälligen request aus
            var r = new Random();
            var req = this.Requests[r.Next(0, Requests.Count)];
            original_selected_option = req.SelectedOption;
            double costdelta = 0;

            //wenn es eine gewählte option gibt, wähle sie ab:
            if (original_selected_option != null)
            {
                costdelta += unselect_option(req.SelectedOption);
            }
            //kann erst nach der deselektion ermittelt werden! sonst wird sie durch die eigene option gesperrt!
            var free_options = get_free_options_of_request(req, original_selected_option);

            //wählt eine beliebige andere oder keine Option aus:
            if (free_options.Count > 0)
            {
                var o = free_options[r.Next(0, free_options.Count)];
                costdelta += select_option(o);
                new_selected_option = o;
                totalCost += costdelta;
            }
            else
            {
                new_selected_option = null;

            }
            //verändert Optionen in Optionssequenz 
            SwapOptions(original_selected_option, new_selected_option);

            return sequence;
        }

        //A soll immer die original Option und B die neue Option sein!
        private void SwapOptions(Option A, Option B)
        {
            int BID;
            int index_of_B_option;
            int AID;
            int index_of_A_option;

            if (A != null)
            {
                AID = A.Id;
               
            }
            else
            {
                AID = this.options.Count - 1;
            }
          
            if (B != null)
            {
               BID = B.Id;
            }
            else //falls B null ist wird A einfach an das Ende der Sequenz gestellt 
            {    //dadurch wird die Option A als letztes versucht!
                BID = this.options.Count-1;               
            }
            index_of_B_option = sequence.IndexOf(BID);
            index_of_A_option = this.sequence.IndexOf(AID);
            this.sequence[index_of_A_option] = BID;
            this.sequence[index_of_B_option] = AID;

        }


        public List<int> get_savings_heuristic_sequence()
        {
            SetAllOptionsAvailable();
            totalCost = 0;
            List<int> sequence = new List<int>();
            List<Saving> savings = new List<MEPModel.Saving>();

            foreach (Request r in Requests)
                r.SelectedOption = null;

            foreach (Option o in options.Values)
                foreach (Option o2 in options.Values)
                    if (o.E == o2.E && o != o2 && !ComputeKollision(o, o2) && o2.Startday >= o.Endday)
                        savings.Add(new Saving(o, o2, get_saving(o.E, o, o2)));

            savings.Sort();
            foreach (Saving s in savings)
            {
                //wenn die beteiligten Optionen noch nicht zugeordnet wurden:
                if (s.A.State == Option.OptionState.Available)
                {
                    this.totalCost += select_option(s.A);
                    sequence.Add(s.A.Id);
                }

                if (s.B.State == Option.OptionState.Available)
                {
                    this.totalCost += select_option(s.B);
                    sequence.Add(s.B.Id);
                }
            }
            return sequence;
        }

        public List<int> get_savings_heuristic_sequence_v1()
        {
            //wird schon im Solver gemacht! 
            //this.InitModel(); 
            SetAllOptionsAvailable();
            totalCost = 0;
            List<int> sequence = new List<int>();
            List<Saving> savings = new List<MEPModel.Saving>();
            //für jeden Mitarbeiter wird 
            foreach (Engineer e in this.engineers)
            {
                //reset assigned requests at the same time.
                e.AssignedRequests.Clear();
                var engineerOptions = get_options_of_Engineer(e);
                //für alle Optionen an denen er teilnimmt und
                //prüfe paarweise den savingswert
                foreach (Option o in engineerOptions)
                {
                    foreach (Option o2 in engineerOptions)
                    {
                        //wenn optionen nicht die selben sind und keine konflikte bestehen...
                        if (o != o2 && !ComputeKollision(o, o2))
                            savings.Add(new Saving(o, o2, get_saving(e, o, o2)));
                    }
                }
            }
            savings.Sort();
            //konstruiere route indem man der reihe nach alle savings betrachtet
            //und die Requests setzt die darin enthalten sind sofern sie noch verfügbar sind!
            foreach (Saving s in savings)
            {
                //wenn noch keiner der beiden beteiligten Requests zugeordnet wurde: 
                if (s.A.State == Option.OptionState.Available && s.B.State == Option.OptionState.Available)
                {
                 
                    sequence.Add(requests.IndexOf(s.A.R));
                    sequence.Add(requests.IndexOf(s.B.R));
                    s.A.State = Option.OptionState.Selected;
                    s.B.State = Option.OptionState.Selected;
                    s.A.R.SelectedOption = s.A;
                    s.B.R.SelectedOption = s.B;
                    foreach (Option co in s.A.CollidingOptions)
                        co.State = Option.OptionState.Locked;
                    foreach (Option co in s.B.CollidingOptions)
                        co.State = Option.OptionState.Locked;
                    this.totalCost += s.A.E.probeOption(s.A) + s.A.Cost;
                    this.totalCost += s.B.E.probeOption(s.B) + s.B.Cost;

                    s.A.E.AssignedRequests.Add(s.A.Startday, s.A.R);
                    s.B.E.AssignedRequests.Add(s.B.Startday, s.B.R);
                    //Console.WriteLine("RequestA:" + s.A.R.Description);
                    //Console.WriteLine("RequestB:" + s.B.R.Description);
                }
                //wenn einer der beiden beteiligten Requests zugeordnet wurde und der andere nicht:
                //wenn der Request von B schon zugeordnet wurde, der Request von A aber nicht: 
                if (s.A.State == Option.OptionState.Available && s.B.State != Option.OptionState.Available)
                {
                    sequence.Add(requests.IndexOf(s.A.R));
                    //füge A zum Engineer des Requests B (sollte der selbe Engineer sein wie der aus A!) 
                    //kosten ermitteln:
                    this.totalCost += s.B.E.probeOption(s.A);
                    //in kalender einfügen
                    s.B.E.AssignedRequests.Add(s.A.Startday,s.A.R);
                    //optionsstate setzen
                    s.A.State = Option.OptionState.Selected;
                    //gesetzt Option merken
                    s.A.R.SelectedOption = s.A;
                    //kollidierende Optionen sperren:
                    foreach (Option co in s.A.CollidingOptions)
                        co.State = Option.OptionState.Locked;
                    //Console.WriteLine("Zu bestehender Hinzu:" + s.A.R.Description);

                }

                //wenn also A schon zugeordnet B aber nicht: 
                if (s.B.State == Option.OptionState.Available && s.A.State != Option.OptionState.Available)
                {
                    sequence.Add(requests.IndexOf(s.B.R));
             
                    //füge A zum Engineer des Requests B (sollte der selbe Engineer sein wie der aus A!) 
                    //kosten ermitteln:
                    this.totalCost += s.A.E.probeOption(s.B);
                    //in kalender einfügen
                    s.A.E.AssignedRequests.Add(s.B.Startday, s.B.R);
                    //optionsstate setzen
                    s.B.State = Option.OptionState.Selected;
                    //gesetzt Option merken
                    s.B.R.SelectedOption = s.B;
                    //kollidierende Optionen sperren:
                    foreach (Option co in s.B.CollidingOptions)
                        co.State = Option.OptionState.Locked;
                    //Console.WriteLine("Zu bestehender Hinzu:" + s.B.R.Description);
                }
              
                //wenn beide beteiligten Requests bereits in einer Route sind -> mache nichts! 

            }
            return sequence;

        }

        private List<Option> get_options_of_Engineer(Engineer e)
        {
            var EngineerOptions = new List<Option>();
            foreach (Option o in Options.Values)
            {
                if (o.E == e)
                    EngineerOptions.Add(o);
            }
            return EngineerOptions;
        }

        private double get_saving (Engineer E, Option A, Option B)
        {
            //berechnet die einsparung wenn man von Option a nach Option b weiterfährt statt
            //zwischendurch nach hause zu fahren
            //beim MEP muss dazu in EURO und nicht in Kilometer gerechnet werden
            //und es müssen die Übernachtungskosten einberechnet werden!

            double distance_between_Requests = get_distance(A.R.RequestLocation, B.R.RequestLocation);
            double travelduration_between_requests = distance_between_Requests / AvgTravelSpeed;
            int days_between_A_and_B = B.Startday - A.Endday;


            double cost_of_going_home_from_A = cost_between_request_and_home(A.R, E);
            double cost_of_going_from_home_to_B = cost_between_request_and_home(B.R, E);

            //wertmäßige kosten + pagatorische kosten:
            double cost_of_direkt_travel = travelduration_between_requests * HourlyWage + distance_between_Requests * MilageAllowance + days_between_A_and_B * HotelCostPerNight *-1;
            double cost_of_going_home_in_between = cost_of_going_home_from_A + cost_of_going_from_home_to_B *-1;//positive
            //zb.nachhause gehen kostet 100
            //weiterfahren kostet 40
            //saving = 60 

            //ersparnis durch weiterreise:
            return (cost_of_going_home_in_between - cost_of_direkt_travel);

        }

        private Option get_best_next_option()
        {
            Option cheapest = null;
            double minadditional_cost = double.MinValue;
            double added_cost = 0;

            foreach (Option o in options.Values)
            {
                if (o.State == Option.OptionState.Available)
                {
                    added_cost = o.E.probeOption(o) + o.Cost;
                    if (added_cost > minadditional_cost)
                    {
                        minadditional_cost = added_cost;
                        cheapest = o;
                    }
                }
            }
            return cheapest;
        }

        public List<int> get_insertion_heuristic_sequence()
        {
            SetAllOptionsAvailable();
            List<int> optionsequence = new List<int>();

            //zusätzlich die Referenz auf die gewählten Optionen aus den Requests löschen!
            foreach (Request r in Requests)
                r.SelectedOption = null;


            //wähle die erste Option aus:
            this.totalCost += select_option(options[0]);
            optionsequence.Add(options[0].Id);

            var nextOption = get_best_next_option();
            while (nextOption != null)
            {
                this.totalCost += select_option(nextOption);
                optionsequence.Add(nextOption.Id);
                nextOption = get_best_next_option();
            }
            return optionsequence;
        }


        public List<int> get_insertion_heuristic_sequence_v1()
        {
            //setzt alle Optionen wieder auf verfügbar!
            SetAllOptionsAvailable();
            //zusätzlich die Referenz auf die gewählten Optionen aus den Requests löschen!
            foreach (Request r in Requests)
                r.SelectedOption = null;

            List<Request> inserted = new List<Request>();

            int no_option_found = 0;
            //solange es noch requests gibt die nicht eingefügt wurden
            while (inserted.Count + no_option_found < this.requests.Count) 
            {
                Option cheapest = null;
                double minadditional_cost = double.MinValue;
                double added_cost = 0;
                //ermittle den Request dessen Option am besten zu den bereits geplanten Routen passt
                foreach (Request r in this.requests)
                {
                    if (r.SelectedOption == null)
                    {
                        
                        foreach (Option o in r.Options)
                        {
                            if (o.State == Option.OptionState.Available)
                            {
                                added_cost = o.E.probeOption(o) + o.Cost;
                                if (added_cost > minadditional_cost)
                                {
                                    minadditional_cost = added_cost;
                                    cheapest = o;
                                }
                            }
                        }
                    }
                }
                if (cheapest != null)
                {
                    cheapest.State = Option.OptionState.Selected;
                    cheapest.E.AssignedRequests.Add(cheapest.Startday, cheapest.R);
                    //lock conflicting options
                    foreach (Option co in cheapest.CollidingOptions)
                        co.State = Option.OptionState.Locked;
                    cheapest.R.SelectedOption = cheapest;
                    totalCost += minadditional_cost;
                    inserted.Add(cheapest.R);
                }
                else
                    no_option_found += 1;
            }

            List<int> sequence = new List<int>();
            foreach (Request r in inserted)
                sequence.Add(this.Requests.IndexOf(r));
            return sequence;

        }

       

        public List<int> GetSequence()
        {
            return this.Sequence;
        }

        public string GetModelString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);   
        }

        public void LoadModelString(String ProblemInstance)
        {
            JsonConvert.PopulateObject(ProblemInstance, this);
        }

        public void InitModel()
        {
            modelEvaluations = 0;
            //setzt Locations wieder zurück
            Reset_Locations();
            Compute_Distances();
            Compute_Options();
        }

        private void Reset_Locations()
        {
            locations.Clear();
            foreach (Request r in Requests)
            {
                if (!locations.Contains(r.RequestLocation))
                    locations.Add(r.RequestLocation);
            }
            foreach (Engineer e in Engineers)
            {
                e.Model = this;
                if (!locations.Contains(e.HomeOffice))
                    locations.Add(e.HomeOffice);
            }

        }
        private void Compute_Distances()
        {
            Distances = new double[locations.Count, locations.Count];

            Parallel.ForEach(locations, l =>
            {
                foreach (Location l2 in locations)
                    Distances[locations.IndexOf(l), locations.IndexOf(l2)] = DistanceBetweenPlaces(l.Longitude, l.Latitude, l2.Longitude, l2.Latitude);

            });

                //foreach (Location l in locations)
                //foreach (Location l2 in locations)
                //    Distances[locations.IndexOf(l), locations.IndexOf(l2)] = DistanceBetweenPlaces(l.Longitude, l.Latitude, l2.Longitude, l2.Latitude);
        }

        public static double DistanceBetweenPlaces(double lon1, double lat1, double lon2, double lat2)
        {
            double R = 6371; // km

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));
            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;
            double d = Math.Acos(cosD);
            double dist = R * d;
            return dist;
        }

        private static double Radians(double degrees)
        {
            return (0.017453292519943295 * degrees);
        }
        
        public object getModelClone()
        {
            MEP clone = new MEPModel.MEP();
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(this), clone);
            //kopiere optionen da sie aufgrund einer zyklischer referenzen vom serialization framework 
            //ausgeschlossen wurden. Verweise dabei auf die korrespondieren Engineer und Request Objekte des Clons!

            //foreach (var eclone in clone.engineers)
            //    eclone.AssignedRequests.Clear();


            foreach (var o in options.Values)
            {
                
                var rclone = clone.Requests[this.Requests.IndexOf(o.R)];
                var eclone = clone.Engineers[this.Engineers.IndexOf(o.E)];
                var oclone = new MEPModel.Option(o.Id, eclone, rclone, o.Startday);
                oclone.Cost = o.Cost;
                rclone.Options.Add(oclone);
                clone.Options.Add(o.Id,oclone);
                
            }

            //wie im original werden optionen innerhalb der requests sortiert nach kosten!
            foreach (var r in clone.Requests)
                r.Options.Sort();

            //übernimm optionskollisionenen
            foreach (var o in options.Values)
                foreach (var co in o.CollidingOptions)
                {
                    clone.options[o.Id].CollidingOptions.Add(clone.options[co.Id]);
                }



            clone.Reset_Locations();
            return (object)clone;
        }

        
    }
}
