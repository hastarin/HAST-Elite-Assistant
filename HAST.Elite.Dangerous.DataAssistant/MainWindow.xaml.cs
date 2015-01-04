// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant
{
    using System;
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
    using HAST.Elite.Dangerous.DataAssistant.Models.Eddn;
    using HAST.Elite.Dangerous.DataAssistant.Properties;
    using HAST.Elite.Dangerous.DataAssistant.ViewModels;
    using HAST.Elite.Dangerous.DataAssistant.Models;

    using NetMQ;
    using NetMQ.Sockets;

    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow
    {
        #region Static Fields

        /// <summary>The URI used to retrieve the initial data for systems in JSON format</summary>
        private const string SystemJsonUri = @"https://raw.githubusercontent.com/hastarin/ed-systems/master/systems.json";

        #endregion

        #region Fields

        /// <summary>The <see cref="MainWindow.speak" /></summary>
        private readonly SpeechSynthesizer speak = new SpeechSynthesizer();

        private static readonly NetMQContext NetMqContext = NetMQContext.Create();

        private static readonly SubscriberSocket EddnSubscriberSocket = NetMqContext.CreateSubscriberSocket();

        private static readonly DispatcherTimer ReceiverTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>Initializes the database.</summary>
        private void InitializeDatabase()
        {
            using (var db = new EliteDangerousDbContext())
            {
                if (Enumerable.Any(db.Stations))
                {
                    return;
                }
                using (var webClient = new WebClient())
                {
                    var stream = webClient.OpenRead(SystemJsonUri);
                    var serializer = new DataContractJsonSerializer(typeof(System[]));
                    if (stream != null)
                    {
                        var systems = (System[])serializer.ReadObject(stream);
                        db.Systems.AddRange(systems);
                        db.SaveChanges();
                    }
                }
            }
        }

        /// <summary>Handles the OnLoaded event of the MainWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.InitializeDatabase();

            var logWatch = new LogWatcher();
            logWatch.StartWatching();
            logWatch.PropertyChanged += (o, args) =>
                {
                    if (args.PropertyName != "CurrentSystem")
                    {
                        return;
                    }
                    var system = this.GetSystem(logWatch.CurrentSystem);
                    if (system == null)
                    {
                        this.speak.SpeakAsync("Warning " + logWatch.CurrentSystem + " is not found in the database!");
                    }
                };

            EddnSubscriberSocket.Connect(Settings.Default.EDDNUri);
            EddnSubscriberSocket.Subscribe(Encoding.Unicode.GetBytes(string.Empty));
            EddnSubscriberSocket.Options.ReceiveTimeout = new TimeSpan(5);

            ReceiverTimer.Tick += ReceiverTimerOnTick;
            ReceiverTimer.Interval = TimeSpan.FromMilliseconds(100);
            ReceiverTimer.Start();

        }

        private void ReceiverTimerOnTick(object sender, EventArgs eventArgs)
        {
            while (EddnSubscriberSocket.HasIn)
            {
                byte[] response;
                try
                {
                    response = EddnSubscriberSocket.Receive();
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
                    Debug.WriteLine("Station: {0}, Item: {1}, BuyPrice: {2}", message.StationName, message.ItemName, message.BuyPrice);

                    decompressedFileStream.Close();
                }
            }
        }

        private System GetSystem(string name)
        {
            using (var db = new EliteDangerousDbContext())
            {
                return db.Systems.Where(s => s.Name == name).FirstOrDefault();
            }
        }

        #endregion
    }
}