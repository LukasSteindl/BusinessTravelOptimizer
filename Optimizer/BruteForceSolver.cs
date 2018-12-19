using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solver
{
    public class BruteForceSolver : Solver
    {
        private List<int> bestsequence = new List<int>();
        private double lowestcost = double.MinValue;
        int sequence_number = 1;
        int max_sequences;

        public BruteForceSolver(ISolvable model) : base(model)
        {

        }
        public override Result Optimize()
        {
            List<int> sequence = new List<int>();
            //Compute Options for MEP
            model.InitModel();
            max_sequences = factorial_WhileLoop(model.GetNumberOfElements());
            //teste jede sequenz und merke dir die günstigste
            for (int j = 0; j < model.GetNumberOfElements(); j++)
                sequence.Add(j);

            //remembers lowest cost sequence in public variable bestsequence
            permutate(new List<int>(),sequence);
            
            //model.SetSequence(bestsequence); //wird beim anzeigen evaluiert
            return new Result(bestsequence,model.getModelClone(),0, max_sequences);
        }

        private int factorial_WhileLoop(int number)
        {
            int result = 1;
            while (number != 1)
            {
                result = result * number;
                number = number - 1;
            }
            return result;
        }


     
        private void permutate(List<int> sequence, List<int> elements)
        {
            //Blattknoten des Permutationsbaums
            if(elements.Count == 0 && !model.CancelToken.IsCancellationRequested)
            {
                //Hier entsteht die vollständige Sequenz
                //Mit ihr wird das Modell aufgerufen
                sequence_number += 1;
                model.SetSequence(sequence);
                double cost = model.GetCost();

                //String sequenceString = "Sequence:";
                //foreach (int i in sequence)
                //    sequenceString += "-" + i.ToString();
                //Console.WriteLine(sequenceString + " " + cost);

                //Merke dir beste Sequenz
                if (cost > lowestcost)
                {
                    lowestcost = cost;
                    bestsequence.Clear();
                    foreach (int i in sequence)
                    {
                        bestsequence.Add(i);
                    }
                }
                if (sequence_number % 1000 == 0)
                {
                    ((IProgress<Result>)progressHandler).Report(new Result(bestsequence, model.getModelClone(), 1.0 * sequence_number / max_sequences, sequence_number));
                }
                return;
            }

            foreach (int e in elements)
            {
                sequence.Add(e);
                //berechne rest:
                List<int> rest = new List<int>();
                foreach (int i in elements)
                   if (e != i)
                      rest.Add(i);
                permutate(sequence,rest);
                sequence.Remove(e);
            }
        }
    }
}
