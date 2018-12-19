
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUI
{
    public class EngineerViewModel
    {
        Location engineerLocation;
        private String engineerName;
        private String skills;
        private ObservableCollection<RequestViewModel> assignedRequests = new ObservableCollection<RequestViewModel>();
        
        public string Name
        {
            get
            {
                return engineerName;
            }

            set
            {
                engineerName = value;
            }
        }

        
        public string Skills
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



        public ObservableCollection<RequestViewModel> AssignedRequests
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

      

        public Location EngineerLocation
        {
            get
            {
                return engineerLocation;
            }

            set
            {
                engineerLocation = value;
            }
        }
    }
}
