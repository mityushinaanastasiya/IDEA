using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class ModuleOperations
    {
        /// <summary>
        /// побитовое исключающее или
        /// </summary>
        /// <param name="A">Первый член</param>
        /// <param name="B">Второй член</param>
        /// <returns></returns>
        public ushort XOR(ushort A, ushort B)
        {
            uint result32;
            uint A32 = Convert.ToUInt32(A);
            uint B32 = Convert.ToUInt32(B);
            result32 = A32 ^ B32;
            return Convert.ToUInt16(result32);
        }
        /// <summary>
        /// Умножение по модулю 2^16 + 1 то есть 65 537
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public ushort multiplication(ushort A, ushort B)
        {
            if (A == 0 && B == 0) return 1;
            ulong Aulong = (A == 0) ? 65536 : Convert.ToUInt64(A);
            ulong Bulong = (B == 0) ? 65536 : Convert.ToUInt64(B);
            ulong mult = (Aulong * Bulong) % 65537;
            if (mult == 65536) return 0;
            return Convert.ToUInt16(mult);
        }

        /// <summary>
        /// Мультипликативная инверсия по модулю 2^16 + 1 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public ushort multiplicationInversion(ushort A)
        {
            long a = 65537, b = A, x1 = 1, x2 = 0, x;
            while (b != 1)
            {
                x = x2 - ((a / b) * x1);
                x2 = x1;
                x1 = x;
                x = a % b;
                a = b;
                b = x;
            }
            long res = (x1 + 65537) % 65537;
            int prov = multiplication(Convert.ToUInt16(res), A) % 65537; //проверка
            return Convert.ToUInt16(res);
        }
        /// <summary>
        /// Сложение по модулю 2^16 то есть 65 536
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>

        public ushort addition(ushort A, ushort B)
        {
            uint summ = Convert.ToUInt32(A + B);
            ushort result = Convert.ToUInt16(summ % 65536);
            return result;
        }

        /// <summary>
        /// Аддитивная инверсия
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public ushort additiveInversion(ushort A)
        {
            if (A == 0) return 0;
            else return Convert.ToUInt16(65536 - A);
        }

    }
}
