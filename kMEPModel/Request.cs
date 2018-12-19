using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace MEPModel
{
    public class Request: IComparable<Request>
    {
        int duration;
        List<int> possibleStartDays = new List<int>();
        List<String> skillsRequested = new List<String>();
        Location requestLocation;
        

        [JsonIgnore]
        List<Option> options = new List<Option>();

        

        public int CompareTo(Request other)
        {
            //if (this.selectedOption != null && other.selectedOption != null)
                return this.selectedOption.Startday.CompareTo(other.selectedOption.Startday);
            //else
            //    return -1;
        }

       
        public Option SelectedOption
        {
            get
            {
                //foreach (Option o in options)
                //    if (o.State == Option.OptionState.Selected)
                //        return o;
                return selectedOption;
            }
            set
            {
                selectedOption = value;
            }
        }
        private Option selectedOption;

        [JsonIgnore]
        public String Description
        {
            get {
                Option o = SelectedOption;
                String desc = this.RequestLocation.ToString() + " Duration" + Duration;
                if (o != null && o.E != null)
                {
                    desc += " Assigned Engineer" + o.E.Name;
                    desc += " Startday" + o.Startday;
                }
                return desc;
            }
        }

        public Request(int duration, Location requestLocation)
        {
            Duration = duration;
            RequestLocation = requestLocation;
            
        }
   
        public int Duration
        {
            get
            {
                return duration;
            }

            set
            {
                duration = value;
            }
        }

        public List<int> PossibleStartDays
        {
            get
            {
                return possibleStartDays;
            }

            set
            {
                possibleStartDays = value;
            }
        }

        public List<String> SkillsRequested
        {
            get
            {
                return skillsRequested;
            }

            set
            {
                skillsRequested = value;
            }
        }

        public Location RequestLocation
        {
            get
            {
                return requestLocation;
            }

            set
            {
                requestLocation = value;
            }
        }

        [JsonIgnore]
        public List<Option> Options
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
    }
}
