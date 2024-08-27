' SPDX-License-Identifier: MIT
'----------------------------------------------------------------------------
' Summary  : VB.Net demo application for the IXXAT VCI .NET-API (For net40 target).
'            This demo demonstrates the following VCI features
'              - adapter selection
'              - controller initialization
'              - creation of a message channel
'              - transmission/reception of CAN messages
' Copyright: Copyright(C) 2016-2023 HMS Technology Center Ravensburg GmbH, 
'            all rights reserved
'----------------------------------------------------------------------------

Imports System
Imports System.Text
Imports System.Collections
Imports System.Threading
Imports Ixxat.Vci4
Imports Ixxat.Vci4.Bal
Imports Ixxat.Vci4.Bal.Can

Module CanConNet4

    '''' <summary>
    ''''   Class holding application logic
    '''' </summary>
    Public Class CanConNet

#Region "Member variables"

        ''' <summary>
        '''   Reference to the used VCI device.
        ''' </summary>
        Private mDevice As Ixxat.Vci4.IVciDevice = Nothing

        ''' <summary>
        '''   Reference to the CAN controller.
        ''' </summary>
        Private mCanCtl As Ixxat.Vci4.Bal.Can.ICanControl = Nothing

        ''' <summary>
        '''   Reference to the CAN message communication channel.
        ''' </summary>
        Private mCanChn As Ixxat.Vci4.Bal.Can.ICanChannel = Nothing

        ''' <summary>
        '''   Reference to the CAN message scheduler.
        ''' </summary>
        Private mCanSched As Ixxat.Vci4.Bal.Can.ICanScheduler = Nothing

        ''' <summary>
        '''   Reference to the message writer of the CAN message channel.
        ''' </summary>
        Private mWriter As Ixxat.Vci4.Bal.Can.ICanMessageWriter = Nothing

        ''' <summary>
        '''   Reference to the message reader of the CAN message channel.
        ''' </summary>
        Private mReader As Ixxat.Vci4.Bal.Can.ICanMessageReader = Nothing

        ''' <summary>
        '''   Thread that handles the message reception.
        ''' </summary>
        Private rxThread As System.Threading.Thread = Nothing

        ''' <summary>
        '''   Quit flag for the receive thread.
        ''' </summary>
        Private mMustQuit As Long = 0

        ''' <summary>
        '''   Event that's set if at least one message was received.
        ''' </summary>
        Private mRxEvent As New AutoResetEvent(True)

#End Region

#Region "Application main"

        Public Sub Main()
            Console.WriteLine(" >>>> VCI.NET - API Example V1.1 <<<<")
            Console.WriteLine(" initializes the CAN with 125 kBaud")
            Console.WriteLine(" creates a cyclic message object with id 200H")
            Console.WriteLine(" key 'c' starts/stops a cyclic message object with id 200H")
            Console.WriteLine(" key 't' sends a message with id 100H")
            Console.WriteLine(" shows all received messages")
            Console.WriteLine(" Quit the application with ESC")

            Console.WriteLine(" Select Adapter...")
            If (SelectDevice()) Then
                Console.WriteLine(" Select Adapter.......... OK !")

                Console.WriteLine(" Initialize CAN...")

                If (Not InitSocket(0)) Then
                    Console.WriteLine(" Initialize CAN............ FAILED !")
                Else
                    Console.WriteLine(" Initialize CAN............ OK !")

                    '
                    ' start the receive thread
                    '
                    Dim rxThread As New Thread(AddressOf ReceiveThreadFunc)
                    rxThread.Start(Me)

                    '
                    ' add a cyclic message when scheduler Is available
                    '
                    Dim cyclicMsg As ICanCyclicTXMsg = Nothing
                    If mCanSched IsNot Nothing Then
                        '
                        ' start a cyclic object
                        '
                        cyclicMsg = mCanSched.AddMessage()

                        cyclicMsg.AutoIncrementMode = CanCyclicTXIncMode.NoInc
                        cyclicMsg.Identifier = 200
                        cyclicMsg.CycleTicks = 100
                        cyclicMsg.DataLength = 8
                        cyclicMsg.SelfReceptionRequest = True

                        Dim i As Byte
                        For i = 0 To cyclicMsg.DataLength - 1
                            cyclicMsg(i) = i
                        Next
                    End If

                    '
                    ' wait for keyboard hit transmit  CAN-Messages cyclically
                    '
                    Dim cki As New ConsoleKeyInfo()

                    Console.WriteLine(" Press T to transmit single message.")
                    If mCanSched IsNot Nothing Then
                        Console.WriteLine(" Press C to start/stop cyclic message.")
                    Else
                        Console.WriteLine(" Cyclic messages not supported.")
                    End If

                    Console.WriteLine(" Press ESC to exit.")
                    Do While cki.Key <> ConsoleKey.Escape

                        Do While Not Console.KeyAvailable
                            Thread.Sleep(10)
                        Loop

                        cki = Console.ReadKey(True)
                        If (cki.Key = ConsoleKey.T) Then
                            TransmitData()
                        ElseIf (cki.Key = ConsoleKey.C) Then
                            If cyclicMsg IsNot Nothing Then
                                If (cyclicMsg.Status <> CanCyclicTXStatus.Busy) Then
                                    cyclicMsg.Start(0)
                                    Console.WriteLine(" Cyclic message started.")
                                Else
                                    cyclicMsg.Stop()
                                    Console.WriteLine(" Cyclic message stopped.")
                                End If
                            End If
                        End If
                    Loop


                    If cyclicMsg IsNot Nothing Then
                        '
                        ' stop cyclic message
                        '
                        cyclicMsg.Stop()
                    End If

                    '
                    ' tell receive thread to quit
                    '
                    Interlocked.Exchange(mMustQuit, 1)

                    '
                    ' Wait for termination of receive thread
                    '
                    rxThread.Join()
                End If
            End If

            Console.WriteLine("Free VCI - Resources...")
            FinalizeApp()
            Console.WriteLine("Free VCI - Resources........ OK !")

            Console.WriteLine("Done")
            Console.ReadLine()
        End Sub

