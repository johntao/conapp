using System;
using System.IO;

var q = Path.DirectorySeparatorChar;
if (Environment.CurrentDirectory.Contains((string) $"bin{q}debug{q}net5.0", StringComparison.OrdinalIgnoreCase))
    Environment.CurrentDirectory = "../../../";
ConApp.RemoveThrow.GO();