using System;
using System.Collections.Generic;

namespace lab5
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            try
            {
                LL1_Parser parser = new LL1_Parser();
                parser.Table.PrintTable();
                Console.WriteLine("Grammar is of type LL(1)\n\n");
                Console.WriteLine("Please enter a sequence:");
                var sequence = Console.ReadLine();
                parser.Parse(sequence);
            }catch(ArgumentException)
            {
                Console.WriteLine("\n********************************\nCONFLICT! GRAMMAR IS NOT LL(1)!\n********************************\n");
            }
        }
    }
}