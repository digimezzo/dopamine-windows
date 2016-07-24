Imports Dopamine.Core.Settings

Public Class Utils
    Public Shared Sub GetVisibleSongsColumns(ByRef iRatingVisible As Boolean,
                                             ByRef iArtistVisible As Boolean,
                                             ByRef iAlbumVisible As Boolean,
                                             ByRef iGenreVisible As Boolean,
                                             ByRef iLengthVisible As Boolean,
                                             ByRef iAlbumArtistVisible As Boolean,
                                             ByRef iTrackNumberVisible As Boolean,
                                             ByRef iYearVisible As Boolean)

        iRatingVisible = False
        iArtistVisible = False
        iAlbumVisible = False
        iGenreVisible = False
        iLengthVisible = False
        iAlbumArtistVisible = False
        iTrackNumberVisible = False
        iYearVisible = False

        Dim visibleColumnsSettings As String = XmlSettingsClient.Instance.Get(Of String)("TracksGrid", "VisibleColumns")
        Dim visibleColumns As String()

        If Not String.IsNullOrEmpty(visibleColumnsSettings) Then
            If visibleColumnsSettings.Contains(";") Then
                visibleColumns = visibleColumnsSettings.Split(";"c)
            Else
                visibleColumns = {visibleColumnsSettings}
            End If
        End If

        If visibleColumns IsNot Nothing AndAlso visibleColumns.Count > 0 Then
            For Each visibleColumn As String In visibleColumns

                Select Case visibleColumn
                    Case "rating"
                        iRatingVisible = True
                    Case "artist"
                        iArtistVisible = True
                    Case "album"
                        iAlbumVisible = True
                    Case "genre"
                        iGenreVisible = True
                    Case "length"
                        iLengthVisible = True
                    Case "albumartist"
                        iAlbumArtistVisible = True
                    Case "tracknumber"
                        iTrackNumberVisible = True
                    Case "year"
                        iYearVisible = True
                End Select
            Next
        End If
    End Sub

    Public Shared Sub SetVisibleSongsColumns(iRatingVisible As Boolean,
                                             iArtistVisible As Boolean,
                                             iAlbumVisible As Boolean,
                                             iGenreVisible As Boolean,
                                             iLengthVisible As Boolean,
                                             iAlbumArtistVisible As Boolean,
                                             iTrackNumberVisible As Boolean,
                                             iYearVisible As Boolean)

        Dim visibleColumns As New List(Of String)

        If iRatingVisible Then visibleColumns.Add("rating")
        If iArtistVisible Then visibleColumns.Add("artist")
        If iAlbumVisible Then visibleColumns.Add("album")
        If iGenreVisible Then visibleColumns.Add("genre")
        If iLengthVisible Then visibleColumns.Add("length")
        If iAlbumArtistVisible Then visibleColumns.Add("albumartist")
        If iTrackNumberVisible Then visibleColumns.Add("tracknumber")
        If iYearVisible Then visibleColumns.Add("year")

        XmlSettingsClient.Instance.Set(Of String)("TracksGrid", "VisibleColumns", String.Join(";", visibleColumns.ToArray))
    End Sub
End Class
