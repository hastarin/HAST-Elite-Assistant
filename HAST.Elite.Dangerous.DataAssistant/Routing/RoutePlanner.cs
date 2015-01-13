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
    using System.Linq;

    using HAST.Elite.Dangerous.DataAssistant.Properties;

    using log4net;

    using SharpDX;

    /// <summary>Class RoutePlanner.</summary>
    public class RoutePlanner : RoutePlannerBase
    {
        #region Static Fields

        /// <summary>
        /// Used to adjust the padding for the BoundingBox based on the jump range.
        /// </summary>
        /// <remarks>Shorter jump ranges may need more systems to find a path.</remarks>
        private static readonly Dictionary<int,double> PaddingFactor = new Dictionary<int, double>(11);

        private static readonly ILog Log = LogManager.GetLogger(typeof(RoutePlanner));

        #endregion

        //private static readonly Dictionary<uint,double> Cache

        #region Fields

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

        #endregion

        #region Public Methods and Operators

        /// <summary>Calculates the configured route.</summary>
        /// <exception cref="RoutePlannerTimeoutException" />
        /// <returns><c>true</c> if a route was found, <c>false</c> otherwise.</returns>
        public override bool Calculate()
        {
            Log.InfoFormat(
                "Calcuating a route between {0} and {1} with a jump range of {2:F2}LY",
                this.Source,
                this.Destination,
                this.JumpRange);
            this.Stopwatch.Reset();
            this.Stopwatch.Start();
            this.Route = null;

            var jumpRangeSquared = this.JumpRange * this.JumpRange;

            var distance = Vector3.Distance(this.sourcePoint, this.destinationPoint);
            var straightLineVector = this.destinationPoint - this.sourcePoint;
            straightLineVector.Normalize();
            var padding = Settings.Default.RoutePadding * PaddingFactor.First(pf => pf.Key >= this.JumpRange).Value;
            var padDistance = distance / 150 * padding;
            Log.DebugFormat(
                "Jump range = {0:F} gives a padding value of {1:F} for a pad distance of {2:F}",
                this.JumpRange,
                padding,
                padDistance);
            var paddedSource = this.sourcePoint - (straightLineVector * (float)padDistance);
            var paddedDestination = this.destinationPoint + (straightLineVector * (float)padDistance);
            var boundingBox = BoundingBox.FromPoints(new[] { paddedSource, paddedDestination });
            Log.DebugFormat(
                "Searching a box from {0} to {1} with a diagonal distance of {2:F}ly",
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

            Log.DebugFormat("Systems considered: {0}", systems.Count);

            var systemDistances = new List<RouteNodeDistance>(systems.Count);
            systemDistances.AddRange(
                from s in systems.AsParallel().AsUnordered() 
                where !this.AvoidSystems.Contains(s.Name)
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
            var rangeSphere = new BoundingSphere(this.sourcePoint, this.JumpRange);
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
                    if (this.Stopwatch.Elapsed > this.Timeout)
                    {
                        Log.WarnFormat("Route planning exceeded timeout of {0}ms", this.Timeout.TotalMilliseconds);
                        throw new RoutePlannerTimeoutException();
                    }
                    if (nextNode == null || nextNode.Point == rangeSphere.Center || nextNode.Point == this.sourcePoint)
                    {
                        if (route.Any())
                        {
                            var deadEnd = route.Last();
                            Log.Debug("Re-routing past dead end " + deadEnd.System);
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
                    // ReSharper disable once PossibleNullReferenceException
                    Log.DebugFormat("Routing to: {0} out of {1} systems", nextNode.System, inRange.Count);
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
                Log.Warn(ex);
                return false;
            }
            finally
            {
                this.Stopwatch.Stop();
                this.CalculationTime = this.Stopwatch.Elapsed;
            }


            // TODO: Try to find a better route than the one found?!

            return true;
        }

        #endregion

        #region Methods

        #endregion
    }
}
