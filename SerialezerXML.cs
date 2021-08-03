using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace EAKompensator
{
    [Serializable]
    public class SerializerXML
    {
        public List<KompensatorDevice> LstsClass1 { get; set; }
       
        public SerializerXML()
        {
            LstsClass1 = new List<KompensatorDevice>();
        }
    }
}

