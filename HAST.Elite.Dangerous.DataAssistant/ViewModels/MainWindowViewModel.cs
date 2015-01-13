// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 08-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 10-01-2015
// ***********************************************************************
// <copyright file="MainWindowViewModel.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.ViewModels
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Speech.Synthesis;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;

    using HAST.Elite.Dangerous.DataAssistant.DataAccessLayer;
    using HAST.Elite.Dangerous.DataAssistant.Migrations;
    using HAST.Elite.Dangerous.DataAssistant.Models;
    using HAST.Elite.Dangerous.DataAssistant.Models.Eddn;
    using HAST.Elite.Dangerous.DataAssistant.Properties;

    using log4net;

    using MahApps.Metro.Controls;

    using Microsoft.Win32;

    using NetMQ;
    using NetMQ.Sockets;

    using SharpDX.Collections;

    /// <summary>Class MainWindowViewModel.</summary>
    public class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region Static Fields

        private static readonly NetMQContext NetMqContext = NetMQContext.Create();

        private static readonly DispatcherTimer ReceiverTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        private static readonly Lazy<MainWindowViewModel> SingletonInstance =
            new Lazy<MainWindowViewModel>(() => new MainWindowViewModel());

        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindowViewModel));

        #endregion

        #region Fields

        private readonly SubscriberSocket eddnSubscriberSocket;

        private readonly LogWatcher logWatcher;

        private readonly RoutePlannerViewModel routePlanner = new RoutePlannerViewModel();

        private readonly SpeechSynthesizer speech = new SpeechSynthesizer();

        private readonly DispatcherTimer speechDelayTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        private readonly DispatcherTimer logRefreshTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        private readonly ObservableCollection<string> systemNames = new ObservableCollection<string>();

        private RelayCommand avoidNextSystemCommand;

        private float backgroundOpacity = 1.0f;

        private string currentSystem = string.Empty;

        private bool disposed;

        private RelayCommand saveSettingsCommand;

        private RelayCommand setSourceToCurrentCommand;

        private RelayCommand speakNextSystemCommand;

        private RelayCommand toggleSettingsFlyoutCommand;

        private RelayCommand toggleTopmostCommand;

        private readonly MetroWindow mainWindow;

        private readonly Dispatcher dispatcher;


        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Prevents a default instance of the <see cref="MainWindowViewModel" /> class from being created.
        /// </summary>
        private MainWindowViewModel()
        {
            Log.Debug("Contructor called.");
            this.InitializeDatabase();

            Log.Debug("Setting up the LogWatcher.");
            this.logWatcher = new LogWatcher();
            this.mainWindow = Application.Current.MainWindow as MetroWindow;
            this.dispatcher = this.MainWindow.Dispatcher;
            this.dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(this.InitializeLogWatcher));

            Log.Debug("Configuring route planner with last used values.");
            this.routePlanner.source = Settings.Default.Source;
            this.routePlanner.RoutePlanner.Source = Settings.Default.Source;
            this.routePlanner.destination = Settings.Default.Destination;
            this.routePlanner.RoutePlanner.Destination = Settings.Default.Destination;
            this.routePlanner.jumpRange = Settings.Default.JumpRange;
            this.routePlanner.RoutePlanner.JumpRange = Settings.Default.JumpRange;
            this.routePlanner.CalculateRoute();

            var repeatNextSystemAfter = Settings.Default.RepeatNextSystemAfter;
            if (repeatNextSystemAfter > 0)
            {
                Log.Debug("Setting up speech delay timer.");
                this.speechDelayTimer.Interval = TimeSpan.FromSeconds(repeatNextSystemAfter);
                this.speechDelayTimer.Tick += this.SpeakNextSystem;
            }

            try
            {
                this.eddnSubscriberSocket = NetMqContext.CreateSubscriberSocket();
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }
            //this.StartListeningToEddn();

            this.MainWindow.Closed += MainWindowOnClosed;
        }

        private void MainWindowOnClosed(object sender, EventArgs eventArgs)
        {
            this.Dispose();
        }

        /// <summary>Finalizes an instance of the <see cref="MainWindowViewModel" /> class.</summary>
        ~MainWindowViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>Gets the Singleton Instance.</summary>
        /// <exception cref="MissingMemberException">
        ///     The <see cref="Lazy{T}" /> instance is initialized to use the default constructor of the type that is
        ///     being lazily initialized, and that type does not have a public, parameterless constructor.
        /// </exception>
        /// <exception cref="MemberAccessException">
        ///     The <see cref="Lazy{T}" /> instance is initialized to use the default constructor of the type that is
        ///     being lazily initialized, and permissions to access the constructor are missing.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     The initialization function tries to access <see cref="Lazy{T}.Value" /> on this instance.
        /// </exception>
        public static MainWindowViewModel Instance
        {
            get
            {
                return SingletonInstance.Value;
            }
        }

        /// <summary>Gets the avoid next system command.</summary>
        public ICommand AvoidNextSystemCommand
        {
            get
            {
                return this.avoidNextSystemCommand
                       ?? (this.avoidNextSystemCommand = new RelayCommand(this.AvoidNextSystem));
            }
        }

        /// <summary>Gets or sets the background opacity.</summary>
        public float BackgroundOpacity
        {
            get
            {
                return this.backgroundOpacity;
            }
            set
            {
                if (this.Set(ref this.backgroundOpacity, value))
                {
                    var brush = this.MainWindow.Background.Clone();
                    brush.Opacity = value;
                    this.MainWindow.Background = brush;
                }
            }
        }

        /// <summary>Gets or sets the current system.</summary>
        public string CurrentSystem
        {
            get
            {
                return this.currentSystem;
            }
            private set
            {
                this.Set(ref this.currentSystem, value);
            }
        }

        /// <summary>Gets the net log watcher.</summary>
        public LogWatcher LogWatcher
        {
            get
            {
                return this.logWatcher;
            }
        }

        /// <summary>Gets the main window.</summary>
        public MetroWindow MainWindow
        {
            get
            {
                return mainWindow;
            }
        }

        /// <summary>Gets the route planner.</summary>
        public RoutePlannerViewModel RoutePlanner
        {
            get
            {
                return this.routePlanner;
            }
        }

        /// <summary>Gets the save settings command.</summary>
        public ICommand SaveSettingsCommand
        {
            get
            {
                return this.saveSettingsCommand
                       ?? (this.saveSettingsCommand = new RelayCommand(() => Settings.Default.Save()));
            }
        }

        /// <summary>Gets the set source to current command.</summary>
        public ICommand SetSourceToCurrentCommand
        {
            get
            {
                return this.setSourceToCurrentCommand
                       ?? (this.setSourceToCurrentCommand =
                           new RelayCommand(() => { this.routePlanner.Source = this.CurrentSystem; }));
            }
        }

        /// <summary>Gets the speak next system command.</summary>
        public ICommand SpeakNextSystemCommand
        {
            get
            {
                return this.speakNextSystemCommand
                       ?? (this.speakNextSystemCommand = new RelayCommand(this.SpeakNextSystem));
            }
        }

        /// <summary>Gets or sets the system names.</summary>
        public ObservableCollection<string> SystemNames
        {
            get
            {
                return this.systemNames;
            }
        }

        /// <summary>Gets the toggle settings flyout command.</summary>
        public ICommand ToggleSettingsFlyoutCommand
        {
            get
            {
                return this.toggleSettingsFlyoutCommand
                       ?? (this.toggleSettingsFlyoutCommand = new RelayCommand(this.ToggleSettingsFlyout));
            }
        }

        /// <summary>Gets the toggle topmost command.</summary>
        public ICommand ToggleTopmostCommand
        {
            get
            {
                return this.toggleTopmostCommand
                       ?? (this.toggleTopmostCommand =
                           new RelayCommand(() => { this.MainWindow.Topmost = !this.MainWindow.Topmost; }));
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
                if (Settings.Default.RememberLastUsed)
                {
                    Settings.Default.JumpRange = this.RoutePlanner.JumpRange;
                    Settings.Default.Source = this.RoutePlanner.Source;
                    Settings.Default.Destination = this.RoutePlanner.Destination;
                    Settings.Default.Save();
                }
                this.RoutePlanner.Dispose();
                this.speech.Dispose();
            }

            // release any unmanaged objects
            // set the object references to null

            this.disposed = true;
        }

        /// <summary>Avoids the next system.</summary>
        private void AvoidNextSystem()
        {
            Log.Debug("AvoidNextSystem called.");
            if (!this.RoutePlanner.Route.Any(r => r.System == this.CurrentSystem))
            {
                return;
            }
            var match = this.RoutePlanner.Route.First(r => r.System == this.CurrentSystem);
            var nextItemIndex = this.RoutePlanner.Route.IndexOf(match) + 1;
            if (nextItemIndex >= this.RoutePlanner.Route.Count)
            {
                return;
            }
            var routeNode = this.RoutePlanner.Route[nextItemIndex];
            this.speech.Speak(string.Format("Avoiding {0} and re-calculating route.", routeNode.System));
            this.RoutePlanner.Avoid(routeNode);
            this.HandleNextSystem();
        }

        /// <summary>Gets the system.</summary>
        /// <param name="name">The name.</param>
        /// <returns>System.</returns>
        private System GetSystem(string name)
        {
            using (var db = new EliteDangerousDbContext())
            {
                return db.Systems.FirstOrDefault(s => s.Name == name);
            }
        }

        /// <summary>Handles the next system.</summary>
        private void HandleNextSystem()
        {
            Log.Debug("HandleNextSystem called.");
            if (!this.routePlanner.Route.Any(r => r.System == this.CurrentSystem))
            {
                return;
            }
            var match = this.RoutePlanner.Route.First(r => r.System == this.CurrentSystem);
            var nextItemIndex = this.RoutePlanner.Route.IndexOf(match) + 1;
            if (nextItemIndex >= this.RoutePlanner.Route.Count)
            {
                if (Settings.Default.SpeakNextSystemDuringJump)
                {
                    this.speech.Speak(string.Format("Entering {0}.  You have reached your destination.", match.System));
                }
                return;
            }
            this.RoutePlanner.SelectedRouteNode = this.RoutePlanner.Route[nextItemIndex];
            if (Settings.Default.AutoCopyNextSystem)
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        var system = this.RoutePlanner.SelectedRouteNode.System;
                        Clipboard.SetDataObject(system, false);
                        i = 10;
                        Log.DebugFormat("{0} copied to the clipboard!", system);
                    }
                    catch
                    {
                        Log.Warn("Unable to write to the clipboard, sleeping for 10ms!");
                        Thread.Sleep(10);
                    }
                } 
            }
            if (Settings.Default.SpeakNextSystemDuringJump)
            {
                this.speech.Speak(
                    string.Format(
                        "Entering {0}.  You're next jump will be to {1}.",
                        match.System,
                        this.RoutePlanner.SelectedRouteNode.System));
                this.speechDelayTimer.Start();
            }
        }

        /// <summary>Initializes the database.</summary>
        private void InitializeDatabase()
        {
            Log.Debug("InitializeDatabase called");
            using (var db = new EliteDangerousDbContext())
            {
                Log.Debug("Upgrading db if needed...");
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<EliteDangerousDbContext, Configuration>());
                Log.Debug("Checking if we already have Systems in the database.");
                if (!Enumerable.Any(db.Systems))
                {
                    Log.Debug("No systems found, attempting to retrieve them from systems.json file.");
                    using (var webClient = new WebClient())
                    {
                        var stream = webClient.OpenRead(Settings.Default.SystemsJsonUri);
                        var serializer = new DataContractJsonSerializer(typeof(System[]));
                        if (stream != null)
                        {
                            Log.Debug("Trying to deserialize systems.json");
                            var systems = (System[])serializer.ReadObject(stream);
                            try
                            {
                                Log.Debug("Saving data to the database");
                                db.Systems.AddRange(systems);
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException ex)
                            {
                                // Retrieve the error messages as a list of strings.
                                foreach (var validationResult in ex.EntityValidationErrors)
                                {
                                    var entityName = validationResult.Entry.Entity.GetType().Name;
                                    foreach (var error in validationResult.ValidationErrors)
                                    {
                                        var message = entityName + "." + error.PropertyName + ": " + error.ErrorMessage;
                                        Log.Warn(message);
                                    }
                                }
                            }
                        }
                    }
                }
                Log.Debug("Getting list of system names from the database.");
                db.Systems.Select(s => s.Name).ToList().ForEach(s => this.SystemNames.Add(s));
                Log.DebugFormat(
                    "{0} systems and {1} stations in the database.",
                    this.SystemNames.Count,
                    db.Stations.Count());
            }
            Log.Debug("InitializeDatabase done");
        }

        /// <summary>Initializes the log watcher.</summary>
        private void InitializeLogWatcher()
        {
            Log.Debug("InitializeLogWatcher called");
            while (!this.logWatcher.IsValidPath())
            {
                var dialog = new OpenFileDialog
                                 {
                                     CheckPathExists = true,
                                     Filter = "netLog files|netLog*.log",
                                     Title = "Please choose one of your netLog files.",
                                     InitialDirectory = this.logWatcher.Path
                                 };
                var result = dialog.ShowDialog(this.MainWindow);
                // ReSharper disable once PossibleInvalidOperationException
                if (result.Value)
                {
                    var path = Path.GetDirectoryName(dialog.FileName);
                    if (this.logWatcher.IsValidPath(path))
                    {
                        Log.DebugFormat("Setting path to {0}", path);
                        this.logWatcher.Path = path;
                    }
                }
                else
                {
                    Log.Warn("User chose to abort LogWatcher setup, Current System functionality will not work!");
                    return;
                }
            }
            Settings.Default.LogsFullPath = this.logWatcher.Path;
            Settings.Default.Save();
            this.logWatcher.StartWatching();
            this.UpdateCurrentSystem();

            this.logRefreshTimer.Interval = TimeSpan.FromSeconds(Settings.Default.LogRefreshIntervalSecs);
            this.logRefreshTimer.Tick += (sender, args) => this.logWatcher.Refresh();
            this.logRefreshTimer.Start();
            this.logWatcher.PropertyChanged +=
                (o, args) =>
                this.dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(this.UpdateCurrentSystem));
            Log.Debug("InitializeLogWatcher done");
        }

        /// <summary>Receivers the timer on tick.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ReceiverTimerOnTick(object sender, EventArgs eventArgs)
        {
            while (this.eddnSubscriberSocket.HasIn)
            {
                byte[] response;
                try
                {
                    response = this.eddnSubscriberSocket.Receive();
                }
                catch (NetMQException)
                {
                    return;
                }

                var decompressedFileStream = new MemoryStream();
                using (decompressedFileStream)
                {
                    var stream = new MemoryStream(response);

                    // Don't forget to ignore the first two bytes of the stream (!)
                    stream.ReadByte();
                    stream.ReadByte();
                    using (var decompressionStream = new DeflateStream(stream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                    }

                    decompressedFileStream.Position = 0;
                    var sr = new StreamReader(decompressedFileStream);
                    var myStr = sr.ReadToEnd();
                    Log.Debug(myStr);

                    decompressedFileStream.Position = 0;
                    var serializer = new DataContractJsonSerializer(typeof(EddnRequest));
                    var rootObject = (EddnRequest)serializer.ReadObject(decompressedFileStream);
                    var message = rootObject.Message;
                    Log.Debug(rootObject.SchemaRef);
                    Log.DebugFormat(
                        "Station: {0}, Item: {1}, BuyPrice: {2}",
                        message.StationName,
                        message.ItemName,
                        message.BuyPrice);

                    decompressedFileStream.Close();
                }
            }
        }

        /// <summary>Speaks the next system.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void SpeakNextSystem(object sender, EventArgs e)
        {
            this.SpeakNextSystem();
        }

        /// <summary>Speaks the next system.</summary>
        private void SpeakNextSystem()
        {
            this.speechDelayTimer.Stop();
            if (!this.routePlanner.Route.Any(r => r.System == this.CurrentSystem))
            {
                if (this.RoutePlanner.Source == this.CurrentSystem)
                {
                    this.speech.Speak("The next system is " + this.RoutePlanner.Route[0].System);
                }
                else
                {
                    this.speech.Speak(
                        "I'm sorry Commander, I'm afraid I can't do that.  Please re-calculate your route.");
                }
                return;
            }
            var match = this.RoutePlanner.Route.First(r => r.System == this.CurrentSystem);
            var nextItemIndex = this.RoutePlanner.Route.IndexOf(match) + 1;
            if (nextItemIndex >= this.RoutePlanner.Route.Count)
            {
                this.speech.Speak("You are already at your destination.");
                return;
            }
            this.speech.Speak("The next system is " + this.RoutePlanner.Route[nextItemIndex].System);
        }

        /// <summary>Starts the listening to eddn.</summary>
        private void StartListeningToEddn()
        {
            this.eddnSubscriberSocket.Connect(Settings.Default.EDDNUri);
            this.eddnSubscriberSocket.Subscribe(Encoding.Unicode.GetBytes(string.Empty));
            this.eddnSubscriberSocket.Options.ReceiveTimeout = new TimeSpan(5);

            ReceiverTimer.Tick += this.ReceiverTimerOnTick;
            ReceiverTimer.Interval = TimeSpan.FromMilliseconds(100);
            ReceiverTimer.Start();
        }

        /// <summary>Toggles the settings flyout.</summary>
        private void ToggleSettingsFlyout()
        {
            Log.Debug("ToggleSettingsFlyout called.");
            var flyout = this.MainWindow.Flyouts.Items[0] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;

            if (!flyout.IsOpen)
            {
                Settings.Default.Save();
            }
        }

        /// <summary>Updates the current system.</summary>
        private void UpdateCurrentSystem()
        {
            Log.Debug("UpdateCurrentSystem called.");
            var system = this.GetSystem(this.logWatcher.CurrentSystem);
            if (system == null)
            {
                this.speech.SpeakAsync("Warning!  The system you are entering is not found in the database!");
            }
            this.CurrentSystem = this.logWatcher.CurrentSystem;
            this.HandleNextSystem();
        }

        #endregion
    }
}