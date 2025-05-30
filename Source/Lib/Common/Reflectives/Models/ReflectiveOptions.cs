using System.Reflection;

namespace Walk.Common.RazorLib.Reflectives.Models;

public record ReflectiveOptions(params Assembly[] AssembliesToScanList);