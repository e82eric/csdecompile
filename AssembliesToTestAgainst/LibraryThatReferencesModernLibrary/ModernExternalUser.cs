using System.Collections.Generic;
using ModernLibrary;
using ModernAlias = ModernLibrary.ModernPerson;

namespace LibraryThatReferencesModernLibrary;

// Each statement is the starting point of one go to definition test.
// The tests find their position by searching for the text, so keep the call text unique.
public class ModernExternalUser
{
    public void Run(string text)
    {
        var person = new ModernPerson("a", 1);
        var personName = person.Name;
        var outer = new ModernOuter { Req = "r" };
        var inner = new ModernOuter.Inner();
        inner.InnerMethod();
        inner.InnerEvent += (s, e) => { };
        var innerField = inner.InnerField;
        var innerProp = inner.InnerProp;
        var deepest = new ModernOuter.Inner.Deepest();
        deepest.DeepestMethod();
        outer.OuterEvent += (s, e) => { };
        var byInt = outer[1];
        var byString = outer["a"];
        var generic = new ModernGeneric<int>();
        generic.GenEvent += (s, e) => { };
        generic.Take(1);
        var genericProp = generic.GenProp;
        var primary = new ModernPrimary(1);
        primary.Next();
        ModernApi.ArrInt(new[] { 1 });
        ModernApi.Arr(new[] { "a" });
        ModernApi.Jagged(new int[1, 1]);
        ModernApi.MaybeInt(1);
        ModernApi.Seq(new List<int>());
        ModernApi.Nested(new Dictionary<string, List<string>>());
        ModernApi.Arity<string>(1);
        ModernApi.Arity<string, int>(2);
        var number = 1;
        ModernApi.ByRef(ref number);
        ModernApi.GenericParam(new List<int>());
        ModernApi.Spans("a");
        ModernApi.ParamsSpan(1, 2);
        ModernApi.Tuples((1, "a"));
        ModernApi.Named(first: 1, second: 2);
        var classic = text.Classic();
        var old = text.Old();
        var twice = text.Twice;
        var thrice = text.Thrice();
        var direct = ModernExtensions.get_Twice(text);
        var aliased = new ModernAlias("b", 2);
        var summed = new ModernLocals().Sum(new List<string>());
        var described = new ModernCtor(1).Describe("p");
    }
}
