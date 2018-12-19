using MEPModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace MEPUI
{
    /// <summary>
    /// Interaktionslogik für EngineerWindow.xaml
    /// </summary>
    public partial class EngineerWindow : Window
    {
        
        public EngineerWindow(MEP m)
        {
            InitializeComponent();
            foreach (String s in m.Skills)
                this.lstSkills.Items.Add(s);
            foreach (Location l in m.Locations)
                this.cbHomeOfficeLocation.Items.Add(l);
        }

        public String EngineerName
        {
            get
            {
                return this.txtEngineerName.Text;
            }
            set
            {
                this.txtEngineerName.Text = value;
            }
        }


        public Location HomeOfficeLocation
        {
            get
            {
                return (Location)this.cbHomeOfficeLocation.SelectedItem;
            }
            set
            {
                bool contained = false;
                foreach (Location l in cbHomeOfficeLocation.Items)
                    if (l.Latitude == value.Latitude && l.Longitude == value.Longitude)
                    { 
                        contained = true;
                        this.cbHomeOfficeLocation.SelectedItem = l;
                    }
                if (!contained)
                { 
                    this.cbHomeOfficeLocation.Items.Add(value);
                    this.cbHomeOfficeLocation.SelectedItem = value;
                }
            }
        }

        public List<string> Skills
        {
            get
            {
                List<string> skills = new List<string>();
                foreach (String s in lstSkills.SelectedItems)
                    skills.Add(s);
                return skills;
            }
            set
            {
                foreach (String s in lstSkills.Items)
                    foreach (String selected in value)
                        if (s.Equals(selected))
                            lstSkills.SelectedItems.Add(s);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
