// License: MIT
// Source, Docs, Issues: https://github.com/cdanek/kaimira-weighted-list/

using System.Collections;
using System.Text;

using static KaimiraGames.WeightErrorHandlingType;

namespace KaimiraGames
{
    /// <summary>
    /// This implements an algorithm for sampling from a discrete probability distribution via a generic list
    /// with extremely fast O(1) get operations and small (close to minimally small) O(n) space complexity and
    /// O(n) CRUD complexity. In other words, you can add any item of type T to a List with an integer weight,
    /// and get a random item from the list with probability ( weight / sum-weights ).
    /// </summary>
    public class WeightedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// Create a new WeightedList with an optional System.Random.
        /// </summary>
        /// <param name="rand"></param>
        public WeightedList(Random? rand = null)
        {
            _rand = rand ?? Random.Shared;
        }

        /// <summary>
        /// Create a WeightedList with the provided items and an optional System.Random.
        /// </summary>
        public WeightedList(ICollection<WeightedListItem<T>> listItems, Random? rand = null)
        {
            _rand = rand ?? Random.Shared;
            foreach (WeightedListItem<T> item in listItems)
            {
                _list.Add(item._item);
                _weights.Add(item._weight);
            }
            Recalculate();
        }

        public WeightErrorHandlingType BadWeightErrorHandling { get; set; } = SetWeightToOne;

        public T Next()
        {
            if (Count == 0) throw new InvalidOperationException();
            int nextInt = _rand.Next(Count);
            if (_areAllProbabilitiesIdentical) return _list[nextInt];
            int nextProbability = _rand.Next(TotalWeight);
            return (nextProbability < _probabilities[nextInt]) ? _list[nextInt] : _list[_alias[nextInt]];
        }

        public int TotalWeight { get; private set; }

        public IReadOnlyList<T> Items => _list.AsReadOnly();

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public void Clear()
        {
            _list.Clear();
            _weights.Clear();
            Recalculate();
        }

        public int IndexOf(T item) => _list.IndexOf(item);

        public int Count => _list.Count;

        public int GetWeightOf(T item) => GetWeightAtIndex(IndexOf(item));

        public int GetWeightAtIndex(int index) => _weights[index];

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append("WeightedList<");
            sb.Append(typeof(T).Name);
            sb.Append(">: TotalWeight:");
            sb.Append(TotalWeight);
            sb.Append(", Min:");
            sb.Append(_minWeight);
            sb.Append(", Max:");
            sb.Append(_maxWeight);
            sb.Append(", Count:");
            sb.Append(Count);
            sb.Append(", {");
            for (int i = 0; i < _list.Count; i++)
            {
                sb.Append(_list[i].ToString());
                sb.Append(":");
                sb.Append(_weights[i].ToString());
                if (i < _list.Count - 1) sb.Append(", ");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private readonly List<T> _list = new();
        private readonly List<int> _weights = new();
        private readonly List<int> _probabilities = new();
        private readonly List<int> _alias = new();
        private readonly Random _rand;
        private bool _areAllProbabilitiesIdentical = false;
        private int _minWeight;
        private int _maxWeight;

        /// <summary>
        /// https://www.keithschwarz.com/darts-dice-coins/
        /// </summary>
        private void Recalculate()
        {
            TotalWeight = 0;
            _areAllProbabilitiesIdentical = false;
            _minWeight = 0;
            _maxWeight = 0;
            bool isFirst = true;

            _alias.Clear(); // STEP 1
            _probabilities.Clear(); // STEP 1

            List<int> scaledProbabilityNumerator = new(Count);
            List<int> small = new(Count); // STEP 2
            List<int> large = new(Count); // STEP 2
            foreach (int weight in _weights)
            {
                if (isFirst)
                {
                    _minWeight = _maxWeight = weight;
                    isFirst = false;
                }
                _minWeight = (weight < _minWeight) ? weight : _minWeight;
                _maxWeight = (_maxWeight < weight) ? weight : _maxWeight;
                TotalWeight += weight;
                scaledProbabilityNumerator.Add(weight * Count); // STEP 3 
                _alias.Add(0);
                _probabilities.Add(0);
            }

            // Degenerate case, all probabilities are equal.
            if (_minWeight == _maxWeight)
            {
                _areAllProbabilitiesIdentical = true;
                return;
            }

            // STEP 4
            for (int i = 0; i < Count; i++)
            {
                if (scaledProbabilityNumerator[i] < TotalWeight)
                    small.Add(i);
                else
                    large.Add(i);
            }

            // STEP 5
            while (small.Count > 0 && large.Count > 0)
            {
                int l = small[^1]; // 5.1
                small.RemoveAt(small.Count - 1);
                int g = large[^1]; // 5.2
                large.RemoveAt(large.Count - 1);
                _probabilities[l] = scaledProbabilityNumerator[l]; // 5.3
                _alias[l] = g; // 5.4
                int tmp = scaledProbabilityNumerator[g] + scaledProbabilityNumerator[l] - TotalWeight; // 5.5, even though using ints for this algorithm is stable
                scaledProbabilityNumerator[g] = tmp;
                if (tmp < TotalWeight)
                    small.Add(g); // 5.6 the large is now in the small pile
                else
                    large.Add(g); // 5.7 add the large back to the large pile
            }

            // STEP 6
            while (large.Count > 0)
            {
                int g = large[^1]; // 6.1
                large.RemoveAt(large.Count - 1);
                _probabilities[g] = TotalWeight; //6.1
            }

            // STEP 7 - Can't happen for this implementation but left in source to match Keith Schwarz's algorithm
#pragma warning disable S125 // Sections of code should not be commented out
            //while (small.Count > 0)
            //{
            //    int l = small[^1]; // 7.1
            //    small.RemoveAt(small.Count - 1);
            //    _probabilities[l] = _totalWeight;
            //}
#pragma warning restore S125 // Sections of code should not be commented out
        }

        // Adjust bad weights silently.
        internal static int FixWeightSetToOne(int weight) => (weight <= 0) ? 1 : weight;

        // Throw an exception when adding a bad weight.
        internal static int FixWeightExceptionOnAdd(int weight) => (weight <= 0) ? throw new ArgumentException("Weight cannot be non-positive") : weight;

        private int FixWeight(int weight) => (BadWeightErrorHandling == ThrowExceptionOnAdd) ? FixWeightExceptionOnAdd(weight) : FixWeightSetToOne(weight);
    }

    /// <summary>
    /// A single item for a list with matching T. Create one or more WeightedListItems, add to a Collection
    /// and Add() to the WeightedList for a single calculation pass.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct WeightedListItem<T>
    {
        internal readonly T _item;
        internal readonly int _weight;

        public WeightedListItem(T item, int weight)
        {
            _item = item;
            _weight = weight;
        }
    }

    public enum WeightErrorHandlingType
    {
        SetWeightToOne, // Default
        ThrowExceptionOnAdd, // Throw exception for adding non-positive weight.
    }
}
