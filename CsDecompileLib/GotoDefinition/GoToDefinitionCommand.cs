﻿using System.Threading.Tasks;
using ICSharpCode.Decompiler.TypeSystem;

namespace CsDecompileLib.GotoDefinition;

public class GoToDefinitionCommand<T> : INavigationCommand<LocationsResponse> where T : IEntity
{
    private readonly T _typeDefinition;
    private readonly IlSpyDefinitionFinderBase<T> _ilSpyTypeFinder;
    private readonly string _assemblyFilePath;

    public GoToDefinitionCommand(
        T typeDefinition,
        IlSpyDefinitionFinderBase<T> ilSpyTypeFinder,
        string assemblyFilePath)
    {
        _typeDefinition = typeDefinition;
        _ilSpyTypeFinder = ilSpyTypeFinder;
        _assemblyFilePath = assemblyFilePath;
    }
        
    public Task<ResponsePacket<LocationsResponse>> Execute()
    {
        var ilSpyMetadataSource = _ilSpyTypeFinder.Find(
            _typeDefinition);
            
        ilSpyMetadataSource.AssemblyFilePath = _assemblyFilePath;
        ilSpyMetadataSource.ParentAssemblyFilePath = _typeDefinition.ParentModule.PEFile.FileName;
            
        var result = new LocationsResponse
        {
            Locations = {  ilSpyMetadataSource },
        };

        var response = new ResponsePacket<LocationsResponse>
        {
            Body = result,
            Success = true
        };

        return Task.FromResult(response);
    }
}