module tdea

#if INTERACTIVE
#load "des.fs"
#load "conv.fs"
#endif

open System.Collections

let encryptBlock (k1, k2, k3) block =
    block
    |> des.encrypt k1
    |> des.decrypt k2
    |> des.encrypt k3


let decryptBlock (k1, k2, k3) block =
    block
    |> des.decrypt k3
    |> des.encrypt k2
    |> des.decrypt k1

let encrypt iv keys data =

    let CBC (previous: BitArray) plaintext =
        let xored = (BitArray previous).Xor plaintext
        encryptBlock keys xored

    data
    |> conv.toBlocks
    |> Array.scan CBC iv
    |> Array.tail
    |> conv.toBytes

let decrypt iv keys data =

    let CBC ((previous: BitArray), _) ciphertext =
        let decrypted = decryptBlock keys ciphertext
        let plaintext = (BitArray previous).Xor decrypted
        (ciphertext, plaintext)

    data
    |> conv.toBlocks
    |> Array.scan CBC (iv, iv)
    |> Array.tail
    |> Array.map snd
    |> conv.toBytes
