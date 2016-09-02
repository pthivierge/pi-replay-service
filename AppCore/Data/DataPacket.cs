using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;

namespace PIReplay.Core.Data
{
    public class DataPacket
    {
       public List<AFValue> Data { get; set; }
       public bool IsBackFillData { get; set; }
    }
}
