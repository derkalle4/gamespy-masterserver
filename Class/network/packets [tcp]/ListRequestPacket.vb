'Main Listrequest & Cryptheader-initialisation packet
'JW "LeKeks" 05/2014
Imports System.Reflection
Public Class ListRequestPacket
    Inherits GamespyTcpPacket

    Public Property ParameterArray As String()
    Public Property Filter As String
    Public Property Options As Byte

    Sub New(ByVal client As GamespyClient, ByVal data() As Byte)
        MyBase.New(client, data)
        Me.UseCipher = True                                  'turn on encryption
        Me.Filter = FetchString(Me.data)                     'Fetch the filter-string
        Me.ParameterArray = Split(FetchString(Me.data), "\") 'Get the requested params
        'TODO: might be casted to int32, however doesn't match std. C-style int32-format (LE?)
        Me.Options = Me.data(bytesParsed + 3) 'BitConverter.ToInt32(Me.data, Me.bytesParsed)
    End Sub

    Public Overrides Sub ManageData()
        Me.client.send(Me)
    End Sub

    Private Function BuildServerArray() As Byte()
        'TODO: Implement Serverside filtering
        Dim buffer() As Byte = {}

        'Header
        ConcatArray(Me.client.RemoteIPEP.Address.GetAddressBytes(), buffer)
        'ConcatArray({25, 100}, buffer) 'TODO: fetch port from db
        ConcatArray(BuildInvertedUInt16Array(6501), buffer)
        'checking query state via bitwise and

        'If Me.Options And GS_FLAG_SEND_GROUPS Then
        'grouped server query (EaW p.e.)
        'ConcatArray({Me.ParameterArray.Length - 1, 0}, buffer)
        'ConcatArray(Me.BuildParameterArray(), buffer)

        'Dim flags As Byte
        'flags = flags Or GS_FLAG_UNSOLICITED_UDP
        'flags = flags Or GS_FLAG_HAS_FULL_RULES
        'flags = flags Or GS_FLAG_HAS_KEYS

        'ConcatArray({flags}, buffer)
        'ConcatArray({127, 0, 0, 1}, buffer) 'empty ipa
        'PushString(buffer, "LEKEKS")
        'PushString(buffer, "1")
        'PushString(buffer, "10")
        'PushString(buffer, "4")
        'PushString(buffer, "3")
        'PushString(buffer, "0")
        'PushString(buffer, "1", False)

        'ConcatArray({&H0, &HFF, &HFF, &HFF, &HFF}, buffer)

        'Logger.Log("Grouping data values for {0}", LogLevel.Verbose, Me.client.GameName)
        If Not (Me.Options = GS_FLAG_NO_SERVER_LIST) Then 'Me.ParameterArray.Count > 1 Then
            ConcatArray({Me.ParameterArray.Length - 1, 0}, buffer)
            ConcatArray(Me.BuildParameterArray(), buffer)

            Dim servers As List(Of GamespyGameserver) = Me.client.server.MySQL.GetServers(Me.client.GameName, Me.client.server.Config.GameserverTimeout)
            Logger.Log("Fetched {0} active servers from database. ({1})", LogLevel.Verbose, servers.Count.ToString)

            For Each server As GamespyGameserver In servers
                If server.ChallengeOK = False Then Continue For 'don't list unauthenticated servers
                Me.BuildServerEntry(server, buffer)
            Next
            ConcatArray({&H0, &HFF, &HFF, &HFF, &HFF}, buffer) 'set last bytes, \xFF\xFF\xFF\xFF indicates last server
        Else
            Logger.Log("Sending header to {0}.", LogLevel.Verbose, Me.client.RemoteIPEP.ToString())
        End If

        Return buffer
    End Function

    Private Sub BuildServerEntry(ByVal server As GamespyGameserver, ByRef buffer() As Byte)
        If server.PortClosed And Me.ParameterArray.Length < 2 Then Return
        Dim serverFlags As Byte = 0
        Dim ip0 As Net.IPAddress = Nothing

        Dim hasLocalIP As Boolean = Net.IPAddress.TryParse(server.GetValue("localip0"), ip0)

        ToggleFlag(serverFlags, GS_FLAG_CONNECT_NEGOTIATE_FLAG)
        ToggleFlag(serverFlags, GS_FLAG_NONSTANDARD_PORT)

        If (Me.Options = GS_FLAG_SEND_FIELDS_FOR_ALL) Then
            ToggleFlag(serverFlags, GS_FLAG_PRIVATE_IP)
            ToggleFlag(serverFlags, GS_FLAG_NONSTANDARD_PRIVATE_PORT)
            ToggleFlag(serverFlags, GS_FLAG_HAS_KEYS)
            ToggleFlag(serverFlags, GS_FLAG_ICMP_IP)
        Else
            If server.IsNatted And Not server.PortClosed Then '85
                ToggleFlag(serverFlags, GS_FLAG_HAS_KEYS)
                ToggleFlag(serverFlags, GS_FLAG_UNSOLICITED_UDP)

            ElseIf server.PortClosed Then '126
                ToggleFlag(serverFlags, GS_FLAG_PRIVATE_IP)
                ToggleFlag(serverFlags, GS_FLAG_NONSTANDARD_PRIVATE_PORT)
                ToggleFlag(serverFlags, GS_FLAG_HAS_KEYS)
                ToggleFlag(serverFlags, GS_FLAG_ICMP_IP)

            Else '21
                ToggleFlag(serverFlags, GS_FLAG_UNSOLICITED_UDP)
            End If

        End If

        'Don't accept direct Querys for homeservers, they'll only slow down the SBQEngine 
        ConcatArray({serverFlags}, buffer)

        'TODO: add compatibility for peerchat-lobbys
        'This implementation is critical: changing to the localip will cause wrong hash-calculations
        'for the peerchat lobby-system -> maybe detect peerchat games
        If server.PublicIP = Me.client.RemoteIPEP.Address.ToString And server.IsNatted And Not server.PortClosed And hasLocalIP Then
            ConcatArray(ip0.GetAddressBytes, buffer)
            'TODO: hostport might be wrong, check for localport instead
            ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(UInt16.Parse(server.HostPort)), buffer)
        Else
            ConcatArray(Net.IPAddress.Parse(server.PublicIP).GetAddressBytes, buffer)       'IP-Address
            ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(UInt16.Parse(server.PublicPort)), buffer)  'Port
        End If

        If serverFlags And GS_FLAG_PRIVATE_IP Then   'Attach natneg-params
            If Not hasLocalIP Then Return
            Dim lport As UInt16 = server.PublicPort
            UInt16.TryParse(server.GetValue("localport"), lport)
            ConcatArray(ip0.GetAddressBytes(), buffer)
            ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(lport), buffer)
        End If

        If serverFlags And GS_FLAG_ICMP_IP Then
            ConcatArray(Net.IPAddress.Parse(server.PublicIP).GetAddressBytes, buffer)
        End If

        'Attaching server details (for servers which aren't queried directly)
        If serverFlags And GS_FLAG_HAS_KEYS Then
            ConcatArray({255}, buffer) 'Attach a delimeter indicating that there's new data here
            For i = 1 To Me.ParameterArray.Length - 1 'Try to attach every desired param
                Dim val As String = server.GetValue(Me.ParameterArray(i))
                Me.PushString(buffer, val, (i = Me.ParameterArray.Length - 1))
            Next
        End If

    End Sub

    Private Sub ToggleFlag(ByRef dest As Byte, ByVal flag As Byte)
        dest = dest Or flag
    End Sub


    Private Sub PushString(ByRef data() As Byte, str As String, Optional ByVal isLast As Boolean = False)
        ConcatArray(GetBytes(str), data)
        If isLast Then
            ConcatArray({0}, data) 'don't attach 0xFF on the last one
        Else
            ConcatArray({0, &HFF}, data) '0xFF: delimeter
        End If
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Logger.Log("Sending Serverlist", LogLevel.Verbose)
        Dim buffer() As Byte = BuildServerArray()   'Write serverlist into a buffer
        Return buffer
    End Function

    Private Function BuildParameterArray() As Byte()
        Dim buffer() As Byte = {}
        For Each Str As String In ParameterArray
            If Str <> String.Empty Then             'Don't attach empty params
                ConcatArray(GetBytes(Str), buffer)
                ConcatArray({0, 0}, buffer)         'Delimeter is \x0\x0 for this one
            End If
        Next
        Return buffer
    End Function
End Class