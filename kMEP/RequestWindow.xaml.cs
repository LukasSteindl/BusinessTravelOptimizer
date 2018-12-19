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
    /// Interaktionslogik für RequestWindow.xaml
    /// </summary>
    public partial class RequestWindow : Window
    {
        public RequestWindow(MEP model)
        {
            InitializeComponent();
            lstSkills.ItemsSource = model.Skills;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


        public Location RequestLocation
        {
            get
            {
                return (Location)this.cbRequestLocation.SelectedItem;
            }
            set
            {
                bool contained = false;
                foreach (Location l in cbRequestLocation.Items)
                    if (l.Latitude == value.Latitude && l.Longitude == value.Longitude)
                    {
                        contained = true;
                        this.cbRequestLocation.SelectedItem = l;
                    }
                if (!contained)
                {
                    this.cbRequestLocation.Items.Add(value);
                    this.cbRequestLocation.SelectedItem = value;
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

        public int Duration
        {
            get { return this.cbDuration.SelectedIndex+1; }
            set { this.cbDuration.SelectedIndex = value-1; }
        }

        public List<int> PossibleStartDays
        {
            get
            {
                var psd = new List<int>();
                foreach (String s in this.txtStartTage.Text.Split(new char[] { ',' }))
                    {
                    psd.Add((int.Parse(s)));
                    }
                return psd;
            }
            set
            {
                String s = "";
                foreach (int i in value)
                    s += i.ToString() + ",";
                this.txtStartTage.Text = s.Substring(0, s.Length - 1);

            }
        }
    }
}
