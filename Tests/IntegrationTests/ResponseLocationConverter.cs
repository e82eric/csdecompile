using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using CsDecompileLib;

namespace IntegrationTests;

public class ResponseLocationConverter : CustomCreationConverter<ResponseLocation>
{
    private LocationType _currentObjectType;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jobj = JObject.ReadFrom(reader);
        _currentObjectType = jobj["Type"].ToObject<LocationType>();
        return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
    }

    public override ResponseLocation Create(Type objectType)
    {
        switch (_currentObjectType)
        {
            case LocationType.Decompiled:
                return new DecompileInfo();
            case LocationType.SourceCode:
                return new SourceFileInfo();
            default:
                return null;
        }
    }
}