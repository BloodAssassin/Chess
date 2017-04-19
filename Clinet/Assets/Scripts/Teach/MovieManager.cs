using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MovieManager : MonoBehaviour {

    public GameObject MoviePlayer;
    public GameObject Background;
    public GameObject HintMessage;

    public MovieTexture movie;
    private AudioSource audio;

    public MovieTexture _movieJiang;
    //private AudioSource _audioJiang;

    public MovieTexture _movieShi;
    //private AudioSource _audioShi;

    public MovieTexture _movieChe;
    //private AudioSource _audioChe;

    public MovieTexture _movieMa;
    //private AudioSource _audioMa;

    public MovieTexture _moviePao;
    //private AudioSource _audioPao;

    public MovieTexture _movieBing;
    //private AudioSource _audioBing;

    public MovieTexture _movieXiang;
    //private AudioSource _audioXiang;

    private bool _playingMovie;

    public void PlayJiang()
    {
        //PlayMovie(_movieJiang, _audioJiang);
        PlayMovie(_movieJiang);
    }

    public void PlayShi()
    {
        //PlayMovie(_movieShi, _audioShi);
        PlayMovie(_movieShi);
    }

    public void PlayChe()
    {
        //PlayMovie(_movieChe, _audioChe);
        PlayMovie(_movieChe);
    }

    public void PlayMa()
    {
        //PlayMovie(_movieMa, _audioMa);
        PlayMovie(_movieMa);
    }

    public void PlayPao()
    {
        //PlayMovie(_moviePao, _audioPao);
        PlayMovie(_moviePao);
    }

    public void PlayBing()
    {
        //PlayMovie(_movieBing, _audioBing);
        PlayMovie(_movieBing);
    }

    public void PlayXiang()
    {
        //PlayMovie(_movieXiang, _audioXiang);
        PlayMovie(_movieXiang);
    }

    private void PlayMovie(MovieTexture _movie,AudioSource _audio)
    {
        movie = _movie;
        audio = _audio;

        _playingMovie = true;

        Background.SetActive(false);
        MoviePlayer.SetActive(true);

        MoviePlayer.GetComponent<RawImage>().texture = movie as MovieTexture;
        audio = MoviePlayer.GetComponent<AudioSource>();
        audio.clip = movie.audioClip;

        movie.Play();
        audio.Play();
    }

    private void PlayMovie(MovieTexture _movie)
    {
        movie = _movie;

        _playingMovie = true;

        Background.SetActive(false);
        MoviePlayer.SetActive(true);

        MoviePlayer.GetComponent<RawImage>().texture = movie as MovieTexture;
        audio = MoviePlayer.GetComponent<AudioSource>();
        audio.clip = movie.audioClip;

        movie.Play();
        audio.Play();
    }


    void Update()
    {
        if (movie != null && audio != null)
        {
            if (_playingMovie)
            {
                if (!movie.isPlaying)
                {
                    _playingMovie = false;
                    HintMessage.SetActive(true);
                }              
            }
        }
    }

    public void Return()
    {
        movie.Stop();
        _playingMovie = false;
        movie = null;
        audio = null;
        Background.SetActive(true);
        HintMessage.SetActive(false);
        MoviePlayer.SetActive(false);
    }

}
