using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Util;

namespace TryOmnisharpExtension.IlSpy
{
    [Export]
    public class AnalyzerScope
    {
        private readonly AssemblyResolverFactory _assemblyResolverFactory;
        private readonly DecompileWorkspace _workspace;

        [ImportingConstructor]
        public AnalyzerScope(
            AssemblyResolverFactory assemblyResolverFactory,
            DecompileWorkspace workspace)
        {
            _workspace = workspace;
            _assemblyResolverFactory = assemblyResolverFactory;
        }

        public async Task<IEnumerable<PEFile>> GetModulesInScope(IEntity entity)
        {
            Accessibility effectiveAccessibility;
            ITypeDefinition typeScope;
            if (entity is ITypeDefinition type)
            {
                typeScope = type;
                effectiveAccessibility = DetermineEffectiveAccessibility(typeScope);
            }
            else
            {
                typeScope = entity.DeclaringTypeDefinition;
                effectiveAccessibility = DetermineEffectiveAccessibility(typeScope, entity.Accessibility);
            }
            
            var isLocal = effectiveAccessibility.LessThanOrEqual(Accessibility.Private);
            
            if (isLocal)
                return new[] { typeScope.ParentModule.PEFile };

            if (effectiveAccessibility.LessThanOrEqual(Accessibility.Internal))
                return await GetModuleAndAnyFriends(typeScope);

            var result = await GetReferencingModules(typeScope.ParentModule.PEFile, typeScope);
            return result;
        }

        private (ITypeDefinition, Accessibility) DetermineEffectiveAccessibility(IEntity entity)
        {
            Accessibility effectiveAccessibility;
            ITypeDefinition typeScope;
            if (entity is ITypeDefinition type)
            {
                typeScope = type;
                effectiveAccessibility = DetermineEffectiveAccessibility(typeScope);
            }
            else
            {
                typeScope = entity.DeclaringTypeDefinition;
                effectiveAccessibility = DetermineEffectiveAccessibility(typeScope, entity.Accessibility);
            }

            return (typeScope, effectiveAccessibility);
        }

        public async Task<IEnumerable<ITypeDefinition>> GetTypesInScope(IEntity entity)
        {
            var (typeScope, effectiveAccessibility) = DetermineEffectiveAccessibility(entity);
            var isLocal = effectiveAccessibility.LessThanOrEqual(Accessibility.Private);
            
            if (isLocal)
            {
                var result = new List<ITypeDefinition>();
                foreach (var type in TreeTraversal.PreOrder(typeScope, t => t.NestedTypes))
                {
                    result.Add(type);
                }

                return result;
            }
            else
            {
                var result = new List<ITypeDefinition>();
                var modulesInScope = await GetModulesInScope(typeScope);
                foreach (var module in modulesInScope)
                {

                    var assemblyResolver = await _assemblyResolverFactory.GetAssemblyResolver(module);
                    var typeSystem = new DecompilerTypeSystem(module, assemblyResolver);
                    foreach (var type in typeSystem.MainModule.TypeDefinitions)
                    {
                        result.Add(type);
                    }
                }

                return result;
            }
        }

        static Accessibility DetermineEffectiveAccessibility(ITypeDefinition typeScope, Accessibility memberAccessibility = Accessibility.Public)
        {
            Accessibility accessibility = memberAccessibility;
            while (typeScope.DeclaringTypeDefinition != null && !accessibility.LessThanOrEqual(Accessibility.Private))
            {
                accessibility = accessibility.Intersect(typeScope.Accessibility);
                typeScope = typeScope.DeclaringTypeDefinition;
            }
            // Once we reach a private entity, we leave the loop with typeScope set to the class that
            // contains the private entity = the scope that needs to be searched.
            // Otherwise (if we don't find a private entity) we return the top-level class.
            return accessibility;
        }

