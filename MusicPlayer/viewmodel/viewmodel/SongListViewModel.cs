﻿using MusicPlayer.model;
using MusicPlayer.model.model;
using MusicPlayer.model.repository;
using MusicPlayer.viewmodel.util;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using DataGrid = System.Windows.Controls.DataGrid;

namespace MusicPlayer.viewmodel
{
    public class SongListViewModel
    {
        private readonly SongRepository repository = SongRepository.Instance;
        private List<Song> songList = new List<Song>();
        private Playlist selectedPlaylist;
        private int selectedSongIndex = -1;


        public Playlist SelectedPlaylist { get => selectedPlaylist; set => selectedPlaylist = value; }
        public int SelectSongIndex { get => selectedSongIndex; set => selectedSongIndex = value; }
        public List<Song> SongList => songList;


        public async Task GetSongList()
        {
            if (!CheckIfThisFormWasOpenFromPlaylist())
            {
                songList = await repository.GetAllSongs();
            }
            else
            {
                List<Song> songs = await repository.GetAllSongs();
                int playlistId = selectedPlaylist.Id;
                List<int> songIds = await repository.GetSongIdsFromPlaylist(playlistId);
                for (int index = 0; index < songs.Count; index++)
                {
                    int id = songs[index].Id;
                    if (songIds.Contains(id)) songList.Add(songs[index]);
                }
            }

        }


        public void GetSelectedSongIndex(object sender)
        {
            if (sender != null)
            {
                if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null && dataGrid.SelectedItems.Count == 1)
                {
                    DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem) as DataGridRow;
                    selectedSongIndex = row.GetIndex();
                }
            }
        }


        public bool CheckIfThisFormWasOpenFromPlaylist()
        {
            return selectedPlaylist != null;
        }


        private bool CheckIfThisSongAlreadyExisted(string name, string artist)
        {
            bool result = false;
            for (int index = 0; index < songList.Count; index++)
            {
                if (name == songList[index].Name && artist == songList[index].Artist)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }


        #region CRUD

        public void AddSongToDb(Song song)
        {
            bool isSongAdded = repository.AddNewSong(song);
            if (CheckIfThisSongAlreadyExisted(song.Name, song.Artist))
            {
                MessageBox.Show("A song with this name and artist has already existed.", "Duplicate song!");
            }
            else if (isSongAdded)
            {
                song.Id = repository.GetSongId(song.Name, song.Artist);
                songList.Add(song);
                MessageBox.Show("Add song succressfully.", "Yeahhhh!");
            }
            else
            {
                MessageBox.Show("Something went wrong :(", "Add song failed!");
            }
        }


        public void MoveSongToMainDir(string filePath)
        {
            if (!FileUtil.CheckIfFileExistInMainDir(filePath))
            {
                FileUtil.CopyPasteFileToMainDir(filePath);
            }
        }


        public void EditSongToDb(Song song, string newName, string newArtist)
        {
            bool isSongUpdated = repository.UpdateSong(song.Id, newName, newArtist);
            if (CheckIfThisSongAlreadyExisted(song.Name, song.Artist))
            {
                MessageBox.Show("A song with this name and artist has already existed.", "Duplicate song!");
            }
            else if (isSongUpdated)
            {
                int index = songList.IndexOf(song);
                songList[index].Name = newName;
                songList[index].Artist = newArtist;
                MessageBox.Show("Update song successfully.", "Yeahhhh!");
            }
            else
            {
                MessageBox.Show("Something went wrong :(", "Update song failed!");
            }    
        }


        public void DeleteSongFromDb(Song song)
        {
            int id = song.Id;
            if (!CheckIfThisFormWasOpenFromPlaylist())
            {
                if (repository.DeleteSong(id))
                {
                    songList.Remove(song);
                    MessageBox.Show("Delete song successfully!", "Yeahhhh!");
                }
                else
                {
                    MessageBox.Show("Something went wrong :(", "Delete song failed!");
                }
            }
            else
            {
                if (repository.DeleteSongIdFromPlaylist(id, selectedPlaylist.Id))
                {
                    songList.Remove(song);
                    MessageBox.Show("Delete song successfully!", "Yeahhhh!");
                }
                else
                {
                    MessageBox.Show("Something went wrong :(", "Delete song failed!");
                }
            }
        }


        public void AddSongToPlaylistInDb(int songId, int playlistId)
        {
            bool isSongAddedToPlaylist = repository.AddSongToPlaylist(songId, playlistId);
            if (isSongAddedToPlaylist)
            {
                MessageBox.Show("Add song to playlist successfully!", "Yeahhhh!");
            }
            else
            {
                MessageBox.Show("Something went wrong :(", "Add song to play list failed!");
            }
        }


        #endregion


    }
}
