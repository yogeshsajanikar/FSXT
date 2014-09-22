namespace Data.Xml.Tree


[<AutoOpen>]
module XmlTreeModule = 

    open FSharpx.Collections
    open FSharpx.Collections.Experimental
    open Data.Xml.Engine

    type XmlTree = 
        XNode RoseTree 


    type LazyListFunc<'t,'a> = 
        LazyListFunc of ('t LazyList -> ('a * 't LazyList) option)

    let runL (LazyListFunc f) x = f x

    type LazyListBuilder () =
        class
            member this.Bind (ma : LazyListFunc<'t,'a>, fab: 'a -> LazyListFunc<'t,'b>) = 
                let mb = fun xs -> 
                    //printf "==> %A\n" xs
                    match runL ma xs with
                    | None          -> None
                    | Some (a, ys)  -> runL (fab a) ys
                LazyListFunc mb 

            member this.Return (t:'a) = 
                let ma = fun xs -> Some (t, xs)
                LazyListFunc ma


            member this.Zero () = LazyListFunc (fun _ -> None)
                    
        end 
        
    let do' = LazyListBuilder ()


    let (+++) (ma: LazyListFunc<'t,'a>) (na: LazyListFunc<'t,'a>) = 
        let ma xs = 
            match runL ma xs with
            | None              -> runL na xs
            | Some y            -> Some y
        LazyListFunc ma 

    let rec many1 (ma: LazyListFunc<'t,'a>) : LazyListFunc<'t, 'a LazyList> = 
        do' {
                let! x  = ma 
                let! xs = many ma
                return (LazyList.cons x xs)
            }

    and many (ma: LazyListFunc<'t,'a>) = (many1 ma) +++ (do' { return LazyList.empty } )

    
    let matchStart (x: XNode) = 
        match x with
        | XElement _    -> true
        | _             -> false


    let matchEnd (x:XNode) = 
        match x with
        | XEndElement _     -> true
        | _                 -> false


    let satisfy (f: XNode -> bool) = 
        LazyListFunc 
            (fun xs -> 
                match xs with 
                | LazyList.Nil          -> None
                | LazyList.Cons(y,ys)   -> if f y then Some (y, ys) else None)


    let orF (f: 'a -> bool) (g: 'a -> bool) = 
        fun x -> 
            let fx = f x 
            if fx then fx else g x

    let andF (f: 'a -> bool) (g: 'a -> bool) = 
        fun x ->
            let fx = f x
            if fx then g x else false

    let notF (f: 'a -> bool) =
        fun x -> not (f x)


    let rec toTree () = 
        do' {
                //printf "matching start "
                let! xstart     = satisfy matchStart
                //printf "%A\n" xstart
                let! xcontents  = many ( (toTree ()) +++ (satisfy (notF (orF matchStart matchEnd))))
                //printf "contents: %A\n" xcontents
                let! xend       = satisfy matchEnd
                return (XElement ("ASD", list.Empty, xcontents))
            }


    let ofList (xs:XNode LazyList) = 
        let ofList' = 
            do' {
                    //printf "Skipping initial elements\n"
                    let! pre  = many (satisfy (notF (orF matchStart matchEnd))) 
                    //printf "Parsing elements\n"
                    let! root = toTree()
                    //printf "Skipping rest of the elements\n"
                    let! post = many (satisfy (notF (orF matchStart matchEnd)))
                    return root
                }
        runL ofList' xs