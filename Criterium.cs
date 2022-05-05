﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link_layer
{

    public abstract class Criterium
    {
        public abstract Frame Encrypt(string source_mac, string destiny_mac, string _data_);
        public Frame Decrypt(Frame encrypted_frame, out bool decrypted, out bool correct_frame, bool force_fix = true)
        {
            Frame result = Decrypt_andTryToFixFrame(encrypted_frame, out decrypted, out correct_frame, force_fix);
            if(decrypted)
            {
                return encrypted_frame;
            }
            return null;
        }
        protected abstract Frame Decrypt_andTryToFixFrame(Frame encrypted_frame, out bool was_fixed, out bool correct_frame, bool force_fix = true);
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

        protected override Frame Decrypt_andTryToFixFrame(Frame encrypted_frame, out bool was_fixed, out bool correct_frame, bool force_fix = true)
        {
            Frame result = new Frame();

            #region Separa la trama en "frame" y "verification_bits"
            byte[] frame = encrypted_frame.ListByte().ToArray();
            bool[] bin_frame = Frame.HexToBinary(frame);
            byte[] verification_bytes = encrypted_frame.Verif;
            bool[] verification_bits = Frame.HexToBinary(verification_bytes);
            #endregion

            #region Verifica las columnas y las filas con error de paridad
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
            #endregion

            #region Si no hubo error, envia el mismo Frame que recibio
            if (columns_error.Count == 0)
            {
                was_fixed = true;
                correct_frame = true;
                return encrypted_frame;
            }
            #endregion

            else
            {
                if(!force_fix)
                {
                    correct_frame = false;
                    was_fixed = false;
                    return encrypted_frame;
                }
                
                #region En caso que se requiera arreglarlo, lo hace

                for (int i = 0; i < columns_error.Count; i++)
                {
                    for (int j = 0; j < rows_error.Count; j++)
                    {
                        bool one = bin_frame[(i * 8) + j];

                        frame[i] += (byte)((one ? -1 : 1) * Math.Pow(2, j));
                    }
                }

                for (int i = 0; i < frame.Length; i++)
                {
                    result.AddBytes(frame);
                }
                #endregion

            }
            correct_frame = false;
            was_fixed = (columns_error.Count == 1 || rows_error.Count == 1);
            return result;
        }
    }
}
