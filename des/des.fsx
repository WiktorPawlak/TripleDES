﻿module des

open System.Collections

#if INTERACTIVE
#load "tables.fs"
#load "conv.fs"
// #load "debug.fs"
#endif

module crypt =


    module permutations =
        let E (bits: BitArray) = // separate from perm for no good reason
            let ans = BitArray(Array.replicate 6 0uy) // 48 bits

            for (loc, old) in (List.indexed tables.E) do
                ans.Set(loc, (bits.Get old))

            ans

        let PC1 (key: BitArray) = // 8 -> 7 bytes
            let ans = BitArray(Array.replicate 7 0uy) // 56 bits

            for (loc, old) in (List.indexed tables.PC1) do
                ans.Set(loc, (key.Get old))

            ans

        let PC2 (key: BitArray) = // 7 -> 6 bytes
            let ans = BitArray(Array.replicate 6 0uy) // 48 bits

            for (loc, old) in (List.indexed tables.PC2) do
                ans.Set(loc, (key.Get old))

            ans


        let perm locations (bits: BitArray) =
            let ans = BitArray bits

            for (loc, old) in (List.indexed locations) do
                ans.Set(loc, (bits.Get old))

            ans

        let initial = perm tables.initial
        let reverse = perm tables.reverse
        let P = perm tables.P

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
        printf "iteracja %i\t" n
        let L' = R
        let keyPart = keySchedule key n
        let R' = (BitArray L).Xor(cipher keyPart R)
        // printf "%s %s %s\n" (debug.toStr L') (debug.toStr R') (debug.toStr keyPart)

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
