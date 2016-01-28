'Sapphire II DES ("GS GOA") cipher algorithm
'.NET -Implementation' JW "LeKeks" 9/2014
'Note: Algorithm is designed to operate without overflow-detection!

Module SapphireII
    Public Const LIST_CHALLENGE_LEN As Byte = 8
    Private cards_ascending() As Byte = Nothing

    Public Structure GOACryptState
        Dim cards() As Byte
        Dim rotor As Byte
        Dim ratchet As Byte
        Dim avalanche As Byte
        Dim last_plain As Byte
        Dim last_cipher As Byte
    End Structure

    Public Structure SBServerList
        Dim queryfromkey() As Byte
        Dim mychallenge() As Byte
        Dim cryptkey As GOACryptState
        Dim cryptHeaderOK As Boolean
    End Structure

    Public Sub GOADecryptWrapper(ByRef slist As SBServerList, ByRef data() As Byte, ByVal key() As Byte, ByVal challenge() As Byte)
        'Remove any reference to keep integrity for given arrays
        IsolateBuffer(slist.queryfromkey, key)
        IsolateBuffer(slist.mychallenge, challenge)

        'Check if the Cryptheader was already processed
        If (Not slist.cryptHeaderOK) Then
            Dim keyOffset As Int32 = (Convert.ToByte(data(0)) Xor &HEC) + 2         'Get the offset from the first byte
            Dim keyLen As Int32 = (Convert.ToByte(data(keyOffset - 1))) Xor &HEA    'Get the Key Lenght
            Dim encKey() As Byte = TrimArray(data, keyOffset)   'Trim off the random-header
            InitCryptKey(slist, encKey, keyLen)                 'Call Key initialisation
            data = TrimArray(data, keyOffset + keyLen)          'Trim off the Key
            slist.cryptHeaderOK = True
        End If
        GOADecrypt(slist.cryptkey, data, data.Length)   'Just continue with normal decryption
    End Sub
    Public Sub GOAEncryptWrapper(ByRef slist As SBServerList, ByRef data() As Byte, ByVal key() As Byte, ByVal challenge() As Byte)
        'Remove any reference to keep integrity for given arrays
        IsolateBuffer(slist.queryfromkey, key)
        IsolateBuffer(slist.mychallenge, challenge)

        'Check if the Cryptheader was already build
        If (Not slist.cryptHeaderOK) Then
            Dim headerLen As Int32 = GOAInitCryptHeader(slist, data)    'Generate a new one
            GOAEncrypt(slist.cryptkey, data, data.Length, headerLen)    'Encrypt any data past the header
            slist.cryptHeaderOK = True
        Else
            GOAEncrypt(slist.cryptkey, data, data.Length, 0)    'Just continue with normal encryption
        End If
    End Sub

    Private Sub IsolateBuffer(ByRef ofBuf() As Byte, ByVal inBuf() As Byte)
        'just a memcpy-like-func so we won't write to the source-array
        Array.Resize(ofBuf, inBuf.Length)       'Resize the destination array
        Array.Copy(inBuf, ofBuf, inBuf.Length)  'Copy over the contents
    End Sub

    Private Sub SWAP(ByRef x As Byte, ByRef y As Byte, ByRef swaptemp As Byte)
        'Swaps two bytes by reference, uses static tempstore var so we don't need to allocate more memory
        swaptemp = x
        x = y
        y = swaptemp
    End Sub

    Private Sub GOAInitAscCards()
        'Builds the ascending cards-array
        Array.Resize(cards_ascending, &HFF + 1)
        For i = 0 To &HFF
            cards_ascending(i) = i
        Next
    End Sub
    Private Function GOAInitCryptHeader(ByRef slist As SBServerList, ByRef data() As Byte) As Int32
        Dim key() As Byte = slist.queryfromkey
        Dim validate() As Byte = slist.mychallenge
        Dim cryptHeader(22) As Byte 'Use a static header lenght

        'Create some "randomness" by doing a few fancy bitwise-operations
        Dim rnd As Int64
        For i = 0 To cryptHeader.Length - 1
            rnd = (rnd * &H343FD) + &H269EC3
            cryptHeader(i) = (10 Xor key(i Mod key.Length) Xor validate(i Mod validate.Length)) And &HFF
        Next

        'Copy the cryptkey to continue key initialisation
        Dim cryptKey(cryptHeader.Length - 10) As Byte
        Array.Copy(cryptHeader, 9, cryptKey, 0, cryptKey.Length)
        InitCryptKey(slist, cryptKey, cryptKey.Length)

        'Set Length-Bytes
        cryptHeader(0) = &HEB
        cryptHeader(1) = 0
        cryptHeader(2) = 0
        cryptHeader(8) = &HE4

        'Attach the header to the data
        Array.Resize(data, data.Length + cryptHeader.Length)
        Array.Copy(data, 0, data, cryptHeader.Length, data.Length - cryptHeader.Length)
        Array.Copy(cryptHeader, 0, data, 0, cryptHeader.Length)

        Return cryptHeader.Length
    End Function
    Private Sub GOACryptInit(ByRef state As GOACryptState, ByVal key() As Byte, ByVal keysize As Byte)
        Dim keypos, mask As UInt32
        Dim toswap, swaptemp, rsum As Byte

        'setup our cryptstream
        If (keysize < 1) Then
            GOAHashInit(state)
            Return
        End If

        'Init our static cards if this hasn't been done already
        If cards_ascending Is Nothing Then
            GOAInitAscCards()
        End If

        'Resize the array for the current crypto to fit the base cards
        Array.Resize(state.cards, cards_ascending.Length)
        'Copy the base cards
        Array.Copy(cards_ascending, state.cards, cards_ascending.Length)

        'Setting up the control vars
        toswap = 0
        keypos = 0
        rsum = 0
        mask = 255

        'iterate through the cards and do some pseudo-randomness
        For i = 255 To 1 Step -1
            toswap = keyrand(state, i, mask, key, keysize, rsum, keypos)
            SWAP(state.cards(i), state.cards(toswap), swaptemp)
            If ((i And (i - 1)) = 0) Then
                mask >>= 1
            End If
        Next

        'setup our cryptostream state
        With state
            .rotor = .cards(1)
            .ratchet = .cards(3)
            .avalanche = .cards(5)
            .last_plain = .cards(7)
            .last_cipher = .cards(rsum)
        End With

        'set out control-vars back to 0
        toswap = swaptemp = rsum = 0
        keypos = 0
    End Sub

    Private Sub InitCryptKey(ByRef slist As SBServerList, ByVal key() As Byte, ByVal keylen As Byte)
        Dim seckeylen As Integer = slist.queryfromkey.Length
        Dim seckey() As Byte = slist.queryfromkey

        'Generate out challenge-key from the given sec.
        For i = 0 To keylen - 1
            slist.mychallenge((i * seckey(i Mod seckeylen)) Mod LIST_CHALLENGE_LEN) = _
                slist.mychallenge((i * seckey(i Mod seckeylen)) Mod LIST_CHALLENGE_LEN) Xor _
                Convert.ToByte((slist.mychallenge(i Mod LIST_CHALLENGE_LEN) Xor key(i)) And &HFF)
        Next

        'Call cryptstream initialisation
        GOACryptInit(slist.cryptkey, slist.mychallenge, LIST_CHALLENGE_LEN)
    End Sub

    Private Sub GOAHashInit(ByRef state As GOACryptState)
        'set default-values
        With state
            .rotor = 1
            .ratchet = 3
            .avalanche = 5
            .last_plain = 7
            .last_cipher = 11
        End With

        'Descending cards
        Dim j As Int32 = 255
        For i = 0 To 255
            state.cards(i) = Convert.ToByte(j)
            j -= 1
        Next
    End Sub

    Private Function keyrand(ByRef state As GOACryptState,
                         ByVal limit As UInt32,
                         ByVal mask As UInt32,
                         ByRef user_key() As Byte,
                         ByVal keysize As Byte,
                         ByRef rsum As Byte,
                         ByRef keypos As UInt32) As Byte

        'Generate some pseudo-randomness for our key:

        Dim u, retry_limiter As UInt32

        If limit = 0 Then Return 0

        retry_limiter = 0

        Do
            rsum = state.cards(rsum) + user_key(keypos)
            keypos += 1
            If keypos >= keysize Then
                keypos = 0
                rsum += keysize
            End If

            u = mask And rsum
            retry_limiter += 1
            If (retry_limiter > 11) Then
                u = u Mod limit 'ensure the algorithm won't get stuck here
            End If
        Loop While (u > limit)

        Return Convert.ToByte(u)
    End Function

    Private Sub GOADecrypt(ByRef state As GOACryptState, ByRef bp() As Byte, ByVal len As Int32)
        Dim Rotor As Byte = state.rotor
        Dim Ratchet As Byte = state.ratchet
        Dim Avalanche As Byte = state.avalanche
        Dim Last_plain As Byte = state.last_plain
        Dim Last_cipher As Byte = state.last_cipher
        Dim swaptemp As Byte

        For i = 0 To len - 1
            Ratchet = Convert.ToByte(Ratchet + state.cards(Rotor))
            Rotor += 1

            With state
                swaptemp = .cards(Last_cipher)
                .cards(Last_cipher) = .cards(Ratchet)
                .cards(Ratchet) = .cards(Last_plain)
                .cards(Last_plain) = .cards(Rotor)
                .cards(Rotor) = swaptemp
                Avalanche = Convert.ToByte(Avalanche + .cards(swaptemp))

                Last_plain = Convert.ToByte( _
                           bp(i) Xor _
                           .cards((.cards(Avalanche) + .cards(Rotor)) And &HFF) Xor _
                           .cards(.cards( _
                                  (.cards(Last_plain) + .cards(Last_cipher) + .cards(Ratchet)) _
                                  And &HFF)))
            End With
            Last_cipher = bp(i)
            bp(i) = Last_plain
        Next

        With state
            .rotor = Rotor
            .ratchet = Ratchet
            .avalanche = Avalanche
            .last_plain = Last_plain
            .last_cipher = Last_cipher
        End With
    End Sub

    Private Sub GOAEncrypt(ByRef state As GOACryptState, ByRef bp() As Byte, ByVal len As Int32, ByVal offset As Int32)
        Dim Rotor As Byte = state.rotor
        Dim Ratchet As Byte = state.ratchet
        Dim Avalanche As Byte = state.avalanche
        Dim Last_plain As Byte = state.last_plain
        Dim Last_cipher As Byte = state.last_cipher
        Dim swaptemp As Byte

        For i = offset To len - 1
            Ratchet = Convert.ToByte(Ratchet + state.cards(Rotor))
            Rotor += 1

            With state
                swaptemp = .cards(Last_cipher)
                .cards(Last_cipher) = .cards(Ratchet)
                .cards(Ratchet) = .cards(Last_plain)
                .cards(Last_plain) = .cards(Rotor)
                .cards(Rotor) = swaptemp
                Avalanche = Convert.ToByte(Avalanche + .cards(swaptemp))

                Last_cipher = Convert.ToByte( _
                              bp(i) Xor _
                             .cards((.cards(Avalanche) + swaptemp) And &HFF) Xor _
                             .cards(.cards( _
                                    (.cards(Last_plain) + .cards(Last_cipher) + .cards(Ratchet)) _
                                    And &HFF)))
            End With
            Last_plain = bp(i)
            bp(i) = Last_cipher
        Next

        With state
            .rotor = Rotor
            .ratchet = Ratchet
            .avalanche = Avalanche
            .last_plain = Last_plain
            .last_cipher = Last_cipher
        End With
    End Sub

End Module