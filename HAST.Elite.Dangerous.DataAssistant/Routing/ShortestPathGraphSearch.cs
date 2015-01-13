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
    using System.Diagnostics;

    /// <summary>
    /// Based on uniform-cost-search/A* from the book Artificial Intelligence: A Modern Approach 3rd Ed by Russell/Norvig
    /// </summary>
    /// <typeparam name="State">The type of the state.</typeparam>
    /// <typeparam name="Action">The type of the action.</typeparam>
    public class ShortestPathGraphSearch<State, Action>
    {
        #region Fields

        /// <summary>
        /// The information
        /// </summary>
        private readonly IShortestPath<State, Action> info;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortestPathGraphSearch{State, Action}"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        public ShortestPathGraphSearch(IShortestPath<State, Action> info)
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
        public List<Action> GetShortestPath(State fromState, State toState)
        {
            var exploredSet = new HashSet<State>();  // The set of nodes already evaluated.
            var frontier = new PriorityQueue<float, SearchNode<State, Action>>(); // The set of tentative nodes to be evaluated, initially containing the start node.
            var frontierMap = new Dictionary<State, SearchNode<State, Action>>(); // The map of navigates nodes.

            var startNode = new SearchNode<State, Action>(null, 0, 0, fromState, default(Action));
            frontier.Enqueue(startNode, 0);

            frontierMap.Add(fromState, startNode);

            while (!frontier.IsEmpty)
            {
                var node = frontier.Dequeue();
                frontierMap.Remove(node.state);

                if (node.state.Equals(toState))
                {
                    return this.BuildSolution(node);
                }
                exploredSet.Add(node.state);
                // expand node and add to frontier
                foreach (var action in this.info.Expand(node.state))
                {
                    var child = this.info.ApplyAction(node.state, action);

                    SearchNode<State, Action> frontierNode;
                    var isNodeInFrontier = frontierMap.TryGetValue(child, out frontierNode);
                    if (!exploredSet.Contains(child) && !isNodeInFrontier)
                    {
                        var searchNode = this.CreateSearchNode(node, action, child, toState);
                        frontier.Enqueue(searchNode, searchNode.f);
                        frontierMap.Add(child, searchNode);
                    }
                    else if (isNodeInFrontier)
                    {
                        var searchNode = this.CreateSearchNode(node, action, child, toState);
                        if (searchNode.f < frontierNode.f)
                        {
                            try
                            {
                                frontier.Replace(frontierNode, frontierNode.f, searchNode.f);
                                frontierNode.f = searchNode.f;
                            }
                            catch (KeyNotFoundException ex)
                            {
                                Debug.WriteLine(ex);
                            }
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
        /// <param name="seachNode">The seach node.</param>
        /// <returns>List&lt;Action&gt;.</returns>
        private List<Action> BuildSolution(SearchNode<State, Action> seachNode)
        {
            var list = new List<Action>();
            while (seachNode != null)
            {
                if ((seachNode.action != null) && (!seachNode.action.Equals(default(Action))))
                {
                    list.Insert(0, seachNode.action);
                }
                seachNode = seachNode.parent;
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
        private SearchNode<State, Action> CreateSearchNode(
            SearchNode<State, Action> node,
            Action action,
            State child,
            State toState)
        {
            var cost = this.info.ActualCost(node.state, action);
            var heuristic = this.info.Heuristic(child, toState);
            return new SearchNode<State, Action>(node, node.g + cost, node.g + cost + heuristic, child, action);
        }

        #endregion
    }

    /// <summary>
    /// Class SearchNode.
    /// </summary>
    /// <typeparam name="State">The type of the state.</typeparam>
    /// <typeparam name="Action">The type of the action.</typeparam>
    internal class SearchNode<State, Action> : IComparable<SearchNode<State, Action>>
    {
        #region Fields

        /// <summary>
        /// The action
        /// </summary>
        public Action action;

        /// <summary>
        /// The f
        /// </summary>
        public float f; // estimate

        /// <summary>
        /// The g
        /// </summary>
        public float g; // cost

        /// <summary>
        /// The parent
        /// </summary>
        public SearchNode<State, Action> parent;

        /// <summary>
        /// The state
        /// </summary>
        public State state;

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
        public SearchNode(SearchNode<State, Action> parent, float g, float f, State state, Action action)
        {
            this.parent = parent;
            this.g = g;
            this.f = f;
            this.state = state;
            this.action = action;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Reverse sort order (smallest numbers first)
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.</returns>
        public int CompareTo(SearchNode<State, Action> other)
        {
            return other.f.CompareTo(this.f);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "SN {f:" + this.f + ", state: " + this.state + " action: " + this.action + "}";
        }

        #endregion
    }
}