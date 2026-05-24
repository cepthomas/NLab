using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;


namespace NLab
{
    public class LinkFiles
    {
        public static string? GetLinkTarget(string filepath)
        {
            // OK @"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Firefox.lnk",
            // NG @"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Administrative Tools\Performance Monitor.lnk",

            string? path = null;


            var bytes = File.ReadAllBytes(filepath);


            try
            {
                // read the LinkFlags structure.
                // OK 0x009F   NG 0x020003e4 -> 0010000000000000001111100100
                uint LinkFlags = BitConverter.ToUInt32(bytes, 0x14); // => 24

                bool HasLinkTargetIDList = (LinkFlags & 0X00000001) > 0; // OK
                bool HasLinkInfo = (LinkFlags & 0X00000002) > 0; // OK
                bool HasName = (LinkFlags & 0X00000004) > 0; // OK  NG
                bool HasRelativePath = (LinkFlags & 0X00000008) > 0; // OK

                bool HasWorkingDir = (LinkFlags & 0X00000010) > 0; // OK
                bool HasArguments = (LinkFlags & 0X00000020) > 0; // NG
                bool HasIconLocation = (LinkFlags & 0X00000040) > 0; // NG
                bool IsUnicode = (LinkFlags & 0X00000080) > 0; // OK NG

                bool ForceNoLinkInfo = (LinkFlags & 0X00000100) > 0; // NG
                bool HasExpString = (LinkFlags & 0X00000200) > 0; // NG
                bool RunInSeparateProcess = (LinkFlags & 0X00000400) > 0;
                bool Unused1 = (LinkFlags & 0X00000800) > 0;

                bool HasDarwinID = (LinkFlags & 0X00001000) > 0;
                bool RunAsUser = (LinkFlags & 0X00002000) > 0;
                bool HasExpIcon = (LinkFlags & 0X00004000) > 0;
                bool NoPidlAlias = (LinkFlags & 0X00008000) > 0;

                bool Unused2 = (LinkFlags & 0X00010000) > 0;
                bool RunWithShimLayer = (LinkFlags & 0X00020000) > 0;
                bool ForceNoLinkTrack = (LinkFlags & 0X00040000) > 0;
                bool EnableTargetMetadata = (LinkFlags & 0X00080000) > 0;

                bool DisableLinkPathTracking = (LinkFlags & 0X00100000) > 0;
                bool DisableKnownFolderTracking = (LinkFlags & 0X00200000) > 0;
                bool DisableKnownFolderAlias = (LinkFlags & 0X00400000) > 0;
                bool AllowLinkToLink = (LinkFlags & 0X00800000) > 0;

                bool UnaliasOnSave = (LinkFlags & 0X01000000) > 0;
                bool PreferEnvironmentPath = (LinkFlags & 0X02000000) > 0; // NG
                bool KeepLocalIDListForUNCTarget = (LinkFlags & 0X04000000) > 0;

                // 0x0050 80  unicode
                // %@%systemroot%\system32\Wdc.dll,-10025_/s_%windir%\system32\wdc.dllf_-_1S
                // 
                // 0x0140 320  utf8
                // %windir%\system32\perfmon.msc
                // 
                // 0x0244 580  unicode
                // %windir%\system32\perfmon.msc

                // 0x202  514  utf8
                // C:\Program Files\Mozilla Firefox\firefox.exe

                int fileInfoStartsAt = -1; // Store the offset where the file info

                int index = 0x4C;

                // if the HasLinkTargetIDList bit is set then skip the stored IDList structure and header
                if (HasLinkTargetIDList)  
                {
                    int offset = BitConverter.ToUInt16(bytes, index);   // Read the length of the Shell item ID list  0x185  389
                    index += 2;
                    fileInfoStartsAt = index + offset;
                }

                if (HasName)
                {
                }


                //long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info

                // structure begins
                int totalStructLength = (int)BitConverter.ToUInt32(bytes, index); // read the length of the whole struct
                index += 4;
                // seek to offset to base pathname
                index += 0x0C;
                //fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                int fileOffset = (int)BitConverter.ToUInt32(bytes, index); // read offset to base pathname
                index += 4;

                // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                index = fileInfoStartsAt + fileOffset; // Seek to beginning of base pathname (target)
                // read the base pathname. I don't need the 2 terminating nulls    <<<================
                int pathLength = totalStructLength + fileInfoStartsAt - index - 2;
                //fileStream.Seek(fileInfoStartsAt + fileOffset, SeekOrigin.Begin); // Seek to beginning of base pathname (target)
                //long pathLength = totalStructLength + fileInfoStartsAt - fileStream.Position - 2; // read the base pathname. I don't need the 2 terminating nulls.

                var targetBytes = bytes.Subset(index, pathLength);
                //br.ReadBytes((int)pathLength); // should be unicode safe
                //char[] linkTarget = br.ReadChars((int)pathLength); // should be unicode safe

                // Convert using UTF-8 (Most common)
                string utf8String = Encoding.UTF8.GetString(targetBytes);

                // Convert using ASCII
                string asciiString = Encoding.ASCII.GetString(targetBytes);
                var link = utf8String;


                int begin = link.IndexOf("\0\0"); // TODO1 returns 0???
                if (begin > -1)
                {
                    int end = link.IndexOf("\\\\", begin + 2) + 2;
                    end = link.IndexOf('\0', end) + 1;

                    string firstPart = link.Substring(0, begin);
                    string secondPart = link.Substring(end);

                    return firstPart + secondPart;
                }
                else
                {
                    return link;
                }
            }
            catch (Exception e)
            {
            }

            return path;
        }

