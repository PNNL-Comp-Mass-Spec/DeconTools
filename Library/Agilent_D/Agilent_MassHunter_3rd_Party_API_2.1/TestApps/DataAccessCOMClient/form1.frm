VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   6120
   ClientLeft      =   2880
   ClientTop       =   2865
   ClientWidth     =   10905
   LinkTopic       =   "Form1"
   ScaleHeight     =   6120
   ScaleWidth      =   10905
   Begin VB.PictureBox m_GraphControl 
      Height          =   4215
      Left            =   120
      ScaleHeight     =   4155
      ScaleWidth      =   14835
      TabIndex        =   0
      Top             =   120
      Width           =   14895
   End
   Begin VB.PictureBox m_SpecGraphControl 
      Height          =   4215
      Left            =   120
      ScaleHeight     =   4155
      ScaleWidth      =   14835
      TabIndex        =   1
      Top             =   5040
      Width           =   14895
   End
   Begin VB.Menu m_File 
      Caption         =   "File"
      Begin VB.Menu m_FileOpen 
         Caption         =   "Open"
      End
      Begin VB.Menu m_FileClose 
         Caption         =   "Close"
      End
      Begin VB.Menu m_Exit 
         Caption         =   "Exit"
      End
   End
   Begin VB.Menu m_Chrom 
      Caption         =   "MSChromatogram"
      Begin VB.Menu m_ChromTIC 
         Caption         =   "TIC"
      End
      Begin VB.Menu m_ChromEIC 
         Caption         =   "EIC"
      End
      Begin VB.Menu m_ChromBPC 
         Caption         =   "BPC"
      End
   End
   Begin VB.Menu m_View 
      Caption         =   "View"
      Begin VB.Menu Sample 
         Caption         =   "Sample"
      End
      Begin VB.Menu m_Actuals 
         Caption         =   "Actuals"
      End
      Begin VB.Menu m_TimeSegment 
         Caption         =   "TimeSegment"
      End
      Begin VB.Menu m_FileInfo 
         Caption         =   "File Info"
      End
   End
   Begin VB.Menu m_MenuNonMSChrom 
      Caption         =   "NonMSChromatogram"
      Begin VB.Menu m_SignalMenu 
         Caption         =   "Signal"
      End
      Begin VB.Menu m_TWCMenu 
         Caption         =   "TWC"
      End
      Begin VB.Menu m_EWCMenu 
         Caption         =   "EWC"
      End
      Begin VB.Menu m_UVSpecMenu 
         Caption         =   "UVSpectrum"
      End
      Begin VB.Menu m_TCC 
         Caption         =   "TCC"
      End
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public m_NonMsReader As MassSpecDataReader.INonmsDataReader
Public m_DataReader As MassSpecDataReader.MassSpecDataReader
Public m_DataFilePathName As String

Private Sub Form_Load()

    m_DataFilePathName = "X:\ArcherData\TOF"
    Set m_NonMsReader = New MassSpecDataReader.MassSpecDataReader
    Set m_DataReader = m_NonMsReader
    
'    Dim objtif As mscorlib
'    Set objtif = m_DataReader
'
'    MsgBox objtif.ToString()
    
    m_View.Enabled = False
    m_Chrom.Enabled = False
    m_MenuNonMSChrom.Enabled = False
        
End Sub



'///////////////////////////////////////////////////////////////////////////
' Menus
'///////////////////////////////////////////////////////////////////////////

Private Sub m_ChromBPC_Click()
' generate BPC
On Error GoTo ErrorHandler
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
    Dim psetchrom As New BDAChromFilter
        
    psetchrom.ChromatogramType = ChromType_BasePeak
    psetchrom.MSLevelFilter = MSLevel_All
    
    psetchrom.MSScanTypeFilter = MSScanType_All
    psetchrom.IonPolarityFilter = IonPolarity_Mixed     'for both positive and negative

    'Set psetchrom.MzOfInterestFilter = New RangeCollection
    'Set paramSet.ScanRange = New
    
    Call GenerateAndPlotChromatogram(psetchrom, "BPC")
    
    
    MsgBox "Generate BPC Success"
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate chromatogram" & Err.Description, vbCritical, "DataReaderTest"

End Sub


Private Sub GenerateAndPlotChromatogram(psetchrom As BDAChromFilter, st As String)
        
    Dim xArray() As Double
    Dim yArray() As Single
    Dim numPoints As Long
    
    Dim chromdataArray() As BDAChromData
    chromdataArray = m_DataReader.GetChromatogram(psetchrom)
