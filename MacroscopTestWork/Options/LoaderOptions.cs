using System.ComponentModel.DataAnnotations;

namespace MacroscopTestWork.Options
{
    public class LoaderOptions
    {
        [Range(1, 50)]
        public int DefaultSlotCount { get; set; } = 3;

        [Range(100, 8000)]
        public int MaxDecodeWidth { get; set; } = 1200;

        [Range(1, 300)]
        public int HttpTimeoutSeconds { get; set; } = 30;
    }
}
