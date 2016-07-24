Imports Dopamine.Core.Settings
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm

Namespace ViewModels
    Public Class CollectionTracksColumnsViewModel
        Inherits BindableBase

#Region "Private"
        Private mCheckBoxRatingVisible As Boolean

        Private mCheckBoxRatingChecked As Boolean
        Private mCheckBoxArtistChecked As Boolean
        Private mCheckBoxAlbumChecked As Boolean
        Private mCheckBoxGenreChecked As Boolean
        Private mCheckBoxLengthChecked As Boolean
        Private mCheckBoxAlbumArtistChecked As Boolean
        Private mCheckBoxTrackNumberChecked As Boolean
        Private mCheckBoxYearChecked As Boolean
#End Region

#Region "Properties"
        Public Property CheckBoxRatingVisible() As Boolean
            Get
                Return mCheckBoxRatingVisible
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxRatingVisible, value)
            End Set
        End Property

        Public Property CheckBoxRatingChecked() As Boolean
            Get
                Return mCheckBoxRatingChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxRatingChecked, value)
            End Set
        End Property

        Public Property CheckBoxArtistChecked() As Boolean
            Get
                Return mCheckBoxArtistChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxArtistChecked, value)
            End Set
        End Property

        Public Property CheckBoxAlbumChecked() As Boolean
            Get
                Return mCheckBoxAlbumChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxAlbumChecked, value)
            End Set
        End Property

        Public Property CheckBoxGenreChecked() As Boolean
            Get
                Return mCheckBoxGenreChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxGenreChecked, value)
            End Set
        End Property

        Public Property CheckBoxLengthChecked() As Boolean
            Get
                Return mCheckBoxLengthChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxLengthChecked, value)
            End Set
        End Property

        Public Property CheckBoxAlbumArtistChecked() As Boolean
            Get
                Return mCheckBoxAlbumArtistChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxAlbumArtistChecked, value)
            End Set
        End Property

        Public Property CheckBoxTrackNumberChecked() As Boolean
            Get
                Return mCheckBoxTrackNumberChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxTrackNumberChecked, value)
            End Set
        End Property

        Public Property CheckBoxYearChecked() As Boolean
            Get
                Return mCheckBoxYearChecked
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(Me.mCheckBoxYearChecked, value)
            End Set
        End Property
#End Region

#Region "Commands"
        Public Property OkCommand As DelegateCommand
        Public Property CancelCommand As DelegateCommand
#End Region

#Region "Construction"
        Public Sub New()

            If XmlSettingsClient.Instance.Get(Of Boolean)("Behaviour", "EnableRating") Then
                Me.CheckBoxRatingVisible = True
            Else
                Me.CheckBoxRatingVisible = False
            End If

            Me.GetVisibleColumns()
        End Sub
#End Region

#Region "Private"
        Private Sub GetVisibleColumns()
            Utils.GetVisibleSongsColumns(Me.CheckBoxRatingChecked,
                                         Me.CheckBoxArtistChecked,
                                         Me.CheckBoxAlbumChecked,
                                         Me.CheckBoxGenreChecked,
                                         Me.CheckBoxLengthChecked,
                                         Me.CheckBoxAlbumArtistChecked,
                                         Me.CheckBoxTrackNumberChecked,
                                         Me.CheckBoxYearChecked)
        End Sub
#End Region

#Region "Public"
        Public Async Function SetVisibleColumns() As Task(Of Boolean)

            Await Task.Run(Sub()
                               Utils.SetVisibleSongsColumns(Me.CheckBoxRatingChecked,
                                                            Me.CheckBoxArtistChecked,
                                                            Me.CheckBoxAlbumChecked,
                                                            Me.CheckBoxGenreChecked,
                                                            Me.CheckBoxLengthChecked,
                                                            Me.CheckBoxAlbumArtistChecked,
                                                            Me.CheckBoxTrackNumberChecked,
                                                            Me.CheckBoxYearChecked)
                           End Sub)

            Return True
        End Function
#End Region
    End Class
End Namespace