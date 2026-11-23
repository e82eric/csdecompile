using ModernLibrary;

namespace ModernLibraryUser;

// A compiled assembly that uses ModernLibrary, so find usages has something to find outside of
// the library itself and the source projects.
public class ModernNestedUser
{
    public void Run()
    {
        var compiledInner = new ModernOuter.Inner();
        compiledInner.InnerMethod();
        compiledInner.InnerField = 1;
        compiledInner.InnerProp = 2;
        var compiledDeepest = new ModernOuter.Inner.Deepest();
        compiledDeepest.DeepestMethod();
    }
}
