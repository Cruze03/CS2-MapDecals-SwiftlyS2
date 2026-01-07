
namespace MapDecals;

public class ConfigDecals
{
    public string UniqId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Material { get; set; } = "";
    public string ShowPermission { get; set; } = "";
}

public sealed class CommandConfig
{
    public string Command { get; set; } = "";
    public List<string> Aliases { get; set; } = [];
    public string Permission { get; set; } = "";
}

public sealed class PluginConfig
{
    public List<ConfigDecals> Props { get; set; } = new()
    {
        new() { UniqId = "exampleTexture", Name = "Example Name", Material = "materials/Example/exampleTexture.vmat", ShowPermission = ""},
        new() { UniqId = "exampleTexture2", Name = "Example Name 2", Material = "materials/Example/exampleTexture2.vmat", ShowPermission = ""},
    };

    public CommandConfig AdToggleCommands = new()
    {
        Command = "decal",
        Aliases = [
            "decals",
        ],
        Permission = "cc-mapdecals.vip"
    };

    public CommandConfig PlaceDecalCommands = new()
    {
        Command = "mapdecal",
        Aliases = [
            "paintmapdecal",
            "placedecals",
            "placedecal",
        ],
        Permission = "cc-mapdecals.admin"
    };
}