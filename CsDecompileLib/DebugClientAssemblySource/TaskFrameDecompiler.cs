using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using CsDecompileLib.GetSource;
using CsDecompileLib.IlSpy;
using CsDecompileLib.IlSpy.Ast;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

public class TaskFrameDecompiler
{
    private readonly DataTargetProvider _targetProvider;
    private readonly DecompilerFactory _decompilerFactory;
    private readonly ClrMdDllExtractor _dllExtractor;
    private readonly MethodNodeInTypeAstFinder _methodNodeInTypeAstFinder;
    
    public TaskFrameDecompiler(
        DataTargetProvider targetProvider,
        DecompilerFactory decompilerFactory,
        ClrMdDllExtractor dllExtractor,
        MethodNodeInTypeAstFinder methodNodeInTypeAstFinder)
    {
        _targetProvider = targetProvider;
        _decompilerFactory = decompilerFactory;
        _dllExtractor = dllExtractor;
        _methodNodeInTypeAstFinder = methodNodeInTypeAstFinder;
    }

    public DecompiledSourceResponse Get(ulong instructionPointer)
    {
        var response = new DecompiledSourceResponse();
        var dataTarget = _targetProvider.Get();
        var runtime = dataTarget.ClrVersions.Single().CreateRuntime();
        var method = runtime.GetMethodByInstructionPointer(instructionPointer);
        var clrMethod = method;
        var peFile = GetPEFile(runtime.DataTarget, clrMethod.Type.Module);
        var decompiler = _decompilerFactory.Get(peFile.FileName);
        //Deal with nil better
        var (syntaxTree, str) = decompiler.Run(MetadataTokenHelpers.EntityHandleOrNil(method.Type.MetadataToken));
        var methodToken = MetadataTokenHelpers.EntityHandleOrNil(clrMethod.MetadataToken);
        
        var typeDefinition = decompiler.TypeSystem.MainModule.Compilation.GetAllTypeDefinitions()
            .FirstOrDefault(t => t.MetadataToken.GetHashCode() == clrMethod.Type.MetadataToken);
        var methodSymbol = typeDefinition.Methods.Where(m => m.MetadataToken == methodToken);
        if (methodSymbol.Count() == 1)
        {
            var methodNode = _methodNodeInTypeAstFinder.Find(methodSymbol.Single(), syntaxTree);
            var location = new DecompileInfo();
            location.FillFromContainingType(typeDefinition);
            if (methodNode == null)
            {
                location.Column = 1;
                location.Line = 1;
            }
            else
            {
                location.FillFromAstNode( methodNode);
            }
            response.Location = location;
            response.SourceText = str;
                    
            return response;
        }

        return null;
    }
    
    //This is duplicated
    private PEFile GetPEFile(DataTarget dataTarget, ClrModule clrModule)
    {
        var runtime = dataTarget.ClrVersions.Single().CreateRuntime();
        PEFile peFile;
        using (var memoryStream = new MemoryStream())
        {
            _dllExtractor.TryExtract(runtime.DataTarget.DataReader, clrModule.ImageBase, memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            peFile = new PEFile(
                clrModule.Name,
                memoryStream,
                streamOptions: PEStreamOptions.PrefetchEntireImage,
                metadataOptions: new DecompilerSettings().ApplyWindowsRuntimeProjections
                    ? MetadataReaderOptions.ApplyWindowsRuntimeProjections
                    : MetadataReaderOptions.None);
        }

        return peFile;
    }
}