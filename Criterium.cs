using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link_layer
{

    public abstract class Criterium
    {
        public abstract Frame Encrypt(string source_mac, string destiny_mac, string _data_);
        //public Frame Decrypt(Frame encrypted_frame, out bool decrypted, out bool correct_frame, bool force_fix = true)
        //{
        //    Frame result = Decrypt_andTryToFixFrame(encrypted_frame, out decrypted, out correct_frame, force_fix);
        //    if(decrypted)
        //    {
        //        return encrypted_frame;
        //    }
        //    return null;
        //}
        public abstract Frame Decrypt_andTryToFixFrame(Frame encrypted_frame, out bool was_fixed, out bool correct_frame, bool force_fix = true);
    }
    public class TwoDimensionalParity : Criterium
    {
        public override Frame Encrypt(string source_mac, string destiny_mac, string _data_)
        {
            Frame encryptedData = new Frame();
            encryptedData.AddBytes(destiny_mac);
            encryptedData.AddBytes(source_mac);
            encryptedData.AddByte((byte)(_data_.Length / 2));                                           // Convirtiendo de hexadecimal a bytes
            byte verifCount = (byte)((12 + _data_.Length) / 2 / 8);
            verifCount++;
            if (((12 + _data_.Length) / 2) % 8 > 0)            
                verifCount++;            
            encryptedData.AddByte(verifCount);                                  // La cantidad de columnas(8) + la cantidad de filas(* 4 a bits /8 a filas).
            encryptedData.AddBytes(_data_);

            bool[] bin_frame = Frame.HexToBinary(encryptedData.ToHex());

            bool[] columns_verification = new bool[8];
            bool[] rows_verification = new bool[bin_frame.Length / 8];

            for (int i = 0; i < bin_frame.Length; i++)
            {
                if (!bin_frame[i]) continue;
                columns_verification[i % 8] ^= bin_frame[i];
                rows_verification[i / 8] ^= bin_frame[i];
            }
            encryptedData.AddBytes(Frame.BinaryToHex(columns_verification));
            Console.WriteLine(Frame.BinaryToHex(columns_verification));
            encryptedData.AddBytes(Frame.BinaryToHex(rows_verification));

            return encryptedData;
        }

        public override Frame Decrypt_andTryToFixFrame(Frame encrypted_frame, out bool was_fixed, out bool correct_frame, bool force_fix = true)
        {
            Frame result = new Frame();

            #region Separa la trama en "frame" y "verification_bits"
            byte[] frame = encrypted_frame.SimpleFrame;
            bool[] bin_frame = Frame.HexToBinary(frame);
            byte[] verification_bytes = encrypted_frame.Verif;
            bool[] verification_bits = Frame.HexToBinary(verification_bytes);
            #endregion

            #region Verifica las columnas y las filas con error de paridad
            List<int> columns_error = new List<int>();
            List<int> rows_error = new List<int>();

            for (int i = 0; i < 8; i++)
            {
                bool x = false;
                for (int j = 0; j < 6 + encrypted_frame.DataCount; j++)
                {
                    x ^= bin_frame[j * 8 + i];
                }
                if (x != verification_bits[i]) columns_error.Add(i);
            }

            int bitsSkip = 8 - (encrypted_frame.SimpleFrame.Length % 8);
            if (bitsSkip == 8)
                bitsSkip = 0;
            for (int k = 0; k < 6 + encrypted_frame.DataCount; k++)
            {
                bool x = false;
                for (int i = 0; i < 8; i++)
                {
                    x ^= bin_frame[k * 8 + i];
                }
                if (x != verification_bits[8 + bitsSkip + k]) rows_error.Add(k);
            }
            #endregion

            #region Si no hubo error, envia el mismo Frame que recibio
            if (columns_error.Count == 0 && rows_error.Count == 0)
            {
                was_fixed = false;
                correct_frame = true;
                return encrypted_frame;
            }
            #endregion
            else
            {
                correct_frame = false;
                if(!force_fix || (rows_error.Count > 1 && columns_error.Count > 1))
                {
                    was_fixed = false;
                    return encrypted_frame;
                }

                #region Arrelgla el error

                bool fix_column = columns_error.Count == 1;
                int length = fix_column ? rows_error.Count : columns_error.Count;

                for (int i = 0; i < length; i++)
                {
                    bool one = bin_frame[                  fix_column ?                     rows_error[i] * 8 + columns_error[0] :                          rows_error[0] * 8 + columns_error[i]];       // fix_column ? la fila correspondiente i con la columna prefijada : la columna i correspondiente con la fila ya fijada
                    frame[fix_column ? rows_error[i] * 8 : rows_error[0] * 8] += (byte)((one ? -1 : 1) * Math.Pow(2, fix_column ? columns_error[0] : columns_error[i]));
                }

                result.AddBytes(frame);
                #endregion

            }
            was_fixed = (columns_error.Count == 1 || rows_error.Count == 1);
            return result;
        }
    }
}
