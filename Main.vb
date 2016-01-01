'Gamespy Master Server Emulator v1.1
'
'Uses GS enctypeX servers list decoder/encoder 0.1.3a
'(see enctypeX.vb)
Module Main
    Private server As GamespyServer

    'Main Program
    Sub Main()
        Console.WriteLine(PRODUCT_NAME)

        server = New GamespyServer()
        server.Run()
    End Sub

End Module
