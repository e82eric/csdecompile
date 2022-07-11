﻿using System.Reflection.Metadata;
using TryOmnisharpExtension.IlSpy;

namespace TryOmnisharpExtension.FindUsages;

public class FieldUsedByMetadataScanner : MemberUsedByMetadataScanner
{
    public FieldUsedByMetadataScanner(AnalyzerScope analyzerScope):base(analyzerScope)
    {
    }

    protected override bool IsSupportedOpCode(ILOpCode opCode)
    {
        switch (opCode)
        {
            case ILOpCode.Call:
            case ILOpCode.Ldfld:
            case ILOpCode.Ldflda:
            case ILOpCode.Ldc_i4:
            case ILOpCode.Ldc_i4_0:
            case ILOpCode.Stfld:
            case ILOpCode.Ldsfld:
            case ILOpCode.Ldsflda:
            case ILOpCode.Stsfld:
            case ILOpCode.Callvirt:
            case ILOpCode.Ldtoken:
            case ILOpCode.Ldftn:
            case ILOpCode.Ldvirtftn:
            case ILOpCode.Newobj:
                return true;
            default:
                return false;
        }
    }
}