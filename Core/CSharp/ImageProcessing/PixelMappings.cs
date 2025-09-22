using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using System;
using System.Collections.Generic;
using Snippets.Core.Modelling;

namespace Snippets.Core.ImageProcessing
{

    [DataContract]
    public class PixelMappings<TValue>
    {
        public static Dictionary<byte, Dictionary<byte, Dictionary<byte, Dictionary<byte, TValue>>>> DictionaryFrom(Tuple<TValue, RGBABytes>[] valueRGBATrueColourPairs) {
            Dictionary<byte, Dictionary<byte, Dictionary<byte, Dictionary<byte, TValue>>>> mapRToMapGToMapBToMapAToValue= 
                valueRGBATrueColourPairs.GroupBy(valueRGBATrueColourPair=> valueRGBATrueColourPair.Item2.R)
                    .ToDictionary(groupedByRs=> groupedByRs.First().Item2.R, 
                        groupedByRs => groupedByRs.GroupBy(groupedByR=> groupedByR.Item2.G)
                            .ToDictionary(groupedByRGs=> groupedByRGs.First().Item2.G, 
                                groupedByRGs => groupedByRGs.GroupBy(groupedByRG=>groupedByRG.Item2.B)
                                    .ToDictionary(groupedByRGBs=> groupedByRGBs.First().Item2.B, 
                                        groupedByRGBs => groupedByRGBs
                                        .ToDictionary(groupedByRGB=>groupedByRGB.Item2.A, 
                                            groupedByRGB=>groupedByRGB.Item1)
            )));
            return mapRToMapGToMapBToMapAToValue;
        }
    }
}
