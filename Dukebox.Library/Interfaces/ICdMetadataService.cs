using Dukebox.Library.Model;
using Dukebox.Model.Services;
using System;
using System.Collections.Generic;

namespace Dukebox.Library.Interfaces
{
    public interface ICdMetadataService
    {
        void CheckForCddbServerConnection();
        List<AudioFileMetaData> GetAudioFileMetaDataForCd(char driveLetter);
        CdMetadata GetMetadataForCd(char driveLetter);
    }
}
