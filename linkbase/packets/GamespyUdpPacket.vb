'UDP-Packet Base class
'JW "LeKeks" 04/2014
Public Class GamespyUdpPacket
    Inherits PacketBase

    Public Property Server As UdpServer
    Public Property GSServer As GamespyServer

    Public Property ClientUUID As Byte() = {}

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        Me.Server = server
        Me.GSServer = server.Server
        Me.RemoteIPEP = remoteIPEP
    End Sub

    Friend Sub SetupIDs(Optional ByVal packet As GamespyUdpPacket = Nothing)
        If Not packet Is Nothing Then
            Array.Resize(Me.ClientUUID, 4)
            Array.Copy(packet.data, 1, Me.ClientUUID, 0, Me.ClientUUID.Length)
        Else
            bytesParsed = 1
            Array.Resize(Me.ClientUUID, 4)
            Array.Copy(Me.data, Me.bytesParsed, Me.ClientUUID, 0, Me.ClientUUID.Length)
            bytesParsed += 4
        End If
    End Sub

End Class