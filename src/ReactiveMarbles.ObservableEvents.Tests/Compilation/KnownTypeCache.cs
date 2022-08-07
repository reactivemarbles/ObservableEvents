// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.TypeSystem.Implementation;
using ICSharpCode.Decompiler.Util;

namespace ReactiveMarbles.ObservableEvents.Tests.Compilation
{
    /// <summary>
    /// Cache for KnownTypeReferences.
    /// Based on https://github.com/icsharpcode/ILSpy/blob/master/ICSharpCode.Decompiler/TypeSystem/Implementation/KnownTypeCache.cs
    /// and the ILSpy project.
    /// </summary>
    internal sealed class KnownTypeCache
    {
        private readonly ICompilation _compilation;
        private readonly IType[] _knownTypes = new IType[(int)KnownTypeCode.MemoryOfT + 1];

        public KnownTypeCache(ICompilation compilation)
        {
            _compilation = compilation;
        }

        public IType? FindType(KnownTypeCode typeCode)
        {
            var type = LazyInit.VolatileRead(ref _knownTypes[(int)typeCode]);
            IType? target = _knownTypes[(int)typeCode];

            var value = type ?? LazyInit.GetOrSet(ref target, SearchType(typeCode));

            if (target != null)
            {
                _knownTypes[(int)typeCode] = target;
            }

            return value;
        }

        private IType SearchType(KnownTypeCode typeCode)
        {
            var typeRef = KnownTypeReference.Get(typeCode);
            if (typeRef == null)
            {
                return SpecialType.UnknownType;
            }

            var typeName = new TopLevelTypeName(typeRef.Namespace, typeRef.Name, typeRef.TypeParameterCount);
            foreach (var asm in _compilation.Modules)
            {
                var typeDef = asm.GetTypeDefinition(typeName);
                if (typeDef != null)
                {
                    return typeDef;
                }
            }

            return new UnknownType(typeName);
        }
    }
}
