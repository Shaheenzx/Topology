﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Topology.Infra
{
    /// <summary>
    /// Provides a set of static methods that related to topology.
    /// </summary>
    public class TopologyUtl
    {
        /// <summary>
        /// Find boundary points (aka frontier points) of a <paramref name="subset"/>.
        /// </summary>
        /// Theorems: If A is a subset of a topological space X, then boundary(A)= closure(A) − int(T)
        /// <typeparam name="T">Type of <paramref name="set"/> elements.</typeparam>
        public static HashSet<T> BoundaryPoints<T>(HashSet<T> set, HashSet<T> subset, HashSet<HashSet<T>> t)
        {
            var closurePoints = ClosurePoints(set, subset, t);
            closurePoints.ExceptWith(InteriorPoints(subset, t));
            return closurePoints;
        }

        /// <summary>
        /// Find exterior points of a <paramref name="subset"/>.
        /// </summary>
        /// Definition:
        ///   Let (X,τ) be a topological space and A ⊆ X, then a point p ∈ X,
        ///   is said to be an exterior point of A if there exists an open set O,
        ///   such that p ∈ O ∈ clo(A)
        /// Exterior of a Set: A - clo(A)
        /// <typeparam name="T">Type of <paramref name="set"/> elements.</typeparam>
        public static HashSet<T> ExteriorPoints<T>(HashSet<T> set, HashSet<T> subset, HashSet<HashSet<T>> t) 
            => set.Except(ClosurePoints(set, subset, t)).ToHashSet();

        /// <summary>
        /// Finds interior points of a <paramref name="subset"/>.
        /// </summary>
        /// Definition:
        /// * Let (X,t) be the topological space and A ⊆ X, then a point p∈A is said
        ///   to be an interior point of set A, if there exists an open set O such that p ∈ O ⊆ A
        /// * Interior of a set: is defined to be the union of all open sets contained in A.
        /// <typeparam name="T">Type of <paramref name="subset"/> elements.</typeparam>
        public static HashSet<T> InteriorPoints<T>(HashSet<T> subset, HashSet<HashSet<T>> t)
        {
            var interiors = new HashSet<T>();

            foreach (var g in t.Where(e => e.IsSubsetOf(subset)))
                interiors.UnionWith(g);

            return interiors;
        }

        /// <summary>
        /// Find closure points of a <paramref name="subset"/>.
        /// </summary>
        /// Definition:
        ///   Let (X,τ) be a topological space and A ⊆ X, then the closure of A
        ///   is the intersection of all closed sets containing A
        ///   i.e. the smallest closed set containing A.
        /// Example
        ///   Let X={a,b,c,d} with topology τ={{},{a},{b,c},{a,b,c},X} and A={b,d}.
        ///   - Open sets are   {},  {a}  {b,c},{a,b,c},X
        ///   - Closed sets are X,{b,c,d},{a,d},{d}    ,{}
        ///   - Closed sets containing A are X,{b,c,d}
        ///   - Now closure(A) = {b,c,d} ∩ X = {b,c,d}
        /// <typeparam name="T">Type of <paramref name="set"/> elements.</typeparam>
        public static HashSet<T> ClosurePoints<T>(HashSet<T> set, HashSet<T> subset, HashSet<HashSet<T>> t)
        {
            // Generate all closed set for every element in t
            var allClosedSets = new HashSet<HashSet<T>>(t.Count);
            foreach (var e in t) allClosedSets.Add(ClosedSet(set, e));

            // Get the closed set that containing the subset.
            var closedSets = allClosedSets.Where(subset.IsSubsetOf).ToArray();

            var closurePoints = closedSets.First();

            for (var i = 1; i < closedSets.Length; i++)
                closurePoints.IntersectWith(closedSets[i]);

            return closurePoints;
        }

        /// <summary>
        /// Finds limit points (aka accumulation points or cluster points) of a <paramref name="subset"/>.
        /// </summary>
        /// Definition: Let A be a subset of a topological space (X, t). A point p ∈ X is said to
        /// be a limit point (or accumulation point or cluster point) of A if every open set G
        /// containing p contains a point of A different from p.
        /// Mathematical form: { p ∈ X: ∀G ∈ t, p ∈ G, A∩(G - p) != {} }
        /// <typeparam name="T">Set elements type.</typeparam>
        public static HashSet<T> LimitPoints<T>(HashSet<T> set, HashSet<T> subset, HashSet<HashSet<T>> t)
        {
            var limitPoints = new HashSet<T>();

            foreach (var point in set)
            {
                var isLimit = true;
                foreach (var e in t.Where(g => g.Contains(point)))
                {
                    var except = e.Except(new[] {point}).ToArray();

                    if (!except.Any())
                    {
                        isLimit = false;
                        break;
                    }

                    if (subset.Intersect(except).Any()) continue;

                    isLimit = false;
                    break;
                }

                if (isLimit) limitPoints.Add(point);
            }

            return limitPoints;
        }

        /// <summary>
        /// Generates all topologies defined on a given set In O(2^(2^set.Count -2)).
        /// </summary>
        /// Example:
        /// - Input: S = {'c', 'b', 'a'} // See unit test.
        ///   Output Pattern: any pattern also have 000 and 111
        ///   Single And Double
        ///   001
        ///       010
        ///           011
        ///               100
        ///                   101
        ///                       110
        /// 
        ///   Single-double (disjoint)
        ///   001                 110
        ///       010         101
        ///           011 100
        /// 
        ///   Single-double
        ///   001     011
        ///   001             101
        ///       010 011
        ///       010             110
        ///               100 101
        ///               100     110
        /// 
        ///   Single-single-double
        ///   001 010 011
        ///   001         100 101
        ///       010     100     110
        ///
        ///   Single-double-double
        ///   001     011     101
        ///       010 011         110
        ///               100 101 110
        ///
        ///   single-single-double-double 
        ///   001 010 011     101
        ///   001 010 011         110
        ///   001     011 100 101
        ///   001         100 101 110
        ///       010 011 100     110
        ///       010     100 101 110
        ///   
        ///
        ///   Power set
        ///   001 010 011 100 101 110
        /// - From this pattern the elements of the power set is exist or not
        ///   so by using a brute force tests we can get all topologies.
        /// - Fact:
        ///   Let T(n) denote the number of distinct topologies on a set with n points.
        ///   There is no known simple formula to compute T(n) for arbitrary n.
        /// - Theorem:
        ///   The number of subsets of size r (or r-combinations) that can be chosen
        ///   from a set of n elements, is given by the formula: nCr = !n / r!(n - r)!
        /// - Theorem:
        ///   The number of subsets of all size is 2^n
        /// <typeparam name="T">Set elements type.</typeparam>
        /// <param name="set">The set that a topologies dif</param>
        /// <returns>Set of all topologies that defined on <paramref name="set"/>.</returns>
        public static IEnumerable<HashSet<HashSet<T>>> Topologies<T>(HashSet<T> set)
        {
            var powerSet = PowerSet(set);

            // remove the set and the empty set. for example, for set of 4 element this
            // make the complexity decrease from 2^(2^4)= 65,536 to 2^(2^4-2)= 16,384
            powerSet.RemoveWhere(s => s.Count == 0);         // O(2^set.Count)
            powerSet.RemoveWhere(s => s.Count == set.Count); // O(2^set.Count)

            var n = 1L << powerSet.Count;
            // loop to get all n subsets
            for (long i = 0; i < n; i++)
            {
                var subset = new HashSet<HashSet<T>>();

                // loop though every element in the set and determine with number 
                // should present in the current subset.
                var j = 0;
                foreach (var e in powerSet)
                    // if the jth element (bit) in the ith subset (binary number of i) add it.
                    if (((1L << j++) & i) > 0)
                        subset.Add(e);

                subset.Add(new HashSet<T>());
                subset.Add(set);
                if (IsTopology(subset, set)) yield return subset;
            }
        }

        /// <summary>
        /// Determine if the <paramref name="t"/> is a topology of the <paramref name="set"/> in O(t.Count^2).
        /// </summary>
        /// Topology Definition:
        /// let X be a set and let τ be a family of subsets of X. Then τ is called a topology on X if:
        /// * Both the empty set and X are elements of τ.
        /// * Any union of elements of τ is an element of τ.
        /// * Any intersection of finitely many elements of τ is an element of τ.
        /// <typeparam name="T">Type of <paramref name="set"/> elements.</typeparam>
        /// <param name="t">The candidate topology.</param>
        /// <param name="set">The set that candidate topology <paramref name="t"/> defined on.</param>
        /// <returns>Returns true if the <paramref name="t"/> if topology on the <paramref name="set"/>, otherwise return false.</returns>
        public static bool IsTopology<T>(HashSet<HashSet<T>> t, HashSet<T> set)
        {
            var comparer = Comparer.GetIEqualityComparer((IEnumerable<T> x, IEnumerable<T> y)
                => ((HashSet<T>) x).SetEquals(y));

            if (!t.Contains(set, comparer) || !t.Contains(new HashSet<T>(), comparer))
                return false;

            foreach (var e1 in t)
            foreach (var e2 in t)
            {
                var union = e1.Union(e2);
                var intersection = e1.Intersect(e2);
                if (!t.Contains(union, comparer) || !t.Contains(intersection, comparer))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Generates the power set of the given set in O(2^set.length).
        /// </summary>
        /// Examples:
        /// - Input: S = {a, b}, Output: {}, {a}, {b}, S
        ///   Note the pattern:          00   01   10  11
        /// - Input: S = {a, b, c}, Output: {}, {a}, {b}, {a, b} {c}, {c, a}, {b, c}, S
        ///   Note the pattern:             000 001  010  011    100  101      110    111 
        /// - Input: S = {a, b, c, d}, Output: {}, {a} , {b}, {c}, {d}, {a,b}, {a,c}, {a,d},
        ///   {b,c}, {b,d}, {c,d}, {a,b,c}, {a,b,d}, {a,c,d}, {b,c,d}, S
        /// <typeparam name="T">The type of set elements.</typeparam>
        /// <param name="set">The target set.</param>
        /// <returns>The power set of the set.</returns>
        public static HashSet<HashSet<T>> PowerSet<T>(HashSet<T> set)
        {
            var n = 1 << set.Count;
            var powerSet = new HashSet<HashSet<T>>(n);

            // loop to get all 2^set.Count subset
            for (var i = 0; i < n; i++)
            {
                var subset = new HashSet<T>();

                // loop though every element in the set and determine with number 
                // should present in the current subset.
                var j = 0;
                foreach (var e in set)
                    // if the jth element (bit) in the ith subset (binary number of i) add it.
                    if (((1L << j++) & i) > 0)
                        subset.Add(e);

                powerSet.Add(subset);
            }

            return powerSet;
        }

        /// <summary>
        /// Find the closure of a <paramref name="subset"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="set"/> elements.</typeparam>
        public static HashSet<T> ClosedSet<T>(HashSet<T> set, HashSet<T> subset)
        {
            if (subset.Count > set.Count)
                throw new Exception("The subset is bigger than the set!");

            return set.Where(e => !subset.Contains(e)).ToHashSet();
        }

        /// <summary>
        /// Converts the set to printable string.
        /// </summary>
        /// <typeparam name="T">Type of set elements.</typeparam>
        /// <param name="set">The set to convert to string</param>
        /// <returns>Printable string represent the set.</returns>
        public static string SetToString<T>(HashSet<HashSet<HashSet<T>>> set)
        {
            var sb = new StringBuilder();
            var counter = 1;

            foreach (var e in set) sb.Append($"{counter++,4}. {SetToString(e)}\n");

            return sb.ToString();
        }

        /// <summary>
        /// Converts the set to printable string.
        /// </summary>
        /// <typeparam name="T">Type of set elements.</typeparam>
        /// <param name="set">The set to convert to string</param>
        /// <returns>Printable string represent the set.</returns>
        public static string SetToString<T>(HashSet<HashSet<T>> set)
        {
            var sb = new StringBuilder("{ ");

            foreach (var e in set) sb.Append(SetToString(e) + ", ");

            // remove the extra ", "
            var len = sb.Length;
            if (len > 1) sb.Remove(len - 2, 2);
            sb.Append(" }");

            return sb.ToString();
        }

        /// <summary>
        /// Converts the set to printable string.
        /// </summary>
        /// <typeparam name="T">Type of set elements.</typeparam>
        /// <param name="set">The set to convert to string</param>
        /// <returns>Printable string represent the set.</returns>
        public static string SetToString<T>(HashSet<T> set)
        {
            var sb = new StringBuilder("{");

            foreach (var e in set) sb.Append(e + ", ");

            var len = sb.Length;
            // remove the extra ", "
            if (len > 1) sb.Remove(len - 2, 2);

            sb.Append("}");

            return sb.ToString();
        }
    }
}