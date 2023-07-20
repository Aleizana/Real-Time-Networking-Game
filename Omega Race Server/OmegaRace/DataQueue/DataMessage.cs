using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OmegaRace
{
    public class DataMessage
    {
        public int horzInput;
        public int vertInput;

        public virtual void Serialize(ref BinaryWriter writer)
        {
            writer.Write(horzInput);
            writer.Write(vertInput);
        }

        public static DataMessage Deserialize(ref BinaryReader reader)
        {
            DataMessage output = new DataMessage();
            output.horzInput = reader.ReadInt32();
            output.vertInput = reader.ReadInt32();

            return output;
        }

    }
}
