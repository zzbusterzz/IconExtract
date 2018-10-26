using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ExtractIcon
{
    static byte[] bytes;
    static byte[] copyToBytes;
    static System.Drawing.Imaging.BitmapData bitmapData;
    static IntPtr Iptr = IntPtr.Zero;

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHGetFileInfo(
                                                string pszPath,
                                                int dwFileAttributes,
                                                out SHFILEINFO psfi,
                                                uint cbfileInfo,
                                                SHGFI uFlags);

    /// <summary>Maximal Length of unmanaged Windows-Path-strings</summary>
    private const int MAX_PATH = 260;
    /// <summary>Maximal Length of unmanaged Typename</summary>
    private const int MAX_TYPE = 80;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEINFO
    {
        public SHFILEINFO(bool b)
        {
            hIcon = IntPtr.Zero;
            iIcon = 0;
            dwAttributes = 0;
            szDisplayName = "";
            szTypeName = "";
        }
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_TYPE)]
        public string szTypeName;
    };

    [Flags]
    enum SHGFI : int
    {
        /// <summary>get icon</summary>
        Icon = 0x000000100,
        /// <summary>get display name</summary>
        DisplayName = 0x000000200,
        /// <summary>get type name</summary>
        TypeName = 0x000000400,
        /// <summary>get attributes</summary>
        Attributes = 0x000000800,
        /// <summary>get icon location</summary>
        IconLocation = 0x000001000,
        /// <summary>return exe type</summary>
        ExeType = 0x000002000,
        /// <summary>get system icon index</summary>
        SysIconIndex = 0x000004000,
        /// <summary>put a link overlay on icon</summary>
        LinkOverlay = 0x000008000,
        /// <summary>show icon in selected state</summary>
        Selected = 0x000010000,
        /// <summary>get only specified attributes</summary>
        Attr_Specified = 0x000020000,
        /// <summary>get large icon</summary>
        LargeIcon = 0x000000000,
        /// <summary>get small icon</summary>
        SmallIcon = 0x000000001,
        /// <summary>get open icon</summary>
        OpenIcon = 0x000000002,
        /// <summary>get shell size icon</summary>
        ShellIconSize = 0x000000004,
        /// <summary>pszPath is a pidl</summary>
        PIDL = 0x000000008,
        /// <summary>use passed dwFileAttribute</summary>
        UseFileAttributes = 0x000000010,
        /// <summary>apply the appropriate overlays</summary>
        AddOverlays = 0x000000020,
        /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
        OverlayIndex = 0x000000040,
    }

    /// <summary>
    /// Get the associated Icon for a file or application, this method always returns
    /// an icon.  If the strPath is invalid or there is no idonc the default icon is returned
    /// </summary>
    /// <param name="strPath">full path to the file</param>
    /// <param name="bSmall">if true, the 16x16 icon is returned otherwise the 32x32</param>
    /// <returns></returns>
    public static System.Drawing.Icon GetIcon(string strPath, bool bSmall)
    {
        SHFILEINFO info = new SHFILEINFO(true);
        int cbFileInfo = Marshal.SizeOf(info);
        SHGFI flags;
        if (bSmall)
            flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
        else
            flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;

        SHGetFileInfo(strPath, 256, out info, (uint)cbFileInfo, flags);
        return System.Drawing.Icon.FromHandle(info.hIcon);
    }

    /// <summary>
    /// Returns extracted icon as Texture2D Object
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bSmall"></param>
    /// <returns></returns>
    public static Texture2D GetTextureFromIconatPath(string path, bool bSmall = false)
    {
        System.Drawing.Icon icon = GetIcon(path, bSmall);
        System.Drawing.Bitmap bitmap = icon.ToBitmap();
        bitmap.MakeTransparent();

        Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.ARGB32, false);
        texture.LoadRawTextureData(Bitmap2RawBytes(bitmap));
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Converts Bitmap data to raw bytes to be used with Textures
    /// </summary>
    /// <param name="bmp"></param>
    /// <returns></returns>
    private static byte[] Bitmap2RawBytes(System.Drawing.Bitmap bmp)
    {
        bytes = new byte[bmp.Width * bmp.Height * 4];
        copyToBytes = new byte[bmp.Width * bmp.Height * 4];

        bmp.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipY);

        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
        bitmapData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
        Iptr = bitmapData.Scan0;
        Marshal.Copy(Iptr, bytes, 0, bytes.Length);

        for (int i = 0; i < bytes.Length; i++)
        {
            copyToBytes[bytes.Length - 1 - i] = bytes[i];
        }
        bmp.UnlockBits(bitmapData);

        return copyToBytes;
    }
}
