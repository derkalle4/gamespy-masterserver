'Used to store "Tables" from the server-heartbeat
Public Structure ElementTable
    Dim header() As String
    Dim data(,) As String
    Dim rows As Byte
End Structure

'Used to store a pair of data from the server-heartbeat
Public Structure DataPair
    Dim varName As String
    Dim value As String
End Structure

'Used for Multimasterserver P2P-com
Public Structure MasterServer
    Dim rIPEP As Net.IPEndPoint
    Dim msName As String
    Dim id As Int32
End Structure