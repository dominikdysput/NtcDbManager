using DbManager.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Logic
{
    public class ChecksumMD5 : IChecksum
    {
        public string CalculateChecksum(string pathToFile)
        {
            using (var md5 = MD5.Create())
            {
                using (var fileStream = File.OpenRead(pathToFile))
                {
                    var hash = md5.ComputeHash(fileStream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
