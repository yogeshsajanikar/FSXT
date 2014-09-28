namespace Data.Tree


[<AutoOpen>]
module TreeModule = 

    open System
    open FSharpx.Collections
    open FSharpx.Collections.Experimental


    type 's State = State of 's

    // Input comprising of token list and a state 's
    type Input<'token, 's> = { IStream : 'token LazyList ; IState : 's State }

    // Output consists of some output 'a, alongwith rest of the input
    type Output<'a, 'token, 's> = { Out : 'a option ; Next : Input<'token, 's>; Error : string }

    // Transformation that transforms the input into an output, reporting any error
    type Transf<'a, 'token, 's> = 
        { Transf : Input<'token, 's> -> Output<'a, 'token, 's> }

    let transform t inp = t.Transf inp

    let runTransformer t inp s = t.Transf { IStream = inp; IState = s }

    type Transformer () =
        class
            member this.Bind (f : Transf<'a, 'token, 's>, fab : 'a -> Transf<'b, 'token, 's>) = 
                let transf inp = match transform f inp with
                                 | { Out = Some x; Next = next; Error = _ } -> transform (fab x) next
                                 | { Next = next; Error = error } -> { Out = None; Next = next; Error = error }
                { Transf = transf }

            member this.Return (t: 'a) = 
                let transf inp = { Out = Some t; Next = inp; Error = String.Empty }
                { Transf = transf }
        end


    let transf = Transformer ()


    // Alternative 
    let (<||>) f g = 
        let alternative inp = 
            let y = transform f inp 
            match y with 
            | { Out = Some _; Next = next; Error = _ } -> y
            | _                                        -> transform g inp
        { Transf = alternative }


    //
    let rec many1 ma = 
        transf {
                let! x  = ma 
                let! xs = many ma
                return (LazyList.cons x xs)
               }

    and many ma = (many1 ma) <||> (transf { return LazyList.empty } )

    let liftState (f : 's -> 't ) =
        fun (State s) -> State (f s)

    let fail s e = { Out = None; Next = { IStream = LazyList.empty; IState = s }; Error = e }

    let matchT (f : 'token -> bool) : Transf<'token, 'token, 's> = 
        let matchF (inp: Input<'token, 's> ) = 
            match inp.IStream with
            | LazyList.Nil          -> fail inp.IState "Empty input"
            | LazyList.Cons(x, xs)  -> if f x then 
                                            { Out = Some x; Next = { IStream = xs; IState = inp.IState }; Error = String.Empty }
                                       else
                                            fail inp.IState "No match"
        { Transf = matchF }

    