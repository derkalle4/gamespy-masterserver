'TCP-server class for MS-services
'JW "LeKeks" 05/2014
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Public Class TcpServer
    Public Property Address As IPAddress    'Bind Address
    Public Property Port As Int32 = 28910   'Bind Port

    Private listenThread As Thread      'mainthread
    Private listener As TcpListener     'TCP-Listener
    Private running As Boolean          'true if server is up

    'Events to pass to the Clientmanager to drop / add clients
    Public Event ClientConnected(ByVal sender As TcpServer, ByVal client As TcpClient)
    Public Event BindFailed(ByVal sender As TcpServer, ByVal ex As Exception)

    Public Sub Open()
        If Not running Then
            Try
                'Create and launch the Listener
                listener = New TcpListener(New Net.IPEndPoint(Me.Address, Me.Port))
                listener.Start()
            Catch ex As Exception
                'Seems like we can't open that socket
                Logger.Log("Bind failed [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Exception)
                RaiseEvent BindFailed(Me, ex)
            End Try
            'Do the loop in an extra Thread
            Me.listenThread = New Thread(AddressOf Me.Listen)
            Me.listenThread.Start()
            running = True 'and we're up
            Logger.Log("TCP Listener started [" & Me.Address.ToString & ":" & Me.Port & "]", LogLevel.Info)
        End If
    End Sub

    Public Sub Close()
        'The thread will exit if running is set to false
        If running Then
            running = False
            Me.listener.Stop()
        End If
    End Sub

    Private Sub Listen()
        While running
            Try
                'Check if there's a new client
                If listener.Pending = True Then
                    'Create a Client for the connection
                    Dim client As TcpClient = listener.AcceptTcpClient()
                    'Throw the event
                    Me.OnClientConnect(client)
                End If
            Catch ex As Exception
                'Just to ensure the thread can't get stuck
                Logger.Log("Listener Error! " & vbCrLf & ex.ToString, LogLevel.Warning)
            End Try
            'some basic delay to limit CPU-usage and prevent DoS
            Threading.Thread.Sleep(10)
        End While
    End Sub

    Friend Overridable Sub OnClientConnect(ByVal client As TcpClient)
        Logger.Log("Client connected: {0}", LogLevel.Verbose, client.Client.RemoteEndPoint.ToString)
        RaiseEvent ClientConnected(Me, client)
    End Sub

End Class