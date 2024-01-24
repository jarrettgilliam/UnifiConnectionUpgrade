namespace UnifiConnectionUpgrade.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true, ReadCommentHandling = JsonCommentHandling.Skip)]
[JsonSerializable(typeof(OptionsModel))]
internal partial class OptionsJsonSerializerContext : JsonSerializerContext
{
}