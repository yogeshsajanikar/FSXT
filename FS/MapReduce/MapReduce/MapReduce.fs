namespace Ary.Algo 

open System.Xml

open System.Collections.Generic

 

module MapReduce =

 

    // Mapping type - Maps a pair of key and value to another set of key and value

    type Mapper<'K1, 'V1, 'K2, 'V2>   = | MapF of ('K1 -> 'V1 -> seq<'K2 * 'V2>)

 

    // Reducer type - Reduces key-value pair to an optional value

    type Reducer<'K2, 'V2, 'V3>       = | ReduceF of ('K2 -> seq<'V2> -> option<'V3>)

 

    //

    let runMap (MapF f) = f

 

    let runReducer (ReduceF r) = r

 

    // Define map reduce

    type MapReducer<'K1, 'V1, 'K2, 'V2, 'V3> = | MapReduceP of Mapper<'K1, 'V1, 'K2, 'V2> * Reducer<'K2, 'V2, 'V3>

 

 

    // Map the input into a group

    let mapWithKey m (inp:seq<'K1*'V1>) = 

        seq {

            for s in (Seq.map (fun (x,y) -> runMap m x y) inp) do 

                for sq in s do

                    yield sq

            }

 

    let addToSMap s (x, y) = if (true <> Map.containsKey x s) then 

                                Map.add x [y] s

                             else

                                Map.add x (y::(Map.find x s)) s

                                //Map.add x (Seq.append (Map.find x s). (Seq.singleton y)) s

 

    let groupPerKey (inp:seq<'K2*'V2>) = Seq.fold addToSMap Map.empty inp

 

    let reducePerKey  r (mp:Map<'K2,'V2>) = Map.fold (fun s x y -> Seq.append s (Seq.singleton ((runReducer r) x (Seq.ofList y)))) Seq.empty mp

 

    let filterPerKey bs = Seq.choose id bs

 

    //let runMapReduce (MapReduceP (m, r))  (inp:seq<'K1 *'V1>) = 

 

    let runMapReduce (MapReduceP (m, r))  (inp:seq<'K1 *'V1>) = 

        mapWithKey m inp |> groupPerKey |> reducePerKey r |> filterPerKey

 
