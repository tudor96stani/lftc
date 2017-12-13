using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lab5
{
    public static class Utils
    {
        public static string EPSILON = "ε";

        public static HashSet<string> PlusCircle(List<string> l1, List<string> l2)
        {

            if (l1.Count > 0 && !l1.Contains(EPSILON))
            {
                return new HashSet<string>(l1.Distinct().ToList());
            }
            var l1Set = l1.Where(x => x != EPSILON).Select(x => x[0].ToString()).Distinct().ToList();
            var l2Set = l2.Where(x => x != EPSILON).Select(x => x[0].ToString()).Distinct().ToList();
            if (l1.Contains(EPSILON) && l2.Contains(EPSILON))
                l1Set.Add(EPSILON);
            return new HashSet<string>(l1Set.Union(l2Set));
        }


        //compute the First table
        //The key of the dictionary is the NonTerminal
        //The value is the set of terminals
        public static Dictionary<string, HashSet<string>> First(Grammar g)
        {

            var fi = new Dictionary<string, HashSet<string>>();


            foreach (var a in g.Terminals)
            {
                fi.Add(a + 0.ToString(), new HashSet<string>() { a });
            }

            foreach (var A in g.Nonterminals)
            {
                bool added = false;
                if (g.ProductionRules.Any(x => x.Key.Left.Equals(A) && g.Terminals.Contains(x.Key.Right[0])))
                {
                    //we have the case A->aX
                    //F0(A)={a}
                    var rule = g.ProductionRules.FirstOrDefault(x => x.Key.Left.Equals(A) && g.Terminals.Contains(x.Key.Right[0]));
                    fi.Add(A + 0.ToString(), new HashSet<string>() { rule.Key.Right[0] });
                    added = true;
                }
                if (g.ProductionRules.Any(x => x.Key.Left.Equals(A) && x.Key.Right.Contains(EPSILON)))
                {
                    if (fi.Keys.Contains(A + 0.ToString()))
                    {
                        fi[A + 0.ToString()].Add(EPSILON);
                    }
                    else
                    {
                        fi.Add(A + 0.ToString(), new HashSet<string>() { EPSILON });
                    }
                    added = true;
                }
                if (!added)
                {
                    fi.Add(A + 0.ToString(), new HashSet<string>());
                }
            }

            int i = 0;
            bool changed = false;
            int tries = 2;
            do
            {
                changed = false;

                i++;
                var tempFi = new Dictionary<string, HashSet<string>>();
                foreach (var A in g.Nonterminals)
                {
                    var rules = g.ProductionRules.Where(x => x.Key.Left == A);
                    foreach (var rule in rules)
                    {

                        var firstOfEachTermInRule = new List<HashSet<string>>();
                        var rightRuleWhere = rule.Key.Right;
                        foreach (var rightElem in rightRuleWhere)
                        {
                            if (g.Nonterminals.Contains(rightElem))
                            {
                                HashSet<string> outV;
                                int iCopy = i - 1;
                                while (!fi.TryGetValue(rightElem + iCopy.ToString(), out outV))
                                {
                                    iCopy--;
                                }
                                if (!firstOfEachTermInRule.Any(x => x.SetEquals(outV)))
                                {
                                    firstOfEachTermInRule.Add(outV);
                                }
                            }
                            else
                            {
                                HashSet<string> outV;
                                int iCopy = i - 1;
                                while (!fi.TryGetValue(A + iCopy.ToString(), out outV))
                                {
                                    iCopy--;
                                }

                                firstOfEachTermInRule.Add(new HashSet<string>() { rightElem });
                            }
                        }
                        if (firstOfEachTermInRule.Count > 0)
                        {
                            var sum = firstOfEachTermInRule[0];
                            for (int t = 1; t < firstOfEachTermInRule.Count(); t++)
                            {
                                sum = PlusCircle(sum.ToList(), firstOfEachTermInRule[t].ToList());
                            }

                            HashSet<string> outV;
                            int iCopy = i;
                            while (!fi.TryGetValue(A + iCopy.ToString(), out outV))
                            {
                                iCopy--;
                            }
                            if (sum.Count != 0)
                            {
                                if (!outV.SetEquals(sum))
                                {
                                    if (fi.ContainsKey(A + i.ToString()))
                                    {
                                        fi[A + i.ToString()] = new HashSet<string>(sum.Union(fi[A + i.ToString()]));

                                        changed = true;
                                    }
                                    else
                                    {
                                        HashSet<string> outV2;
                                        int iCopy2 = i - 1;
                                        while (!fi.TryGetValue(A + iCopy2.ToString(), out outV2))
                                        {
                                            iCopy2--;
                                        }
                                        if (!outV2.SetEquals(sum))
                                        {
                                            if (sum.Any(x => !outV2.Contains(x)) && !sum.Except(outV2).All(x => x == EPSILON))

                                            {
                                                fi.Add(A + i.ToString(), new HashSet<string>(sum.Union(outV2)));

                                                changed = true;
                                            }
                                        }
                                    }
                                }
                            }

                        }

                    }

                }
                if (!changed)
                    tries--;
            } while (changed || tries >= 0);

            var result = new Dictionary<string, HashSet<string>>();
            foreach (var nont in g.Nonterminals)
            {
                HashSet<string> outV2;
                int iCopy2 = i - 1;
                while (!fi.TryGetValue(nont + iCopy2.ToString(), out outV2))
                {
                    iCopy2--;
                }
                result.Add(nont, new HashSet<string>(fi[nont + iCopy2.ToString()]));
            }

            foreach (var term in g.Terminals)
            {
                HashSet<string> outV2;
                int iCopy2 = i - 1;
                while (!fi.TryGetValue(term + iCopy2.ToString(), out outV2) && iCopy2 > 0)
                {

                    iCopy2--;
                }

                result.Add(term, outV2);
            }
            return result;
        }

        /*
         //Another implementation for First -> not 100% correct
        public static Dictionary<string, HashSet<string>> First2(Grammar g)
        {
            var fi = new Dictionary<string, HashSet<string>>();

            //Step 1 - if X is terminal, First(X)+={X}
            foreach (var a in g.Terminals)
            {
                fi.Add(a, new HashSet<string>() { a });
            }

            //Step 2 - if X non terminal, X->aα, First(X)+={a}
            //If there is a X->ε production rule, First(X)+={ε}
            foreach (var X in g.Nonterminals)
            {
                foreach (var rule in g.ProductionRules.Where(x => x.Key.Left.Equals(X) && g.Terminals.Contains(x.Key.Right[0])))
                {
                    if (fi.ContainsKey(X))
                    {
                        fi[X].UnionWith(new HashSet<string>() { rule.Key.Right[0] });
                    }
                    else
                    {
                        fi.Add(X, new HashSet<string>() { rule.Key.Right[0] });
                    }
                }

                foreach (var rule in g.ProductionRules.Where(x => x.Key.Left.Equals(X) && x.Key.Right.All(y => y == EPSILON)))
                {
                    if (fi.ContainsKey(X))
                    {
                        fi[X].UnionWith(new HashSet<string>() { EPSILON });
                    }
                    else
                    {
                        fi.Add(X, new HashSet<string>() { EPSILON});
                    }
                }
            }

            //Step 3 - if there is a production rule X->Y1 Y2 ... Yk
            //For all i for which Y1...Yi-1 are non term and First(Y1),First(Y2),...,First(Y-1) all cotaint ε,
            //then add all non-ε from First(Y1)...First(Yi-1) to First(X).
            //If First(Yj) contains ε, for all j=0...k, add ε to First(X).
            foreach (var X in g.Nonterminals)
            {
                foreach (var rule in g.ProductionRules.Where(x => x.Key.Left.Equals(X)))
                {
                    var RHS = rule.Key.Right;//Y1...Yk
                    int k = RHS.Count();
                    for (int i = 1; i < k; i++)
                    {
                        var l = RHS.Take(i);
                        if (!l.Except(g.Nonterminals).Any())
                        {
                            //there are only non terminals in this current sublist of the RHS


                            List<HashSet<string>> firsts = new List<HashSet<string>>();
                            foreach (var y in l)
                            {
                                if (fi.ContainsKey(y))
                                {
                                    firsts.Add(fi[y]);
                                }
                                else
                                {
                                    fi.Add(y, new HashSet<string>());
                                    firsts.Add(new HashSet<string>());
                                }
                            }
                            var allFirsts = new HashSet<string>();
                            foreach (var first in firsts)
                            {
                                foreach (var innerFirst in first)
                                    allFirsts.Add(innerFirst);
                            }
                            if (fi.ContainsKey(X))
                            {
                                fi[X].UnionWith(allFirsts.Where(x => x != EPSILON));
                            }
                            else
                            {
                                fi.Add(X, new HashSet<string>(allFirsts.Where(x => x != EPSILON)));
                            }
                            if (i == k - 1)
                            {
                                if (allFirsts.Contains(EPSILON))
                                {
                                    if (fi.ContainsKey(X))
                                    {
                                        fi[X].Add(EPSILON);
                                    }
                                    else
                                    {
                                        fi.Add(X, new HashSet<string>() { EPSILON });
                                    }
                                }
                            }

                        }

                    }
                }
            }

            return fi;
        }
        */


        //Determine the first set for sym, where sym is the first element in the RHS of the rule
        public static HashSet<string> FirstOf(string sym, Dictionary<string, HashSet<string>> firsts, ProductionRule rule)
        {

            var first = firsts[sym];
            if (first.Count() == 0 || first.Any(x => x == EPSILON))
            {
                foreach (var rightElem in rule.Right.Where(x => x != sym))
                {
                    first.UnionWith(firsts[rightElem]);
                    if (first.Count() == 0 || first.Any(x => x == EPSILON))
                        continue;
                    else
                        break;
                }
            }
            return first;
        }

        //Determine the first set for sym, where sym is followed in the RHS of the production rule by tail
        public static HashSet<string> FirstOfWithoutRule(string sym, Dictionary<string, HashSet<string>> firsts, List<string> tail)
        {
            var first = firsts[sym];
            if (first.Count() == 0 || first.Any(x => x == EPSILON))
            {
                foreach (var rightElem in tail)
                {
                    first.UnionWith(firsts[rightElem]);
                    if (first.Count() == 0 || first.Any(x => x == EPSILON))
                        continue;
                    else
                        break;
                }
            }
            return first;
        }

        //Compute the follow sets for all the non terminals in the grammar
        public static Dictionary<string, HashSet<string>> Follow(Grammar g, Dictionary<string, HashSet<string>> Firsts)
        {
            Dictionary<string, HashSet<string>> res = new Dictionary<string, HashSet<string>>();
            res.Add(g.StartSymbol, new HashSet<string>() { "$" });
            foreach (var A in g.Nonterminals.Where(x => x != g.StartSymbol))
                res.Add(A, new HashSet<string>());
            bool changed = false;
            int tries = 2;
            do
            {
                changed = false;
                foreach (var A in g.Nonterminals)
                {
                    var rules = g.ProductionRules.Where(x => x.Key.Right.Contains(A));
                    foreach (var rule in rules)
                    {

                        var listOfIndexes = rule.Key.Right.Select((x, i) => new { x, i })
                            .Where(x => x.x == A)
                            .Select(x => x.i);
                        foreach (var i in listOfIndexes)
                        {
                            if (i < rule.Key.Right.Count() - 1)
                            {
                                var firstOfBeta = new HashSet<string>();
                                /*
                                for (int j = i + 1; j < rule.Key.Right.Count(); j++)
                                {
                                    if (!g.Terminals.Contains(rule.Key.Right[j]))
                                    {
                                        firstOfBeta = new HashSet<string>(firstOfBeta.Union(Firsts[rule.Key.Right[j]]));
                                    }
                                }*/
                                var tail = rule.Key.Right.Skip(i + 1).ToList();
                                firstOfBeta = FirstOfWithoutRule(tail[0], Firsts, tail);

                                var reunion = new HashSet<string>(res[rule.Key.Right[i]].Union(firstOfBeta.Where(x => x != EPSILON)));
                                if (!res[rule.Key.Right[i]].SetEquals(reunion))
                                {
                                    changed = true;
                                    res[rule.Key.Right[i]] = reunion;
                                }
                                if (firstOfBeta.Contains(EPSILON))
                                {
                                    var reunion2 = new HashSet<string>(res[rule.Key.Right[i]].Union(res[rule.Key.Left]));

                                    if (!res[rule.Key.Right[i]].SetEquals(reunion2))
                                    {
                                        changed = true;
                                        res[rule.Key.Right[i]] = reunion2;
                                    }
                                }
                            }
                            else
                            {
                                var reunion2 = new HashSet<string>(res[rule.Key.Right[i]].Union(res[rule.Key.Left]));
                                if (!res[rule.Key.Right[i]].SetEquals(reunion2))
                                {
                                    changed = true;
                                    res[rule.Key.Right[i]] = reunion2;
                                }
                            }
                        }


                    }
                }
                if (!changed)
                    tries--;
            } while (changed || tries >= 0);
            return res;
        }

        /*
         //Another implementation for Follow -> not 100% correct
        public static Dictionary<string, HashSet<string>> Follow2(Grammar g, Dictionary<string, HashSet<string>> Firsts)
        {
            Dictionary<string, HashSet<string>> res = new Dictionary<string, HashSet<string>>();
            res.Add(g.StartSymbol, new HashSet<string>() { "$" });
            foreach (var A in g.Nonterminals.Where(x => x !=g.StartSymbol))
                res.Add(A, new HashSet<string>());
            bool changed = false;
            int tries = 2;
            do
            {
                changed = false;
                foreach (var A in g.Nonterminals)
                {
                    var rules = g.ProductionRules.Where(x => x.Key.Right.Contains(A));
                    foreach (var rule in rules)
                    {
                        if (rule.Key.Right.Where(x => x == A).Count() == 1)
                        {
                            var i = rule.Key.Right.IndexOf(A);
                            if (i < rule.Key.Right.Count() - 1)
                            {
                                var firstOfBeta = new HashSet<string>();

                               // for (int j = i + 1; j < rule.Key.Right.Count(); j++)
                               // {
                               //    if (!g.Terminals.Contains(rule.Key.Right[j]))
                               //     {
                               //         firstOfBeta = new HashSet<string>(firstOfBeta.Union(Firsts[rule.Key.Right[j]]));
                               //     }
                               // }
                                var tail = rule.Key.Right.Skip(i + 1).ToList();
                                firstOfBeta = FirstOfWithoutRule(tail[0], Firsts, tail);

                                var reunion = new HashSet<string>(res[rule.Key.Right[i]].Union(firstOfBeta.Where(x => x != EPSILON)));
                                if (!res[rule.Key.Right[i]].SetEquals(reunion))
                                {
                                    changed = true;
                                    res[rule.Key.Right[i]] = reunion;
                                }
                                if (firstOfBeta.Contains(EPSILON))
                                {
                                    var reunion2 = new HashSet<string>(res[rule.Key.Right[i]].Union(res[rule.Key.Left]));

                                    if (!res[rule.Key.Right[i]].SetEquals(reunion2))
                                    {
                                        changed = true;
                                        res[rule.Key.Right[i]] = reunion2;
                                    }
                                }
                            }
                            else
                            {
                                var reunion2 = new HashSet<string>(res[rule.Key.Right[i]].Union(res[rule.Key.Left]));
                                if (!res[rule.Key.Right[i]].SetEquals(reunion2))
                                {
                                    changed = true;
                                    res[rule.Key.Right[i]] = reunion2;
                                }
                            }
                        }
                        else
                        {
                            var listOfIndexes = rule.Key.Right.Select((x, i) => new { x, i })
                                .Where(x => x.x == A)
                                .Select(x => x.i);
                            foreach(var i in listOfIndexes)
                            {
                                if (i < rule.Key.Right.Count() - 1)
                                {
                                    var firstOfBeta = new HashSet<string>();

                             //       for (int j = i + 1; j < rule.Key.Right.Count(); j++)
                               //     {
                                //        if (!g.Terminals.Contains(rule.Key.Right[j]))
                                //        {
                                //            firstOfBeta = new HashSet<string>(firstOfBeta.Union(Firsts[rule.Key.Right[j]]));
                                  //      }
                                 //   }
                                    var tail = rule.Key.Right.Skip(i + 1).ToList();
                                    firstOfBeta = FirstOfWithoutRule(tail[0], Firsts, tail);

                                    var reunion = new HashSet<string>(res[rule.Key.Right[i]].Union(firstOfBeta.Where(x => x != EPSILON)));
                                    if (!res[rule.Key.Right[i]].SetEquals(reunion))
                                    {
                                        changed = true;
                                        res[rule.Key.Right[i]] = reunion;
                                    }
                                    if (firstOfBeta.Contains(EPSILON))
                                    {
                                        var reunion2 = new HashSet<string>(res[rule.Key.Right[i]].Union(res[rule.Key.Left]));

                                        if (!res[rule.Key.Right[i]].SetEquals(reunion2))
                                        {
                                            changed = true;
                                            res[rule.Key.Right[i]] = reunion2;
                                        }
                                    }
                                }
                                else
                                {
                                    var reunion2 = new HashSet<string>(res[rule.Key.Right[i]].Union(res[rule.Key.Left]));
                                    if (!res[rule.Key.Right[i]].SetEquals(reunion2))
                                    {
                                        changed = true;
                                        res[rule.Key.Right[i]] = reunion2;
                                    }
                                }
                            }
                        }

                    }
                }
                if (!changed)
                    tries--;
            } while (changed || tries >= 0);
            return res;
        }
        */


        //UNUSED METHODS:

        public static bool CompareDict(Dictionary<string, HashSet<string>> olddict1, Dictionary<string, HashSet<string>> olddict2)
        {
            Dictionary<string, HashSet<string>> dict1 = new Dictionary<string, HashSet<string>>();
            Dictionary<string, HashSet<string>> dict2 = new Dictionary<string, HashSet<string>>();
            foreach (var kvp in olddict1)
            {
                dict1.Add(kvp.Key[0].ToString(), kvp.Value);
            }
            foreach (var kvp in olddict2)
            {
                dict2.Add(kvp.Key[0].ToString(), kvp.Value);
            }
            if (dict1.Keys.Count != dict2.Keys.Count)
                return false;
            foreach (var key in dict1.Keys)
            {
                if (!dict2.Keys.Contains(key))
                    return false;
            }
            foreach (var key in dict2.Keys)
            {
                if (!dict1.Keys.Contains(key))
                    return false;
            }
            foreach (var kvp in dict1)
            {
                if (!dict2[kvp.Key].SetEquals(kvp.Value))
                    return false;
            }
            foreach (var kvp in dict2)
            {
                if (!dict1[kvp.Key].SetEquals(kvp.Value))
                    return false;
            }

            return true;
        }

        private static Dictionary<string, HashSet<string>> MergeDictionaries(Dictionary<string, HashSet<string>> dict1, Dictionary<string, HashSet<string>> dict2)
        {
            var result = new Dictionary<string, HashSet<string>>();
            foreach (var kvp in dict1)
            {
                if (dict2.ContainsKey(kvp.Key))
                {
                    result.Add(RecreateKey(kvp.Key), new HashSet<string>(kvp.Value.Union(dict2[kvp.Key])));
                }
                else
                {
                    result.Add(RecreateKey(kvp.Key), new HashSet<string>(kvp.Value));
                }
            }
            return result;
        }

        private static Dictionary<string, HashSet<string>> UpdateFiMinus1(Dictionary<string, HashSet<string>> dict)
        {
            Dictionary<string, HashSet<string>> res = new Dictionary<string, HashSet<string>>();
            foreach (var kvp in dict)
            {
                res.Add(RecreateKey(kvp.Key), new HashSet<string>(kvp.Value));
            }
            return res;
        }

        private static string RecreateKey(string oldKey)
        {
            var builder = new StringBuilder();
            var A = oldKey[0];
            builder.Append(A);
            var iStr = oldKey.Substring(1);
            int i;
            if (int.TryParse(iStr, out i))
            {
                builder.Append(i++);
            }
            else
            {
                throw new Exception("Invalid input!");

            }
            return builder.ToString();
        }

    }
}
