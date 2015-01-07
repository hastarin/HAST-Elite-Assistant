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
        private readonly IRoutePlanner routePlanner = new RoutePlanner();

        #endregion

        #region Public Methods and Operators

        /// <summary>Longs the route.</summary>
        [TestMethod]
        public void LongRoute()
        {
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
        public void PopularRoute()
        {
            Debug.WriteLine("Popular route test");

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

        /// <summary>Shorts the route.</summary>
        [TestMethod]
        public void ShortRoute()
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
            Debug.WriteLine("");
            Debug.WriteLine(
                "Time taken for route from {0} to {1} with jump range of {2} was {3}ms",
                routePlanner.Source,
                routePlanner.Destination,
                routePlanner.JumpRange,
                routePlanner.CalculationTime.TotalMilliseconds);
            if (routeFound)
            {
                Debug.WriteLine("Route found with {0} jumps", routePlanner.Route.Count());
                double distance = 0;
                foreach (var routeNode in routePlanner.Route)
                {
                    Debug.WriteLine("{0} : {1:F}ly", routeNode.System, routeNode.Distance);
                    distance += routeNode.Distance;
                }
                Debug.WriteLine("TOTAL {0:F}ly distance", distance);
            }
            else
            {
                Debug.WriteLine("Sorry you probably need more jump range");
            }
            Debug.WriteLine("");
        }

        #endregion
    }
}