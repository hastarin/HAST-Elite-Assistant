// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 07-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 08-01-2015
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
    using System.Windows.Input;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;

    using HAST.Elite.Dangerous.DataAssistant.Routing;

    /// <summary>Class RoutePlannerViewModel.</summary>
    public class RoutePlannerViewModel : ObservableObject, IDisposable
    {
        #region Fields

        /// <summary>The <see cref="route" /></summary>
        private readonly ObservableCollection<IRouteNode> route = new ObservableCollection<IRouteNode>();

        /// <summary>The <see cref="route" /> planner</summary>
        private readonly IRoutePlanner routePlanner = new RoutePlanner();

        /// <summary>The calculate <see cref="route" /> command</summary>
        private RelayCommand calculateRouteCommand;

        /// <summary>The calculation time</summary>
        private TimeSpan calculationTime;

        /// <summary>The <see cref="destination" /></summary>
        private string destination;

        /// <summary>The <see cref="disposed" /></summary>
        private bool disposed;

        /// <summary>The jump range</summary>
        private float jumpRange;

        private IRouteNode selectedRouteNode;

        /// <summary>The <see cref="source" /></summary>
        private string source;

        /// <summary>The swap systems command</summary>
        private RelayCommand swapSystemsCommand;

        /// <summary>The <see cref="timeout" /></summary>
        private TimeSpan timeout;

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
        ///     Gets the calculate <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.RoutePlannerViewModel.route" />
        ///     command.
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

        /// <summary>Gets or sets the route.</summary>
        public ObservableCollection<IRouteNode> Route
        {
            get
            {
                return this.route;
            }
        }

        /// <summary>Gets or sets the selected <see cref="route" /> node.</summary>
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

        #endregion

        #region Public Methods and Operators

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
                var result = this.routePlanner.Calculate();
                if (!result)
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            foreach (var node in this.routePlanner.Route)
            {
                this.Route.Add(node);
            }
            this.CalculationTime = this.routePlanner.CalculationTime;
        }

        #endregion
    }
}