// ***********************************************************************
// Assembly         : HAST Elite Navigator
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="System.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

 /// <summary>
/// The Models namespace.
/// </summary>

namespace HAST_Elite_Navigator.Models
{
    using global::System.Collections.Generic;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.Runtime.Serialization;

    /// <summary>Class System.</summary>
    [DataContract]
    public class System
    {
        #region Public Properties

        /// <summary>Gets or sets the allegiance.</summary>
        [DataMember(Name = "allegiance")]
        public string Allegiance { get; set; }

        /// <summary>Gets or sets the date of contribution.</summary>
        [DataMember(Name = "contributed")]
        public string ContributedTimestamp { get; set; }

        /// <summary>Gets or sets the contributor.</summary>
        [DataMember(Name = "contributor")]
        public string Contributor { get; set; }

        /// <summary>Gets or sets the economy.</summary>
        [DataMember(Name = "economy")]
        public string Economy { get; set; }

        /// <summary>Gets or sets the government.</summary>
        [DataMember(Name = "government")]
        public string Government { get; set; }

        /// <summary>Gets or sets the name.</summary>
        [DataMember(Name = "name")]
        [Key]
        public string Name { get; set; }

        /// <summary>Gets or sets the population.</summary>
        [DataMember(Name = "population")]
        public long? Population { get; set; }

        /// <summary>Gets or sets the region.</summary>
        [DataMember(Name = "region")]
        public string Region { get; set; }

        /// <summary>Gets or sets the security.</summary>
        [DataMember(Name = "security")]
        public string Security { get; set; }

        /// <summary>Gets or sets the stations.</summary>
        [DataMember(Name = "stations")]
        public List<Station> Stations { get; set; }

        /// <summary>Gets or sets the x.</summary>
        [DataMember(Name = "x")]
        public double X { get; set; }

        /// <summary>Gets or sets the y.</summary>
        [DataMember(Name = "y")]
        public double Y { get; set; }

        /// <summary>Gets or sets the z.</summary>
        [DataMember(Name = "z")]
        public double Z { get; set; }

        #endregion
    }
}