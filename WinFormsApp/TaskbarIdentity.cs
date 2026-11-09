using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WinFormsApp;

/// <summary>
/// Assigns a per-window AppUserModelID and explicit relaunch icon so Windows
/// can resolve the correct icon for an independently grouped taskbar button.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class TaskbarIdentity
{
    private static readonly Guid PropertyStoreInterfaceId =
        new("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");

    private static readonly Guid AppUserModelFormatId =
        new("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3");

    private static readonly PropertyKey RelaunchIconResourceKey = new(AppUserModelFormatId, 3);
    private static readonly PropertyKey AppUserModelIdKey = new(AppUserModelFormatId, 5);

    public static void SetWindowAppUserModelId(IntPtr windowHandle, string appUserModelId, Icon icon, string iconFileName)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("A valid window handle is required.", nameof(windowHandle));

        ArgumentException.ThrowIfNullOrWhiteSpace(appUserModelId);
        ArgumentNullException.ThrowIfNull(icon);
        ArgumentException.ThrowIfNullOrWhiteSpace(iconFileName);

        var iconDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceLauncherBlazor",
            "TaskbarIcons");
        Directory.CreateDirectory(iconDirectory);

        var iconPath = Path.Combine(iconDirectory, iconFileName);
        using (var stream = new FileStream(iconPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            icon.Save(stream);

        IPropertyStore? propertyStore = null;

        try
        {
            var iid = PropertyStoreInterfaceId;
            Marshal.ThrowExceptionForHR(
                SHGetPropertyStoreForWindow(windowHandle, ref iid, out propertyStore));

            // Windows uses this property as the taskbar icon when an explicit
            // AppUserModelID is assigned and no matching pinned shortcut exists.
            SetStringProperty(propertyStore, RelaunchIconResourceKey, $"{iconPath},0");

            // Set the AppUserModelID last so the shell sees the icon metadata
            // when it is notified that this window has a new taskbar identity.
            SetStringProperty(propertyStore, AppUserModelIdKey, appUserModelId);
            Marshal.ThrowExceptionForHR(propertyStore.Commit());
        }
        finally
        {
            if (propertyStore != null && Marshal.IsComObject(propertyStore))
                Marshal.ReleaseComObject(propertyStore);
        }
    }

    private static void SetStringProperty(IPropertyStore propertyStore, PropertyKey key, string text)
    {
        var valueString = Marshal.StringToCoTaskMemUni(text);
        try
        {
            var value = new PropVariant
            {
                VariantType = 31, // VT_LPWSTR
                PointerValue = valueString
            };

            Marshal.ThrowExceptionForHR(propertyStore.SetValue(ref key, ref value));
        }
        finally
        {
            Marshal.FreeCoTaskMem(valueString);
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
