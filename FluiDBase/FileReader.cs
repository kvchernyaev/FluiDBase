using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UtfUnknown;

namespace FluiDBase
{
    public class FileReader
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        public string ReadFile(string filePath)
        {
            DetectionResult detectionResult = CharsetDetector.DetectFromFile(filePath);
            Encoding encoding = detectionResult.Detected?.Encoding;

            if (encoding == null)
            {
                // detector is not shure =)
                DetectionDetail detectionDetail = detectionResult.Details.OrderByDescending(x => x.Confidence).FirstOrDefault();
                if (detectionDetail == null)
                    throw new ProcessException("can not read file [{0}] - can not recognize encoding", filePath);

                encoding = detectionDetail.Encoding ?? Encoding.GetEncoding(detectionDetail.EncodingName);
            }

            Logger.Info("reading file {filepath}: encoding is {encoding}", filePath, encoding.EncodingName);
            Logger.Trace("DetectionDetails: {DetectionDetail}", detectionResult.Details.Select(x => $"{x.Encoding}:{x.EncodingName}:{x.Confidence}"));

            string fileContents = File.ReadAllText(filePath, encoding);
            return fileContents;
        }


        public string[] GetFiles(string sourceFolder, string filters, bool topOnly = true)
        {
            return filters.Split('|')
                .SelectMany(filter => System.IO.Directory.GetFiles(sourceFolder, filter, topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories))
                .OrderBy(x => x).ToArray();
        }


        public string GetFileType(string filePath) => Path.GetExtension(filePath);
        public string GetDirectory(string filePath) => Path.GetDirectoryName(filePath);

        public string GetRelativePath(string from, string to) => Path.GetRelativePath(from, to);
        public string CombinePath(string path1, string path2) => Path.Combine(path1, path2);

        public bool FileExists(string filePath) => File.Exists(filePath);
        public bool DirectoryExists(string dir) => Directory.Exists(dir);
    }
}
