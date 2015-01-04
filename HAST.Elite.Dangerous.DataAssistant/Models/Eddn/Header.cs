// ***********************************************************************
// Assembly         : HAST Elite Navigator
// Author           : Jon Benson
// Created          : 03-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 03-01-2015
// ***********************************************************************
// <copyright file="Header.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST_Elite_Navigator.Models.Eddn
{
    using global::System.Runtime.Serialization;

    /// <summary>Class Header.</summary>
    [DataContract]
    public class Header
    {
        #region Public Properties

        /// <summary>Gets or sets the gateway timestamp.</summary>
        [DataMember(Name = "gatewayTimestamp")]
        public string GatewayTimestamp { get; set; }

        /// <summary>Gets or sets the name of the software.</summary>
        [DataMember(Name = "softwareName")]
        public string SoftwareName { get; set; }

        /// <summary>Gets or sets the software version.</summary>
        [DataMember(Name = "softwareVersion")]
        public string SoftwareVersion { get; set; }

        /// <summary>Gets or sets the uploader identifier.</summary>
        [DataMember(Name = "uploaderID")]
        public string UploaderID { get; set; }

        #endregion
    }
}