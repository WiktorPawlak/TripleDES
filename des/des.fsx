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

    let unpad (chunks:array<array<byte>>) =
        let lastChunk = Array.last chunks
        let lastByte = Array.last lastChunk
        let padding = (int lastByte)
        let contentLength = 8 - padding
        if padding <= 7 then // maximum padding
            let count = [contentLength + 1  .. chunks.Length]
                         |> List.map (Array.get lastChunk)
                         |> List.forall (fun x -> x = lastByte)
            if count then
                let truncated = Array.take contentLength lastChunk
                Array.set chunks ((Array.length chunks) - 1)  truncated
        chunks
                
                
                


    let toBlocks bytes  =
        bytes 
        |> Array.chunkBySize 8 // 64 bits
        |> pad
        |> Array.map BitArray // BitVector byłby szybszy, ale ma za małą pojemność

    let BitArrayToBytes (bits:BitArray) =
        let ans = Array.replicate 8 0uy
        bits.CopyTo(ans, 0)
        ans
    
    let toBytes blocks =
        blocks
        |> Array.map BitArrayToBytes
        |> unpad
        |> Array.concat
