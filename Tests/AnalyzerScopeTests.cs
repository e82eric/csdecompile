using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;
using NUnit.Framework;
using TryOmnisharpExtension;
using TryOmnisharpExtension.FindImplementations;
using TryOmnisharpExtension.IlSpy;

namespace Tests
{
    [TestFixture]
    public class AnalyzerScopeTests
    {
        [Test]
        public async Task SomeTest()
        {
            var paths = new[]
            {
                "C:\\src\\TryOmnisharpExtension\\LibraryThatJustReferencesFramework\\bin\\Debug\\LibraryThatJustReferencesFramework.dll"
            };
            
            var peFileCahce = new PeFileCache();
            var resolverFactory = new AssemblyResolverFactory(peFileCahce);
            var typeSystemFactory = new IlSpyTypeSystemFactory(resolverFactory);
            DecompileWorkspace assemblyList = new DecompileWorkspace(paths, typeSystemFactory);

            var typeSystem = await typeSystemFactory.GetTypeSystem(paths.First());
            var type = typeSystem.FindType(new FullTypeName("LibraryThatJustReferencesFramework.Class1"));
            
            var analyzerScope = new AnalyzerScope(resolverFactory, assemblyList);

            var modulesInScope = await analyzerScope.GetModulesInScope((ITypeDefinition)type);

            var typesInScope = await analyzerScope.GetTypesInScope((IEntity)type);
            Assert.AreEqual(1, 1);
        }
       
        [Test]
        public async Task SomeTest2()
        {
            string n = null;
            var paths = new[]
            {
                "C:\\src\\TryOmnisharpExtension\\LibraryThatJustReferencesFramework\\bin\\Debug\\LibraryThatJustReferencesFramework.dll"
            };
            
            var peFileCahce = new PeFileCache();
            var resolverFactory = new AssemblyResolverFactory(peFileCahce);
            var typeSystemFactory = new IlSpyTypeSystemFactory(resolverFactory);
            DecompileWorkspace assemblyList = new DecompileWorkspace(paths, typeSystemFactory);

            var typeSystem = await typeSystemFactory.GetTypeSystem(paths.First());
            var type = typeSystem.FindType(new FullTypeName("System.String"));
            
            var analyzerScope = new AnalyzerScope(resolverFactory, assemblyList);

            var modulesInScope = await analyzerScope.GetModulesInScope((ITypeDefinition)type);

            var typesInScope = await analyzerScope.GetTypesInScope((IEntity)type);
            Assert.AreEqual(1, 1);
        }
        
        [Test]
        public async Task SomeTest3()
        {
            var paths = new[]
            {
                "C:\\src\\TryOmnisharpExtension\\LibraryThatReferencesLibrary\\bin\\Debug\\LibraryThatReferencesLibrary.dll"
            };
            
            var peFileCahce = new PeFileCache();
            var resolverFactory = new AssemblyResolverFactory(peFileCahce);
            var typeSystemFactory = new IlSpyTypeSystemFactory(resolverFactory);
            DecompileWorkspace assemblyList = new DecompileWorkspace(paths, typeSystemFactory);

            var typeSystem = await typeSystemFactory.GetTypeSystem("C:\\src\\TryOmnisharpExtension\\LibraryThatReferencesLibrary\\bin\\Debug\\LibraryThatReferencesLibrary.dll");
            var type = typeSystem.FindType(new FullTypeName("LibraryThatJustReferencesFramework.Class1"));
            
            var analyzerScope = new AnalyzerScope(resolverFactory, assemblyList);

            var modulesInScope = await analyzerScope.GetModulesInScope((ITypeDefinition)type);

            var typesInScope = await analyzerScope.GetTypesInScope((IEntity)type);
            Assert.AreEqual(1, 1);
        }
        
        [Test]
        public async Task SomeTest4()
        {
            var paths = new[]
            {
                "C:\\src\\TryOmnisharpExtension\\LibraryThatReferencesLibrary\\bin\\Debug\\LibraryThatReferencesLibrary.dll",
                "C:\\src\\TryOmnisharpExtension\\AnotherLibraryThatReferencesLibrary\\bin\\Debug\\AnotherLibraryThatReferencesLibrary.dll"
            };
            
            var peFileCahce = new PeFileCache();
            var resolverFactory = new AssemblyResolverFactory(peFileCahce);
            var typeSystemFactory = new IlSpyTypeSystemFactory(resolverFactory);
            DecompileWorkspace assemblyList = new DecompileWorkspace(paths, typeSystemFactory);

            var typeSystem = await typeSystemFactory.GetTypeSystem("C:\\src\\TryOmnisharpExtension\\LibraryThatReferencesLibrary\\bin\\Debug\\LibraryThatReferencesLibrary.dll");
            var type = typeSystem.FindType(new FullTypeName("LibraryThatJustReferencesFramework.Class1"));
            
            var analyzerScope = new AnalyzerScope(resolverFactory, assemblyList);
            var typeUsedByAnalyzer = new TypesThatUseTypeAsBaseTypeMetadataScanner(analyzerScope);
            var finder = new IlSpyBaseTypeUsageFinder(typeSystemFactory, typeUsedByAnalyzer, new TypeUsedInTypeFinder(new DecompilerFactory(typeSystemFactory)));
            var commandFactory = new IlSpyCommandFactory<>(new DecompilerFactory(typeSystemFactory),
                new FindImplementationsCommandFactory(finder));

            var someResult = await finder.Run(
                "LibraryThatJustReferencesFramework.Class1",
                "C:\\src\\TryOmnisharpExtension\\LibraryThatReferencesLibrary\\bin\\Debug\\LibraryThatReferencesLibrary.dll");

            var modulesInScope = await analyzerScope.GetModulesInScope((ITypeDefinition)type);

            var typesInScope = await analyzerScope.GetTypesInScope((IEntity)type);

            var implementations = await commandFactory.Find(new DecompileFindImplementationsFromDecompileRequest()
                {
                    AssemblyFilePath = "C:\\src\\TryOmnisharpExtension\\LibraryThatReferencesLibrary\\bin\\Debug\\LibraryThatReferencesLibrary.dll",
                    ContainingTypeFullName = "LibraryThatJustReferencesFramework.Class1",
                    Line = 7,
                    Column = 17
                },
                "C:\\src\\TryOmnisharpExtension\\LibraryThatReferencesLibrary\\bin\\Debug\\LibraryThatReferencesLibrary.dll");
            var result = await implementations.Execute();
            Assert.AreEqual(1, 1);
        }
    }
}