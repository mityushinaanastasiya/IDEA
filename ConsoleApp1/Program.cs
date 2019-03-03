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
        //исходный ключ, который будет разбиваться 
        static BitArray key = new BitArray(128);
        //подключи для шифрования
        static List<ushort> subKeys = new List<ushort>();
        //подключи для дешифрования
        static List<ushort> subKeysDeshifr = new List<ushort>();

        //класс работы с операциями 
        static ModuleOperations moduleOperations = new ModuleOperations();
        //побитовый сдвиг на 25 бит  влево
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

        /// <summary>
        /// Генерация ключей для шифрования
        /// </summary>
        static void generationKeys()
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //НИЖЕ закоментирован код генерации ключа и субключа
            // ДЛЯ ОТЛАДКИ ИСПОЛЬЗУЮ СТАТИЧЕСКИЙ НАБОР СУБКЛЮЧЕЙ
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ////случайная генерация мсхдного ключа
            //Random random = new Random();
            //for (int i = 0; i < 128; i++)
            //{
            //    key[i] = random.Next(2) == 0 ? true : false;
            //}

            ////разбиваем 128-битный ключ на восемь 16-битных блоков
            ////128-битовый ключ IDEA определяет первые восемь субключей
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

            ////Последующие ключи (44) получаются путем последовательности 
            ///цик-лических сдвигов влево этого кода на 25 двоичных разрядов.
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

            //тестовая версия из его документика
            //страница 65
            ushort[] k = new ushort[52] { 55, 54, 53, 52, 51, 50, 49, 56,
                27648, 27136, 26624, 26112, 25600, 25088, 28672, 28160,
                208, 204, 200, 196, 224, 220, 216, 212, 38913, 36865,
                34817, 49153, 47105, 45057, 43009, 40961, 784, 896,
                880, 864, 848, 832, 816, 800, 6, 57350, 49158,
                40966, 32774, 24582, 16390, 8199, 3456, 3392, 3328, 3328 };

            for (int i = 0; i < 52; i++)
            {
                subKeys.Add(k[i]);
            }

        }

        /// <summary>
        /// Генерация ключей для дешифрования
        /// </summary>
        static void generationDeKeys()
        {
            //Первые четыре субключа расшифрования определяются несколько иначе, чем остальные.
            subKeysDeshifr.Add(moduleOperations.multiplicationInversion(subKeys[48]));
            subKeysDeshifr.Add(moduleOperations.additiveInversion(subKeys[49]));
            subKeysDeshifr.Add(moduleOperations.additiveInversion(subKeys[50]));
            subKeysDeshifr.Add(moduleOperations.multiplicationInversion(subKeys[51]));


            //Последующие операции производятся восемь раз 
            //с добавлением 6 к индексу ключей расшифрования 
            //и вычитанием 6 из индекса ключей шифрования
            for (int i = 0; i < 8; i++)
            {
                if (i == 7)
                {
                    subKeysDeshifr.Add(subKeys[46 - i * 6]);
                    subKeysDeshifr.Add(subKeys[47 - i * 6]);
                    subKeysDeshifr.Add(moduleOperations.multiplicationInversion(subKeys[42 - i * 6]));
                    subKeysDeshifr.Add(moduleOperations.additiveInversion(subKeys[43 - i * 6]));
                    subKeysDeshifr.Add(moduleOperations.additiveInversion(subKeys[44 - i * 6]));
                    subKeysDeshifr.Add(moduleOperations.multiplicationInversion(subKeys[45 - i * 6]));
                }
                else
                {
                    subKeysDeshifr.Add(subKeys[46 - i * 6]);
                    subKeysDeshifr.Add(subKeys[47 - i * 6]);
                    subKeysDeshifr.Add(moduleOperations.multiplicationInversion(subKeys[42 - i * 6]));
                    subKeysDeshifr.Add(moduleOperations.additiveInversion(subKeys[44 - i * 6]));
                    subKeysDeshifr.Add(moduleOperations.additiveInversion(subKeys[43 - i * 6]));
                    subKeysDeshifr.Add(moduleOperations.multiplicationInversion(subKeys[45 - i * 6]));
                }
            }
        }


        /// <summary>
        /// Шифратор
        /// </summary>
        static void encryptingIDEA()
        {

            
            string textHP = @".\HP.txt"; ;
            FileStream fstream = new FileStream(textHP, FileMode.Open);

            //проверка на кол-во байт 
            //необходимо чтобы входящий поток байтов (lenght)
            //был кратен 64
            int remainder = Convert.ToInt32(fstream.Length % 8); // остаток от деления  
            int additionaByte=0; //сколько байтов будет добавлено в конец файла
            //если у нас есть остаток, то дописываем байты необходимые для кратности 
            if (remainder != 0)
            {
                fstream.Position = fstream.Length;
                additionaByte = 8 - remainder;
                for (int i = 0; i < additionaByte; i++)
                {
                    fstream.WriteByte(10);
                }
            }
            //встаем в начало файла
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


                // применяем к считанным блокам шифрацию 
                // согласно его примеру 
                ushort T1, T2;
                for (int i = 0; i < 8; i++)
                {
                    A = moduleOperations.multiplication(A, subKeys[i * 6]);
                    B = moduleOperations.addition(B, subKeys[i * 6 + 1]);
                    C = moduleOperations.addition(C, subKeys[i * 6 + 2]);
                    D = moduleOperations.multiplication(D, subKeys[i * 6 + 3]);

                    T1 = moduleOperations.XOR(A, C);
                    T2 = moduleOperations.XOR(B, D);
                    T1 = moduleOperations.multiplication(T1, subKeys[i * 6 + 4]);
                    T2 = moduleOperations.addition(T1, T2);
                    T2 = moduleOperations.multiplication(T2, subKeys[i * 6 + 5]);
                    T1 = moduleOperations.addition(T1, T2);

                    A = moduleOperations.XOR(A, T2);
                    B = moduleOperations.XOR(B, T1);
                    C = moduleOperations.XOR(C, T2);
                    D = moduleOperations.XOR(D, T1);

                    //смена местамми В и С во всех раундах кроме псоледнего
                    if (i != 7)
                    {
                        ushort buf = B;
                        B = C;
                        C = buf;
                    }
                }

                //дополнительные преобразования, 9ый раунд алгоритма
                A = moduleOperations.multiplication(A, subKeys[48]);
                B = moduleOperations.addition(B, subKeys[49]);
                C = moduleOperations.addition(C, subKeys[50]);
                D = moduleOperations.multiplication(D, subKeys[51]);


                //запись в файл 
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


        /// <summary>
        /// Дешифратор
        /// </summary>
        static void decryptingIDEA()
        {

            string codeIDE = @".\code.IDE"; ;
            FileStream fstream = new FileStream(codeIDE, FileMode.Open);

            //считываем из конца файла, сколько байтов надо будет потом удалить
            fstream.Position  = fstream.Length - 1;
            int countDeletByte = fstream.ReadByte();
            
            // в начало файла
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
                    A = moduleOperations.multiplication(A, subKeysDeshifr[i * 6]);
                    B = moduleOperations.addition(B, subKeysDeshifr[i * 6 + 1]);
                    C = moduleOperations.addition(C, subKeysDeshifr[i * 6 + 2]);
                    D = moduleOperations.multiplication(D, subKeysDeshifr[i * 6 + 3]);

                    T1 = moduleOperations.XOR(A, C);
                    T2 = moduleOperations.XOR(B, D);
                    T1 = moduleOperations.multiplication(T1, subKeysDeshifr[i * 6 + 4]);
                    T2 = moduleOperations.addition(T1, T2);
                    T2 = moduleOperations.multiplication(T2, subKeysDeshifr[i * 6 + 5]);
                    T1 = moduleOperations.addition(T1, T2);

                    A = moduleOperations.XOR(A, T2);
                    B = moduleOperations.XOR(B, T1);
                    C = moduleOperations.XOR(C, T2);
                    D = moduleOperations.XOR(D, T1);

                    //смена местамми В и С 
                    if (i != 7)
                    {
                        ushort buf = B;
                        B = C;
                        C = buf;
                    }
                }

                //дополнительные преобразования, 9ый раунд 
                A = moduleOperations.multiplication(A, subKeysDeshifr[48]);
                B = moduleOperations.addition(B, subKeysDeshifr[49]);
                C = moduleOperations.addition(C, subKeysDeshifr[50]);
                D = moduleOperations.multiplication(D, subKeysDeshifr[51]);


                //
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

            //манипуляции с файлами чтобы "удалить" лишнее
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