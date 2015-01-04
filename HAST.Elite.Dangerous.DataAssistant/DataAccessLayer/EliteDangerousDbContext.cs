// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="EliteDangerousDbContext.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.DataAccessLayer
{
    using System.Data.Entity;

    using HAST.Elite.Dangerous.DataAssistant.Models;

    /// <summary>Class EliteDangerousDbContext.</summary>
    public class EliteDangerousDbContext : DbContext
    {
        #region Public Properties

        /// <summary>Gets or sets the stations.</summary>
        public DbSet<Station> Stations { get; set; }

        /// <summary>Gets or sets the systems.</summary>
        public DbSet<System> Systems { get; set; }

        #endregion
    }
}