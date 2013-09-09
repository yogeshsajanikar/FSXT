// Yogesh Sajanikar (02/2013)
// Learn more about F# at http://fsharp.net
// Inspired from F# Powerpack

namespace Institis.Collections

open System.Collections
open System.Collections.Generic

module FCollections =

    type 't LazyList =  | Empty      // An empty collection
                        | Cons     of  't * (unit -> 't LazyList)
                        | Delay    of  (unit -> 't LazyList)
                        | Combine  of  't LazyList * 't LazyList with

        interface IEnumerable<'t> with 
            member self.GetEnumerator() = 
                let rec toSeq stack = 
                    match stack with 
                    | []        -> Seq.empty
                    | hd::tl    -> 
                        match hd with 
                        | Empty         -> toSeq tl
                        | Cons(v,r)     -> seq { yield v; yield! (r()::tl) |> toSeq; }
                        | Delay(l)      -> seq { yield! (l()::tl) |> toSeq; }
                        | Combine(l1,l2)-> seq { yield! (l1::l2::tl) |> toSeq }
                (toSeq [self]).GetEnumerator()

        interface IEnumerable with
            member self.GetEnumerator() = (self :> IEnumerable<'t>).GetEnumerator() :> _

    let cons st tt  = match st with
                        | Empty         -> tt
                        | _             -> match tt with 
                                            | Empty     -> st
                                            | _         -> Combine(st, tt)

    let rec head st =   match st with
                        | Empty             -> None
                        | Cons(v,r)         -> Some v
                        | Delay(f)          -> head (f())
                        | Combine(l,r)      -> match l with 
                                                | Empty  -> head r
                                                | _      -> head l

    let rec tail st = match st with
                        | Empty             -> Empty
                        | Cons(v,r)         -> Delay(r)
                        | Delay(f)          -> tail (f())
                        | Combine(l,r)      -> match l with
                                                | Empty     -> tail r
                                                | _         -> match tail l with 
                                                                | Empty     -> tail r
                                                                | lt        -> Combine(lt, r)


    type LazyListBuilder() =
        member self.Yield       value       = Cons( value, fun () -> Empty )
        member self.YieldFrom   value       = value
        member self.Combine(first, second)  = Combine(first, second)
        member self.Delay f                 = Delay f
        member self.Zero()                  = Empty
        member self.For(seqf, fab:('a->'a LazyList))    =   Seq.fold (fun s t -> cons s (fab t)) Empty seqf 
                                                                                        

    let lazyList = new LazyListBuilder()



module Suspended = 

    type 'a StreamCell =    | Nil
                            | Cons of 'a * 'a Stream
    and  'a Stream     =    | Stream of ('a StreamCell) Lazy with

        interface 'a IEnumerable with
            member self.GetEnumerator() = 
                let rec toSeq strs = 
                    match strs with
                    | []                -> Seq.empty
                    | Stream(hd)::tl    -> match hd.Force() with
                                            | Nil       -> toSeq tl
                                            | Cons(v,s) -> seq { yield v; yield! (toSeq (s::tl)) } 
                (toSeq [self]).GetEnumerator()
                                    
        interface IEnumerable with
            member self.GetEnumerator() = (self :> IEnumerable<'a>).GetEnumerator() :> _


    let cell strm = match strm with
                    | Stream(strh)  -> strh



    