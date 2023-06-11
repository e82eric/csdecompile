using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using CsDecompileLib.FindImplementations;
using CsDecompileLib.IlSpy;
using CsDecompileLib.Roslyn;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using AssemblyReference = ICSharpCode.Decompiler.Metadata.AssemblyReference;

namespace CsDecompileLib.Nuget;

public class SearchNugetFromLocationHandler : HandlerBase<DecompiledLocationRequest, SearchNugetResponse>
{
    private readonly NodeFinder _nodeFinder;
    private readonly IDecompileWorkspace _workspace;
    private readonly IlSpySymbolFinder _symbolFinder;

    public SearchNugetFromLocationHandler(NodeFinder nodeFinder, IlSpySymbolFinder spySymbolFinder, IDecompileWorkspace workspace)
    {
        _nodeFinder = nodeFinder;
        _workspace = workspace;
        _symbolFinder = spySymbolFinder;
    }

    public override async Task<ResponsePacket<SearchNugetResponse>> Handle(DecompiledLocationRequest request)
    {
        var node = _nodeFinder.GetNode(request);
        var symbol = _symbolFinder.FindSymbolFromNode(node);
        var peFile = _workspace.GetAssembly(request.AssemblyFilePath);

        AssemblyReference found = null;
        switch (symbol.SymbolKind)
        {
            case SymbolKind.Method:
                var memberSymbol = (IMember)symbol;
                foreach (var memberReference in peFile.Metadata.MemberReferences)
                {
                    var member = peFile.Metadata.GetMemberReference(memberReference);
                    if (peFile.Metadata.StringComparer.Equals(member.Name, symbol.Name))
                    {
                        var typeFullName = member.Parent.GetFullTypeName(peFile.Metadata);
                        if (memberSymbol.FullName.StartsWith(typeFullName.ReflectionName))
                        {
                            if (member.Parent.Kind == HandleKind.TypeReference)
                            {
                                var typeReference =
                                    peFile.Metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
                                found = peFile.AssemblyReferences.FirstOrDefault(ar => ar.Handle == typeReference.ResolutionScope);
                                break;
                            }
                            // var type = member.Parent.GetDeclaringType(peFile.Metadata);
                            // var typeDef = peFile.Metadata.GetTypeDefinition(type);
                            // found = null;
                        }
                    }
                }
                break;
            case SymbolKind.TypeDefinition:
                var typeSymbol = (ITypeDefinition)symbol;
                foreach (var typeReference in peFile.Metadata.TypeReferences)
                {
                    var typeDef = peFile.Metadata.GetTypeReference(typeReference);
                    var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
                    var nspace = peFile.Metadata.GetString(typeDef.Namespace);
                    if (foundTypeName == symbol.Name && nspace == typeSymbol.Namespace)
                    {
                        found = peFile.AssemblyReferences.FirstOrDefault(
                            r => r.Handle == typeDef.ResolutionScope);
                        break;
                    }
                }
                break;
        }
        // if (node.Item2 is MemberResolveResult memberResolveResult)
        // {
        //     var me = peFile.Metadata.MemberReferences.Where(m =>
        //     {
        //         var member = peFile.Metadata.GetMemberReference(m);
        //         var name = peFile.Metadata.GetString(member.Name);
        //         var result = name == memberResolveResult.Member.Name;
        //         return result;
        //     });
        //     var me2 = peFile.Metadata.GetMemberReference(me.First());
        //     var typeDef = peFile.Metadata.GetTypeReference((TypeReferenceHandle)me2.Parent);
        //     found = peFile.AssemblyReferences.FirstOrDefault(r => r.Handle == typeDef.ResolutionScope);
        // }
        // else
        // {
        //     foreach (var typeReference in peFile.Metadata.TypeReferences)
        //     {
        //         var typeDef = peFile.Metadata.GetTypeReference(typeReference);
        //         var foundTypeName = peFile.Metadata.GetString(typeDef.Name);
        //         var nspace = peFile.Metadata.GetString(typeDef.Namespace);
        //         if (foundTypeName == node.Item2.Type.Name && nspace == node.Item2.Type.Namespace)
        //         {
        //             found = peFile.AssemblyReferences.FirstOrDefault(
        //                 r => r.Handle == typeDef.ResolutionScope);
        //             break;
        //         }
        //     }
        // }

        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>();
        SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

        var itemsPerPage = 100;
        var offset = 0;
        var response = new SearchNugetResponse();
        bool anyResults = true;
        while (anyResults)
        {
            IEnumerable<IPackageSearchMetadata> results = await resource.SearchAsync(
                found.Name,
                searchFilter,
                skip: offset,
                take: itemsPerPage,
                logger,
                cancellationToken);

            foreach (IPackageSearchMetadata result in results)
            {
                var package = new Package
                {
                    Identity = result.Identity.Id,
                };
                response.Packages.Add(package);
            }

            offset += itemsPerPage;
            anyResults = results.Any();
        }

        return new ResponsePacket<SearchNugetResponse>
        {
            Body = response,
            Success = true
        };
    }
}