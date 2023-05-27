using LibraryThatReferencesFrameworkCore;

namespace LibraryThatReferencesCoreLibrary;

public class DataContractUser
{
    public void Run()
    {
        new DataContractTarget();
    }
}