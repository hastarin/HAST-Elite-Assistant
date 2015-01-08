// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 04-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 08-01-2015
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant
{
    using System;
    using System.Reflection;

    using HAST.Elite.Dangerous.DataAssistant.ViewModels;

    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow
    {
        #region Fields

        /// <summary>The <see cref="HAST.Elite.Dangerous.DataAssistant.MainWindow.version" /></summary>
        private readonly Version version = Assembly.GetExecutingAssembly().GetName().Version;

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = string.Format("{0} ({1})", this.Title, this.version);
            this.AllowsTransparency = true;
        }

        #endregion

        #region Public Properties

        /// <summary>Gets the view model.</summary>
        public MainWindowViewModel ViewModel
        {
            get
            {
                return MainWindowViewModel.Instance;
            }
        }

        #endregion
    }
}