#End Region

#Region "Device selection"

        ''' <summary>
        '''   Selects the first CAN adapter.
        ''' </summary>
        Private Function SelectDevice() As Boolean
            Dim succeeded As Boolean = False
            Dim deviceManager As IVciDeviceManager = Nothing
            Dim deviceList As IVciDeviceList = Nothing
            Dim deviceEnum As IEnumerator = Nothing

            Try
                '
                ' Get device manager from VCI server
                '
                deviceManager = VciServer.Instance().DeviceManager

                '
                ' Get the list of installed VCI devices
                '
                deviceList = deviceManager.GetDeviceList()

                '
                ' Get enumerator for the list of devices
                '
                deviceEnum = deviceList.GetEnumerator()

                '
                ' Get first device
                '
                deviceEnum.MoveNext()
                mDevice = deviceEnum.Current

                If mDevice IsNot Nothing Then
                    '
                    ' print bus type And controller type of first controller
                    '
                    Dim info As IVciCtrlInfo = mDevice.Equipment(0)
                    Console.WriteLine(" BusType    : {0}", info.BusType)
                    Console.WriteLine(" CtrlType   : {0}", info.ControllerType)

                    ' show the device name And serial number
                    Dim serialNumberText As String = If(mDevice.UniqueHardwareId.ToString(), "<device id not available>")
                    Console.WriteLine(" Interface    : " + mDevice.Description)
                    Console.WriteLine(" Serial number: " + serialNumberText)
                    succeeded = True
                End If

            Catch exc As Exception
                Console.WriteLine("Error: " + exc.Message)
            Finally
                '
                ' Dispose device manager  it's no longer needed.
                '
                DisposeVciObject(deviceManager)

                '
                ' Dispose device list  it's no longer needed.
                '
                DisposeVciObject(deviceList)

                '
                ' Dispose device list  it's no longer needed.
                '
                DisposeVciObject(deviceEnum)
            End Try

            Return succeeded
        End Function

#End Region

#Region "Opening socket"

        ''' <summary>
        '''   Opens the specified socket, creates a message channel, initializes
        '''   And starts the CAN controller.
        ''' </summary>
        ''' <param name="canNo">
        '''   Number of the CAN controller to open.
        ''' </param>
        ''' <returns>
        '''   A value indicating if the socket initialization succeeded Or failed.
        ''' </returns>
        Private Function InitSocket(canNo As Byte) As Boolean
            Dim Bal As IBalObject = Nothing
            Dim succeeded As Boolean = False

            Try
                '
                ' Open bus access layer
                '
                Bal = mDevice.OpenBusAccessLayer()

                '
                ' Open a message channel for the CAN controller
                '
                mCanChn = Bal.OpenSocket(canNo, GetType(Ixxat.Vci4.Bal.Can.ICanChannel))

                If mCanChn IsNot Nothing Then
                    '
                    ' check if device supports the cyclic message scheduler
                    '
                    If (mCanChn.Features.HasFlag(CanFeatures.Scheduler)) Then
                        '
                        ' Open the scheduler of the CAN controller
                        '
                        mCanSched = Bal.OpenSocket(canNo, GetType(Ixxat.Vci4.Bal.Can.ICanScheduler))
                        If mCanSched IsNot Nothing Then
                            ' take scheduler into defined state (no messages, running)
                            mCanSched.Reset()
                            mCanSched.Resume()
                        End If

                    End If

                    ' Initialize the message channel
                    mCanChn.Initialize(1024, 128, False)

                    ' Get a message reader object
                    mReader = mCanChn.GetMessageReader()

                    ' Initialize message reader
                    mReader.Threshold = 1

                    ' Create And assign the event that's set if at least one message
                    ' was received.
                    mRxEvent = New AutoResetEvent(False)
                    mReader.AssignEvent(mRxEvent)

                    ' Get a message wrtier object
                    mWriter = mCanChn.GetMessageWriter()

                    ' Initialize message writer
                    mWriter.Threshold = 1

                    ' Activate the message channel
                    mCanChn.Activate()

                    '
                    ' Open the CAN controller
                    '
                    mCanCtl = Bal.OpenSocket(canNo, GetType(Ixxat.Vci4.Bal.Can.ICanControl))
                    If mCanCtl IsNot Nothing Then
                        ' Initialize the CAN controller
                        mCanCtl.InitLine(CanOperatingModes.Standard Or
                            CanOperatingModes.Extended Or
                            CanOperatingModes.ErrFrame,
                            CanBitrate.Cia125KBit)

                        '
                        ' print line status
                        '
                        Console.WriteLine(" LineStatus: {0}", mCanCtl.LineStatus)

                        ' Set the acceptance filter for std identifiers
                        mCanCtl.SetAccFilter(CanFilter.Std, CanAccCode.All, CanAccMask.All)

                        ' Set the acceptance filter for ext identifiers
                        mCanCtl.SetAccFilter(CanFilter.Ext, CanAccCode.All, CanAccMask.All)

                        ' Start the CAN controller
                        mCanCtl.StartLine()
                    End If

                    succeeded = True
                End If

            Catch exc As Exception
                Console.WriteLine("Error: Initializing socket failed : " + exc.Message)
                succeeded = False
            Finally
                '
                ' Dispose bus access layer
                '
                DisposeVciObject(Bal)
            End Try

            Return succeeded

        End Function

#End Region

#Region "Message transmission"

        ''' <summary>
        '''   Transmits a CAN message with ID 0x100.
        ''' </summary>
        Private Sub TransmitData()
            If mWriter Is Nothing Then
                Return
            End If

            Dim factory As IMessageFactory = VciServer.Instance().MsgFactory
            Dim canMsg As ICanMessage = factory.CreateMsg(GetType(Ixxat.Vci4.Bal.Can.ICanMessage))

            canMsg.TimeStamp = 0
            canMsg.Identifier = &H100
            canMsg.FrameType = CanMsgFrameType.Data
            canMsg.DataLength = 8
            canMsg.SelfReceptionRequest = True  ' show this message in the console window

            Dim i As Byte
            For i = 0 To canMsg.DataLength - 1
                canMsg(i) = i
            Next

            ' Write the CAN message into the transmit FIFO
            mWriter.SendMessage(canMsg)
        End Sub

