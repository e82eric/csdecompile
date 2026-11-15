using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LibraryThatReferencesModernLibrary;

// Each statement is the starting point of one go to definition test.
// The tests find their position by searching for the text, so keep the call text unique.
public partial class ModernBclUser
{
    public async Task Run(List<int> list, string text, int? maybe, byte[] bytes)
    {
        Console.WriteLine("simple");
        var joined = string.Join(",", new[] { "a", "b" });
        var concat = string.Concat(list);
        list.AddRange(new[] { 1, 2 });
        var evens = list.Where(x => x % 2 == 0).ToList();
        var first = list.Select(x => x.ToString()).First();
        var folder = Environment.SpecialFolder.ApplicationData;
        var oc = new ObservableCollection<int>();
        oc.CollectionChanged += (s, e) => { };
        File.WriteAllBytes("x", bytes);
        await Task.Run(() => 1);
        var item = list[0];
        var arr = new int[3];
        Nullable<int> n2 = maybe;
        var has = maybe.HasValue;
        var e2 = list.GetEnumerator();
        e2.MoveNext();
        var dt = DateTime.UtcNow.AddDays(1);
        var min = Math.Min(val1: 1, val2: 2);
        using var ms = new MemoryStream();
        ms.Write(bytes, 0, 1);
        var m = MyRegex().IsMatch(text);
        var tuple = (1, "a");
        var t1 = tuple.Item1;
        var quotient = Math.DivRem(7, 2).Quotient;
    }

    [GeneratedRegex("a+")]
    private static partial Regex MyRegex();
}
