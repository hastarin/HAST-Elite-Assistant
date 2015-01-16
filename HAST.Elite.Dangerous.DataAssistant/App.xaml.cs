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
    using System.Windows.Threading;

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

            try
            {
                var filePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                string logFileName = Path.GetDirectoryName(filePath) + "\\log.txt";
                GlobalContext.Properties["LogFileName"] = logFileName;
                log4net.Config.XmlConfigurator.Configure();
                log = LogManager.GetLogger(typeof(App));
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show(
                    "There was an error in the application configuration.  Please check your configuration.");
            }
            catch (PathTooLongException)
            {
                MessageBox.Show(
                    "The path to your configuration files is too long?!");
            }

            // Make sure we handle any "unhandled" exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Setup Quick Converter.
            // Add the System namespace so we can use primitive types (i.e. int, etc.).
            QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
            // Add the System.Windows namespace so we can use Visibility.Collapsed, etc.
            QuickConverter.EquationTokenizer.AddNamespace(typeof(Visibility));

            EventManager.RegisterClassHandler(
                typeof(TextBox),
                UIElement.KeyDownEvent,
                new KeyEventHandler(TextBoxKeyDown));

            if (Settings.Default.SettingsUpgraded)
            {
                return;
            }
            log.Debug("Upgrading settings from previous version.");
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
            log.Debug("Upgrade done.");
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!e.IsTerminating)
            {
                return;
            }

            var message = "Unhandled exception:" + Environment.NewLine + e.ExceptionObject;

            log.Fatal(message);
        }

        /// <summary>Catch unhandled exceptions thrown by the main UI thread.</summary>
        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var errorMessage = string.Format("An application error occurred. If this error occurs again there seems to be a serious bug in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will try to continue, if you click No the application will close)", e.Exception.Message);

            log.Warn("Dispatcher Unhandled Exception", e.Exception);
            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                Current.Shutdown();
            }
            e.Handled = true;
        }

        /// <summary>
        /// Moves to next UI element.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private static void MoveToNextUiElement(KeyEventArgs e)
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

        /// <summary>
        /// Moves focus to the next UI element when a user hits enter in a text box that doesn't accept returns.
        /// </summary>
        private static void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter & ((TextBox)sender).AcceptsReturn == false)
            {
                MoveToNextUiElement(e);
            }
        }

        #endregion
    }
}