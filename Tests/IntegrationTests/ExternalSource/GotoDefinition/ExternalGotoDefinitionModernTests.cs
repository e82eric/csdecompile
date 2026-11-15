using System;
using System.IO;
using System.Linq;
using CsDecompileLib;
using NUnit.Framework;

namespace IntegrationTests;

// Go to definition from a net10.0 project into the .NET base library and into a library built with
// current language features. The starting position is found by searching the source file for a piece
// of text, so the target sources can be edited without renumbering every test.
[TestFixture]
public class ExternalGotoDefinitionModernTests : ExternalGotoDefinitionTestBase
{
    private const string External = "ModernExternalUser.cs";
    private const string Bcl = "ModernBclUser.cs";

    public ExternalGotoDefinitionModernTests() : base(TestHarness.DotNetCoreIoClient)
    {
    }

    // --- library built with the current SDK
    [TestCase("ModernPerson(\"a\"", 0, "public record ModernPerson(string Name, int Age);", "ModernLibrary.ModernPerson")]
    [TestCase("person.Name", 7, "public record ModernPerson(string Name, int Age);", "ModernLibrary.ModernPerson")]
    [TestCase("Req = \"r\"", 0, "public required string Req { get; init; }", "ModernLibrary.ModernOuter")]
    [TestCase("ModernOuter.Inner()", 12, "public class Inner", "ModernLibrary.ModernOuter")]
    [TestCase("InnerMethod()", 0, "public void InnerMethod()", "ModernLibrary.ModernOuter")]
    [TestCase("InnerEvent +=", 0, "public event EventHandler? InnerEvent;", "ModernLibrary.ModernOuter")]
    [TestCase("inner.InnerField", 6, "public int InnerField;", "ModernLibrary.ModernOuter")]
    [TestCase("inner.InnerProp", 6, "public int InnerProp { get; set; }", "ModernLibrary.ModernOuter")]
    [TestCase("Inner.Deepest()", 6, "public class Deepest", "ModernLibrary.ModernOuter")]
    [TestCase("DeepestMethod()", 0, "public void DeepestMethod()", "ModernLibrary.ModernOuter")]
    [TestCase("OuterEvent +=", 0, "public event EventHandler? OuterEvent;", "ModernLibrary.ModernOuter")]
    [TestCase("outer[1]", 5, "public int this[int i] => i;", "ModernLibrary.ModernOuter")]
    [TestCase("outer[\"a\"]", 5, "public int this[string s] => 0;", "ModernLibrary.ModernOuter")]
    [TestCase("GenEvent +=", 0, "public event EventHandler? GenEvent;", "ModernLibrary.ModernGeneric`1")]
    [TestCase("Take(1)", 0, "public void Take(T item)", "ModernLibrary.ModernGeneric`1")]
    [TestCase("generic.GenProp", 8, "public T? GenProp { get; set; }", "ModernLibrary.ModernGeneric`1")]
    [TestCase("ModernPrimary(1)", 0, "public class ModernPrimary(int seed)", "ModernLibrary.ModernPrimary")]
    [TestCase("ArrInt(", 0, "public static void ArrInt(int[] values)", "ModernLibrary.ModernApi")]
    [TestCase("Arr(new[] { \"a\"", 0, "public static void Arr(string[] values)", "ModernLibrary.ModernApi")]
    [TestCase("Jagged(", 0, "public static void Jagged(int[,] values)", "ModernLibrary.ModernApi")]
    [TestCase("MaybeInt(", 0, "public static void MaybeInt(int? value)", "ModernLibrary.ModernApi")]
    [TestCase("Seq(new List<int>", 0, "public static void Seq(IEnumerable<int> values)", "ModernLibrary.ModernApi")]
    [TestCase("Nested(", 0, "public static void Nested(Dictionary<string, List<string>> values)", "ModernLibrary.ModernApi")]
    [TestCase("Arity<string>(1)", 0, "public static void Arity<T>(int x)", "ModernLibrary.ModernApi")]
    [TestCase("Arity<string, int>(2)", 0, "public static void Arity<T1, T2>(int x)", "ModernLibrary.ModernApi")]
    [TestCase("ByRef(ref number)", 0, "public static void ByRef(ref int x)", "ModernLibrary.ModernApi")]
    [TestCase("GenericParam(new List<int>", 0, "public static void GenericParam<T>(List<T> items)", "ModernLibrary.ModernApi")]
    [TestCase("Spans(", 0, "public static void Spans(ReadOnlySpan<char> s)", "ModernLibrary.ModernApi")]
    [TestCase("ParamsSpan(", 0, "public static void ParamsSpan(params ReadOnlySpan<int> xs)", "ModernLibrary.ModernApi")]
    [TestCase("Tuples(", 0, "public static void Tuples((int a, string b) pair)", "ModernLibrary.ModernApi")]
    [TestCase("first: 1", 0, "public static void Named(int first, int second)", "ModernLibrary.ModernApi")]
    [TestCase("Classic()", 0, "public static int Classic(this string s)", "ModernLibrary.ModernApi")]
    [TestCase("Old()", 0, "public static int Old(this string s)", "ModernLibrary.ModernExtensions")]
    [TestCase("text.Twice", 5, "public int Twice => s.Length * 2;", "ModernLibrary.ModernExtensions")]
    [TestCase("Thrice()", 0, "public int Thrice()", "ModernLibrary.ModernExtensions")]
    [TestCase("get_Twice(text)", 0, "public int Twice => s.Length * 2;", "ModernLibrary.ModernExtensions")]
    [TestCase("ModernAlias(\"b\"", 0, "public record ModernPerson(string Name, int Age);", "ModernLibrary.ModernPerson")]
    public void ExternalLibrary(string textToFind, int offset, string expectedLine, string expectedType)
    {
        var (line, sourceResponse) = GoToDefinition(External, textToFind, offset);
        Assert.AreEqual(expectedLine, line);
        Assert.AreEqual(expectedType, sourceResponse.Location.ContainingTypeFullName);
    }

