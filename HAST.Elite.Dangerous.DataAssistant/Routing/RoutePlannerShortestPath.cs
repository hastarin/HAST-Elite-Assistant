using System.Collections.Generic;

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using System;
    using System.Linq;

    using HAST.Elite.Dangerous.DataAssistant.Properties;

    using log4net;

    using SharpDX;

    /// <summary>
    /// A route planner implementing the A* search algorithm
    /// </summary>
    public class RoutePlannerShortestPath : RoutePlannerBase, IShortestPath<KeyValuePair<string,Vector3>, KeyValuePair<string,Vector3>>
    {
        #region Static Fields

        /// <summary>
        /// Used to adjust the padding for the BoundingBox based on the jump range.
        /// </summary>
        /// <remarks>Shorter jump ranges may need more systems to find a path.</remarks>
        private static readonly Dictionary<int,double> PaddingFactor = new Dictionary<int, double>(11);

        private static readonly ILog Log = LogManager.GetLogger(typeof(RoutePlannerShortestPath));

        #endregion

        //private static readonly Dictionary<uint,double> Cache

        #region Fields

        private readonly Dictionary<string,Vector3> systemsInBoundingBox = new Dictionary<string, Vector3>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RoutePlannerShortestPath()
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
        ~RoutePlannerShortestPath()
        {
            this.Dispose(false);
        }

        #endregion

        /// <summary>Return the actual cost between two adjecent locations</summary>
        public float ActualCost(KeyValuePair<string, Vector3> fromLocation, KeyValuePair<string, Vector3> action)
        {
            return Vector3.DistanceSquared(fromLocation.Value, action.Value);
        }

        /// <summary>Returns the new state after an <paramref name="action" /> has been applied</summary>
        public KeyValuePair<string, Vector3> ApplyAction(KeyValuePair<string, Vector3> location, KeyValuePair<string, Vector3> action)
        {
            return action;
        }

        /// <summary>Return the legal moves from a state</summary>
        public List<KeyValuePair<string, Vector3>> Expand(KeyValuePair<string, Vector3> position)
        {
            //if (position.Key == this.Destination)
            //{
            //    return new List<KeyValuePair<string, Vector3>>();
            //}

            var rangeSphere = new BoundingSphere(position.Value, this.JumpRange);
            var systemPoints = from s in systemsInBoundingBox.AsParallel().AsUnordered()
                               where !this.AvoidSystems.Contains(s.Key) && s.Key != position.Key
                               select s;
            var inRange = systemPoints.Where(s =>
                        {
                            var vertex1 = s.Value;
                            return rangeSphere.Contains(ref vertex1) == ContainmentType.Contains;
                        }).ToList();

            return inRange.ToList();
        }

        /// <summary>
        ///     Should return a estimate of shortest distance. The estimate must be admissible (never overestimate)
        /// </summary>
        public float Heuristic(KeyValuePair<string, Vector3> fromLocation, KeyValuePair<string, Vector3> toLocation)
        {
            return Vector3.DistanceSquared(fromLocation.Value, toLocation.Value);
        }

        /// <summary>Calculates the configured route.</summary>
        /// <exception cref="UnknownSystemException" />
        /// <returns><c>true</c> if a route was found, <c>false</c> otherwise.</returns>
        public override bool Calculate()
        {
            this.Stopwatch.Reset();
            this.Stopwatch.Start();

            this.Route = null;
            this.systemsInBoundingBox.Clear();
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
            systems.ForEach(s => this.systemsInBoundingBox.Add(s.Name, new Vector3(s.X, s.Y, s.Z)));
            Log.DebugFormat("Systems considered: {0}", systems.Count);

            var search = new ShortestPathGraphSearch<KeyValuePair<string,Vector3>,KeyValuePair<string,Vector3>>(this);
            try
            {
                var sourceValue = new KeyValuePair<string, Vector3>(Source, this.sourcePoint);
                var destinationValue = new KeyValuePair<string, Vector3>(Destination, this.destinationPoint);
                var route = search.GetShortestPath(sourceValue, destinationValue);
                if (route == null)
                {
                    return false;
                }
                var routeData = new List<RouteNode>(route.Count);
                var previousVector3 = sourcePoint;
                foreach (var pair in route)
                {
                    routeData.Add(
                        new RouteNode
                            {
                                Distance = Vector3.Distance(previousVector3, pair.Value),
                                System = pair.Key
                            });
                    previousVector3 = pair.Value;
                }
                this.Route = routeData;
            }
            finally
            {
                this.Stopwatch.Stop();
                this.CalculationTime = this.Stopwatch.Elapsed;
            }
            return true;
        }
    }
}
