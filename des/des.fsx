module des
open System.Collections


module crypt =
    let initialLocations = [ 57; 49; 41; 33; 25; 17; 9; 1; 59; 51; 43; 35; 27; 19; 11; 3; 61; 53; 45; 37; 29; 21; 13; 5; 63; 55; 47; 39; 31; 23; 15; 7; 56; 48; 40; 32; 24; 16; 8; 0; 58; 50; 42; 34; 26; 18; 10; 2; 60; 52; 44; 36; 28; 20; 12; 4; 62; 54; 46; 38; 30; 22; 14; 6]
    let reverseLocations = [39; 7; 47; 15; 55; 23; 63; 31; 38; 6; 46; 14; 54; 22; 62; 30; 37; 5; 45; 13; 53; 21; 61; 29; 36; 4; 44; 12; 52; 20; 60; 28; 35; 3; 43; 11; 51; 19; 59; 27; 34; 2; 42; 10; 50; 18; 58; 26; 33; 1; 41; 9; 49; 17; 57; 25; 32; 0; 40; 8; 48; 16; 56; 24 ]
    let Permutation locations (bits:BitArray) =
        let ans = BitArray bits
        for (old, loc) in (List.indexed locations) do
            ans.Set(loc, (bits.Get old))
        ans
    let initialPermutation = Permutation initialLocations
    let reversePermutation = Permutation reverseLocations


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

    let unpad chunks =
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
