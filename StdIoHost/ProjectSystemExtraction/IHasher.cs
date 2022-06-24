namespace StdIoHost.ProjectSystemExtraction;

public interface IHasher
{
    HashedString HashInput(string clearText);
}