// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 07-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 07-01-2015
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
    using System.Windows.Input;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;

    using HAST.Elite.Dangerous.DataAssistant.Routing;

    /// <summary>Class RoutePlannerViewModel.</summary>
    public class RoutePlannerViewModel : ObservableObject, IDisposable
    {
        #region Fields

        private readonly IRoutePlanner routePlanner = new RoutePlanner();
        private RelayCommand calculateRouteCommand;
        private TimeSpan calculationTime;
        private string destination;
        private bool disposed;
        private float jumpRange;
        private readonly ObservableCollection<IRouteNode> route = new ObservableCollection<IRouteNode>();
        private string source;
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

        /// <summary>Gets the calculate <see cref="route" /> command.</summary>
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
                    this.routePlanner.Destination = value;
                }
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
            if (!this.routePlanner.Calculate())
            {
                return;
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