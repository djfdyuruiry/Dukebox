using System.Collections.Generic;
using Dukebox.Library.Model;

namespace Dukebox.Library.Interfaces
{
    public interface ICdMetadataService
    {
        List<IAudioFileMetadata> GetAudioFileMetaDataForCd(char driveLetter);
        CdMetadata GetMetadataForCd(char driveLetter);
    }
}
