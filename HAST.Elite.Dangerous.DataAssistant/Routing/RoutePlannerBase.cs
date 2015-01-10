// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 10-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 10-01-2015
// ***********************************************************************
// <copyright file="RoutePlannerBase.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using HAST.Elite.Dangerous.DataAssistant.DataAccessLayer;

    using SharpDX;

    /// <summary>Class RoutePlannerBase.</summary>
    public class RoutePlannerBase : IRoutePlanner
    {
        #region Static Fields

        /// <summary>The database</summary>
        protected static readonly EliteDangerousDbContext Db = new EliteDangerousDbContext();

        #endregion

        #region Fields

        /// <summary>The <see cref="Stopwatch" /></summary>
        protected readonly Stopwatch Stopwatch = new Stopwatch();

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.RoutePlannerBase.destination" />
        /// </summary>
        protected string destination;

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.RoutePlannerBase.destination" /> point
        /// </summary>
        protected Vector3 destinationPoint;

        /// <summary>The <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.RoutePlannerBase.disposed" /></summary>
        protected bool disposed;

        /// <summary>The <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.RoutePlannerBase.source" /></summary>
        protected string source;

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.RoutePlannerBase.source" /> point
        /// </summary>
        protected Vector3 sourcePoint;

        private List<string> avoidSystems = new List<string>();

        private TimeSpan timeout = TimeSpan.FromSeconds(3);

        #endregion

        #region Public Properties

        /// <summary>Gets or sets the systems to be avoided when calculating the route.</summary>
        public List<string> AvoidSystems
        {
            get
            {
                return this.avoidSystems;
            }
            set
            {
                this.avoidSystems = value;
            }
        }

        /// <summary>Gets the calculation time taken for the last route.</summary>
        public TimeSpan CalculationTime { get; protected set; }

        /// <summary>
        ///     Gets or sets the <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.RoutePlannerBase.destination" />
        ///     system name.
        /// </summary>
        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                var d = Db.Systems.First(s => s.Name == value);
                this.destinationPoint = new Vector3(d.X, d.Y, d.Z);
                this.destination = value;
            }
        }

        /// <summary>Gets or sets the jump range the ship is capable of.</summary>
        public float JumpRange { get; set; }

        /// <summary>Gets the route.</summary>
        public IEnumerable<IRouteNode> Route { get; protected set; }

        /// <summary>
        ///     Gets or sets the <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.RoutePlannerBase.source" /> system
        ///     name.
        /// </summary>
        public string Source
        {
            get
            {
                return this.source;
            }
            set
            {
                var d = Db.Systems.FirstOrDefault(s => s.Name == value);
                if (d != null)
                {
                    this.sourcePoint = new Vector3(d.X, d.Y, d.Z);
                }
                this.source = value;
            }
        }

        /// <summary>Gets or sets the <see cref="timeout" /> for calculating a route.</summary>
        public TimeSpan Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>Calculates the configured route.</summary>
        /// <exception cref="RoutePlannerTimeoutException" />
        /// <returns><c>true</c> if a route was found, <c>false</c> otherwise.</returns>
        public virtual bool Calculate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                // free other managed objects that implement
                // IDisposable only
                Db.Dispose();
            }

            // release any unmanaged objects
            // set the object references to null

            this.disposed = true;
        }

        #endregion
    }
}