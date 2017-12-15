﻿using System;
using System.Collections.Generic;
using lab5.LexicalAnalysis;
using lab5.SyntacticAnalyzer;

namespace lab5
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            try
            {
                LexicalAnalyzer lexicalAnalyzer = new LexicalAnalyzer();
                var pif = lexicalAnalyzer.ComputePIF();
                LL1_Parser parser = new LL1_Parser();
                parser.Table.PrintTable();
                Console.WriteLine("Grammar is of type LL(1)\n\n");
                /*
                Console.WriteLine("Please enter a sequence:");
                var sequence = Console.ReadLine();
                parser.ParseSequence(sequence);
                */
                parser.Parse(pif);
                Console.WriteLine("\n\nDo you want to see the derivations string?y/n");
                if (Console.ReadLine().ToLower() == "y")
                    parser.PrintDerivationString();
                
            }catch(ArgumentException)
            {
                Console.WriteLine("\n********************************\nCONFLICT! GRAMMAR IS NOT LL(1)!\n********************************\n");
            }
        }
    }
}