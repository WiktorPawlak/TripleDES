module des

open System.Collections

#if INTERACTIVE
#load "tables.fs"
#load "conv.fs"
#endif

module crypt =


    module permutations =
        let perm len locations (bits: BitArray) =
            let ans = BitArray(Array.replicate len 0uy)

            for (loc, old) in (List.indexed locations) do
                ans.Set(loc, (bits.Get old))

            ans

        let initial = perm 8 tables.initial
        let reverse = perm 8 tables.reverse
        let P = perm 4 tables.P
        let PC2 = perm 6 tables.PC2
        let PC1 = perm 7 tables.PC1
        let E = perm 6 tables.E

        let rec keyShift (input: BitArray) count =
            // ABCD EFGH --> BCDA FGHE
            let output = BitArray input
            output.RightShift(1) |> ignore // ¯\_(ツ)_/¯
            let first = input.Get 0
            let second = input.Get 28
            output.Set(27, first)
            output.Set(55, second)

            match count with
            | 1 -> output
            | _ -> keyShift output (count - 1)


    let expandKey key =
        // key --> subkey lookup table
        let reduced = permutations.PC1 key

        let items =
            List.scan permutations.keyShift reduced tables.shiftcount
            |> List.map permutations.PC2

        items.Tail


    let keySchedule (key: list<BitArray>) n = key.Item(n - 1)


    let S (n, addr) =
        let table = tables.Sraw[n]
        table[addr]

    let makeSAddress bits =
        let unpacked = bits |> Array.map (fun b -> if b then 1 else 0)

        let i = unpacked[0] * 2 + unpacked[5]

        let j =
            unpacked[1] * 8
            + unpacked[2] * 4
            + unpacked[3] * 2
            + unpacked[4]

        i * 16 + j



    let cipher (keyPart: BitArray) (bits: BitArray) = // the $f$ function
        let parts = (permutations.E bits).Xor keyPart

        parts
        |> conv.toSixBitPieces
        |> Array.map makeSAddress
        |> Array.indexed
        |> Array.map S
        |> Array.rev // tutaj działa
        |> conv.ConcatenateFourBitPieces
        |> permutations.P

    let rec cryptIter key n ((L: BitArray), (R: BitArray)) =
        let L' = R
        let keyPart = keySchedule key n
        let R' = (BitArray L).Xor(cipher keyPart R)

        match n with // List.fold?
        | 16 -> (R', L')
        | _ -> cryptIter key (n + 1) (L', R')



    let cryptBlock key block =
        block
        |> permutations.initial
        |> conv.split
        |> cryptIter key 1
        |> conv.join
        |> permutations.reverse



    let encryptBlock key block = cryptBlock (expandKey key) block

    let decryptBlock key block =
        cryptBlock (List.rev (expandKey key)) block
