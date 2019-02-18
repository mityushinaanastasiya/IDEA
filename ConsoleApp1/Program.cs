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
        static BitArray key = new BitArray(128);
        static List<ushort> subKeys = new List<ushort>();
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
        static void generationKeys()
        {
            //случайная генерация ключа
            Random random = new Random();
            for (int i = 0; i < 128; i++)
            {
                key[i] = random.Next(2) == 0 ? true : false;
            }


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
            }
            //пока не 52 ключа выполняй
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
                    //если это 52 подключ, то выход
                    if (subKeys.Count == 52) break;
                }
            }
        }

        //побитовое исключающее или
        static void XOR(ushort A, ushort B)
        {

        }

        //Умножение по модулю 2^16 + 1 то есть 65 537
        static void multiplication (ushort A, ushort B)
        {

        }

        //Сложение по модулю 2^16 то есть 65 536
        static void addition (ushort A, ushort B)
        {

        }

        //шифратор
        static void encryptingIDEA ()
        {

        }

        //дешифратор
        static void decryptingIDEA ()
        {

        }
        static void Main(string[] args)
        {
            string textHP = @".\HP.txt"; ;
            FileStream fstream = new FileStream(textHP, FileMode.Open);  
            generationKeys();
             
            //считывание из файла по 64 бита и дробление на блоки по 16 бит 
            while (fstream.CanRead)
            {
                byte[] array = new byte[8];
                fstream.Read(array, 0, 8);
                ulong I = 0;
                I = array[0];
                for (int i = 1; i < 8; i++)
                {
                    I = I << 8;
                    I = I |= array[i];
                }
                ushort A = Convert.ToUInt16((I & 0xFFFF000000000000) >> 48);
                ushort B = Convert.ToUInt16((I & 0x0000FFFF00000000) >> 32);
                ushort C = Convert.ToUInt16((I & 0x00000000FFFF0000) >> 16);
                ushort D = Convert.ToUInt16(I & 0x000000000000FFFF);

            }
        }
    }
}
