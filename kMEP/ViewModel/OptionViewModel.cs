using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MEPUI
{
    public class OptionViewModel
    {
        private LocationCollection _mapLocations = new LocationCollection();

        private OptionViewModel ovm;
            

        public LocationCollection MapLocations
        {
            get { return _mapLocations; }

            set
            {
                if (_mapLocations == value)
                {
                    return;
                }

                _mapLocations = value;
                
            }
        }

        public SolidColorBrush Brush
        {
            get
            {
                return brush;
            }

            set
            {
                brush = value;
            }
        }

        public OptionViewModel Ovm
        {
            get
            {
                return this;
            }

            set
            {
                ovm = value;
            }
        }

        SolidColorBrush brush = new SolidColorBrush();
     

     
    }
}
