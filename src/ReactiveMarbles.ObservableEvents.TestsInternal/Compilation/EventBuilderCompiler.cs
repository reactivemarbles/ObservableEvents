// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;

using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using ICSharpCode.Decompiler.Util;

using NuGet.Frameworks;

using ReactiveMarbles.NuGet.Helpers;
using ReactiveMarbles.SourceGenerator.TestNuGetHelper.Comparers;

namespace ReactiveMarbles.SourceGenerator.TestNuGetHelper.Compilation
{
    /// <summary>
    /// This class is based on ICSharpCode.Decompiler SimpleCompiler.
    /// This has been changed to allow searching through reference types.
    /// </summary>
    /// <summary>
    /// Simple compilation implementation.
    /// </summary>
    public sealed class EventBuilderCompiler : ICompilation, IDisposable
    {
        private readonly KnownTypeCache _knownTypeCache;
        private readonly List<IModule> _assemblies = new();
        private readonly List<IModule> _referencedAssemblies = new();
        private readonly List<IModule> _neededAssemblies = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventBuilderCompiler"/> class.
        /// </summary>
        /// <param name="input">The input files from NuGet.</param>
        /// <param name="neededInput">The needed input files that aren't part of the compiler but side files.</param>
        /// <param name="framework">The NuGet framework which will be grabbed against.</param>
        public EventBuilderCompiler(InputAssembliesGroup input, InputAssembliesGroup neededInput, NuGetFramework framework)
        {
            _knownTypeCache = new KnownTypeCache(this);
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var context = new SimpleTypeResolveContext(this);

            HandleModules(input, neededInput, framework, context);

            RootNamespace = CreateRootNamespace();
        }

        /// <summary>
        /// Gets the main module we are extracting information from.
        /// This is mostly just here due to ILDecompile needing it.
        /// </summary>
        public IModule MainModule => _assemblies[0];

        /// <summary>
        /// Gets the modules we want to extract events from.
        /// </summary>
        public IReadOnlyList<IModule> Modules => _assemblies;

        /// <summary>
        /// Gets the referenced modules. These are support modules where we want additional information about types from.
        /// This will likely be either the system reference libraries or .NET Standard libraries.
        /// </summary>
        public IReadOnlyList<IModule> ReferencedModules => _referencedAssemblies;

        /// <summary>
        /// Gets the needed modules.
        /// </summary>
        public IReadOnlyList<IModule> NeededModules => _neededAssemblies;

        /// <summary>
        /// Gets the root namespace for our assemblies. We can start analyzing from here.
        /// </summary>
        public INamespace RootNamespace { get; }

        /// <summary>
        /// Gets the comparer we are going to use for comparing names of items.
        /// </summary>
        public StringComparer NameComparer => StringComparer.Ordinal;

        /// <summary>
        /// Gets the cache manager. This is mostly here for ILDecompile.
        /// </summary>
        public CacheManager CacheManager { get; } = new();

        /// <summary>
        /// Gets the root namespace if avaiable.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>The namespace instance.</returns>
        public INamespace? GetNamespaceForExternAlias(string? alias) => string.IsNullOrEmpty(alias) ? RootNamespace : null;

        /// <summary>
        /// Finds a type based on a type code.
        /// </summary>
        /// <param name="typeCode">The type code.</param>
        /// <returns>The type.</returns>
        public IType FindType(KnownTypeCode typeCode) => _knownTypeCache.FindType(typeCode) ?? new UnknownType("Unknown", "Unknown");

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var assembly in _assemblies)
            {
                assembly.PEFile?.Dispose();
            }

