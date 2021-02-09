using System.Collections.Generic;
using System.IO;

namespace TSMapEditor.CCEngine
{
    public class CCFileManager
    {
        private Dictionary<uint, FileLocationInfo> fileLocationInfos = new Dictionary<uint, FileLocationInfo>();
        private List<MixFile> mixFiles = new List<MixFile>();

        private List<string> searchDirectories = new List<string>();


        /// <summary>
        /// Adds a directory to the list of directories where files will be
        /// searched from.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        public void AddSearchDirectory(string path)
        {
            char lastChar = path[path.Length - 1];
            if (lastChar != '/' && lastChar != '\\')
                path = path + "/";
            searchDirectories.Add(path);
        }

        /// <summary>
        /// Attempts to search for and load a MIX file from the search directories.
        /// Returns true if loading the MIX file succeeds, otherwise false.
        /// </summary>
        /// <param name="name">The name of the MIX file.</param>
        /// <returns></returns>
        private bool LoadMixFromDirectories(string name)
        {
            string searchDir = null;

            foreach (string dir in searchDirectories)
            {
                if (File.Exists(dir + name))
                {
                    searchDir = dir;
                    break;
                }
            }

            if (searchDir == null)
                return false;

            var mixFile = new MixFile();
            mixFile.Parse(searchDir + name);
            AddMix(mixFile);

            return true;
        }

        /// <summary>
        /// Loads a MIX file.
        /// Does not search for it in already loaded MIX files.
        /// Throws a FileNotFoundException if the MIX file isn't found.
        /// </summary>
        /// <param name="path">The name of the MIX file.</param>
        public void LoadPrimaryMixFile(string name)
        {
            if (!LoadMixFromDirectories(name))
                throw new FileNotFoundException("Primary MIX file not found: " + name);
        }


        /// <summary>
        /// Loads a MIX file.
        /// Searches for it in already loaded MIX files.
        /// Does not throw an exception if the MIX file is not found.
        /// </summary>
        /// <param name="name">The name of the MIX file.</param>
        public void LoadSecondaryMixFile(string name)
        {
            uint identifier = MixFile.GetFileID(name);

            MixFile mixFile = null;

            if (fileLocationInfos.TryGetValue(identifier, out FileLocationInfo value))
            {
                mixFile = new MixFile(value.MixFile, value.Offset);
                mixFile.Parse();
                AddMix(mixFile);
            }
            else
            {
                LoadMixFromDirectories(name);
            }
        }

        private void AddMix(MixFile mixFile)
        {
            mixFiles.Add(mixFile);

            foreach (MixFileEntry fileEntry in mixFile.GetEntries())
            {
                fileLocationInfos[fileEntry.Identifier] = new FileLocationInfo(mixFile, fileEntry.Offset, fileEntry.Size);
            }
        }

        public byte[] LoadFile(string name)
        {
            uint id = MixFile.GetFileID(name);

            if (fileLocationInfos.TryGetValue(id, out FileLocationInfo value))
            {
                return value.MixFile.GetSingleFileData(value.Offset, value.Size);
            }

            return null;
        }
    }

    internal struct FileLocationInfo
    {
        public FileLocationInfo(MixFile mixFile, int offset, int size)
        {
            MixFile = mixFile;
            Offset = offset;
            Size = size;
        }

        public MixFile MixFile { get; private set; }
        public int Offset { get; private set; }
        public int Size { get; private set; }
    }
}
