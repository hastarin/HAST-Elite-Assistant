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

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Based on uniform-cost-search/A* from the book Artificial Intelligence: A Modern Approach 3rd Ed by Russell/Norvig
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TAction">The type of the action.</typeparam>
    public class ShortestPathGraphSearch<TState, TAction>
    {
        #region Fields

        /// <summary>
        /// The information
        /// </summary>
        private readonly IShortestPath<TState, TAction> info;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortestPathGraphSearch{TState, TAction}"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        public ShortestPathGraphSearch(IShortestPath<TState, TAction> info)
        {
            this.info = info;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the shortest path.
        /// </summary>
        /// <param name="fromState">From state.</param>
        /// <param name="toState">To state.</param>
        /// <returns>List&lt;Action&gt;.</returns>
        public List<TAction> GetShortestPath(TState fromState, TState toState)
        {
            var exploredSet = new HashSet<TState>();  // The set of nodes already evaluated.
            var frontier = new PriorityQueue<float, SearchNode<TState, TAction>>(); // The set of tentative nodes to be evaluated, initially containing the start node.
            var frontierMap = new Dictionary<TState, SearchNode<TState, TAction>>(); // The map of navigates nodes.

            var startNode = new SearchNode<TState, TAction>(null, 0, 0, fromState, default(TAction));
            frontier.Enqueue(startNode, 0);

            frontierMap.Add(fromState, startNode);

            while (!frontier.IsEmpty)
            {
                var node = frontier.Dequeue();
                frontierMap.Remove(node.State);

                if (node.State.Equals(toState))
                {
                    return this.BuildSolution(node);
                }
                exploredSet.Add(node.State);
                // expand node and add to frontier
                foreach (var action in this.info.Expand(node.State))
                {
                    var child = this.info.ApplyAction(node.State, action);
                    if (exploredSet.Contains(child))
                    {
                        continue;
                    }
                    var searchNode = this.CreateSearchNode(node, action, child, toState);

                    SearchNode<TState, TAction> frontierNode;
                    var isNodeInFrontier = frontierMap.TryGetValue(child, out frontierNode);
                    if (!isNodeInFrontier)
                    {
                        frontier.Enqueue(searchNode, searchNode.F);
                        frontierMap.Add(child, searchNode);
                    }
                    else
                    {
                        if (searchNode.F < frontierNode.F)
                        {
                            frontier.Replace(frontierNode, frontierNode.F, searchNode.F);
                            frontierNode.F = searchNode.F;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the solution.
        /// </summary>
        /// <param name="searchNode">The seach node.</param>
        /// <returns>List&lt;Action&gt;.</returns>
        private List<TAction> BuildSolution(SearchNode<TState, TAction> searchNode)
        {
            var list = new List<TAction>();
            while (searchNode != null)
            {
                // ReSharper disable once CompareNonConstrainedGenericWithNull
                if ((searchNode.Action != null) && (!searchNode.Action.Equals(default(TAction))))
                {
                    list.Insert(0, searchNode.Action);
                }
                searchNode = searchNode.Parent;
            }
            return list;
        }

        /// <summary>
        /// Creates the search node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="action">The action.</param>
        /// <param name="child">The child.</param>
        /// <param name="toState">To state.</param>
        /// <returns>SearchNode&lt;State, Action&gt;.</returns>
        private SearchNode<TState, TAction> CreateSearchNode(
            SearchNode<TState, TAction> node,
            TAction action,
            TState child,
            TState toState)
        {
            var cost = this.info.ActualCost(node.State, action);
            var heuristic = this.info.Heuristic(child, toState);
            return new SearchNode<TState, TAction>(node, node.G + cost, node.G + cost + heuristic, child, action);
        }

        #endregion
    }

    /// <summary>
    /// Class SearchNode.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TAction">The type of the action.</typeparam>
    internal class SearchNode<TState, TAction> : IComparable<SearchNode<TState, TAction>>
    {
        #region Fields

        /// <summary>
        /// The action
        /// </summary>
        public TAction Action;

        /// <summary>
        /// The f
        /// </summary>
        public float F; // estimate

        /// <summary>
        /// The g
        /// </summary>
        public float G; // cost

        /// <summary>
        /// The parent
        /// </summary>
        public SearchNode<TState, TAction> Parent;

        /// <summary>
        /// The state
        /// </summary>
        public TState State;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchNode{State, Action}"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="g">The g.</param>
        /// <param name="f">The f.</param>
        /// <param name="state">The state.</param>
        /// <param name="action">The action.</param>
        public SearchNode(SearchNode<TState, TAction> parent, float g, float f, TState state, TAction action)
        {
            this.Parent = parent;
            this.G = g;
            this.F = f;
            this.State = state;
            this.Action = action;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Reverse sort order (smallest numbers first)
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.</returns>
        public int CompareTo(SearchNode<TState, TAction> other)
        {
            return other.F.CompareTo(this.F);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "SN {f:" + this.F + ", state: " + this.State + " action: " + this.Action + "}";
        }

        #endregion
    }
}