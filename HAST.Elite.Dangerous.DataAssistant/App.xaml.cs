// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 04-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 08-01-2015
// ***********************************************************************
// <copyright file="App.xaml.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using HAST.Elite.Dangerous.DataAssistant.Properties;

    using log4net;

    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App
    {
        private static ILog log;
        #region Methods

        /// <summary>Raises the <see cref="System.Windows.Application.Startup" /> event.</summary>
        /// <param name="e">A <see cref="StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // ReSharper disable ExceptionNotDocumented
            var filePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            string logFileName = Path.GetDirectoryName(filePath) + "\\log.txt";
            // ReSharper restore ExceptionNotDocumented
            GlobalContext.Properties["LogFileName"] = logFileName;
            log4net.Config.XmlConfigurator.Configure();
            log = LogManager.GetLogger(typeof(App));
            EventManager.RegisterClassHandler(
                typeof(TextBox),
                UIElement.KeyDownEvent,
                new KeyEventHandler(this.TextBoxKeyDown));

            if (!Settings.Default.SettingsUpgraded)
            {
                try
                {
                    Settings.Default.Upgrade();
                }
                catch (ConfigurationErrorsException configurationErrorsException)
                {
                    log.Warn(configurationErrorsException);
                }
                Settings.Default.SettingsUpgraded = true;
                Settings.Default.Save();
            }
        }

        private void MoveToNextUIElement(KeyEventArgs e)
        {
            // Creating a FocusNavigationDirection object and setting it to a
            // local field that contains the direction selected.
            // MoveFocus takes a TraveralReqest as its argument.
            var request = new TraversalRequest(FocusNavigationDirection.Next);

            // Gets the element with keyboard focus.
            var elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
            {
                if (elementWithFocus.MoveFocus(request))
                {
                    e.Handled = true;
                }
            }
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter & ((TextBox)sender).AcceptsReturn == false)
            {
                this.MoveToNextUIElement(e);
            }
        }

        #endregion
    }
}