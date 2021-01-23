using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using DriveConverter.Drive;
using Gibbed.IO;

namespace DriveConverter.Files
{
    public class Binary
    {
        public class Parameter
        {
            public uint Key;
            public uint Type;
            public object Value;

            public Parameter(uint key, uint type, object value)
            {
                this.Key = key;
                this.Type = type;
                this.Value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
        }

        private Endian endian = Endian.Little;

        public List<Parameter> parameters = new List<Parameter>();

        public void Deserialize(Stream input)
        {
            ulong inputLength = (ulong) input.Length;

            ushort numberOfParameters = (ushort) (inputLength / 12f);

            for (int a = 0; a < numberOfParameters; a++)
            {
                uint key = input.ReadValueU32(endian);
                uint type = input.ReadValueU32(endian);

                object value = null;

                switch (type)
                {
                    case 1: // uint
                        value = input.ReadValueU32(endian);
                        break;
                    case 2: // float
                        value = input.ReadValueF32(endian);
                        break;
                    default:
                        throw new NotSupportedException("Unknown value type");
                }

                Parameter parameter = new Parameter(key, type, value);

                this.parameters.Add(parameter);
            }
        }

        public void Serialize(Stream output)
        {
            foreach (Parameter parameter in this.parameters)
            {
                uint key = parameter.Key;
                uint type = parameter.Type;
                object value = parameter.Value;

                output.WriteValueU32(key, endian);
                output.WriteValueU32(type, endian);

                switch (type)
                {
                    case 1: // uint
                        output.WriteValueU32(Convert.ToUInt32(value, CultureInfo.InvariantCulture), endian);
                        break;
                    case 2: // float
                        output.WriteValueF32(Convert.ToSingle(value, CultureInfo.InvariantCulture), endian);
                        break;
                    default:
                        throw new NotSupportedException("Unknown value type");
                }
            }
        }
    }
}
