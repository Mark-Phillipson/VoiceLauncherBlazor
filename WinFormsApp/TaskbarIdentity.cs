using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace WinFormsApp;

/// <summary>
/// Assigns a per-window AppUserModelID so Windows gives independent tool windows
/// their own taskbar identity instead of grouping them under the executable.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class TaskbarIdentity
{
    private static readonly Guid PropertyStoreInterfaceId =
        new("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");

    private static readonly PropertyKey AppUserModelIdKey = new(
        new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"),
        5);

    public static void SetWindowAppUserModelId(IntPtr windowHandle, string appUserModelId)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("A valid window handle is required.", nameof(windowHandle));

        ArgumentException.ThrowIfNullOrWhiteSpace(appUserModelId);

        IPropertyStore? propertyStore = null;
        IntPtr valueString = IntPtr.Zero;

        try
        {
            var iid = PropertyStoreInterfaceId;
            Marshal.ThrowExceptionForHR(
                SHGetPropertyStoreForWindow(windowHandle, ref iid, out propertyStore));

            valueString = Marshal.StringToCoTaskMemUni(appUserModelId);
            var value = new PropVariant
            {
                VariantType = 31, // VT_LPWSTR
                PointerValue = valueString
            };

            var key = AppUserModelIdKey;
            Marshal.ThrowExceptionForHR(propertyStore.SetValue(ref key, ref value));
            Marshal.ThrowExceptionForHR(propertyStore.Commit());
        }
        finally
        {
            if (valueString != IntPtr.Zero)
                Marshal.FreeCoTaskMem(valueString);

            if (propertyStore != null && Marshal.IsComObject(propertyStore))
                Marshal.ReleaseComObject(propertyStore);
        }
    }

    [DllImport("shell32.dll", PreserveSig = true)]
    private static extern int SHGetPropertyStoreForWindow(
        IntPtr windowHandle,
        ref Guid interfaceId,
        [MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore);

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint propertyCount);
        [PreserveSig] int GetAt(uint propertyIndex, out PropertyKey key);
        [PreserveSig] int GetValue(ref PropertyKey key, out PropVariant value);
        [PreserveSig] int SetValue(ref PropertyKey key, ref PropVariant value);
        [PreserveSig] int Commit();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PropertyKey
    {
        public Guid FormatId;
        public uint PropertyId;

        public PropertyKey(Guid formatId, uint propertyId)
        {
            FormatId = formatId;
            PropertyId = propertyId;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PropVariant
    {
        public ushort VariantType;
        private ushort _reserved1;
        private ushort _reserved2;
        private ushort _reserved3;
        public IntPtr PointerValue;
    }
}
