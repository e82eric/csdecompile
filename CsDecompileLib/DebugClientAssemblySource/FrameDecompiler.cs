using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using CsDecompileLib.GetSource;
using CsDecompileLib.IlSpy;
using CsDecompileLib.IlSpy.Ast;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

public class FrameDecompiler
{
    private readonly DataTargetProvider _targetProvider;
    private readonly DecompilerFactory _decompilerFactory;
    private readonly ClrMdDllExtractor _dllExtractor;
    private readonly MethodNodeInTypeAstFinder _methodNodeInTypeAstFinder;

    public FrameDecompiler(
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

    public DecompiledSourceResponse Get(ulong stackPointer)
    {
        var response = new DecompiledSourceResponse();
        var dataTarget = _targetProvider.Get();
        var runtime = dataTarget.ClrVersions.Single().CreateRuntime();
        ClrStackFrame? currentFrame = null;
        foreach (var clrThread in runtime.Threads)
        {
            foreach (var frame in clrThread.EnumerateStackTrace())
            {
                if (frame.StackPointer == stackPointer)
                {
                    currentFrame = frame;
                    break;
                }
            }

            if (currentFrame != null)
            {
                break;
            }
        }
        
        if (currentFrame?.Method != null)
        {
            var clrMethod = currentFrame.Method;
            var peFile = GetPEFile(runtime.DataTarget, clrMethod.Type.Module);
            var decompiler = _decompilerFactory.Get(peFile.FileName);
            
            var typeDefinition = decompiler.TypeSystem.MainModule.Compilation.GetAllTypeDefinitions()
                .FirstOrDefault(t => t.MetadataToken.GetHashCode() == clrMethod.Type.MetadataToken);

            if (typeDefinition != null)
            {
                var (syntaxTree, str) = decompiler.Run(typeDefinition);
                var methodToken = MetadataTokenHelpers.EntityHandleOrNil(clrMethod.MetadataToken);
                var methodSymbol = typeDefinition.Methods.Where(m => m.MetadataToken == methodToken);
                if (methodSymbol.Count() == 1)
                {
                    var methodNode = _methodNodeInTypeAstFinder.Find(methodSymbol.Single(), syntaxTree);
                    var location = new DecompileInfo();
                    location.FillFromContainingType(typeDefinition);
                    location.FillFromAstNode( methodNode);
                    response.Location = location;
                    response.SourceText = str;
                    var sequencePoints = decompiler.CreateSequencePoints(syntaxTree);
                    var offset = GetILOffsetForNativeOffset(clrMethod, currentFrame.InstructionPointer);
                    if (sequencePoints != null && sequencePoints.Count > 0)
                    {
                        var sps = sequencePoints.Where(s => s.Key.Method?.MetadataToken == methodToken);
                        if (sps.Any())
                        {
                            var sp = FindSeqPointByOffset(offset, sps.First());
                            if (sp != null)
                            {
                                location.Line = sp.StartLine;
                            }
                        }
                    }
                    
                    return response;
                }
            }
        }

        return null;
    }
    
    private static ICSharpCode.Decompiler.DebugInfo.SequencePoint? FindSeqPointByOffset(int ilOffset, KeyValuePair<ILFunction, List<ICSharpCode.Decompiler.DebugInfo.SequencePoint>> first)
    {
        ICSharpCode.Decompiler.DebugInfo.SequencePoint? result = null;
        foreach (var point in first.Value)
        {
            if (ilOffset >= point.Offset && ilOffset <= point.EndOffset)
            {
                result = point;
            }
        }

        return result;
    }
    
    private static int GetILOffsetForNativeOffset(ClrMethod method, ulong ip)
    {
        ImmutableArray<ILToNativeMap> ilmap = method.ILOffsetMap;
        if (ilmap.IsDefaultOrEmpty)
        {
            return -1;
        }

        (ulong Distance, int Offset) closest = (ulong.MaxValue, -1);
        foreach (ILToNativeMap entry in ilmap)
        {
            ulong distance = GetDistance(entry, ip);
            if (distance == 0)
            {
                return entry.ILOffset;
            }

            if (distance < closest.Distance)
            {
                closest = (distance, entry.ILOffset);
            }
        }

        return closest.Offset;
    }   
    private static ulong GetDistance(ILToNativeMap entry, ulong nativeOffset)
    {
        ulong distance = 0;
        if (nativeOffset < entry.StartAddress)
        {
            distance = entry.StartAddress - nativeOffset;
        }
        else if (nativeOffset > entry.EndAddress)
        {
            distance = nativeOffset - entry.EndAddress;
        }

        return distance;
    }

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

