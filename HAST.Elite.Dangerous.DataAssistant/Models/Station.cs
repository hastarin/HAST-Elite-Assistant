// ***********************************************************************
// Assembly         : HAST Elite Navigator
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="Station.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST_Elite_Navigator.Models
{
    using global::System.Runtime.Serialization;

    /// <summary>Class Station.</summary>
    public class Station
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Station" /> has a black_market.</summary>
        [DataMember(Name = "black_market")]
        public bool HasBlackMarket { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Station" /> has a commodities_market.</summary>
        [DataMember(Name = "commodities_market")]
        public bool HasCommoditiesMarket { get; set; }

        /// <summary>Gets or sets the distance.</summary>
        [DataMember(Name = "distance")]
        public double Distance { get; set; }

        /// <summary>Gets or sets the faction.</summary>
        [DataMember(Name = "faction")]
        public string Faction { get; set; }

        /// <summary>Gets or sets the largest_pad.</summary>
        [DataMember(Name = "largest_pad")]
        public string LargestPad { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Station" /> has <see cref="outfitting" /> services.</summary>
        [DataMember(Name = "outfitting")]
        public bool HasOutfitting { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Station" /> has a shipyard.</summary>
        [DataMember(Name = "shipyard")]
        public bool HasShipyard { get; set; }

        /// <summary>Gets or sets the station.</summary>
        [DataMember(Name = "station")]
        public string StationName { get; set; }

        /// <summary>Gets or sets the type.</summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        #endregion
    }
}