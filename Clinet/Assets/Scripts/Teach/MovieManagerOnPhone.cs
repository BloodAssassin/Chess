using UnityEngine;
using System.Collections;
using RenderHeads.Media.AVProVideo;

public class MovieManagerOnPhone : MonoBehaviour {

    public MediaPlayer _mediaPlayer;
    public MediaPlayer _mediaPlayer_jiang;
    public MediaPlayer _mediaPlayer_shi;
    public MediaPlayer _mediaPlayer_xiang;
    public MediaPlayer _mediaPlayer_che;
    public MediaPlayer _mediaPlayer_ma;
    public MediaPlayer _mediaPlayer_pao;
    public MediaPlayer _mediaPlayer_bing;

    public GameObject moviePanel;
	public GameObject moviePlayer;

    public GameObject HintMessage;

	void Update () 
    {
        if (_mediaPlayer != null)
        {
            if (_mediaPlayer.Control.IsFinished())
            {
                HintMessage.SetActive(true);
                _mediaPlayer = null;
            }
        }
	}

    public void btn_playJiang()
    {
        DisplayUGUI newGuiObject = moviePlayer.GetComponent<DisplayUGUI>();
        newGuiObject._mediaPlayer = _mediaPlayer_jiang;
        _mediaPlayer_jiang.Control.Rewind();
        _mediaPlayer = _mediaPlayer_jiang;
        _mediaPlayer_jiang.Control.Play();
        moviePanel.SetActive(true);
    }

    public void btn_playShi()
    {
        DisplayUGUI newGuiObject = moviePlayer.GetComponent<DisplayUGUI>();
        newGuiObject._mediaPlayer = _mediaPlayer_shi;
        _mediaPlayer_shi.Control.Rewind();
        _mediaPlayer = _mediaPlayer_shi;
        _mediaPlayer_shi.Control.Play();
        moviePanel.SetActive(true);
    }

    public void btn_playXiang()
    {
        DisplayUGUI newGuiObject = moviePlayer.GetComponent<DisplayUGUI>();
        newGuiObject._mediaPlayer = _mediaPlayer_xiang;
        _mediaPlayer_xiang.Control.Rewind();
        _mediaPlayer = _mediaPlayer_xiang;
        _mediaPlayer_xiang.Control.Play();
        moviePanel.SetActive(true);
    }

    public void btn_playChe()
    {
        DisplayUGUI newGuiObject = moviePlayer.GetComponent<DisplayUGUI>();
        newGuiObject._mediaPlayer = _mediaPlayer_che;
        _mediaPlayer_che.Control.Rewind();
        _mediaPlayer = _mediaPlayer_che;
        _mediaPlayer_che.Control.Play();
        moviePanel.SetActive(true);
    }

    public void btn_playMa()
    {
        DisplayUGUI newGuiObject = moviePlayer.GetComponent<DisplayUGUI>();
        newGuiObject._mediaPlayer = _mediaPlayer_ma;
        _mediaPlayer_ma.Control.Rewind();
        _mediaPlayer = _mediaPlayer_ma;
        _mediaPlayer_ma.Control.Play();
        moviePanel.SetActive(true);
    }

    public void btn_playPao()
    {
        DisplayUGUI newGuiObject = moviePlayer.GetComponent<DisplayUGUI>();
        newGuiObject._mediaPlayer = _mediaPlayer_pao;
        _mediaPlayer_pao.Control.Rewind();
        _mediaPlayer = _mediaPlayer_pao;
        _mediaPlayer_pao.Control.Play();
        moviePanel.SetActive(true);
    }

    public void btn_playBing()
    {
        DisplayUGUI newGuiObject = moviePlayer.GetComponent<DisplayUGUI>();
        newGuiObject._mediaPlayer = _mediaPlayer_bing;
        _mediaPlayer_bing.Control.Rewind();
        _mediaPlayer = _mediaPlayer_bing;
        _mediaPlayer_bing.Control.Play();
        moviePanel.SetActive(true);
    }


    public void btn_back()
    {
        _mediaPlayer_jiang.Control.Stop();
        _mediaPlayer_shi.Control.Stop();
        _mediaPlayer_xiang.Control.Stop();
        _mediaPlayer_che.Control.Stop();
        _mediaPlayer_ma.Control.Stop();
        _mediaPlayer_pao.Control.Stop();
        _mediaPlayer_bing.Control.Stop();
        moviePanel.SetActive(false);
        HintMessage.SetActive(false);
    }

}
