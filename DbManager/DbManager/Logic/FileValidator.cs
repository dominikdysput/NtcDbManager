using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic
{
    class FileValidator : IFileValidator
    {
        private readonly IMetaData _metaData;
        public FileValidator(IMetaData metaData)
        {
            _metaData = metaData;
        }
        public bool CheckVersionAlreadyExists(int id, string checksumForInputFile)
        {
            var table = _metaData.ReadDetails(id);
            var checksumAllFiles = table.AsEnumerable().Select(r => r.Field<string>("Checksum")).ToList();

            foreach (var checksum in checksumAllFiles)
            {
                if (checksumForInputFile.Equals(checksum))
                    return true;
            }
            return false;
        }
    }
}
