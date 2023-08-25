/**
 * @file IniFile.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief Provides utilities for reading and writing INI files.
 */

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DiplomaMB.Utils
{
    /// <summary>
    /// Provides utilities for reading and writing INI files.
    /// </summary>
    internal class IniFile
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        /// <summary>
        /// Initializes a new instance of the <see cref="IniFile"/> class.
        /// </summary>
        /// <param name="IniPath">The path to the INI file.</param>
        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName;
        }

        /// <summary>
        /// Reads a value from the INI file.
        /// </summary>
        /// <param name="Key">The key for which to read the value.</param>
        /// <param name="Section">The section from which to read the value. Defaults to the name of the executing assembly.</param>
        /// <returns>Returns the read value as a string.</returns>
        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        /// <summary>
        /// Writes a key-value pair to the INI file.
        /// </summary>
        /// <param name="Key">The key for which to write the value.</param>
        /// <param name="Value">The value to write.</param>
        /// <param name="Section">The section to which to write the value. Defaults to the name of the executing assembly.</param>
        public void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        /// <summary>
        /// Deletes a key from the INI file.
        /// </summary>
        /// <param name="Key">The key to delete.</param>
        /// <param name="Section">The section from which to delete the key. Defaults to the name of the executing assembly.</param>
        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        /// <summary>
        /// Deletes a section from the INI file.
        /// </summary>
        /// <param name="Section">The section to delete. Defaults to the name of the executing assembly.</param>
        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        /// <summary>
        /// Checks if a key exists in the INI file.
        /// </summary>
        /// <param name="Key">The key to check.</param>
        /// <param name="Section">The section in which to check for the key. Defaults to the name of the executing assembly.</param>
        /// <returns>Returns true if the key exists, otherwise false.</returns>
        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
