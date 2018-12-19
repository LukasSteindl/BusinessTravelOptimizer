using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for SzenarioTester.xaml
    /// </summary>
    public partial class SzenarioTester : Window
    {
        public SzenarioTester()
        {
            InitializeComponent();
            foreach (var fi in Directory.EnumerateFiles("Szenarios"))
            {
                if (fi.EndsWith(".json"))
                {
                    string s = fi.Replace(".json", "");
                    lstScenarios.Items.Add(s);
                }
            }
            lstScenarios.SelectAll();
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

        }
    }
}
