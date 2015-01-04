// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="EddnRequest.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Models.Eddn
{
    using global::System.Runtime.Serialization;

    /// <summary>Class EddnRequest.</summary>
    [DataContract]
    public class EddnRequest
    {
        #region Public Properties

        /// <summary>Gets or sets the header.</summary>
        [DataMember(Name = "header")]
        public Header Header { get; set; }

        /// <summary>Gets or sets the message.</summary>
        [DataMember(Name = "message")]
        public Message Message { get; set; }

        /// <summary>Gets or sets the schema reference.</summary>
        [DataMember(Name = "$schemaRef")]
        public string SchemaRef { get; set; }

        #endregion
    }
}