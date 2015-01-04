// ***********************************************************************
// Assembly         : HAST Elite Navigator
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="RootObject.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST_Elite_Navigator.Models.Eddn
{
    using global::System.Runtime.Serialization;

    /// <summary>Class RootObject.</summary>
    [DataContract]
    public class RootObject
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