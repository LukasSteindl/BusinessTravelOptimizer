using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Solver
{
    public interface ISolvable
    {
        //setzt eine vollständige Lösung in das Model ein
        void SetSequence(List<int> sequence);

        //gibt zurück wie oft das Modell evaluiert wurde (sprich eine Sequenz gesetzt wurde seit initialisierung)
        int ModelEvaluations
        {
            get;
            set;
        }

        //gibt neue sequenz zurück
        List<int> random_option_change();
        //gibt vorherige sequenz zurück
        void undo_last_random_option_change();

        List<int> GetSequence();

        CancellationToken CancelToken
        {
            get;
            set;
        }
        object getModelClone();

        void InitModel();

        List<int> get_insertion_heuristic_sequence();
        List<int> get_savings_heuristic_sequence();

        //liefert die Kosten der aktuell gesetzten Lösung
        double GetCost();

        //liefert die Anzahl der Workshops bzw Städte des TSP
        int GetNumberOfElements();

        String GetModelString();

        void LoadModelString(String ProblemInstance);
        
   }
}
