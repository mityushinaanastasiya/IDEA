using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ConsoleApp1
{
    class Program
    {
        static List<ushort> subKeys = new List<ushort>();
        //static ushort[] outputTransform = new ushort[4];
        static void shiftKeyToLeft25 (BitArray key)
        {
            bool buf;
            for (int i =0; i<25; i++)
            {
                buf = key[0];
                for ( int j=0; j<127; j++)
                {
                    key[j] = key[j + 1];
                }
                key[127] = buf;
            }
        }
        static void generationKeys(BitArray key)
        {
            //разбиваем 128-битный ключ на восемь 16-битных блоков
            for (int j = 0; j < 8; j++) 
            {
                BitArray oneBlock = new BitArray(16);
                //считываем 16 бит в один блок
                for (int l = 0; l < 16; l++)
                {
                    oneBlock[l] = key[j * 16 + l];
                }
                int[] buf = new int[1];
                oneBlock.CopyTo(buf, 0); // получила в инт 32 
                subKeys.Add(Convert.ToUInt16(buf[0])); 


                //if (Enumerable.Range(0, 6).Contains(j))
                //{
                //    subKeys[0, j] = Convert.ToUInt16(buf[0]);
                //    countSubKeys++;
                //}
                //else
                //{
                //    subKeys[1, j - 6] = Convert.ToUInt16(buf[0]);
                //    countSubKeys++;
                //}
            }
            
            while (subKeys.Count != 52)
            {
                //сдвигаем на 25 бит влево
                shiftKeyToLeft25(key);
                //разбиваем 128-битный ключ на восемь 16-битных блоков
                for (int j = 0; j < 8; j++)
                {
                    BitArray oneBlock = new BitArray(16);
                    //считываем 16 бит в один блок
                    for (int l = 0; l < 16; l++)
                    {
                        oneBlock[l] = key[j * 16 + l];
                    }
                    int[] buf = new int[1];
                    oneBlock.CopyTo(buf, 0); // получила в инт 32 
                    subKeys.Add(Convert.ToUInt16(buf[0]));
                    if (subKeys.Count == 52) break;
                }
            }

       
        }
        static void Main(string[] args)
        {
            string textHP = @".\HP.txt"; ;
            FileStream fstream = new FileStream(textHP, FileMode.OpenOrCreate);
            byte[] array = new byte[8];
            BitArray key = new BitArray(128);
            Random random = new Random();
            for (int i = 0; i < 128; i++)
            {
                key[i] = random.Next(2) == 0 ? true : false;
            }
            generationKeys(key);
            fstream.Read(array, 0, 8);
            Int64 I = 0;
            I = array[0];
            for (int i = 1; i < 8; i++)
            {
                I = I << 8;
                I = I |= array[i];
            }
            Int32 left = Convert.ToInt32(I >> 32);
            Int32 Right = Convert.ToInt32(I & 0x00000000FFFFFFFF);
        }
    }
}
