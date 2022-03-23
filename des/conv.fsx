module conv

open System.Collections


let pad bytes =
    let len = Array.length bytes
    let pad = 7 - (len % 8)
    let zeros = Array.replicate pad 0uy
    let padding = Array.append zeros (Array.singleton (byte pad))
    Array.append bytes padding

let unpad (bytes: byte []) =
    let len = Array.length bytes
    let last = (int (Array.last bytes))
    bytes[0 .. len - last - 1]


let toBlocks bytes =
    bytes
    |> pad
    |> Array.chunkBySize 8 // 64 bits
    |> Array.map BitArray // BitVector byłby szybszy, ale ma za małą pojemność

let BitArrayToBytes (bits: BitArray) =
    let ans = Array.replicate 8 0uy
    bits.CopyTo(ans, 0)
    ans

let toBytes blocks =
    blocks
    |> Array.map BitArrayToBytes
    |> Array.concat
    |> unpad


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
