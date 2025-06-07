using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine.Helper
{
    public class FileHelper
    {
        public static string FromProjectRoot(string relativePath)
        {
            string basePath = AppContext.BaseDirectory;
            string projectRoot = Path.Combine(basePath, @"../../../");
            string path = Path.Combine(projectRoot, relativePath);

            return path;
        }
    }
}
