// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 04-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 13-01-2015
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

    using log4net;

    /// <summary>Class EliteDangerousDbContext.</summary>
    public class EliteDangerousDbContext : DbContext
    {
        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger(typeof(EliteDangerousDbContext));

        #endregion

        #region Public Properties

        /// <summary>Gets or sets the stations.</summary>
        public DbSet<Station> Stations { get; set; }

        /// <summary>Gets or sets the systems.</summary>
        public DbSet<System> Systems { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     This method is called when the model for a derived context has been initialized, but before the model has
        ///     been locked down and used to initialize the context. The default implementation of this method does nothing,
        ///     but it can be overridden in a derived class such that the model can be further configured before it is
        ///     locked down.
        /// </summary>
        /// <remarks>
        ///     Typically, this method is called only once when the first instance of a derived context is created. The
        ///     model for that context is then cached and is for all further instances of the context in the app domain.
        ///     This caching can be disabled by setting the ModelCaching property on the given ModelBuidler, but note that
        ///     this can seriously degrade performance. More control over caching is provided through use of the
        ///     DbModelBuilder and DbContextFactory classes directly.
        /// </remarks>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Log.Debug("OnModelCreating entered.");
            //modelBuilder.Entity<Station>().HasKey(t => new { t.SystemId, t.StationName});
            base.OnModelCreating(modelBuilder);
            Log.Debug("base.OnModelCreating was called.");
        }

        #endregion
    }
}