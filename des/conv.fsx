module conv

open System.Collections

// !! damages input for performance !!
let pad chunks =
    let last = Array.last chunks
    let size = Array.length last
    let difference = 8 - size

    if difference <> 0 then
        let padding = Array.replicate difference (byte difference)
        let padded = Array.append last padding
        Array.set chunks ((Array.length chunks) - 1) padded

    chunks

// !! damages input for performance !!
let unpad chunks =
    let lastChunk = Array.last chunks
    let lastByte = Array.last lastChunk
    let padding = (int lastByte)
    let contentLength = 8 - padding

    if padding <= 7 then // maximum padding
        let count =
            [ contentLength + 1 .. chunks.Length ]
            |> List.map (Array.get lastChunk)
            |> List.forall (fun x -> x = lastByte)

        if count then
            let truncated = Array.take contentLength lastChunk
            Array.set chunks ((Array.length chunks) - 1) truncated

    chunks





let toBlocks bytes =
    bytes
    |> Array.chunkBySize 8 // 64 bits
    |> pad
    |> Array.map BitArray // BitVector byłby szybszy, ale ma za małą pojemność

let BitArrayToBytes (bits: BitArray) =
    let ans = Array.replicate 8 0uy
    bits.CopyTo(ans, 0)
    ans

let toBytes blocks =
    blocks
    |> Array.map BitArrayToBytes
    |> unpad
    |> Array.concat


let split (block: BitArray) =
    let parts = Array.replicate 2 0 // 2 * 32-bit integer
    block.CopyTo(parts, 0) // kopiujemy zawartość do bloków
    let L = BitArray parts[0..0]
    let R = BitArray parts[1..1]
    (L, R)

let join ((L: BitArray), (R: BitArray)) =
    let l = Array.singleton 0
    let r = Array.singleton 0
    L.CopyTo(l, 0)
    R.CopyTo(r, 0)
    let joined = Array.concat [ l; r ]
    BitArray joined

let ConcatenateFourBitPieces (arr: array<int>) =
    let mutable acc = 0

    for i in (Array.rev arr) do
        acc <- ((acc <<< 4) ||| i)

    BitArray(Array.singleton acc)

let toSixBitPieces (bits: BitArray) =
    let arr = (Array.replicate 48 false)
    bits.CopyTo(arr, 0)
    arr |> Array.chunkBySize 6
