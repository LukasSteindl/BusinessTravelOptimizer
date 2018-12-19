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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TSPModel;
using MEPModel;
using Solver;
using Microsoft.Win32;
using System.IO;
using Geocoding;
using Microsoft.Maps.MapControl.WPF;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;

namespace MEPUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SolidColorBrush BlackBrush = new SolidColorBrush();
        SolidColorBrush GreenBrush = new SolidColorBrush();
        SolidColorBrush RedBrush = new SolidColorBrush();
        MEP model;
        Microsoft.Maps.MapControl.WPF.Location pinLocation = null;
        MainViewModel mvm;


        //int generations = 100;
        //int population_size = 1000;
        //int mutation_probability = 5;



        public MainWindow()
        {
            BlackBrush.Color = Colors.Gray;
            GreenBrush.Color = Colors.Green;
            RedBrush.Color = Colors.Red;
            InitializeComponent();
            btnAddressSearch.Focus();

            mvm = new MainViewModel();
            mvm.Routes = new ObservableCollection<OptionViewModel>();
            mvm.Engineers = new ObservableCollection<EngineerViewModel>();
            mvm.Requests = new ObservableCollection<RequestViewModel>();
            this.DataContext = mvm;
            cbAllowUnprofitableRequests.ToolTip = "Manche Einsätze sind für sich allein betrachtet unprofitabel(negativer Deckungsbeitrag)." + Environment.NewLine +
                          "Jedoch können mehrere unprofitable Einsätze in Serie(z.b in der gleichen Gegend) profitabel sein. Das Problem dabei ist, dass es sowohl Einsätze geben kann, die in der Gruppe sinnvoll sind wie auch solche, " + Environment.NewLine +
                          "die es selbst dann nicht sind und durch dieses Setting dennoch ausgeliefert werden müssen." + Environment.NewLine +
                          "Eine zukünftige Version des Modells könnte dies berücksichtigen indem ein übergeordneter Binpacking Algorithmus lokal unrentable Requests hinzunimmt / nicht hinzunimmt.";

        }



        private void write_result(ISolvable model)
        {
            this.lblCost.Content = "Erfolg: " + model.GetCost();
            String sequence = "Sequence:";
            foreach (int i in model.GetSequence())
                sequence += "-" + i.ToString();
            lblSequence.Content = sequence;
        }


        private void btnSaveModel(object sender, RoutedEventArgs e)
        {
            if (model != null)
            {
                string s = this.model.GetModelString();
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Filter = "Json file (*.json)|*.json";
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, s);
            }
        }

        private void btnLoadModel(object sender, RoutedEventArgs e)
        {
            model = new MEPModel.MEP();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ofd.Filter = "Json file (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                String problemInstance = File.ReadAllText(ofd.FileName);
                model.LoadModelString(problemInstance);
                rbGreedy.IsChecked = true;
                btnRunAsync_Click(sender, e);

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            String problemInstance = File.ReadAllText(@"MEP-Small.json");
            model = new MEPModel.MEP();
            model.LoadModelString(problemInstance);
            btnRunAsync_Click(sender, e);
        }


        private void myMap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pinLocation = myMap.ViewportPointToLocation(e.GetPosition(myMap));

        }

        private async void btnAddEngineer_Click(object sender, RoutedEventArgs e)
        {
            var l = await Lookup.reversegeocode(pinLocation.Latitude, pinLocation.Longitude);

            EngineerWindow ew = new EngineerWindow(model);
            ew.HomeOfficeLocation = l;
            if (ew.ShowDialog() == true)
            {
                Engineer engineer = new Engineer(ew.EngineerName, ew.HomeOfficeLocation, model);
                AddLocationIfNew(ew.HomeOfficeLocation);

                engineer.Skills = ew.Skills;
                model.Engineers.Add(engineer);

                UpdateViewModel(new GreedySolver(model).Optimize());
            }

        }

        private void AddLocationIfNew(MEPModel.Location l)
        {
            bool contained = false;
            foreach (MEPModel.Location loc in model.Locations)
                if (loc.Longitude == l.Longitude && loc.Latitude == l.Latitude)
                    contained = true;
            if (!contained)
                model.Locations.Add(l);
        }

        private async void btnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            var l = await Lookup.reversegeocode(pinLocation.Latitude, pinLocation.Longitude);
            RequestWindow rw = new RequestWindow(model);
            rw.RequestLocation = l;
            if (rw.ShowDialog() == true)
            {
                Request r = new Request(rw.Duration, rw.RequestLocation);
                AddLocationIfNew(rw.RequestLocation);
                r.PossibleStartDays = rw.PossibleStartDays;
                r.SkillsRequested = rw.Skills;
                model.Requests.Add(r);

                UpdateViewModel(new GreedySolver(model).Optimize());
            }
        }


        private void btnAddressSearch_Click(object sender, RoutedEventArgs e)
        {
            move_map();
        }

        private async void move_map()
        {
            if (txtAddress.Text != null)
            {
                var l = await Lookup.geocode(txtAddress.Text);
                myMap.SetView(new Microsoft.Maps.MapControl.WPF.Location(l.Latitude, l.Longitude), 15.4);
            }
        }

        private void btnNewMEP(object sender, RoutedEventArgs e)
        {
            createNewModel();
        }

        private void createNewModel()
        {
            model = new MEP();
            // Schedule.ItemsSource = model.Engineers;
            model.Skills.Add("SQL Server Admin");
            model.Skills.Add("SQL Server Development");
            model.Skills.Add("Windows Server");
            model.Skills.Add("Data Factory");
            model.Skills.Add("C# Development");
            model.Skills.Add("Integration Services");
            model.Skills.Add("Reporting Services");
            model.Skills.Add("Azure Machine Learning");
            model.Skills.Add("SQL Server on Linux");

            UpdateViewModel(new GreedySolver(model).Optimize());

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                move_map();
        }




        private void btnEditRequest_Click(object sender, RoutedEventArgs e)
        {
            if (lstRequests.SelectedItem != null)
            {
                var request = model.Requests[mvm.Requests.IndexOf((RequestViewModel)lstRequests.SelectedItem)];

                RequestWindow rw = new RequestWindow(model);
                rw.RequestLocation = request.RequestLocation;
                rw.Skills = request.SkillsRequested;
                rw.Duration = request.Duration;
                rw.PossibleStartDays = request.PossibleStartDays;
                if (rw.ShowDialog() == true)
                {
                    request.RequestLocation = rw.RequestLocation;
                    request.SkillsRequested = rw.Skills;
                    request.Duration = rw.Duration;
                    request.PossibleStartDays = rw.PossibleStartDays;

                    UpdateViewModel(new GreedySolver(model).Optimize());
                }
            }
        }

        private void btnRemoveRequest_Click(object sender, RoutedEventArgs e)
        {
            if (lstRequests.SelectedItem != null)
            {
                var request = model.Requests[mvm.Requests.IndexOf((RequestViewModel)lstRequests.SelectedItem)];
                model.Requests.Remove(request);

                UpdateViewModel(new GreedySolver(model).Optimize());
            }
        }

        private void btnEditEngineer_Click(object sender, RoutedEventArgs e)
        {
            if (Schedule.SelectedItem != null)
            {
                //var engineer = ((EngineerViewModel)Schedule.SelectedItem).EngineerEntity;
                var engineer = model.Engineers[mvm.Engineers.IndexOf((EngineerViewModel)Schedule.SelectedItem)];
                EngineerWindow ew = new EngineerWindow(model);
                ew.HomeOfficeLocation = engineer.HomeOffice;
                ew.Skills = engineer.Skills;
                ew.EngineerName = engineer.Name;

                if (ew.ShowDialog() == true)
                {

                    engineer.HomeOffice = ew.HomeOfficeLocation;
                    engineer.Skills.Clear();
                    engineer.Skills.AddRange(ew.Skills);
                    engineer.Name = ew.EngineerName;

                    UpdateViewModel(new GreedySolver(model).Optimize());
                }
            }
        }

        private void btnRemoveEngineer_Click(object sender, RoutedEventArgs e)
        {
            if (Schedule.SelectedItem != null)
            {
                var engineer = model.Engineers[mvm.Engineers.IndexOf((EngineerViewModel)Schedule.SelectedItem)];
                model.Engineers.Remove(engineer);

                UpdateViewModel(new GreedySolver(model).Optimize());
            }

        }

        CancellationTokenSource tokensource;
        Task<Result> optimizerTask;


        private void set_model_parameter()
        {
            if (cbAllowTripContinuation.IsChecked == true)
                model.AllowTripContinuation = true;
            else
                model.AllowTripContinuation = false;


            if (cbPermutateOptions.IsChecked == true)
                model.PermutateOptions = true;
            else
                model.PermutateOptions = false;

            if (cbAllowUnprofitableRequests.IsChecked == true)
                model.AllowUnprofitableRequests = true;
            else
                model.AllowUnprofitableRequests = false;

            model.AvgTravelSpeed = (int)sldAvgSpeed.Value;
            model.HotelCostPerNight = (int)sldAvgHotel.Value;
            model.HourlyWage = (int)sldAvgWage.Value;
            model.MilageAllowance = (int)sldAvgMileage.Value;
            model.RevenuePerDayOnsite = (int)sldAvgRevenue.Value;
        }
        bool optimizer_running = false;
        private void btnRunAsync_Click(object sender, RoutedEventArgs e)
        {
            output_to_file = false;
            tokensource = new CancellationTokenSource();
            if (model != null && model.Requests.Count > 0)
            {
                btnRunAsync.IsEnabled = false;
                set_model_parameter();
                //clear optimizer Output:
                txtOptimizerOutput.Text = "";

                Solver.Solver s = null;
                if (rbGeneticAlgorithm.IsChecked == true)
                {
                    s = new GeneticSolver(model, Convert.ToInt32(txtGenerations.Text), Convert.ToInt32(txtPopulationSize.Text), Convert.ToInt32(txtMutationProbability.Text), Convert.ToInt32(txtReportProgress.Text));
                }
                if (rbBruteForce.IsChecked == true)
                {
                    if (model.GetNumberOfElements() > 11)
                        MessageBox.Show("I´m sorry Dave, I`m afraid I can`t do that!");
                    else
                        s = new BruteForceSolver(model);
                }
                if (rbSimulatedAnnealing.IsChecked == true)
                {
                    s = new SimulatedAnnealingSolver(model, Convert.ToInt32(txtStartTemp.Text), Convert.ToInt32(txtSteps.Text), Convert.ToInt32(txtReportProgress.Text), Convert.ToInt32(txtParallelAnnealings.Text));
                }
                if (rbGreedy.IsChecked == true)
                {
                    s = new GreedySolver(model);
                }
                if (rbInsertion.IsChecked == true)
                {
                    s = new NearestInsertionSolver(model);
                }
                if (rbSavings.IsChecked == true)
                {
                    s = new SavingsSolver(model);
                }

                if (s != null)
                    Start_Optimizer(s);
            }
        }


        private void Start_Optimizer(Solver.Solver s)
        {
            optimizer_running = true;
            if (s != null)
            {
                model.CancelToken = tokensource.Token;

                //hier fehlt noch die Cancelation genauso wie die cancel token überprüfung in allen optimizern
                optimizerTask = new Task<Result>(() => s.Optimize(), model.CancelToken);
                s.ProgressHandler.ProgressChanged += Prog_ProgressChanged;
                //optimizerTask.Start();
                //await optimizerTask;

                //Func<Solver.Solver, Result> func = (so) => { return so.Optimize(); };
                Task.Factory.StartNew((obj) =>
                {
                    var so = obj as Solver.Solver;
                    return so.Optimize();
                }, s, model.CancelToken).ContinueWith((tResult) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //gibt nochmal das letzte ergebnis aus: 
                        UpdateViewModel(tResult.Result);
                        if (!output_to_file)
                            txtOptimizerOutput.Text += getRouteDescription();
                        if (output_to_file)
                        {
                            File.WriteAllText(scenario + "_" + start.ToString("dd_HH_mm_ss")+ "_" + DateTime.Now.ToString("dd_HH_mm_ss") +  ".txt", txtOptimizerOutput.Text);
                            txtOptimizerOutput.Text = "";
                        }
                        btnRunAsync.IsEnabled = true;
                        optimizer_running = false;
                    });
                });
            }

        }
        DateTime start;

        private bool updating = false;


        private Microsoft.Maps.MapControl.WPF.Location ConvertToWPFLocation(MEPModel.Location l)
        {
            return new Microsoft.Maps.MapControl.WPF.Location(l.Latitude, l.Longitude);
        }


        Result last_result = null;

        //wird vom Solver nach jedem Zwischenergebnis und mit dem Endergebnis aufgerufen:
        private void UpdateViewModel(Result r)//hier sollte eine kopie des modells geliefert werden!
        {
            if (!updating)
            {
                updating = true;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //für den export button soll das letzte ergebnis gespeichert werden!
                last_result = r;
             
                //Clear everything
                ClearViewModel();


                //extract and set resultmodel
                MEP resultmodel = (MEP)r.Model;
                progressSolver.Value = r.Percent;
            
                int modelevaluations = r.ModelEvaluations;
               // resultmodel.InitModel();
                resultmodel.SetSequence(r.Sequence);

                if (cbMinimalOutput.IsChecked == false)
                {
                    String sequence = "Sequence:";
                    foreach (int i in r.Sequence)
                        sequence += "-" + i.ToString();
                    mvm.Sequence = sequence;
                    //minimalausgabe
                    show_model(resultmodel);
                }

                int satisfied_Requests = 0;
                foreach (Request req in resultmodel.Requests)
                    if (req.SelectedOption != null)
                        satisfied_Requests++;
             
                txtOptimizerOutput.Text += modelevaluations + ";" + resultmodel.GetCost() + ";" + satisfied_Requests
                //verboser output:
                //+ sequence 
                + Environment.NewLine;
                
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds + "ms to render");
                updating = false;
            }
        }

        private void ClearViewModel()
        {
            mvm.Engineers.Clear();
            mvm.Requests.Clear();
            mvm.Routes.Clear();
        }

        private void show_model(MEP resultmodel)
        {
            
            mvm.Cost = "Erfolg: " + Math.Round(resultmodel.GetCost());
            //visualisiere das Modell:             
            foreach (Request req in resultmodel.Requests)
            {
                RequestViewModel rvm = new RequestViewModel();
                rvm.City = req.RequestLocation.City;
                rvm.SkillsRequested = String.Concat(req.SkillsRequested);
                rvm.Duration = req.Duration;
                if (req.SelectedOption != null)
                    rvm.Startday = req.SelectedOption.Startday;
                rvm.Description = req.Description;
                rvm.RequestLocation = ConvertToWPFLocation(req.RequestLocation);
                mvm.Requests.Add(rvm);

            }
            var rand = new Random();

            foreach (Engineer en in resultmodel.Engineers)
            {
                var col = Color.FromRgb((byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255));
                var evm = new EngineerViewModel();
                evm.Name = en.Name;
                evm.Skills = String.Concat(en.Skills);
                evm.EngineerLocation = ConvertToWPFLocation(en.HomeOffice);
                mvm.Engineers.Add(evm);
                var ovm = new MEPUI.OptionViewModel();
                ovm.Brush = new SolidColorBrush(col);

                //berechne Rundreise des Engineers:
                if (resultmodel.AllowTripContinuation == true && en.AssignedRequests.Count > 0)
                {
                    //erste anreise:
                    ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(en.HomeOffice.Latitude, en.HomeOffice.Longitude));
                    ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(en.AssignedRequests.Values[0].RequestLocation.Latitude, en.AssignedRequests.Values[0].RequestLocation.Longitude));
                    //Console.WriteLine("Erste Anreise:" + en.HomeOffice.City + " " + en.AssignedRequests.Values[0].RequestLocation.City + " Cost:" + resultmodel.cost_between_request_and_home(en.AssignedRequests.Values[0],en));

                    for (int i = 0; i < en.AssignedRequests.Count; i++)
                    {

                        Request re = en.AssignedRequests.Values[i];
                        //   Console.WriteLine("Revenue:" + re.SelectedOption.Cost);
                        var rvm = new RequestViewModel();
                        rvm.Duration = re.Duration;
                        rvm.Startday = re.SelectedOption.Startday;
                        rvm.City = re.RequestLocation.City;
                        rvm.Description = re.Description;
                        evm.AssignedRequests.Add(rvm);
                        //ermittle Route:
                        if (i + 1 < en.AssignedRequests.Count)
                        {
                            var rnext = en.AssignedRequests.Values[i + 1];
                            foreach (Route route in en.optimalRoute_between_Options(re.SelectedOption, rnext.SelectedOption))
                            {
                                //       Console.WriteLine(route.From.City + " " + route.To.City + " Cost:" + route.Cost);
                                ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(route.From.Latitude, route.From.Longitude));
                                ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(route.To.Latitude, route.To.Longitude));

                            }
                        }
                    }
                    //letzte Rückreise:
                    ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(en.AssignedRequests.Values[en.AssignedRequests.Count - 1].RequestLocation.Latitude, en.AssignedRequests.Values[en.AssignedRequests.Count - 1].RequestLocation.Longitude));
                    ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(en.HomeOffice.Latitude, en.HomeOffice.Longitude));
                    //  Console.WriteLine("Letzte Rückreise" + en.AssignedRequests.Values[en.AssignedRequests.Count - 1].RequestLocation.City + " " + en.HomeOffice.City + " Cost:" + resultmodel.cost_between_request_and_home(en.AssignedRequests.Values[en.AssignedRequests.Count - 1], en));

                    mvm.Routes.Add(ovm);

                }
                else //Immer von zuhause aus zum Ziel:
                {
                    foreach (Request re in en.AssignedRequests.Values)
                    {
                        var rvm = new RequestViewModel();
                        rvm.Duration = re.Duration;
                        rvm.Startday = re.SelectedOption.Startday;
                        rvm.City = re.RequestLocation.City;
                        rvm.Description = re.Description;
                        evm.AssignedRequests.Add(rvm);

                        ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(en.HomeOffice.Latitude, en.HomeOffice.Longitude));
                        ovm.MapLocations.Add(new Microsoft.Maps.MapControl.WPF.Location(re.RequestLocation.Latitude, re.RequestLocation.Longitude));
                    }
                    mvm.Routes.Add(ovm);
                }
            }

            //Bug im Bing WPF Control - manueller Layout Refresh benötigt
            myMap.UpdateLayout();
            var c = myMap.Center;
            c.Latitude += 0.00001;
            myMap.SetView(c, myMap.ZoomLevel);
            //zeichne Optionen:
            OptionGraph.Children.Clear();

            if (resultmodel.Options.Count < 200)
            {    
                //Zeichne Optionsgraph
                int request_column = 0;
                int option_row = 0;
                foreach (Request re in resultmodel.Requests)
                {
                    var ui_request = new Label();
                    ui_request.Content = re.RequestLocation.City;

                    ui_request.FontSize = 10;

                    OptionGraph.Children.Add(ui_request);
                    Canvas.SetLeft(ui_request, request_column * 110);
                    foreach (Option o in re.Options)
                    {
                        var ui_option = new Label();

                        ui_option.Width = 100;
                        ui_option.Height = 40;
                        ui_option.Content = o.Id + " " + o.Description;
                        if (o.State == Option.OptionState.Selected)
                            ui_option.Background = GreenBrush;
                        else if (o.State == Option.OptionState.Locked)
                            ui_option.Background = RedBrush;
                        else
                            ui_option.Background = BlackBrush;
                        OptionGraph.Children.Add(ui_option);
                        Canvas.SetLeft(ui_option, request_column * 110);
                        Canvas.SetTop(ui_option, option_row * 50 + 50);
                        option_row += 1;
                    }
                    option_row = 0;
                    request_column += 1;
                }
            }
            else
            {
                Label l = new Label();
                l.Content = "Mehr als 200 Optionen können nicht angezeigt werden. Da die Ausgabe sehr langsam wäre.";
                OptionGraph.Children.Add(l);
                
            }
            

        }

        private void Prog_ProgressChanged(object sender, Result r)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateViewModel(r);
            });
        }

        private void MapPolyline_Loaded(object sender, RoutedEventArgs e)
        {
            var polygon = sender as MapPolyline;
            var model = polygon.DataContext as OptionViewModel;
            polygon.Stroke = model.Brush;
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            if (optimizerTask != null && !tokensource.IsCancellationRequested)
            {
                tokensource.Cancel();
                btnRunAsync.IsEnabled = true;
            }
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            myMap.ZoomLevel = myMap.ZoomLevel * 1.1;
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            myMap.ZoomLevel = myMap.ZoomLevel * 0.9;
        }



        private String getRouteDescription()
        {
            //Funktioniert derzeit nur für die Ausgabe von Ergebnissen die Gabeleinsätze erlauben!
            String Fahrten = "";
            double total_cost = 0;
            double cost_per_emp;
            //export nur möglich wenn das letzte ergebnis != null ist! 
            if (last_result != null)
            {

                MEP resultmodel = (MEP)last_result.Model;
                resultmodel.SetSequence(last_result.Sequence);
                if (resultmodel.AllowTripContinuation)
                {
                    foreach (Engineer en in resultmodel.Engineers)
                    {
                        if (en.AssignedRequests.Count > 0)
                        {
                            cost_per_emp = 0;
                            //erste Anreise
                            Fahrten += en.Name + ":";
                            var first_request = en.AssignedRequests.Values[0];
                            var cost_erste_anreise = resultmodel.cost_between_request_and_home(first_request, en);
                            Fahrten += en.HomeOffice.City + "->(" + cost_erste_anreise + ")->";
                            cost_per_emp += cost_erste_anreise;

                            for (int i = 0; i < en.AssignedRequests.Count; i++)
                            {
                                //aktuelle einsatz beginnend mit dem ersten!
                                var re = en.AssignedRequests.Values[i];
                                //Umsatz berücksichtigen
                                cost_per_emp += re.SelectedOption.Cost;
                                Fahrten += "(" + re.SelectedOption.Cost + ")"; //Umsatz
                                if (i + 1 < en.AssignedRequests.Count)
                                {
                                    var rnext = en.AssignedRequests.Values[i + 1];
                                    foreach (Route route in en.optimalRoute_between_Options(re.SelectedOption, rnext.SelectedOption))
                                    {
                                        cost_per_emp += route.Cost;
                                        Fahrten += route.From.City + "->(" + route.Cost + ")->";
                                    }
                                }
                            }

                            var last_request = en.AssignedRequests.Values[en.AssignedRequests.Count - 1];
                            //letzte Rückreise
                            var last_request_travel_home_cost = resultmodel.cost_between_request_and_home(last_request, en);
                            cost_per_emp += last_request_travel_home_cost; //auch die kosten der letzten Rückreise berücksichtigen!
                            Fahrten += last_request.RequestLocation.City + "->(" + last_request_travel_home_cost + ")->" + en.HomeOffice.City;
                            Fahrten += " Gesamt:" + cost_per_emp + Environment.NewLine;
                            total_cost += cost_per_emp;
                        }
                    }

                }
                else
                {
                    //Immer von Zuhause aus zum Kunden und zurück:
                    foreach (Engineer e in resultmodel.Engineers)
                    {
                        cost_per_emp = 0;
                        Fahrten += e.Name + ":";
                        foreach (Request re in e.AssignedRequests.Values)
                        {
                            Fahrten += e.HomeOffice.City + "->" + re.RequestLocation.City + "(" + re.SelectedOption.Cost + ")->";
                            cost_per_emp += re.SelectedOption.Cost;
                        }
                        Fahrten += e.HomeOffice.City + Environment.NewLine;
                        total_cost += cost_per_emp;
                    }
                }
                Fahrten += " Gesamt über alle Mitarbeiter:" + total_cost + Environment.NewLine;
            }
            return Fahrten;
        }

        private void btnSetSavings_Click(object sender, RoutedEventArgs e)
        {
            //model.set_savings_heuristic_sequence();
            //ClearViewModel();
            //show_model(model);

        }

        private void btnSzenarioGenerator(object sender, RoutedEventArgs e)
        {
            SzenarioGenerator gen = new MEPUI.SzenarioGenerator();
            if (gen.ShowDialog() == true)
            {
                Directory.CreateDirectory("Szenarios");
                //var cities = File.ReadAllLines("simplemaps-europecities-basic.csv");
                var cities = File.ReadAllLines("simplemaps-worldcities-basic.csv");
                var r = new Random();
                int max_engineer_size = Convert.ToInt32(gen.txtEngineers.Text);
                int requests_per_engineer = Convert.ToInt32(gen.txtRequests.Text);
                int stepwidth = Convert.ToInt32(gen.txtIncreaseSizeBy.Text);
                int number_of_scenarios = Convert.ToInt32(gen.txtSzenariosAtSize.Text);

                //starte immer mit 5 mitarbeitern und erhöhe solange bis die maximale anzahl an mitarbeitern erreicht ist
                //erzeuge immer ein neues szenario!
                for (int engineer_size = 1; engineer_size < max_engineer_size; engineer_size+= stepwidth)
                {
                    for (int x = 0; x < number_of_scenarios; x++)
                    {
                        createNewModel();

                        for (int i = 0; i <= engineer_size; i++)
                        {
                            var eng = new Engineer("E" + i, get_random_location(cities, r), this.model);

                            //jedem Engineer zufällig 1-3 Skills hinzufügen:
                            for (int j = 0; j < 4; j++)
                            {
                                var s = get_random_skill(r);
                                if (!eng.Skills.Contains(s))
                                    eng.Skills.Add(s);
                            }
                            this.model.Engineers.Add(eng);
                        }

                        //Für jeden Mitarbeiter n Requests angelegt
                        for (int i = 0; i < engineer_size * requests_per_engineer; i++)
                        {
                            var req = new Request(r.Next(1, 5), get_random_location(cities, r));
                            //jedem Request zufällig einen Skill hinzufügen:
                            var s = get_random_skill(r);
                            req.SkillsRequested.Add(s);
                            //jedem Request zufällig 4 mögliche Starttage hinzugefügt
                            while (req.PossibleStartDays.Count < 5)
                            {
                                var day = r.Next(0, 30);
                                if (!req.PossibleStartDays.Contains(day))
                                    req.PossibleStartDays.Add(day);
                            }
                            this.model.Requests.Add(req);
                        }

                        File.WriteAllText("Szenarios\\Szenario_" + engineer_size + "_"+ x.ToString() +".json", this.model.GetModelString());
                    }

                }
            }
            //recompute with greedy solver to visualize:
            rbGreedy.IsChecked = true;
            btnRunAsync_Click(sender, e);

        }

        private String get_random_skill(Random r)
        {
            var s = r.Next(0, model.Skills.Count);
            return model.Skills[s];
        }

        private MEPModel.Location get_random_location(String[] cities,Random r)
        {

            String[] cityproperties = null;
            while (cityproperties == null || (cityproperties != null && cityproperties.Length > 10))
            {
                var n = cities.Count();
                var c = r.Next(1, n - 1);
                var city = cities[c];
                cityproperties = city.Split(new char[] { ';' });
            }
            return new MEPModel.Location(Convert.ToDouble(cityproperties[2].Replace(',','.')), Convert.ToDouble(cityproperties[3].Replace(',', '.')), "", "", cityproperties[0], cityproperties[5]);
        }

        private void cbPermutateOptions_Click(object sender, RoutedEventArgs e)
        {
            if (cbPermutateOptions.IsChecked == false)
            {
                rbGreedy.IsChecked = true; //springe auf greedy! 
            }
          
        }


        //Mit dem folgenden Button wird eine Versuchsreihe gestartet 
        //und alle Ergebnisse in das Programmverzeichnis ausgegeben.
        bool output_to_file = false;
        String scenario = "";
        private void btnPerformanceTest(object sender, RoutedEventArgs e)
        {
            txtOptimizerOutput.Text = "";
            SzenarioTester st = new SzenarioTester();
            if (st.ShowDialog() == true)
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += Bw_DoWork;

                bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
                var solvers = new List<String>();

                if (st.cbGreedy.IsChecked == true)
                    solvers.Add("Greedy");
                if (st.cbSA.IsChecked == true)
                    solvers.Add("SA");
                if (st.cbGA.IsChecked == true)
                    solvers.Add("GA");
                if (st.cbInsertion.IsChecked == true)
                    solvers.Add("Insertion");
                if (st.cbSavings.IsChecked == true)
                    solvers.Add("Savings");

                var szenario = new List<String>();
                    
                foreach (var s in st.lstScenarios.SelectedItems)
                {
                        szenario.Add(s.ToString());
                }
                
                //if (st.cbSmall.IsChecked == true)
                //    szenario.Add("MEP-Small");
                //if (st.cbMedium.IsChecked == true)
                //    szenario.Add("MEP-Medium");
                //if (st.cbBig.IsChecked == true)
                //    szenario.Add("MEP-Big");
                
                var param = new Tuple<List<String>,List<String>> (solvers,szenario);

                bw.RunWorkerAsync(param);
            }
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Test Serie abgeschlossen!");
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var param = (Tuple<List<String>, List<String>>)e.Argument;
            List<String> solvers = param.Item1;
            List<String> szenarios = param.Item2;
          
            
            output_to_file = true;
            foreach (String sz in szenarios)
            {
                foreach (var s in solvers)
                {
                    if (s.Equals("SA") || s.Equals("GA"))
                        run_szenario(1,sz,s); //SA und GA kann hier unterschiedlich oft ausgeführt werden! (einfach die 1 ersetzen!)
                    else
                        run_szenario(1,sz,s);
                }
            }
        }

        public void run_szenario(int times,String sz,String s)
        {
            int render_every = 5120; //model evaluations
             //n mal laufen lassen für empirische Auswertung
            //(bei den direkten Heuristiken (greedy,insertion,savings nicht nötig, daher n = 1)
            for (int i = 0; i < times; i++)
            {
                //solange ein Prozess läuft mache nichts! 
                while (optimizer_running)
                    Thread.Sleep(5000);

                String problemInstance = File.ReadAllText(sz + ".json");
                model = new MEPModel.MEP();
                model.LoadModelString(problemInstance);
                scenario = sz + "_" + s + "_" + i;
                Console.WriteLine("Starting Szenario:" + scenario);
                //setze Kontext
                model.AllowTripContinuation = true;
                model.PermutateOptions = false;
                model.AllowUnprofitableRequests = true;
                model.AvgTravelSpeed = 400;
                model.HotelCostPerNight = -100;
                model.HourlyWage = -60;
                model.MilageAllowance = -0.1;
                model.RevenuePerDayOnsite = 2000;

                Solver.Solver so = null;
                switch (s)
                {
                    case "Greedy":
                        so = new GreedySolver(model);
                        break;
                    case "SA":
                        so = new SimulatedAnnealingSolver(model, 10000, 1600, render_every,64); //somit maximal 10.000*16 = 160.000 modell evaluationen!
                        break;
                    case "GA":
                        so = new GeneticSolver(model, 19, 5120, 5, render_every); //somit maximal 100 * 1.600 = 160.000 modell evaluationen!
                        break;
                    case "Insertion":
                        so = new NearestInsertionSolver(model);
                        break;
                    case "Savings":
                        so = new SavingsSolver(model);
                        break;
                }
                start = DateTime.Now;
                Start_Optimizer(so);
            }


        }

        private void cbPermutateOptions_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbInsertion_Click(object sender, RoutedEventArgs e)
        {
            //für savings und insertion muss IMMER mit Optionssequenzen gearbeitet werden! 
            cbPermutateOptions.IsChecked = true;

        }

        private void rbSavings_Click(object sender, RoutedEventArgs e)
        {
            //für savings und insertion muss IMMER mit Optionssequenzen gearbeitet werden! 
            cbPermutateOptions.IsChecked = true;
        }

        private void btnAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow a = new AboutWindow();
            a.ShowDialog();
        }
    }
}
