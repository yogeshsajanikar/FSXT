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
                    match runL ma xs with
                    | None          -> None
                    | Some (a, ys)  -> runL (fab a) ys
                LazyListFunc mb 

            member this.Return (t:'a) = 
                let ma = fun xs -> Some (t, xs)
                LazyListFunc ma

            //member this.Zero () = LazyListFunc (fun _ -> None)
                    
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

   



