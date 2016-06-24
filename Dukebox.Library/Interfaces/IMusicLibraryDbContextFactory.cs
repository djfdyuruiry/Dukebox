using System.Data.Entity.Validation;
using Dukebox.Library.Interfaces;

namespace Dukebox.Library.Interfaces
{
    public interface IMusicLibraryDbContextFactory
    {
        IMusicLibraryDbContext GetInstance();
        void LogEntityValidationException(DbEntityValidationException ex);
    }
}