// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 04-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant
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
    using System.Windows.Threading;

    using HAST.Elite.Dangerous.DataAssistant.DataAccessLayer;
    using HAST.Elite.Dangerous.DataAssistant.Migrations;
    using HAST.Elite.Dangerous.DataAssistant.Models;
    using HAST.Elite.Dangerous.DataAssistant.Models.Eddn;
    using HAST.Elite.Dangerous.DataAssistant.Properties;
    using HAST.Elite.Dangerous.DataAssistant.ViewModels;

    using Microsoft.Win32;

    using NetMQ;
    using NetMQ.Sockets;

    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow
    {
        #region Static Fields

        private readonly SubscriberSocket eddnSubscriberSocket;

        private static readonly LogWatcher LogWatcher = new LogWatcher();

        private static readonly NetMQContext NetMqContext = NetMQContext.Create();

        private static readonly DispatcherTimer ReceiverTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        #endregion

        #region Fields

        private readonly SpeechSynthesizer speech = new SpeechSynthesizer();

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.eddnSubscriberSocket = NetMqContext.CreateSubscriberSocket(); 
        }

        #endregion

        #region Methods

        private System GetSystem(string name)
        {
            using (var db = new EliteDangerousDbContext())
            {
                return db.Systems.Where(s => s.Name == name).FirstOrDefault();
            }
        }

        /// <summary>Initializes the database.</summary>
        private void InitializeDatabase()
        {
            using (var db = new EliteDangerousDbContext())
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<EliteDangerousDbContext, Configuration>());
                if (Enumerable.Any(db.Stations))
                {
                    return;
                }
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
        }

        private void InitializeLogWatcher()
        {
            while (!LogWatcher.IsValidPath())
            {
                var dialog = new OpenFileDialog
                                 {
                                     CheckPathExists = true,
                                     Filter = "netLog files|netLog*.log",
                                     Title = "Please choose the folder containing your Logs",
                                     InitialDirectory = LogWatcher.Path
                                 };
                var result = dialog.ShowDialog(this);
                if (result.Value)
                {
                    LogWatcher.Path = Path.GetDirectoryName(dialog.FileName);
                }
            }
            Settings.Default.LogsPath = LogWatcher.Path;
            Settings.Default.Save();
            LogWatcher.StartWatching();
            LogWatcher.PropertyChanged += (o, args) =>
                {
                    if (args.PropertyName != "CurrentSystem")
                    {
                        return;
                    }
                    var system = this.GetSystem(LogWatcher.CurrentSystem);
                    if (system == null)
                    {
                        this.speech.SpeakAsync("Warning " + LogWatcher.CurrentSystem + " is not found in the database!");
                    }
                };
        }

        /// <summary>Handles the OnLoaded event of the MainWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.InitializeDatabase();

            this.InitializeLogWatcher();

            this.eddnSubscriberSocket.Connect(Settings.Default.EDDNUri);
            this.eddnSubscriberSocket.Subscribe(Encoding.Unicode.GetBytes(string.Empty));
            this.eddnSubscriberSocket.Options.ReceiveTimeout = new TimeSpan(5);

            ReceiverTimer.Tick += this.ReceiverTimerOnTick;
            ReceiverTimer.Interval = TimeSpan.FromMilliseconds(100);
            ReceiverTimer.Start();
        }

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

        #endregion
    }
}