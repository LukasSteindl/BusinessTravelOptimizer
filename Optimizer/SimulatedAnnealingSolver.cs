using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solver;
using System.Threading;
using System.Collections.Concurrent;

namespace Solver
{
    public class SimulatedAnnealingSolver : Solver
    {
        int starttemperature = 10000;
        int reportevery = 1000;
        int maxsteps = 1000;
        int parallelanealings;
       // int COUNTER = 0;
        public SimulatedAnnealingSolver(ISolvable model,int starttemperature,int maxsteps, int reportevery, int parallelanealings) : base(model)
        {
            this.reportevery = reportevery;
            this.starttemperature = starttemperature;
            this.maxsteps = maxsteps;
            this.parallelanealings = parallelanealings;

        }
        ConcurrentBag<Tuple<List<int>, double>> best_sequences = new ConcurrentBag<Tuple<List<int>, double>>();

        ConcurrentStack<ISolvable> modelcache = new ConcurrentStack<ISolvable>();
        object sync = new object();
        public override Result Optimize()
        {
            //zuerst die optionen berechnen!
            model.InitModel();
            best_sequences = new ConcurrentBag<Tuple<List<int>, double>>();
            Parallel.For(0, parallelanealings, i =>
            {
                ISolvable modelclone;
                //modelcache bringt sich beim SA vermutlich nichts! eventuell ausbauen!
                //versuche ein Modell vom cache zu holen: 
               // if (modelcache.TryPop(out modelclone) == false)
                modelclone = (ISolvable)model.getModelClone(); //falls keines existiert erzeuge eines! 

                var sequence = new List<int>();
             
                for (int j = 0; j < modelclone.GetNumberOfElements(); j++)
                    sequence.Add(j);
                //              Let s = s0
                //              For k = 0 through kmax (exclusive):
                //                  T ← temperature(k ∕ kmax)
                //                  Pick a random neighbour, snew ← neighbour(s)
                //                  If P(E(s), E(snew), T) ≥ random(0, 1):
                //                      s ← snew

                var r = new Random();
                int kmax = maxsteps;
                double T;
                sequence = sequence.OrderBy(a => Guid.NewGuid()).ToList();
                modelclone.SetSequence(sequence);
                double last_cost = modelclone.GetCost() * -1;
                double new_cost = 0;
                double highest_cost_so_far = double.MaxValue;
                List<int> best_sequence = null; //innerhalb dieses Annealers (es kann mehrere geben!)
                int k;
                for (k = 1; k < kmax && !model.CancelToken.IsCancellationRequested; k++)
                {
                   
                    T = get_temperature(1.0 * k / kmax);

                    //robuster code, setzt jedoch immer gesamte sequenz! 
                    var next_sequence = get_local_neighbor_swap_random_element_to_last_position(sequence);
                    modelclone.SetSequence(next_sequence);

                    //versuch einer Lösung mit Lokaler Änderung und Repair der Sequenz
                    //leider noch sehr instabil! (es kann durch auskommentieren der beiden zeilen oben
                    //und einkommentieren der nächsten Zeile und dem else zweig unten aktiviert werden.
                    //var next_sequence = model.random_option_change();

                    //new_cost entspricht eigentlich dem gewinn (der Summe aller positiven Deckungsbeiträge!) 
                    new_cost = modelclone.GetCost() * -1;   
                    
                    //wenn die Kosten der neuen Sequenz niedriger als die bisher niedrigsten Kosten sind, merke dir die Sequenz! 
                    if (new_cost < highest_cost_so_far)
                    {
                        highest_cost_so_far = new_cost;
                        best_sequence = next_sequence;
                        best_sequences.Add(new Tuple<List<int>, double>(best_sequence.ToList(), highest_cost_so_far));
                    }

                    if (((k - 1) * parallelanealings) % reportevery == 0 && i == 0) // nur der erste Annealer darf reporten! 
                    {
                        modelclone.SetSequence(sequence);
                        ((IProgress<Result>)progressHandler).Report(new Result(best_sequence, modelclone.getModelClone(), 1.0 * k / kmax, (k - 1) * parallelanealings));
                        Thread.Sleep(200);
                    }


                    var acceptance_probabiltiy = p(last_cost, new_cost, T);
                    //Console.WriteLine("oldcost:" + last_cost + " newcost:" + new_cost + "temperature:"+ T +  " probability:" + acceptance_probabiltiy);
                    if (acceptance_probabiltiy >= r.NextDouble())
                    {
                        sequence = next_sequence;
                        last_cost = new_cost;
                        // Console.WriteLine("new state accepted with p:" + acceptance_probabiltiy);
                    }
                    //  else
                    //  {
                    //       model.undo_last_random_option_change(); //mache letzten Optionsänderung rückgängig! 
                    //  }

                }
               // modelcache.Push(modelclone);
            });

            
         
            return new Result(get_best_sequence(), model.getModelClone(),0, maxsteps*parallelanealings);
        }

        private List<int> get_best_sequence()
        {
            var topseqences = best_sequences.ToList<Tuple<List<int>, double>>();
            double best_so_far = double.MaxValue;
            List<int> best_sequence = new List<int>();
            foreach (Tuple<List<int>,double> x in topseqences)
            {
                if (x.Item2 < best_so_far)
                {
                    best_so_far = x.Item2;
                    best_sequence = x.Item1;
                }
            }
            return best_sequence;
        }

        private double p(double last_cost,double new_cost,double T)
        {
            //The acceptance probability function {\displaystyle P(e, e',T)} P(e,e', T) was defined as 
            //1 if {\displaystyle e'<e} e' < e, and {\displaystyle \exp(-(e'-e)/T)} \exp(-(e' - e) / T) otherwise.
            if (new_cost <= last_cost)
                return 1;
            else
            {
                var delta = new_cost - last_cost;
                var p = Math.Exp(-(delta) / T);
                return p;
            }
        }

        private double get_temperature(double fraction_of_time)
        {
            return starttemperature * Math.Pow(0.90,fraction_of_time*100);
        }

        private List<int> get_local_neighbor_swap_random_element_to_last_position(List<int> sequence)
        {
            //Verschiebt zufaellig gewaehlten Eintrag an das Ende der Liste
            //damit wird dieser Eintrag ganz niedrig priorisiert.
            var r = new Random();
            int random_position = r.Next(0, sequence.Count - 1);
            int x = sequence[random_position];
            sequence.RemoveAt(random_position);
            sequence.Add(x);
            return sequence;
        }


        private List<int> get_local_neighbor(List<int> sequence)
        {

            //always swap with left neighbor
            var r = new Random();
            int random_position = r.Next(1,sequence.Count-1);
            List<int> new_sequence = new List<int>();
            //copy old list 
            
            for (int i = 0; i < sequence.Count;i++)
            { 
                //wenn das nächste i die random position ist
                if (i + 1 == random_position) //füge zuerst nächsten hinzu und dann aktuellen
                { 
                    new_sequence.Add(sequence[random_position]);

                    new_sequence.Add(sequence[i]);
                    //und erhöhe i um 1 
                    //sodass der nächste übersprungen wird.
                    i += 1;
                } //sonst füge einfach nächste position hinzu
                else
                new_sequence.Add(sequence[i]);
            }
        
            return new_sequence;
        }
    }
}
