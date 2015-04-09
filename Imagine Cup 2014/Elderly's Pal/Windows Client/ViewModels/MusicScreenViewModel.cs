using System;
using System.Collections.Generic;
using NiceDreamers.Windows.Models;
using NiceDreamers.Windows.Navigation;

namespace NiceDreamers.Windows.ViewModels
{
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.MusicScreen)]
    public class MusicScreenViewModel : ViewModelBase
    {
        public List<MusicModel> SongList
        {
            get
            {
                return new List<MusicModel>
                {
                    new MusicModel
                    {
                        SongName = "Don't Matter",
                        SongImgUri = "/Content/Music/Image/Akon.jpg",
                        SongUri = new Uri("Content/Music/Songs/DontMatter.mp3", UriKind.Relative)
                    },
                    new MusicModel
                    {
                        SongName = "La La La",
                        SongImgUri = "/Content/Music/Image/ongcaothang.jpg",
                        SongUri = new Uri("Content/Music/Songs/La La La.mp3", UriKind.Relative)
                    },
                    new MusicModel
                    {
                        SongName = "How Can I Tell Her",
                        SongImgUri = "/Content/Music/Image/lobo.jpg",
                        SongUri = new Uri("Content/Music/Songs/How Can I Tell Her.mp3", UriKind.Relative)
                    },
                    new MusicModel
                    {
                        SongName = "Lắng Nghe Nước Mắt",
                        SongImgUri = "/Content/Music/Image/mr.siro.jpg",
                        SongUri = new Uri("Content/Music/Songs/Lắng Nghe Nước Mắt.mp3", UriKind.Relative)
                    },
                    new MusicModel
                    {
                        SongName = "Noi Tinh Yeu Ket Thuc",
                        SongImgUri = "/Content/Music/Image/buianhtuan.jpg",
                        SongUri = new Uri("Content/Music/Songs/Noi Tinh Yeu Ket Thuc.mp3", UriKind.Relative)
                    },
                    new MusicModel
                    {
                        SongName = "Whistle",
                        SongImgUri = "/Content/Music/Image/florida.jpg",
                        SongUri = new Uri("Content/Music/Songs/Whistle.mp3", UriKind.Relative)
                    },
                };
            }
        }
    }
}