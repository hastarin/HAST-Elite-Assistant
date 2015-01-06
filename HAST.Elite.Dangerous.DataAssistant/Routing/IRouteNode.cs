// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 06-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="IRouteNode.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    /// <summary>Interface <see cref="IRouteNode" /> represents a node in a route.</summary>
    public interface IRouteNode
    {
        #region Public Properties

        /// <summary>Gets or sets the distance of the node from the previous node.</summary>
        double Distance { get; set; }

        /// <summary>Gets or sets the system name.</summary>
        string System { get; set; }

        #endregion
    }
}