using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MacroscopTestWork.Services
{
    public interface IImageDownloadService
    {
        Task<ImageSource?> DownloadAsync(string url, CancellationToken cancellationToken);
    }
}
