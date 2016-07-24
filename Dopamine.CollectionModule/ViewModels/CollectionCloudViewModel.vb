Imports Dopamine.Common.Presentation.ViewModels
Imports Dopamine.Common.Services.Playback
Imports Dopamine.Core.Database.Entities
Imports Dopamine.Core.Database.Repositories.Interfaces
Imports Dopamine.Core.Logging
Imports Dopamine.Core.Utils
Imports Microsoft.Practices.Prism.Commands
Imports Microsoft.Practices.Prism.Mvvm
Imports Microsoft.Practices.Prism.Regions
Imports Dopamine.Core.Base

Namespace ViewModels
    Public Class CollectionCloudViewModel
        Inherits BindableBase

#Region "Variables"
        Private mAlbumRepository As IAlbumRepository
        Private mPlaybackService As IPlaybackService
        Private mHasCloud As Boolean

        Private mAlbumViewModel1 As AlbumViewModel
        Private mAlbumViewModel2 As AlbumViewModel
        Private mAlbumViewModel3 As AlbumViewModel
        Private mAlbumViewModel4 As AlbumViewModel
        Private mAlbumViewModel5 As AlbumViewModel
        Private mAlbumViewModel6 As AlbumViewModel
        Private mAlbumViewModel7 As AlbumViewModel
        Private mAlbumViewModel8 As AlbumViewModel
        Private mAlbumViewModel9 As AlbumViewModel
        Private mAlbumViewModel10 As AlbumViewModel
        Private mAlbumViewModel11 As AlbumViewModel
        Private mAlbumViewModel12 As AlbumViewModel
        Private mAlbumViewModel13 As AlbumViewModel
        Private mAlbumViewModel14 As AlbumViewModel
#End Region

#Region "Commands"
        Public Property ClickCommand As DelegateCommand(Of Object)
        Public Property LoadedCommand As DelegateCommand
#End Region

#Region "Properties"
        Public Property HasCloud() As Boolean
            Get
                Return mHasCloud
            End Get
            Set(ByVal value As Boolean)
                SetProperty(Of Boolean)(mHasCloud, value)
            End Set
        End Property
        Public Property AlbumViewModel1() As AlbumViewModel
            Get
                Return mAlbumViewModel1
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel1, value)
            End Set
        End Property

        Public Property AlbumViewModel2() As AlbumViewModel
            Get
                Return mAlbumViewModel2
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel2, value)
            End Set
        End Property

        Public Property AlbumViewModel3() As AlbumViewModel
            Get
                Return mAlbumViewModel3
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel3, value)
            End Set
        End Property

        Public Property AlbumViewModel4() As AlbumViewModel
            Get
                Return mAlbumViewModel4
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel4, value)
            End Set
        End Property

        Public Property AlbumViewModel5() As AlbumViewModel
            Get
                Return mAlbumViewModel5
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel5, value)
            End Set
        End Property

        Public Property AlbumViewModel6() As AlbumViewModel
            Get
                Return mAlbumViewModel6
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel6, value)
            End Set
        End Property

        Public Property AlbumViewModel7() As AlbumViewModel
            Get
                Return mAlbumViewModel7
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel7, value)
            End Set
        End Property

        Public Property AlbumViewModel8() As AlbumViewModel
            Get
                Return mAlbumViewModel8
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel8, value)
            End Set
        End Property

        Public Property AlbumViewModel9() As AlbumViewModel
            Get
                Return mAlbumViewModel9
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel9, value)
            End Set
        End Property

        Public Property AlbumViewModel10() As AlbumViewModel
            Get
                Return mAlbumViewModel10
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel10, value)
            End Set
        End Property

        Public Property AlbumViewModel11() As AlbumViewModel
            Get
                Return mAlbumViewModel11
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel11, value)
            End Set
        End Property

        Public Property AlbumViewModel12() As AlbumViewModel
            Get
                Return mAlbumViewModel12
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel12, value)
            End Set
        End Property

        Public Property AlbumViewModel13() As AlbumViewModel
            Get
                Return mAlbumViewModel13
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel13, value)
            End Set
        End Property

        Public Property AlbumViewModel14() As AlbumViewModel
            Get
                Return mAlbumViewModel14
            End Get
            Set(ByVal value As AlbumViewModel)
                SetProperty(Of AlbumViewModel)(mAlbumViewModel14, value)
            End Set
        End Property
#End Region

#Region "Construction"
        Public Sub New(iAlbumRepository As IAlbumRepository, iPlaybackService As IPlaybackService)

            mAlbumRepository = iAlbumRepository
            mPlaybackService = iPlaybackService

            AddHandler mPlaybackService.TrackStatisticsChanged, Async Sub() Await Me.PopulateAlbumHistoryAsync()

            Me.ClickCommand = New DelegateCommand(Of Object)(Sub(iAlbum)
                                                                 Try
                                                                     If iAlbum IsNot Nothing Then
                                                                         mPlaybackService.Enqueue(CType(iAlbum, Album))
                                                                     End If
                                                                 Catch ex As Exception
                                                                     LogClient.Instance.Logger.Error("An error occurred during Album enqueue. Exception: {0}", ex.Message)
                                                                 End Try
                                                             End Sub)

            Me.LoadedCommand = New DelegateCommand(Async Sub()
                                                       Await Task.Delay(Constants.CommonListLoadDelay)
                                                       Await Me.PopulateAlbumHistoryAsync()
                                                   End Sub)

            ' Default is True. This prevents the description text of briefly being displayed, even when there is a cloud available. 
            Me.HasCloud = True
        End Sub
#End Region

#Region "Private"
        Private Sub UpdateAlbumViewModel(iNumber As Integer, iAlbums As List(Of Album), ByRef iAlbumViewModel As AlbumViewModel)

            If iAlbums.Count < iNumber Then
                iAlbumViewModel = Nothing
            Else
                Dim alb As Album = iAlbums(iNumber - 1)
                If iAlbumViewModel Is Nothing OrElse Not iAlbumViewModel.Album.Equals(alb) Then
                    iAlbumViewModel = New AlbumViewModel With {.Album = alb, .ArtworkPath = ArtworkUtils.GetArtworkPath(alb)}
                End If
            End If
        End Sub

        Private Async Function PopulateAlbumHistoryAsync() As Task

            Dim albums = Await mAlbumRepository.GetAlbumHistoryAsync(14)

            If albums.Count = 0 Then
                Me.HasCloud = False
            Else
                Me.HasCloud = True

                Await Task.Run(Sub()
                                   Me.UpdateAlbumViewModel(1, albums, Me.AlbumViewModel1)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(2, albums, Me.AlbumViewModel2)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(3, albums, Me.AlbumViewModel3)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(4, albums, Me.AlbumViewModel4)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(5, albums, Me.AlbumViewModel5)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(6, albums, Me.AlbumViewModel6)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(7, albums, Me.AlbumViewModel7)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(8, albums, Me.AlbumViewModel8)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(9, albums, Me.AlbumViewModel9)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(10, albums, Me.AlbumViewModel10)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(11, albums, Me.AlbumViewModel11)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(12, albums, Me.AlbumViewModel12)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(13, albums, Me.AlbumViewModel13)
                                   System.Threading.Thread.Sleep(Constants.CloudLoadDelay)
                                   Me.UpdateAlbumViewModel(14, albums, Me.AlbumViewModel14)
                               End Sub)
            End If
        End Function
#End Region
    End Class
End Namespace