'    Dim chromdata As BDAChromData
'    Set chromdata = m_DataReader.GetTIC
    
    Dim chromData As BDAChromData
    Set chromData = chromdataArray(0)
    numPoints = chromData.TotalDataPoints
    xArray = chromData.xArray
    yArray = chromData.yArray
    
    MsgBox CStr(numPoints), , "Number of Points"
'    MsgBox CStr(UBound(xArray))
'    MsgBox CStr(UBound(yArray))
    
    'Dim data1 As New XYData
    'Dim i As Integer
    'For i = 0 To numPoints - 1
        'Debug.Print CStr(xArray(i)), CStr(yArray(i))
    '    Call data1.AppendDataPoint(xArray(i), yArray(i))
    'Next
    
    'm_GraphControl.SeriesColl.Item(1).DataObject = data1
    'm_GraphControl.SeriesColl.Item(1).Title.Text = st
        
End Sub

Private Sub m_ChromEIC_Click()
' generate EIC
On Error GoTo ErrorHandler
    
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
   ' Dim psetchrom As IBDAChromFilter
   ' Set psetchrom = New BDAChromFilter
   
    Dim psetchrom As New BDAChromFilter
    
    psetchrom.ChromatogramType = ChromType_ExtractedIon
    psetchrom.MSLevelFilter = MSLevel_All
    
    psetchrom.MSScanTypeFilter = MSScanType_All
    psetchrom.IonPolarityFilter = IonPolarity_Mixed     'for both positive and negative

    'Set psetchrom.MzOfInterestFilter = New RangeCollection
    'Set paramSet.ScanRange = New minmaxrange
    
    Call GenerateAndPlotChromatogram(psetchrom, "EIC")
    
    MsgBox "Generate EIC Success"
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate chromatogram" & Err.Description, vbCritical, "DataReaderTest"

End Sub

Private Sub m_ChromTIC_Click()
' generate TIC
On Error GoTo ErrorHandler
    
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
    Dim psetchrom As New BDAChromFilter
                      
    psetchrom.ChromatogramType = ChromType_TotalIon
    psetchrom.MSLevelFilter = MSLevel_All
    psetchrom.MSScanTypeFilter = MSScanType_All
    psetchrom.IonPolarityFilter = IonPolarity_Mixed     'for both positive and negative
    
    'Set psetchrom.MzOfInterestFilter = New RangeCollection
    'Set paramSet.ScanRange = New minmaxrange

    Call GenerateAndPlotChromatogram(psetchrom, "TIC")
            
    'other call tests for for chromatogram
    'Call TestOtherChromCalls(psetchrom)

    MsgBox "Generate TIC Success"
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate chromatogram" & Err.Description, vbCritical, "DataReaderTest"
End Sub

Private Sub TestOtherChromCalls(psetchrom As BDAChromFilter)

MsgBox "Other Chrom calls"
    Dim chromdataArray() As BDAChromData
    Dim chromData As BDAChromData
    Dim xArray() As Double
    Dim yArray() As Single
    Dim numPoints As Long
        
    chromdataArray = m_DataReader.GetChromatogram(psetchrom)
    chromData = chromdataArray(0)
    numPoints = chromData.NumDataPoints
    xArray = chromData.xArray
    yArray = chromData.yArray
    
    'Dim rangecoll As RangeCollection
    'Set rangecoll = chromdata.MeasuredMassRange
    
    MsgBox CStr(numPoints)
'    MsgBox CStr(UBound(xArray))
'    MsgBox CStr(UBound(yArray))
    
    'Dim data1 As New XYData
    'Dim i As Integer
   ' For i = 0 To numPoints - 1
        'Debug.Print CStr(xArray(i)), CStr(yArray(i))
        'Call data1.AppendDataPoint(xArray(i), yArray(i))
    'Next
    
    'm_GraphControl.SeriesColl.Item(1).DataObject = data1
    'm_GraphControl.SeriesColl.Item(1).Title.Text = psetchrom.Description

End Sub

Private Sub m_EWCMenu_Click()

