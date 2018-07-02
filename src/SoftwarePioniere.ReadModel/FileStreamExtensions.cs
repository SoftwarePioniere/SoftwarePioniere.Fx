using System;
using System.IO;
using System.Reflection;

namespace SoftwarePioniere.ReadModel
{
    public static class FileStreamExtensions
    {
        private static Stream GetFileStream(this Assembly assembly, string fileName)
        {
            //var auxList = assembly.GetManifestResourceNames();
            var file = $"{assembly.GetName().Name}.{fileName}";
            var stream = assembly.GetManifestResourceStream(file);
            return stream;
        }

        public static string GetResourceFileContent(this Assembly assembly, string fileName)
        {
            using (var stream = GetFileStream(assembly, fileName))
            {
                if (stream == null) throw new InvalidOperationException($"Cannot open stream {fileName}");

                using (var textStreamReader = new StreamReader(stream))
                {
                    return textStreamReader.ReadToEnd();
                }
            }
        }
    }
}
