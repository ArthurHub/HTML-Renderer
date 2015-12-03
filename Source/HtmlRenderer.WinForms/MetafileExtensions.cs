using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace TheArtOfDev.HtmlRenderer.WinForms
{
    public static class MetafileExtensions
    {
        public static void SaveAsEmf(Metafile me, string fileName)
        {
            /* http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/12a1c749-b320-4ce9-aff7-9de0d7fd30ea 
                How to save or serialize a Metafile: Solution found 
                by : SWAT Team member _1 
                Date : Friday, February 01, 2008 1:38 PM 
                */
            int enfMetafileHandle = me.GetHenhmetafile().ToInt32();
            int bufferSize = GetEnhMetaFileBits(enfMetafileHandle, 0, null); // Get required buffer size.  
            byte[] buffer = new byte[bufferSize]; // Allocate sufficient buffer  
            if (GetEnhMetaFileBits(enfMetafileHandle, bufferSize, buffer) <= 0) // Get raw metafile data.  
                throw new SystemException("Fail");

            FileStream ms = File.Open(fileName, FileMode.Create);
            ms.Write(buffer, 0, bufferSize);
            ms.Close();
            ms.Dispose();
            if (!DeleteEnhMetaFile(enfMetafileHandle)) //free handle  
                throw new SystemException("Fail Free");
        }

        [DllImport("gdi32")]
        public static extern int GetEnhMetaFileBits(int hemf, int cbBuffer, byte[] lpbBuffer);

        [DllImport("gdi32")]
        public static extern bool DeleteEnhMetaFile(int hemfbitHandle);
    }  
}
