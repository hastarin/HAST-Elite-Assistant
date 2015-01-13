// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 13-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 13-01-2015
// ***********************************************************************
// <copyright file="RouteNodePoint.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using SharpDX;

    /// <summary>Class RouteNodePoint.</summary>
    public class RouteNodePoint : RouteNode
    {
        #region Public Properties

        /// <summary>Gets or sets the point.</summary>
        public Vector3 Point { get; set; }

        #endregion
    }
}