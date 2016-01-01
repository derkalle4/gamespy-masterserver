Public Class P2PUdpPacket
    Inherits PacketBase
    Public Property Server As P2PServerHandler

    Public Property GSServer As GamespyServer

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        Me.Server = server
        Me.GSServer = server.Server
        Me.RemoteIPEP = remoteIPEP
        Me.bytesParsed = 0
    End Sub
End Class