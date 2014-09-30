namespace Data.Parser.Xml 


[<AutoOpen>]
module XmlParserModule = 

    open FSharpx.Collections
    open FSharpx.Collections.Experimental
    open Data.Xml.Engine
    open Data.Parser

    type XmlState = XNode list State 

    type XmlInput = Input<XNode, XmlState>

    type XmlOutput = Output<XmlTree, XNode, XmlState> 
       
    type XmlTransf = Transf<XmlTree, XNode, XmlState> 


    let matchStart s = fun x -> match x with 
                                | XElement (t,_) -> s = t 
                                | _              -> false

    

    let matchEnd s = fun x -> match x with 
                              | XEndElement t -> s = t
                              | _             -> false

    let matchStartAny = fun x -> match x with
                                 | XElement (_,_) -> true
                                 | _              -> false


    let xstart s = matchStart s |> matchT 


    let xend s = matchEnd s |> matchT

    let matchText = fun x -> match x with 
                               | XText _ -> true
                               | _       -> false

    //type Transf<'t> = Transf<XNode, XNode, 't>

    let xtext () = matchT matchText

    let pushState x = 
        transf {
                    let! s = getState
                    return! putState (x::s)
               }

    let popState ()  = 
        transf 
            {
                let! s = getState
                match s with 
                | (x :: xs)     -> return x 
                | _             -> return! (failP "No state to pop")
            }


    let xbracket before inner after = 
        transf {
                    let! xst = before
                    pushState xst |> ignore
                    let! inn = inner
                    let! xed = after
                    popState () |> ignore
                    return xst
               }

    let simpleElement s = xbracket (xstart s) (xtext ()) (xend s)

    let rec xmany1 t = 
        transf {
                    let! x = t
                    let! xs = xmany t
                    return (x :: xs) 
                }

    and xmany t = 
        transf {
                    let! xs = (xmany1 t) <||> empty
                    return xs
                }

    let getName x = match x with 
                    | XElement (n,_) -> n
                    | _ -> failwith <| "Invalid element"

    let rec genericXml () = 
        transf {
                    let! x = matchStartAny |> matchT
                    let s  = getName x

                    pushState x 

                    let! xt = (xtext ()) <||> empty

                    pushState xt |> ignore

                    let! inners = many (genericXml ())
                    //let! inner = (many (genericXml ()))
                    let! xe =  xend s
                   
                    //return! (getState ())
                    return! getState
                }


    let testPushX () =
        transf  {
                    pushState (XElement ("ABC",[]))
                    pushState (XElement ("DEF",[]))
                    return! getState
                }

    let testPutX () =
        transf  {
                    let! y = putState (XElement ("ABC", []))
                    let! x = getState
                    return x
                }