            foreach (var referenceAssembly in _referencedAssemblies)
            {
                referenceAssembly.PEFile?.Dispose();
            }
        }

        private static IEnumerable<IModule> GetFrameworkModules(NuGetFramework framework, ITypeResolveContext context) =>
            FileSystemHelpers.GetFilesWithinSubdirectories(framework.GetNuGetFrameworkFolders())
                .Select(file => new PEFile(file, PEStreamOptions.PrefetchMetadata))
                .Cast<IModuleReference>()
                .Select(moduleReference => moduleReference.Resolve(context))
                .Where(module => module is not null)
                .Select(module => module!);

        private static IEnumerable<IModule> GetModules(FilesGroup input, ITypeResolveContext context)
        {
            var modules = new List<IModule>();
            foreach (var file in input.GetAllFileNames()
                .Where(x => AssemblyHelpers.AssemblyFileExtensionsSet.Contains(Path.GetExtension(x))))
            {
                try
                {
                    var peFile = new PEFile(file, PEStreamOptions.PrefetchMetadata) as IModuleReference;
                    var module = peFile.Resolve(context);

                    if (module is not null)
                    {
                       modules.Add(module);
                    }
                }
                catch (PEFileNotSupportedException)
                {
                }
            }

            return modules;
        }

        private static void HandleModules(IEnumerable<IModule> modules, HashSet<string> seenAssemblies, IList<IModule> target)
        {
            foreach (var assembly in modules)
            {
                if (seenAssemblies.Contains(assembly.Name))
                {
                    continue;
                }

                seenAssemblies.Add(assembly.Name);

                target.Add(assembly);
            }
        }

        private void HandleModules(InputAssembliesGroup input, InputAssembliesGroup neededInput, NuGetFramework framework, ITypeResolveContext context)
        {
            var mainAssemblies = GetModules(input.IncludeGroup, context);
            var mainSupportAssemblies = GetModules(input.SupportGroup, context);
            var neededAssemblies = GetModules(neededInput.IncludeGroup, context);
            var neededSupportAssemblies = GetModules(neededInput.SupportGroup, context);
            var frameworkAssemblies = GetFrameworkModules(framework, context);

            var seenAssemblies = new HashSet<string>();

            HandleModules(mainAssemblies, seenAssemblies, _assemblies);
            HandleModules(mainSupportAssemblies, seenAssemblies, _referencedAssemblies);
            HandleModules(neededAssemblies, seenAssemblies, _neededAssemblies);
            HandleModules(frameworkAssemblies, seenAssemblies, _referencedAssemblies);
            HandleModules(neededSupportAssemblies, seenAssemblies, _referencedAssemblies);
        }

        private IEnumerable<IModule> GetReferenceModules(IEnumerable<IModule> mainModules, InputAssembliesGroup input, NuGetFramework framework, ITypeResolveContext context)
        {
            var assemblyReferencesSeen = new HashSet<IModule>(_neededAssemblies.Concat(_assemblies), ModuleNameComparer.Default);
            var referenceModulesToProcess = new Stack<(IModule Parent, IAssemblyReference Reference)>(mainModules.SelectMany(x => x.PEFile?.AssemblyReferences.Select(reference => (x, (IAssemblyReference)reference)) ?? Enumerable.Empty<(IModule, IAssemblyReference)>()));

            while (referenceModulesToProcess.Count > 0)
            {
                var (parent, reference) = referenceModulesToProcess.Pop();

                var moduleReference = (IModuleReference?)reference.Resolve(parent, input, framework);

                var module = moduleReference?.Resolve(context);

                if (module is null)
                {
                    continue;
                }

                if (!assemblyReferencesSeen.Add(module))
                {
                    continue;
                }

                yield return module;

                if (module.PEFile is null)
                {
                    continue;
                }

                foreach (var childAssemblyReference in module.PEFile.AssemblyReferences)
                {
                    referenceModulesToProcess.Push((module, childAssemblyReference));
                }
            }
        }

        private INamespace CreateRootNamespace()
        {
            var namespaces = new List<INamespace>();
            foreach (var module in _assemblies)
            {
                // SimpleCompilation does not support extern aliases; but derived classes might.
                // CreateRootNamespace() is virtual so that derived classes can change the global namespace.
                namespaces.Add(module.RootNamespace);
                namespaces.AddRange(_referencedAssemblies.Select(assembly => assembly.RootNamespace));
            }

            return new MergedNamespace(this, namespaces.ToArray());
        }
    }
}
