// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 06-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="IRoutePlanner.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using System;
    using System.Collections.Generic;

    /// <summary>Interface <see cref="IRoutePlanner" /> is used for a route planner.</summary>
    public interface IRoutePlanner : IDisposable
    {
        #region Public Properties

        /// <summary>Gets or sets the systems to be avoided when calculating the route.</summary>
        List<string> AvoidSystems { get; set; }

        /// <summary>Gets the calculation time taken for the last route.</summary>
        TimeSpan CalculationTime { get; }

        /// <summary>Gets or sets the destination system name.</summary>
        string Destination { get; set; }

        /// <summary>Gets or sets the jump range the ship is capable of.</summary>
        float JumpRange { get; set; }

        /// <summary>Gets the route.</summary>
        IEnumerable<IRouteNode> Route { get; }

        /// <summary>Gets or sets the source system name.</summary>
        string Source { get; set; }

        /// <summary>Gets or sets the timeout for calculating a route.</summary>
        TimeSpan Timeout { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>Calculates the configured route.</summary>
        /// <exception cref="RoutePlannerTimeoutException" />
        /// <returns><c>true</c> if a route was found, <c>false</c> otherwise.</returns>
        bool Calculate();

        #endregion
    }
}