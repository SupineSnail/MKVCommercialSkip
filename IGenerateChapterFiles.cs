using Bulletfish.MKVCommercialSkip.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulletfish.MKVCommercialSkip
{
    public interface IGenerateChapterFiles
    {
        string GetChapterFileContents(Dictionary<Argument, string> arguments, FileInfo file, TextWriter progressStream);
    }
}
