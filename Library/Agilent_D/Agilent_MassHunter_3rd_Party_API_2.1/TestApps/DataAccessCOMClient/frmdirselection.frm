VERSION 5.00
Begin VB.Form FormDirSelection 
   BorderStyle     =   3  'Fixed Dialog
   Caption         =   "Select Data file"
   ClientHeight    =   5100
   ClientLeft      =   45
   ClientTop       =   330
   ClientWidth     =   4440
   Icon            =   "frmdirselection.frx":0000
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   5100
   ScaleWidth      =   4440
   ShowInTaskbar   =   0   'False
   StartUpPosition =   1  'CenterOwner
   Begin VB.DriveListBox Drive1 
      Height          =   315
      Left            =   120
      TabIndex        =   3
      Top             =   120
      Width           =   4215
   End
   Begin VB.CommandButton Cancel 
      Cancel          =   -1  'True
      Caption         =   "Cancel"
      Height          =   375
      Left            =   3360
      TabIndex        =   2
      Top             =   4680
      Width           =   975
   End
   Begin VB.CommandButton Ok 
      Caption         =   "OK"
      Default         =   -1  'True
      Height          =   375
      Left            =   2280
      TabIndex        =   1
      Top             =   4680
      Width           =   975
   End
   Begin VB.DirListBox Dir1 
      Height          =   4140
      Left            =   120
      TabIndex        =   0
      Top             =   480
      Width           =   4215
   End
End
Attribute VB_Name = "FormDirSelection"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit
Private m_FSObj As New FileSystemObject

Public m_strPath As String
Public m_bCancel As Boolean

Private Sub Cancel_Click()
On Error GoTo ErrorHandler
    
    m_bCancel = True
    Unload Me
    
    Exit Sub
ErrorHandler:
    MsgBox Err.Description, vbCritical, "Data File Browser"
End Sub


Private Sub Dir1_KeyDown(KeyCode As Integer, Shift As Integer)
On Error GoTo ErrorHandler

    Dim lPos As Long
    Dim szParentFolder As String
    Dim szSelectedFolder As String

    If KeyCode = vbKeyReturn Or KeyCode = vbKeyRight Then
        Dir1.Path = Dir1.List(Dir1.ListIndex)
    ElseIf KeyCode = vbKeyLeft Then
        szSelectedFolder = Dir1.List(Dir1.ListIndex)
        lPos = CLng(InStrRev(szSelectedFolder, _
                    "\", -1, vbTextCompare))
        szParentFolder = Strings.Left$(szSelectedFolder, lPos)
        If Len(szParentFolder) > 3 Then     '> 3 means not just the drive
            Dir1.Path = szParentFolder
            Dir1.ListIndex = 0
        End If
    End If


    Exit Sub
ErrorHandler:
    MsgBox Err.Description, vbCritical, "Data File Browser"
End Sub

Private Sub Drive1_Change()
On Error GoTo ErrorHandler
    Dir1.Path = Drive1

    Exit Sub
ErrorHandler:
    MsgBox Err.Description, vbCritical, "Data File Browser"
End Sub

Private Sub Form_Load()
On Error GoTo ErrorHandler

    m_bCancel = False

    If Not (m_strPath = "") Then
        Drive1 = m_strPath
        Dir1.Path = m_strPath
    End If
    
    Exit Sub
ErrorHandler:
    MsgBox Err.Description, vbCritical, "Data File Browser"
End Sub

Private Sub Ok_Click()
On Error GoTo ErrorHandler
    
    Dim strPath As String
    strPath = Dir1.List(Dir1.ListIndex)
    
    Dim lenpath As Long
    lenpath = Len(strPath)
    
    Dim mypos As Long
    mypos = InStr(1, strPath, ".d", vbTextCompare)
    If (mypos <> (lenpath - 1)) Then
        MsgBox "Invalid Directory. Please select data file folder with .d extension", vbCritical, "Data File Browser"
        Exit Sub
    End If
    
    m_strPath = strPath
    Unload Me
    Exit Sub
ErrorHandler:
    MsgBox Err.Description, vbCritical, "Data File Browser"
End Sub
