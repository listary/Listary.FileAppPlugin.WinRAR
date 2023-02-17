using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listary.FileAppPlugin.WinRAR.ExtractDialog
{
    public class ExtractDialogPlugin : IFileAppPlugin
    {
        private IFileAppPluginHost _host;

        public bool IsOpenedFolderProvider => false;
        
        public bool IsQuickSwitchTarget => true;
        
        public bool IsSharedAcrossApplications => false;

        public SearchBarType SearchBarType => SearchBarType.Fixed;
        
        public async Task<bool> Initialize(IFileAppPluginHost host)
        {
            _host = host;
            return true;
        }

        public IFileWindow BindFileWindow(IntPtr hWnd)
        {
            // It is a Win32 dialog box?
            if (Win32Utils.GetClassName(hWnd) == "#32770")
            {
                // It is from WinRAR?
                if (Win32Utils.GetProcessPathFromHwnd(hWnd).EndsWith("\\WinRAR.exe", StringComparison.OrdinalIgnoreCase))
                {
                    // It is WinRAR's extract dialog?
                    IntPtr tab = Win32Utils.GetDlgItem(hWnd, 0x3020);
                    if (Win32Utils.GetClassName(tab) == "SysTabControl32")
                    {
                        return new ExtractDialogWindow(_host, hWnd);
                    }
                }
            }
            return null;
        }
    }
}
