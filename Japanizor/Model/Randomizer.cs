using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components.Web;

namespace Japanizor.Model
{
    public class Randomizer
    {
        public static void Randomize<T>(T[] items)
        {
            T temp;
            var rand = new Random();

            for (int i = 0; i < items.Length - 1; i++)
            {
                int j = rand.Next(i, items.Length);
                temp = items[i];
                items[i] = items[j];
                items[j] = temp;
            }
        }
    }
}