    // --- .NET base library. The decompiled text carries attributes and nullable annotations that can
    // change between runtime versions, so these only check for the part that identifies the member.
    [TestCase("WriteLine(\"simple\")", 0, "public static void WriteLine(string", "System.Console")]
    [TestCase("Join(\",\", new[]", 0, "public static string Join(string? separator, params string?[] value)", "System.String")]
    [TestCase("Concat(list)", 0, "public static string Concat<T>(IEnumerable<T> values)", "System.String")]
    [TestCase("AddRange(", 0, "public void AddRange(IEnumerable<T> collection)", "System.Collections.Generic.List`1")]
    [TestCase("Where(x", 0, " Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)", "System.Linq.Enumerable")]
    [TestCase("ToList()", 0, " ToList<TSource>(this IEnumerable<TSource> source)", "System.Linq.Enumerable")]
    [TestCase("Select(x", 0, " Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)", "System.Linq.Enumerable")]
    [TestCase("First()", 0, " First<TSource>(this IEnumerable<TSource> source)", "System.Linq.Enumerable")]
    [TestCase("SpecialFolder.App", 0, "public enum SpecialFolder", "System.Environment")]
    [TestCase("ApplicationData", 0, "ApplicationData =", "System.Environment")]
    [TestCase("CollectionChanged +=", 0, " CollectionChanged", "System.Collections.ObjectModel.ObservableCollection`1")]
    [TestCase("WriteAllBytes(", 0, "public static void WriteAllBytes(string path, byte[] bytes)", "System.IO.File")]
    [TestCase("Run(() => 1)", 0, " Run<TResult>(Func<TResult> function)", "System.Threading.Tasks.Task")]
    [TestCase("list[0]", 4, " this[int index]", "System.Collections.Generic.List`1")]
    [TestCase("var arr", 0, "struct Int32", "System.Int32")]
    [TestCase("Nullable<int> n2", 0, "struct Nullable<T>", "System.Nullable`1")]
    [TestCase("HasValue", 0, "bool HasValue", "System.Nullable`1")]
    [TestCase("MoveNext()", 0, "public bool MoveNext()", "System.Collections.Generic.List`1")]
    [TestCase("UtcNow", 0, "static DateTime UtcNow", "System.DateTime")]
    [TestCase("val1: 1", 0, "public static int Min(int val1, int val2)", "System.Math")]
    [TestCase("Write(bytes, 0", 0, "public override void Write(byte[] buffer, int offset, int count)", "System.IO.MemoryStream")]
    [TestCase("tuple.Item1", 6, "public T1 Item1", "System.ValueTuple`2")]
    public void BaseLibrary(string textToFind, int offset, string expectedLineContains, string expectedType)
    {
        var (line, sourceResponse) = GoToDefinition(Bcl, textToFind, offset);
        StringAssert.Contains(expectedLineContains, line);
        Assert.AreEqual(expectedType, sourceResponse.Location.ContainingTypeFullName);
    }

