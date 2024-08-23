using System;
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
    private readonly MemberNodeInTypeAstFinder _methodNodeInTypeAstFinder;
    
    public TaskFrameDecompiler(
        DataTargetProvider targetProvider,
        DecompilerFactory decompilerFactory,
        ClrMdDllExtractor dllExtractor,
        MemberNodeInTypeAstFinder methodNodeInTypeAstFinder)
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
        if (method != null)
        {
            var peFile = GetPEFile(runtime.DataTarget, method.Type.Module);
            var decompiler = _decompilerFactory.Get(peFile.FileName);
            var stateMachineTypeDefinition = decompiler.TypeSystem.MainModule.Compilation
                .GetAllTypeDefinitions()
                .FirstOrDefault(t => t.MetadataToken.GetHashCode() == method.Type.MetadataToken);
            
            if (stateMachineTypeDefinition != null)
            {
                var realTypeDefinition = stateMachineTypeDefinition.DeclaringTypeDefinition;

                IMember realMember = null;
                if (realTypeDefinition != null)
                {
                    foreach (var member in realTypeDefinition.Members)
                    {
                        var stateMachineAttribute = member.GetAttribute(KnownAttribute.AsyncStateMachine);
                        if (stateMachineAttribute != null)
                        {
                            var typeArgument = stateMachineAttribute.FixedArguments
                                .FirstOrDefault(a => a.Type.IsKnownType(KnownTypeCode.Type));
                            
                            if (typeArgument.Value is IType stateMachineType &&
                                stateMachineType.FullName == stateMachineTypeDefinition.FullName)
                            {
                                realMember = member;
                                break;
                            }
                        }
                    }

                    if (realMember != null)
                    {
                        var (syntaxTree, str) = decompiler.Run(realTypeDefinition);
                        var memberNode = _methodNodeInTypeAstFinder.Find(realMember, syntaxTree);
                        var location = new DecompileInfo();
                        location.FillFromContainingType(realTypeDefinition);
                        if (memberNode == null)
                        {
                            location.Column = 1;
                            location.Line = 1;
                        }
                        else
                        {
                            location.FillFromAstNode(memberNode);
                        }

                        response.Location = location;
                        response.SourceText = str;

                        return response;
                    }
                }
            }
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