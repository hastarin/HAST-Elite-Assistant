// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 07-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 10-01-2015
// ***********************************************************************
// <copyright file="RoutePlannerViewModel.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;

    using HAST.Elite.Dangerous.DataAssistant.Routing;

    /// <summary>Class RoutePlannerViewModel.</summary>
    public class RoutePlannerViewModel : ObservableObject, IDisposable
    {
        #region Fields

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.route" />
        /// </summary>
        private readonly ObservableCollection<IRouteNode> route = new ObservableCollection<IRouteNode>();

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.route" /> planner
        /// </summary>
        private readonly IRoutePlanner routePlanner = new RoutePlanner();

        /// <summary>
        ///     <para>
        ///         The calculate <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.route" />
        ///     </para>
        ///     <para>command</para>
        /// </summary>
        private RelayCommand calculateRouteCommand;

        /// <summary>The calculation time</summary>
        private TimeSpan calculationTime;

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.destination" />
        /// </summary>
        private string destination;

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.disposed" />
        /// </summary>
        private bool disposed;

        private double distance;

        /// <summary>The jump range</summary>
        private float jumpRange;

        private string nextSystem;

        private int numberOfJumps;

        private IRouteNode selectedRouteNode;

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.source" />
        /// </summary>
        private string source;

        /// <summary>The swap systems command</summary>
        private RelayCommand swapSystemsCommand;

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.timeout" />
        /// </summary>
        private TimeSpan timeout;

        private bool wasRouteFound;

        #endregion

        #region Constructors and Destructors

        /// <summary>Finalizes an instance of the <see cref="RoutePlannerViewModel" /> class.</summary>
        ~RoutePlannerViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     <para>
        ///         Gets the calculate <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.route" />
        ///     </para>
        ///     <para>command.</para>
        /// </summary>
        public ICommand CalculateRouteCommand
        {
            get
            {
                return this.calculateRouteCommand
                       ?? (this.calculateRouteCommand =
                           new RelayCommand(
                               this.CalculateRoute,
                               () =>
                               !string.IsNullOrWhiteSpace(this.Source) && !string.IsNullOrWhiteSpace(this.Destination)));
            }
        }

        /// <summary>Gets the calculation time.</summary>
        public TimeSpan CalculationTime
        {
            get
            {
                return this.calculationTime;
            }
            private set
            {
                this.Set(ref this.calculationTime, value);
            }
        }

        /// <summary>Gets or sets the destination.</summary>
        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                if (this.Set(ref this.destination, value))
                {
                    this.routePlanner.Destination = value;
                    this.CalculateRoute();
                }
            }
        }

        /// <summary>Gets or sets the distance.</summary>
        public double Distance
        {
            get
            {
                return this.distance;
            }
            set
            {
                this.Set(ref this.distance, value);
            }
        }

        /// <summary>Gets or sets the jump range.</summary>
        public float JumpRange
        {
            get
            {
                return this.jumpRange;
            }
            set
            {
                if (this.Set(ref this.jumpRange, value))
                {
                    this.routePlanner.JumpRange = value;
                    this.CalculateRoute();
                }
            }
        }

        /// <summary>Gets or sets the next system.</summary>
        public string NextSystem
        {
            get
            {
                return this.nextSystem;
            }
            private set
            {
                this.Set(ref this.nextSystem, value);
            }
        }

        /// <summary>Gets or sets the number of jumps.</summary>
        public int NumberOfJumps
        {
            get
            {
                return this.numberOfJumps;
            }
            set
            {
                this.Set(ref this.numberOfJumps, value);
            }
        }

        /// <summary>Gets or sets the route.</summary>
        public ObservableCollection<IRouteNode> Route
        {
            get
            {
                return this.route;
            }
        }

        /// <summary>
        ///     Gets or sets the selected
        ///     <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.route" /> node.
        /// </summary>
        public IRouteNode SelectedRouteNode
        {
            get
            {
                return this.selectedRouteNode;
            }
            set
            {
                this.Set(ref this.selectedRouteNode, value);
            }
        }

        /// <summary>Gets or sets the source.</summary>
        public string Source
        {
            get
            {
                return this.source;
            }
            set
            {
                if (this.Set(ref this.source, value))
                {
                    this.routePlanner.Source = value;
                    this.CalculateRoute();
                }
            }
        }

        /// <summary>Gets the swap systems command.</summary>
        public ICommand SwapSystemsCommand
        {
            get
            {
                return this.swapSystemsCommand ?? (this.swapSystemsCommand = new RelayCommand(
                                                                                 () =>
                                                                                     {
                                                                                         var temp = this.Source;
                                                                                         this.Source = this.Destination;
                                                                                         this.Destination = temp;
                                                                                     }));
            }
        }

        /// <summary>Gets or sets the timeout.</summary>
        public TimeSpan Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                if (this.Set(ref this.timeout, value))
                {
                    this.routePlanner.Timeout = this.Timeout;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether a
        ///     <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.route" /> was found on the
        ///     last calculation.
        /// </summary>
        public bool WasRouteFound
        {
            get
            {
                return this.wasRouteFound;
            }
            set
            {
                this.Set(ref this.wasRouteFound, value);
                if (!value)
                {
                    this.CalculationTime = TimeSpan.MinValue;
                    this.NumberOfJumps = 0;
                    this.Distance = 0;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>Avoids the specified <see cref="route" /> node.</summary>
        /// <param name="routeNode">The <see cref="route" /> node.</param>
        public void Avoid(IRouteNode routeNode)
        {
            this.routePlanner.AvoidSystems.Add(routeNode.System);
            this.CalculateRoute();
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
                this.routePlanner.Dispose();
            }

            // release any unmanaged objects
            // set the object references to null

            this.disposed = true;
        }

        /// <summary>Calculates the route.</summary>
        private void CalculateRoute()
        {
            this.Route.Clear();
            try
            {
                this.WasRouteFound = this.routePlanner.Calculate();
                if (!this.WasRouteFound)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                this.WasRouteFound = false;
            }
            foreach (var node in this.routePlanner.Route)
            {
                this.Route.Add(node);
            }
            this.CalculationTime = this.routePlanner.CalculationTime;
            this.NumberOfJumps = this.Route.Count;
            this.Distance = this.Route.Sum(r => r.Distance);
        }

        #endregion
    }
}