#End Region

#Region "Message reception"

        ''' <summary>
        ''' Print a CAN message
        ''' </summary>
        ''' <param name="canMessage"></param>
        Private Sub PrintMessage(canMessage As ICanMessage)
            Select Case canMessage.FrameType
                '
                ' show data frames
                '
                Case CanMsgFrameType.Data
                    If (Not canMessage.RemoteTransmissionRequest) Then
                        Console.Write("Time: {0,10}  ID: {1,3:X}  DLC: {2,1}  Data:",
                            canMessage.TimeStamp,
                            canMessage.Identifier,
                            canMessage.DataLength)

                        Dim index As Integer
                        For index = 0 To canMessage.DataLength - 1
                            Console.Write(" {0,2:X}", canMessage(index))
                        Next
                        Console.Write(vbLf)
                    Else
                        Console.WriteLine("Time: {0,10}  ID: {1,3:X}  DLC: {2,1}  Remote Frame",
                            canMessage.TimeStamp,
                            canMessage.Identifier,
                            canMessage.DataLength)
                    End If

                '
                ' show informational frames
                '
                Case CanMsgFrameType.Info
                    Select Case (CType(canMessage(0), CanMsgInfoValue))
                        Case CanMsgInfoValue.Start
                            Console.WriteLine("CAN started...")
                        Case CanMsgInfoValue.Stop
                            Console.WriteLine("CAN stopped...")
                        Case CanMsgInfoValue.Reset
                            Console.WriteLine("CAN reseted...")
                    End Select

                '
                ' show error frames
                '
                Case CanMsgFrameType.Error
                    Select Case (CType(canMessage(0), CanMsgError))
                        Case CanMsgError.Stuff
                            Console.WriteLine("stuff error...")
                        Case CanMsgError.Form
                            Console.WriteLine("form error...")
                        Case CanMsgError.Acknowledge
                            Console.WriteLine("acknowledgment error...")
                        Case CanMsgError.Bit
                            Console.WriteLine("bit error...")
                        Case CanMsgError.Fdb
                            Console.WriteLine("fast data bit error...")
                        Case CanMsgError.Crc
                            Console.WriteLine("CRC error...")
                        Case CanMsgError.Dlc
                            Console.WriteLine("Data length error...")
                        Case CanMsgError.Other
                            Console.WriteLine("other error...")
                    End Select
            End Select
        End Sub

        ''' <summary>
        ''' Demonstrate reading messages via MsgReader:ReadMessages() function
        ''' </summary>
        Private Sub ReadMultipleMsgsViaReadMessages()
            If (mReader Is Nothing) Or
               (mRxEvent Is Nothing) Then
                Return
            End If

            Do While (0 = mMustQuit)
                ' Wait 100 msec for a message reception
                If (mRxEvent.WaitOne(100, False)) Then
                    Dim msgArray() As ICanMessage = Nothing
                    If (mReader.ReadMessages(msgArray) > 0) Then
                        For Each entry In msgArray
                            PrintMessage(entry)
                        Next
                    End If
                End If
            Loop
        End Sub

        ''' <summary>
        ''' Demonstrate reading messages via MsgReader:ReadMessage() function
        ''' </summary>
        Private Sub ReadMsgsViaReadMessage()
            If (mReader Is Nothing) Or
               (mRxEvent Is Nothing) Then
                Return
            End If

            Dim canMessage As Ixxat.Vci4.Bal.Can.ICanMessage = Nothing

            Do While (0 = mMustQuit)
                ' Wait 100 msec for a message reception
                If (mRxEvent.WaitOne(100, False)) Then
                    ' read a CAN message from the receive FIFO
                    Do While (mReader.ReadMessage(canMessage))
                        PrintMessage(canMessage)
                    Loop
                End If
            Loop
        End Sub

        ''' <summary>
        '''   This method Is the works as receive thread.
        ''' </summary>
        Public Shared Sub ReceiveThreadFunc(context As Object)

            Dim app As CanConNet = TryCast(context, CanConNet)
            If app IsNot Nothing Then
                app.ReadMsgsViaReadMessage()
            End If
            '
            ' alternative: use app.ReadMultipleMsgsViaReadMessages()
            '
        End Sub

#End Region

#Region "Utility methods"

        ''' <summary>
        '''   Finalizes the application 
        ''' </summary>
        Private Sub FinalizeApp()
            '
            ' Dispose all hold VCI objects.
            '

            ' Dispose message reader
            DisposeVciObject(mReader)

            ' Dispose message writer 
            DisposeVciObject(mWriter)

            ' Dispose CAN channel
            DisposeVciObject(mCanChn)

            ' Dispose CAN controller
            DisposeVciObject(mCanCtl)

            ' Dispose VCI device
            DisposeVciObject(mDevice)
        End Sub


        ''' <summary>
        '''   This method tries to dispose the specified object.
        ''' </summary>
        ''' <param name="obj">
        '''   Reference to the object to be disposed.
        ''' </param>
        ''' <remarks>
        '''   The VCI interfaces provide access to native driver resources. 
        '''   Because the .NET garbage collector Is only designed to manage memory, 
        '''   but Not native OS And driver resources the application itself Is 
        '''   responsible to release these resources via calling 
        '''   IDisposable.Dispose() for the obects obtained from the VCI API 
        '''   when these are no longer needed. 
        '''   Otherwise native memory And resource leaks may occure.  
        ''' </remarks>
        Private Shared Sub DisposeVciObject(ByVal obj As Object)
            If obj IsNot Nothing Then
                Dim dispose As System.IDisposable
                dispose = obj
                If dispose IsNot Nothing Then
                    dispose.Dispose()
                    obj = Nothing
                End If
            End If
        End Sub

#End Region

    End Class

#Region "Application entry point"

    Sub Main()
        Dim App = New CanConNet()
        App.Main()
    End Sub

#End Region

End Module
