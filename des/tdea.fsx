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

let encrypt iv keys blocks =

    let CBC (previous: BitArray) plaintext =
        let xored = (BitArray previous).Xor plaintext
        encryptBlock keys xored

    blocks |> Array.scan CBC iv |> Array.tail

let decrypt iv keys blocks =

    let CBC ((previous: BitArray), _) ciphertext =
        let decrypted = decryptBlock keys ciphertext
        let plaintext = (BitArray previous).Xor decrypted
        (ciphertext, plaintext)

    blocks
    |> Array.scan CBC (iv, iv)
    |> Array.tail
    |> Array.map snd

let cryptBytes crypt iv keys bytes =
    bytes
    |> conv.toBlocks
    |> crypt iv keys
    |> conv.toBytes

let encryptBytes = cryptBytes encrypt
let decryptBytes = cryptBytes decrypt

let encryptString iv keys (string: string) =
    string
    |> System.Text.Encoding.UTF8.GetBytes
    |> encryptBytes iv keys
    |> System.Convert.ToBase64String

let decryptString iv keys (string: string) =
    string
    |> System.Convert.FromBase64String
    |> encryptBytes iv keys
    |> System.Text.Encoding.UTF8.GetString
