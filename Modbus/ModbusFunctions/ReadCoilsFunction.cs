using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters parameters = CommandParameters as ModbusReadCommandParameters;
            byte[] bytes = new byte[12];

            bytes[0] = BitConverter.GetBytes(parameters.TransactionId)[1];
            bytes[1] = BitConverter.GetBytes(parameters.TransactionId)[0];
            bytes[2] = BitConverter.GetBytes(parameters.ProtocolId)[1];
            bytes[3] = BitConverter.GetBytes(parameters.ProtocolId)[0];
            bytes[4] = BitConverter.GetBytes(parameters.Length)[1];
            bytes[5] = BitConverter.GetBytes(parameters.Length)[0];
            bytes[6] = parameters.UnitId;
            bytes[7] = parameters.FunctionCode;
            bytes[8] = BitConverter.GetBytes(parameters.StartAddress)[1];
            bytes[9] = BitConverter.GetBytes(parameters.StartAddress)[0];
            bytes[10] = BitConverter.GetBytes(parameters.Quantity)[1];
            bytes[11] = BitConverter.GetBytes(parameters.Quantity)[0];

            return bytes;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            var ret = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }
            else
            {
                int cnt = 0;
                ushort adress = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
                ushort value;
                byte mask = 1;
                for (int i = 0; i < response[8]; i++)
                {
                    byte tempByte = response[9 + i];
                    for (int j = 0; j < 8; j++)
                    {
                        value = (ushort)(tempByte & mask);
                        tempByte >>= 1;
                        ret.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, adress), value);
                        cnt++;
                        adress++;
                        if (cnt == ((ModbusReadCommandParameters)CommandParameters).Quantity)
                        {
                            break;
                        }
                    }
                }
            }
            return ret;
        }
    }
}