module des
let hello name = printfn "Hello %s" name

open System.Collections

module conv =
    // !! damages input !!
    let pad chunks =
        let last = Array.last chunks
        let size = Array.length last
        let difference = 8 - size
        if difference <> 0 then
            let padding = Array.replicate difference (byte difference)
            let padded = Array.append last padding
            Array.set chunks ((Array.length chunks) - 1)  padded
        chunks
        
        

    let toBlocks input  =
        input 
        |> Array.chunkBySize 8 // 64 bits
        |> pad
        |> Array.map BitArray

