// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="LogWatcher.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    using HAST.Elite.Dangerous.DataAssistant.Properties;

    /// <summary>
    ///     Class LogWatcher.
    /// </summary>
    public class LogWatcher : FileSystemWatcher, INotifyPropertyChanged
    {
        #region Constants

        /// <summary>
        ///     The default filter
        /// </summary>
        private const string DefaultFilter = @"netLog.*.log";

        /// <summary>
        ///     The default path
        /// </summary>
        private string DefaultPath = Settings.Default.DefaultLogsPath;

        #endregion

        #region Fields

        /// <summary>
        ///     The system line regex
        /// </summary>
        private readonly Regex systemLineRegex = new Regex(
            @"^\{[0-9:]{8}\}\sSystem:\d+\((?<system>[^)]+)",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        ///     The current system.
        /// </summary>
        private string currentSystem;

        /// <summary>
        ///     The last offset
        /// </summary>
        private long lastOffset;

        /// <summary>
        ///     The latest log file
        /// </summary>
        private string latestLogFile;

        /// <summary>
        ///     The property changed
        /// </summary>
        [NonSerialized]
        private PropertyChangedEventHandler propertyChanged;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.IO.FileSystemWatcher" /> class, given the specified directory
        ///     to monitor.
        /// </summary>
        /// <param name="path">The directory to monitor, in standard or Universal Naming Convention (UNC) notation.</param>
        /// <exception cref="System.ArgumentException">path</exception>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The <paramref name="path" /> parameter is an empty string ("").-or- The
        ///     path specified through the <paramref name="path" /> parameter does not exist.
        /// </exception>
        /// <exception cref="T:System.IO.PathTooLongException"><paramref name="path" /> is too long.</exception>
        public LogWatcher(string path)
            : base(path, DefaultFilter)
        {
            this.Path = path;
            if (!Directory.GetFiles(this.Path, this.Filter).Any())
            {
                throw new ArgumentException(
                    string.Format("Directory {0} does not contain netLog files?!", this.Path),
                    "path");
            }
            this.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        public LogWatcher()
        {
            this.Path = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.None);
            if (!string.IsNullOrWhiteSpace(this.Path))
            {
                this.Path = System.IO.Path.Combine(this.Path, this.DefaultPath);
            }
            this.Filter = DefaultFilter;
            if (!Directory.GetFiles(this.Path, this.Filter).Any())
            {
                throw new Exception(string.Format("Directory {0} does not contain netLog files?!", this.Path));
            }
            this.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                this.propertyChanged += value;
            }
            remove
            {
                this.propertyChanged -= value;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the current system.
        /// </summary>
        public string CurrentSystem
        {
            get
            {
                return this.currentSystem;
            }
            set
            {
                this.SetProperty(ref this.currentSystem, value);
            }
        }

        /// <summary>
        ///     Gets or sets the latest log file.
        /// </summary>
        public string LatestLogFile
        {
            get
            {
                return this.latestLogFile;
            }
            set
            {
                this.SetProperty(ref this.latestLogFile, value);
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Starts the watching.
        /// </summary>
        public void StartWatching()
        {
            if (this.EnableRaisingEvents)
            {
                return;
            }
            this.UpdateLatestLogFile();
            this.Created += (sender, args) => this.UpdateLatestLogFile();
            this.CheckForSystemChange();
            this.Changed += (sender, args) => this.CheckForSystemChange();
            this.EnableRaisingEvents = true;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var changedEventHandler = this.propertyChanged;
            if (changedEventHandler == null)
            {
                return;
            }
            changedEventHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Sets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage">The storage.</param>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///     Checks for system change.
        /// </summary>
        private void CheckForSystemChange()
        {
            using (var logFileStream = new FileStream(
                    System.IO.Path.Combine(this.Path, this.latestLogFile),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
            {
                logFileStream.Seek(this.lastOffset, SeekOrigin.Begin);
                using (var sr = new StreamReader(logFileStream))
                {
                    var newData = sr.ReadToEnd();
                    this.lastOffset = logFileStream.Position;
                    var matches = this.systemLineRegex.Matches(newData);
                    if (matches.Count > 0)
                    {
                        var lastSystemFound = matches[matches.Count - 1].Groups["system"].Value;
                        if (this.currentSystem != lastSystemFound)
                        {
                            this.CurrentSystem = lastSystemFound;
                        }
                    }
                    sr.Close();
                }
            }
        }

        /// <summary>
        ///     Updates the latest log file.
        /// </summary>
        private void UpdateLatestLogFile()
        {
            var di = new DirectoryInfo(this.Path);
            var files = di.GetFileSystemInfos();
            var lastFile = files.ToList().Where(fi => fi.Name.StartsWith("netLog")).OrderBy(f => f.Name).Last().Name;
            if (this.latestLogFile != lastFile)
            {
                this.latestLogFile = lastFile;
                this.lastOffset = 0;
            }
        }

        #endregion
    }
}