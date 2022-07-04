using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using TryOmnisharpExtension;

namespace IntegrationTests;

public class ResponseLocationConverter : CustomCreationConverter<ResponseLocation>
{
    private ResponseLocationType _currentObjectType;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jobj = JObject.ReadFrom(reader);
        _currentObjectType = jobj["Type"].ToObject<ResponseLocationType>();
        return base.ReadJson(jobj.CreateReader(), objectType, existingValue, serializer);
    }

    public override ResponseLocation Create(Type objectType)
    {
        switch (_currentObjectType)
        {
            case ResponseLocationType.Decompiled:
                return new DecompileInfo();
            case ResponseLocationType.SourceCode:
                return new SourceFileInfo();
            default:
                return null;
        }
    }
}