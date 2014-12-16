using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DampingFinder
{
    static class ObjectManager
    {
        public static GraphFFT currentFftControl { get; set; }
        //public static GraphInverseFFT currentInverseFftControl { get; set; }
        public static WavFile CurrentFile { get; set; }

        //public static List<Frequency> FrequencysSelectedList { get; set; }
    }
}
