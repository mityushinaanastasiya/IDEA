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
        static List<ushort> subKeysDeshifr = new List<ushort>();
        static void shiftKeyToLeft25(BitArray key)
        {
            bool buf;
            for (int i = 0; i < 25; i++)
            {
                buf = key[0];
                for (int j = 0; j < 127; j++)
                {
                    key[j] = key[j + 1];
                }
                key[127] = buf;
            }
        }
        static void generationKeys()
        {
            ////случайная генерация ключа
            //Random random = new Random();
            //for (int i = 0; i < 128; i++)
            //{
            //    key[i] = random.Next(2) == 0 ? true : false;
            //}
            ////разбиваем 128-битный ключ на восемь 16-битных блоков
            //for (int j = 0; j < 8; j++) 
            //{
            //    BitArray oneBlock = new BitArray(16);
            //    //считываем 16 бит в один блок
            //    for (int l = 0; l < 16; l++)
            //    {
            //        oneBlock[l] = key[j * 16 + l];
            //    }
            //    int[] buf = new int[1];
            //    oneBlock.CopyTo(buf, 0); // получила в инт 32 
            //    subKeys.Add(Convert.ToUInt16(buf[0])); 
            //}
            ////пока не 52 ключа выполняй
            //while (subKeys.Count != 52)
            //{
            //    //сдвигаем на 25 бит влево
            //    shiftKeyToLeft25(key);
            //    //разбиваем 128-битный ключ на восемь 16-битных блоков
            //    for (int j = 0; j < 8; j++)
            //    {
            //        BitArray oneBlock = new BitArray(16);
            //        //считываем 16 бит в один блок
            //        for (int l = 0; l < 16; l++)
            //        {
            //            oneBlock[l] = key[j * 16 + l];
            //        }
            //        int[] buf = new int[1];
            //        oneBlock.CopyTo(buf, 0); // получила в инт 32 
            //        subKeys.Add(Convert.ToUInt16(buf[0]));
            //        //если это 52 подключ, то выход
            //        if (subKeys.Count == 52) break;
            //    }
            //}

            while (subKeys.Count != 52)
            {
                subKeys.Add(18);
            }
        }
        static void generationDeKeys()
        {
            subKeysDeshifr.Add(multiplicationInversion(subKeys[48]));
            subKeysDeshifr.Add(additiveInversion(subKeys[49]));
            subKeysDeshifr.Add(additiveInversion(subKeys[50]));
            subKeysDeshifr.Add(multiplicationInversion(subKeys[51]));

            for (int i = 0; i < 8; i++)
            {
                subKeysDeshifr.Add(subKeys[46 - i * 6]);
                subKeysDeshifr.Add(subKeys[47 - i * 6]);
                subKeysDeshifr.Add(multiplicationInversion(subKeys[42 - i * 6]));
                subKeysDeshifr.Add(additiveInversion(subKeys[44 - i * 6]));
                subKeysDeshifr.Add(additiveInversion(subKeys[43 - i * 6]));
                subKeysDeshifr.Add(multiplicationInversion(subKeys[45 - i * 6]));
            }
        }

        //побитовое исключающее или
        static ushort XOR(ushort A, ushort B)
        {
            uint result32;
            uint A32 = Convert.ToUInt32(A);
            uint B32 = Convert.ToUInt32(B);
            result32 = A32 ^ B32;
            return Convert.ToUInt16(result32);
        }
        //Умножение по модулю 2^16 + 1 то есть 65 537
        static ushort multiplication(ushort A, ushort B)
        {
            if (A == 0 && B == 0) return 0;
            ulong Aulong = (A == 0) ? 65536 : Convert.ToUInt64(A);
            ulong Bulong = (B == 0) ? 65536 : Convert.ToUInt64(B);
            ulong mult = (Aulong * Bulong) % 65537;
            if (mult == 65536) return 0;
            return Convert.ToUInt16(mult);
        }
        //Мультипликативная инверсия по модулю 2^16 + 1 
        static ushort multiplicationInversion(ushort A)
        {
            //uint q, r, t, r1 = 65537, r2 = A, t1 = 0, t2 = 1;
            //    uint res = 0;
            //    while (r2 > 0)
            //    {
            //        q = r1 / r2;


            //        r = r1 - q * r2;
            //        r1 = r2;
            //        r2 = r;

            //        t = t1 - q * t2;
            //        t1 = t2;
            //        t2 = r;
            //    }
            //    if (r1 == 1) res = t1;
            //    uint prov = (res * A) % 65537;
            //}

            uint a = 65537, b = A, x1 = 1, x2 = 0, x;
            while (b != 1)
            {
                x = x2 - ((a / b) * x1);
                x2 = x1;
                x1 = x;
                x = a % b;
                a = b;
                b = x;
            }
            uint res = x1 % 65537;

            uint prov = (res * A) % 65537;
            return Convert.ToUInt16(res);
        }
        //Аддитивная инверсия
        static ushort additiveInversion(ushort A)
        {
            if (A == 0) return 0;
            else return Convert.ToUInt16(65536 - A);
        }

        //Сложение по модулю 2^16 то есть 65 536
        static ushort addition(ushort A, ushort B)
        {
            uint summ = Convert.ToUInt32(A + B);
            ushort result = Convert.ToUInt16(summ % 65536);
            return result;
        }

        //шифратор
        static void encryptingIDEA()
        {

            //добавление 
            string textHP = @".\HP.txt"; ;
            FileStream fstream = new FileStream(textHP, FileMode.Open);
            //проверка на кол-во байт 
            int remainder = Convert.ToInt32(fstream.Length % 8); // остаток байтов 
            int additionaByte=0; //добавлено байтов в конце
            if (remainder != 0)
            {
                fstream.Position = fstream.Length;
                additionaByte = 8 - remainder;
                for (int i = 0; i < additionaByte; i++)
                {
                    fstream.WriteByte(10);
                }
            }
            fstream.Position = 0;

            FileStream FSWrite = new FileStream(@".\code.IDE", FileMode.Create);
            //считывание из файла по 64 бита и дробление на блоки по 16 бит 
            while (fstream.Position != fstream.Length)
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

                ushort T1, T2;
                for (int i = 0; i < 8; i++)
                {
                    A = multiplication(A, subKeys[i * 6]);
                    B = addition(B, subKeys[i * 6 + 1]);
                    C = addition(C, subKeys[i * 6 + 2]);
                    D = multiplication(D, subKeys[i * 6 + 3]);

                    T1 = XOR(A, C);
                    T2 = XOR(B, D);
                    T1 = multiplication(T1, subKeys[i * 6 + 4]);
                    T2 = addition(T1, T2);
                    T2 = multiplication(T2, subKeys[i * 6 + 5]);
                    T1 = addition(T1, T2);

                    A = XOR(A, T2);
                    B = XOR(B, T1);
                    C = XOR(C, T2);
                    D = XOR(D, T1);

                    //смена местамми В и С 
                    if (i != 7)
                    {
                        ushort buf = B;
                        B = C;
                        C = buf;
                    }
                }

                //дополнительные преобразования, 9ый раунд алгоритма
                A = multiplication(A, subKeys[48]);
                B = addition(B, subKeys[49]);
                C = addition(C, subKeys[50]);
                D = multiplication(D, subKeys[51]);

                byte[] result = new byte[8];
                result[0] = Convert.ToByte((A & 0xFF00) >> 8);
                result[1] = Convert.ToByte(A & 0x00FF);
                result[2] = Convert.ToByte((B & 0xFF00) >> 8);
                result[3] = Convert.ToByte(B & 0x00FF);
                result[4] = Convert.ToByte((C & 0xFF00) >> 8);
                result[5] = Convert.ToByte(C & 0x00FF);
                result[6] = Convert.ToByte((D & 0xFF00) >> 8);
                result[7] = Convert.ToByte(D & 0x00FF);

                FSWrite.Write(result, 0, 8);
            }
            FSWrite.WriteByte(Convert.ToByte (additionaByte));
            FSWrite.Close();
            fstream.Close();
        }

        //дешифратор
        static void decryptingIDEA()
        {
            //добавление 
            string codeIDE = @".\code.IDE"; ;
            FileStream fstream = new FileStream(codeIDE, FileMode.Open);

            //считываем сколько байтов надо будет потом удалить
            fstream.Position  = fstream.Length - 1;
            int countDeletByte = fstream.ReadByte();
            
            fstream.Position = 0;

            FileStream FSWrite = new FileStream(@".\decode1.IDE", FileMode.Create);
            //считывание из файла по 64 бита и дробление на блоки по 16 бит 
            while (fstream.Position != fstream.Length-1)
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

                ushort T1, T2;
                for (int i = 0; i < 8; i++)
                {
                    A = multiplication(A, subKeysDeshifr[i * 6]);
                    B = addition(B, subKeysDeshifr[i * 6 + 1]);
                    C = addition(C, subKeysDeshifr[i * 6 + 2]);
                    D = multiplication(D, subKeysDeshifr[i * 6 + 3]);

                    T1 = XOR(A, C);
                    T2 = XOR(B, D);
                    T1 = multiplication(T1, subKeysDeshifr[i * 6 + 4]);
                    T2 = addition(T1, T2);
                    T2 = multiplication(T2, subKeysDeshifr[i * 6 + 5]);
                    T1 = addition(T1, T2);

                    A = XOR(A, T2);
                    B = XOR(B, T1);
                    C = XOR(C, T2);
                    D = XOR(D, T1);

                    //смена местамми В и С 
                    if (i != 7)
                    {
                        ushort buf = B;
                        B = C;
                        C = buf;
                    }
                }

                //дополнительные преобразования, 9ый раунд алгоритма
                A = multiplication(A, subKeysDeshifr[48]);
                B = addition(B, subKeysDeshifr[49]);
                C = addition(C, subKeysDeshifr[50]);
                D = multiplication(D, subKeysDeshifr[51]);

                byte[] result = new byte[8];
                result[0] = Convert.ToByte((A & 0xFF00) >> 8);
                result[1] = Convert.ToByte(A & 0x00FF);
                result[2] = Convert.ToByte((B & 0xFF00) >> 8);
                result[3] = Convert.ToByte(B & 0x00FF);
                result[4] = Convert.ToByte((C & 0xFF00) >> 8);
                result[5] = Convert.ToByte(C & 0x00FF);
                result[6] = Convert.ToByte((D & 0xFF00) >> 8);
                result[7] = Convert.ToByte(D & 0x00FF);

                FSWrite.Write(result, 0, 8);
            }

            FileStream FSWrite1 = new FileStream(@".\decode.txt", FileMode.Create);
            int n = Convert.ToInt32(fstream.Length - 1 - countDeletByte); 
            byte[] arr = new byte[n];
            FSWrite.Close();
            FSWrite = new FileStream(@".\decode1.IDE", FileMode.Open);
            FSWrite.Read(arr, 0,  n);
            FSWrite1.Write(arr, 0, n);

            FSWrite.Close();
            FSWrite1.Close();
            fstream.Close();
            
        }
   
    
        static void Main(string[] args)
        {
            generationKeys();
            generationDeKeys();
            encryptingIDEA();
            decryptingIDEA();
        }
    }
}
