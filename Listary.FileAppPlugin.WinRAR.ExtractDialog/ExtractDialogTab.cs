using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listary.FileAppPlugin.WinRAR.ExtractDialog
{
    public class ExtractDialogTab : IFileTab, IGetFolder, IOpenFolder
    {
        private IFileAppPluginHost _host;
        private IntPtr _pathEditor = IntPtr.Zero;
        private IntPtr _displayButton = IntPtr.Zero;

        public ExtractDialogTab(IFileAppPluginHost host, IntPtr dialog)
        {
            _host = host;

            IntPtr general = Win32Utils.FindWindowEx(dialog, IntPtr.Zero, "#32770", null);
            if (general == IntPtr.Zero)
            {
                _host.Logger.LogError("Failed to find General dialog");
                return;
            }

            _displayButton = Win32Utils.GetDlgItem(general, 0x66);
            if (_displayButton == IntPtr.Zero)
            {
                _host.Logger.LogWarning("Failed to find Display button");
            }

            IntPtr comboBox = Win32Utils.GetDlgItem(general, 0x65);
            if (comboBox == IntPtr.Zero)
            {
                _host.Logger.LogError("Failed to find the path combo box");
                return;
            }

            _pathEditor = Win32Utils.GetDlgItem(comboBox, 0x3E9);
            if (_pathEditor == IntPtr.Zero)
            {
                _host.Logger.LogError("Failed to find the path editor");
            }
        }

        public async Task<string> GetCurrentFolder()
        {
            if (_pathEditor != IntPtr.Zero)
            {
                return _host.GetWindowText(_pathEditor);
            }
            return string.Empty;
        }

        public async Task<bool> OpenFolder(string path)
        {
            if (_pathEditor != IntPtr.Zero)
            {
                if (_host.SetWindowText(_pathEditor, path))
                {
                    _host.PostMessage(_displayButton, Win32Utils.BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                    return true;
                }
                else
                {
                    _host.Logger.LogError("Failed to set the text of the path editor");
                }
            }
            return false;
        }
    }
}
