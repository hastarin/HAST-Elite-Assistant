// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 13-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 13-01-2015
// ***********************************************************************
// Modified from a version in:
// UnityUtils https://github.com/mortennobel/UnityUtils
// By Morten Nobel-Jørgensen
// License lgpl 3.0 (http://www.gnu.org/licenses/lgpl-3.0.txt)
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Based on http://blogs.msdn.com/b/ericlippert/archive/2007/10/08/path-finding-using-a-in-c-3-0-part-three.aspx
    ///     Backported to C# 2.0
    /// </summary>
    public class PriorityQueue<TPriority, TValue>
    {
        #region Fields

        private readonly SortedDictionary<TPriority, LinkedList<TValue>> list = new SortedDictionary<TPriority, LinkedList<TValue>>();

        #endregion

        #region Public Properties

        /// <summary>Gets a value indicating whether this instance is empty.</summary>
        public bool IsEmpty
        {
            get
            {
                return !this.list.Any();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>Dequeues this instance.</summary>
        /// <exception cref="System.InvalidOperationException">Thrown if there is nothing to Dequeue.</exception>
        /// <returns>V.</returns>
        public TValue Dequeue()
        {
            var keys = this.list.Keys.GetEnumerator();
            keys.MoveNext();
            var key = keys.Current;
            var v = this.list[key];
            var res = v.First.Value;
            v.RemoveFirst();
            if (v.Count == 0)
            {
                // nothing left of the top priority.
                this.list.Remove(key);
            }
            return res;
        }

        /// <summary>Enqueues the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority.</param>
        public void Enqueue(TValue value, TPriority priority)
        {
            LinkedList<TValue> q;
            if (!this.list.TryGetValue(priority, out q))
            {
                q = new LinkedList<TValue>();
                this.list.Add(priority, q);
            }
            q.AddLast(value);
        }

        /// <summary>Replaces the specified value.</summary>
        /// <param name="value">The value.</param>
        /// <param name="oldPriority">The old priority.</param>
        /// <param name="newPriority">The new priority.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     The property is retrieved and <paramref name="key" /> does not exist in the collection.
        /// </exception>
        public void Replace(TValue value, TPriority oldPriority, TPriority newPriority)
        {
            var v = this.list[oldPriority];
            v.Remove(value);

            if (v.Count == 0)
            {
                // nothing left of the top priority.
                this.list.Remove(oldPriority);
            }

            this.Enqueue(value, newPriority);
        }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.list.Keys.SelectMany(key => this.list[key])
                .Aggregate("", (current, val) => current + (val + ", "));
        }

        #endregion
    }
}