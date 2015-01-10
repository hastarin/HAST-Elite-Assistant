// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 10-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 10-01-2015
// ***********************************************************************
// <copyright file="RouteNodeDistance.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using SharpDX;

    /// <summary>Class RouteNodeDistance.</summary>
    public class RouteNodeDistance : IRouteNode
    {
        #region Public Properties

        /// <summary>Gets or sets the destination distance squared.</summary>
        public float DestinationDistanceSquared { get; set; }

        /// <summary>Gets or sets the distance of the node from the previous node.</summary>
        public double Distance { get; set; }

        /// <summary>Gets or sets the point.</summary>
        public Vector3 Point { get; set; }

        /// <summary>Gets or sets the source distance squared.</summary>
        public float SourceDistanceSquared { get; set; }

        /// <summary>Gets or sets the system name.</summary>
        public string System { get; set; }

        #endregion
    }
}