' generate EWC
On Error GoTo ErrorHandler
    
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
    Dim deviceInfo() As IDeviceInfo
    deviceInfo = m_NonMsReader.GetNonmsDevices()
    If (UBound(deviceInfo) - LBound(deviceInfo) + 1) = 0 Then
        MsgBox "No Non-MS device present"
    End If
    
    Dim countLength As Integer
    countLength = UBound(deviceInfo)
    Dim i As Integer
    For i = LBound(deviceInfo) To UBound(deviceInfo)
        If deviceInfo(i).DeviceType = DeviceType.DeviceType_DiodeArrayDetector Then
            Dim rangeSignal As ICenterWidthRange
            Dim rangeRef As ICenterWidthRange
            Set rangeSignal = New CenterWidthRange
            Set rangeRef = New CenterWidthRange
            
            rangeSignal.Center = 250
            rangeSignal.Width = 40
           
            rangeRef.Center = 250
            rangeRef.Width = 40
           
            Dim rangeSig As IRange
            Set rangeSig = rangeSignal
            Dim rangeRefrange As IRange
            Set rangeRefrange = rangeRef
            
            Dim chromDataEWC As IBDAChromData
            Set chromDataEWC = m_NonMsReader.GetEWC(deviceInfo(i), rangeSig, rangeRefrange)
            Dim count As Integer
            count = chromDataEWC.TotalDataPoints
            If count = 0 Then
                MsgBox "No EWC found"
            Else
                Dim xArray() As Double
                xArray = chromDataEWC.xArray
                Dim yArray() As Single
                yArray = chromDataEWC.yArray
                MsgBox "Generate EWC Success"
            End If
        End If
    Next
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate Signal" & Err.Description, vbCritical, "DataReaderTest"

End Sub

Private Sub m_Exit_Click()
    If Not m_DataReader Is Nothing Then
        m_DataReader.CloseDataFile
    End If
    
    Unload Me
End Sub

Private Sub m_FileClose_Click()
    If Not m_DataReader Is Nothing Then
        m_DataReader.CloseDataFile
        Me.Caption = ""
        
        m_View.Enabled = False
        m_Chrom.Enabled = False
        m_MenuNonMSChrom.Enabled = False
        
    End If
End Sub


Private Sub m_FileOpen_Click()
On Error GoTo ErrorHandler

    If m_DataReader Is Nothing Then
        Exit Sub
    End If

    Dim ff As New FormDirSelection
    ff.m_strPath = m_DataFilePathName
    
    Load ff
    ff.Show vbModal
    
    If (ff.m_bCancel = True) Then
        Exit Sub
    End If
    
    m_DataFilePathName = ff.m_strPath
    Unload ff

    m_DataReader.OpenDataFile (m_DataFilePathName)
    Me.Caption = m_DataFilePathName
            
    m_View.Enabled = True
    m_Chrom.Enabled = True
    m_MenuNonMSChrom.Enabled = True
            
    Exit Sub
ErrorHandler:
    MsgBox "Failed to open the datafile " & Err.Description, vbCritical, "DataReaderTest"
End Sub


Private Sub m_GraphControl_DblClkInSelection(ByVal index As Long, ByVal xVal As Double)
'    If (False = m_Chrom.Enabled) Then
'        Exit Sub
'    End If
'
'    Dim psetSpec As IBDASpecFilter
'    Set psetSpec = New BDASpecFilter
'
'    'Dim a As New RangeCollection
'    'Dim b As New MinMaxRange
'    ' b.
'    'psetSpec.ScanRange = a
'
'    Dim filter As IMsdrPeakFilter
'    Set filter = New MsdrPeakFilter
'    filter.AbsoluteThreshold = 100
'    filter.RelativeThreshold = 1#
'    'filter.MaximumPeakCount = 100
'
'    Dim spec() As IBDASpecData
''    spec = m_DataReader.GetSpectrum_4(psetSpec, filter)
'
'    Dim spec1 As IBDASpecData
''    Set spec1 = spec(0)
'    MsgBox ("hello")
'
'    Set spec1 = m_DataReader.GetSpectrum(1#, MSScanType_All, IonPolarity_Mixed, IonizationMode_Unspecified)
'
'    MsgBox "reading time range"
'
'    Dim numPoints As Long
'    Dim r1() As RangeBase
'    Set r1 = spec1.AcquiredTimeRange
'
'    MsgBox "reading time range after"
'    numPoints = UBound(r1)
'    MsgBox CStr(numPoints)
'
'    Dim i As Integer
'    For i = 0 To numPoints
'        MsgBox r1(i).Start
'        MsgBox r1(i).End
'    Next
'
'    Dim xArray() As Double
'    Dim yArray() As Single
'
'    numPoints = spec1.TotalDataPoints
'    xArray = spec1.xArray
'    yArray = spec1.yArray
'
'    MsgBox CStr(numPoints)
'
'    'Dim rangecoll As RangeCollection
'    'MsgBox CStr(numPoints)
'    '    MsgBox CStr(UBound(xArray))
'    '    MsgBox CStr(UBound(yArray))
'
'    Dim data1 As New XYData
'    For i = 0 To numPoints - 1
'       'Debug.Print CStr(xArray(i)), CStr(yArray(i))
'       Call data1.AppendDataPoint(xArray(i), yArray(i))
'    Next
'
'    'm_SpecGraphControl.SeriesColl.Item(1).DataObject = data1
'    'm_SpecGraphControl.SeriesColl.Item(1).Title.Text = "Spectrum"
    
