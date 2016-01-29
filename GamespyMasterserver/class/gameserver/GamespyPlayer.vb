'Player-Management Class
'JW "LeKeks" 09/2014
Imports System.Reflection
Public Class GamespyPlayer
    Public Property player_ As String
    Public Property score_ As Int32
    Public Property deaths_ As Int32
    Public Property ping_ As Int32
    Public Property team_ As Byte
    Public Property kills_ As Int32

    Public Sub SetProperty(ByVal propName As String, ByVal value As String)
        Dim propInfo As PropertyInfo = GetType(GamespyPlayer).GetProperty(propName, BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.IgnoreCase)
        If Not propInfo Is Nothing Then
            Select Case propInfo.PropertyType
                Case GetType(Int32)
                    Dim i As Int32 = 0
                    If Not Int32.TryParse(value, i) Then Return
                    propInfo.SetValue(Me, i)
                Case GetType(Byte)
                    Dim b As Byte = 0
                    If Not Byte.TryParse(value, b) Then Return
                    propInfo.SetValue(Me, b)
                Case GetType(String)
                    propInfo.SetValue(Me, value)
            End Select
        End If
    End Sub
End Class