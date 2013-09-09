// Yogesh Sajanikar (02/2013)
// Learn more about F# at http://fsharp.net
#nowarn "40" 

namespace Institis.FXml 

module Xml =

    open System
    open System.Xml

    // This is a node attribute
    type Node = | NodeStart of String 
                | NodeEnd   of String
                | NodeAttr  of String * String
                | NodeText  of String
                | NodeEmpty

    let toString (n:Node) = match n with
                                | NodeStart(s)          -> "{" + s
                                | NodeEnd(s)            -> "}"
                                | NodeAttr(s,v)         -> s + ":" + v
                                | NodeText(s)           -> "Text:" + s
                                | _                     -> String.Empty

    type NodeIterator = | NodeIt of Node * XmlReader

    let rec private moveNext (rdr:XmlReader) = 
        if rdr.Read() then
            match rdr.NodeType with
                | XmlNodeType.EndElement      -> NodeIt(NodeEnd(rdr.LocalName), rdr)
                | XmlNodeType.Document        -> NodeIt(NodeStart(rdr.LocalName), rdr)
                | XmlNodeType.Element         -> NodeIt(NodeStart(rdr.LocalName), rdr)
                | XmlNodeType.Attribute       -> NodeIt(NodeAttr(rdr.Name, rdr.Value), rdr)
                | _                           -> moveNext rdr
                    
        else
            NodeIt(NodeEmpty, rdr)

    let next (nit:NodeIterator) = 
        match nit with
            | NodeIt(NodeStart(_),rdr)             -> if rdr.MoveToFirstAttribute() then 
                                                            NodeIt(NodeAttr(rdr.Name,rdr.Value), rdr)
                                                      else
                                                            if rdr.IsEmptyElement then
                                                                NodeIt(NodeEnd(rdr.Name), rdr)
                                                            else
                                                                moveNext rdr
            | NodeIt(NodeAttr(_,_), rdr)           -> if rdr.MoveToNextAttribute() then
                                                            NodeIt(NodeAttr(rdr.Name,rdr.Value), rdr)
                                                      else
                                                            if rdr.IsEmptyElement then
                                                                NodeIt(NodeEnd(rdr.Name), rdr)
                                                            else
                                                                moveNext rdr
            | NodeIt(_,rdr)                        -> moveNext rdr

    let first (x:XmlReader) = moveNext x
            

    let rec nodeSeq nd = seq {  
                            match nd with
                            | NodeIt(NodeEmpty,_)   ->  yield NodeEmpty
                            | NodeIt(n,r)           ->  yield n
                                                        yield! nodeSeq(next(nd)) }      
                                                        
    type NodeStream = | NodeS of Node list * NodeIterator 

    let nodeStream (x:XmlReader) = NodeS([], moveNext x)

    let head (ns:NodeStream) = 
        match ns with
            | NodeS([], NodeIt(n,r))        ->    n
            | NodeS(x::xs, _)               ->    x

    let tail (ns:NodeStream) =
        match ns with
            | NodeS([], n)      -> NodeS([], next n)
            | NodeS(x::xs, n)   -> NodeS(xs, n)

    let cons (x:Node) (NodeS(xs,n)) = NodeS(x::xs,n)


    // A parser with the above nodes
    type 'a NodeParser = | NodeP of (NodeStream  -> ('a Option * NodeStream))

    // Run the parser
    let runNodeP (NodeP p) = p

    // Either or parser 
    type OrT<'a,'b> = | EitherA of 'a
                      | EitherB of 'b

    let orP (ap:'a NodeParser) (bp:'b NodeParser) = 
        NodeP (fun nd -> match runNodeP ap nd with
                            | (None,ns)         -> match runNodeP bp ns with
                                                    | (None,nb)         -> (None,nb)
                                                    | (Some(bps),nb)    -> (Some(EitherB(bps)),nb)
                            | (Some(aps),ns)    -> (Some(EitherA(aps)),ns)
               )
            
    // Matches a sequence
    let seqP (ma: 'a NodeParser) (fab: 'a -> 'b NodeParser) = 
        NodeP (
                fun nd -> match runNodeP ma nd with
                            | (None,ns)         -> (None,ns)
                            | (Some(aps),ns)    -> runNodeP (fab aps) ns
              )


    let returnP (f:'a)  = NodeP ( function ns -> (Some(f),ns))

    let failP           = NodeP ( function ns -> (None,ns))


    let createP (name:String)  (f:'a ->'b) (r:unit -> 'a) = 
        NodeP ( function ns -> match head ns with
                                | NodeStart(nm)       ->  if name = nm then 
                                                            runNodeP (returnP (f (r()))) (tail ns)
                                                          else
                                                            runNodeP failP ns
                                | n                   ->  runNodeP failP ns
               )

    let createP1 (nmatch:Node)  (f:unit ->'b) = 
        NodeP ( function ns -> head ns |> function n -> if nmatch.Equals(n) then runNodeP (returnP (f() )) (tail ns) else runNodeP failP ns)

    let rec manyP' (ma:'a NodeParser) ns xs = match runNodeP ma ns with
                                                | (None,nd)     -> match xs with 
                                                                    | []    ->  (Some([]),nd)
                                                                    | _     ->  (Some(xs),nd)
                                                | (Some(x),nd)  -> manyP' ma nd (x::xs)


    // Will match zero or more
    let manyP (ma: 'a NodeParser) = 
        NodeP ( function nds -> manyP' ma nds [] )

    // Will at least match one node
    let manyP1 (ma: 'a NodeParser) = seqP ma (function x -> NodeP ( function nds -> manyP' ma nds [x] ) )
                                

    // Create the builder
    type NodeParserBuilder () =
        class
            // Sequencing 
            member this.Bind ((ma: 'a NodeParser), (fab: 'a -> 'b NodeParser)) = seqP ma fab

            // Delay
            member this.Delay (fa: unit -> 'a NodeParser) = fa()

            // Return
            member this.Return (t: 'a) = returnP t

            // 
            member this.ReturnFrom (ma: 'a NodeParser) = ma

            // Run the parser
            member this.Run (ma: 'a NodeParser) = ma

            // 
            member this.Combine ((ma1: 'a NodeParser), (ma2: 'a NodeParser)) = seqP ma1 (function _ -> ma2 )

            // 
            member this.For ((sa: 'a seq), (fab: 'a -> 'b NodeParser)) = seq { for s in sa do 
                                                                                    yield (fab s)
                                                                             }
            // 
            member this.TryFinally ((ma: 'a NodeParser), (fuu: unit -> unit)) =
                NodeP ( function ns -> let x = runNodeP ma ns 
                                       (fuu())
                                       x )

            member this.While ((fb: unit -> bool), (ma: 'a NodeParser)) = if (fb()) then ma else failP 

            // 
            member this.Yield t = returnP t

            //
            member this.YieldFrom (ma:'a NodeParser) = NodeP ( function ns -> runNodeP ma ns )

            // 
            member this.Zero () = returnP None

        end


module Test = 

    open System

    type Compartment = | EmptyC
                       | CompartmentC of Compartment list

    type Vessel = | EmptyV
                  | VesselV of Compartment list 

    let rec cmpP =  Xml.NodeParserBuilder() {
                    let! c  = Xml.createP1 (Xml.NodeStart("Compartment")) (function () -> EmptyC)
                    let! childCmps = Xml.manyP cmpP
                    let! cs = Xml.createP1 (Xml.NodeEnd("Compartment"))   (function () -> CompartmentC(childCmps))
                    return cs
                }

    let shipP = Xml.NodeParserBuilder() { 
                    let! s  = Xml.createP1 (Xml.NodeStart("Ship")) (function () -> EmptyV )
                    let! cs = Xml.manyP cmpP
                    return VesselV(cs)
                }


    let run ((p:'a Xml.NodeParser),ns:Xml.NodeStream) = match Xml.runNodeP p ns with
                                                            | (Some(x),_)       -> x
                                                            | (None,_)          -> Unchecked.defaultof<'a>