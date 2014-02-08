Imports System.Windows.Forms
Module MainModule
    Private Const Version As String = "1.0.0.7m"
    Dim isPercent As Boolean = Nothing
    Dim newTempo As String = Nothing

    Sub Main()
        Application.CurrentCulture = New Globalization.CultureInfo("en-US", False)
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("Map Speed Changer Plus v{0}" + vbNewLine + "Original code and implementation by smoogipooo" + vbNewLine + "Modified by MillhioreF", Version)
        Do
            Using ofd As New OpenFileDialog
                ofd.Title = "Please select the .osu file you want to convert."
                ofd.Filter = "osu! Beatmap Files (*.osu)|*.osu"
                If ofd.ShowDialog = DialogResult.OK Then
                    If ofd.FileName.Substring(ofd.FileName.LastIndexOf(".")) = ".osu" Then
                        Console.Write(vbNewLine + "Please input the percentage you want to change your map's speed by." + vbNewLine + "Alternatively, input a new BPM without a percentage sign" + vbNewLine + "to change the speed to that BPM." + vbNewLine + vbNewLine + "Input: ")
                        Do
                            newTempo = Console.ReadLine()
                            Try
                                If (newTempo.IndexOf("%") <> -1) Then
                                    isPercent = True
                                    newTempo = (Double.Parse(newTempo.Substring(0, Len(newTempo) - 1))) / 100.0
                                Else
                                    isPercent = False
                                    newTempo = Double.Parse(newTempo)
                                End If
                                Exit Do
                            Catch er As Exception
                                Console.Write("Invalid input, please try again: ")
                            End Try
                        Loop
                        Try
                            ProcessBeatmap(ofd.FileName, newTempo, isPercent)
                            Console.WriteLine("Press any key to select another beatmap.")
                        Catch ex As Exception
                            Console.WriteLine(vbNewLine + "A fatal error has occurred and the program was" + vbNewLine + "not able to complete the conversion process." + vbNewLine + "Please show MillhioreF this error via PM" + vbNewLine + "(unless you know why it happened):")
                            Console.WriteLine()
                            Dim location As String = ofd.FileName.Substring(0, ofd.FileName.LastIndexOf("\"))
                            Dim beatmapname As String = ofd.FileName.Substring(ofd.FileName.LastIndexOf("\") + 1)
                            Console.WriteLine("Map: " & beatmapname)
                            Console.WriteLine("Message: " & ex.Message)
                            Console.WriteLine("StackTrace: " & ex.StackTrace)
                            Console.WriteLine(vbNewLine + "Press any key to try again.")
                        End Try
                    Else
                        Console.WriteLine("The selected file was not a valid osu beatmap file!" + vbNewLine + "Please check that your file ends in .osu!" + vbNewLine + vbNewLine + "Press any key to try again.")
                    End If
                    Console.ReadKey()
                Else
                    Exit Do
                End If
            End Using
        Loop
    End Sub

    Function GetBPM(ByVal file As String)
        Return 0
    End Function

    Sub ProcessBeatmap(ByVal file As String, ByVal tempo As Double, ByVal percent As Boolean)
        Console.ForegroundColor = ConsoleColor.White
        Dim UniversalOffset As Integer = 0
        Dim newmp3filename As String = Nothing
        Dim newbeatmapname As String = Nothing
        Dim beatmapname As String = file.Substring(file.LastIndexOf("\") + 1, file.LastIndexOf(".") - (file.LastIndexOf("\") + 1))
        If (percent = True) Then
            newbeatmapname = beatmapname.Substring(0, beatmapname.LastIndexOf("]")) & " " & (tempo * 100.0) & "%" & "]"
        Else
            newbeatmapname = beatmapname.Substring(0, beatmapname.LastIndexOf("]")) & " " & tempo & "BPM" & "]"
        End If
        Dim beatmaplocation As String = file.Substring(0, file.LastIndexOf("\"))
        Dim beatmapcontents As New IO.StreamReader(file)
        Dim mp3filename As String = ""
        Dim percentChange As Double = tempo
        Do While beatmapcontents.Peek <> -1
            Dim s As String = beatmapcontents.ReadLine
            Dim atTiming As Boolean = False
            If s.Contains("AudioFilename") Then
                mp3filename = s.Substring(s.IndexOf("AudioFilename") + 15)
                If percent = True Then
                    Exit Do
                End If
            End If
            If (s.Contains("[TimingPoints]")) Then
                atTiming = True
            End If
            If (atTiming) Then
                Try
                    Dim bpmdelay As Double = CDbl(SubStr(s, nthDexOf(s, ",", 0) + 1, nthDexOf(s, ",", 1)))
                    If bpmdelay > 0 Then
                        Dim bpm As Double = 60000 / bpmdelay
                        percentChange = tempo / bpm
                        atTiming = False
                        Exit Do
                    End If
                Catch
                End Try
            End If
        Loop
        If (percent = True) Then
            newmp3filename = (mp3filename.Substring(0, mp3filename.ToLower.IndexOf(".mp3")) & " " & (tempo * 100.0) & "percent.mp3").Replace(" ", "")
        Else
            newmp3filename = (mp3filename.Substring(0, mp3filename.ToLower.IndexOf(".mp3")) & " " & tempo & "BPM.mp3").Replace(" ", "")
        End If

        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("Processing beatmap data...")
        Console.ForegroundColor = ConsoleColor.White

        Dim newbeatmapcontents As String = IO.File.ReadAllText(file)
        Dim lines() As String = System.IO.File.ReadAllLines(file)
        Dim linecount As Integer = lines.Length

        'This part is commented out because who needs AR11

        ''Check if the beatmap contains an ApproachRate value
        'Dim containsAR As Boolean = IIf(newbeatmapcontents.Contains("ApproachRate:"), True, False)

        'newbeatmapcontents = newbeatmapcontents.Replace(mp3filename, newmp3filename)
        ''Add ApproachRate value of 10 if it doesn't exists
        'If containsAR = False Then
        '    Dim temp2(lines.Length) As String
        '    For i = 0 To lines.Length - 1
        '        If lines(i).Contains("[Difficulty]") Then
        '            temp2(i) = lines(i)
        '            temp2(i + 1) = "ApproachRate:10"
        '            i += 1
        '        Else
        '            temp2(i) = lines(i)
        '        End If
        '    Next
        '    lines = temp2
        'End If

        'Process
        Dim processedlines As Integer = 0
        Dim newlines As New List(Of String)
        Dim currentsection As String = ""
        For Each l In lines
            If l = "" Then
                newlines.Add(l)
                processedlines += 1
                If processedlines Mod 100 = 0 Then
                    Console.WriteLine("Processed {0}/{1} lines", processedlines, linecount)
                    Continue For
                End If
            End If
            Dim temp As String = l

            If (l = "[General]") Or (l = "[Metadata]") Or (l = "[Difficulty]") Or (l = "[TimingPoints]") Or (l = "[HitObjects]") Or (l = "[Events]") Then
                newlines.Add(temp)
                currentsection = l
                processedlines += 1
                If processedlines Mod 100 = 0 Then
                    Console.WriteLine("Processed {0}/{1} lines", processedlines, linecount)
                End If
                Continue For
            End If

            If (currentsection = "[General]") And (l.Contains("AudioFilename:")) Then
                temp = "AudioFilename: " & newmp3filename
            End If

            If (currentsection = "[Metadata]") And (l.Contains("Version:")) Then
                If percent = True Then
                    temp = l & " " & (tempo * 100.0) & "% Speed"
                Else
                    temp = l & " " & tempo & "BPM"
                End If
            End If

            'If (currentsection = "[Difficulty]") And (l.Contains("ApproachRate:")) Then
            '    If l.Substring(l.IndexOf(":") + 1) <> 10 Then
            '        temp = "ApproachRate:10"
            '    Else
            '        temp = l
            '    End If
            'End If

            If (currentsection = "[Events]") Then
                Try
                    If l.Substring(0, 1) = "2" Then
                        Dim breaktiming1 As Integer = l.Substring(l.IndexOf(",") + 1, l.LastIndexOf(",") - (l.IndexOf(",") + 1))
                        Dim breaktiming2 As Integer = l.Substring(l.LastIndexOf(",") + 1)
                        Dim newbreaktiming1 As Integer = breaktiming1 / percentChange + UniversalOffset
                        Dim newbreaktiming2 As Integer = breaktiming2 / percentChange + UniversalOffset
                        temp = "2," & newbreaktiming1 & "," & newbreaktiming2
                    End If
                Catch
                End Try
            End If

            If (currentsection = "[TimingPoints]") Then
                Try
                    Dim timing As String = l.Substring(0, l.IndexOf(","))
                    Dim bpmdelay As Double = CDbl(SubStr(l, nthDexOf(l, ",", 0) + 1, nthDexOf(l, ",", 1)))
                    Dim newtiming As String = (Math.Round(CInt(timing) / percentChange) + UniversalOffset).ToString
                    If bpmdelay > 0 Then
                        bpmdelay = bpmdelay / percentChange
                    End If
                    temp = newtiming & "," & bpmdelay & l.Substring(l.IndexOf(",", l.IndexOf(",") + 1))
                Catch
                End Try
            End If

            If (currentsection = "[HitObjects]") Then
                Dim timing As String = SubStr(l, nthDexOf(l, ",", 1) + 1, nthDexOf(l, ",", 2))
                Dim newtiming As String = (Math.Round(CInt((Math.Round(CInt(timing)) / percentChange))) + UniversalOffset).ToString
                If nthDexOf(l, ",", 5) <> -1 Then
                    Dim s As String = SubStr(l, nthDexOf(l, ",", 4) + 1, nthDexOf(l, ",", 5))
                    If (s.Contains("L")) Or (s.Contains("P")) Or (s.Contains("B")) Or (s.Contains("C")) Or (s.Contains("|")) Then 'Sliders exist in the form X|x:y where X is L,P,B, or C
                        'Slider
                        temp = SubStr(l, 0, nthDexOf(l, ",", 1) + 1) & newtiming & SubStr(l, nthDexOf(l, ",", 2))
                    Else
                        Try
                            'Spinner
                            Dim newsecondtiming As String = (Math.Round(CInt((Math.Round(CInt(s)) / percentChange))) + UniversalOffset).ToString
                            temp = SubStr(l, 0, nthDexOf(l, ",", 1) + 1) & newtiming & SubStr(l, nthDexOf(l, ",", 2), nthDexOf(l, ",", 4) + 1) & newsecondtiming & SubStr(l, nthDexOf(l, ",", 5))
                        Catch
                            'Circle
                            temp = SubStr(l, 0, nthDexOf(l, ",", 1) + 1) & newtiming & SubStr(l, (nthDexOf(l, ",", 2)))
                        End Try
                    End If
                Else
                    Try
                        'Spinner
                        Dim newsecondtiming As String = (Math.Round(CInt((Math.Round(CInt(SubStr(l, nthDexOf(l, ",", 4) + 1))) / percentChange))) + UniversalOffset).ToString
                        temp = SubStr(l, 0, nthDexOf(l, ",", 1) + 1) & newtiming & SubStr(l, nthDexOf(l, ",", 2), nthDexOf(l, ",", 4) + 1) & newsecondtiming
                    Catch
                        'Circle
                        temp = SubStr(l, 0, nthDexOf(l, ",", 1) + 1) & newtiming & SubStr(l, nthDexOf(l, ",", 2))
                    End Try
                End If
            End If
            newlines.Add(temp)
            processedlines += 1
            If processedlines Mod 100 = 0 Then
                Console.WriteLine("Processed {0}/{1} lines", processedlines, linecount)
            End If
        Next

        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("Done. Now processing audio...")
        Console.ForegroundColor = ConsoleColor.White
        Dim tempoChange As Double = -100 + (percentChange.ToString * 100.0)

        If My.Computer.FileSystem.FileExists(beatmaplocation & "\" & newmp3filename) = False Then
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\temp.mp3") Then
                My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\temp.mp3")
            End If
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\temp.wav") Then
                My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\temp.wav")
            End If
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\temp2.wav") Then
                My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\temp2.wav")
            End If
            My.Computer.FileSystem.CopyFile(beatmaplocation & "\" & mp3filename, Application.StartupPath & "\temp.mp3")
            Shell(Application.StartupPath & "\lame.exe --decode temp.mp3 temp.wav", AppWinStyle.Hide, True)
            Shell(Application.StartupPath & "\soundstretch.exe temp.wav temp2.wav -tempo=" + tempoChange.ToString + "%", AppWinStyle.Hide, True)
            Shell(Application.StartupPath & "\lame.exe temp2.wav " & newmp3filename, AppWinStyle.Hide, True)
            My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\temp.mp3")
            My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\temp.wav")
            My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\temp2.wav")
            My.Computer.FileSystem.MoveFile(Application.StartupPath & "\" & newmp3filename, beatmaplocation & "\" & newmp3filename, True)
        End If

        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine("Done. Now saving")
        Console.ForegroundColor = ConsoleColor.White


        Using sw As New IO.StreamWriter(beatmaplocation & "\" & newbeatmapname & ".osu", True)
            For Each l In newlines
                sw.WriteLine(l)
            Next
        End Using
        Console.WriteLine("Done. Press the F5 button to refresh the osu! map list and see the new map!")
    End Sub

    'Homebrewed functions which work close to as fast as Substring & IndexOf in CPU time and faster than String.Split()
    Function nthDexOf(ByVal str As String, ByVal splitter As String, ByVal n As Integer) As Integer
        Dim camnt As Integer = -1
        Dim indx As Integer = 0
        Do Until (camnt = n) Or (indx = -1)
            indx = str.IndexOf(splitter, indx + 1)
            If indx = -1 Then
                Return -1
            End If
            camnt += 1
        Loop
        Return indx
    End Function
    Function SubStr(ByVal str As String, ByVal startindex As Integer, Optional ByVal endindex As Integer = -1) As String
        If endindex = -1 Then
            Return str.Substring(startindex, str.Length - startindex)
        Else
            Return str.Substring(startindex, endindex - startindex)
        End If
    End Function
End Module
