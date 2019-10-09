using System;
using System.Collections.Generic;
using System.Text;

namespace FluiDBase
{
    public class FileDescriptor
    {
        private readonly FileReader _fileReader;
        private readonly string _baseDir;


        FileDescriptor(string path, string baseDir, FileReader fileReader)
        {
            Path = path;
            _baseDir = baseDir;
            _fileReader = fileReader;
        }


        public FileDescriptor(string path, FileDescriptor parent)
            : this(path, parent._baseDir, parent._fileReader)
        {
            Parent = parent;
        }


        public FileDescriptor(string path, FileReader fileReader)
            : this(path, fileReader.GetDirectory(path), fileReader)
        {
            Parent = new FileDescriptor(_baseDir, _baseDir, fileReader);
        }



        public string Path;

        public string Dir => _fileReader.GetDirectory(Path);
        public string Type => _fileReader.GetFileType(Path);


        public FileDescriptor Parent;


        public string PathFromBase => _fileReader.GetRelativePath(_baseDir, Path);
        public string PathFromParent => _fileReader.GetRelativePath(Parent.Dir, Path);

    }
}
