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
        public Frame Decrypt(Frame encrypted_frame, out bool decrypted, bool force_fix = true)
        {
            Frame result = Decrypt_andTryToFixFrame(encrypted_frame, out decrypted, force_fix);
            if(decrypted)
            {
                return encrypted_frame;
            }
            return null;
        }
        protected abstract Frame Decrypt_andTryToFixFrame(Frame encrypted_frame, out bool was_fixed, bool force_fix = true);
    }
    public class TwoDimensionalParity : Criterium
    {
        public override Frame Encrypt(string source_mac, string destiny_mac, string _data_)
        {
            Frame encryptedData = new Frame();
            encryptedData.AddBytes(destiny_mac);
            encryptedData.AddBytes(source_mac);
            encryptedData.AddByte((byte)(_data_.Length / 2));                                           // Convirtiendo de hexadecimal a bytes
            encryptedData.AddByte((byte)(8 + (_data_.Length / 2)));                                  // La cantidad de columnas(8) + la cantidad de filas(* 4 a bits /8 a filas).
            encryptedData.AddBytes(_data_);

            bool[] bin_frame = Frame.HexToBinary(source_mac + destiny_mac +(_data_.Length / 2).ToString("X") + (8 + (_data_.Length / 2)).ToString("X") + _data_) ;

            bool[] columns_verification = new bool[8];
            bool[] rows_verification = new bool[bin_frame.Length / 8];

            for (int i = 0; i < bin_frame.Length; i++)
            {
                if (!bin_frame[i]) continue;
                columns_verification[i % 8] ^= bin_frame[i];
                rows_verification[i / 8] ^= bin_frame[i];
            }
            encryptedData.AddBytes(Frame.BinaryToHex(columns_verification));
            encryptedData.AddBytes(Frame.BinaryToHex(rows_verification));

            return encryptedData;
        }

        protected override Frame Decrypt_andTryToFixFrame(Frame encrypted_frame, out bool was_fixed, bool force_fix = true)
        {
            #region Separando la trama en "frame" y "verification_bits"
            byte[] frame = encrypted_frame.ListByte().ToArray();
            bool[] bin_frame = Frame.HexToBinary(frame);
            byte[] verification_bytes = encrypted_frame.Verif;
            bool[] verification_bits = Frame.HexToBinary(verification_bytes);



            List<int> columns_error = new List<int>();
            List<int> rows_error = new List<int>();

            for (int i = 0; i < 8; i++)
            {
                bool x = true;
                for (int j = 0; j < frame[4]; j++)
                {
                    x ^= bin_frame[j * 8 + i];
                }
                if (x != verification_bits[i]) columns_error.Add(i);
            }

            for (int k = 0; k < frame[4]; k++)
            {
                bool x = true;
                for (int i = 0; i < 8; i++)
                {
                    x ^= bin_frame[k * 8 + i];
                }
                if (x != verification_bits[8 + k]) rows_error.Add(k);
            }

        }
    }
}
