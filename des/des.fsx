module des

open System.Collections

#if INTERACTIVE
#load "tables.fsx"
#load "conv.fsx"
#load "utils.fsx"
#endif

module permutations =
    let perm length locations (bits: BitArray) =
        let ans = Array.replicate length false

        for loc in 0 .. length - 1 do
            let index = (Array.get locations loc)
            let value = (bits.Get index)
            ans[loc] <- value

        BitArray ans

    let initial = perm 64 tables.initial
    let reverse = perm 64 tables.reverse
    let P = perm 32 tables.P
    let PC2 = perm 48 tables.PC2
    let PC1 = perm 56 tables.PC1
    let E = perm 48 tables.E

module keys =
    let schedule (key: array<BitArray>) n = key[n - 1]

    let rec shift (input: BitArray) count =
        // ABCD EFGH --> BCDA FGHE
        let output = BitArray input
        output.RightShift(1) |> ignore // w specyfikacji jest LShift, ale to kwestia końcówkowości
        let first = input.Get 0
        let second = input.Get 28
        output.Set(27, first)
        output.Set(55, second)

        match count with
        | 1 -> output
        | _ -> shift output (count - 1)


    let expand key =
        // key --> subkey lookup table
        let reduced = permutations.PC1 key

        let items =
            List.scan shift reduced tables.shiftcount
            |> List.map permutations.PC2

        items.Tail |> Array.ofList

    let expd = utils.memoize expand



module sbox =
    let lookupBox (n, addr) =
        let table = tables.Sproc[n]
        table[addr]

    let lookupBoxes (bits: BitArray) =
        let bools = (Array.replicate 48 false)
        bits.CopyTo(bools, 0)
        let unpacked = bools |> Array.map System.Convert.ToInt32
        let mutable acc = 0

        for j = 7 downto 0 do
            let o = j * 6
            let row = unpacked[o + 0] * 2 + unpacked[o + 5]

            let column =
                unpacked[o + 1] * 8
                + unpacked[o + 2] * 4
                + unpacked[o + 3] * 2
                + unpacked[o + 4]

            let addr = row * 16 + column
            let value = lookupBox (j, addr)
            acc <- ((acc <<< 4) ||| value)

        BitArray(Array.singleton acc)


    let apply bits = bits |> lookupBoxes


module rec crypt =
    let xor a (b: BitArray) = b.Xor a // destroys b

    let cipher keyPart bits = // the $f$ function
        bits
        |> permutations.E
        |> xor keyPart
        |> sbox.apply
        |> permutations.P

    let rec iter key n (L, R) =
        let L' = R
        let keyPart = keys.schedule key n
        let R' = xor L (cipher keyPart R)

        match n with // List.fold?
        | 16 -> (R', L')
        | _ -> iter key (n + 1) (L', R')



    let crypt key block =
        block
        |> permutations.initial
        |> conv.split
        |> iter key 1
        |> conv.join
        |> permutations.reverse


let encrypt key block = crypt.crypt (keys.expd key) block

let decrypt key block =
    crypt.crypt (Array.rev (keys.expd key)) block
