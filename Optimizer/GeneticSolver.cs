using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solver
{
    public class GeneticSolver : Solver
    {
        int generations = 100;
        int population_size = 1000;
        int mutation_probability = 5;
        int reportevery = 100;
        int total_model_evaluations;
      
       // ConcurrentBag<Individual> population = new ConcurrentBag<Individual>();
        ConcurrentDictionary<int,Individual> population = new ConcurrentDictionary<int, Individual>();


        public GeneticSolver(ISolvable model): base(model)
        {
            
        }

        public GeneticSolver(ISolvable model,int generations, int population_size, int mutation_probability, int reportevery) : base(model)
        {
            this.generations = generations;
            this.population_size = population_size;
            this.mutation_probability = mutation_probability;
            this.reportevery = reportevery;
            total_model_evaluations = population_size * generations;
        }

        //brich ab wenn es keine verbesserung nach x generationen gibt!
        const int abort_after = 100; //brich nicht ab genauso wie der SA solver!
        ConcurrentDictionary<int,Individual> next_population = new ConcurrentDictionary<int, Individual>();
        ConcurrentBag<Tuple<List<int>, double>> best_sequences = new ConcurrentBag<Tuple<List<int>, double>>();

        public override Result Optimize()
        {
            //Compute Options for MEP
            model.InitModel();
            if (model.GetNumberOfElements() == 1)
                return new Result(new List<int> { 0 }, model.getModelClone(),0,0);



            var r = new Random();
            //init
            ConcurrentStack<ISolvable> modelcache = new ConcurrentStack<ISolvable>();
            ConcurrentDictionary<int,Individual> first_population = new ConcurrentDictionary<int, Individual>();
            double best_profit = double.MinValue;
            object sync = new object();

            Parallel.For(0, population_size, i =>
            {
                List<int> best_sequence = null;
                ISolvable modelclone;
                //versuche ein Modell vom cache zu holen: 
                if (modelcache.TryPop(out modelclone) == false)
                    modelclone = (ISolvable)model.getModelClone(); //falls keines existiert erzeuge eines! 


                //erzeuge zufällige startreihenfolge
                var sequence = new List<int>();
                var rnd = new Random();
                for (int j = 0; j < modelclone.GetNumberOfElements(); j++)
                    sequence.Add(j);

                //Evaluate Fitness
                sequence = sequence.OrderBy(a => Guid.NewGuid()).ToList();
                //Interlocked.Increment(ref COUNTER);
                modelclone.SetSequence(sequence);

                var cost = modelclone.GetCost();
                if (cost > best_profit) //cost ist eigentlich der deckungsbeitrag
                {
                    best_sequence = sequence;
                    best_profit = cost;
                    best_sequences.Add(new Tuple<List<int>, double>(best_sequence.ToList(), best_profit));
                }
                

                    if (i % reportevery == 0)
                    {
                        var fittest = getFittestOfPopulation();
                        ((IProgress<Result>)progressHandler).Report(new Result(fittest, modelclone.getModelClone(), 1.0 * i / total_model_evaluations, i));
                        //Thread.Sleep(100);
                    }
            
                first_population.TryAdd(i,new Individual(sequence, modelclone.GetCost()));
                modelcache.Push(modelclone);
            });

            population = first_population;

            //wenn sich das Ergebnis über x generationen nicht verbessert brich ab:
            int has_not_improved = 0;



            //Start Evolution...
            for (int g = 0; g < generations && !model.CancelToken.IsCancellationRequested && has_not_improved < abort_after; g++)
            {
                var best_profit_last = best_profit;
                //List<Individual> next_population = new List<Individual>();
                next_population = new ConcurrentDictionary<int,Individual>();


                //parallele version kopiert das model 
                Parallel.For(0, population_size, i =>
                {
                    List<int> best_sequence = null; //innerhalb dieser Generation!
                    ISolvable modelclone;
                    //versuche ein Modell vom cache zu holen: 
                    if (modelcache.TryPop(out modelclone) == false)
                        modelclone = (ISolvable)model.getModelClone(); //falls keines existiert erzeuge eines! 
                    
                    //Tunierselektion:
                    int i1 = r.Next(0, population_size - 1);
                    int i2 = r.Next(0, population_size - 1);
                    int i3 = r.Next(0, population_size - 1);
                    int i4 = r.Next(0, population_size - 1);

                    Individual p1 = null;
                    Individual p2 = null;
                    if (population[i1].Fitness > population[i2].Fitness)
                        p1 = population[i1];
                    else
                        p1 = population[i2];

                    if (population[i3].Fitness > population[i4].Fitness)
                        p2 = population[i3];
                    else
                        p2 = population[i4];

                    //eventuell Rouletteselektion hier testen

                    //PMX Crossover
                    var child_sequence = crossover(p1.Sequence, p2.Sequence);

                    //Mutation
                    if (r.Next(100) <= mutation_probability)
                    {
                        //println("mutate!");
                        var m1 = r.Next(modelclone.GetNumberOfElements());
                        var m2 = r.Next(modelclone.GetNumberOfElements());
                        var gen1 = child_sequence[m1];
                        var gen2 = child_sequence[m2];
                        child_sequence[m1] = gen2;
                        child_sequence[m2] = gen1;
                    }

                    //Interlocked.Increment(ref COUNTER);
                    //Evaluate Fitness
                    modelclone.SetSequence(child_sequence);



                    var cost = modelclone.GetCost();
                    if (cost > best_profit) //cost ist eigentlich der deckungsbeitrag
                    {
                        best_sequence = child_sequence;
                        best_profit = cost;
                        best_sequences.Add(new Tuple<List<int>, double>(best_sequence.ToList(), best_profit));
                    }
                    //Report every N Modelevaluations
                    // if (model.ModelEvaluations % reportevery == 0)

                    if (i % reportevery == 0)
                    {
                        var fittest = getFittestOfPopulation();
                        ((IProgress<Result>)progressHandler).Report(new Result(fittest, modelclone.getModelClone(), i + (g + 1) * population_size / total_model_evaluations, i + (g + 1) * population_size));
                        Thread.Sleep(100);
                    }
                    Individual child = new Individual(child_sequence, cost);
                    next_population.TryAdd(i, child);
                    modelcache.Push(modelclone);

                }
                );
                
                
                population = next_population;
                //double AVGFitness = population.Average(item => item.Fitness);
                //Console.WriteLine("Generation:" + g + "Fitness" + AVGFitness);
                //if (g % 10 == 0)
                //{
                //    //Thread.Sleep(3000);
                //}     

                //wenn der profit nicht gesteigert werden konnte in der letzten generation
                if (best_profit <= best_profit_last)
                {
                    has_not_improved += 1;
                }
                else
                {
                    has_not_improved = 0; 
                }
                    
            }

            
            //if (has_not_improved == abort_after)
            //    Console.WriteLine("Aborted GA due to missing improvement in " + abort_after + " generations!");
            return new Result(getFittestOfPopulation(), model.getModelClone(),0, (generations+1) * population_size);
        }

        private List<int> getFittestOfPopulation()
        {
            var topseqences = best_sequences.ToList<Tuple<List<int>, double>>();
            double best_so_far = double.MinValue;
            List<int> best_sequence = new List<int>();
            foreach (Tuple<List<int>, double> x in topseqences)
            {
                if (x.Item2 > best_so_far)
                {
                    best_so_far = x.Item2;
                    best_sequence = x.Item1;
                }
            }
            return best_sequence;
        }

        //private Individual getFittestOfPopulation()
        //{
        //    var pop = next_population.ToList<Individual>();
        //    Individual fittest_individual_so_far = pop[0];
        //    foreach (Individual i in pop)
        //    {
        //        if (i.Fitness > fittest_individual_so_far.Fitness)
        //            fittest_individual_so_far = i;
        //    }
        //    return fittest_individual_so_far;
        //}

        private List<int> crossover(List<int> p1, List<int> p2)
        {
            var p1_c = p1.ToArray();
            var p2_c = p2.ToArray();

            var r = new Random();
            int[] child = new int [p1.Count];
            var crossoverpoint = r.Next(0,p1.Count-2);

            //PMX special CrossOver Method
            for (int x = 0; x <= crossoverpoint; x++)
            {
                int gen = p2_c[x];
                child[x] = gen; //setze Kind bis zum crosspoint auf die sequenz des Parent2
                for (int y = (x + 1); y < p1.Count; y++) //durchsuche rechte seite
                {
                    if (p1_c[y] == gen) //falls stadt schon besucht wurde links 
                    {
                        p1_c[y] = p1_c[x]; //repariere sequenz 
                     }
                }
            }

            //setze kind ab dem kreuzungspunkt auf die gene des parent 2
            for (int y = crossoverpoint + 1; y < p1.Count; y++)
                child[y] = p1_c[y];
            return child.ToList<int>();
        }
    }
}
