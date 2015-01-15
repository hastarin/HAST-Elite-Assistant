// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant.UnitTests
// Author           : Jon Benson
// Created          : 06-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 07-01-2015
// ***********************************************************************
// <copyright file="RoutePlanningTests.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using HAST.Elite.Dangerous.DataAssistant.Routing;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Class RoutePlanningTests.</summary>
    [TestClass]
    public class RoutePlanningTests
    {
        #region Fields

        /// <summary>The route planner</summary>
        private IRoutePlanner routePlanner;

        #endregion

        #region Public Methods and Operators
        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a popular route.
        /// </summary>
        [TestMethod]
        public void LongRouteGreedyDfs()
        {
            this.routePlanner = new RoutePlanner();
            this.LongRouteTests();
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a popular route.
        /// </summary>
        [TestMethod]
        public void LongRouteEconomy()
        {
            var planner = new RoutePlannerAStar();
            planner.CurrentAlgorithm = RoutePlannerAStar.Algorithm.Economy;
            this.routePlanner = planner;
            this.LongRouteTests();
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a popular route.
        /// </summary>
        [TestMethod]
        public void LongRouteLeastJumps()
        {
            var planner = new RoutePlannerAStar();
            planner.CurrentAlgorithm = RoutePlannerAStar.Algorithm.LeastJumps;
            this.routePlanner = planner;
            this.LongRouteTests();
        }

        private void LongRouteTests()
        {
            this.routePlanner.Source = "Di Jian";
            this.routePlanner.Destination = "38 Virginis";
            this.routePlanner.JumpRange = 20;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Er Tcher";
            this.routePlanner.Destination = "Lave";
            this.routePlanner.JumpRange = 20;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Er Tcher";
            this.routePlanner.Destination = "Kupoledari";
            this.routePlanner.JumpRange = 20;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Er Tcher";
            this.routePlanner.Destination = "Lave";
            this.routePlanner.JumpRange = 16;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Er Tcher";
            this.routePlanner.Destination = "Kupoledari";
            this.routePlanner.JumpRange = 16;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Er Tcher";
            this.routePlanner.Destination = "Lave";
            this.routePlanner.JumpRange = 14;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNull(this.routePlanner.Route);

            this.routePlanner.Source = "Er Tcher";
            this.routePlanner.Destination = "Kupoledari";
            this.routePlanner.JumpRange = 14;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNull(this.routePlanner.Route);
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a popular route.
        /// </summary>
        [TestMethod]
        public void PopularRouteGreedyDfs()
        {
            this.routePlanner = new RoutePlanner();
            this.PopularRouteTests();
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a popular route.
        /// </summary>
        [TestMethod]
        public void PopularRouteEconomy()
        {
            var planner = new RoutePlannerAStar();
            planner.CurrentAlgorithm = RoutePlannerAStar.Algorithm.Economy;
            this.routePlanner = planner;
            this.PopularRouteTests();
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a popular route.
        /// </summary>
        [TestMethod]
        public void PopularRouteLeastJumps()
        {
            var planner = new RoutePlannerAStar();
            planner.CurrentAlgorithm = RoutePlannerAStar.Algorithm.LeastJumps;
            this.routePlanner = planner;
            this.PopularRouteTests();
        }

        private void PopularRouteTests()
        {
            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "Leesti";
            this.routePlanner.JumpRange = 20;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "Leesti";
            this.routePlanner.JumpRange = 18;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "Leesti";
            this.routePlanner.JumpRange = 16;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "Leesti";
            this.routePlanner.JumpRange = 12;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "Leesti";
            this.routePlanner.JumpRange = 11;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Timeout = TimeSpan.FromSeconds(5);
            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "Leesti";
            this.routePlanner.JumpRange = 10;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNull(this.routePlanner.Route);
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a short route.
        /// </summary>
        [TestMethod]
        public void ShortRouteGreedyDfs()
        {
            this.routePlanner = new RoutePlanner();
            this.ShortRouteTests();
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a short route.
        /// </summary>
        [TestMethod]
        public void ShortRouteEconomy()
        {
            var planner = new RoutePlannerAStar();
            planner.CurrentAlgorithm = RoutePlannerAStar.Algorithm.Economy;
            this.routePlanner = planner;
            this.ShortRouteTests();
        }

        /// <summary>
        ///     Tests a class that supports <see cref="HAST.Elite.Dangerous.DataAssistant.Routing.IRoutePlanner" /> against
        ///     a short route.
        /// </summary>
        [TestMethod]
        public void ShortRouteLeastJumps()
        {
            var planner = new RoutePlannerAStar();
            planner.CurrentAlgorithm = RoutePlannerAStar.Algorithm.LeastJumps;
            this.routePlanner = planner;
            this.ShortRouteTests();
        }

        private void ShortRouteTests()
        {
            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "He Bo";
            this.routePlanner.JumpRange = 20;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "He Bo";
            this.routePlanner.JumpRange = 5;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNotNull(this.routePlanner.Route);

            this.routePlanner.Source = "Ethgreze";
            this.routePlanner.Destination = "He Bo";
            this.routePlanner.JumpRange = 4;
            this.routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute();
            Assert.IsNull(this.routePlanner.Route);
        }

        #endregion

        #region Methods

        /// <summary>Times the and output route.</summary>
        private void TimeAndOutputRoute()
        {
            var routeFound = routePlanner.Calculate();
            Debug.WriteLine(
                "{0} to {1} ({2}) = {3}ms",
                routePlanner.Source,
                routePlanner.Destination,
                routePlanner.JumpRange,
                routePlanner.CalculationTime.TotalMilliseconds);
            if (routeFound)
            {
                double distance = 0;
                foreach (var routeNode in routePlanner.Route)
                {
                    //Debug.WriteLine("{0} : {1:F}ly", routeNode.System, routeNode.Distance);
                    distance += routeNode.Distance;
                }
                Debug.WriteLine("TOTAL {1} jumps {0:F}ly distance", distance, routePlanner.Route.Count());
            }
            else
            {
                Debug.WriteLine("Sorry you probably need more jump range");
            }
        }

        #endregion
    }
}