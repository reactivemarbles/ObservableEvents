﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReactiveMarbles.NuGet.Helpers
{
    internal static class FileSystemHelpers
    {
        public static IEnumerable<string> GetSubdirectoriesWithMatch(IEnumerable<string> directories, ISet<string> extensions)
        {
            var searchStack = new Stack<DirectoryInfo>(directories.Select(x => new DirectoryInfo(x)));

            while (searchStack.Count != 0)
            {
                var directoryInfo = searchStack.Pop();

                if (directoryInfo.EnumerateFiles().Any(file => extensions.Contains(file.Extension)))
                {
                    yield return directoryInfo.FullName;
                }

                foreach (var directory in directoryInfo.EnumerateDirectories())
                {
                    searchStack.Push(directory);
                }
            }
        }

        public static IEnumerable<string> GetFilesWithinSubdirectories(IEnumerable<string> directories)
        {
            return GetFilesWithinSubdirectories(directories, AssemblyHelpers.AssemblyFileExtensionsSet);
        }

        public static IEnumerable<string> GetFilesWithinSubdirectories(IEnumerable<string> directories, ISet<string> extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            var searchStack = new Stack<DirectoryInfo>(directories.Select(x => new DirectoryInfo(x)));

            while (searchStack.Count != 0)
            {
                var directoryInfo = searchStack.Pop();

                foreach (var file in directoryInfo.EnumerateFiles().Where(file => extensions.Contains(file.Extension)))
                {
                    yield return file.FullName;
                }

                foreach (var directory in directoryInfo.EnumerateDirectories())
                {
                    searchStack.Push(directory);
                }
            }
        }
    }
}
