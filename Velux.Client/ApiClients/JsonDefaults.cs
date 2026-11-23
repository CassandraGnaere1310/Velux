using System.Text.Json;
using System.Text.Json.Serialization;

namespace Velux.Client.ApiClients;

internal static class JsonDefaults
{
    // IgnoreCycles: e.g. MaterialAlternative.Supplier.Alternatives points back to the same alternative
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
}