        /// <summary>
        /// Source - https://stackoverflow.com/a/64126237
        /// Posted by GLJ
        /// Retrieved 2026-05-22, License - CC BY-SA 4.0
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string? GetLinkTarget_orig(string filepath)
        {
            //python version  https://stackoverflow.com/a/28952464
            //file spec  https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-shllink/16cb4ca1-9339-4d0c-a68d-bf1d6cc0f943?redirectedfrom=MSDN

            // ok @"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Firefox.lnk",
            // ng @"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Administrative Tools\Performance Monitor.lnk",


            string? path = null;

            using var br = new BinaryReader(File.OpenRead(filepath));
            try
            {
                // skip the first 20 bytes (HeaderSize and LinkCLSID)
                br.ReadBytes(0x14);

                // read the LinkFlags structure (4 bytes)
                uint lflags = br.ReadUInt32();

                // if the HasLinkTargetIDList bit is set then skip the stored IDList structure and header
                if ((lflags & 0x01) == 1)
                {
                    br.ReadBytes(0x34);
                    var skip = br.ReadUInt16(); // this counts of how far we need to skip ahead
                    br.ReadBytes(skip);
                }

                // get the number of bytes the path contains
                var length = br.ReadUInt32();

                // skip 12 bytes (LinkInfoHeaderSize, LinkInfoFlgas, and VolumeIDOffset)
                br.ReadBytes(0x0C);

                // Find the location of the LocalBasePath position
                var lbpos = br.ReadUInt32();

                // Skip to the path position (subtract the length of the read (4 bytes), the length of
                // the skip (12 bytes), and the length of the lbpos read (4 bytes) from the lbpos)
                br.ReadBytes((int)lbpos - 0x14); //TODO1 lbpos is 0 for:
                                                 //C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Administrative Tools\Performance Monitor.lnk
                                                 //C:\ProgramData\Microsoft\Windows\Start Menu\Programs\System Tools\Task Manager.lnk

                var size = length - lbpos - 0x02;
                var bytePath = br.ReadBytes((int)size);

                path = Encoding.UTF8.GetString(bytePath, 0, bytePath.Length);
            }
            catch (Exception e)
            {
            }

            return path;
        }



        // /// <summary>
        // /// Version that doesn't throw. TODO1 put somewhere else?
        // /// </summary>
        // /// <param name="name"></param>
        // /// <returns></returns>
        // public static Icon? SafeExtractIcon(string name)
        // {
        //     try
        //     {
        //         var icon = Icon.ExtractAssociatedIcon(name);
        //         return icon;
        //     }
        //     catch (Exception)
        //     {
        //         return null;
        //     }
        // }

        // /// <summary>Resize the image to the specified width and height.</summary>
        // /// <param name="bmp">The image to resize.</param>
        // /// <param name="width">The width to resize to.</param>
        // /// <param name="height">The height to resize to.</param>
        // /// <returns>The resized image.</returns>
        // public static Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        // {
        //     Bitmap result = new(width, height);
        //     result.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

        //     using (Graphics graphics = Graphics.FromImage(result))
        //     {
        //         // Set high quality.
        //         graphics.CompositingQuality = CompositingQuality.HighQuality;
        //         graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //         graphics.SmoothingMode = SmoothingMode.HighQuality;
        //         graphics.CompositingQuality = CompositingQuality.HighQuality;
        //         graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //         graphics.SmoothingMode = SmoothingMode.HighQuality;

        //         // Draw the image.
        //         graphics.DrawImage(bmp, 0, 0, result.Width, result.Height);
        //     }

        //     return result;
        // }
    }
}
