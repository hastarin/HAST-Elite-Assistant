// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 04-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 10-01-2015
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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    using HAST.Elite.Dangerous.DataAssistant.Properties;

    /// <summary>
    ///     Class LogWatcher.
    /// </summary>
    [DesignerCategory("")]
    public class LogWatcher : FileSystemWatcher, INotifyPropertyChanged
    {
        #region Constants

        /// <summary>
        ///     The default filter
        /// </summary>
        private const string DefaultFilter = @"netLog.*.log";

        #endregion

        #region Fields

        /// <summary>
        ///     The default path
        /// </summary>
        private readonly string defaultPath = Settings.Default.DefaultLogsPath;

        /// <summary>
        ///     The system line regex
        /// </summary>
        private readonly Regex systemLineRegex = new Regex(
            @"^\{[0-9:]{8}\}\sSystem:\d+\((?<system>[^)]+)",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        ///     The current system.
        /// </summary>
        private string currentSystem = string.Empty;

        /// <summary>
        ///     The last offset used when reading the netLog file.
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
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public LogWatcher()
        {
            this.Filter = DefaultFilter;
            this.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
            if (Settings.Default.LogsFullPath != null && Directory.Exists(Settings.Default.LogsFullPath))
            {
                this.Path = Settings.Default.LogsFullPath;
            }
            else
            {
                var localAppDataPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.None);
                try
                {
                    var path = System.IO.Path.Combine(localAppDataPath, this.defaultPath);
                    this.Path = path;
                }
                catch (ArgumentException ae)
                {
                    Debug.WriteLine(ae);
                }
            }
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
                // ReSharper disable once DelegateSubtraction
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
        ///     Determines whether the Path contains netLog files.
        /// </summary>
        /// <returns><c>true</c> if the Path contains netLog files; otherwise, <c>false</c>.</returns>
        public bool IsValidPath()
        {
            return this.IsValidPath(this.Path);
        }

        /// <summary>
        ///     Determines whether the specified path contains netLog files.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the specified path contains netLog files; otherwise, <c>false</c>.</returns>
        public bool IsValidPath(string path)
        {
            var filesFound = false;
            try
            {
                filesFound = Directory.GetFiles(path, this.Filter).Any();
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            return filesFound;
        }

        /// <summary>
        ///     Forces a read of the log file to check for a system change.
        /// </summary>
        public void Refresh()
        {
            this.CheckForSystemChange();
        }

        /// <summary>
        ///     Starts the watching.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Throws an exception if the <see cref="Path" /> does not contain netLogs
        ///     files.
        /// </exception>
        public void StartWatching()
        {
            if (this.EnableRaisingEvents)
            {
                return;
            }
            if (!this.IsValidPath())
            {
                throw new InvalidOperationException(
                    string.Format("Directory {0} does not contain netLog files?!", this.Path));
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
        /// <param name="propertyName">StationName of the property.</param>
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
        /// <param name="propertyName">StationName of the property.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            // ReSharper disable once ExplicitCallerInfoArgument
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///     Checks for system change.
        /// </summary>
        private void CheckForSystemChange()
        {
            using (
                var logFileStream = new FileStream(
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
        ///     Updates the <see cref="LatestLogFile" /> property.
        /// </summary>
        private void UpdateLatestLogFile()
        {
            var di = new DirectoryInfo(this.Path);
            var files = di.GetFileSystemInfos();
            var lastFile = files.ToList().Where(fi => fi.Name.StartsWith("netLog")).OrderBy(f => f.Name).Last().Name;
            if (this.latestLogFile != lastFile)
            {
                this.LatestLogFile = lastFile;
                this.lastOffset = 0;
            }
        }

        #endregion
    }
}