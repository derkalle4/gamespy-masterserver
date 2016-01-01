Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Public Class UdpServer

    Public Property Address As IPAddress
    Public Property Port As Int32 = 27900

    Friend listenThread As Thread
    Friend client As UdpClient
    Friend running As Boolean

    Public Property Server As GamespyServer

    Sub New(ByVal server As GamespyServer)
        Me.Server = server
    End Sub
    Friend Overridable Sub Open()
        If Not running Then
            Try
                client = New UdpClient(New Net.IPEndPoint(Me.Address, Me.Port))
            Catch ex As Exception
                Logger.Log("Bind failed [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Exception)
            End Try
            running = True
            Me.listenThread = New Thread(AddressOf Me.Listen)
            Me.listenThread.Start()
            Logger.Log("UDP Listener started [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Info)
        End If
    End Sub
    Friend Overridable Sub Close()
        If running Then
            running = False
            Me.client.Close()
        End If
    End Sub

    Private Sub Listen()
        While running
            Try
                Dim rIPEP As IPEndPoint = Nothing
                Dim data() As Byte = Me.client.Receive(rIPEP)

                If data.Length > 0 Then
                    Me.OnDataInput(data, rIPEP)
                End If
            Catch ex As Exception
                Logger.Log(ex.ToString, LogLevel.Verbose)
            End Try
            Threading.Thread.Sleep(10)
        End While
    End Sub

    Friend Overridable Sub OnDataInput(ByVal data() As Byte, ByVal rIPEP As IPEndPoint)
        Logger.Log("Fetched " & data.Count & " Bytes from " & rIPEP.Address.ToString & " [UDP]", LogLevel.Verbose)
    End Sub

    Public Overridable Sub send(ByVal p As PacketBase)
        Try
            Dim buf() As Byte = p.CompileResponse()
            Me.client.Send(buf, buf.Length, p.RemoteIPEP)
            Logger.Log("Sending to  " & p.RemoteIPEP.ToString, LogLevel.Verbose)
        Catch ex As Exception
            Logger.Log("Couldn't send UDP-Packet to " & p.RemoteIPEP.Address.ToString & vbCrLf & ex.ToString, LogLevel.Warning)
        End Try
    End Sub


    'Debug-Method
    Public Sub InjectPacket(ByVal buf() As Byte, rIPEP As IPEndPoint)
        Me.OnDataInput(buf, rIPEP)
    End Sub
End Class