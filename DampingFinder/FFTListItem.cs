using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DampingFinder
{
    class FFTListItem
    {
        public bool NeedToShow { get; set; }
        public bool isManual { get; set; }
        public int Frequency { get; set; }
        public int WindowWidth { get; set; }
    }
}
