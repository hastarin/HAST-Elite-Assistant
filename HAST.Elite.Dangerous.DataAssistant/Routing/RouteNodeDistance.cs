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
    /// <summary>Class RouteNodeDistance.</summary>
    public class RouteNodeDistance : RouteNodePoint
    {
        #region Public Properties

        /// <summary>Gets or sets the destination distance squared.</summary>
        public float DestinationDistanceSquared { get; set; }

        /// <summary>Gets or sets the source distance squared.</summary>
        public float SourceDistanceSquared { get; set; }

        #endregion
    }
}