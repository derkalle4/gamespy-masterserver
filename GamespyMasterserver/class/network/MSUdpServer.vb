'UDP-server wrapper for GM Masterservers
'JW "LeKeks" 05/2014
Imports System.Net.Sockets
Imports System.Threading
Imports System.Net

Public Class MSUdpServer
    Inherits UdpServer

    Public Property CheckPort As UInt16 = 27910

    Sub New(ByVal server As GamespyServer)
        MyBase.New(server)
    End Sub

    Friend Overrides Sub OnDataInput(ByVal data() As Byte, ByVal rIPEP As Net.IPEndPoint)
        MyBase.OnDataInput(data, rIPEP)

        'packet-handling
        Dim p As GamespyUdpPacket
        Select Case data(0)
            Case GS_MASTER_CMD_REGISTER
                p = New ServerRegisterPacket(Me, rIPEP)
            Case GS_MASTER_CMD_HEARTBEAT
                p = New ServerHeartbeatPacket(Me, rIPEP)
            Case GS_MASTER_CMD_CHALLENGE_RES
                p = New ChallengeRequestPacket(Me, rIPEP)
            Case GS_MASTER_CMD_HANDSHAKE_ACK
                p = New HandshakePacket(Me, rIPEP)
            Case GS_MASTER_CMD_AVAILIABLE
                p = New GameAvailiablePacket(Me, rIPEP)
            Case GS_MASTER_CMD_MESSAGE_ACK
                p = New ClientMessagePacket(Me, rIPEP)
            Case Else 'drop any unkown packet
                Logger.Log("Dropping unkown UDP Packet #{0} from {1}", LogLevel.Verbose, data(0).ToString, rIPEP.ToString)
                Return
        End Select
        p.data = data
        p.ManageData()
    End Sub

    '2nd socket for firewall-check
    Private checkClient As UdpClient
    Private checkListenThread As Thread

    Friend Overrides Sub Open()
        If Not Me.running Then
            MyBase.Open()
            Try
                Me.checkClient = New UdpClient(New Net.IPEndPoint(Me.Address, Me.CheckPort))
            Catch ex As Exception
                Logger.Log("Bind failed [{0}:{1}]", LogLevel.Exception, Me.Address.ToString, Me.CheckPort.ToString)
            End Try
            Logger.Log("Bound Firewall-Test socket [{0}:{1}]", LogLevel.Info, Me.Address.ToString, Me.CheckPort.ToString)
            Me.checkListenThread = New Thread(AddressOf Me.waitForCheckResponse)
            Me.checkListenThread.Start()
        End If
    End Sub

    Private Sub waitForCheckResponse()
        While running
            Try
                Dim rIPEP As IPEndPoint = Nothing
                Dim data() As Byte = Me.checkClient.Receive(rIPEP)

                If data.Length > 0 Then
                    Me.OnDataInput(data, rIPEP)
                End If
            Catch ex As Exception
                Logger.Log(ex.ToString, LogLevel.Verbose)
            End Try
            Threading.Thread.Sleep(10)
        End While
    End Sub

    Public Sub sendCheck(ByVal p As GamespyUdpPacket)
        Try
            Dim buf() As Byte = p.CompileResponse
            Me.checkClient.Send(buf, buf.Length, p.RemoteIPEP)
            Logger.Log("Sending to {0}", LogLevel.Verbose, p.RemoteIPEP.ToString)
        Catch ex As Exception
            Logger.Log("Couldn't send UDP-Packet to " & p.RemoteIPEP.Address.ToString & vbCrLf & ex.ToString, LogLevel.Warning)
        End Try
    End Sub

End Class