using System;
using System.Collections.Generic;
using System.Linq;

namespace lab5.SyntacticAnalyzer
{
    public class ParseTable
    {
        public Dictionary<string, string> Table = new Dictionary<string, string>();
        public Dictionary<string, HashSet<string>> Firsts;
        public Dictionary<string, HashSet<string>> Follows;
        public ParseTable(Grammar g)
        {
            Firsts = Utils.First(g);
            Follows = Utils.Follow(g, Firsts);
            Init(g);
        }


        /*
         //Init2 * Init3 probably don't fill the parsing table correctly
        private void Init3(Grammar g)
        {
            foreach (var A in g.Nonterminals)
            {

                foreach (var rule in g.ProductionRules.Where(x => x.Key.Left.Equals(A)))
                {
                    foreach (var a in g.Terminals)
                    {
                        try
                        {
                            if (!g.Terminals.Contains(rule.Key.Right[0]))
                            {
                                var firstOfAlpha = Utils.FirstOf(rule.Key.Right[0], Firsts, rule.Key);
                                if (!firstOfAlpha.Contains(Utils.EPSILON))
                                {
                                    //no epsilon
                                    if (firstOfAlpha.Contains(a))
                                        Table.Add(A + "," + a, String.Join("", rule.Key.Right) + "," + rule.Value);
                                }
                                else
                                {
                                    var followOfAlpha = Follows[A];
                                    if (followOfAlpha.Contains(a))
                                        Table.Add(A + "," + a, String.Join("", rule.Key.Right) + "," + rule.Value);
                                }
                            }
                            else
                            {
                                var firstOfAlpha = rule.Key.Right[0];
                                if (a == firstOfAlpha)
                                    Table.Add(A + "," + a, String.Join("", rule.Key.Right) + "," + rule.Value);
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            foreach (var a in g.Terminals)
            {
                foreach (var a2 in g.Terminals)
                {
                    if (a == a2)
                        Table.Add(a + "," + a, "pop");
                }
            }
            Table.Add("$,$", "acc");
        }

        private void Init2(Grammar g)
        {
            foreach(var rule in g.ProductionRules)
            {
                if (!g.Terminals.Contains(rule.Key.Right[0]))
                {
                    var firstOfAlpha = Utils.FirstOf(rule.Key.Right[0], Firsts, rule.Key);
                    if (!firstOfAlpha.Contains(Utils.EPSILON))
                    {
                        //no epsilon
                        foreach (var fi in firstOfAlpha)
                        {
                            Table.Add(rule.Key.Left + "," + fi, String.Join("", rule.Key.Right) + "," + rule.Value);
                        }
                    }
                    else
                    {
                        //epsilon
                        var foll = Follows[rule.Key.Left];
                        foreach (var fo in foll)
                        {
                            Table.Add(rule.Key.Left + "," + fo, String.Join("", rule.Key.Right) + "," + rule.Value);
                        }

                    }
                }
                else
                {
                    var firstOfAlpha = rule.Key.Right[0];
                    //if (a == firstOfAlpha)
                        Table.Add(rule.Key.Left + "," + firstOfAlpha, String.Join("", rule.Key.Right) + "," + rule.Value);
                }
            }
            foreach (var a in g.Terminals)
            {
                foreach (var a2 in g.Terminals)
                {
                    if (a == a2)
                        Table.Add(a + "," + a, "pop");
                }
            }
            Table.Add("$,$", "acc");
        }

        */

        private void Init(Grammar g)
        {

            foreach (var rule in g.ProductionRules)
            {
                var firstOfAlpha = Utils.FirstOf(rule.Key.Right[0], Firsts, rule.Key);
                foreach (var f in firstOfAlpha.Where(x => !x.Contains(Utils.EPSILON)))
                {
                    Table.Add(rule.Key.Left + "," + f, String.Join(" ", rule.Key.Right) + " " + rule.Value);
                }

                if (firstOfAlpha.Contains(Utils.EPSILON))
                {
                    var followOfLHS = Follows[rule.Key.Left];
                    foreach (var fo in followOfLHS)
                    {
                        Table.Add(rule.Key.Left + "," + fo, String.Join(" ", rule.Key.Right) + " " + rule.Value);
                    }
                }
            }

            foreach (var a in g.Terminals.Where(x => !x.Contains(Utils.EPSILON)))
            {
                Table.Add(a + "," + a, "pop");
            }
            Table.Add("$,$", "acc");
        }



        public void PrintTable()
        {
            Console.WriteLine("\nPARSING TABLE:");
            foreach (var kvp in Table)
            {
                Console.WriteLine("("+kvp.Key + ")=" + kvp.Value);
            }
        }
    }
}
