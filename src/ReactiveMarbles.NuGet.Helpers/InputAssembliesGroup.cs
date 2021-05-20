// Copyright (c) 2019-2021 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.NuGet.Helpers
{
    /// <summary>
    /// A series of folders and files for processing.
    /// </summary>
    public class InputAssembliesGroup
    {
        /// <summary>
        /// Gets a folder group which should contain the inclusions.
        /// </summary>
        public FilesGroup IncludeGroup { get; internal set; } = new FilesGroup();

        /// <summary>
        /// Gets a folder group with folders for including for support files only.
        /// </summary>
        public FilesGroup SupportGroup { get; internal set; } = new FilesGroup();
    }
}