    // --- starting inside decompiled code
    [TestCase("ModernPrimary(1)", "return seed \\+ 1;", "return (?<token>seed) \\+ 1;", "public class ModernPrimary(int seed)", "ModernLibrary.ModernPrimary")]
    [TestCase("ModernPrimary(1)", "class ModernPrimary\\(", "\\(int (?<token>seed)\\)", "public class ModernPrimary(int seed)", "ModernLibrary.ModernPrimary")]
    [TestCase("ModernPerson(\"a\"", "record ModernPerson\\(", "\\(string (?<token>Name),", "public record ModernPerson(string Name, int Age);", "ModernLibrary.ModernPerson")]
    [TestCase("Thrice()", "extension\\(string s\\)", "extension\\((?<token>string) s\\)", "public sealed class String", "System.String")]
    public void FromDecompiledCode(string textToFind, string lineToFind, string tokenToRequest, string expectedLineContains, string expectedType)
    {
        var start = CreateRequest(External, textToFind, 0).Arguments;
        var requestArguments = GotoDefinitionAndCreateRequestForToken(
            start.FileName,
            start.Column,
            start.Line,
            lineToFind,
            tokenToRequest);

        var sourceResponse = GotoDefinitionHelper.Run(IoClient, new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = requestArguments
        });

        var lines = GetLines(sourceResponse.Body.SourceText);
        StringAssert.Contains(expectedLineContains, lines[sourceResponse.Body.Location.Line - 1].Trim());
        Assert.AreEqual(expectedType, sourceResponse.Body.Location.ContainingTypeFullName);
    }

    // A position in decompiled code where no syntax node starts
    [Test]
    public void PositionWithoutASymbolInDecompiledCodeIsReported()
    {
        var start = CreateRequest(External, "ModernPrimary(1)", 0).Arguments;
        var requestArguments = GotoDefinitionAndCreateRequestForToken(
            start.FileName,
            start.Column,
            start.Line,
            "return seed \\+ 1;",
            "return (?<token>seed) \\+ 1;");
        requestArguments.Column = 500;

        var response = IoClient.ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(
            new CommandPacket<DecompiledLocationRequest>
            {
                Command = Endpoints.DecompileGotoDefinition,
                Arguments = requestArguments
            });

        Assert.False(response.Success);
        Assert.AreEqual(
            $"SYMBOL_NOT_FOUND_AT_LOCATION ModernLibrary.ModernPrimary:{requestArguments.Line}:500",
            response.Message);
    }

    // The implementation of a [GeneratedRegex] method is written by a source generator and only exists
    // in memory. The location has to be a file the editor can open.
    [Test]
    public void SourceGeneratedPartialMethodGoesToAFileThatExists()
    {
        var generated = GoToGeneratedRegexImplementation();

        Assert.True(File.Exists(generated.FileName), $"{generated.FileName} does not exist");
        var lines = File.ReadAllLines(generated.FileName);
        StringAssert.Contains("Regex MyRegex()", lines[generated.Line - 1]);
        StringAssert.Contains("Regex MyRegex()", generated.SourceText);
    }

    [Test]
    public void GoToDefinitionWorksFromInsideSourceGeneratedFile()
    {
        var generated = GoToGeneratedRegexImplementation();
        var line = File.ReadAllLines(generated.FileName)[generated.Line - 1];

        var request = new CommandPacket<DecompiledLocationRequest>
        {
            Command = Endpoints.DecompileGotoDefinition,
            Arguments = new DecompiledLocationRequest
            {
                FileName = generated.FileName,
                Type = LocationType.SourceCode,
                Line = generated.Line,
                Column = line.IndexOf("Regex MyRegex()", StringComparison.Ordinal)
            }
        };

        var response = IoClient.ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);

        Assert.True(response.Success, response.Message);
        Assert.AreEqual("System.Text.RegularExpressions.Regex", response.Body.Locations.First().ContainingTypeFullName);
    }

    private SourceFileInfo GoToGeneratedRegexImplementation()
    {
        var response = IoClient.ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(
            CreateRequest(Bcl, "MyRegex().IsMatch", 0));

        Assert.True(response.Success, response.Message);
        Assert.AreEqual(1, response.Body.Locations.Count);
        Assert.AreEqual(LocationType.SourceCode, response.Body.Locations.First().Type);
        return (SourceFileInfo)response.Body.Locations.First();
    }

    // A tuple element name that comes from another assembly (Math.DivRem returns
    // (int Quotient, int Remainder)) exists only as an attribute, so there is no member to find in
    // the decompiled ValueTuple. That is reported by name instead of jumping to a nearby location.
    [Test]
    public void MemberThatCannotBeMatchedIsReportedByName()
    {
        var response = IoClient.ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(
            CreateRequest(Bcl, "Quotient", 0));

        Assert.False(response.Success);
        Assert.AreEqual("MEMBER_NOT_FOUND System.ValueTuple`2.Quotient", response.Message);
    }

    private (string line, CsDecompileLib.GetSource.DecompiledSourceResponse response) GoToDefinition(
        string fileName,
        string textToFind,
        int offset)
    {
        var request = CreateRequest(fileName, textToFind, offset);
        var response = IoClient.ExecuteCommand<DecompiledLocationRequest, LocationsResponse>(request);
        Assert.True(response.Success, response.Message);

        var sourceResponse = GotoDefinitionHelper.Run(IoClient, request);
        var lines = GetLines(sourceResponse.Body.SourceText);
        var line = lines[sourceResponse.Body.Location.Line - 1].Trim();
        return (line, sourceResponse.Body);
    }

    private static CommandPacket<DecompiledLocationRequest> CreateRequest(string fileName, string textToFind, int offset)
    {
        var filePath = TestHarness.GetLibraryThatReferencesModernLibraryFilePath(fileName);
        var lines = File.ReadAllLines(filePath);
        for (var i = 0; i < lines.Length; i++)
        {
            var index = lines[i].IndexOf(textToFind, StringComparison.Ordinal);
            if (index >= 0)
            {
                return new CommandPacket<DecompiledLocationRequest>
                {
                    Command = Endpoints.DecompileGotoDefinition,
                    Arguments = new DecompiledLocationRequest
                    {
                        FileName = filePath,
                        Type = LocationType.SourceCode,
                        Line = i + 1,
                        Column = index + offset
                    }
                };
            }
        }

        Assert.Fail($"'{textToFind}' was not found in {filePath}");
        return null;
    }
}
