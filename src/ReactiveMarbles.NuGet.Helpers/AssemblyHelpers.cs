// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ReactiveMarbles.NuGet.Helpers
{
    /// <summary>
    /// Helps get details about assemblies.
    /// </summary>
    public static class AssemblyHelpers
    {
        private static readonly string[] AssemblyFileExtensions =
        {
            ".winmd", ".dll", ".exe"
        };

        /// <summary>
        /// Gets the assembly file extensions set.
        /// </summary>
        public static ISet<string> AssemblyFileExtensionsSet { get; } = new HashSet<string>(AssemblyFileExtensions, StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Finds the union metadata file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="version">The version.</param>
        /// <returns>The file if found.</returns>
        public static string? FindUnionMetadataFile(string name, Version version)
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "UnionMetadata");

            if (!Directory.Exists(basePath))
            {
                return null;
            }

            basePath = Path.Combine(basePath, FindClosestVersionDirectory(basePath, version));

            if (!Directory.Exists(basePath))
            {
                return null;
            }

            var file = Path.Combine(basePath, name + ".winmd");

            return !File.Exists(file) ? null : file;
        }

        /// <summary>
        /// Finds the windows metadata file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="version">The version.</param>
        /// <returns>The file if found.</returns>
        public static string? FindWindowsMetadataFile(string name, Version? version)
        {
            // This is only supported on windows at the moment.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits", "10", "References");

            if (!Directory.Exists(basePath))
            {
                return FindWindowsMetadataInSystemDirectory(name);
            }

            if (version is null)
            {
                return null;
            }

            basePath = Path.Combine(basePath, FindClosestVersionDirectory(basePath, version));

            if (!Directory.Exists(basePath))
            {
                return FindWindowsMetadataInSystemDirectory(name);
            }

            var file = Path.Combine(basePath, name + ".winmd");

            return !File.Exists(file) ? FindWindowsMetadataInSystemDirectory(name) : file;
        }

        private static string? FindWindowsMetadataInSystemDirectory(string name)
        {
            var file = Path.Combine(Environment.SystemDirectory, "WinMetadata", name + ".winmd");
            return File.Exists(file) ? file : null;
        }

        private static string? FindClosestVersionDirectory(string basePath, Version version)
        {
            string? path = null;
            foreach (var folder in new DirectoryInfo(basePath)
                .EnumerateDirectories()
                .Select(d => ConvertToVersion(d.Name))
                .Where(v => v.Version != null)
                .OrderByDescending(v => v.Version))
            {
                if (path == null || folder.Version >= version)
                {
                    path = folder.Name;
                }
            }

            return path ?? version.ToString();
        }

        [SuppressMessage("Design", "CA1031: Modify to catch a more specific exception type, or rethrow the exception.", Justification = "Deliberate usage.")]
        private static (Version? Version, string? Name) ConvertToVersion(string name)
        {
            string RemoveTrailingVersionInfo()
            {
                var shortName = name;
                var dashIndex = shortName.IndexOf('-');
                if (dashIndex > 0)
                {
                    shortName = shortName.Remove(dashIndex);
                }

                return shortName;
            }

            try
            {
                return (new Version(RemoveTrailingVersionInfo()), name);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }
    }
}
