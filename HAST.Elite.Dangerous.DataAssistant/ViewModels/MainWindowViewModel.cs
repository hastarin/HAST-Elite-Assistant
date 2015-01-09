// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 08-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 09-01-2015
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
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Speech.Synthesis;
    using System.Text;
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

    using Microsoft.Win32;

    using NetMQ;
    using NetMQ.Sockets;

    using SharpDX.Collections;

    /// <summary>Class MainWindowViewModel.</summary>
    public class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region Static Fields

        /// <summary>The net mq context</summary>
        private static readonly NetMQContext NetMqContext = NetMQContext.Create();

        /// <summary>The receiver timer</summary>
        private static readonly DispatcherTimer ReceiverTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        /// <summary>The singleton instance</summary>
        private static readonly Lazy<MainWindowViewModel> SingletonInstance =
            new Lazy<MainWindowViewModel>(() => new MainWindowViewModel());

        #endregion

        #region Fields

        /// <summary>The eddn subscriber socket</summary>
        private readonly SubscriberSocket eddnSubscriberSocket;

        /// <summary>The route planner</summary>
        private readonly RoutePlannerViewModel routePlanner = new RoutePlannerViewModel();

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.MainWindowViewModel.speech" />
        /// </summary>
        private readonly SpeechSynthesizer speech = new SpeechSynthesizer();

        private readonly ObservableCollection<string> systemNames = new ObservableCollection<string>();

        private float backgroundOpacity = 1.0f;

        private string currentSystem;

        /// <summary>
        ///     The <see cref="HAST.Elite.Dangerous.DataAssistant.ViewModels.MainWindowViewModel.disposed" />
        /// </summary>
        private bool disposed;

        /// <summary>The log watcher</summary>
        private LogWatcher logWatcher;

        private RelayCommand setSourceToCurrentCommand;

        private RelayCommand toggleTopmostCommand;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Prevents a default instance of the <see cref="MainWindowViewModel" /> class from being created.
        /// </summary>
        private MainWindowViewModel()
        {
            try
            {
                this.InitializeDatabase();

                this.InitializeLogWatcher();

                this.routePlanner.Source = "Ethgreze";
                this.routePlanner.Destination = "Leesti";
                this.routePlanner.JumpRange = 20;

                this.eddnSubscriberSocket = NetMqContext.CreateSubscriberSocket();
                //this.StartListeningToEddn();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            //this.StartListeningToEddn();
        }

        /// <summary>Finalizes an instance of the <see cref="MainWindowViewModel" /> class.</summary>
        ~MainWindowViewModel()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>Gets the Singleton Instance.</summary>
        /// <exception cref="System.MissingMemberException">
        ///     The <see cref="System.Lazy`1" /> instance is initialized to use the default constructor of the type that is
        ///     being lazily initialized, and that type does not have a public, parameterless constructor.
        /// </exception>
        /// <exception cref="System.MemberAccessException">
        ///     The <see cref="System.Lazy`1" /> instance is initialized to use the default constructor of the type that is
        ///     being lazily initialized, and permissions to access the constructor are missing.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     The initialization function tries to access <see cref="System.Lazy`1.Value" /> on this instance.
        /// </exception>
        public static MainWindowViewModel Instance
        {
            get
            {
                return SingletonInstance.Value;
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
        public Window MainWindow
        {
            get
            {
                return Application.Current.MainWindow;
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

        /// <summary>Gets or sets the system names.</summary>
        public ObservableCollection<string> SystemNames
        {
            get
            {
                return this.systemNames;
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
            }

            // release any unmanaged objects
            // set the object references to null

            this.disposed = true;
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

        /// <summary>Initializes the database.</summary>
        private void InitializeDatabase()
        {
            using (var db = new EliteDangerousDbContext())
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<EliteDangerousDbContext, Configuration>());
                if (!Enumerable.Any(db.Stations))
                {
                    using (var webClient = new WebClient())
                    {
                        var stream = webClient.OpenRead(Settings.Default.SystemsJsonUri);
                        var serializer = new DataContractJsonSerializer(typeof(System[]));
                        if (stream != null)
                        {
                            var systems = (System[])serializer.ReadObject(stream);
                            try
                            {
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
                                        Debug.WriteLine(message);
                                    }
                                }
                            }
                        }
                    }
                }
                db.Systems.Select(s => s.Name).ToList().ForEach(s => this.SystemNames.Add(s));
            }
        }

        /// <summary>Initializes the log watcher.</summary>
        private void InitializeLogWatcher()
        {
            this.logWatcher = new LogWatcher();
            while (!this.logWatcher.IsValidPath())
            {
                var dialog = new OpenFileDialog
                                 {
                                     CheckPathExists = true,
                                     Filter = "netLog files|netLog*.log",
                                     Title = "Please choose the folder containing your Logs",
                                     InitialDirectory = this.logWatcher.Path
                                 };
                var result = dialog.ShowDialog();
                // ReSharper disable once PossibleInvalidOperationException
                if (result.Value)
                {
                    this.logWatcher.Path = Path.GetDirectoryName(dialog.FileName);
                }
                else
                {
                    MessageBox.Show("Sorry that's not a valid path, please try again!");
                }
            }
            Settings.Default.LogsFullPath = this.logWatcher.Path;
            Settings.Default.Save();
            this.logWatcher.StartWatching();
            this.UpdateCurrentSystem();
            this.logWatcher.PropertyChanged += (o, args) =>
                {
                    if (args.PropertyName != "CurrentSystem")
                    {
                        return;
                    }
                    this.UpdateCurrentSystem();
                };
        }

        /// <summary>Receivers the timer on tick.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="System.EventArgs" /> instance containing the event data.</param>
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
                    Debug.WriteLine(myStr);

                    decompressedFileStream.Position = 0;
                    var serializer = new DataContractJsonSerializer(typeof(EddnRequest));
                    var rootObject = (EddnRequest)serializer.ReadObject(decompressedFileStream);
                    var message = rootObject.Message;
                    Debug.WriteLine(rootObject.SchemaRef);
                    Debug.WriteLine(
                        "Station: {0}, Item: {1}, BuyPrice: {2}",
                        message.StationName,
                        message.ItemName,
                        message.BuyPrice);

                    decompressedFileStream.Close();
                }
            }
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

        private void UpdateCurrentSystem()
        {
            var system = this.GetSystem(this.logWatcher.CurrentSystem);
            if (system == null)
            {
                this.speech.SpeakAsync("Warning " + this.logWatcher.CurrentSystem + " is not found in the database!");
            }
            this.CurrentSystem = this.logWatcher.CurrentSystem;
            if (!this.routePlanner.Route.Any(r => r.System == this.CurrentSystem))
            {
                return;
            }
            var match = this.RoutePlanner.Route.First(r => r.System == this.CurrentSystem);
            var nextItemIndex = this.RoutePlanner.Route.IndexOf(match) + 1;
            if (nextItemIndex >= this.RoutePlanner.Route.Count)
            {
                return;
            }
            this.RoutePlanner.SelectedRouteNode = this.RoutePlanner.Route[nextItemIndex];
            if (Settings.Default.AutoCopyNextSystem)
            {
                Clipboard.SetText(this.RoutePlanner.SelectedRouteNode.System);
            }
        }

        #endregion
    }
}