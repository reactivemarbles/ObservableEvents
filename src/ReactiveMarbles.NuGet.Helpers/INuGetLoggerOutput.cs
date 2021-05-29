// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveMarbles.NuGet.Helpers
{
    /// <summary>
    /// Provides logging to the NuGet helpers.
    /// </summary>
    public interface INuGetLoggerOutput
    {
        /// <summary>
        /// Outputs a warning string.
        /// </summary>
        /// <param name="data">The logging data.</param>
        void Warn(string data);

        /// <summary>
        /// Outputs a error string.
        /// </summary>
        /// <param name="data">The logging data.</param>
        void Error(string data);

        /// <summary>
        /// Outputs a information string.
        /// </summary>
        /// <param name="data">The logging data.</param>
        void Info(string data);

        /// <summary>
        /// Outputs a debug string.
        /// </summary>
        /// <param name="data">The logging data.</param>
        void Debug(string data);
    }
}
