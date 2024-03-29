using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Link_layer
{
    public class Frame : IEnumerable<Value>{

        public Frame(){            
            bytes = new List<byte>();
            bitCount = 0;
        }

        public Frame(string s){
            bytes = new List<byte>();
            bitCount = 0;
            AddBytes(s);
        }
        
        private List<byte> bytes;
        private byte byteActual;
        private int bitCount;

        public string SourceMAC => bytes[0].ToString("X") + bytes[1].ToString("X");
        public string DestinyMAC => bytes[2].ToString("X") + bytes[3].ToString("X");
        public int DataCount  => bytes[4];
        public int VerifCount => bytes[5];

        public string DataString => dataString();
        public byte[] Data => data();
        public string VerifString => verifString();
        public byte[] Verif => verif();

        private string dataString(){
            string data = "";
            for (int i = 6; i < DataCount + 6; i++){
                data += bytes[i];
            }
            return data;
        }

        private byte[] data(){
            byte[] data = new byte[DataCount];
            for (int i = 6; i < DataCount + 6; i++){
                data[i-6] = bytes[i];
            }
            return data;
        }

        private string verifString(){
            string verif = "";
            for (int i = 6 + DataCount ; i < VerifCount + DataCount + 6; i++){
                verif += bytes[i];
            }
            return verif;
        }

        private byte[] verif(){
            byte[] verif =  new byte[VerifCount];
            for (int i = 6 + DataCount ; i < VerifCount + DataCount + 6; i++){
                verif[i-6] = bytes[i];
            }
            return verif;
        }

        public void AddBit(Value value){
            byteActual += (byte)((byte)(value-1) * (byte)Math.Pow(2,7 - bitCount));
            bitCount++;
            if (bitCount == 8){
                bitCount = 0;
                bytes.Add(byteActual);
                byteActual = 0;
            }
        }

        public void AddByte(byte b, bool cleanByte = true){
            if (cleanByte){
                bitCount = 0;
                byteActual = 0;
            }
            bytes.Add(b);
        }
        public void AddByte(string b, bool cleanByte = true){
            if (cleanByte){
                bitCount = 0;
                byteActual = 0;
            }
            bytes.Add(Convert.ToByte(b,16));
        }

        public void AddBytes(byte[] bs, bool cleanByte =true){
            foreach(byte b in bs){
                AddByte(b,cleanByte);
            }
        }
        public void AddBytes(string s, bool cleanByte =true){
            for (int i = 0; i < s.Length; i+=2){
                AddByte(Convert.ToByte((s[i].ToString() + s[i+1].ToString()), 16));
            }
        }

        public bool IsComplete(){
            if (bytes.Count<=6){
                return false;
            }
            if (bytes.Count == 6 + DataCount + VerifCount){
                return true;
            }
            return false;
        }

        public void Clean(){
            bytes.Clear();
            byteActual = 0;
            bitCount = 0;
        }

        public override string ToString()
        {
            return "MAC source: "+ SourceMAC + "\nMAC destiny: " + DestinyMAC +
                    "Data count: " + DataCount + "\nData: " + DataString + 
                    "Verif count: " + VerifCount + "\nVerif: " + VerifString;
        }


        public string ToHex(){
            string frame = "";
            foreach (byte b in bytes)
            {
                if (b <= 16){
                    frame += "0";
                }
                frame += b.ToString("X");
            }
            return frame;
        }

        public IEnumerator<Value> GetEnumerator(){
            foreach (byte b in bytes){
                byte iterByte = b;
                byte pow = 128;
                for (int i = 0; i<8;i++){
                    if (iterByte >= pow){
                        iterByte -= pow;
                        yield return Value.ONE;
                    }else{
                        yield return Value.ZERO;
                    }
                    pow /= 2;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<byte> ListByte(){
            return new List<byte>(bytes);
        }

        public static bool Parity(byte b){
            byte iterByte = b;
            byte pow = 128;
            bool parity = true;
            for (int i = 0; i<8;i++){
                if (iterByte >= pow){
                    iterByte -= pow;
                    parity = !parity;
                }
                pow /= 2;
            }
            return parity;
        }

        public static bool Parity(byte[] bytes){
            bool parity = true;
            foreach(byte b in bytes){
                parity = Parity(parity, Parity(b));
            }
            return parity;
        }

        public static bool Parity(bool b1, bool b2){
            return !(b1 ^ b2);
        }

        public static bool[] HexToBinary(string s){
            return HexToBinary(StringToHex(s));
        }

        public static byte[] StringToHex(string s){
            byte[] bytes = new byte[s.Length/2];
            for (int i = 0; i < s.Length; i+=2){
                bytes[i/2] = Convert.ToByte((s[i].ToString() + s[i+1].ToString()), 16);
            }
            return bytes;
        }

        public static bool[] HexToBinary(byte[] bytes){
            bool[] binary = new bool[bytes.Length*8];
            int indexBinary = 0;
            foreach (byte b in bytes){
                byte iterByte = b;
                byte pow = 128;
                for (int i = 0; i<8;i++){
                    if (iterByte >= pow){
                        iterByte -= pow;
                        binary[indexBinary] = true;
                    }else{
                        binary[indexBinary] = false;
                    }
                    pow /= 2;
                }
            }
            return binary;
        }

        public static string BytesToHex(byte[] bytes)
        {
            string Hex = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                Hex += bytes[i].ToString("X");
            }
            return Hex;
        }
        public static string BinaryToHex(bool[] binary)
        {
            int dec = 0;
            for (int i = 0; i < binary.Length; i++)
            {
                if (binary[binary.Length - i - 1]) dec += (int)Math.Pow(2, i);
            }
            return dec.ToString("X");
        }

        public static string HexToStringBinary(byte[] bytes){
            string s = "";
            foreach(bool b in HexToBinary(bytes)){
                s += b ? "1" : "0";
            }
            return s;
        }
    }
}