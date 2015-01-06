// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 04-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="Station.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Models
{
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;
    using global::System.Runtime.Serialization;

    /// <summary>Class Station.</summary>
    [DataContract]
    public class Station
    {
        #region Public Properties

        /// <summary>Gets or sets the distance.</summary>
        [DataMember(Name = "distance")]
        public double Distance { get; set; }

        /// <summary>Gets or sets the faction.</summary>
        [DataMember(Name = "faction")]
        public string Faction { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Station" /> has a black market.</summary>
        [DataMember(Name = "black_market")]
        public bool HasBlackMarket { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Station" /> has a commodities market.
        /// </summary>
        [DataMember(Name = "commodities_market")]
        public bool HasCommoditiesMarket { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Station" /> has outfitting services.
        /// </summary>
        [DataMember(Name = "outfitting")]
        public bool HasOutfitting { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Station" /> has a shipyard.</summary>
        [DataMember(Name = "shipyard")]
        public bool HasShipyard { get; set; }

        /// <summary>Gets or sets the largest pad available.</summary>
        [DataMember(Name = "largest_pad")]
        public string LargestPad { get; set; }

        /// <summary>Gets or sets the station name.</summary>
        [DataMember(Name = "station")]
        [Key]
        [Column(Order = 2)]
        public string StationName { get; set; }

        /// <summary>Gets or sets the system identifier.</summary>
        [Key]
        [Column(Order = 1)]
        public long SystemId { get; set; }

        /// <summary>Gets or sets the type.</summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        #endregion
    }
}