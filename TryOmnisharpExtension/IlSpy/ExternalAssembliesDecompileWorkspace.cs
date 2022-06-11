﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Metadata;

namespace TryOmnisharpExtension.IlSpy;

public class ExternalAssembliesDecompileWorkspace : IDecompileWorkspace
{
    public ExternalAssembliesDecompileWorkspace(IDecompilerTypeSystemFactory typeSystemFactory)
    {
    }

    public PEFile[] GetAssemblies()
    {
        return new PEFile[] { };
    }

    public void LoadDlls()
    {
        
    }
}