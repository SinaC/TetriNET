// © 2010 IDesign Inc. All rights reserved 
//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceModelEx
{
   public static class ArrayExtensions 
   {
      public static U[] ConvertAll<T,U>(this T[] array,Converter<T,U> converter)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         IEnumerable<T> enumerable = array;
         return enumerable.ConvertAll(converter).ToArray(); 
      }

      public static T[] SkipWhile<T>(this T[] array,Predicate<T> match)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         if(match == null)
         {
            throw new ArgumentNullException("match");
         }
         Func<T,bool> func = (t)=>
                             {
                                return match(t);
                             };
         return array.SkipWhile(func).ToArray();
      }
      public static T[] Take<T>(this T[] array,int count)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         if(count < 0)
         {
            throw new IndexOutOfRangeException("count");
         }
         IEnumerable<T> enumerable = array;
         return enumerable.Take(count).ToArray();
      }
      public static T[] TakeWhile<T>(this T[] array,Predicate<T> match)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         if(match == null)
         {
            throw new ArgumentNullException("match");
         }
         Func<T,bool> func = (t)=>
                             {
                                return match(t);
                             };
         return array.TakeWhile(func).ToArray();
      }
      public static T[] Skip<T>(this T[] array,int count)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         if(count < 0)
         {
            throw new IndexOutOfRangeException("count");
         }
         IEnumerable<T> enumerable = array;
         return enumerable.Skip(count).ToArray();
      }
      public static T[] Repeat<T>(T element,int count)
      {
         if(count <= 0)
         {
            throw new ArgumentOutOfRangeException("count");
         }
         return Enumerable.Repeat<T>(element,count).ToArray();
      }
      public static T[] Concat<T>(this T[] first,T[] second)
      {
         if(first == null)
         {
            throw new ArgumentNullException("first");
         }
         if(second == null)
         {
            throw new ArgumentNullException("second");
         }
         IEnumerable<T> enumerable = first;
         return enumerable.Concat(second).ToArray();
      }

      public static T[] Reverse<T>(this T[] array)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         IEnumerable<T> enumerable = array;
         return enumerable.Reverse().ToArray();
      }
      public static T[] Union<T>(this T[] array1,T[] array2) where T : IEquatable<T>
      {
         if(array1 == null)
         {
            throw new ArgumentNullException("array1");
         }
         if(array2 == null)
         {
            throw new ArgumentNullException("array2");
         }

         IEnumerable<T> enumerable1 = array1;
         return enumerable1.Union(array2).ToArray();
      }
      public static T[] Complement<T>(this T[] array1,T[] array2) where T : IEquatable<T>
      {
         if(array1 == null)
         {
            throw new ArgumentNullException("array1");
         }
         if(array2 == null)
         {
            throw new ArgumentNullException("array2");
         }
         IEnumerable<T> enumerable1 = array1;
         return enumerable1.Complement(array2).ToArray();
      }
      public static T[] Except<T>(this T[] array1,T[] array2) where T : IEquatable<T>
      {
         if(array1 == null)
         {
            throw new ArgumentNullException("array1");
         }
         if(array2 == null)
         {
            throw new ArgumentNullException("array2");
         }
         IEnumerable<T> enumerable1 = array1;
         return enumerable1.Except(array2).ToArray();
      }
      public static T[] Intersect<T>(this T[] array1,T[] array2) where T : IEquatable<T>
      {
         if(array1 == null)
         {
            throw new ArgumentNullException("array1");
         }
         if(array2 == null)
         {
            throw new ArgumentNullException("array2");
         }
         IEnumerable<T> enumerable1 = array1;
         return enumerable1.Intersect(array2).ToArray();
      }

      public static T[] Distinct<T>(this T[] array)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         IEnumerable<T> enumerable = array;
         return enumerable.Distinct().ToArray();
      }


      public static T[] Sort<T>(this T[] array)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         IEnumerable<T> enumerable = array;
         return enumerable.Sort().ToArray();
      }
      public static T[] Where<T>(this T[] array,Predicate<T> match)
      {
         if(array == null)
         {
            throw new ArgumentNullException("array");
         }
         if(match == null)
         {
            throw new ArgumentNullException("match");
         }
         Func<T,bool> func = (t)=>
                             {
                                return match(t);
                             };
         IEnumerable<T> enumerable = array;
         return enumerable.Where(func).ToArray();
      }
   }
}