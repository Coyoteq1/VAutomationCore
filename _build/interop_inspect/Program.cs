using System.Reflection;
using System.Runtime.Loader;

var interopDir = @"D:\DedicatedServerLauncher\VRisingServer\BepInEx\interop";
var extra = new[]
{
    @"C:\Users\coyot.RWE\.nuget\packages\il2cppinterop.runtime\1.4.6-ci.426\lib\net6.0\Il2CppInterop.Runtime.dll"
};

var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
foreach (var dll in Directory.EnumerateFiles(interopDir, "*.dll"))
{
    map[Path.GetFileNameWithoutExtension(dll)] = dll;
}

foreach (var dll in extra)
{
    if (File.Exists(dll))
    {
        map[Path.GetFileNameWithoutExtension(dll)] = dll;
    }
}

var alc = new PluginLoadContext(map);
var allAssemblies = new List<Assembly>();
foreach (var dll in Directory.EnumerateFiles(interopDir, "ProjectM*.dll"))
{
    try
    {
        allAssemblies.Add(alc.LoadFromAssemblyPath(dll));
    }
    catch
    {
        // Skip assemblies that fail to load for scanning.
    }
}

var allTypes = new List<Type>();
foreach (var assembly in allAssemblies)
{
    try
    {
        allTypes.AddRange(assembly.GetTypes());
    }
    catch (ReflectionTypeLoadException ex)
    {
        allTypes.AddRange(ex.Types.Where(t => t != null)!);
    }
    catch
    {
    }
}

var types = allTypes
    .Where(t => t != null)
    .Distinct()
    .ToArray();

Console.WriteLine($"Assemblies loaded: {allAssemblies.Count}");
Console.WriteLine($"ProjectM* types loaded: {types.Length}");
Console.WriteLine();

var exactTargets = new[]
{
    "ProjectM.GiveItemCommandUtility",
    "ProjectM.InventoryBuffer",
    "ProjectM.InventoryComponent",
    "ProjectM.InventoryItem",
    "ProjectM.InventoryUtilities",
    "ProjectM.InventoryUtilitiesServer",
    "ProjectM.InventoryUtilities_Events",
    "ProjectM.User",
    "ProjectM.Network.User",
    "ProjectM.PlayerCharacter",
    "ProjectM.CharStat",
    "ProjectM.CharacterName",
    "ProjectM.AddItemSettings",
    "ProjectM.GetInventoryResponse",
    "ProjectM.RestrictedInventory",
    "ProjectM.InventoryOwner",
    "ProjectM.InventoryConnection",
    "ProjectM.ItemData",
    "ProjectM.ItemCategory",
    "ProjectM.ItemType",
    "ProjectM.AddItemResponse",
    "ProjectM.AddItemResult",
    "ProjectM.MoveItemResponse",
    "ProjectM.MoveItemResult",
    "ProjectM.Network.MoveItemBetweenInventoriesEvent",
    "ProjectM.Network.EquipItemFromInventoryEvent",
    "ProjectM.Equipment",
    "ProjectM.EquippableData",
    "ProjectM.DropItemSystem",
    "ProjectM.DropInventorySystem",
    "ProjectM.InventoryRouteUtility_Server",
    "ProjectM.InventoryMoveItemEvent",
    "ProjectM.EquipItemFromInventoryEvent",
    "ProjectM.CreateItemDropEvent",
    "ProjectM.ItemData"
};

foreach (var name in exactTargets)
{
    var t = types.FirstOrDefault(x => string.Equals(x?.FullName, name, StringComparison.Ordinal));
    Console.WriteLine($"=== {name} ===");
    if (t == null)
    {
        Console.WriteLine("<not found>");
        Console.WriteLine();
        continue;
    }

    DumpFields(t);
    DumpMethods(t);
    Console.WriteLine();
}

var prefabCollectionType = types.FirstOrDefault(t => string.Equals(t?.FullName, "ProjectM.PrefabCollectionSystem", StringComparison.Ordinal));
if (prefabCollectionType != null)
{
    Console.WriteLine("=== ProjectM.PrefabCollectionSystem Fields ===");
    foreach (var field in prefabCollectionType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
    {
        try
        {
            Console.WriteLine($"  {field.FieldType.Name} {field.Name}");
        }
        catch
        {
            Console.WriteLine($"  <field-read-failed> {field.Name}");
        }
    }
}

void DumpFields(Type t)
{
    try
    {
        var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        Console.WriteLine($"fields={fields.Length}");
        foreach (var f in fields)
        {
            try
            {
                Console.WriteLine($"  {f.FieldType.Name} {f.Name}");
            }
            catch
            {
                Console.WriteLine($"  <field-sig-failed> {f.Name}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"fields failed: {ex.GetType().Name}: {ex.Message}");
    }
}

void DumpMethods(Type t)
{
    try
    {
        var methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        Console.WriteLine($"methods={methods.Length}");
        foreach (var m in methods)
        {
            var printAll = string.Equals(t.FullName, "ProjectM.InventoryUtilities", StringComparison.Ordinal)
                || string.Equals(t.FullName, "ProjectM.InventoryUtilitiesServer", StringComparison.Ordinal);

            if (!printAll && !IsInterestingMethodName(m.Name))
            {
                continue;
            }

            try
            {
                var p = string.Join(", ", m.GetParameters().Select(x => x.ParameterType.Name + " " + x.Name));
                Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({p})");
            }
            catch
            {
                Console.WriteLine($"  <sig-failed> {m.Name}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"methods failed: {ex.GetType().Name}: {ex.Message}");
    }
}

bool IsInterestingMethodName(string name)
{
    var methodKeywords = new[]
    {
        "give",
        "grant",
        "add",
        "create",
        "move",
        "equip",
        "unequip",
        "drop",
        "insert",
        "remove",
        "destroy",
        "run",
        "send"
    };

    return methodKeywords.Any(k => name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
}

class PluginLoadContext : AssemblyLoadContext
{
    private readonly Dictionary<string, string> _map;
    public PluginLoadContext(Dictionary<string, string> map) : base(isCollectible: true)
    {
        _map = map;
    }

    protected override Assembly? Load(AssemblyName name)
    {
        if (_map.TryGetValue(name.Name ?? string.Empty, out var path))
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }
}
