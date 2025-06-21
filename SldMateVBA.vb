' ******************************************************************************
' C:\Users\belov\AppData\Local\Temp\swx3396\Macro1.swb - macro recorded on 11/06/23 by belov
' ******************************************************************************

Option Explicit


Dim swApp As SldWorks.SldWorks
Dim swModel As SldWorks.ModelDoc2
Dim boolstat As Boolean
Dim swDocExt As ModelDocExtension
Dim swAssemblyDoc As SldWorks.assemblyDoc
Dim nameAssemble As String
Dim plane(2) As String

Sub main()
 Dim swComponent As SldWorks.Component2
 Dim swMathUtil As SldWorks.MathUtility

 plane(0) = "Right"
 plane(1) = "Top"
 plane(2) = "Front"
 
 Set swApp = Application.SldWorks
 Set swModel = swApp.ActiveDoc
 Set swMathUtil = swApp.GetMathUtility()
 Set swDocExt = swModel.Extension
  
  If swModel Is Nothing Then
    MsgBox "Solidworks document is not opened."
    Exit Sub
  End If
  
  If swModel.GetType() <> swDocumentTypes_e.swDocASSEMBLY Then
    MsgBox "Òîëüêî äëÿ ñáîðêè"
    Exit Sub
  End If
  
    Set swAssemblyDoc = swModel
    nameAssemble = GetNameAssemble(swAssemblyDoc)
     
  Dim vArray As Variant
  vArray = swAssemblyDoc.GetComponents(True)
  Dim Component As Variant
  Dim nameComp As String
    For Each Component In vArray
      Set swComponent = Component
      componentMate swComponent
    Next
  
End Sub

Sub componentMate(swComponent As SldWorks.Component2)
    
  Dim nChild As String
  Dim swTrChild As MathTransform
  Dim listVecor(2) As MathVector
  Dim listLocComp As Collection
  Dim coord() As Double
    
  Dim ScaleOutb As Double
  Dim Xch As MathVector
  Dim Ych As MathVector
  Dim Zch As MathVector
  Dim TrObjOutch As MathVector
  Dim ScaleOutch As Double
  
  Set listLocComp = New Collection
  
    ScaleOutb = 0
    Set Xch = Nothing
    Set Ych = Nothing
    Set Zch = Nothing
    Set TrObjOutch = Nothing
    ScaleOutch = 0
    
    nChild = swComponent.Name2
    
    Set swTrChild = swComponent.Transform2
    
    swTrChild.GetData Xch, Ych, Zch, TrObjOutch, ScaleOutch
    Set listVecor(0) = Xch
    Set listVecor(1) = Ych
    Set listVecor(2) = Zch
    
    coord = TrObjOutch.ArrayData
    Dim i As Integer
    Dim count As Integer
    Dim l As LocationComponent
    
     For i = 0 To 2
        Set l = New LocationComponent
        l.baseComp = nameAssemble
        l.childComp = nChild + "@" + nameAssemble
        IsFlipedAndAlign l, listVecor(i), coord(i)
        l.dist = Math.Abs(Math.Round(coord(i), 5))
        listLocComp.Add l
     Next i
     
        DeletingMateComp swComponent
        AddMate listLocComp
End Sub

Sub IsFlipedAndAlign(loc As LocationComponent, vector As MathVector, coord As Double)
    Dim orientation As Variant
    Dim temp As Double
    Dim i As Integer
    orientation = vector.ArrayData
    
    For i = 0 To 2
       
        temp = Math.Round(orientation(i))
        If (temp = 1) Then
            loc.PlanComp = plane(i)
            If (coord > 0) Then
                loc.Fliped = True
            Else
                loc.Fliped = False
            End If
           loc.Align = swMateAlign_e.swMateAlignALIGNED
        ElseIf (temp = -1) Then
            loc.PlanComp = plane(i)
            If (coord > 0) Then
                loc.Fliped = False
            Else
                loc.Fliped = True
            End If
           loc.Align = swMateAlign_e.swMateAlignANTI_ALIGNED
        End If
    Next i
End Sub

 Public Function AddMate(orientation As Collection)
   Dim i As Integer
   i = 0
   Dim locComp As LocationComponent
    For Each locComp In orientation
      AddMateToAssemble locComp, plane(i)
      i = i + 1
    Next
 End Function
        
        
     Function AddMateToAssemble(orientation As LocationComponent, planeCoord As String)
        Dim planePart As String
        Dim flipped As Boolean
        Dim Align As swMateAlign_e
        Dim distance As Double
        Dim mateError As Long
        
        planePart = orientation.PlanComp
        flipped = orientation.Fliped
        Align = orientation.Align
        distance = orientation.dist
           
        Dim FirstSelection As String
        Dim SecondSelection As String
        Dim MateName As String
        Dim matefeature As Mate2
        
        FirstSelection = planePart & "@" & orientation.childComp
        SecondSelection = planeCoord & "@" & orientation.baseComp
        MateName = planeCoord
        swModel.ClearSelection2 (True)
        boolstat = swDocExt.SelectByID2(FirstSelection, "PLANE", 0, 0, 0, True, 1, Nothing, swSelectOption_e.swSelectOptionDefault)
        boolstat = swDocExt.SelectByID2(SecondSelection, "PLANE", 0, 0, 0, True, 1, Nothing, swSelectOption_e.swSelectOptionDefault)
        Set matefeature = swAssemblyDoc.AddMate3(swMateType_e.swMateDISTANCE, Align, flipped, distance, distance, distance, 0, 0, 0, 0, 0, False, mateError)
        'matefeature = MateName
        Debug.Print planeCoord
        Debug.Print mateError
        swAssemblyDoc.EditRebuild
     End Function
     
      
      Function GetNameAssemble(assemblyDoc As SldWorks.ModelDoc2) As String
          Dim strings As Variant
          Dim assemblyTitle As String
          assemblyTitle = swModel.GetTitle
          strings = Split(assemblyTitle, ".")
          GetNameAssemble = strings(0)
      End Function
      
          
     Sub DeletingMateComp(swComp As Component2)
           
       Dim Mates As Variant
       Dim SingleMate As Variant
       Dim swMate As SldWorks.Mate2
       Dim nameMate As String
       Dim boolstat As Boolean
       
        Mates = swComp.GetMates()
        If (Not IsEmpty(Mates)) Then
        For Each SingleMate In Mates
            If TypeOf SingleMate Is SldWorks.Mate2 Then
                Set swMate = SingleMate
                nameMate = SingleMate.name
                swModel.ClearSelection2 (True)
                boolstat = swDocExt.SelectByID2(nameMate, "MATE", 0, 0, 0, True, 1, Nothing, swSelectOption_e.swSelectOptionDefault)
                swModel.EditDelete
            End If
         Next
        End If
    End Sub
