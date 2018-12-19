using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MEPUI
{
    public class RequestViewModel
    {
        private string skills;
        public RequestViewModel()
        {
            this.startday = -1;
        }

        

        public string SkillsRequested
        {
            get
            {
                return this.skills;
            }
            set
            {
                this.skills = value;
            }
        }

        Location requestLocation;
        public SolidColorBrush Brush
        {
            get
            {
                if (this.startday == -1)
                    return new SolidColorBrush(Colors.Red);
                else
                    return new SolidColorBrush(Colors.Green);
            }

        }




        String city;

        public Thickness LeftMargin {
            get {
                return new Thickness(startday * 22,0,0,0);
            } 
        }

        public int Width
        {
            get
            {
                return Duration * 20;
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

        public string City
        {
            get
            {
                return city;
            }

            set
            {
                city = value;
            }
        }

        string description; 
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                this.description = value;
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

        int startday;
        int duration;
    }
}
