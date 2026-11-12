using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace CursorlessVisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuid)]
    public sealed class CursorlessPackage : AsyncPackage
    {
        public const string PackageGuid = "8c659f0b-6f7f-4852-9126-281c01e7cc90";
        private static readonly Guid CommandSet = new Guid("77014590-f211-4431-9581-9e7952ec42e2");
        private HatSettings settings = null!;
        private IVsEditorAdaptersFactoryService adapters = null!;
        private IVsTextManager textManager = null!;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var componentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel
                ?? throw new InvalidOperationException("The Visual Studio component model is unavailable.");
            settings = componentModel.GetService<HatSettings>();
            adapters = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            textManager = await GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager
                ?? throw new InvalidOperationException("The Visual Studio editor service is unavailable.");
            var commands = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService
                ?? throw new InvalidOperationException("The Visual Studio command service is unavailable.");
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var toggle = new OleMenuCommand((sender, args) => settings.Toggle(), new CommandID(CommandSet, 0x0100));
            toggle.BeforeQueryStatus += (sender, args) => toggle.Checked = settings.Enabled;
            commands.AddCommand(toggle);
            commands.AddCommand(new MenuCommand(SelectHat, new CommandID(CommandSet, 0x0101)));
            commands.AddCommand(new MenuCommand(ShowStatus, new CommandID(CommandSet, 0x0102)));
        }

        private HatViewManager? GetManager()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (ErrorHandler.Succeeded(textManager.GetActiveView(0, null, out var nativeView)) && nativeView != null)
            {
                IWpfTextView? view = adapters.GetWpfTextView(nativeView);
                if (view != null && view.Properties.TryGetProperty(typeof(HatViewManager), out HatViewManager manager))
                    return manager;
            }
            ShowMessage("Open a text document in the editor first.");
            return null;
        }

        private void SelectHat(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var manager = GetManager();
            if (manager == null) return;
            try
            {
                var revision = manager.CaptureRevision();
                new SelectHatDialog(mark => manager.Select(mark, revision)).ShowModal();
            }
            catch (InvalidOperationException exception)
            {
                ShowMessage(exception.Message);
            }
        }

        private void ShowStatus(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var manager = GetManager();
            if (manager != null) ShowMessage(manager.Status);
        }

        private void ShowMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VsShellUtilities.ShowMessageBox(this, message, "Cursorless prototype",
                OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
