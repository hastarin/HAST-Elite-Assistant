// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant.UnitTests
// Author           : Jon Benson
// Created          : 06-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="RoutePlanningTests.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.UnitTests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using HAST.Elite.Dangerous.DataAssistant.Routing;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Class RoutePlanningTests.</summary>
    [TestClass]
    public class RoutePlanningTests
    {
        IRoutePlanner routePlanner = new RoutePlanner();
        #region Public Methods and Operators

        [TestMethod]
        public void LongRoute()
        {
            routePlanner.Source = "Er Tcher";
            routePlanner.Destination = "Lave";
            routePlanner.JumpRange = 20;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Er Tcher";
            routePlanner.Destination = "Kupoledari";
            routePlanner.JumpRange = 20;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Er Tcher";
            routePlanner.Destination = "Lave";
            routePlanner.JumpRange = 16;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Er Tcher";
            routePlanner.Destination = "Kupoledari";
            routePlanner.JumpRange = 16;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);
        }

        [TestMethod]
        public void ShortRoute()
        {
            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "He Bo";
            routePlanner.JumpRange = 20;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "He Bo";
            routePlanner.JumpRange = 5;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);
            Assert.IsNotNull(routePlanner.Route);

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "He Bo";
            routePlanner.JumpRange = 4;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);
            Assert.IsNull(routePlanner.Route);
        }

        /// <summary>Tests a class that supports <see cref="IRoutePlanner"/> against a popular route.</summary>
        [TestMethod]
        public void PopularRoute()
        {
            Debug.WriteLine("Popular route test");

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "Leesti";
            routePlanner.JumpRange = 20;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "Leesti";
            routePlanner.JumpRange = 18;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "Leesti";
            routePlanner.JumpRange = 16;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "Leesti";
            routePlanner.JumpRange = 12;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "Leesti";
            routePlanner.JumpRange = 11;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);
            Assert.IsNotNull(routePlanner.Route);

            routePlanner.Source = "Ethgreze";
            routePlanner.Destination = "Leesti";
            routePlanner.JumpRange = 10;
            routePlanner.AvoidSystems = new List<string>(new[] { "Alioth", "Sol" });
            this.TimeAndOutputRoute(routePlanner);
            Assert.IsNull(routePlanner.Route);
        }

        #endregion

        #region Methods

        /// <summary>Times the and output route.</summary>
        /// <param name="routePlanner">The route planner.</param>
        private void TimeAndOutputRoute(IRoutePlanner routePlanner)
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
                    //Debug.WriteLine("{0} : {1:F}ly", routeNode.System, routeNode.Distance);
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