End Sub

Private Sub m_SignalMenu_Click()

' generate Signal
On Error GoTo ErrorHandler
    
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
       
    Dim deviceInfo() As IDeviceInfo
    deviceInfo = m_NonMsReader.GetNonmsDevices()
    
    If (UBound(deviceInfo) - LBound(deviceInfo) + 1) = 0 Then
        MsgBox "No Non-MS device present"
    End If
            
    Dim i As Integer
    For i = LBound(deviceInfo) To UBound(deviceInfo)
        If deviceInfo(i).DeviceType = DeviceType.DeviceType_DiodeArrayDetector Then
            Dim sigInfo() As ISignalInfo
            sigInfo = m_NonMsReader.GetSignalInfo(deviceInfo(i), StoredDataType.StoredDataType_Chromatograms)
            If (UBound(sigInfo) - LBound(sigInfo) + 1) = 0 Then
                MsgBox "No DAD signal found"
            Else
            Dim index As Integer
            For index = LBound(sigInfo) To UBound(sigInfo)
                Dim chromData As IBDAChromData
                Set chromData = m_NonMsReader.GetSignal(sigInfo(index))
                Dim count As Integer
                count = chromData.TotalDataPoints
                If count = 0 Then
                    MsgBox "No DAD Signal found"
                Else
                    Dim xArray() As Double
                    xArray = chromData.xArray
                    Dim yArray() As Single
                    yArray = chromData.yArray
                End If
            Next index
            MsgBox "Generate DAD Signal Success"
            End If
        End If
    Next
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate Signal" & Err.Description, vbCritical, "DataReaderTest"
End Sub

Private Sub m_TCC_Click()

On Error GoTo ErrorHandler
    
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
    Dim deviceInfo() As IDeviceInfo
    deviceInfo = m_NonMsReader.GetNonmsDevices()
    If (UBound(deviceInfo) - LBound(deviceInfo) + 1) = 0 Then
        MsgBox "No Non-MS device present"
    End If
    
    Dim countLength As Integer
    countLength = UBound(deviceInfo)
    Dim i As Integer
     For i = LBound(deviceInfo) To UBound(deviceInfo)
        If deviceInfo(i).DeviceType = DeviceType.DeviceType_ThermostattedColumnCompartment Then
            Dim sigInfo() As ISignalInfo
            sigInfo = m_NonMsReader.GetSignalInfo(deviceInfo(i), StoredDataType.StoredDataType_InstrumentCurves)
            If (UBound(sigInfo) - LBound(sigInfo) + 1) = 0 Then
                MsgBox "No TCC found"
            Else
                Dim index As Integer
                index = LBound(sigInfo)
                Dim chromData As IBDAChromData
                Set chromData = m_NonMsReader.GetSignal(sigInfo(index))
                Dim count As Integer
                count = chromData.TotalDataPoints
                If count = 0 Then
                    MsgBox "No TCC found"
                Else
                    Dim xArray() As Double
                    xArray = chromData.xArray
                    Dim yArray() As Single
                    yArray = chromData.yArray
                    MsgBox "Generate TCC Success"
                End If
            End If
        End If
    Next
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate TCC" & Err.Description, vbCritical, "DataReaderTest"

End Sub

Private Sub m_TWCMenu_Click()

' generate TWC
On Error GoTo ErrorHandler
    
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
   
    Dim deviceInfo() As IDeviceInfo
    deviceInfo = m_NonMsReader.GetNonmsDevices()
    If (UBound(deviceInfo) - LBound(deviceInfo) + 1) = 0 Then
        MsgBox "No Non-MS device present"
    End If
    
    Dim countLength As Integer
    countLength = UBound(deviceInfo)
    Dim i As Integer
    For i = LBound(deviceInfo) To UBound(deviceInfo)
        If deviceInfo(i).DeviceType = DeviceType.DeviceType_DiodeArrayDetector Then
            Dim chromDataTWC As IBDAChromData
            Set chromDataTWC = m_NonMsReader.GetTWC(deviceInfo(i))
            Dim count As Integer
            count = chromDataTWC.TotalDataPoints
            If count = 0 Then
            MsgBox "No TWC found"
            Else
                Dim xArray() As Double
                xArray = chromDataTWC.xArray
                Dim yArray() As Single
                yArray = chromDataTWC.yArray
                MsgBox "Generate TWC Success"
            End If
        End If
    Next
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate TWC" & Err.Description, vbCritical, "DataReaderTest"

