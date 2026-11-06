using System;
using System.IO;

namespace WindBot
{
    /// <summary>
    /// File access seam. Desktop uses the file system; Unity installs a Provider that serves
    /// embedded TextAssets, because mobile players cannot read loose files in the package.
    /// </summary>
    public static class WindBotIO
    {
        /// <summary>(directory, filename, extension) -> stream, or null when missing.</summary>
        public static Func<string, string, string, Stream> Provider;

        public static Stream ReadFile(string directory, string filename, string extension)
        {
            if (Provider != null)
                return Provider(directory, filename, extension);

            return OpenFromDisk(directory, filename, extension);
        }

        internal static Stream OpenFromDisk(string directory, string filename, string extension)
        {
            string tryfilename = filename + "." + extension;
            string fullpath = Path.Combine(directory, tryfilename);
            if (!File.Exists(fullpath))
                fullpath = filename;
            if (!File.Exists(fullpath))
                fullpath = Path.Combine("../", filename);
            if (!File.Exists(fullpath))
                fullpath = Path.Combine("../deck/", filename);
            if (!File.Exists(fullpath))
                fullpath = Path.Combine("../", tryfilename);
            if (!File.Exists(fullpath))
                fullpath = Path.Combine("../deck/", tryfilename);
            return new FileStream(fullpath, FileMode.Open, FileAccess.Read);
        }
    }
}
