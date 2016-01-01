'Gameserver-Management Class
'JW "LeKeks" 05/2014
Imports System.Reflection
Public Class GamespyGameserver
    Public Const BYTE_ZERO As Byte = 0
    Private doubleNull As String = ArrayFunctions.GetString({BYTE_ZERO, BYTE_ZERO})
    Private null As String = ArrayFunctions.GetString({BYTE_ZERO})

#Region "Server Properties"
    'Generic Parameters for quick access
    Public Property InternalId As Int32 = -1
    Public Property ServerProtocol As String = "gamespy2"

    Public Property PublicIP As String = "0"
    Public Property PublicPort As String = "0"
    Public Property HostPort As String = "0"

    Public Property MaxPlayers As String = "0"
    Public Property NumPlayers As String = "0"

    Public Property Hostname As String = "?"
    Public Property MapName As String = "?"
    Public Property GameType As String = "?"
    Public Property GameMode As String = "?"
    Public Property Password As String = "0"

    Public Property GameVer As String = "?"

    Public Property Session As String = "0"
    Public Property PrevSession As String = "0"
    Public Property ServerType As String = "0"

    Public Property StateChanged As String = "0"
    Public Property GameName As String = "?"

    Public Property ChallengeOK As Boolean = False '-> Server scheint OK zu sein
    Public Property HandshakeOK As Boolean = False '-> Port ist offen
    Public Property ClientID As Int32

    Public Property LastHeartbeat As Date
    Public Property HeartBeatToken() As Byte() = {}

    Private playerTable As ElementTable

    Public ReadOnly Property PortClosed As Boolean
        Get
            Return Not Me.HandshakeOK
        End Get
    End Property

    'Dynamic Field Storage
    Public Property DynamicStorage As DynamicFieldStorage
#End Region

    Sub New()
        Me.DynamicStorage = New DynamicFieldStorage()
    End Sub

    Public Function GetPlayers() As List(Of GamespyPlayer)
        If Me.playerTable.data Is Nothing Then Return Nothing
        Dim pt As ElementTable = Me.playerTable
        Dim res As New List(Of GamespyPlayer)

        For i = 0 To pt.rows - 1
            Dim p As New GamespyPlayer
            For j = 0 To pt.header.Length - 1
                p.SetProperty(pt.header(j), pt.data(i, j))
            Next
            res.Add(p)
        Next

        Return res
    End Function
    Public Function CheckDataIntegrity() As Boolean
        Return Not (Me.GameName = "?")
    End Function
    Public Function GetChallengeToken() As Byte()
        If Me.HeartBeatToken.Length <> 6 Then
            Array.Resize(Me.HeartBeatToken, 6)
            Dim r As New Random
            r.NextBytes(Me.HeartBeatToken)
            r = Nothing
        End If
        'Me.HeartBeatToken = System.Text.Encoding.ASCII.GetBytes("XXi$gB")
        Return Me.HeartBeatToken()
    End Function
    Public ReadOnly Property IsNatted As Boolean
        Get
            For i = 0 To PARAM_MAX_LOCALIP_SEEK
                Dim localIP As String = Me.DynamicStorage.GetValue("localip" & i.ToString())
                If localIP = String.Empty Then
                    Exit For
                ElseIf Not IsLocalIp(localIP) Then
                    Return False
                End If
            Next
            Return True
        End Get
    End Property

