module des

open System.Collections

#if INTERACTIVE
#load "tables.fs"
#load "conv.fs"
#endif



module permutations =
    let perm len locations (bits: BitArray) =
        let ans = BitArray(Array.replicate len 0uy)

        for (loc, old) in (Array.indexed locations) do
            ans.Set(loc, (bits.Get old))

        ans


    let initial = perm 8 tables.initial
    let reverse = perm 8 tables.reverse
    let P = perm 4 tables.P
    let PC2 = perm 6 tables.PC2
    let PC1 = perm 7 tables.PC1
    let E = perm 6 tables.E

module keys =
    let schedule (key: list<BitArray>) n = key.Item(n - 1)

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

        items.Tail



module sbox =
    let lookupBox (n, addr) =
        let table = tables.Sproc[n]
        table[addr]

    let lookupBoxes (bits: BitArray) =
        let bools = (Array.replicate 48 false)
        bits.CopyTo(bools, 0)
        let unpacked = bools |> Array.map (fun b -> if b then 1 else 0)
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
    let cipher keyPart bits = // the $f$ function
        bits
        |> permutations.E
        |> (fun x -> x.Xor keyPart)
        |> sbox.apply
        |> permutations.P

    let rec iter key n ((L: BitArray), (R: BitArray)) =
        let L' = R
        let keyPart = keys.schedule key n
        let R' = (BitArray L).Xor(cipher keyPart R)

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


let encrypt key block = crypt.crypt (keys.expand key) block

let decrypt key block =
    crypt.crypt (List.rev (keys.expand key)) block