End Sub

Private Sub m_UVSpecMenu_Click()

' generate UV
On Error GoTo ErrorHandler
    
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
    Dim deviceInfo() As IDeviceInfo
    deviceInfo = m_NonMsReader.GetNonmsDevices()
    If (UBound(deviceInfo) - LBound(deviceInfo) + 1) = 0 Then
        MsgBox "No Non-MS device present"
    End If
    
    Dim i As Integer
    For i = LBound(deviceInfo) To UBound(deviceInfo)
        If deviceInfo(i).DeviceType = DeviceType.DeviceType_DiodeArrayDetector Then
            Dim chromDataUV() As IBDASpecData
            
            Dim range As IMinMaxRange
            Set range = New MinMaxRange  ' 1.409, 1.109
            range.Max = 1.409
            range.Min = 1.109
            
            Dim rangeSig As IRange
            Set rangeSig = range
            
            chromDataUV = m_NonMsReader.GetUVSpectrum(deviceInfo(i), rangeSig)
            If Not chromDataUV Then
                MsgBox "No UV found"
            ElseIf UBound(chromDataUV) = -1 Then
                MsgBox "No UV found"
            Else
                Dim count As Integer
                count = chromDataUV(0).TotalDataPoints
                Dim xArray() As Double
                xArray = chromDataUV(0).xArray
                Dim yArray() As Single
                yArray = chromDataUV(0).yArray
                MsgBox "Generate UV Success"
            End If
        End If
    Next
    Exit Sub
ErrorHandler:
    MsgBox "Failed to generate UV" & Err.Description, vbCritical, "DataReaderTest"

End Sub

Private Sub Sample_Click()
On Error GoTo ErrorHandler

    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
MsgBox "Not yet implemented", vbCritical
'    Dim a As Collection
'    Set a = m_DataReader.GetSampleInfo(SampleCategory_All)
    
    
    Exit Sub
ErrorHandler:
    MsgBox "Failed to get sample information" & Err.Description, vbCritical, "DataReaderTest"
End Sub


Private Sub m_Actuals_Click()
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
    'Dim actuals As MassSpecDataReader
    'Set actuals = m_DataReader

MsgBox "Not yet implemented", vbCritical

    Dim s() As BDAActualData
'    s = m_DataReader.GetActualCollection(3#)

    'MsgBox s.count

'    Dim sa As DACoreDefinitions.IActualData
'    Dim s1 As String
'
'    For i = 0 To sCount - 1
'        Set sa = s.Item(i)
'        s1 = s1 & sa.DisplayName & " " & sa.DisplayValue & " , "
'    Next
    
'    MsgBox s1, , "Actuals Info"

End Sub

Private Sub m_TimeSegment_Click()
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
MsgBox "Not yet implemented", vbCritical
    
'    Dim rCol() As Double
'    rCol = m_DataReader.GetTimeSegmentRanges

'    For i = 0 To rColCount - 1
'        MsgBox "StartTime = " & rCol(i).Start & " End Time = " & rCol(i).End
'    Next
    
'    Dim rCol() As TimeSegmentStruct
'    rCol = m_DataReader.GetTimeSegmentInfo
'
'    For i = 0 To UBound(rCol)
'        MsgBox "Id = " & rCol(i).m_TimeSegmentId & ", StartTime = " & rCol(i).m_TimeSegmentRange.Start & " EndTime = " & rCol(i).m_TimeSegmentRange.End
'    Next


End Sub


    
Private Sub m_FileInfo_Click()
    If m_DataReader Is Nothing Then
        Exit Sub
    End If
    
    Dim ifile As BDAFileInformation
    Set ifile = m_DataReader.FileInformation
    
    '' get some file info
    Dim ims As BDAMSScanFileInformation
    Set ims = m_DataReader.MSScanFileInformation
    
    Dim temp1 As BDAMSScanTypeInformation
    Dim temp As IEnumerable
    Set temp = ims
    
    For Each temp1 In temp
        s = temp1.MSScanType
        MsgBox s, , "the scantypes"
    Next
    
    MsgBox ifile.AcquisitionTime, , "Acq time"
    MsgBox ims.FileHasMassSpectralData, , "File has spectral data"
    MsgBox ims.IonModes, , "Ion modes present"
    MsgBox ims.ScanTypesInformationCount, , "scantype count"
    
    Dim mss As BDAMSScanTypeInformation
    Set mss = ims.GetMSScanTypeInformation(MSScanType_Scan)
    
    MsgBox mss.IonPolarities, , "scan type ion polarities"
    

End Sub

