// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 06-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="RouteNode.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    /// <summary>Class RouteNode.</summary>
    public class RouteNode : IRouteNode
    {
        #region Public Properties

        /// <summary>Gets or sets the distance of the node from the previous node.</summary>
        public double Distance { get; set; }

        /// <summary>Gets or sets the system name.</summary>
        public string System { get; set; }

        #endregion
    }
}