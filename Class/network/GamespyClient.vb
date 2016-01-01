'TCP-Client management class
'JW "LeKeks" 05/2014

Imports System.Net.Sockets
Imports System.Threading

Public Class GamespyClient
    Public Property server As GamespyServer         'Ref. to serverobj.
    Public Property RemoteIPEP As Net.IPEndPoint    'client's remote endpoint

    Private workthread As Thread    'workthread
    Private client As TcpClient     'client
    Private stream As NetworkStream 'tcp-stream
    Private running As Boolean      'true if client is ok
    Private initOk As Boolean       'used to remember if the header was processed

    Private slist As SBServerList   'used to store the cryptstream state

    Public Property CryptVersion As Byte
    Public Property ProtocolVersion As Byte
    Public Property ServerKey As Byte() = {}
    Public Property ChallengeKey As Byte() = {}
    Public Property GameName As String = String.Empty
    Public Property QueryName As String = String.Empty

    Public Event ConnectionLost(ByVal sender As GamespyClient)  'Used to pass connection loss to server

    Sub New(ByVal server As GamespyServer, client As TcpClient)
        Me.server = server
        Me.running = True
        Me.stream = client.GetStream()
        Me.client = client
        Me.RemoteIPEP = DirectCast(client.Client.RemoteEndPoint, Net.IPEndPoint)
        Me.workthread = New Threading.Thread(AddressOf Me.listen)
        Me.workthread.Start()
    End Sub

    Private Sub Listen()
        Try

            stream.ReadTimeout = TCP_CLIENT_TIMEOUT
            Dim buffer() As Byte = Nothing

            While Me.ReadTcpStream(Me.stream, buffer) And Me.running

                If Not Me.HandlePacket(buffer) Then
                    Exit Try
                End If

                Threading.Thread.Sleep(TCP_CLIENT_PSH_SLEEP)
            End While

            If Me.running Then
                Logger.Log("{0}: Reached end of stream - FIN", LogLevel.Verbose, Me.RemoteIPEP.ToString)
            End If

        Catch ex As Exception

            If ex.InnerException Is Nothing Then
                Logger.Log("{0}: FIN: caused an Exception:" & vbCrLf & ex.ToString, LogLevel.Info, Me.RemoteIPEP.ToString())
            ElseIf ex.InnerException.GetType().IsEquivalentTo(GetType(SocketException)) Then
                Dim se As SocketException = DirectCast(ex.InnerException, SocketException)

                Select Case se.ErrorCode
                    Case SocketError.TimedOut
                        Logger.Log("{0}: read timed out after {1} seconds", LogLevel.Verbose, Me.RemoteIPEP.ToString, (stream.ReadTimeout / 1000).ToString())
                    Case SocketError.ConnectionAborted
                        Logger.Log("{0}: sent FIN -> closing connection", LogLevel.Verbose, Me.RemoteIPEP.ToString)
                    Case Else
                        Logger.Log("{0}: connection jammed - closing", LogLevel.Verbose, Me.RemoteIPEP.ToString)
                End Select
            Else
                Logger.Log("{0}: FIN: caused an Exception:" & vbCrLf & ex.ToString, LogLevel.Info, Me.RemoteIPEP.ToString())
            End If
        End Try

        Me.Dispose()
    End Sub

    Public Sub Dispose()
        On Error Resume Next 'continue disposing the object in any case
        Me.running = False   'client is no longer operational
        Me.stream.Close()    'close stream
        Me.client.Close()    'send FIN-ACK
        Me.stream = Nothing  'kill stream-ref
        Me.client = Nothing  'kill client-ref
        Me.server = Nothing  'kill server-ref
        Me.slist = Nothing   'kill header-ref
        RaiseEvent ConnectionLost(Me) 'throw event
    End Sub

    Private Function ReadTcpStream(ByVal stream As Net.Sockets.NetworkStream, ByRef buffer() As Byte) As Boolean
        buffer = New Byte(1) {}

        Dim bufferLen As Int32 = 0     'Bytes expected
        Dim bufferRead As Int32 = 0    'Bytes read

        'Read length header
        bufferRead = stream.Read(buffer, 0, 2)
        If (bufferRead = 0) Then Return False
        bufferLen = ArrayFunctions.GetInvertedUInt16(buffer, 0) - 2
        If bufferLen < 0 Then Return False

        'Read packet body
        Array.Resize(buffer, bufferLen)
        bufferRead = stream.Read(buffer, 0, bufferLen)
        If (bufferRead = 0) Then Return False

        'Limit the number of pushs to prevent DoS
        Dim readCount As Int32 = 0

        'Check for fragmented data
        While (bufferRead < bufferLen And readCount < TCP_CLIENT_PSH_MAXCOUNT)
            Logger.Log("{0}:  waiting for next PSH", LogLevel.Verbose, Me.RemoteIPEP.ToString())

            'Wait for the next push
            bufferLen -= bufferRead 'remaining data len
            bufferRead = stream.Read(buffer, bufferRead, bufferLen)
            readCount += 1
            Thread.Sleep(TCP_CLIENT_PSH_SLEEP)
        End While

        'Check if the client exceeded max. push packets
        If (readCount = TCP_CLIENT_PSH_MAXCOUNT) Then
            Logger.Log("{0}: too much PSHs - dropping client", LogLevel.Verbose, Me.RemoteIPEP.ToString())
            Return False
        End If

        Logger.Log("{0}: fetched {1} bytes from stream", LogLevel.Verbose, Me.RemoteIPEP.ToString(), bufferLen.ToString())
        Return True
    End Function

    Private Function HandlePacket(ByVal buffer() As Byte) As Boolean
        Dim packetID As Byte = buffer(0)
        Dim packet As GamespyTcpPacket = Nothing

        'client init
        If packetID = GS_MS_CLIENT_CMD_LIST_REQ And Not initOk Then
            If Not Me.InitializeClient(buffer) Then Return False
        End If

        Select Case packetID
            Case GS_MS_CLIENT_CMD_LIST_REQ
                packet = New ListRequestPacket(Me, buffer)
                Logger.Log("Fetched packet #{0} (server list request)", LogLevel.Verbose, packetID.ToString)

            Case GS_MS_CLIENT_CMD_SERVERINFO
                packet = New ServerinfoRequestPacket(Me, buffer)
                Logger.Log("Fetched packet #{0} (server info request)", LogLevel.Verbose, packetID.ToString)

            Case GS_MS_CLIENT_CMD_MESSAGE
                packet = New MessagePacket(Me, buffer)
                Logger.Log("Fetched packet #{0} (message forward request)", LogLevel.Verbose, packetID.ToString)

            Case GS_MS_CLIENT_CMD_KEEPALIVE
                Logger.Log("Fetched packet #{0} (keepalive)", LogLevel.Verbose, packetID.ToString)

            Case GS_MS_CLIENT_CMD_MAPLOOP
                Logger.Log("Fetched packet #{0} (maploop request)", LogLevel.Verbose, packetID.ToString)

            Case GS_MS_CLIENT_CMD_PLAYERSEARCH
                Logger.Log("Fetched packet #{0} (playersearch request)", LogLevel.Verbose, packetID.ToString)

            Case Else
                Logger.Log("Dropping unknown TCP packet (#{0})", LogLevel.Verbose, packetID.ToString)
        End Select

        If Not packet Is Nothing Then
            packet.PacketId = packetID
            packet.ManageData()
        End If

        Return True
    End Function

    Public Sub send(ByVal packet As GamespyTcpPacket)
        Dim data() As Byte = packet.CompileResponse()

        If packet.UseCipher Then
            'Encrypt the packet if there's any encryption set up
            If slist.cryptHeaderOK = True Or (Me.ChallengeKey.Length > 0 And Me.ServerKey.Length > 0) Then
                SapphireII.GOAEncryptWrapper(Me.slist, data, Me.ServerKey, Me.ChallengeKey)
            Else
                Logger.Log("{0}: cipher required but no valid header sent", LogLevel.Info, Me.RemoteIPEP.ToString())
            End If
        End If

        Me.stream.Write(data, 0, data.Length)
        Me.stream.Flush()
    End Sub

    Private Function InitializeClient(ByRef buffer() As Byte) As Boolean
        Dim offset As UInt16 = 0

        'fetch protocol info
        Me.ProtocolVersion = buffer(1)
        Me.CryptVersion = buffer(2)

        offset += 3 + 4 'skip 4-byte flag

        'fetch gamenames
        Me.GameName = ArrayFunctions.GrabZeroDelimetedString(buffer, offset)
        Me.QueryName = ArrayFunctions.GrabZeroDelimetedString(buffer, offset)

        'check if there's a valid challenge
        If buffer.Length - offset < 8 Then
            Logger.Log("{0}: Invalid crypt header - dropping client", LogLevel.Verbose, Me.RemoteIPEP.ToString)
            Return False
        End If

        'copy challenge key
        Array.Resize(Me.ChallengeKey, GS_CRYPT_CHALLENGELEN)
        Array.Copy(buffer, offset, Me.ChallengeKey, 0, GS_CRYPT_CHALLENGELEN)

        'fetch server's cryptkey
        Me.ServerKey = ArrayFunctions.GetBytes(Me.server.MySQL.FetchServerKey(Me.GameName))
        offset += GS_CRYPT_CHALLENGELEN

        If ServerKey.Length = 0 Then
            Logger.Log("{0}: unknown game '{1}' - dropping client", LogLevel.Verbose, Me.RemoteIPEP.ToString, Me.GameName)
            Return False
        End If

        'remove the init-header to continue normal data processing
        buffer = ArrayFunctions.TrimArray(buffer, offset - 1)

        Me.initOk = True
        Return True
    End Function

End Class