// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="Message.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Models.Eddn
{
    using global::System.Runtime.Serialization;

    /// <summary>Class Message.</summary>
    [DataContract]
    public class Message
    {
        #region Public Properties

        /// <summary>Gets or sets the buy price.</summary>
        [DataMember(Name = "buyPrice")]
        public int BuyPrice { get; set; }

        /// <summary>Gets or sets the demand.</summary>
        [DataMember(Name = "demand ")]
        public int Demand { get; set; }

        /// <summary>Gets or sets the name of the item.</summary>
        [DataMember(Name = "itemName")]
        public string ItemName { get; set; }

        /// <summary>Gets or sets the sell price.</summary>
        [DataMember(Name = "sellPrice")]
        public int SellPrice { get; set; }

        /// <summary>Gets or sets the name of the StationName.</summary>
        [DataMember(Name = "stationName")]
        public string StationName { get; set; }

        /// <summary>Gets or sets the StationName stock.</summary>
        [DataMember(Name = "stationStock ")]
        public int StationStock { get; set; }

        /// <summary>Gets or sets the name of the system.</summary>
        [DataMember(Name = "systemName")]
        public string SystemName { get; set; }

        /// <summary>Gets or sets the timestamp.</summary>
        [DataMember(Name = "timestamp")]
        public string Timestamp { get; set; }

        #endregion
    }
}