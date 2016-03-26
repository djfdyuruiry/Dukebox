using Dukebox.Library.Model;
using Dukebox.Model.Services;
using System;
using System.Collections.Generic;

namespace Dukebox.Library.Interfaces
{
    public interface ICdMetadataService
    {
        List<AudioFileMetadata> GetAudioFileMetaDataForCd(char driveLetter);
        CdMetadata GetMetadataForCd(char driveLetter);
    }
}
