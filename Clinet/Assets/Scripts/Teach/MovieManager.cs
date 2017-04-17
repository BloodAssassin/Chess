using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MovieManager : MonoBehaviour {

    public GameObject MoviePlayer;
    public GameObject Background;

    public MovieTexture movie;
    private AudioSource audio;

    public MovieTexture _movieJiang;
    private AudioSource _audioJiang;

    public MovieTexture _movieShi;
    private AudioSource _audioShi;

    public MovieTexture _movieChe;
    private AudioSource _audioChe;

    public MovieTexture _movieMa;
    private AudioSource _audioMa;

    public MovieTexture _moviePao;
    private AudioSource _audioPao;

    public MovieTexture _movieBing;
    private AudioSource _audioBing;

    public MovieTexture _movieXiang;
    private AudioSource _audioXiang;

    public void PlayJiang()
    {
        PlayMovie(_movieJiang, _audioJiang);
    }

    public void PlayShi()
    {
        PlayMovie(_movieShi, _audioShi);
    }

    public void PlayChe()
    {
        PlayMovie(_movieChe, _audioChe);
    }

    public void PlayMa()
    {
        PlayMovie(_movieMa, _audioMa);
    }

    public void PlayPao()
    {
        PlayMovie(_moviePao, _audioPao);
    }

    public void PlayBing()
    {
        PlayMovie(_movieBing, _audioBing);
    }

    public void PlayXiang()
    {
        PlayMovie(_movieXiang, _audioXiang);
    }

    private void PlayMovie(MovieTexture _movie,AudioSource _audio)
    {
        movie = _movie;
        audio = _audio;

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
            if (!movie.isPlaying)
            {
                movie = null;
                audio = null;
                Background.SetActive(true);
                MoviePlayer.SetActive(false);
                Debug.Log("播放完毕");
            }
        }
    }

}
