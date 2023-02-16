using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listary.FileAppPlugin.WinRAR.ExtractDialog
{
    public class ExtractDialogWindow : IFileWindow
    {
        private IFileAppPluginHost _host;

        public IntPtr Handle { get; }

        public ExtractDialogWindow(IFileAppPluginHost host, IntPtr hWnd)
        {
            Handle = hWnd;
            _host = host;
        }

        public async Task<IFileTab> GetCurrentTab()
        {
            return new ExtractDialogTab(_host, Handle);
        }
    }
}
