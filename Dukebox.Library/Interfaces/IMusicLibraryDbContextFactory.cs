using System.Threading.Tasks;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryDbContextFactory
    {
        IMusicLibraryDbContext GetInstance();
        Task SaveDbChanges(IMusicLibraryDbContext dukeboxData);
        string ImportLibraryFile(string libraryFileToImport);
        void ExportCurrentLibraryFile(string outputPath);
    }
}