        #region Find modules
        async Task<IEnumerable<PEFile>> GetReferencingModules(PEFile self, ITypeDefinition typeScope)
        {
            var result = new List<PEFile>();
            result.Add(self);

            string reflectionTypeScopeName = typeScope.Name;
            if (typeScope.TypeParameterCount > 0)
                reflectionTypeScopeName += "`" + typeScope.TypeParameterCount;

            var toWalkFiles = new Stack<PEFile>();
            var checkedFiles = new HashSet<PEFile>();

            toWalkFiles.Push(self);
            checkedFiles.Add(self);

            do
            {
                PEFile curFile = toWalkFiles.Pop();
                var modules = await _workspace.GetAssemblies();
                foreach (var module in modules)
                {
                    bool found = false;
                    if (module == null || !module.IsAssembly)
                        continue;
                    if (checkedFiles.Contains(module))
                        continue;
                    var resolver = await _assemblyResolverFactory.GetAssemblyResolver(module);
                    foreach (var reference in module.AssemblyReferences)
                    {
                        if (await resolver.ResolveAsync(reference) == curFile)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found && checkedFiles.Add(module))
                    {
                        if (ModuleReferencesScopeType(module.Metadata, reflectionTypeScopeName, typeScope.Namespace))
                            result.Add(module);
                        if (ModuleForwardsScopeType(module.Metadata, reflectionTypeScopeName, typeScope.Namespace))
                            toWalkFiles.Push(module);
                    }
                }
            } while (toWalkFiles.Count > 0);

            return result;
        }

        async Task<IEnumerable<PEFile>> GetModuleAndAnyFriends(ITypeDefinition typeScope)
        {
            var result = new List<PEFile>();
            
            var self = typeScope.ParentModule.PEFile;
            result.Add(self);

            var typeProvider = MetadataExtensions.MinimalAttributeTypeProvider;
            var attributes = self.Metadata.CustomAttributes.Select(h => self.Metadata.GetCustomAttribute(h))
                .Where(ca => ca.GetAttributeType(self.Metadata).GetFullTypeName(self.Metadata).ToString() == "System.Runtime.CompilerServices.InternalsVisibleToAttribute");
            var friendAssemblies = new HashSet<string>();
            foreach (var attribute in attributes)
            {
                string assemblyName = attribute.DecodeValue(typeProvider).FixedArguments[0].Value as string;
                assemblyName = assemblyName.Split(',')[0]; // strip off any public key info
                friendAssemblies.Add(assemblyName);
            }

            if (friendAssemblies.Count > 0)
            {
                IEnumerable<PEFile> modules = await _workspace.GetAssemblies();

                foreach (var module in modules)
                {
                    if (friendAssemblies.Contains(module.Name))
                    {
                        if (ModuleReferencesScopeType(module.Metadata, typeScope.Name, typeScope.Namespace))
                            result.Add(module);
                    }
                }
            }

            return result;
        }

        bool ModuleReferencesScopeType(MetadataReader metadata, string typeScopeName, string typeScopeNamespace)
        {
            bool hasRef = false;
            foreach (var h in metadata.TypeReferences)
            {
                var typeRef = metadata.GetTypeReference(h);
                if (metadata.StringComparer.Equals(typeRef.Name, typeScopeName) && metadata.StringComparer.Equals(typeRef.Namespace, typeScopeNamespace))
                {
                    hasRef = true;
                    break;
                }
            }
            return hasRef;
        }

        bool ModuleForwardsScopeType(MetadataReader metadata, string typeScopeName, string typeScopeNamespace)
        {
            bool hasForward = false;
            foreach (var h in metadata.ExportedTypes)
            {
                var exportedType = metadata.GetExportedType(h);
                if (exportedType.IsForwarder && metadata.StringComparer.Equals(exportedType.Name, typeScopeName) && metadata.StringComparer.Equals(exportedType.Namespace, typeScopeNamespace))
                {
                    hasForward = true;
                    break;
                }
            }
            return hasForward;
        }
        #endregion
    }
}
