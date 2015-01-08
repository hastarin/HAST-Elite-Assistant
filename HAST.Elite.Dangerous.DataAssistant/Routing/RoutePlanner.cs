// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 06-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="RoutePlanner.cs" company="Jon Benson">
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
    using HAST.Elite.Dangerous.DataAssistant.Properties;

    using SharpDX;

    /// <summary>Class RoutePlanner.</summary>
    public class RoutePlanner : IRoutePlanner
    {
        #region Static Fields

        /// <summary>The database</summary>
        private static readonly EliteDangerousDbContext Db = new EliteDangerousDbContext();

        /// <summary>
        /// Used to adjust the padding for the BoundingBox based on the jump range.
        /// </summary>
        /// <remarks>Shorter jump ranges may need more systems to find a path.</remarks>
        private static readonly Dictionary<int,double> PaddingFactor = new Dictionary<int, double>(11);

        #endregion

        //private static readonly Dictionary<uint,double> Cache

        #region Fields

        /// <summary>The <see cref="RoutePlanner.stopwatch" /></summary>
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>The <see cref="destination" /></summary>
        private string destination;

        /// <summary>The <see cref="destination" /> point</summary>
        private Vector3 destinationPoint;

        /// <summary>The <see cref="RoutePlanner.disposed" /></summary>
        private bool disposed;

        /// <summary>The <see cref="source" /></summary>
        private string source;

        /// <summary>The <see cref="source" /> point</summary>
        private Vector3 sourcePoint;

        private TimeSpan timeout = TimeSpan.FromSeconds(3);

        private List<string> avoidSystems = new List<string>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RoutePlanner()
        {
            if (PaddingFactor.Count != 0)
            {
                return;
            }
            PaddingFactor.Add(6,3.0);
            PaddingFactor.Add(7,2.8);
            PaddingFactor.Add(8,2.6);
            PaddingFactor.Add(9,2.4);
            PaddingFactor.Add(10,2.2);
            PaddingFactor.Add(12,2.0);
            PaddingFactor.Add(14,1.8);
            PaddingFactor.Add(16,1.6);
            PaddingFactor.Add(18,1.4);
            PaddingFactor.Add(20,1.0);
            PaddingFactor.Add(200,1.0);
        }

        /// <summary>Finalizes an instance of the <see cref="RoutePlanner" /> class.</summary>
        ~RoutePlanner()
        {
            this.Dispose(false);
        }

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
        public TimeSpan CalculationTime { get; private set; }

        /// <summary>Gets or sets the <see cref="destination" /> system name.</summary>
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
        public IEnumerable<IRouteNode> Route { get; private set; }

        /// <summary>Gets or sets the <see cref="source" /> system name.</summary>
        public string Source
        {
            get
            {
                return this.source;
            }
            set
            {
                var d = Db.Systems.First(s => s.Name == value);
                this.sourcePoint = new Vector3(d.X, d.Y, d.Z);
                this.source = value;
            }
        }

        /// <summary>Gets or sets the timeout for calculating a route.</summary>
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
        public bool Calculate()
        {
            this.stopwatch.Reset();
            this.stopwatch.Start();
            this.Route = null;

            var jumpRangeSquared = this.JumpRange * this.JumpRange;

            var distance = Vector3.Distance(sourcePoint, destinationPoint);
            var straightLineVector = destinationPoint - sourcePoint;
            straightLineVector.Normalize();
            var padding = Settings.Default.RoutePadding * PaddingFactor.First(pf => pf.Key >= this.JumpRange).Value;
            var padDistance = distance / 150 * padding;
            Debug.WriteLine(
                "Jump range = {0:F} gives a padding value of {1:F} for a pad distance of {2:F}",
                this.JumpRange,
                padding,
                padDistance);
            var paddedSource = sourcePoint - (straightLineVector * (float)padDistance);
            var paddedDestination = destinationPoint + (straightLineVector * (float)padDistance);
            var boundingBox = BoundingBox.FromPoints(new[] { paddedSource, paddedDestination });
            Debug.WriteLine(
                "{0} - {1} = {2:F}ly",
                boundingBox.Minimum,
                boundingBox.Maximum,
                Vector3.Distance(paddedSource, paddedDestination));
            var systems =
                Db.Systems.Where(
                    s =>
                    s.X >= boundingBox.Minimum.X && s.X <= boundingBox.Maximum.X && s.Y >= boundingBox.Minimum.Y && s.Y <= boundingBox.Maximum.Y 
                    && s.Z >= boundingBox.Minimum.Z && s.Z <= boundingBox.Maximum.Z)
                    .Select(s => new { s.Name, s.X, s.Y, s.Z })
                    .ToList();

            Debug.WriteLine("Systems found: {0}", systems.Count());

            var systemDistances = new List<RouteNodeDistance>(systems.Count);
            systemDistances.AddRange(
                from s in systems.AsParallel().AsUnordered() 
                where !AvoidSystems.Contains(s.Name)
                let v = new Vector3(s.X, s.Y, s.Z)
                select
                    new RouteNodeDistance
                        {
                            System = s.Name,
                            Point = v,
                            SourceDistanceSquared = Vector3.DistanceSquared(this.sourcePoint, v),
                            DestinationDistanceSquared =
                                Vector3.DistanceSquared(v, this.destinationPoint),
                        });
            var sortedSystemDistances = systemDistances.OrderBy(s => s.SourceDistanceSquared).ToList();
            var rangeSphere = new BoundingSphere(sourcePoint, this.JumpRange);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (sortedSystemDistances.TrueForAll(s => s.SourceDistanceSquared == 0.0 || s.SourceDistanceSquared > jumpRangeSquared))
            {
                return false;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (sortedSystemDistances.TrueForAll(s => s.DestinationDistanceSquared == 0.0 || s.DestinationDistanceSquared > jumpRangeSquared))
            {
                return false;
            }

            var route = new List<RouteNodeDistance>();
            var workingSet = sortedSystemDistances;
            var upperDestination = this.Destination.ToUpperInvariant();
            var destinationName = workingSet.First(r => r.System.ToUpperInvariant() == upperDestination).System;
            try
            {
                while (true)
                {
                    var inRange = workingSet.Where(r =>
                        {
                            var vertex1 = r.Point;
                            return rangeSphere.Contains(ref vertex1) == ContainmentType.Contains;
                        }).ToList();
                    var nextNode = inRange.OrderBy(r => r.DestinationDistanceSquared).FirstOrDefault();
                    if (nextNode != null && nextNode.System.ToUpperInvariant() == upperDestination)
                    {
                        nextNode = workingSet.First(r => r.System == destinationName);
                        if (route.Any())
                        {
                            nextNode.SourceDistanceSquared = Vector3.DistanceSquared(route.Last().Point, nextNode.Point);
                        }
                        route.Add(nextNode);
                        break;
                    }
                    if (stopwatch.Elapsed > Timeout)
                    {
                        throw new RoutePlannerTimeoutException();
                    }
                    if (nextNode == null || nextNode.Point == rangeSphere.Center || nextNode.Point == sourcePoint)
                    {
                        if (route.Any())
                        {
                            var deadEnd = route.Last();
                            //Debug.WriteLine("Re-routing past dead end " + deadEnd.System);
                            workingSet.Remove(deadEnd);
                            route.RemoveAt(route.Count - 1);
                            rangeSphere = !route.Any()
                                              ? new BoundingSphere(this.sourcePoint, this.JumpRange)
                                              : new BoundingSphere(route.Last().Point, this.JumpRange);
                            continue;
                        }
                        if (inRange.Count == 1)
                        {
                            return false;
                        }
                        else
                        {
                            // Try going back to get forward
                            nextNode =
                                inRange.OrderBy(r => r.DestinationDistanceSquared)
                                    .FirstOrDefault(r => r.Point != this.sourcePoint);
                        }
                    }
                    //Debug.WriteLine("Routing to: {0} out of {1} systems", nextNode.System, inRange.Count);
                    // ReSharper disable once PossibleNullReferenceException
                    rangeSphere = new BoundingSphere(nextNode.Point, this.JumpRange);
                    if (route.Any())
                    {
                        nextNode.SourceDistanceSquared = Vector3.DistanceSquared(route.Last().Point, nextNode.Point);
                    }
                    route.Add(nextNode);
                }

                route.AsParallel().ForAll(r => r.Distance = Math.Sqrt(r.SourceDistanceSquared));
                this.Route = route;
            }
            catch (RoutePlannerTimeoutException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            finally
            {
                this.stopwatch.Stop();
                this.CalculationTime = this.stopwatch.Elapsed;
            }


            // TODO: Try to find a better route than the one found?!

            return true;
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
}
