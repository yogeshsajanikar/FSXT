// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "LazyList.fs"
open Institis.Collections.Suspended
open System.Collections.Generic

let b = Stream ( lazy (Cons(10, Stream(lazy Nil))))



let s = seq { 1..100 }

//let r = build s

//let rec build (ie:IEnumerator<'t>) = lazyList   {     
//                                                    if ie.MoveNext() then
//                                                        yield ie.Current
//                                                        yield! build ie
//                                                }
//
//
//let builds (sq:seq<'t>) = build (sq.GetEnumerator())


