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
    class XMLFile
    {
        public void SaveStreamXml(string filename, SerializerXML serialezerXML)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializerXML), "Serialization");

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, serialezerXML);

                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    stream.WriteTo(fs);
                    fs.Flush();
                }
            }
        }


        public void OpenStreamXml(string filename, SerializerXML serialezerXML)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializerXML), "Serialization");
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    try
                    {
                        serialezerXML = (SerializerXML)serializer.Deserialize(fs);         // сделать через try/catch()
                        fs.Flush();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                        fs.Flush();
                        throw;
                    }
                }
            }
        }

        public void OpenFileXML(ref string filename)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "EA Files *.eac|*.eac";
            if (dlg.ShowDialog() == true)
            {
                filename = dlg.FileName;
            }
        }

        public void SaveFileXML(ref string filename)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "EA Files *.eac|*.eac";

            if (dlg.ShowDialog() == true)
            {
                filename = dlg.FileName;
            }
        }
    }
}
