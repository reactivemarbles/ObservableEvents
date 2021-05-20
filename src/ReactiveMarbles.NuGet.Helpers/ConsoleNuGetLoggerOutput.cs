// Copyright (c) 2019-2021 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace ReactiveMarbles.NuGet.Helpers
{
    /// <summary>
    /// Provides logging to the console.
    /// </summary>
    public class ConsoleNuGetLoggerOutput : INuGetLoggerOutput
    {
        /// <inheritdoc/>
        public void Debug(string data)
        {
            Console.WriteLine("[DEBUG]: " + data);
        }

        /// <inheritdoc/>
        public void Error(string data)
        {
            Console.WriteLine("[ERROR]: " + data);
        }

        /// <inheritdoc/>
        public void Info(string data)
        {
            Console.WriteLine("[INFO]: " + data);
        }

        /// <inheritdoc/>
        public void Warn(string data)
        {
            Console.WriteLine("[WARN]: " + data);
        }
    }
}
