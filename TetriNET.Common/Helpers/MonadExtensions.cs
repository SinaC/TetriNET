﻿//using System;
//using JetBrains.Annotations;

////http://www.codeproject.com/Articles/109026/Chained-null-checks-and-the-Maybe-monad
////http://stackoverflow.com/questions/18859299/implicitly-captured-closures-resharper-warning

//namespace TetriNET.Common.Helpers
//{
//    public static class MonadExtensions
//    {
//        public static TResult With<TInput, TResult>(this TInput o, [InstantHandle] Func<TInput, TResult> evaluator)
//            where TResult : class
//            where TInput : class
//        {
//            if (o == null)
//                return null;
//            return evaluator(o);
//        }

//        public static TResult Return<TInput, TResult>(this TInput o, [InstantHandle] Func<TInput, TResult> evaluator, TResult failureValue) where TInput : class
//        {
//            if (o == null)
//                return failureValue;
//            return evaluator(o);
//        }

//        public static TInput If<TInput>(this TInput o, [InstantHandle] Func<TInput, bool> evaluator)
//            where TInput : class
//        {
//            if (o == null)
//                return null;
//            return evaluator(o) ? o : null;
//        }

//        public static TInput Unless<TInput>(this TInput o, [InstantHandle] Func<TInput, bool> evaluator)
//            where TInput : class
//        {
//            if (o == null)
//                return null;
//            return evaluator(o) ? null : o;
//        }

//        public static TInput Do<TInput>(this TInput o, [InstantHandle] Action<TInput> action) where TInput : class
//        {
//            if (o == null)
//                return null;
//            action(o);
//            return o;
//        }
//    }
//}