#Region "Property-Storage"
    'Main method for parsing the incoming byte-array from the UDP-heartbeat
    Public Sub ParseParameterArray(ByVal buf() As Byte)
        Try
            Dim parameterArray() As Byte = Nothing
            'Start by splitting the source array if there's a \x0\x0\x0 delimeter
            'which is caused by attaching at least 3 C-style strings
            SliceArray(buf, parameterArray, {BYTE_ZERO, BYTE_ZERO, BYTE_ZERO})
            'process the parameter-array
            Me.ParseParameters(parameterArray)

            'Rest of buf contains dynamic table data
            Dim offset As Int32 = 0

            'Process the datatables in a loop
            While offset < buf.Length
                Dim tbl As ElementTable = Nothing
                offset = ManageTableData(buf, tbl, offset)
                If Not tbl.header Is Nothing Then
                    If Not tbl.header.Length > 0 Then Exit While

                    If tbl.header(0) = "player_" Then
                        Me.playerTable = tbl
                    Else
                        Me.DynamicStorage.AttachDataTable(tbl)
                    End If
                Else 'Exit on broken Table
                    Exit While
                End If
            End While
            'the server sent something so it's still alive
            Me.LastHeartbeat = Now
        Catch ex As Exception
            Logger.Log("Parse Error: Unable to manage Parameter-Array ({0} Bytes) from {1}:{2}", LogLevel.Warning, buf.Length.ToString(), Me.PublicIP, Me.PublicPort)
        End Try
    End Sub
    Private Function ManageTableData(ByRef arr() As Byte, ByRef tbl As ElementTable, ByVal offset As Int32) As Int32
        Dim rowCount As Byte = arr(offset)
        Dim index As Int32 = offset + 1 'Skip length byte
        Dim header() As String = {}

        While index < (arr.Length - 1)
            Array.Resize(header, header.Length + 1)
            header(header.Length - 1) = GrabZeroDelimetedString(arr, index)
            If arr(index) = 0 Then Exit While
        End While
        index += 1 'Add +1 to compensate the x0 x0 delimeter

        Dim data(rowCount - 1, header.Length - 1) As String

        For i = 0 To rowCount - 1
            For j = 0 To header.Length - 1
                data(i, j) = GrabZeroDelimetedString(arr, index)
            Next
        Next
        index += 1 'Add +1 to compensate the x0 x0 delimeter

        tbl.rows = rowCount
        tbl.header = header
        tbl.data = data

        Return index
    End Function
    Private Sub ParseParameters(ByVal buf() As Byte)
        'Create a correct reference to ensure that "0 == 0"
        Dim str As String = ArrayFunctions.GetString(buf)

        Dim part0Params() As String = Split(str, null)
        For i = 0 To part0Params.Length - 1 Step 2
            If i + 1 < part0Params.Length Then
                Me.PushVar(part0Params(i), part0Params(i + 1))
            End If
        Next
    End Sub

    Private Sub PushVar(ByVal name As String, ByVal value As String)
        Dim propInfo As PropertyInfo = Me.GetType().GetProperty(name, BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.IgnoreCase)
        If Not propInfo Is Nothing Then
            propInfo.SetValue(Me, value)
        Else
            Me.DynamicStorage.PushValue(name, value)
        End If
    End Sub

    Public Function GetValue(ByVal varName As String) As String
        Dim propInfo As PropertyInfo = GetType(GamespyGameserver).GetProperty(varName, BindingFlags.Public + BindingFlags.Instance + BindingFlags.IgnoreCase)
        Dim val As String = String.Empty
        If propInfo Is Nothing Then
            'Search the dynamic params
            val = Me.DynamicStorage.GetValue(varName)
            If val = String.Empty Then
                Logger.Log("{0} is not set!", LogLevel.Warning, varName)
                val = PARAM_UNKNOWN_REPLACEMENT 'Replace the property with at least "something"
            End If
        Else
            val = propInfo.GetValue(Me)
        End If
        Return val
    End Function

    Private Function IsLocalIp(ByVal addr As String) As Boolean
        Dim ipAddr As Net.IPAddress = Nothing
        If Not Net.IPAddress.TryParse(addr, ipAddr) Then Return True
        Dim addrBuf() As Byte = ipAddr.GetAddressBytes()
        'TODO: implement proper local ip-range detection
        Return addrBuf(0) = 10 Or addrBuf(0) = 169 Or (addrBuf(0) = 192 And addrBuf(1) = 168) Or addrBuf(0) = 127
    End Function
#End Region

End Class
