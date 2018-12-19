using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MEPUI
{
    public class MainViewModel: INotifyPropertyChanged
    {



        public ObservableCollection<OptionViewModel> Routes { get; set; }
        public ObservableCollection<EngineerViewModel> Engineers { get; set; }

        public ObservableCollection<RequestViewModel> Requests { get; set; }

        private String sequence;
        private String cost;
        public String Sequence { get { return sequence; }
            set
            {
                this.sequence = value;
                NotifyPropertyChanged("Sequence");
            }
        }


        private double milageAllowance= -1;
        private int avgTravelSpeed = 400;
        private int hourlyWage = -60;
        private int hotelCostPerNight = -120;
        private int revenuePerDayOnsite = 1640;

    
        public string Cost
        {
            get
            {
                return cost;
            }

            set
            {
                cost = value;
                NotifyPropertyChanged("Cost");
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
                NotifyPropertyChanged("MilageAllowanceDisplay");
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
                NotifyPropertyChanged("AvgTravelSpeedDisplay");
            }
        }

        public String AvgTravelSpeedDisplay
        {
            get { return "Ø Reisegeschwindigkeit: " + Environment.NewLine + AvgTravelSpeed.ToString() + " km/h"; }
        }

        public String HourlyWageDisplay
        {
            get { return "Ø Stundenlohn: " + Environment.NewLine + HourlyWage.ToString() + " EUR/h"; }
        }

        public String MilageAllowanceDisplay
        {
            get { return "Kilometergeld: " +Environment.NewLine + MilageAllowance.ToString() + " EUR/km"; }
        }

        public String HotelCostPerNightDisplay
        {
            get { return "Hotelkosten: " + Environment.NewLine + HotelCostPerNight.ToString() + " EUR/Nacht"; }
        }

        public String RevenuePerDayOnsiteDisplay
        {
            get { return "Gewinn pro Tag beim Kunden: " + Environment.NewLine + RevenuePerDayOnsite.ToString() + " EUR/Mitarbeiter"; }
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
                NotifyPropertyChanged("HourlyWageDisplay");
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
                NotifyPropertyChanged("HotelCostPerNightDisplay");
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
                NotifyPropertyChanged("RevenuePerDayOnsiteDisplay");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
