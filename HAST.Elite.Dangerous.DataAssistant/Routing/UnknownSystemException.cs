// ***********************************************************************
// Assembly         : HAST.Elite.Dangerous.DataAssistant
// Author           : Jon Benson
// Created          : 06-01-2015
// 
// Last Modified By : Jon Benson
// Last Modified On : 06-01-2015
// ***********************************************************************
// <copyright file="UnknownSystemException.cs" company="Jon Benson">
//     Copyright (c) Jon Benson. All rights reserved.
// </copyright>
// ***********************************************************************
// 

namespace HAST.Elite.Dangerous.DataAssistant.Routing
{
    using System;

    /// <summary>Represents a timeout when attempting to calculate a route.</summary>
    public class UnknownSystemException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class.
        /// </summary>
        public UnknownSystemException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        public UnknownSystemException(string message)
            : base(message)
        {
        }
    }
}