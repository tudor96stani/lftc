using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace lab5
{
    public class LL1_Parser
    {
        public Grammar G { get; set; } 
        public ParseTable Table { get; set; } 
        public Stack<string> Stack { get; set; }
        public List<string> Output { get; set; }
        public string Input { get; set; }
        public LL1_Parser()
        {
            try
            {
                G = new Grammar("Grammar.txt");
                Table = new ParseTable(G);
                PrintFirstAndFollow();
                Stack = new Stack<string>();
                Stack.Push("$");
                Stack.Push(G.StartSymbol);
                Output = new List<string>();
            }
            catch (ArgumentException ex)
            {
                throw ex;

            }

        }

        private void PrintFirstAndFollow()
        {
            Console.WriteLine("First(1) for each terminal:");
            foreach (var kvp in Table.Firsts.Where(x=>G.Nonterminals.Contains(x.Key)))
            {
                Console.Write(kvp.Key + ":");
                foreach (var e in kvp.Value)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("Follow(1) for each terminal:");
            foreach (var kvp in Table.Follows)
            {
                Console.Write(kvp.Key + ":");
                foreach (var e in kvp.Value)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();
            }
        }

        public void PrintCurrentConfiguration()
        {
            Console.Write("("+Input+",");
            foreach(var stackElem in Stack)
            {
                Console.Write(stackElem);
            }
            Console.Write(",");
            if (Output.Count == 0)
                Console.Write(Utils.EPSILON);
            else
            {
                foreach (var outputElem in Output)
                {
                    Console.Write(outputElem);
                }
            }
            Console.Write(")|-");
        }

        public void Parse(string sequence)
        {
           
            Input = sequence + "$";
            PrintCurrentConfiguration();
            bool working = true;
            while(working)
            {
                string tableEntry;
                var A = Stack.Peek();
                var a = Input[0];
                if(Table.Table.TryGetValue(A[0]+","+a,out tableEntry))
                {
                    //we have a value that is either a (Rule,number) pair, pop or acc
                    if(tableEntry=="acc")
                    {
                        working = false;
                        Console.Write("acc");
                        break;
                    }
                    else if(tableEntry=="pop")
                    {
                        Console.Write($"(pop {Input[0]})");
                        Input = Input.Substring(1);
                        Stack.Pop();
                        working = true;

                        PrintCurrentConfiguration();
                    }
                    else
                    {
                        Console.Write("(push)");
                        var elements = tableEntry.Split(',');
                        var alpha = elements[0];
                        var i = elements[1];
                        Stack.Pop();
                        var charArrayOfAlpha = alpha.ToCharArray();
                        Array.Reverse(charArrayOfAlpha);
                        foreach(var ch in new string(charArrayOfAlpha))
                            Stack.Push(ch.ToString());
                        Output.Add(i);
                        PrintCurrentConfiguration();
                    }
                }
                else
                {
                    //error
                    working = false;
                    Console.WriteLine($"\nERROR\nUnexpected token {a}.");
                    break;
                }
            }
        }
    }
}
