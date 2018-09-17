using System;
using System.IO;

namespace LECommonLibrary
{
    public enum PEType
    {
        X32,
        X64,
        Unknown
    }

    public static class PEFileReader
    {
        public static PEType GetPEType(string path)
        {
            if (string.IsNullOrEmpty(path))
                return PEType.Unknown;

            //The following if clauses are meant to fix non-win32 executables with .exe extension
            //Possible fixes:
            //1. move BinaryReader br outside the try block and call br.close() in catch
            //   (proposed by original author, but I was afraid of creating br throwing execptions)
            //2. Veryfy the range every time the binary reader reads anything
            //   (The most direct fix)
            //3. add a line for verifying the first two bytes of the .exe to be "MZ" or 0x4D5A
            //   (This may be a better solution, but I'm not sure if things will go fine if
            //    the file is actually a dll or so with the same bytes)
            //PS: I don't know why the file handle doen't close after both the stream and
            //    the binary reader moves out of scope...
            //PPS:Following code verifies the first two bytes, moved the reader to a outside try block
            //    and made sure that the range is correct (all three methods)
            try
            {
                using (var br = new BinaryReader(new FileStream(path,
                                                    FileMode.Open,
                                                    FileAccess.Read,
                                                    FileShare.ReadWrite)))
                {
                    if (br.BaseStream.Length < 0x3C + 4 || br.ReadUInt16() != 0x5A4D)
                        return PEType.Unknown;

                    br.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                    var pos = br.ReadUInt32() + 4;

                    if (pos + 2 > br.BaseStream.Length)
                        return PEType.Unknown;

                    br.BaseStream.Seek(pos, SeekOrigin.Begin);
                    var machine = br.ReadUInt16();

                    if (machine == 0x014C)
                        return PEType.X32;
                    else if (machine == 0x8664)
                        return PEType.X64;

                    return PEType.Unknown;
                }
            }
            catch
            {
                //method to deal with error in starting to read
                return PEType.Unknown;
            }
        }
    }
}
