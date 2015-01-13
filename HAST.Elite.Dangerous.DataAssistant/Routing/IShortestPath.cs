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

using System.Collections.Generic;

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    /// <summary>Interface for a shortest path problem</summary>
    public interface IShortestPath<TState, TAction>
    {
        #region Public Methods and Operators

        /// <summary>Return the actual cost between two adjecent locations</summary>
        float ActualCost(TState fromLocation, TAction action);

        /// <summary>Returns the new state after an <paramref name="action" /> has been applied</summary>
        TState ApplyAction(TState location, TAction action);

        /// <summary>Return the legal moves from a state</summary>
        List<TAction> Expand(TState position);

        /// <summary>
        ///     Should return a estimate of shortest distance. The estimate must me admissible (never overestimate)
        /// </summary>
        float Heuristic(TState fromLocation, TState toLocation);

        #endregion
    }
}