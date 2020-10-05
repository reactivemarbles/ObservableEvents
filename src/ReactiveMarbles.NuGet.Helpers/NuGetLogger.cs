// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;

using NuGet.Common;

using LogLevel = NuGet.Common.LogLevel;

namespace ReactiveMarbles.NuGet.Helpers
{
    /// <summary>
    /// A logger provider for the NuGet clients API.
    /// </summary>
    internal class NuGetLogger : ILogger
    {
        private INuGetLoggerOutput _logger;

        public NuGetLogger(INuGetLoggerOutput? logger = null)
        {
            _logger = logger ?? new ConsoleNuGetLoggerOutput();
        }

        /// <inheritdoc />
        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    _logger.Warn(data);
                    break;
                case LogLevel.Error:
                    _logger.Error(data);
                    break;
                case LogLevel.Information:
                    _logger.Info(data);
                    break;
                case LogLevel.Debug:
                    _logger.Debug(data);
                    break;
                default:
                    _logger.Info(data);
                    break;
            }
        }

        /// <inheritdoc />
        public void Log(ILogMessage message)
        {
            Log(message.Level, message.Message);
        }

        /// <inheritdoc />
        public Task LogAsync(LogLevel level, string data)
        {
            Log(level, data);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task LogAsync(ILogMessage message)
        {
            Log(message);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void LogDebug(string data)
        {
            _logger.Debug(data);
        }

        /// <inheritdoc />
        public void LogError(string data)
        {
            _logger.Error(data);
        }

        /// <inheritdoc />
        public void LogInformation(string data)
        {
            _logger.Info(data);
        }

        /// <inheritdoc />
        public void LogInformationSummary(string data)
        {
            _logger.Info(data);
        }

        /// <inheritdoc />
        public void LogMinimal(string data)
        {
            _logger.Info(data);
        }

        /// <inheritdoc />
        public void LogVerbose(string data)
        {
            _logger.Info(data);
        }

        /// <inheritdoc />
        public void LogWarning(string data)
        {
            _logger.Warn(data);
        }
    }
}
