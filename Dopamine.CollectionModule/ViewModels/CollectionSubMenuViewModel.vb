Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.Mvvm

Namespace ViewModels
    Public Class CollectionSubMenuViewModel
        Inherits BindableBase

#Region "Variables"
        Private mIsArtistsSelected As Boolean
        Private mIsGenresSelected As Boolean
        Private mIsAlbumsSelected As Boolean
        Private mIsTracksSelected As Boolean
        Private mIsPlaylistsSelected As Boolean
        Private mIsCloudSelected As Boolean
#End Region

#Region "Properties"
        Public Property IsArtistsSelected() As Boolean
            Get
                Return mIsArtistsSelected
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsArtistsSelected, value)
            End Set
        End Property

        Public Property IsGenresSelected() As Boolean
            Get
                Return mIsGenresSelected
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsGenresSelected, value)
            End Set
        End Property

        Public Property IsAlbumsSelected() As Boolean
            Get
                Return mIsAlbumsSelected
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsAlbumsSelected, value)
            End Set
        End Property

        Public Property IsTracksSelected() As Boolean
            Get
                Return mIsTracksSelected
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsTracksSelected, value)
            End Set
        End Property

        Public Property IsPlaylistsSelected() As Boolean
            Get
                Return mIsPlaylistsSelected
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsPlaylistsSelected, value)
            End Set
        End Property

        Public Property IsCloudSelected() As Boolean
            Get
                Return mIsCloudSelected
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mIsCloudSelected, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New()

            Me.SelectMenuItem()
        End Sub
#End Region

#Region "Private"

        Private Sub SelectMenuItem()

            If XmlSettingsClient.Instance.Get(Of Boolean)("Startup", "ShowLastSelectedPage") Then
                Dim screen As SelectedPage = CType(XmlSettingsClient.Instance.Get(Of Integer)("FullPlayer", "SelectedPage"), SelectedPage)

                Select Case screen
                    Case SelectedPage.Artists
                        Me.IsArtistsSelected = True
                    Case SelectedPage.Genres
                        Me.IsGenresSelected = True
                    Case SelectedPage.Albums
                        Me.IsAlbumsSelected = True
                    Case SelectedPage.Tracks
                        Me.IsTracksSelected = True
                    Case SelectedPage.Playlists
                        Me.IsPlaylistsSelected = True
                    Case SelectedPage.Recent
                        Me.IsCloudSelected = True
                    Case Else
                        Me.IsArtistsSelected = True
                End Select
            Else
                Me.IsArtistsSelected = True
            End If
        End Sub
#End Region
    End Class
End Namespace